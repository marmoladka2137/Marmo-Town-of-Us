# Marmo Town of Us — Steam installer

## Instalacja po polsku

1. Kup i zainstaluj Among Us na Steamie. Rozpakuj caly ZIP instalatora.
2. Uruchom `Marmo Town of Us.exe`. Jesli mod jest juz zainstalowany, aplikacja od razu otworzy gre. W przeciwnym razie otworzy instalator, ktory powinien znalezc Steam automatycznie.
3. Wybierz nowy folder na kopie z modem i kliknij **Install MTOU**.
4. Poczekaj na pobranie bibliotek i skopiowanie swojej gry. Steamowa instalacja pozostaje bez zmian.
5. Zostaw Steam uruchomiony. Uzywaj skrotu Marmo Town of Us lub `Play MTOU.cmd` w nowym folderze.

Pierwsze uruchomienie moze trwac kilka minut: loader tworzy biblioteki IL2CPP i moze pobierac potrzebne pliki Unity. Potrzebne jest polaczenie z internetem i okolo 2 GB wolnego miejsca. Instalator nie wymaga uprawnien administratora przy instalacji do wlasnego folderu.

Obslugiwany punkt odniesienia: Among Us 2026.9.29, Windows x64. Instalator sprawdza dokladny build GameAssembly.dll. Przy innej wersji zatrzyma instalacje zamiast tworzyc niezgodna kopie. Nie pobiera starych plikow gry, nie zmienia Steama ani zabezpieczen gry.

**Nie udostepniaj gotowego folderu po instalacji** — zawiera juz lokalnie skopiowana gre. Udostepniaj ZIP instalatora.

## English

Extract the entire app archive and run **Marmo Town of Us.exe**. It launches an installed MTOU copy directly, or opens the included installer on first use. The installer detects Steam, checks the game build, creates a separate modded copy, downloads pinned official dependencies and installs MTOU. The desktop shortcut uses the app and its Discord artwork icon. Keep Steam running when playing. Every participant needs the same compatible setup.

The installer never overwrites an existing destination, Steam game, player profile or cosmetics settings. It copies only game files from your own Steam installation. It does not copy unrelated plugins, cosmetics caches, logs, user configuration or pre-generated game wrappers. A failed copy stays in a clearly marked `.installing-*` folder instead of being presented as ready.

The archive contains MTOU 1.1.0 Beta (assembly 1.1.0.13), installer source and notices. It contains no Among Us executable or game data, and no dependency implementation binaries. The unmodified game executable is copied locally during installation. No separate TownOfUsMira plugin is installed.

## Downloads and source

- Required loader/runtime and MiraAPI 0.6.0, Reactor 2.5.1, CorsacCosmetics 4.0.0: selected files from the official TOU Mira 1.7.9-beta1 Windows x64 release, downloaded at installation time. The source archive and all selected files are pinned by SHA-256 in installer-manifest.json; unselected plugins are not extracted.
- Official dependency archive: https://github.com/AU-Avengers/TOU-Mira/releases/tag/1.7.9-beta1
- Plugin source matching the included DLL: https://github.com/marmoladka2137/Marmo-Town-of-Us/tree/53221dd705fe5454c3de7a9423390613f0ab3912
- Installer and launcher source: Source/SteamInstaller and Source/Launcher in the app ZIP; tools/SteamInstaller and tools/Launcher in the repository.
- Community: https://dc.gg/mtou

Internet access is needed for initial dependencies and first-launch generation. The verified dependency ZIP is cached in LocalAppData/MTOU/InstallerCache; logs are stored in LocalAppData/MTOU/InstallerLogs. No account credentials or telemetry are collected.

Full gameplay and host/client multiplayer are not certified by installer checks. See STEAM_INSTALLER_TESTS.md in the repository for the exact executed test scope.

## Building and testing the installer

On Windows with .NET Framework 4.8, run Build.ps1 with PowerShell to compile the C# sources into a Windows x64 GUI executable. Test.ps1 compiles and runs filesystem/manifest/archive tests without launching the game. Package.ps1 accepts PluginDll and OutputDirectory and verifies the plugin against the checked-in manifest before packaging. Build artifacts are placed outside tracked source.

Code is GPL-3.0. Original MTOU artwork is CC BY 4.0 with Marmo attribution. Upstream dependencies retain their own licenses and sources; see Notices. The base game is not licensed or distributed by this installer.

This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.
