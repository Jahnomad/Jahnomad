#!/usr/bin/env python3
"""Convert the Unity Cloud ZIP to a GitHub-Pages-ready static website."""
from pathlib import Path, PurePosixPath
import gzip
import shutil
import sys
import zipfile

root = Path(__file__).resolve().parents[1]
incoming = sorted((root / "webgl-upload").glob("*.zip"), key=lambda p: p.stat().st_mtime)
if not incoming:
    sys.exit("Upload the Unity Cloud ZIP into webgl-upload/ before publishing.")

# One deployment at a time; prevent accidental use of stale ZIPs.
if len(incoming) != 1:
    sys.exit("Exactly one ZIP must be present in webgl-upload/. Found: " + ", ".join(x.name for x in incoming))
archive = incoming[0]
out = root / "_site"
if out.exists():
    shutil.rmtree(out)
out.mkdir(parents=True)

with zipfile.ZipFile(archive) as z:
    broken = z.testzip()
    if broken:
        sys.exit("ZIP CRC error: " + broken)
    candidates = [i for i in z.infolist() if i.filename.endswith("/index.html") or i.filename == "index.html"]
    if len(candidates) != 1:
        sys.exit("Expected exactly one Unity WebGL index.html.")
    parent = str(PurePosixPath(candidates[0].filename).parent)
    prefix = "" if parent == "." else parent + "/"
    files = 0
    for obj in z.infolist():
        if obj.is_dir() or not obj.filename.startswith(prefix):
            continue
        relative = PurePosixPath(obj.filename[len(prefix):])
        if not relative.parts or relative.is_absolute() or ".." in relative.parts:
            sys.exit("Unsafe ZIP path: " + obj.filename)
        dest = out.joinpath(*relative.parts)
        dest.parent.mkdir(parents=True, exist_ok=True)
        with z.open(obj) as src, dest.open("wb") as tgt:
            shutil.copyfileobj(src, tgt)
        files += 1

build = out / "Build"
if not build.is_dir():
    sys.exit("Missing Unity Build/ assets")
for p in build.glob("*.gz"):
    target = p.with_suffix("")
    target.write_bytes(gzip.decompress(p.read_bytes()))
    p.unlink()

wasms = list(build.glob("*.wasm"))
datas = list(build.glob("*.data"))
frameworks = list(build.glob("*.framework.js"))
loaders = list(build.glob("*.loader.js"))
if not (len(wasms) == len(datas) == len(frameworks) == len(loaders) == 1):
    sys.exit("Missing or ambiguous Unity loader/data/WASM/framework assets")
if wasms[0].read_bytes()[:4] != bytes([0,97,115,109]):
    sys.exit("WebAssembly binary magic is invalid")
if (out / "index.html").exists():
    (out / "index.html").unlink()

template = (root / "tools/webgl_player.html").read_text(encoding="utf-8")
for placeholder, file in (
    ("{{DATA}}", datas[0]), ("{{WASM}}", wasms[0]),
    ("{{FRAMEWORK}}", frameworks[0]), ("{{LOADER}}", loaders[0])):
    template = template.replace(placeholder, "Build/" + file.name)
if "{{" in template:
    sys.exit("Unresolved template parameters")
(out / "index.html").write_text(template, encoding="utf-8")
(out / ".nojekyll").write_text("", encoding="utf-8")
# Publish the separately labeled approved menu/music preview without replacing
# the existing compiled Unity binary.
preview = out / "preview"
assets = preview / "art"
assets.mkdir(parents=True, exist_ok=True)
shutil.copy2(root / "preview/index.html", preview / "index.html")
for resource in ("MenuMaster.png", "MenuTheme.ogg"):
    source = root / "Assets/Resources" / resource
    if not source.exists():
        sys.exit("Approved preview resource missing: " + resource)
    shutil.copy2(source, assets / resource)

print(f"READY: {files} Unity files extracted; website size {sum(p.stat().st_size for p in out.rglob('*') if p.is_file())/1048576:.1f} MiB")
