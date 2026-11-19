// GPL-3.0. Public GitHub releases supply the update channel.
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Web.Script.Serialization;
using Marmo.SteamInstaller;

namespace Marmo.Launcher
{
    public sealed class ReleaseFile
    {
        public string Path { get; set; }
        public string Sha256 { get; set; }
        public long Bytes { get; set; }
        public bool PreserveExisting { get; set; }
    }
    public sealed class ReleaseManifest
    {
        public int Schema { get; set; }
        public string Revision { get; set; }
        public string Version { get; set; }
        public string ArchiveSha256 { get; set; }
        public ReleaseFile[] Files { get; set; }
    }
    public sealed class PublicRelease
    {
        public string Tag, Url, ManifestUrl, ArchiveUrl;
    }
    public static class ReleaseUpdate
    {
        public const string Repository = "marmoladka2137/Marmo-Town-of-Us";
        public const string Feed = "https://api.github.com/repos/" + Repository + "/releases?per_page=30";
        public const string PublicChannelRoot = "https://raw.githubusercontent.com/" + Repository + "/downloads/";
        public static PublicRelease Channel()
        {
            return new PublicRelease { Tag = "public-downloads", Url = "https://github.com/" + Repository + "/tree/downloads",
                ManifestUrl = PublicChannelRoot + "mtou-release.json", ArchiveUrl = PublicChannelRoot + "MTOU-Mods-Windows.zip" };
        }
        public static PublicRelease Latest(Func<string, byte[]> fetch = null)
        {
            try { return Select(System.Text.Encoding.UTF8.GetString((fetch ?? Download)(Feed))) ?? Channel(); }
            catch (WebException) { return Channel(); }
        }
        public static PublicRelease Select(string json)
        {
            var releases = new JavaScriptSerializer().Deserialize<List<Dictionary<string, object>>>(json);
            foreach (var release in releases.OrderByDescending(r => Convert.ToString(r["published_at"]), StringComparer.Ordinal))
            {
                if (Convert.ToBoolean(release["draft"])) continue;
                var assets = (System.Collections.ArrayList)release["assets"];
                var result = new PublicRelease { Tag = Convert.ToString(release["tag_name"]), Url = Convert.ToString(release["html_url"]) };
                foreach (Dictionary<string, object> asset in assets)
                {
                    string name = Convert.ToString(asset["name"]);
                    if (name == "mtou-release.json") result.ManifestUrl = Convert.ToString(asset["browser_download_url"]);
                    if (name == "MTOU-Mods-Windows.zip") result.ArchiveUrl = Convert.ToString(asset["browser_download_url"]);
                }
                if (result.ManifestUrl != null && result.ArchiveUrl != null) return result;
            }
            return null;
        }
        public static bool Trusted(string url)
        {
            Uri uri;
            return Uri.TryCreate(url, UriKind.Absolute, out uri) && uri.Scheme == "https"
                && (uri.Host == "api.github.com" && uri.AbsolutePath.StartsWith("/repos/" + Repository + "/", StringComparison.Ordinal)
                    || uri.Host == "github.com" && uri.AbsolutePath.StartsWith("/" + Repository + "/releases/download/", StringComparison.Ordinal)
                    || uri.Host == "raw.githubusercontent.com" && uri.AbsolutePath.StartsWith("/" + Repository + "/downloads/", StringComparison.Ordinal));
        }
        public static byte[] Download(string url)
        {
            if (!Trusted(url)) throw new IOException("The update address is outside the MTOU GitHub repository.");
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = "Marmo-Town-of-Us-Launcher/2.0";
            request.Timeout = 30000; request.ReadWriteTimeout = 30000;
            using (var response = request.GetResponse()) using (var input = response.GetResponseStream())
            using (var output = new MemoryStream())
            {
                byte[] block = new byte[65536]; int count;
                while ((count = input.Read(block, 0, block.Length)) > 0)
                {
                    if (output.Length + count > 256L * 1024 * 1024) throw new IOException("The update exceeds the supported package size.");
                    output.Write(block, 0, count);
                }
                return output.ToArray();
            }
        }
        public static string Hash(byte[] data)
        {
            using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(data)).Replace("-", "");
        }
        public static bool Newer(string candidate, string installed)
        {
            Version next, current;
            return Version.TryParse(candidate, out next) && (!Version.TryParse(installed, out current) || next > current);
        }
        public static void Validate(ReleaseManifest manifest)
        {
            Version revision;
            if (manifest == null || manifest.Schema != 1 || !Version.TryParse(manifest.Revision, out revision)
                || manifest.Files == null || manifest.Files.Length == 0
                || manifest.Files.Length > 500 || String.IsNullOrEmpty(manifest.ArchiveSha256))
                throw new IOException("Unsupported or incomplete release manifest.");
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            long total = 0;
            foreach (var file in manifest.Files)
            {
                InstallerCore.SafePath(Path.GetTempPath(), file.Path);
                total += file.Bytes;
                if (!paths.Add(file.Path.Replace(Path.DirectorySeparatorChar, '/')) || file.Bytes < 0 || file.Bytes > 128L * 1024 * 1024 || total > 256L * 1024 * 1024
                    || file.Sha256 == null || !System.Text.RegularExpressions.Regex.IsMatch(file.Sha256, "^[a-fA-F0-9]{64}$"))
                    throw new IOException("Invalid file entry in release manifest.");
                if (!file.Path.StartsWith("BepInEx/", StringComparison.OrdinalIgnoreCase)
                    && !file.Path.StartsWith("CorsacCosmetics/", StringComparison.OrdinalIgnoreCase)
                    && !file.Path.StartsWith("MTOU Notices/", StringComparison.OrdinalIgnoreCase)
                    && !new[] { "winhttp.dll", "doorstop_config.ini", ".doorstop_version" }.Contains(file.Path))
                    throw new IOException("Unexpected file in mod package: " + file.Path);
                if (file.Path.EndsWith("TownOfUsMira.dll", StringComparison.OrdinalIgnoreCase))
                    throw new IOException("The release contains a conflicting standalone ToU plugin.");
            }
        }
        // Validate the complete archive before mutating an installation. Restore overwritten
        // files and remove newly created files if any individual replacement fails.
        public static void Apply(string game, ReleaseManifest manifest, byte[] archive)
        {
            Validate(manifest);
            if (LauncherCore.IsRunning(game)) throw new IOException("Close this MTOU game before updating.");
            if (!String.Equals(Hash(archive), manifest.ArchiveSha256, StringComparison.OrdinalIgnoreCase))
                throw new IOException("The mod package failed SHA-256 verification.");
            InstallerCore.NoLinks(game);
            var expected = manifest.Files.ToDictionary(f => f.Path.Replace(Path.DirectorySeparatorChar, '/'), StringComparer.OrdinalIgnoreCase);
            var payload = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            using (var memory = new MemoryStream(archive)) using (var zip = new ZipArchive(memory, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries)
                {
                    InstallerCore.SafePath(game, entry.FullName.TrimEnd('/'));
                    if (entry.FullName.EndsWith("/")) continue;
                    ReleaseFile pin;
                    if (!expected.TryGetValue(entry.FullName, out pin) || payload.ContainsKey(entry.FullName) || entry.Length != pin.Bytes)
                        throw new IOException("Unexpected or duplicate package entry: " + entry.FullName);
                    using (var data = new MemoryStream()) using (var input = entry.Open())
                    {
                        input.CopyTo(data); byte[] bytes = data.ToArray();
                        if (!String.Equals(Hash(bytes), pin.Sha256, StringComparison.OrdinalIgnoreCase))
                            throw new IOException("Package file verification failed: " + entry.FullName);
                        payload.Add(entry.FullName, bytes);
                    }
                }
            if (payload.Count != expected.Count) throw new IOException("The mod package is incomplete.");
            var before = new Dictionary<string, byte[]>();
            try
            {
                foreach (var pair in payload)
                {
                    string path = InstallerCore.SafePath(game, pair.Key); InstallerCore.NoLinks(path);
                    if (expected[pair.Key].PreserveExisting && File.Exists(path)) continue;
                    before.Add(path, File.Exists(path) ? File.ReadAllBytes(path) : null);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    string temp = path + ".mtou-new-" + Guid.NewGuid().ToString("N");
                    try
                    {
                        File.WriteAllBytes(temp, pair.Value);
                        if (File.Exists(path)) File.Replace(temp, path, null);
                        else File.Move(temp, path);
                    }
                    finally { if (File.Exists(temp)) File.Delete(temp); }
                }
            }
            catch
            {
                foreach (var pair in before.Reverse())
                {
                    if (pair.Value == null) { if (File.Exists(pair.Key)) File.Delete(pair.Key); }
                    else if (!File.Exists(pair.Key) || !String.Equals(Hash(File.ReadAllBytes(pair.Key)), Hash(pair.Value), StringComparison.Ordinal))
                        File.WriteAllBytes(pair.Key, pair.Value);
                }
                throw;
            }
        }
    }
}

