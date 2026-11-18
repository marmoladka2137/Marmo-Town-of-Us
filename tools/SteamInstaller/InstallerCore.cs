// Marmo Town of Us installer. GPL-3.0; see the repository LICENSE.
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using Microsoft.Win32;

namespace Marmo.SteamInstaller
{
    public sealed class FilePin
    {
        public string Path { get; set; }
        public string Sha256 { get; set; }
        public long Bytes { get; set; }
    }
    public sealed class InstallerManifest
    {
        public string Version { get; set; }
        public string AssemblyRevision { get; set; }
        public string SourceCommit { get; set; }
        public string ArchiveUrl { get; set; }
        public string ArchiveSha256 { get; set; }
        public string ArchivePrefix { get; set; }
        public string GameAssemblySha256 { get; set; }
        public FilePin Payload { get; set; }
        public FilePin Launcher { get; set; }
        public FilePin[] RuntimeFiles { get; set; }
        public static InstallerManifest Read(string filename)
        {
            return new JavaScriptSerializer().Deserialize<InstallerManifest>(File.ReadAllText(filename));
        }
    }

    public sealed class InstallerCore
    {
        public readonly InstallerManifest Manifest;
        public readonly string PackageRoot;
        public Action<int, string> Progress = delegate { };
        public Action<string> Log = delegate { };
        static readonly string[] RootFiles = {
            "Among Us.exe", "UnityPlayer.dll", "GameAssembly.dll", "baselib.dll",
            "UnityCrashHandler64.exe", "crashpad_handler.exe", "msvcp140.dll",
            "vcruntime140.dll", "vcruntime140_1.dll", "steam_api64.dll"
        };
        public InstallerCore(InstallerManifest manifest, string packageRoot)
        {
            Manifest = manifest; PackageRoot = Path.GetFullPath(packageRoot);
        }
        public static string Hash(string filename)
        {
            using (var hash = SHA256.Create()) using (var stream = File.OpenRead(filename))
                return BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", "");
        }
        public static string Full(string path) { return Path.GetFullPath(path).TrimEnd('\\', '/'); }
        public static bool Within(string path, string root)
        {
            path = Full(path); root = Full(root);
            return path.Equals(root, StringComparison.OrdinalIgnoreCase)
                || path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        }
        public static void NoLinks(string path)
        {
            string current = Path.GetFullPath(path);
            while (!String.IsNullOrEmpty(current))
            {
                if ((File.Exists(current) || Directory.Exists(current))
                    && (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                    throw new IOException("Folder contains a link/junction: " + current);
                var parent = Directory.GetParent(current);
                current = parent == null ? null : parent.FullName;
            }
        }
        public static string SafePath(string root, string relative)
        {
            if (String.IsNullOrWhiteSpace(relative) || Path.IsPathRooted(relative)
                || relative.Contains(":") || relative.Split('/', '\\').Any(p => p == ".."))
                throw new IOException("Unsafe archive path: " + relative);
            string path = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
            if (!Within(path, root) || Full(path) == Full(root)) throw new IOException("Unsafe archive entry.");
            return path;
        }
        static string VdfString(string text, string key)
        {
            var match = Regex.Match(text, "\"" + Regex.Escape(key) + "\"\\s*\"([^\"]*)\"", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value.Replace("\\\\", "\\") : null;
        }
        public static IEnumerable<string> LibraryPaths(string steamRoot)
        {
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            paths.Add(Full(steamRoot));
            string vdf = Path.Combine(steamRoot, "steamapps", "libraryfolders.vdf");
            if (File.Exists(vdf))
                foreach (Match match in Regex.Matches(File.ReadAllText(vdf), "\"path\"\\s*\"([^\"]+)\"", RegexOptions.IgnoreCase))
                    paths.Add(Full(match.Groups[1].Value.Replace("\\\\", "\\")));
            return paths;
        }
        public static string GameInLibrary(string library)
        {
            string manifest = Path.Combine(library, "steamapps", "appmanifest_945360.acf");
            if (!File.Exists(manifest)) return null;
            string content = File.ReadAllText(manifest);
            if (VdfString(content, "appid") != "945360") return null;
            string dir = VdfString(content, "installdir");
            if (String.IsNullOrWhiteSpace(dir)) return null;
            string game = SafePath(Path.Combine(library, "steamapps", "common"), dir);
            return File.Exists(Path.Combine(game, "Among Us.exe")) ? game : null;
        }
        public static string[] FindSteamGames()
        {
            var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (RegistryView view in new[] { RegistryView.Registry32, RegistryView.Registry64 })
                foreach (RegistryHive hive in new[] { RegistryHive.CurrentUser, RegistryHive.LocalMachine })
                    try
                    {
                        using (var baseKey = RegistryKey.OpenBaseKey(hive, view))
                        using (var key = baseKey.OpenSubKey(@"Software\Valve\Steam"))
                            if (key != null)
                                foreach (string name in new[] { "SteamPath", "InstallPath" })
                                {
                                    string value = key.GetValue(name) as string;
                                    if (!String.IsNullOrWhiteSpace(value)) roots.Add(value);
                                }
                    } catch (System.Security.SecurityException) { } catch (UnauthorizedAccessException) { }
            roots.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam"));
            var games = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string root in roots)
                if (Directory.Exists(root))
                    foreach (string library in LibraryPaths(root))
                    {
                        string game = GameInLibrary(library);
                        if (game != null) games.Add(game);
                    }
            return games.ToArray();
        }
        public void ValidateSource(string source)
        {
            NoLinks(source);
            if (!File.Exists(Path.Combine(source, "Among Us.exe")) || !Directory.Exists(Path.Combine(source, "Among Us_Data")))
                throw new IOException("Select the Among Us game folder from Steam > Properties > Installed Files > Browse.");
            var common = Directory.GetParent(Full(source));
            if (common == null || common.Parent == null || common.Parent.Parent == null
                || !String.Equals(common.Name, "common", StringComparison.OrdinalIgnoreCase)
                || !String.Equals(common.Parent.Name, "steamapps", StringComparison.OrdinalIgnoreCase))
                throw new IOException("The source must be an installed Steam game, not a downloaded game copy.");
            string installed = GameInLibrary(common.Parent.Parent.FullName);
            if (installed == null || !Full(installed).Equals(Full(source), StringComparison.OrdinalIgnoreCase))
                throw new IOException("Steam's Among Us installation manifest does not match this folder.");
            string assembly = Path.Combine(source, "GameAssembly.dll");
            if (!File.Exists(assembly) || !Hash(assembly).Equals(Manifest.GameAssemblySha256, StringComparison.OrdinalIgnoreCase))
                throw new IOException("This game build is not the MTOU 1.1.0 Beta reference build. No files were changed. Check the supported version in the README.");
        }
        public static void ValidateDestination(string source, string destination, string packageRoot)
        {
            NoLinks(destination);
            if (Directory.Exists(destination) || File.Exists(destination))
                throw new IOException("The destination already exists. Choose a NEW folder; existing installations are never overwritten.");
            if (Within(destination, source) || Within(source, destination)
                || Within(destination, packageRoot) || Within(packageRoot, destination))
                throw new IOException("Choose a separate destination outside Steam's game folder and outside the installer package.");
            if (Full(destination).Equals(Full(Path.GetPathRoot(destination)), StringComparison.OrdinalIgnoreCase))
                throw new IOException("A drive root cannot be the destination.");
        }
        public List<string> GameFiles(string source)
        {
            var files = new List<string>();
            foreach (string name in RootFiles) if (File.Exists(Path.Combine(source, name))) files.Add(Path.Combine(source, name));
            var pending = new Stack<string>(); pending.Push(Path.Combine(source, "Among Us_Data"));
            while (pending.Count != 0)
            {
                string dir = pending.Pop(); NoLinks(dir);
                foreach (string file in Directory.GetFiles(dir)) { NoLinks(file); files.Add(file); }
                foreach (string child in Directory.GetDirectories(dir)) pending.Push(child);
            }
            foreach (string file in files) NoLinks(file);
            return files;
        }
        public static void CheckPin(string filename, FilePin pin)
        {
            NoLinks(filename);
            if (!File.Exists(filename) || new FileInfo(filename).Length != pin.Bytes
                || !Hash(filename).Equals(pin.Sha256, StringComparison.OrdinalIgnoreCase))
                throw new IOException("File verification failed: " + pin.Path);
        }
        public string GetDependencies(string archiveOverride)
        {
            if (!String.IsNullOrEmpty(archiveOverride))
            {
                if (!Hash(archiveOverride).Equals(Manifest.ArchiveSha256, StringComparison.OrdinalIgnoreCase))
                    throw new IOException("Dependency archive SHA-256 does not match. Nothing installed.");
                return archiveOverride;
            }
            string cache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MTOU", "InstallerCache");
            Directory.CreateDirectory(cache); NoLinks(cache);
            string filename = Path.Combine(cache, Manifest.ArchiveSha256 + ".zip");
            if (File.Exists(filename))
            {
                if (Hash(filename).Equals(Manifest.ArchiveSha256, StringComparison.OrdinalIgnoreCase)) return filename;
                throw new IOException("Cached dependency archive is corrupt. Delete this single cache file and retry: " + filename);
            }
            var uri = new Uri(Manifest.ArchiveUrl);
            if (uri.Scheme != "https" || uri.Host != "github.com") throw new IOException("Untrusted dependency download URL.");
            string temporary = filename + "." + Guid.NewGuid().ToString("N") + ".partial";
            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                var request = (HttpWebRequest)WebRequest.Create(uri);
                request.UserAgent = "Marmo-Town-of-Us-Steam-Installer/1.0";
                request.Timeout = 30000; request.ReadWriteTimeout = 30000;
                using (var response = request.GetResponse()) using (var input = response.GetResponseStream())
                using (var output = new FileStream(temporary, FileMode.CreateNew))
                {
                    byte[] buffer = new byte[1024 * 1024]; int read; long done = 0;
                    while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        output.Write(buffer, 0, read); done += read;
                        Progress(response.ContentLength > 0 ? (int)Math.Min(40, done * 40 / response.ContentLength) : 15, "Downloading official dependencies: " + (done / 1048576) + " MB");
                    }
                }
                if (!Hash(temporary).Equals(Manifest.ArchiveSha256, StringComparison.OrdinalIgnoreCase))
                    throw new IOException("Downloaded dependency archive failed SHA-256 verification.");
                File.Move(temporary, filename); return filename;
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }
        public void ExtractRuntime(string archive, string stage)
        {
            var pins = Manifest.RuntimeFiles.ToDictionary(p => p.Path, StringComparer.OrdinalIgnoreCase);
            var extracted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var zip = ZipFile.OpenRead(archive))
                foreach (var entry in zip.Entries)
                {
                    // Validate even unselected entries; never tolerate traversal or absolute archive paths.
                    SafePath(stage, entry.FullName.TrimEnd('/', '\\'));
                    if (!entry.FullName.StartsWith(Manifest.ArchivePrefix + "/", StringComparison.Ordinal)) continue;
                    string relative = entry.FullName.Substring(Manifest.ArchivePrefix.Length + 1);
                    FilePin pin;
                    if (!pins.TryGetValue(relative, out pin)) continue;
                    if (!extracted.Add(relative)) throw new IOException("Duplicate dependency archive entry: " + relative);
                    string destination = SafePath(stage, relative);
                    if (entry.Length != pin.Bytes) throw new IOException("Unexpected dependency size: " + relative);
                    Directory.CreateDirectory(Path.GetDirectoryName(destination));
                    using (var input = entry.Open()) using (var output = new FileStream(destination, FileMode.CreateNew)) input.CopyTo(output);
                    CheckPin(destination, pin);
                }
            if (extracted.Count != pins.Count) throw new IOException("Required dependency files are missing from the archive.");
        }
        public string Install(string source, string destination, string archiveOverride)
        {
            source = Full(source); destination = Full(destination);
            ValidateSource(source); ValidateDestination(source, destination, PackageRoot);
            CheckPin(SafePath(PackageRoot, Manifest.Payload.Path), Manifest.Payload);
            if (Manifest.Launcher != null) CheckPin(SafePath(PackageRoot, Manifest.Launcher.Path), Manifest.Launcher);
            var files = GameFiles(source);
            long size = files.Sum(f => new FileInfo(f).Length) + Manifest.RuntimeFiles.Sum(f => f.Bytes) + Manifest.Payload.Bytes;
            var drive = new DriveInfo(Path.GetPathRoot(destination));
            if (drive.AvailableFreeSpace < size + 256L * 1024 * 1024) throw new IOException("Not enough free disk space for a separate game copy.");
            string archive = GetDependencies(archiveOverride);
            ValidateDestination(source, destination, PackageRoot);
            string stage = destination + ".installing-" + Guid.NewGuid().ToString("N").Substring(0, 8);
            Directory.CreateDirectory(stage);
            try
            {
                long copied = 0;
                foreach (string file in files)
                {
                    string relative = file.Substring(source.Length).TrimStart('\\', '/');
                    string target = SafePath(stage, relative);
                    Directory.CreateDirectory(Path.GetDirectoryName(target)); File.Copy(file, target, false);
                    copied += new FileInfo(file).Length;
                    Progress(42 + (int)(copied * 40 / Math.Max(1, size)), "Copying your Steam game: " + (copied / 1048576) + " MB");
                }
                Progress(84, "Installing verified loader and required plugins...");
                ExtractRuntime(archive, stage);
                string plugin = Path.Combine(stage, "BepInEx", "plugins", "TOU-Marmo.dll");
                Directory.CreateDirectory(Path.GetDirectoryName(plugin));
                File.Copy(SafePath(PackageRoot, Manifest.Payload.Path), plugin, false);
                CheckPin(plugin, Manifest.Payload);
                if (Manifest.Launcher != null)
                {
                    string launcher = Path.Combine(stage, "Marmo Town of Us.exe");
                    File.Copy(SafePath(PackageRoot, Manifest.Launcher.Path), launcher, false);
                    CheckPin(launcher, Manifest.Launcher);
                }
                File.WriteAllText(Path.Combine(stage, "steam_appid.txt"), "945360\n", Encoding.ASCII);
                File.WriteAllText(Path.Combine(stage, "Play MTOU.cmd"), "@echo off\r\ncd /d \"%~dp0\"\r\nstart \"\" \"%~dp0Among Us.exe\"\r\n", Encoding.ASCII);
                string notices = Path.Combine(PackageRoot, "Notices");
                if (Directory.Exists(notices))
                    foreach (string file in Directory.GetFiles(notices, "*", SearchOption.AllDirectories))
                    {
                        NoLinks(file); string relative = file.Substring(notices.Length).TrimStart('\\', '/');
                        string target = SafePath(Path.Combine(stage, "MTOU Notices"), relative);
                        Directory.CreateDirectory(Path.GetDirectoryName(target)); File.Copy(file, target, false);
                    }
                File.WriteAllText(Path.Combine(stage, "MTOU-INSTALLATION.txt"),
                    "Marmo Town of Us " + Manifest.Version + ", revision " + Manifest.AssemblyRevision + "\r\n"
                    + "Game files were copied locally from your installed Steam game. Keep Steam running.\r\n"
                    + "Source: https://github.com/marmoladka2137/Marmo-Town-of-Us/tree/" + Manifest.SourceCommit + "\r\n"
                    + "Required dependencies were downloaded directly from the official upstream release.\r\n"
                    + "Do not redistribute this installed folder: it now contains the Among Us base game.\r\n"
                    + "First launch generates IL2CPP wrappers and can take several minutes.\r\n", Encoding.UTF8);
                if (File.Exists(Path.Combine(stage, "BepInEx", "plugins", "TownOfUsMira.dll"))) throw new IOException("Conflicting plugin detected.");
                // Publish the completed folder once; Directory.Move refuses an existing destination.
                Directory.Move(stage, destination);
                Progress(100, "Ready. Keep Steam running and use Play MTOU.cmd.");
                Log("Installed MTOU " + Manifest.AssemblyRevision + "; SHA256 " + Manifest.Payload.Sha256);
                return destination;
            }
            catch (Exception error)
            {
                if (Directory.Exists(stage)) File.WriteAllText(Path.Combine(stage, "INSTALL_FAILED.txt"), error.ToString());
                throw new IOException(error.Message + "\r\nIncomplete files, if any, are in: " + stage, error);
            }
        }
    }
}
