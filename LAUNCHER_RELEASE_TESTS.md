# Launcher release verification Ă˘â‚¬â€ť assembly 1.1.0.19

Executed locally on Windows, 2026-11-19:

- MTOU Release build: 0 errors, 425 existing warnings. Visible version: 1.1.0 Beta.
- Launcher build: 0 errors or warnings, .NET Framework 4.8 / x64.
- Launcher discovery/process tests: 14 assertions passed. The real launcher executable starts a fixture child with the right working directory; no game/DRM arguments are injected.
- Installer fixtures: 39 assertions passed, including separate destination, immutable source, pinned downloads/payload, traversal rejection and failure isolation.
- Update/Epic discovery tests: 29 assertions passed. Includes drafts, revision comparison, SHA-256 verification, corrupt/incomplete ZIPs, locked-file rollback including an already replaced earlier file, configuration preservation and Epic manifest discovery.
- VentSpikeState: 61 assertions passed, linked directly from the production source. Covers limits up to 50, per-owner limits, vent-to-vent entries, duplicate trigger suppression, single-use nails, damage threshold, rearming and game reset.
- Native Unity startup: DLL loads; the main-menu Discord link is present. The private Practice Plumber test did not reach gameplay: after the port-conflict attempt, retries with a private-port fixture also timed out. Native vent switching and remote player synchronization remain unverified. No two-client multiplayer pass is claimed.
- Launcher UI displayed using the supplied banner and existing Discord application icon. Public feed without an eligible update shows a status message, not a crash dialog.
- AleLuduMod 1.2.3, AUnlocker 1.3.2 and Mini.RegionInstall 1.2.0 DLL hashes match their official tagged release assets. Exact tagged source ZIPs/licenses accompany those binaries.

Unverified: actual Epic game authentication/startup, all host/client synchronization, complete role interactions, SUS Modder integration and live installation of a newer public update from a published release. HTTP/ZIP/update logic was tested with fixtures; that is not an end-to-end public release test.

The initial native Practice attempt was blocked by another Among Us process occupying UDP 22023. Only the private QA process was stopped. The user's TEST MODY DLL and running client were not overwritten or closed.


- Additional compact-payload tests verify extraction of the pinned DLL from the supplement ZIP without a duplicate loose DLL, including SHA-256 failure. Update tests cover the public channel and API-unavailable fallback.

- Chunked update tests cover exact reconstruction, per-part size/hash errors, part path validation and the full assembled ZIP hash.

## Public channel and real Steam copy â€” 2026-11-19

50 assertions passed against the actual public `downloads` branch: downloaded bootstrap matches the published artifact hash, all three mod parts and the assembled archive verify, the compact payload resolves correctly, and a separate copy of the locally installed Steam game receives exactly AleLuduMod, AUnlocker, CorsacCosmetics, Mini.RegionInstall, MiraAPI, Reactor and MTOU. Reapplying the update preserves configuration and the installed DLL matches its pinned SHA-256. This is a real download/install check, not a multiplayer or complete gameplay test.

The canonical source archive was checked against every Git blob in the published source tree. Windows archive export can convert text line endings; the packager now explicitly disables that conversion. Source code and embedded resources correspond to assembly revision 1.1.0.19. The later packaging/documentation commits do not change the compiled gameplay or launcher UI code.

## Dependency correction discovered by native startup

AleLuduMod 1.2.2 threw `Null method for aleludu.miraapi.patch` against public MiraAPI 0.6 because CustomPlayerMenu.Begin gained parameters. The official AleLuduMod 1.2.3 fixes this reflection lookup. The final seven-plugin package upgrades AleLuduMod to that official version, with its tagged sources and MIT notice. A repeat native startup reached the main menu, displayed the Discord link and logged successful patching of both Begin overloads without that initialization exception. This does not turn the unsuccessful Practice/vent test into a gameplay pass. The visible version stays 1.1.0 Beta; internal revision 19 makes installations with a lower internal revision eligible for this dependency correction.

## Unified launcher version — 2026-11-19

Launcher rebuild: zero errors. Discovery/process tests: 26 assertions passed. Setup UI state tests: 11 assertions passed. Update/Epic discovery tests: 29 assertions passed on the rebuilt launcher sources. ProductVersion is 1.1.0 Beta; file/assembly version is 1.1.0.19, derived from the game project. This launcher-only change does not claim another gameplay test.
