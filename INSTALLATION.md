# Test installation

For the new Steam installer, see [tools/SteamInstaller/README.md](tools/SteamInstaller/README.md). It creates a separate copy from a supported, already installed Steam game and obtains pinned dependencies automatically. The instructions below describe manual plugin-only installation.

This is an all-client plugin-only candidate, not a complete Among Us installer or a certified SUS Modder package. This new original-resource beta candidate is prepared for owner review; see RELEASE_CHECKLIST.md for unverified tests.

1. Create a separate copy of your legally owned compatible Among Us PC installation. Keep your normal game and configuration backed up.
2. Set up the matching BepInEx IL2CPP environment and the prerequisites in DEPENDENCIES.md using their upstream releases. Start once for interop generation.
3. Close that test instance. Extract the candidate archive into its game root so the result is `BepInEx/plugins/TOU-Marmo.dll`.
4. Ensure this copy does not also load `TownOfUsMira.dll` or another duplicate MTOU DLL. Preserve any files you move outside `plugins`.
5. Start the test copy. Check the log for `Marmo Town of Us loaded.`, version `1.1.0-beta`, lobby-registration UUID and initialization failures. A successful line alone is not a complete compatibility test. All participants need this same build: the Necromancer attack-result RPC changed in beta.2 and Furas RPC payloads changed in beta.3.
6. Every participant must have the same MTOU build and compatible prerequisites. Verify a real host/client lobby before playing a match.

MTOU embeds its own resources. It does not need a separately copied MTOU textures folder. CorsacCosmetics and upstream cosmetic discovery can download cosmetic bundles; these are separate from the plugin archive and have not been cleared by this audit.

Not required or bundled: AleLuduMod, AUnlocker, Mini.RegionInstall, QA probes, another Town of Us plugin, game binaries, cache, logs or private configuration. Perfect Comms is an optional integration, not installed by the compile-only API package.

Do not overwrite your working TEST MODY DLL until you explicitly decide to test this candidate. To undo, close the test copy and restore its backed-up plugin. No updater/deployment action is performed by the build scripts.

AchievementsAPI is no longer required by MTOU. Its achievement integration is removed; existing save data is left intact.



## Publication status, 2026-11-18

The prepared thin plugin and matching complete source are local beta candidates. Repository: https://github.com/marmoladka2137/Marmo-Town-of-Us. SUS Modder acceptance is not claimed. Publish matching source with any binary; no game or external loader libraries are part of this package. Full gameplay/multiplayer and a clean dependency installation still require verification. See PUBLICATION_STATUS.md.
