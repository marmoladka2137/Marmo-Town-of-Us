using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;
using Marmo.SteamInstaller;
namespace Marmo.Launcher
{
    public sealed class VersionChoice
    {
        public string Tag, Name, ManifestUrl, ArchiveUrl, ChecksumUrl, SourceCommit;
        public bool Automatic;
        public override string ToString() { return Automatic ? "Latest version (automatic updates)" : Name; }
    }
    public static class VersionLibrary
    {
        public static List<VersionChoice> Parse(string json)
        {
            var result = new List<VersionChoice>();
            var releases = new JavaScriptSerializer().Deserialize<List<Dictionary<string,object>>>(json);
            foreach (var release in releases.OrderByDescending(r => Convert.ToString(r["published_at"]),StringComparer.Ordinal))
            {
                if (Convert.ToBoolean(release["draft"])) continue;
                var choice = new VersionChoice { Tag=Convert.ToString(release["tag_name"]), Name=Convert.ToString(release["tag_name"]).TrimStart('v'), SourceCommit=Convert.ToString(release["target_commitish"]) };
                foreach (Dictionary<string,object> asset in (System.Collections.ArrayList)release["assets"])
                {
                    string name=Convert.ToString(asset["name"]), url=Convert.ToString(asset["browser_download_url"]);
                    if (!ReleaseUpdate.Trusted(url)) continue;
                    if (name=="mtou-release.json") choice.ManifestUrl=url;
                    if (name=="MTOU-Mods-Windows.zip") choice.ArchiveUrl=url;
                    if (name=="SHA256SUMS.txt") choice.ChecksumUrl=url;
                }
                if (choice.ManifestUrl==null)
                    foreach(Dictionary<string,object> asset in (System.Collections.ArrayList)release["assets"])
                    {
                        string name=Convert.ToString(asset["name"]),url=Convert.ToString(asset["browser_download_url"]);
                        if(name.StartsWith("MTOU-Steam-",StringComparison.Ordinal)&&name.EndsWith(".zip",StringComparison.OrdinalIgnoreCase)&&ReleaseUpdate.Trusted(url)) choice.ArchiveUrl=url;
                    }
                if(choice.ArchiveUrl!=null && (choice.ManifestUrl!=null || choice.ChecksumUrl!=null)) result.Add(choice);
            }
            return result;
        }
        public static string Destination(string localData,bool epic,string tag)
        {
            if(String.IsNullOrEmpty(tag)||tag.Length>80||tag.Any(c=>!Char.IsLetterOrDigit(c)&&c!='.'&&c!='-'&&c!='_')) throw new IOException("Invalid version identifier.");
            return Path.Combine(localData,"MTOU","Versions",tag,epic?"Epic":"Steam");
        }
        public static void PrepareInstaller(InstallerManifest installer,ReleaseManifest manifest)
        {
            ReleaseUpdate.Validate(manifest);
            var pin=manifest.Files.Single(f=>f.Path=="BepInEx/plugins/TOU-Marmo.dll");
            installer.Version=manifest.Version;installer.AssemblyRevision=manifest.Revision;installer.SourceCommit=manifest.SourceCommit;
            installer.Payload=new FilePin {Path="Payload/TOU-Marmo.dll",Bytes=pin.Bytes,Sha256=pin.Sha256};
        }
        public static ReleaseManifest Load(VersionChoice choice,string cache)
        {
            Destination(cache,false,choice.Tag); // Validate the untrusted tag before using it as a filename.
            Directory.CreateDirectory(cache);InstallerCore.NoLinks(cache);
            string manifestCache=Path.Combine(cache,"version-"+choice.Tag+".json");
            if(File.Exists(manifestCache))
            {
                try{var saved=new JavaScriptSerializer().Deserialize<ReleaseManifest>(File.ReadAllText(manifestCache));ReleaseUpdate.Validate(saved);return saved;}
                catch(IOException){}catch(ArgumentException){}catch(InvalidOperationException){}
            }
            if(choice.ManifestUrl!=null)
            {
                var manifest=new JavaScriptSerializer().Deserialize<ReleaseManifest>(Encoding.UTF8.GetString(ReleaseUpdate.Download(choice.ManifestUrl)));
                ReleaseUpdate.Validate(manifest);File.WriteAllText(manifestCache,new JavaScriptSerializer().Serialize(manifest));return manifest;
            }
            // Older public releases have a Steam ZIP and checksum list, but no
            // updater manifest. Verify that ZIP before constructing a mod-only payload.
            string sums=Encoding.UTF8.GetString(ReleaseUpdate.Download(choice.ChecksumUrl));
            string filename=Uri.UnescapeDataString(new Uri(choice.ArchiveUrl).Segments.Last());
            string expected=null;
            foreach(string line in sums.Split('\n'))
            {
                string value=line.Trim();
                if(value.Length>66 && value.Substring(64).Trim().TrimStart('*')==filename)expected=value.Substring(0,64);
            }
            if(expected==null || !System.Text.RegularExpressions.Regex.IsMatch(expected,"^[a-fA-F0-9]{64}$")) throw new IOException("The old release has no matching package checksum.");
            Directory.CreateDirectory(cache);string original=Path.Combine(cache,expected+"-original.zip");
            InstallerCore.NoLinks(original);
            byte[] archive=File.Exists(original)?File.ReadAllBytes(original):ReleaseUpdate.Download(choice.ArchiveUrl);
            if(!String.Equals(ReleaseUpdate.Hash(archive),expected,StringComparison.OrdinalIgnoreCase)) throw new IOException("Old release checksum mismatch.");
            if(!File.Exists(original)) File.WriteAllBytes(original,archive);
            var files=new List<ReleaseFile>();byte[] plugin=null;byte[] payload;
            using(var output=new MemoryStream())
            {
                using(var dest=new ZipArchive(output,ZipArchiveMode.Create,true))
                using(var input=new ZipArchive(new MemoryStream(archive),ZipArchiveMode.Read))
                foreach(var entry in input.Entries)
                {
                    InstallerCore.SafePath(cache,entry.FullName.TrimEnd('/'));
                    if(entry.FullName.EndsWith("/"))continue;
                    if(!entry.FullName.StartsWith("BepInEx/plugins/",StringComparison.Ordinal)&&!entry.FullName.StartsWith("MTOU Notices/",StringComparison.Ordinal)&&entry.FullName!="BepInEx/config/AUnlocker.cfg")continue;
                    byte[] bytes;using(var m=new MemoryStream()){using(var r=entry.Open())r.CopyTo(m);bytes=m.ToArray();}
                    files.Add(new ReleaseFile {Path=entry.FullName,Bytes=bytes.Length,Sha256=ReleaseUpdate.Hash(bytes),PreserveExisting=entry.FullName.EndsWith(".cfg")});
                    using(var w=dest.CreateEntry(entry.FullName,CompressionLevel.Optimal).Open())w.Write(bytes,0,bytes.Length);
                    if(entry.FullName=="BepInEx/plugins/TOU-Marmo.dll")plugin=bytes;
                }
                payload=output.ToArray();
            }
            if(plugin==null)throw new IOException("Old release does not contain TOU-Marmo.dll.");
            string probe=Path.Combine(cache,Guid.NewGuid()+".dll");string revision;
            try{File.WriteAllBytes(probe,plugin);revision=AssemblyName.GetAssemblyName(probe).Version.ToString();}finally{if(File.Exists(probe))File.Delete(probe);}
            string sha=ReleaseUpdate.Hash(payload);File.WriteAllBytes(Path.Combine(cache,sha+"-mods.zip"),payload);
            var result=new ReleaseManifest {Schema=1,Version=choice.Name,Revision=revision,SourceCommit=choice.SourceCommit,ArchiveSha256=sha,Files=files.ToArray()};
            ReleaseUpdate.Validate(result);File.WriteAllText(manifestCache,new JavaScriptSerializer().Serialize(result));return result;
        }
    }
}
