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
