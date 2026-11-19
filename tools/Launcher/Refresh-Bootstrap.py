"""Refresh the launcher while retaining the exact existing mod/dependency pins."""
import argparse, hashlib, json, zipfile
import xml.etree.ElementTree as ET
from pathlib import Path

def sha(data): return hashlib.sha256(data).hexdigest().upper()

def main():
    parser=argparse.ArgumentParser()
    parser.add_argument('--base',type=Path,required=True)
    parser.add_argument('--base-sha256',required=True)
    parser.add_argument('--launcher',type=Path,required=True)
    parser.add_argument('--source-commit',required=True)
    parser.add_argument('--output',type=Path,required=True)
    args=parser.parse_args()
    if args.output.exists(): raise SystemExit('Choose a new output file.')
    base=args.base.read_bytes()
    if sha(base)!=args.base_sha256.upper(): raise SystemExit('Base bootstrap SHA-256 does not match.')
    with zipfile.ZipFile(args.base) as archive:
        entries={}
        for item in archive.infolist():
            if item.is_dir(): continue
            name=item.filename.replace('\\','/')
            if name.startswith('/') or ':' in name or '..' in name.split('/') or name in entries:
                raise SystemExit('Unsafe or duplicate bootstrap path.')
            entries[name]=archive.read(item)
    old=json.loads(entries['installer-manifest.json'].decode('utf-8-sig'))
    mod_manifest=entries['mtou-release.json']
    entries['Marmo Town of Us.exe']=args.launcher.read_bytes()
    old['Launcher']={'Path':'Marmo Town of Us.exe','Bytes':len(entries['Marmo Town of Us.exe']),
                     'Sha256':sha(entries['Marmo Town of Us.exe'])}
    version=ET.parse(Path(__file__).resolve().parents[2]/'TOU-Marmo.csproj').findtext('.//InformationalVersion')
    old['LauncherVersion']=version
    old['LauncherSourceCommit']=args.source_commit
    entries['installer-manifest.json']=json.dumps(old,indent=2).encode()
    url='https://github.com/marmoladka2137/Marmo-Town-of-Us/archive/'+args.source_commit+'.zip'
    entries['Notices/LauncherSource.txt']=('Marmo Town of Us launcher '+version+'\nCode: GPL-3.0; artwork: existing credited terms.\n'
        'Complete launcher source: '+url+'\nBuild: tools/Launcher/Build.ps1\n'
        'The plugin payload has its own unchanged source commit in installer-manifest.json.\n').encode()
    entries['START-HERE.txt']=('Marmo Town of Us '+version+'\n\n'
        'Extract the complete ZIP and run Marmo Town of Us.exe.\n'
        '1. Choose Steam or Epic Games. Auto-detect selects your installed Among Us.\n'
        '2. Leave the default separate MTOU folder, or use Choose folder.\n'
        '3. Click INSTALL MTOU, then PLAY MTOU. Keep the storefront client running.\n'
        'If discovery fails, use Choose .exe and select the original Among Us.exe.\n'
        'The original game folder is never the install destination. No game files are distributed.\n'
        'Epic gameplay is not verified. Discord: https://dc.gg/mtou\n'
        'Launcher source: '+url+'\n').encode()
    args.output.parent.mkdir(parents=True,exist_ok=True)
    with zipfile.ZipFile(args.output,'w',zipfile.ZIP_DEFLATED,compresslevel=9) as archive:
        for name,data in sorted(entries.items()):archive.writestr(name,data)
    with zipfile.ZipFile(args.output) as archive:
        assert archive.read('mtou-release.json')==mod_manifest
        assert json.loads(archive.read('installer-manifest.json'))['Payload']==old['Payload']
    print(sha(args.output.read_bytes())+'  '+args.output.name)
    print('Mod manifest, payload pins and dependency pins retained; launcher replaced.')

if __name__=='__main__':main()
