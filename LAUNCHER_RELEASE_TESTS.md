# Launcher release verification â€” assembly 1.1.0.18

Executed locally on Windows, 2026-11-19:

- MTOU Release build: 0 errors, 425 existing warnings. Visible version: 1.1.0 Beta.
- Launcher 2.0.0.0 build: 0 errors or warnings, .NET Framework 4.8 / x64.
- Launcher discovery/process tests: 14 assertions passed. The real launcher executable starts a fixture child with the right working directory; no game/DRM arguments are injected.
- Installer fixtures: 39 assertions passed, including separate destination, immutable source, pinned downloads/payload, traversal rejection and failure isolation.
- Update/Epic discovery tests: 28 assertions passed. Includes drafts, revision comparison, SHA-256 verification, corrupt/incomplete ZIPs, locked-file rollback including an already replaced earlier file, configuration preservation and Epic manifest discovery.
- VentSpikeState: 61 assertions passed, linked directly from the production source. Covers limits up to 50, per-owner limits, vent-to-vent entries, duplicate trigger suppression, single-use nails, damage threshold, rearming and game reset.
- Native Unity startup: DLL loads; the main-menu Discord link is present. The private Practice Plumber test did not reach gameplay: after the port-conflict attempt, retries with a private-port fixture also timed out. Native vent switching and remote player synchronization remain unverified. No two-client multiplayer pass is claimed.
- Launcher UI displayed using the supplied banner and existing Discord application icon. Public feed without an eligible update shows a status message, not a crash dialog.
- AleLuduMod 1.2.2, AUnlocker 1.3.2 and Mini.RegionInstall 1.2.0 DLL hashes match their official tagged release assets. Exact tagged source ZIPs/licenses accompany those binaries.

Unverified: actual Epic game authentication/startup, all host/client synchronization, complete role interactions, SUS Modder integration and live installation of a newer public update from a published release. HTTP/ZIP/update logic was tested with fixtures; that is not an end-to-end public release test.

The initial native Practice attempt was blocked by another Among Us process occupying UDP 22023. Only the private QA process was stopped. The user's TEST MODY DLL and running client were not overwritten or closed.


- Additional compact-payload tests verify extraction of the pinned DLL from the supplement ZIP without a duplicate loose DLL, including SHA-256 failure. Update tests cover the public channel and API-unavailable fallback.

- Chunked update tests cover exact reconstruction, per-part size/hash errors, part path validation and the full assembled ZIP hash.
