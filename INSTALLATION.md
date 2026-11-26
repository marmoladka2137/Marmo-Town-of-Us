# Installation / Instalacja

## [Download MTOU for Windows / Pobierz MTOU](https://raw.githubusercontent.com/marmoladka2137/Marmo-Town-of-Us/downloads/MTOU-1.1.0-beta-Launcher-Windows.zip)

1. Zainstaluj kupione Among Us na Steamie lub Epic Games. / Install your purchased Among Us through Steam or Epic Games.
2. Rozpakuj cały ZIP i uruchom **Marmo Town of Us.exe**. / Extract the entire ZIP and run **Marmo Town of Us.exe**.
3. Wybierz sklep. Launcher wykryje grę; w razie potrzeby użyj **Choose .exe** i wskaż **Among Us.exe**. / Choose your store; use **Choose .exe** only if automatic detection fails.
4. Zostaw osobny domyślny folder MTOU i kliknij **INSTALL MTOU**, potem **PLAY MTOU**. / Keep the separate default MTOU folder, choose **INSTALL MTOU**, then **PLAY MTOU**.

Keep the storefront client running. All participants need matching MTOU and dependency versions. First startup can take several minutes while BepInEx prepares IL2CPP files. A compatible purchased game, Internet access and space for a separate copy are required. The game executable/data are copied locally and are not included in the download.

Current package: MTOU 1.1.0 Beta, assembly 1.1.0.19; launcher 1.1.0 Beta. The launcher recreates the supported seven-plugin set without a separate TownOfUsMira.dll. It verifies public update files and preserves marked existing configuration. [Launcher details](tools/Launcher/README.md) · [Dependencies](DEPENDENCIES.md).

Steam download/install/startup was checked. Actual Epic gameplay, full multiplayer and all role interactions remain unverified. See [test results](LAUNCHER_RELEASE_TESTS.md) and [known issues](KNOWN_ISSUES.md). Windows SmartScreen may warn because the launcher is unsigned; no antivirus exclusion is required or recommended.

For development-only manual setup, obtain prerequisites from their official sources and build using [BUILDING.md](BUILDING.md). Do not load duplicate MTOU/TOU Mira DLLs or distribute the locally copied base game. Existing license and author notices remain applicable.
