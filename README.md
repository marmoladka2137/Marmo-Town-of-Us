# Marmo Town of Us (MTOU)

![Marmo Town of Us](Resources/Branding/MarmoLogo.png)

**1.1.0-beta · 1.1.0 Beta · community modification**

Marmo Town of Us is an independent community modification based on Town of Us: Mira. It combines the Mira role framework with additional roles, modifiers, configurable abilities and reworked features in one `TOU-Marmo.dll`.

## Project Overview

Every participant, including the host, needs the same compatible MTOU installation. This is an **all-client mod**, not host-only. Do not install a separate `TownOfUsMira.dll` alongside MTOU. No official partnership with Mira or Innersloth is implied.

## Features

- Integrated Town of Us: Mira roles, meetings, ability buttons and game modes.
- Twelve MTOU roles, additional modifiers and configurable mechanics.
- Role/modifier search and All/ToU/MToU filters.
- Reworked protection, transformation and meeting interactions.
- Original runtime animation frames, synthesized cues, Marmo branding and Discord Rich Presence.

These describe implemented code. Full gameplay and host/client regression remain unverified; see [test results](RUNTIME_MOTION_TESTS.md) and [known issues](KNOWN_ISSUES.md).

## Custom Roles

| Category | MTOU roles |
|---|---|
| Crewmate Investigative | Exorcist |
| Crewmate Killing | Composter, Furas |
| Crewmate Power | Bandit, Necromancer |
| Crewmate Protective | Doctor |
| Crewmate Support | Wladca |
| Impostor Power | Hugger, Sweets |
| Neutral Benign | Dirtek |
| Neutral Evil | Gambler |
| Neutral Killing | Barszcz |

[Complete role catalog](ROLES.md) · [Concise catalog for SUS Modder](SUSMODDER_ROLES.md).

## Modifiers

MTOU modifiers include Balloon, Big Brain, Kemada, One More Chance and Simp. [MODIFIERS.md](MODIFIERS.md) separates assignable modifiers from temporary effects. Existing Double Shot/Triple Shot and Hugger's current grant remain in this beta; retirement remains unresolved as documented.

## Reworked Town of Us Mira Features

Source includes Executioner's first-round choices, Jester/Spectre result handling, Deputy presentation, Plumber vent nails, Time Lord rewind risk and protection changes. See [mechanics](docs/MECHANICS.md), [provenance](SOURCE_PROVENANCE.md) and [known issues](KNOWN_ISSUES.md) for implementation and limits.

## Screenshots

The project logo above is original MTOU artwork. It is not evidence that all gameplay interactions passed testing.

## Supported Among Us Versions

Development reference: **Among Us 2026.9.29, Unity 2022.3.44f1, Windows x64**. This is an observed development environment, not certification for every store or platform. Clean public-dependency installation, Steam/Epic parity and official-server gameplay remain unverified.

## Installation

**MTOU app:** [Download the app ZIP](https://github.com/marmoladka2137/Marmo-Town-of-Us/releases/download/v1.1.0-beta.13/MTOU-1.1.0-beta-Steam-App.zip), extract it and run **Marmo Town of Us.exe**. It uses the Discord widget icon and opens your installed MTOU game directly. On first use without an installation, it opens setup. [App instructions](tools/Launcher/README.md).

**Steam / Windows x64:** [Download the installer ZIP](https://github.com/marmoladka2137/Marmo-Town-of-Us/releases/download/v1.1.0-beta.13/MTOU-1.1.0-beta-Steam-Installer.zip). Extract it, run `MTOU Installer.exe` and choose a new destination. It copies your own supported Steam game locally, downloads pinned official dependencies and installs MTOU without changing your Steam installation. [Installer instructions](tools/SteamInstaller/README.md) · [Executed installer tests](STEAM_INSTALLER_TESTS.md).

Use the [beta release](https://github.com/marmoladka2137/Marmo-Town-of-Us/releases/tag/v1.1.0-beta) and [INSTALLATION.md](INSTALLATION.md). The thin archive places `TOU-Marmo.dll` in `BepInEx/plugins`. Install prerequisites separately and use the same version on every client. No game files, loader libraries or separate original TOU plugin are bundled.



**Manual installation, earlier revision 1.1.0.12:** [Steam mod files](https://github.com/marmoladka2137/Marmo-Town-of-Us/releases/download/v1.1.0-beta/MTOU-1.1.0-beta-Steam.zip) · [Steam instructions](INSTALLATION_STEAM.md). These earlier thin files require prerequisites installed separately. The installer above includes the newer text revision 1.1.0.13. Neither download contains the base game.

## Requirements

A legally owned compatible Among Us PC installation, BepInEx IL2CPP, Reactor, MiraAPI and CorsacCosmetics. Exact observed versions and external-source uncertainties: [DEPENDENCIES.md](DEPENDENCIES.md). AchievementsAPI is not required. Perfect Comms is an optional integration.

## Building from Source

[BUILDING.md](BUILDING.md) documents the pinned modern SDK, net6.0 runtime and portable `AMONG_US_ROOT`/`GameRoot` reference path. The release supplies matching full source, preferred resource graphs, PNG frames and generators. Building never deploys to the game automatically.

## Known Issues

This is a **beta**, not a stable or fully multiplayer-tested release. Local compilation, deterministic state tests and native Unity fixtures passed. Full role interactions, host/client gameplay, public-dependency clean installation and SUS Modder installation remain unverified. The original-resource pipeline changes the visual/audio style. [Full limitations](KNOWN_ISSUES.md) · [Executed tests](RUNTIME_MOTION_TESTS.md).

## Releases

[Downloads](https://github.com/marmoladka2137/Marmo-Town-of-Us/releases) · [1.1.0-beta](https://github.com/marmoladka2137/Marmo-Town-of-Us/releases/tag/v1.1.0-beta). Publish/use the matching source archive and SHA-256 list with the DLL. Release availability does not imply SUS Modder acceptance.

## Credits

Based on [Town of Us: Mira](https://github.com/AU-Avengers/TOU-Mira), its contributors and predecessors. **Original MTOU artwork — created by Marmo**. [CREDITS.md](CREDITS.md), [ASSET_CREDITS.md](ASSET_CREDITS.md) and [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md) retain the original authors and separate terms.

## License

Combined code: [GNU GPLv3](LICENSE). Original MTOU graphics: [CC BY 4.0](MTOU_ARTWORK_LICENSE.md), Marmo attribution. Font: OFL-1.1. Other credited materials and game IP retain their own terms. [License audit](LICENSE_AUDIT.md).

## Disclaimer

This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.

## Support and Bug Reports

Community: [dc.gg/mtou](https://dc.gg/mtou). [Report an issue](https://github.com/marmoladka2137/Marmo-Town-of-Us/issues). Include mod/game/store versions, role/settings, host/client perspective, reproduction steps and a redacted log. Never upload credentials, player profiles or the base game. Upstream maintainers are not responsible for MTOU-specific issues.
