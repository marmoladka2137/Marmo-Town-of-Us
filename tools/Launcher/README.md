# Marmo Town of Us launcher

Windows x64 launcher, built with .NET Framework 4.8. Extract the complete launcher ZIP and run **Marmo Town of Us.exe**. Keep the Steam or Epic Games client running.

Choose an existing MTOU folder, or use **Install a new copy** and select Steam/Epic Games. Installation reads the storefront manifest and copies your locally installed Among Us into a separate folder. No game executable is distributed. Epic detection is implemented, but an actual Epic Among Us installation was unavailable for gameplay testing.

The launcher checks public releases of `marmoladka2137/Marmo-Town-of-Us`, including published beta releases. Drafts are ignored. If no compatible Release exists, or its API is unavailable, the public `downloads` branch supplies the manifest and ZIP instead. Published Releases take priority. A release must attach `mtou-release.json` and `MTOU-Mods-Windows.zip`. A higher numeric assembly revision triggers an update. The manifest pins SHA-256 and length for every file and the complete ZIP; validation finishes before installed files are replaced. Locked files cause rollback. Close the game before updating. Existing configuration files marked PreserveExisting remain unchanged. Offline checks display an error and still allow launching an existing installation. Updates cover mod files; the launcher executable itself is replaced by downloading a new launcher package.

The bundled plugin additions reproduce the TEST MODY set: MTOU, AleLuduMod, AUnlocker, Mini.RegionInstall, CorsacCosmetics, MiraAPI and Reactor. The initial loader and three required plugins are obtained from the pinned official upstream distribution. MiraAPI uses the public 0.6.0 baseline instead of the historical local 0.5.0-dev binary. AUnlocker's guest/minor overrides remain disabled. No accounts, private profiles, cache or local game files are packaged.

The Discord link opens `https://dc.gg/mtou`. Artwork: Marmo; see ASSET_CREDITS.md. Launcher code: GPLv3.

Build: `powershell -File tools/Launcher/Build.ps1 -OutputDirectory artifacts/launcher`.
Tests: `Test.ps1` (discovery/process launch), `Test-Updates.ps1` (manifest, verification, rollback and Epic discovery); `tools/SteamInstaller/Test.ps1` (isolated installer fixtures). These do not certify multiplayer or Epic gameplay.

The public bootstrap ZIP omits the duplicate plugin/archive payload. On first install, the launcher downloads pinned archive parts from the public channel, verifies each part and the assembled ZIP, and extracts the pinned DLL into its cache. It never asks users to combine parts manually. The locally assembled complete launcher ZIP can install using its bundled supplement.

## Launcher 2.1 — quick setup

Select Steam or Epic Games. The original game is detected from Steam libraries or Epic manifests and selected automatically. Auto-detect rescans; Choose .exe opens an Explorer file dialog filtered to Among Us.exe. Multiple discovered installations appear in a drop-down.

A separate modded copy defaults to %LOCALAPPDATA%/MTOU/Games/Steam or %LOCALAPPDATA%/MTOU/Games/Epic. Use default restores that location. Choose folder uses the modern Windows folder dialog, with an address bar, search and Quick Access. Selecting an existing MTOU copy enables Play; selecting a parent folder creates a new child destination. Store selections and destinations are remembered separately. Play remains disabled for incomplete installations.

The launcher runs as the current user and requests no administrator elevation. Build.ps1 accepts an optional SigningCertificateThumbprint for a valid existing code-signing certificate. Without one, the executable is unsigned: a clean antivirus scan does not establish SmartScreen reputation or remove its unknown-publisher warning. No self-signed trust workaround, antivirus exclusion or protection change is made.

For launcher-only releases, Refresh-Bootstrap.py updates the app and its pin in a verified existing bootstrap while retaining the original mod manifest and dependency pins. Test-Setup.ps1 checks setup states using isolated profiles and synthetic store fixtures.
