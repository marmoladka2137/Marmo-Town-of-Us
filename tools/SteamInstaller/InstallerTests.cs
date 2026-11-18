using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Marmo.SteamInstaller;

public static class InstallerTests
{
    static int checks;
    static string root;
    static void Check(bool value, string name) { if (!value) throw new Exception("FAIL: " + name); checks++; Console.WriteLine("PASS: " + name); }
    static void Reject(Action action, string name) { bool rejected = false; try { action(); } catch (IOException) { rejected = true; } Check(rejected, name); }
    static void Put(string path, string text) { Directory.CreateDirectory(Path.GetDirectoryName(path)); File.WriteAllText(path, text); }
    static FilePin Pin(string path, string relative) { return new FilePin { Path = relative, Bytes = new FileInfo(path).Length, Sha256 = InstallerCore.Hash(path) }; }
    static string Zip(string name, Dictionary<string, string> entries)
    {
        string path = Path.Combine(root, name);
        using (var archive = ZipFile.Open(path, ZipArchiveMode.Create))
            foreach (var entry in entries)
                using (var output = new StreamWriter(archive.CreateEntry(entry.Key).Open(), new UTF8Encoding(false))) output.Write(entry.Value);
        return path;
    }
    public static int Main(string[] args)
    {
        try
        {
            if (args.Length > 0 && args[0] == "--integration")
            {
                var core = new InstallerCore(InstallerManifest.Read(Path.Combine(args[1], "installer-manifest.json")), args[1]);
                core.Progress = (p, text) => { if (p % 10 == 0) Console.WriteLine(p + "% " + text); };
                core.Install(args[2], args[3], args.Length > 4 ? args[4] : null);
                Console.WriteLine("PASS: real Steam copy and official dependency installation completed."); return 0;
            }
            root = Path.Combine(Path.GetTempPath(), "MTOU-Installer-Tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            string library = Path.Combine(root, "Steam library with spaces");
            string source = Path.Combine(library, "steamapps", "common", "Among Us");
            Put(Path.Combine(source, "Among Us.exe"), "fixture game, never executed");
            Put(Path.Combine(source, "GameAssembly.dll"), "fixture compatible assembly");
            Put(Path.Combine(source, "Among Us_Data", "level0"), "fixture game data");
            Put(Path.Combine(source, "BepInEx", "plugins", "Unrelated.dll"), "private mod");
            Put(Path.Combine(source, "dotnet", "private.txt"), "private runtime");
            Put(Path.Combine(source, "settings.amogus"), "private settings");
            Put(Path.Combine(source, "unrelated.exe"), "unrelated executable");
            Put(Path.Combine(library, "steamapps", "appmanifest_945360.acf"), "\"AppState\"\n{\n \"appid\" \"945360\"\n \"installdir\" \"Among Us\"\n}");
            string package = Path.Combine(root, "installer package");
            Put(Path.Combine(package, "Payload", "TOU-Marmo.dll"), "fixture mod");
            Put(Path.Combine(package, "Marmo Town of Us.exe"), "fixture launcher");
            Put(Path.Combine(package, "Notices", "LICENSE"), "fixture notice");
            string zip = Zip("dependencies.zip", new Dictionary<string, string> {
                {"pack/BepInEx/core/Loader.dll", "fixture loader"},
                {"pack/BepInEx/plugins/MiraAPI.dll", "fixture API"},
                {"pack/BepInEx/plugins/TownOfUsMira.dll", "excluded duplicate"},
                {"pack/BepInEx/plugins/AUnlocker.dll", "excluded mod"},
                {"pack/Among Us.exe", "excluded game executable"}
            });
            var pins = new List<FilePin>();
            foreach (var entry in new[] { new[] { "BepInEx/core/Loader.dll", "fixture loader" }, new[] { "BepInEx/plugins/MiraAPI.dll", "fixture API" } })
            {
                string file = Path.Combine(root, "pins", entry[0]); Put(file, entry[1]); pins.Add(Pin(file, entry[0]));
            }
            var manifest = new InstallerManifest {
                Version = "test", AssemblyRevision = "test", SourceCommit = "test",
                GameAssemblySha256 = InstallerCore.Hash(Path.Combine(source, "GameAssembly.dll")),
                ArchiveSha256 = InstallerCore.Hash(zip), ArchivePrefix = "pack", ArchiveUrl = "http://invalid.invalid/never-requested",
                Payload = Pin(Path.Combine(package, "Payload", "TOU-Marmo.dll"), "Payload/TOU-Marmo.dll"), RuntimeFiles = pins.ToArray(),
                Launcher = Pin(Path.Combine(package, "Marmo Town of Us.exe"), "Marmo Town of Us.exe")
            };
            var engine = new InstallerCore(manifest, package);
            Check(InstallerCore.Full(InstallerCore.GameInLibrary(library)) == InstallerCore.Full(source), "Steam manifest resolves a folder with spaces");
            Put(Path.Combine(library, "steamapps", "libraryfolders.vdf"), "\"libraryfolders\" { \"1\" { \"path\" \"" + library.Replace("\\", "\\\\") + "\" } }");
            Check(InstallerCore.LibraryPaths(library).Count() == 1, "VDF paths are unescaped and deduplicated");
            foreach (string unsafePath in new[] { "../outside", "a/../../outside", "a\\..\\outside", "C:\\outside", "x:y", "/outside" })
                Reject(() => InstallerCore.SafePath(root, unsafePath), "Reject archive path " + unsafePath);
            Reject(() => InstallerCore.ValidateDestination(source, source, package), "Reject existing source as destination");
            Reject(() => InstallerCore.ValidateDestination(source, Path.Combine(source, "new"), package), "Reject nested game destination");
            Reject(() => InstallerCore.ValidateDestination(source, Path.Combine(package, "new"), package), "Reject nested package destination");
            string destination = Path.Combine(root, "installed copy");
            string originalGameHash = InstallerCore.Hash(Path.Combine(source, "Among Us.exe"));
            engine.Install(source, destination, zip);
            Check(File.Exists(Path.Combine(destination, "Among Us.exe")), "Copy the local game executable");
            Check(File.ReadAllText(Path.Combine(destination, "Among Us_Data", "level0")) == "fixture game data", "Copy game data");
            Check(InstallerCore.Hash(Path.Combine(destination, "Among Us.exe")) == originalGameHash, "The game's executable is not patched");
            Check(!Directory.Exists(Path.Combine(destination, "dotnet")), "Ignore the source's runtime and private files");
            Check(!File.Exists(Path.Combine(destination, "settings.amogus")), "Exclude private settings");
            Check(!File.Exists(Path.Combine(destination, "unrelated.exe")), "Exclude unrelated root executables");
            Check(!File.Exists(Path.Combine(destination, "BepInEx", "plugins", "Unrelated.dll")), "Exclude source plugins");
            Check(!File.Exists(Path.Combine(destination, "BepInEx", "plugins", "TownOfUsMira.dll")), "Exclude original ToU plugin");
            Check(!File.Exists(Path.Combine(destination, "BepInEx", "plugins", "AUnlocker.dll")), "Exclude unselected downloaded plugins");
            Check(File.ReadAllText(Path.Combine(destination, "BepInEx", "plugins", "TOU-Marmo.dll")) == "fixture mod", "Install the verified MTOU payload");
            Check(File.Exists(Path.Combine(destination, "MTOU Notices", "LICENSE")), "Install notices");
            Check(File.ReadAllText(Path.Combine(destination, "Marmo Town of Us.exe")) == "fixture launcher", "Install the pinned MTOU launcher");
            Check(File.ReadAllText(Path.Combine(destination, "steam_appid.txt")).Trim() == "945360", "Write the Steam AppID without modifying DRM");
            Reject(() => engine.Install(source, destination, zip), "Never overwrite a completed installation");
            Check(InstallerCore.Hash(Path.Combine(source, "Among Us.exe")) == originalGameHash, "Steam source remains unchanged");
            string badArchive = Path.Combine(root, "bad.zip"); Put(badArchive, "corrupt");
            Reject(() => engine.Install(source, Path.Combine(root, "bad-install"), badArchive), "Reject a dependency hash mismatch");
            Check(!Directory.Exists(Path.Combine(root, "bad-install")), "Hash failure publishes no folder");
            string oldHash = manifest.GameAssemblySha256; manifest.GameAssemblySha256 = new string('0', 64);
            Reject(() => engine.Install(source, Path.Combine(root, "wrong-version"), zip), "Reject an unsupported game build");
            manifest.GameAssemblySha256 = oldHash;
            string malicious = Zip("traversal.zip", new Dictionary<string, string> { { "../outside.txt", "malicious" } });
            Reject(() => engine.ExtractRuntime(malicious, Path.Combine(root, "traversal-target")), "Reject traversal even in an unselected entry");
            Check(!File.Exists(Path.Combine(root, "outside.txt")), "Traversal creates no escaped file");
            string missing = Zip("missing.zip", new Dictionary<string, string> { { "pack/ignored.txt", "unselected" } });
            string archiveHash = manifest.ArchiveSha256; manifest.ArchiveSha256 = InstallerCore.Hash(missing);
            Reject(() => engine.Install(source, Path.Combine(root, "incomplete"), missing), "Missing runtime file fails installation");
            Check(!Directory.Exists(Path.Combine(root, "incomplete")), "A partial install is never published as ready");
            Check(Directory.GetDirectories(root, "incomplete.installing-*").Length == 1, "Retain only the explicitly marked incomplete staging folder");
            manifest.ArchiveSha256 = archiveHash;
            Put(Path.Combine(package, "Marmo Town of Us.exe"), "modified launcher");
            Reject(() => engine.Install(source, Path.Combine(root, "bad-launcher"), zip), "Reject a modified launcher");
            Put(Path.Combine(package, "Marmo Town of Us.exe"), "fixture launcher");
            Put(Path.Combine(package, "Payload", "TOU-Marmo.dll"), "corrupt payload");
            Reject(() => engine.Install(source, Path.Combine(root, "bad-payload"), zip), "Reject a modified MTOU payload");
            Console.WriteLine("PASS: " + checks + " installer checks. Fixtures retained at " + root);
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
