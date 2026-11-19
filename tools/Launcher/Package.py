"""Assemble the launcher supplement; never read or distribute installed game files.

Run after Build.ps1 and Release build. Extra binaries/sources are official releases
downloaded separately, with exact tag notices. The loader is downloaded at install.
"""
import argparse, hashlib, json, shutil, subprocess, zipfile
from pathlib import Path

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()

def pin(path, name):
    return dict(Path=name, Bytes=path.stat().st_size, Sha256=sha(path))

def archive(folder, target):
    with zipfile.ZipFile(target, 'w', zipfile.ZIP_DEFLATED, compresslevel=9) as z:
        for p in sorted(folder.rglob('*')):
            if p.is_file(): z.write(p, p.relative_to(folder).as_posix())

def main():
    a=argparse.ArgumentParser()
    a.add_argument('--launcher',type=Path,required=True)
    a.add_argument('--downloads',type=Path,required=True)
    a.add_argument('--output',type=Path,required=True)
    a.add_argument('--commit',required=True)
    o=a.parse_args(); source=Path(__file__).resolve().parents[2]
    if o.output.exists(): raise SystemExit('Choose a new output directory.')
    package=o.output/'package'; files=o.output/'mod-files'
    package.mkdir(parents=True); files.mkdir()
    def put(src,name):
        target=files/name; target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,target)
    put(source/'bin/Release/net6.0/TOU-Marmo.dll','BepInEx/plugins/TOU-Marmo.dll')
    extra=[('AleLuduMod','AleLuduMod.dll'),('AUnlocker','AUnlocker_v1.3.2.dll'),('Mini.RegionInstall','Mini.RegionInstall.dll')]
    for name,dll in extra:
        put(o.downloads/dll,'BepInEx/plugins/'+dll)
        put(o.downloads/(name+'-LICENSE.txt'),'MTOU Notices/LICENSES/'+name+'.txt')
        put(o.downloads/(name+'-source.zip'),'MTOU Notices/CorrespondingSource/'+name+'-source.zip')
    # Public defaults only. Do not import personal game or account configuration.
    cfg=files/'BepInEx/config/AUnlocker.cfg';cfg.parent.mkdir(parents=True,exist_ok=True)
    cfg.write_text('[Account]\nRemoveGuestStatus = false\nRemoveMinorStatus = false\n\n[Unsafe]\nNoCharacterLimit = false\nNoChatCooldown = false\nNoOptionsLimits = false\n',encoding='utf-8')
    for name in ['LICENSE','CREDITS.md','THIRD_PARTY_NOTICES.md','ASSET_CREDITS.md','MTOU_ARTWORK_LICENSE.md']:
        put(source/name,'MTOU Notices/'+name)
    for p in (source/'LICENSES').glob('*'):
        if p.is_file(): put(p,'MTOU Notices/LICENSES/'+p.name)
    (files/'MTOU Notices/SOURCE.txt').write_text('MTOU corresponding source: https://github.com/marmoladka2137/Marmo-Town-of-Us/tree/'+o.commit+'\nThe release includes the matching MTOU source archive. Additional GPL plugin source archives accompany their binaries.\n',encoding='utf-8')
    payload=package/'Payload';payload.mkdir();shutil.copy2(source/'bin/Release/net6.0/TOU-Marmo.dll',payload/'TOU-Marmo.dll')
    shutil.copy2(o.launcher,package/'Marmo Town of Us.exe')
    archive(files,package/'MTOU-Mods-Windows.zip')
    manifest=dict(Schema=1,Version='1.1.0 Beta',Revision='1.1.0.18',SourceCommit=o.commit,ArchiveSha256=sha(package/'MTOU-Mods-Windows.zip'),Files=[])
    for p in sorted(files.rglob('*')):
        if p.is_file():
            item=pin(p,p.relative_to(files).as_posix())
            item['PreserveExisting']=item['Path'].startswith('BepInEx/config/')
            manifest['Files'].append(item)
    (package/'mtou-release.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
    installer=json.loads((source/'tools/SteamInstaller/installer-manifest.json').read_text(encoding='utf-8-sig'))
    installer.update(Version='1.1.0 Beta',AssemblyRevision='1.1.0.18',SourceCommit=o.commit,
                     Payload=pin(payload/'TOU-Marmo.dll','Payload/TOU-Marmo.dll'),
                     Launcher=pin(package/'Marmo Town of Us.exe','Marmo Town of Us.exe'))
    (package/'installer-manifest.json').write_text(json.dumps(installer,indent=2),encoding='utf-8')
    shutil.copytree(files/'MTOU Notices',package/'Notices')
    (package/'START-HERE.txt').write_text('Marmo Town of Us 1.1.0 Beta\n\nExtract this complete ZIP, then run Marmo Town of Us.exe. Select Steam or Epic Games and an installed, purchased Among Us. Install into a NEW separate folder. Keep the storefront client running. No game files are included. Public GitHub updates are checked automatically. Epic gameplay and multiplayer remain unverified.\n\nDiscord: https://dc.gg/mtou\nSource: https://github.com/marmoladka2137/Marmo-Town-of-Us/tree/'+o.commit+'\n',encoding='utf-8')
    archive(package,o.output/'MTOU-1.1.0-beta-Launcher-Windows.zip')
    # Export the index, not the workspace: ignored retired frames/cache are absent.
    names=subprocess.check_output(['git','ls-files','-z'],cwd=source).decode().split('\0')
    with zipfile.ZipFile(o.output/'MTOU-1.1.0-beta-source.zip','w',zipfile.ZIP_DEFLATED,compresslevel=9) as z:
        for name in sorted(filter(None,names)):
            p=source/name
            if not p.is_file(): raise SystemExit('Tracked source missing: '+name)
            z.writestr('Marmo-Town-of-Us/'+name,subprocess.check_output(['git','show',':'+name],cwd=source))
    sums=[]
    for p in [*sorted(o.output.glob('*.zip')),package/'mtou-release.json',package/'MTOU-Mods-Windows.zip',payload/'TOU-Marmo.dll',package/'Marmo Town of Us.exe']:
        sums.append(sha(p)+'  '+p.relative_to(o.output).as_posix())
    (o.output/'SHA256SUMS.txt').write_text('\n'.join(sums)+'\n',encoding='utf-8')
    print('\n'.join(sums))

if __name__=='__main__': main()
