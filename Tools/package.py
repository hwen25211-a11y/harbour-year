from pathlib import Path
import zipfile, hashlib, json
root=Path(__file__).resolve().parents[1]
out=root/'release';out.mkdir(exist_ok=True)
manifest={}
def pack(name, files):
    dest=out/name
    with zipfile.ZipFile(dest,'w',zipfile.ZIP_DEFLATED,compresslevel=6) as z:
        for path,arc in files:z.write(path,arc)
    with zipfile.ZipFile(dest) as z:
        if z.testzip() is not None:raise RuntimeError('ZIP integrity failed')
    manifest[name]={'bytes':dest.stat().st_size,'sha256':hashlib.sha256(dest.read_bytes()).hexdigest()}
    print(name,manifest[name]['bytes'])
files=[]
for folder in ['Assets','Packages','ProjectSettings','Tools','docs']:
    for p in (root/folder).rglob('*'):
        if not p.is_file() or p.suffix=='.log' or p.name.endswith('-source.ttf') or '.DS_Store' in p.name:continue
        files.append((p,str(Path('HarbourYear-Unity')/p.relative_to(root))))
for name in ['README.txt','README.md','CONTRIBUTING.md','.gitignore','.gitattributes','开始游戏.command','浏览器试玩.command']:files.append((root/name,str(Path('HarbourYear-Unity')/name)))
pack('HarbourYear-UnitySource.zip',files)
mac=root/'Build/macOS'
pack('HarbourYear-macOS.zip',[(p,str(p.relative_to(mac))) for p in mac.rglob('*') if p.is_file()]+[(root/'README.txt','README.txt')])
web=root/'Build/WebGL'
pack('HarbourYear-WebGL.zip',[(p,str(p.relative_to(web))) for p in web.rglob('*') if p.is_file()])
(out/'checksums.json').write_text(json.dumps(manifest,indent=2)+'\n')
