> Current evidence: [tests actually run on 2026-11-18](TEST_RESULTS_2026-11-18.md). Older results below remain historical.

> Follow-up 1.1.0-beta.2: see [GAMEPLAY_FIX_REPORT.md](GAMEPLAY_FIX_REPORT.md). The earlier publication snapshot and test history below remain baseline evidence, not verification of a complete match for this build.

# Test results — 2026-11-17

## Passed checks

| Check | Result / actual scope |
|---|---|
| Release solution build | PASS, 0 errors and 426 warnings on the full build; .NET SDK 10.0.403, target net6.0. No claim that every warning is harmless or newly introduced. |
| Locked NuGet restore | PASS using packages.lock.json and nuget.org. Only PerfectComms.Api 4.1.7.1 remains as a direct package. |
| Portable Check-Source.ps1 entry point | PASS with explicit GameRoot and SDK path; build output is not deployed automatically. |
| Extracted source archive rebuild | PASS: extracted candidate ZIP into a different directory, restored locked packages, rebuilt Release with the same local GameRoot. Rebuilt DLL SHA-256 exactly matched the tested DLL. This does not prove an independent public-dependency installation is reproducible. |
| Check-Rules.ps1 | PASS: 998 in-memory assertions for deterministic rules, including Doctor percentages, Executioner availability/candidates, role-loan limits, misfire eligibility and path/group logic. Also locale XML/keys and numeric RPC source checks. These are not full role/RPC execution tests. |
| Check-JesterExile.ps1 | PASS: 231 ballot/exile state assertions; includes nonqualifying states and preserved results. No actual voting UI/network transport. |
| Check-Spikes.ps1 | PASS: 54 replicated vent-state assertions; placement limits, duplicate entry suppression, counters and single-use state. No live vent movement/network packets. |
| Candidate/startup/manifest assertions | PASS: 26 checks of native version, stamp active state, registration parser, twelve MTOU roles appearing once, Bandit not being a modifier, duplicate Composter absent, single plugin entry point, no embedded DLL implementations and key resources present. |
| Native Unity startup | PASS WITH DIAGNOSTICS: final DLL loaded with Reactor, MiraAPI, AchievementsAPI, CorsacCosmetics and a local QA probe in an isolated game copy. No AleLuduMod, AUnlocker, Mini.RegionInstall or TownOfUsMira plugin loaded. |
| Source inventory/security patterns | Source tree enumerated and text scanned for selected token/private-key/webhook patterns and personal build paths. No matches in the checked candidate; pattern checks are not a complete security audit. |

Total: **1,283 deterministic gameplay-rule/state assertions plus 26 candidate/startup/manifest assertions**. Locale/RPC checks are additional source checks, not an inflated count of multiplayer tests.

## Native environment and evidence

Observed game: **2026.9.29**, Unity **2022.3.44f1**, Windows x64 executable. Probe window requested 960×540; this was a startup fixture, not an animation/resolution regression match. The probe wrote a registry snapshot and exited. Native role registry: 111 custom/mode variants and 11 vanilla entries. Modifier registry: 158 entries; source classification separates 51 regular assignable types, two system-assigned helpers, two test-only helpers and 103 temporary effects/markers.

`ModStamp.gameObject.activeInHierarchy` was true. `CurrentModRegistration.TryGetModRegistrationGuid` returned true and the field contained MTOU's stable UUID. This verifies actual native helper parsing, not an official-server lobby exchange. Loaded bundle enumeration produced 1,931 internal paths across six loaded bundles, including 488 paths in the embedded Windows TOU bundle; rights remain unverified.

The tested final DLL and the isolated test-copy DLL had identical SHA-256:

`7CA7C8D9559CC952FF528B1323DA47535C2D9870616FAD2C2941D17353C54D27`

Private raw build/native logs and fixture sources are retained outside the clean publication tree in the preparation workspace. Public structured evidence is in docs/audit/native-startup.json, roles.csv, modifiers.csv, bundle-assets.csv and dependency hashes. Startup still logs IL2CPP registration and duplicate-tab/string diagnostics; plugin loading is not described as error-free.

## Not executed / not certified

- A full match, actual multi-client host/client synchronization or a public win screen.
- Official-server lobby creation/filtering/joining using the registration helper.
- Stamp visibility in every gameplay state, meeting and scene transition.
- Executioner button clicks and actual target/Amnesiac change across host and clients.
- Jester/Spectre actual vote, tie/skip, special-death and final winner UI across clients.
- Deputy repeated shots, shields, interruptions, death/end/next-game cleanup or 1920×1080/other proportions for this candidate. Historical fixtures from an older task are not counted as a new run.
- Doctor reveal/cosmetics/audio for every player, shield/guessing/exile combinations; all Dirtek/One More Chance protected-death paths.
- Complete Exorcist drag/group/history/disconnect cases; Sweets cycles with several Sweets, immunity and latency.
- Furas collision/attack, Bandit loan timing, Hugger meeting/world kills, Gambler and all modifier combinations in real gameplay.
- Fresh end-user installation using only independently sourced public dependencies, Epic parity, SUS Modder installation or update flow.
- Complete manual semantic review of every source file, every generated bundle asset's editable source, or all transitive licenses.

These limitations remain release-checklist items. Passing compilation or a type appearing in the registry is not evidence that its abilities work correctly in a match.


## 1.1.0 Beta metadata candidate — 2026-11-17

Release compiled with 0 errors / 427 warnings. CosmeticSelectionStore tests: 12 assertions passed. No native restart or multiplayer test was repeated for this metadata-only change. All earlier results above apply only to the binaries and scopes explicitly identified there. See PUBLICATION_STATUS.md.

