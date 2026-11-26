using System;
using System.Diagnostics;
using System.IO;
using Marmo.Launcher;
public static class LauncherTests
{
    static int count;
    static void Check(bool value,string name){if(!value)throw new Exception(name);count++;Console.WriteLine("PASS: "+name);}
    static void Put(string path,string value){Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path,value);}
    public static int Main(string[] args)
    {
        try
        {
            string root=Path.Combine(Path.GetTempPath(),"MTOU-Launcher-"+Guid.NewGuid().ToString("N"));
            string game=Path.Combine(root,"game with spaces");
            Put(Path.Combine(game,"GameAssembly.dll"),"fixture");
            foreach(string file in new[]{"winhttp.dll","BepInEx/plugins/MiraAPI.dll","BepInEx/plugins/Reactor.dll","BepInEx/plugins/CorsacCosmetics.dll"})Put(Path.Combine(game,file),"fixture");
            File.Copy(args[0],Path.Combine(game,"BepInEx/plugins/TOU-Marmo.dll"));
            File.Copy(args[2],Path.Combine(game,"Among Us.exe"));
            Directory.CreateDirectory(Path.Combine(game,"Among Us_Data"));
            Check(LauncherCore.FolderFromSelection(Path.Combine(game,"Among Us.exe"))==Path.GetFullPath(game),"Executable selection resolves the game folder");
            Check(LauncherCore.FolderFromSelection(game)==Path.GetFullPath(game),"Folder selection resolves the game folder");
            Check(LauncherCore.FolderFromSelection(root)==null,"Reject Downloads or another folder without the game");
            Put(Path.Combine(game,"unrelated.exe"),"fixture");
            Check(LauncherCore.FolderFromSelection(Path.Combine(game,"unrelated.exe"))==null,"Reject an unrelated executable");
            string profile=Path.Combine(root,"local data");
            Check(LauncherCore.DefaultDestination(profile,false)!=LauncherCore.DefaultDestination(profile,true),"Steam and Epic use separate default copies");
            Check(LauncherCore.DefaultDestination(profile,true).StartsWith(profile),"Default copies stay inside user-writable local data");
            Check(LauncherCore.DestinationFromSelection(game,false)==game,"Selecting an existing MTOU folder uses that copy");
            string empty=Path.Combine(root,"empty parent");Directory.CreateDirectory(empty);
            Check(LauncherCore.DestinationFromSelection(empty,false)==Path.Combine(empty,"Marmo Town of Us"),"An existing parent gets a new child destination");
            Directory.CreateDirectory(Path.Combine(empty,"Marmo Town of Us"));
            Check(LauncherCore.DestinationFromSelection(empty,false)==Path.Combine(empty,"Marmo Town of Us (2)"),"Destination selection avoids an occupied folder");
            string vanilla=Path.Combine(root,"original");Directory.CreateDirectory(Path.Combine(vanilla,"Among Us_Data"));
            Put(Path.Combine(vanilla,"Among Us.exe"),"fixture");Put(Path.Combine(vanilla,"GameAssembly.dll"),"fixture");
            bool protectedOriginal=false;try{LauncherCore.DestinationFromSelection(vanilla,false);}catch(IOException){protectedOriginal=true;}
            Check(protectedOriginal,"An original installation cannot become the mod-copy destination");
            Put(Path.Combine(game,"EOSSDK-Win64-Shipping.dll"),"fixture");
            Check(LauncherCore.MatchesStore(game,true)&&!LauncherCore.MatchesStore(game,false),"An Epic copy is not reused as the Steam destination");
            Put(Path.Combine(game,"steam_appid.txt"),"945360");
            Check(LauncherCore.MatchesStore(game,false)&&!LauncherCore.MatchesStore(game,true),"A Steam copy is not reused as the Epic destination");
            Check(LauncherCore.IsModdedGame(game),"Recognize an installed MTOU folder");
            Check(!LauncherCore.IsModdedGame(root),"Do not launch a vanilla or incomplete folder");
            Check(!LauncherCore.IsModdedGame(null),"Handle a missing saved path");
            string saved=Path.Combine(root,"preferences","game-path.txt");
            string desktop=Path.Combine(root,"desktop");
            Check(LauncherCore.FindGame(root,desktop,saved)==null,"No random fallback to another game");
            LauncherCore.Remember(game,saved);
            Check(File.ReadAllText(saved)==Path.GetFullPath(game),"Remember an explicitly selected installation");
            Check(LauncherCore.FindGame(root,desktop,saved)==Path.GetFullPath(game),"Find the saved installation");
            Put(saved,"missing folder");
            Check(LauncherCore.FindGame(game,desktop,saved)==Path.GetFullPath(game),"Adjacent installation has priority");
            var start=LauncherCore.StartInfo(game);
            Check(start.FileName==Path.Combine(game,"Among Us.exe"),"Launch the modded game executable");
            Check(start.WorkingDirectory==Path.GetFullPath(game)&&!start.UseShellExecute,"Use the game folder and inherit the test environment");
            Check(String.IsNullOrEmpty(start.Arguments),"Do not add DRM or game command-line overrides");
            string debugLog=Path.Combine(root,"log with spaces.txt");
            Check(LauncherCore.StartInfo(game,debugLog).Arguments=="-logFile \""+debugLog+"\"","Optional diagnostic log path is quoted");
            File.Copy(args[1],Path.Combine(game,"Marmo Town of Us.exe"));
            using(var process=Process.Start(new ProcessStartInfo(Path.Combine(game,"Marmo Town of Us.exe"),"--wait --no-remember"){UseShellExecute=false,WorkingDirectory=root}))
            {
                Check(process.WaitForExit(10000)&&process.ExitCode==0,"The actual launcher starts its child and exits successfully");
            }
            Check(File.ReadAllText(Path.Combine(game,"launcher-child.txt"))==Path.GetFullPath(game),"Child receives the correct working directory");
            if(args.Length>3 && !String.IsNullOrEmpty(args[3]))
            {
                File.Delete(Path.Combine(game,"steam_appid.txt"));
                bool missing=false;try{LauncherCore.StartInfo(game);}catch(IOException){missing=true;}
                Check(missing,"Epic never silently falls back to a direct unauthenticated game launch");
                byte[] archive=File.ReadAllBytes(args[3]);
                LauncherCore.EnsureEpicStarter(game,archive);
                var epicStart=LauncherCore.StartInfo(game);
                Check(epicStart.FileName==Path.Combine(game,"EpicGamesStarter.exe")&&epicStart.WorkingDirectory==game,"Epic uses the verified sign-in starter in the modded game folder");
                Check(epicStart.UseShellExecute&&String.IsNullOrEmpty(epicStart.Arguments),"Epic sign-in stays interactive and no authentication tokens are fabricated");
                Check(File.ReadAllText(Path.Combine(game,"launcher-child.txt"))==Path.GetFullPath(game),"Preparing the Epic starter does not execute the game or alter game data");
                File.Delete(Path.Combine(game,"EpicGamesStarter.exe"));
                byte[] corrupt=(byte[])archive.Clone();corrupt[100]^=1;
                bool corruptBlocked=false;try{LauncherCore.EnsureEpicStarter(game,corrupt);}catch(IOException){corruptBlocked=true;}
                Check(corruptBlocked&&!File.Exists(Path.Combine(game,"EpicGamesStarter.exe")),"Corrupt starter download never publishes an executable");
                Put(Path.Combine(game,"EpicGamesStarter.exe"),"modified helper");
                bool modifiedBlocked=false;try{LauncherCore.StartInfo(game);}catch(IOException){modifiedBlocked=true;}
                Check(modifiedBlocked,"A modified Epic helper is never launched");
                bool preserved=false;try{LauncherCore.EnsureEpicStarter(game,archive);}catch(IOException){preserved=File.ReadAllText(Path.Combine(game,"EpicGamesStarter.exe"))=="modified helper";}
                Check(preserved,"An unknown existing helper is preserved and reported instead of overwritten");
                Put(Path.Combine(game,"steam_appid.txt"),"945360");
                Check(LauncherCore.StartInfo(game).FileName==Path.Combine(game,"Among Us.exe"),"Steam keeps its original direct launch path");
            }
            Put(Path.Combine(game,"BepInEx/plugins/TownOfUsMira.dll"),"duplicate");
            Check(!LauncherCore.IsModdedGame(game),"Reject a conflicting standalone ToU plugin");
            Console.WriteLine("PASS: "+count+" launcher checks. Fixture child only; no game or multiplayer test.");return 0;
        }
        catch(Exception error){Console.Error.WriteLine(error);return 1;}
    }
}
