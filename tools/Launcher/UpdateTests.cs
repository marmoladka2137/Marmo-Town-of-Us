using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Web.Script.Serialization;
using Marmo.Launcher;
using Marmo.SteamInstaller;

public static class UpdateTests
{
    static int checks;
    static void Check(bool value, string message) { if (!value) throw new Exception(message); checks++; Console.WriteLine("PASS " + message); }
    static void Reject(Action action, string message) { bool failed = false; try { action(); } catch (IOException) { failed = true; } Check(failed, message); }
    static byte[] Zip(string path, byte[] bytes)
    {
        using (var output = new MemoryStream())
        {
            using (var zip = new ZipArchive(output, ZipArchiveMode.Create, true))
                using (var stream = zip.CreateEntry(path).Open()) stream.Write(bytes, 0, bytes.Length);
            return output.ToArray();
        }
    }
    public static void Main()
    {
        const string url = "https://github.com/marmoladka2137/Marmo-Town-of-Us/releases/download/vtest/";
        string feed = "[{\"draft\":true,\"published_at\":\"2030\",\"tag_name\":\"draft\",\"html_url\":\"x\",\"assets\":[]},"
            + "{\"draft\":false,\"published_at\":\"2026\",\"tag_name\":\"vtest\",\"html_url\":\"x\",\"assets\":["
            + "{\"name\":\"mtou-release.json\",\"browser_download_url\":\"" + url + "mtou-release.json\"},"
            + "{\"name\":\"MTOU-Mods-Windows.zip\",\"browser_download_url\":\"" + url + "MTOU-Mods-Windows.zip\"}]}]";
        Check(ReleaseUpdate.Select(feed).Tag == "vtest", "Choose a public release and skip drafts");
        Check(ReleaseUpdate.Select("[]") == null, "Handle a repository without eligible releases");
        Check(ReleaseUpdate.Latest(address => Encoding.UTF8.GetBytes("[]")).Tag == "public-downloads", "Use the public channel when no eligible Release exists");
        Check(ReleaseUpdate.Latest(address => { throw new System.Net.WebException("rate limit"); }).Tag == "public-downloads", "Use the public channel if the release API is unavailable");
        Check(ReleaseUpdate.Trusted(url + "x"), "Accept release assets in this repository");
        Check(!ReleaseUpdate.Trusted("https://github.com/other/repo/releases/download/v/x"), "Reject another repository");
        Check(ReleaseUpdate.Trusted(ReleaseUpdate.Channel().ManifestUrl), "Accept the public downloads channel");
        Check(!ReleaseUpdate.Trusted("https://raw.githubusercontent.com/other/repo/downloads/mtou-release.json"), "Reject another raw-file channel");
        Check(!ReleaseUpdate.Trusted("http://github.com/marmoladka2137/Marmo-Town-of-Us/releases/download/v/x"), "Require HTTPS");
        Check(ReleaseUpdate.Newer("1.1.0.18", "1.1.0.17"), "Detect a newer assembly revision");
        Check(!ReleaseUpdate.Newer("1.1.0.17", "1.1.0.18"), "Do not downgrade installations");
        Check(!ReleaseUpdate.Newer("bad", "1.1.0.17"), "Reject malformed update versions");
        var root = Path.Combine(Path.GetTempPath(), "MTOU-update-test-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(root);
        var bytes = Encoding.UTF8.GetBytes("new plugin contents"); string path = "BepInEx/plugins/TOU-Marmo.dll";
        var archive = Zip(path, bytes);
        var manifest = new ReleaseManifest { Schema = 1, Revision = "1.1.0.18", ArchiveSha256 = ReleaseUpdate.Hash(archive),
            Files = new[] { new ReleaseFile { Path = path, Bytes = bytes.Length, Sha256 = ReleaseUpdate.Hash(bytes) } } };
        ReleaseUpdate.Apply(root, manifest, archive);
        Check(File.ReadAllText(InstallerCore.SafePath(root, path)) == "new plugin contents", "Install a verified package");
        File.WriteAllText(InstallerCore.SafePath(root, path), "keep me");
        manifest.Files[0].PreserveExisting = true;
        ReleaseUpdate.Apply(root, manifest, archive);
        Check(File.ReadAllText(InstallerCore.SafePath(root, path)) == "keep me", "Preserve existing configuration entries");
        manifest.Files[0].PreserveExisting = false;
        Reject(() => ReleaseUpdate.Apply(root, manifest, new byte[] { 1, 2 }), "Reject a corrupt ZIP before installation");
        Check(File.ReadAllText(InstallerCore.SafePath(root, path)) == "keep me", "Corrupt downloads leave installed files intact");
        manifest.Files[0].Path = "../escape.dll";
        Reject(() => ReleaseUpdate.Validate(manifest), "Reject traversal in the manifest");
        manifest.Files[0].Path = "Among Us.exe";
        Reject(() => ReleaseUpdate.Validate(manifest), "Mod packages cannot replace the base game executable");
        manifest.Files[0].Path = path;
        var duplicate = Zip("BepInEx/plugins/other.dll", bytes);
        manifest.ArchiveSha256 = ReleaseUpdate.Hash(duplicate);
        Reject(() => ReleaseUpdate.Apply(root, manifest, duplicate), "Reject unexpected ZIP entries");
        manifest.ArchiveSha256 = ReleaseUpdate.Hash(archive);
        string locked = InstallerCore.SafePath(root, path); File.WriteAllText(locked, "original");
        using (var stream = new FileStream(locked, FileMode.Open, FileAccess.Read, FileShare.Read))
            Reject(() => ReleaseUpdate.Apply(root, manifest, archive), "Handle a locked installed file");
        Check(File.ReadAllText(locked) == "original", "Failed replacement preserves the original plugin");
        byte[] twoFiles;
        using (var output = new MemoryStream())
        {
            using (var zip = new ZipArchive(output, ZipArchiveMode.Create, true))
                foreach (var name in new[] { "BepInEx/plugins/first.dll", path })
                    using (var target = zip.CreateEntry(name).Open()) target.Write(bytes, 0, bytes.Length);
            twoFiles = output.ToArray();
        }
        string first = InstallerCore.SafePath(root, "BepInEx/plugins/first.dll"); File.WriteAllText(first, "first original");
        manifest.Files = new[] { new ReleaseFile { Path = "BepInEx/plugins/first.dll", Bytes = bytes.Length, Sha256 = ReleaseUpdate.Hash(bytes) }, manifest.Files[0] };
        manifest.ArchiveSha256 = ReleaseUpdate.Hash(twoFiles);
        using (var stream = new FileStream(locked, FileMode.Open, FileAccess.Read, FileShare.Read))
            Reject(() => ReleaseUpdate.Apply(root, manifest, twoFiles), "Reject a locked second file after the first replacement");
        Check(File.ReadAllText(first) == "first original" && File.ReadAllText(locked) == "original", "Roll back earlier replacements when a later file is locked");
        string epicRoot = Path.Combine(root, "epic-manifests"), game = Path.Combine(root, "epic-game");
        Directory.CreateDirectory(epicRoot); Directory.CreateDirectory(game); File.WriteAllText(Path.Combine(game, "Among Us.exe"), "fixture");
        File.WriteAllText(Path.Combine(epicRoot, "game.item"), new JavaScriptSerializer().Serialize(new { DisplayName = "Among Us", InstallLocation = game }));
        File.WriteAllText(Path.Combine(epicRoot, "broken.item"), "invalid JSON");
        Check(InstallerCore.FindEpicGames(epicRoot).Length == 1, "Discover Epic Among Us from its installation manifest");
        Console.WriteLine("PASS " + checks + " updater and Epic discovery assertions.");
    }
}
