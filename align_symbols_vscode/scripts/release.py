import json, os, re, subprocess, tempfile

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
CHANGELOG = os.path.join(ROOT, "CHANGELOG.md")
REPO = "PandaShitsuke/AlignSymbols"
GH = r"C:\Program Files\GitHub CLI\gh.exe"

with open(CHANGELOG, encoding="utf-8") as fh:
    text = fh.read()

# Parse "## X.Y.Z" sections into {version: notes}.
sections = re.split(r"^##\s+", text, flags=re.M)
notes = {}
for s in sections[1:]:
    lines = s.splitlines()
    if not lines:
        continue
    ver = lines[0].strip()
    notes[ver] = "\n".join(lines[1:]).strip()

versions = sorted(notes.keys(), key=lambda v: [int(x) for x in v.split(".")])

for ver in versions:
    vsix = os.path.join(ROOT, "release", "align_symbols_vscode-%s.vsix" % ver)
    if not os.path.exists(vsix):
        print("SKIP %s: missing %s" % (ver, vsix))
        continue
    with tempfile.NamedTemporaryFile("w", suffix=".md", delete=False, encoding="utf-8") as tf:
        tf.write(notes[ver])
        notes_file = tf.name
    tag = "v" + ver
    cmd = [GH, "release", "create", tag, vsix, "--title", tag, "--notes-file", notes_file, "--repo", REPO]
    print("==> %s" % ver)
    r = subprocess.run(cmd, capture_output=True, text=True)
    print((r.stdout or "").strip())
    if r.stderr.strip():
        print((r.stderr or "").strip())
    os.unlink(notes_file)

print("done")
