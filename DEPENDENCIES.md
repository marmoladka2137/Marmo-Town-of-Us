# Dependencies and development baseline

The current launcher supplies a verified mod supplement and obtains a pinned loader/dependency baseline. It does not distribute game files or a separate TownOfUsMira plugin. Dependency versions and independent license/source obligations are retained below.

## Installation baseline

The installer uses the pinned MTOU 1.1.0 Beta DLL and downloads selected files directly from the official [TOU Mira 1.7.9-beta1 Windows x64 archive](https://github.com/AU-Avengers/TOU-Mira/releases/tag/1.7.9-beta1). It selects BepInEx be.753, its core/runtime, Reactor 2.5.1, CorsacCosmetics 4.0.0 and **MiraAPI 0.6.0**. It does not install the separate TownOfUsMira plugin, AchievementsAPI or other optional mods. The dependency archive is not included in the installer ZIP.

The MiraAPI DLL from that TOU release has SHA-256 `337CB6D8C2B75105578A05EC48A2364BF006E9EEE9BBF0D753E59B2105F92E9D`; it differs from the earlier local 0.5.0-dev build and from the separately published MiraAPI 0.6.0 asset. The complete archive and all 233 selected files are pinned in [installer-manifest.json](tools/SteamInstaller/installer-manifest.json). Never substitute an arbitrary latest library.

A real Steam copy was installed from these public files, generated fresh wrappers and reached the native main menu with MTOU and the public MiraAPI 0.6.0 loaded. Current MTOU source also compiles against this API. This resolves the earlier lack of a publicly obtainable startup baseline for this installer; it does not establish full role, match or host/client compatibility. See [installer tests](STEAM_INSTALLER_TESTS.md). The table below remains the historical development baseline.

## Observed startup baseline

| Component | Exact observed version / source | Packaging and reproducibility |
|---|---|---|
| Among Us | 2026.9.29, Windows PE x64; Unity 2022.3.44f1 | Legally owned local installation; not distributed. Only startup certified, not a supported-version range. |
| .NET SDK | 10.0.403 | Build tool, pinned by global.json. Runtime target remains net6.0. |
| BepInEx | 6.0.0-be.753+0d275a4df3b6bbcb990a9715d0f037789624c377 | External IL2CPP loader. [Source](https://github.com/BepInEx/BepInEx/tree/0d275a4df3b6bbcb990a9715d0f037789624c377); loader core/runtime subdependencies are not all source-matched. |
| Reactor | 2.5.1 | External required plugin. [Upstream](https://github.com/nuclearpowered/Reactor/releases/tag/2.5.1). |
| MiraAPI | 0.5.0-dev | External required development plugin. Local source pin 7a4e891 plus a credit patch is available as provenance, but it is not a certified match for this DLL. [Source](https://github.com/All-Of-Us-Mods/MiraAPI). This is a fresh-developer-build blocker until matched/rebuilt. |
| AchievementsAPI (historical) | Previously 2.0.0+e62c9c9032d293187d170f2e4ac37d05c1c11a63 | Removed from MTOU's compiled references and required plugins. Historical [binary-identified source](https://github.com/am-clonec/AchievementsAPI/tree/e62c9c9032d293187d170f2e4ac37d05c1c11a63) and notices are retained as provenance. |
| CorsacCosmetics | 4.0.0+9ddf4833e47104f58889fd6b275c32c838b1b7d5 | External direct reference. [Binary-identified source](https://github.com/XtraCube/CorsacCosmetics/tree/9ddf4833e47104f58889fd6b275c32c838b1b7d5). Downloaded cosmetics have separate rights. |
| HarmonyX | 2.10.2, assembly 0Harmony | External loader-provided dependency; not vanilla Harmony. |
| Il2CppInterop Runtime/Common | 1.5.3 with hash 5d129493f28ff78d6e3316149698597d76012587 in metadata | Version label known; the queried commit was unavailable upstream. Exact binary match not established. Generator/HarmonySupport are CI builds 1.5.0-ci.620+763f4279… rather than the same source label. |
| PerfectComms.Api | 4.1.7.1, NuGet source commit 59068ee4a9753731cb880cee3dad675e583f0b1f | Compile-time reference API restored from nuget.org, LGPL-2.1-only. ExcludeAssets=runtime; full voice-chat mod is optional and not installed/bundled. |

The four direct plugin hashes are in [dependency-hashes.csv](docs/audit/dependency-hashes.csv). The inspected core/interop assembly names, references, resource names, versions and hashes are in [assembly-inventory.json](docs/audit/assembly-inventory.json). References to generated game wrappers are context, not a distribution authorization.

## Build graph and embedding

TOU-Marmo.csproj compiles MTOU and the integrated TOU source into one assembly. Seven superseded integrated Composter files are excluded. `external` is not a compile dependency and is omitted from publication. Resource logical names are intentionally kept for existing asset loaders.

Original Costura.Fody 5.7.0 pulled Fody 6.5.5 and an obsolete transitive framework package graph. The original candidate had Costura metadata but no embedded implementation DLLs; the publication build removes this unnecessary toolchain. `packages.lock.json` records the remaining compile-only NuGet package. No MiraAPI or other external library is merged into the new DLL.

Core and interop wildcard references are obtained only from GameRoot and use Private=false. This makes path configuration portable but does not define a fully independent, publicly reproducible reference set. A trusted, exact dependency baseline still needs selection and licensing/source verification.

## Compatibility scope

MiraAPI and Reactor are declared as BepInDependency attributes; CorsacCosmetics is also a direct compiled reference. Build checks require these three plugins. AchievementsAPI was removed as a reference and prerequisite: MTOU achievement integration is inactive. Historical dependency inventories below are records of the earlier baseline, not the current dependency graph.

AleLuduMod, AUnlocker and Mini.RegionInstall were present in the owner's installation but were not loaded by the isolated startup test and are not required or packaged. TownOfUsMira.dll conflicts with this integrated arrangement and is not a prerequisite. Other optional compatibility integrations exist in inherited code; their plugins are not bundled or certified here.

Licenses, authors, obligations and unresolved transitive review are in THIRD_PARTY_NOTICES.md. No unsupported guarantee of LGPL compliance, full runtime redistributability or clean multiplayer compatibility is made.
