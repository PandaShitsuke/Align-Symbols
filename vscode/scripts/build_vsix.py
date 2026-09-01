import json, os, zipfile

# Repo root is one level above this script (which sits in "scripts/").
ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
PACKAGE = os.path.join(ROOT, "package.json")
RELEASE = os.path.join(ROOT, "release")

with open(PACKAGE, encoding="utf-8") as fh:
    PKG = json.load(fh)
VERSION = PKG["version"]
PUBLISHER = PKG["publisher"]
ID = "%s.%s" % (PUBLISHER, PKG["name"])
VSIX = os.path.join(RELEASE, "align_symbols_vscode-%s.vsix" % VERSION)

MANIFEST = (
    '<?xml version="1.0" encoding="utf-8"?>\n'
    '<PackageManifest Version="2.0.0" xmlns="http://schemas.microsoft.com/developer/vsx-schema/2011" '
    'xmlns:d="http://schemas.microsoft.com/developer/vsx-schema-design/2011">\n'
    '  <Metadata>\n'
    '    <Identity Id="%s" Version="%s" Language="en-US" Publisher="%s"/>\n'
    '    <DisplayName>Symbol Align (by column)</DisplayName>\n'
    '    <Description xml:space="preserve">Align assignment symbols (=, +, &lt;&lt;, [, ], ;) by column for chained bitfield assignments.</Description>\n'
    '  </Metadata>\n'
    '  <Installation>\n'
    '    <InstallationTarget Id="Microsoft.VisualStudio.Code"/>\n'
    '  </Installation>\n'
    '  <Dependencies/>\n'
    '  <Assets>\n'
    '    <Asset Type="Microsoft.VisualStudio.Code.Manifest" Path="extension/package.json" Addressable="true"/>\n'
    '    <Asset Type="Microsoft.VisualStudio.Services.Content.Details" Path="extension/README.md" Addressable="true"/>\n'
    '    <Asset Type="Microsoft.VisualStudio.Services.Content.License" Path="extension/LICENSE" Addressable="true"/>\n'
    '  </Assets>\n'
    '</PackageManifest>\n'
) % (ID, VERSION, PUBLISHER)

CONTENT_TYPES = (
    '<?xml version="1.0" encoding="utf-8"?>\n'
    '<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">\n'
    '  <Default Extension="json" ContentType="application/json"/>\n'
    '  <Default Extension="js" ContentType="text/javascript"/>\n'
    '  <Default Extension="md" ContentType="text/markdown"/>\n'
    '  <Default Extension="vsixmanifest" ContentType="text/xml"/>\n'
    '  <Default Extension="xml" ContentType="text/xml"/>\n'
    '  <Default Extension="txt" ContentType="text/plain"/>\n'
    '</Types>\n'
)


def add(zf, arcname, data):
    if isinstance(data, str):
        data = data.encode("utf-8")
    zf.writestr(arcname, data)


os.makedirs(RELEASE, exist_ok=True)
if os.path.exists(VSIX):
    os.remove(VSIX)

files = ["package.json", "extension.js", "aligner.js", "README.md", "CHANGELOG.md", "LICENSE", "icon.png"]
with zipfile.ZipFile(VSIX, "w", zipfile.ZIP_DEFLATED) as zf:
    add(zf, "[Content_Types].xml", CONTENT_TYPES)
    add(zf, "extension.vsixmanifest", MANIFEST)
    for f in files:
        p = os.path.join(ROOT, f)
        if not os.path.exists(p):
            raise SystemExit("missing " + p)
        with open(p, "rb") as fh:
            add(zf, "extension/" + f, fh.read())

print("wrote", VSIX, os.path.getsize(VSIX), "bytes")
