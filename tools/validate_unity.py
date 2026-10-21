#!/usr/bin/env python3
"""Static Unity repository checks. No Unity Editor or third-party Python packages required."""
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
issues = []

def check(condition, message):
    if not condition:
        issues.append(message)
    print(("PASS" if condition else "FAIL") + ": " + message)

def read(path):
    p = ROOT / path
    check(p.is_file(), f"exists: {path}")
    return p.read_text(encoding="utf-8") if p.is_file() else ""

scene_path = "Assets/Scenes/00_Boot.unity"
scene = read(scene_path)
meta = read(scene_path + ".meta")
settings = read("ProjectSettings/EditorBuildSettings.asset")
manifest = read("Packages/manifest.json")
version = read("ProjectSettings/ProjectVersion.txt")
boot = read("Assets/Scripts/Core/BootFlowController.cs")
art = read("Assets/Scripts/Core/ReutovRuntimeArt.cs")

guid_match = re.search(r"^guid:\s*([a-fA-F0-9]{32})\s*$", meta, re.M)
guid = guid_match.group(1) if guid_match else ""
check(bool(guid) and f"guid: {guid}" in settings, "Build Settings GUID matches scene .meta")
check("path: " + scene_path in settings, "Boot scene is registered for builds")
check(bool(re.search(r"m_Scenes:\s*\n\s*- enabled:\s*1", settings)), "Boot scene is enabled")
check(scene.startswith("%YAML 1.1") and "SceneRoots:" in scene
      and "GameObject:" in scene and "Transform:" in scene, "Scene YAML has engine headers and a valid root structure")
check(scene.count("--- !u!1 &") == 1 and scene.count("--- !u!4 &") == 1,
      "Boot scene has one root GameObject and one Transform")
check("m_Roots:\n  - {fileID: 10001}" in scene, "Root Transform reference resolves")

try:
    deps = json.loads(manifest).get("dependencies", {})
except (ValueError, AttributeError):
    deps = {}
for module in ("com.unity.ugui", "com.unity.modules.audio",
               "com.unity.modules.imageconversion"):
    check(module in deps, "required Unity module: " + module)
check("6000.0.58f2" in version, "Unity editor version is pinned")
check("RuntimeInitializeOnLoadMethod" in boot, "Boot controller self-starts in a standalone scene")
check("ReutovRuntimeArt.GetSprite(name)" in boot, "Missing textures have a runtime fallback")
check("#if UNITY_EDITOR" in boot and
      "UnityEditor.EditorApplication.isPlaying=false;" in boot,
      "Editor-only quit logic is protected by an editor preprocessor branch")
check("UnityEditor" not in art and "System.IO" not in art and "File.WriteAllBytes" not in art,
      "Runtime fallback is independent of editor and disk")
for item in ("Backdrop", "StudioLogo", "GameTitle", "NewGame",
             "Settings", "Quit", "Back", "Skip", "SoundOn",
             "SoundOff", "PrologueCard", "ExitNotice"):
    check(f'case "{item}":' in art, f"generated art case exists: {item}")

def delimiters(source):
    opening = "([{"
    closing = ")]}"
    stack = []
    i = 0
    state = "code"
    while i < len(source):
        c = source[i]
        n = source[i + 1] if i + 1 < len(source) else ""
        if state == "line":
            if c == "\n": state = "code"
        elif state == "block":
            if c == "*" and n == "/":
                state = "code"; i += 1
        elif state == "string":
            if c == "\\": i += 1
            elif c == '"': state = "code"
        elif state == "char":
            if c == "\\": i += 1
            elif c == "'": state = "code"
        elif c == "/" and n == "/":
            state = "line"; i += 1
        elif c == "/" and n == "*":
            state = "block"; i += 1
        elif c == '"':
            state = "string"
        elif c == "'":
            state = "char"
        elif c in opening:
            stack.append(c)
        elif c in closing:
            if not stack or opening.index(stack.pop()) != closing.index(c):
                return False
        i += 1
    return not stack and state in ("code", "line")

check(delimiters(boot), "Boot controller source braces and quotes are balanced")
check(delimiters(art), "Pixel-art runtime source braces and quotes are balanced")

# Sprint 1: prevent missing runtime symbols and invisible interface actions.
courtyard = read("Assets/Scripts/Core/CourtyardGameplay.cs")
pixel_sprites = read("Assets/Scripts/Core/ReutovCourtyardArt.cs")
music = read("Assets/Scripts/Core/ReutovSynthwave.cs")
for path, source in (("CourtyardGameplay", courtyard), ("ReutovCourtyardArt", pixel_sprites),
                     ("ReutovSynthwave", music)):
    check(delimiters(source), path + " source braces and quotes are balanced")
check("public static partial class ReutovRuntimeArt" in art and
      "public static partial class ReutovRuntimeArt" in pixel_sprites,
      "Pixel art is split into matching static partial classes")
check("private sealed partial class PixelCanvas" in art and
      "private sealed partial class PixelCanvas" in pixel_sprites,
      "TextAt and Line helpers extend the same nested PixelCanvas type")
check("CourtyardGameplay" in boot and "ScreenMode.Playing" in boot,
      "New game flows to an interactive courtyard, not an inert prologue")
check("Button(\"Continue\"" in boot, "Prologue continue is present and clickable")
check("PlayerPrefs.SetInt" in boot and "ReutovSynthwave.GetMenuLoop()" in boot,
      "Sound settings persist and original soundtrack is connected")
check("IPointerDownHandler" in courtyard and "IDragHandler" in courtyard and
      "IPointerUpHandler" in courtyard and "activePointer" in courtyard,
      "Analog touch controls are multi-pointer safe")
check("Mathf.Clamp(at.x" in courtyard and
      "(closest - at).sqrMagnitude < Radius * Radius" in courtyard,
      "Character uses real rectangular collision footprints")
check("private static void MakeCourtyard()" not in pixel_sprites and
      "private static PixelCanvas MakeCourtyard()" in pixel_sprites,
      "Original courtyard renderer creates a static scene")
check('p.TextAt("ЛАРЁК"' in art,
      "Kiosk signage is placed on the building, not centred on the screen")
check('p.TextCentered("JAH NOMAD", 3' in art,
      "Studio name uses the agreed small title credit")
cases = set(re.findall(r'case "([^"]+)"', art))
names = set(re.findall(r'(?:Picture|Button|UI|GetSprite)\("([^"]+)"', boot + courtyard))
missing = sorted(name for name in names if name not in cases)
check(not missing, "All runtime art references resolve: " + ", ".join(missing))

print(f"Static validation: {'FAILED: ' + str(len(issues)) + ' checks' if issues else 'PASSED'}")
if issues:
    sys.exit(1)
