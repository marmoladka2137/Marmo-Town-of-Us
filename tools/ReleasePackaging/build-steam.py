"""Build a mod-only Steam installation from pinned public upstream files."""
from pathlib import Path
import argparse
import hashlib
import io
import json
import urllib.request
import zipfile

def digest(data):
    return hashlib.sha256(data).hexdigest().upper()

def download(url, expected, size=None):
    request = urllib.request.Request(url, headers={'User-Agent': 'MTOU-release-packaging/1.1.0'})
    with urllib.request.urlopen(request, timeout=90) as response:
        data = response.read(256 * 1024 * 1024 + 1)
    assert len(data) <= 256 * 1024 * 1024
    assert digest(data) == expected.upper(), 'Download hash mismatch'
    assert size is None or len(data) == size, 'Download size mismatch'
    return data

def build(config, output):
    loader = download(config['ArchiveUrl'], config['ArchiveSha256'])
    feed = download(config['UpdateManifestUrl'], config['UpdateManifestSha256'])
    release = json.loads(feed)
    assert release['Revision'] == config['AssemblyRevision']
    channel = config['UpdateManifestUrl'].rsplit('/', 1)[0] + '/'
    pieces = []
    for part in release['ArchiveParts']:
        assert '/' not in part['Path'] and '\\' not in part['Path'] and ':' not in part['Path']
        pieces.append(download(channel + part['Path'], part['Sha256'], part['Bytes']))
    mods = b''.join(pieces)
    assert digest(mods) == release['ArchiveSha256']
    entries = {}
    pinned = []
    for payload, files, prefix in [(loader, config['RuntimeFiles'], config['ArchivePrefix'] + '/'), (mods, release['Files'], '')]:
        with zipfile.ZipFile(io.BytesIO(payload)) as archive:
            for pin in files:
                name = pin['Path']
                assert not name.startswith(('/', '\\')) and '..' not in name.split('/') and ':' not in name
                data = archive.read(prefix + name)
                assert len(data) == pin['Bytes'] and digest(data) == pin['Sha256'].upper(), name
                if '/CorrespondingSource/' in name:
                    continue
                assert name not in entries
                entries[name] = data
                pinned.append(pin)
    entries['steam_appid.txt'] = b'945360\n'
    entries['MTOU-INSTALACJA.txt'] = '''Marmo Town of Us 1.1.0 Beta - Steam, Windows x64

1. Steam > Among Us > Properties > Installed Files > Browse.
2. Copy the entire game folder to a new separate folder.
3. Extract this ZIP into that copy, directly beside Among Us.exe.
4. Keep Steam running and signed in. Open Among Us.exe in the modded copy.

Po polsku: skopiuj folder swojej gry, rozpakuj ten ZIP do kopii obok Among Us.exe i uruchom go przy wlaczonym, zalogowanym Steamie.
Pierwszy start moze potrwac kilka minut i wymagac internetu.
Reference game build: Among Us 2026.9.29, Windows x64. Every player needs the same compatible mod version.
The base game is NOT included. Do not redistribute the resulting folder containing your game files.
Do not combine with a separate TownOfUsMira.dll or other MTOU versions.
This package is for Steam. Epic copies require their authentication starter.
Keep MTOU Notices and its licenses. Corresponding source locations are recorded there and in the public source repository.
'''.encode()
    entries['MTOU Notices/PACKAGE.txt'] = ('MTOU Steam package. Unchanged selected runtime files: ' + config['ArchiveUrl'] + '\nArchive SHA256: ' + config['ArchiveSha256'] + '\nUnchanged plugin source: https://github.com/marmoladka2137/Marmo-Town-of-Us/tree/' + release['SourceCommit'] + '\nOriginal third-party licenses and credits remain applicable. No game files, accounts, private profiles or source archives are included. Source locations and unresolved asset/source matching limitations remain in existing notices; this packaging is not new legal clearance.\n').encode()
    output.mkdir(parents=True, exist_ok=True)
    filename = output / 'MTOU-Steam-1.1.0-Beta.zip'
    with zipfile.ZipFile(filename, 'w', zipfile.ZIP_DEFLATED, compresslevel=9) as archive:
        for name, data in sorted(entries.items()):
            item = zipfile.ZipInfo(name, (2026, 11, 20, 0, 0, 0))
            item.compress_type = zipfile.ZIP_DEFLATED
            item.external_attr = 0o644 << 16
            archive.writestr(item, data, compresslevel=9)
    with zipfile.ZipFile(filename) as archive:
        assert archive.testzip() is None
        assert set(archive.namelist()) == set(entries)
        for name in archive.namelist():
            assert name not in ['Among Us.exe', 'GameAssembly.dll', 'BepInEx/plugins/TownOfUsMira.dll']
            assert not name.startswith(('Among Us_Data/', 'BepInEx/interop/', 'Source/'))
            assert not name.endswith(('.pdb', '.cs', '.csproj', '.sln'))
        for pin in pinned:
            data = archive.read(pin['Path'])
            assert len(data) == pin['Bytes'] and digest(data) == pin['Sha256'].upper()
        assert len([n for n in archive.namelist() if n.startswith('BepInEx/plugins/') and n.endswith('.dll')]) == 7
    result = {'file': filename.name, 'bytes': filename.stat().st_size, 'sha256': digest(filename.read_bytes()), 'verifiedPins': len(pinned), 'baseGameIncluded': False}
    (output / 'steam-verification.json').write_text(json.dumps(result, indent=2), encoding='utf-8')
    print(json.dumps(result))
    return result

if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--config', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    options = parser.parse_args()
    build(json.loads(options.config.read_text(encoding='utf-8')), options.output)
