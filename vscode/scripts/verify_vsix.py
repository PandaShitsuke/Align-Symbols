import json, os, re, sys, zipfile

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
vsix = os.path.join(ROOT, "release", "align-symbols-0.9.0.vsix")
if not os.path.exists(vsix):
    print("missing", vsix)
    sys.exit(1)

z = zipfile.ZipFile(vsix)
names = z.namelist()
print("entries:", names)
print("has icon.png:", "extension/icon.png" in names)

manifest = z.read("extension.vsixmanifest").decode("utf-8")
m = re.search(r'<Identity Id="([^"]+)"[^>]*Publisher="([^"]+)"', manifest)
print("manifest Identity:", m.groups() if m else "NOT FOUND")

pkg = json.loads(z.read("extension/package.json").decode("utf-8"))
print("publisher:", pkg.get("publisher"))
print("icon:", pkg.get("icon"))
print("repository:", (pkg.get("repository") or {}).get("url"))
print("version:", pkg.get("version"))
