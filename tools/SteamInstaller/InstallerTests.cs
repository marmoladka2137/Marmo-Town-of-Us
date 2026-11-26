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
    [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
    static extern Microsoft.Win32.SafeHandles.SafeFileHandle CreateFile(string path, uint access, uint share, IntPtr security, uint creation, uint flags, IntPtr template);
    static void Check(bool value, string name) { if (!value) throw new Exception("FAIL: " + name); checks++; Console.WriteLine("PASS: " + name); }
    static void Reject(Action action, string name) { bool rejected = false; try { action(); } catch (IOException) { rejected = true; } Check(rejected, name); }
    static void Put(string path, string text) { Directory.CreateDirectory(Path.GetDirectoryName(path)); File.WriteAllText(path, text); }
    static void VersionData(string path, params string[] versions)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        using (var stream = File.Create(path)) using (var writer = new BinaryWriter(stream))
        {
            writer.Write(new byte[64]);
            foreach (string version in versions)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(version); writer.Write(bytes.Length); writer.Write(bytes);
                while (stream.Position % 4 != 0) writer.Write((byte)0);
            }
        }
    }
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
            if (args.Length == 2 && args[0] == "--game-version")
            {
                Check(InstallerCore.ReadGameVersion(args[1]) == "2026.9.29", "Read the installed game's actual Unity version record");
                return 0;
            }
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
            string compact = Path.Combine(root, "compact package"); Directory.CreateDirectory(compact);
            File.Copy(Zip("supplement.zip", new Dictionary<string, string> { { "BepInEx/plugins/TOU-Marmo.dll", "fixture mod" } }), Path.Combine(compact, "MTOU-Mods-Windows.zip"));
            var compactEngine = new InstallerCore(manifest, compact);
            Check(File.ReadAllText(compactEngine.ResolvePayload()) == "fixture mod", "Resolve the pinned plugin directly from the supplement ZIP");
            Check(!Directory.Exists(Path.Combine(compact, "Payload")), "Compact launcher needs no duplicate loose plugin");
            string payloadHash = manifest.Payload.Sha256; manifest.Payload.Sha256 = new string('F',64);
            Reject(() => compactEngine.ResolvePayload(), "Reject an extracted plugin with the wrong SHA-256");
            manifest.Payload.Sha256 = payloadHash;
            string epic = Path.Combine(root, "Epic Among Us"), epicManifests = Path.Combine(root, "Epic manifests");
            Put(Path.Combine(epic, "Among Us.exe"), "fixture");
            Put(Path.Combine(epic, "GameAssembly.dll"), "Epic assembly differs from Steam");
            string versionFile = Path.Combine(epic, "Among Us_Data", "globalgamemanagers");
            VersionData(versionFile, "2026.9.29");
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            string epicManifest = Path.Combine(epicManifests, "among.item");
            Put(epicManifest, serializer.Serialize(new { DisplayName = "Among Us", InstallLocation = epic, AppVersionString = "opaque-store-build-98765" }));
            var epicEngine = new InstallerCore(new InstallerManifest { Platform = "Epic", GameAssemblySha256 = "steam-only-hash" }, package);
            epicEngine.ValidateSource(epic, epicManifests);
            Check(true, "Accept supported Epic game with opaque storefront version and its own assembly hash");
            Check(InstallerCore.ReadGameVersion(versionFile) == "2026.9.29", "Read the complete length-prefixed game version");
            VersionData(versionFile, "2026.09.29");
            Check(InstallerCore.ReadGameVersion(versionFile) == "2026.9.29", "Normalize zero-padded calendar version");
            VersionData(versionFile, "2026.10.27");
            Put(epicManifest, serializer.Serialize(new { DisplayName = "Among Us", InstallLocation = epic, AppVersionString = "2026.9.29" }));
            Reject(() => epicEngine.ValidateSource(epic, epicManifests), "Actual incompatible game version overrides a stale storefront label");
            VersionData(versionFile, "2026.9.29", "2026.10.27");
            Reject(() => InstallerCore.ReadGameVersion(versionFile), "Do not guess between conflicting date versions");
            Reject(() => epicEngine.ValidateSource(epic, epicManifests), "A storefront label cannot override ambiguous game data");
            Put(versionFile, "not a version record");
            Put(epicManifest, serializer.Serialize(new { DisplayName = "Among Us", InstallLocation = epic, AppVersionString = "opaque-build" }));
            Reject(() => epicEngine.ValidateSource(epic, epicManifests), "Unknown file and opaque store version remain blocked");
            Check(InstallerCore.SupportedEpicVersion(null, new[] { "19.0.0" }), "Accept the documented storefront alias when no game date is available");
            Check(!InstallerCore.SupportedEpicVersion(null, new[] { "19.0.01", "2026.9.290", "prefix2026.9.29" }), "Reject partial version matches");
            Check(!InstallerCore.SupportedEpicVersion("2026.10.27", new[] { "19.0.0" }), "Store alias cannot override an unsupported installed game");
            VersionData(versionFile, "2026.9.29");
            Reject(() => epicEngine.ValidateSource(epic, Path.Combine(root, "missing manifests")), "An unregistered Epic folder remains blocked");
            Put(Path.Combine(epicManifests, "broken.item"), "{invalid");
            epicEngine.ValidateSource(epic, epicManifests);
            Check(true, "A broken unrelated manifest does not block a valid Epic installation");
            File.Delete(Path.Combine(epic, "GameAssembly.dll"));
            Reject(() => epicEngine.ValidateSource(epic, epicManifests), "Reject an incomplete Epic installation");
            string lockedStage = Path.Combine(root, "locked.installing"), published = Path.Combine(root, "completed");
            Put(Path.Combine(lockedStage, "complete.txt"), "verified content");
            using (var handle = CreateFile(Path.Combine(lockedStage, "complete.txt"), 0x80000000, 1, IntPtr.Zero, 3, 0, IntPtr.Zero))
            {
                Check(!handle.IsInvalid, "Acquire a real Windows file handle without delete sharing");
                bool blocked = false;
                try { Directory.Move(lockedStage, published); }
                catch (UnauthorizedAccessException) { blocked = true; }
                catch (IOException) { blocked = true; }
                Check(blocked, "Reproduce the directory rename failure with a real Windows lock");
                var releaseLock = new System.Threading.Thread(() => { System.Threading.Thread.Sleep(500); handle.Dispose(); });
                releaseLock.Start();
                try { InstallerCore.PublishInstallation(lockedStage, published); }
                finally { releaseLock.Join(); }
            }
            Check(File.ReadAllText(Path.Combine(published, "complete.txt")) == "verified content", "Publish a completed installation after a temporary Windows lock releases");
            Check(!Directory.Exists(lockedStage), "Successful publish leaves no staging directory");
            string competing = Path.Combine(root, "competing.installing");
            Put(Path.Combine(competing, "complete.txt"), "new content");
            Reject(() => InstallerCore.PublishInstallation(competing, published), "Never overwrite a destination created by another process");
            Check(File.ReadAllText(Path.Combine(published, "complete.txt")) == "verified content" && Directory.Exists(competing), "Destination conflict preserves both original and staging content");
            string permanent = Path.Combine(root, "permanent.installing"), neverPublished = Path.Combine(root, "never-published");
            Put(Path.Combine(permanent, "complete.txt"), "retained");
            using (var handle = CreateFile(Path.Combine(permanent, "complete.txt"), 0x80000000, 1, IntPtr.Zero, 3, 0, IntPtr.Zero))
            {
                bool blocked = false;
                try { InstallerCore.PublishInstallation(permanent, neverPublished); }
                catch (IOException) { blocked = true; }
                catch (UnauthorizedAccessException) { blocked = true; }
                Check(blocked, "A permanent Windows lock stops after bounded retries");
            }
            Check(!Directory.Exists(neverPublished) && File.ReadAllText(Path.Combine(permanent, "complete.txt")) == "retained", "Failed publication preserves staging without claiming success");
            Console.WriteLine("PASS: " + checks + " installer checks. Fixtures retained at " + root);
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}

