# Steam version — Windows mod files

Marmo Town of Us (MTOU) 1.1.0-beta, assembly revision 1.1.0.12.

This package is for installing MTOU into a legally owned Steam copy of Among Us. It contains mod files and instructions, not the Among Us base game or a complete dependency/loader pack.

## Install

1. In Steam, open Among Us → Properties → Installed Files → Browse. Close the game. Back up the installation, or create a separate copy for MTOU.
2. Set up the compatible BepInEx IL2CPP loader and required Reactor, MiraAPI and CorsacCosmetics plugins separately, following DEPENDENCIES.md. The development baseline used a MiraAPI 0.5.0-dev build; a clean installation using public dependency builds is not certified.
3. Extract this ZIP into that modded game folder. The plugin must be at BepInEx/plugins/TOU-Marmo.dll. No separate textures folder is required.
4. Do not load TownOfUsMira.dll or another duplicate MTOU DLL alongside this integrated plugin. Back up any plugin you replace.
5. Keep Steam running and use the prepared modded copy. Every participant needs the same MTOU build and compatible dependencies.

Development reference: Among Us 2026.9.29, Windows x64. The inspected local installation contains Steam AppID 945360 and steam_api64.dll. This is installation-target evidence, not a completed clean Steam install or multiplayer test. A different current game version may require a different compatible build.

## Contents and verification

Only BepInEx/plugins/TOU-Marmo.dll and relevant documentation/licenses are included. No game executable, game data, Unity/game DLLs, external plugin implementations, cosmetics cache, logs, player settings, credentials, QA probes, bin/obj directories or unrelated mods are packaged.

DLL SHA-256: 6220471EF06BB2C755B9BF2F6236FD2E0B966722E3A4ADA7493AC5740ED55A30
Matching complete source: https://github.com/marmoladka2137/Marmo-Town-of-Us/releases/download/v1.1.0-beta/MTOU-1.1.0-beta-source.zip
Repository: https://github.com/marmoladka2137/Marmo-Town-of-Us
Community: https://dc.gg/mtou

This remains a beta. Full gameplay, host/client multiplayer, clean public-dependency installation, Steam/Epic parity and SUS Modder installation/acceptance are unverified. See RUNTIME_MOTION_TESTS.md.

Code GPLv3; original MTOU graphics CC BY 4.0 with Marmo attribution; retained font OFL and third-party notices apply. Keep matching source and notices when redistributing the plugin.

Innersloth's mod policy does not permit distributing a PC mod with the base game: https://www.innersloth.com/among-us-mod-policy/

This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.

