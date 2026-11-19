// Marmo Town of Us launcher. GPL-3.0; see LICENSE.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Marmo.Launcher
{
    public static class LauncherCore
    {
        public static string DisplayVersion(string folder)
        {
            try
            {
                string path=Path.Combine(folder,"BepInEx","plugins","TOU-Marmo.dll");
                string product=FileVersionInfo.GetVersionInfo(path).ProductVersion;
                return String.IsNullOrWhiteSpace(product)?AssemblyName.GetAssemblyName(path).Version.ToString():product;
            }
            catch(IOException){return "";}catch(ArgumentException){return "";}
            catch(UnauthorizedAccessException){return "";}catch(BadImageFormatException){return "";}
        }
        public static bool IsModdedGame(string folder)
        {
            if (String.IsNullOrWhiteSpace(folder)) return false;
            try
            {
                foreach (string file in new[] { "Among Us.exe", "GameAssembly.dll", "winhttp.dll",
                    "BepInEx/plugins/TOU-Marmo.dll", "BepInEx/plugins/MiraAPI.dll",
                    "BepInEx/plugins/Reactor.dll", "BepInEx/plugins/CorsacCosmetics.dll" })
                    if (!File.Exists(Path.Combine(folder, file))) return false;
                return AssemblyName.GetAssemblyName(Path.Combine(folder, "BepInEx/plugins/TOU-Marmo.dll")).Name == "TOU-Marmo"
                    && !File.Exists(Path.Combine(folder, "BepInEx/plugins/TownOfUsMira.dll"));
            }
            catch (IOException) { return false; }
            catch (UnauthorizedAccessException) { return false; }
            catch (BadImageFormatException) { return false; }
            catch (ArgumentException) { return false; }
        }
        public static bool IsOriginalGame(string folder)
        {
            if (String.IsNullOrWhiteSpace(folder)) return false;
            try { return File.Exists(Path.Combine(folder,"Among Us.exe")) && File.Exists(Path.Combine(folder,"GameAssembly.dll"))
                && Directory.Exists(Path.Combine(folder,"Among Us_Data")); }
            catch (ArgumentException) { return false; } catch (IOException) { return false; }
            catch (UnauthorizedAccessException) { return false; }
        }
        public static string FolderFromSelection(string selected)
        {
            if (String.IsNullOrWhiteSpace(selected)) return null;
            try
            {
                string path = Path.GetFullPath(selected.Trim().Trim('"'));
                if (File.Exists(path))
                {
                    if (!String.Equals(Path.GetFileName(path),"Among Us.exe",StringComparison.OrdinalIgnoreCase)) return null;
                    path = Path.GetDirectoryName(path);
                }
                return IsOriginalGame(path) ? path : null;
            }
            catch (ArgumentException) { return null; } catch (IOException) { return null; }
            catch (UnauthorizedAccessException) { return null; } catch (NotSupportedException) { return null; }
        }
        public static string DefaultDestination(string localData, bool epic)
        {
            return Path.Combine(localData,"MTOU","Games",epic ? "Epic" : "Steam");
        }
        public static bool MatchesStore(string folder, bool epic)
        {
            if(!IsModdedGame(folder))return false;
            if(File.Exists(Path.Combine(folder,"steam_appid.txt")))return !epic;
            try
            {
                string note=Path.Combine(folder,"MTOU-INSTALLATION.txt");
                if(File.Exists(note))
                {
                    string text=File.ReadAllText(note);
                    if(text.Contains("installed Epic Games game"))return epic;
                    if(text.Contains("installed Steam game"))return !epic;
                }
            }
            catch(IOException){}catch(UnauthorizedAccessException){}
            return epic==File.Exists(Path.Combine(folder,"EOSSDK-Win64-Shipping.dll"));
        }
        public static string DestinationFromSelection(string selected, bool epic)
        {
            string path = Path.GetFullPath(selected);
            if (IsModdedGame(path)) return path;
            if (IsOriginalGame(path)) throw new IOException("This is the original game. Select it under ORIGINAL AMONG US; choose a separate MTOU folder below.");
            if (Directory.Exists(path))
            {
                string name=epic ? "Marmo Town of Us Epic" : "Marmo Town of Us";
                for(int index=1;index<=100;index++)
                {
                    string candidate=Path.Combine(path,name+(index==1?"":" ("+index+")"));
                    if(IsModdedGame(candidate)||!Directory.Exists(candidate)&&!File.Exists(candidate))return candidate;
                }
                throw new IOException("Choose another parent folder for your MTOU copy.");
            }
            return path;
        }
        public static string FindGame(string appFolder, string desktop, string rememberedFile)
        {
            var candidates = new List<string> { appFolder };
            try
            {
                if (File.Exists(rememberedFile)) candidates.Add(File.ReadAllText(rememberedFile).Trim());
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            candidates.Add(Path.Combine(desktop, "Marmo Town of Us"));
            candidates.Add(Path.Combine(desktop, "Marmo Town of Us Epic"));
            candidates.Add(Path.Combine(desktop, "TEST MODY"));
            candidates.Add(Path.Combine(appFolder, "Marmo Town of Us"));
            foreach (string candidate in candidates)
                if (IsModdedGame(candidate)) return Path.GetFullPath(candidate);
            return null;
        }
        public static ProcessStartInfo StartInfo(string folder, string logFile = null)
        {
            if (!IsModdedGame(folder)) throw new IOException("Select the installed MTOU game folder, containing Among Us.exe and the MTOU plugin.");
            folder = Path.GetFullPath(folder);
            var start = new ProcessStartInfo(Path.Combine(folder, "Among Us.exe"))
            {
                WorkingDirectory = folder,
                UseShellExecute = false
            };
            if (!String.IsNullOrWhiteSpace(logFile)) start.Arguments = "-logFile \"" + Path.GetFullPath(logFile) + "\"";
            return start;
        }
        public static bool IsRunning(string folder)
        {
            string exe = Path.GetFullPath(Path.Combine(folder, "Among Us.exe"));
            foreach (var process in Process.GetProcessesByName("Among Us"))
                using (process)
                    try
                    {
                        if (String.Equals(process.MainModule.FileName, exe, StringComparison.OrdinalIgnoreCase)) return true;
                    }
                    catch (System.ComponentModel.Win32Exception) { }
                    catch (InvalidOperationException) { }
            return false;
        }
        public static void Remember(string folder, string filename)
        {
            if (!IsModdedGame(folder)) throw new IOException("The selected folder is not an MTOU installation.");
            Directory.CreateDirectory(Path.GetDirectoryName(filename));
            File.WriteAllText(filename, Path.GetFullPath(folder));
        }
    }
}
