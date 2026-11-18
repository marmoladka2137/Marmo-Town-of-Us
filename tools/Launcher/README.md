# Marmo Town of Us app

Run **Marmo Town of Us.exe**. Its icon is the same `mtou_logo` artwork used by the MTOU Discord widget.

If MTOU is already installed, the app opens it directly. It first checks the folder beside the launcher, then a previously selected installation and the default Desktop/Marmo Town of Us folder. If the app package includes the installer and no game is installed yet, it opens setup. The installed desktop shortcut points to this launcher and uses its icon.

The standalone launcher can also be placed inside your existing modded game folder. If it cannot find an installation, select the MTOU folder once. It remembers that path in LocalAppData/MTOU/Launcher/game-path.txt. It never selects an unmodified Steam game as a substitute and does not patch the game's executable, profiles, skins or plugin DLL.

Keep Steam running. Download/install the app package only; the resulting local game folder contains your copy of Among Us and must not be redistributed.

Source is GPLv3. The Discord artwork was selected by Marmo for this app; original MTOU artwork terms and underlying Among Us IP notices remain in the project credits. The PNG is converted into a multi-resolution Windows ICO without changing the artwork. Source asset: https://cdn.discordapp.com/app-assets/1571528434805510255/1571542621472423986.png

Build on Windows with .NET Framework 4.8 using Build.ps1. Build-Icon.ps1 regenerates Logo.ico from DiscordLogo.png. Test.ps1 runs launcher discovery and child-process tests. `--wait --no-remember` is provided for isolated launch tests; `--game-log PATH` redirects the game's diagnostic log. Normal use needs no arguments.
