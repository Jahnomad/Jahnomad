#!/usr/bin/env python3
"""Import the approved pixel-art menu and synthwave audio from one ZIP."""
from pathlib import Path
from zipfile import ZipFile
import sys
import hashlib

root = Path(__file__).resolve().parents[1]
archives = sorted((root / "art-upload").glob("*.zip"))
if len(archives) != 1:
    sys.exit("Exactly one asset ZIP is required in art-upload/")
allowed = {
    "Assets/Resources/MenuMaster.png": (b"\x89PNG\r\n\x1a\n", 8000000),
    "Assets/Resources/MenuTheme.ogg": (b"OggS", 4000000),
}
approved_sha256 = {
    "Assets/Resources/MenuMaster.png": "3bd00d15cdc2973d32f02c49bbb494bc668b89b5781fbe7d664389ed508408dd",
    "Assets/Resources/MenuTheme.ogg": "7de42c584f4468b9dd4b7a6fafccbf463fd26ba5678180c7f5ef353afae00b68",
}
with ZipFile(archives[0]) as archive:
    broken = archive.testzip()
    if broken:
        sys.exit("ZIP integrity error: " + broken)
    files = {i.filename: i for i in archive.infolist() if not i.is_dir()}
    if set(files) != set(allowed) | {"README_RU.txt"}:
        sys.exit("Unexpected art pack entries: " + repr(sorted(files)))
    for name, (signature, maximum) in allowed.items():
        info = files[name]
        if info.file_size > maximum:
            sys.exit("Asset is too big: " + name)
        data = archive.read(info)
        if not data.startswith(signature):
            sys.exit("Invalid signature: " + name)
        if hashlib.sha256(data).hexdigest() != approved_sha256[name]:
            sys.exit("Asset does not match approved artwork or soundtrack: " + name)
        if name.endswith(".png"):
            width = int.from_bytes(data[16:20], "big")
            height = int.from_bytes(data[20:24], "big")
            if not (640 <= width <= 4096 and 360 <= height <= 4096):
                sys.exit("Unsupported PNG dimensions")
        destination = root / name
        destination.parent.mkdir(parents=True, exist_ok=True)
        destination.write_bytes(data)
        print(f"Imported {name}: {len(data)} bytes")
print("Approved art pack imported.")
