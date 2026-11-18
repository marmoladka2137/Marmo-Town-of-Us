# Steam installer verification — 2026-11-18

## App launcher follow-up

The additional app ZIP supplies `Marmo Town of Us.exe` with the existing Discord `mtou_logo` icon. The installed desktop shortcut targets the launcher; first use without a game opens the included installer. No mod DLL or gameplay C# changed.

- Updated installer checks: **36 passed**, including launcher pin verification and copying.
- Launcher checks: **14 passed**, including installation discovery, rejecting an unmodded/conflicting folder, starting a real fixture child through the compiled launcher and preserving its working directory.
- The launcher started the isolated native MTOU copy and reached the main menu with MTOU 1.1.0.13 and public MiraAPI 0.6.0. The six existing game save/settings files stayed unchanged. Unity wrote its normal Player.log, so the first harness's overbroad profile guard reported a log-file difference. Optional diagnostic log redirection was then added and covered by launcher tests. A further native repeat was intentionally skipped because the owner had started their game.
- The owner's existing desktop shortcut was backed up, then updated to the app executable and its embedded icon. The installed game/plugin files were not replaced.

No launcher multiplayer test or second-PC test was performed. The historical installer checks below retain their original scope and results.

## Artifact scope

MTOU 1.1.0 Beta, plugin assembly 1.1.0.13; Windows x64 installer 1.0.0.0. The installer contains the unchanged text-update plugin, its own C# sources, a pinned manifest and license notices. It downloads dependencies directly from an official upstream archive during installation. No Among Us game files, generated wrappers, external plugin implementations, private configuration or QA probes are distributed.

Plugin SHA-256: `2890EA25293C2EA3F318288DC4C46556568B00847F1FB44218469240AC28A97C`.

## Executed

- Compile the GUI installer with the Windows .NET Framework compiler: passed, no compiler diagnostics.
- Run `tools/SteamInstaller/Test.ps1`: **34 checks passed**. Includes Steam VDF parsing, archive traversal, copy exclusions, game/payload/dependency hash checks, incomplete-install isolation, notices and refusal to overwrite an existing destination.
- Run the production installer core with an actual Steam installation and network downloads, not mock game files: passed. A new copy was created; all 233 selected runtime files were verified. Only MTOU, MiraAPI, Reactor and CorsacCosmetics were installed as gameplay plugins.
- Start that newly installed copy with a private test-only probe and isolated profile: reached the native main menu and player prefab; verified MTOU 1.1.0.13, official public MiraAPI 0.6.0 and isolated save path. Fresh IL2CPP wrappers were generated locally. The probe quit its own test process after the assertions.
- Check existing profile files and TEST MODY plugin hash: unchanged. Restore temporary display settings after the isolated test.
- Compile current MTOU source against public MiraAPI 0.6.0: zero errors, the same 427-warning count as the earlier baseline.
- Inspect the actual installer window: source Steam folder detected correctly; input fields, progress bar and action button fit without overlap.

## Observed diagnostics and limitations

Startup logs still include inherited IL2CPP registration messages, duplicate English `Options`/`Enabled`/`Disabled` strings and developer news/version lines written at Error level. Main-menu startup completed despite these messages; this report does not claim an error-free log or repair those unrelated systems.

No match, individual role interaction, multiplayer, clean second-PC installation, alternate game build, alternate store or multi-resolution game session was tested for this installer. Desktop shortcut creation and the GUI Play button were not exercised; installation and startup were tested through the same core and an isolated test launcher. The game build gate intentionally rejects a different GameAssembly.dll instead of assuming forward compatibility.

The installer is unsigned. Building does not grant Microsoft signing trust. No security prompts, Steam ownership checks or game DRM are bypassed.

Full earlier test scope: [TEXT_UPDATE_TESTS.md](TEXT_UPDATE_TESTS.md) and [RUNTIME_MOTION_TESTS.md](RUNTIME_MOTION_TESTS.md). Successful installation and startup are not full gameplay certification.
