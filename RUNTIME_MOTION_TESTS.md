# Runtime-motion candidate test results — 2026-11-18

Native fixtures below were executed on internal revision 1.1.0.11. Publication revision 1.1.0.12 changes only the Source Code repository URL and version metadata, plus documentation; it was separately compiled. No full native rerun of revision 1.1.0.12 is claimed. Public version remains 1.1.0-beta / 1.1.0 Beta. Tests used the local legally owned Windows x64 development references. Native runs used an isolated executable/profile copy; no installed TEST MODY DLL deployment.

| Test | Actual result and boundary |
|---|---|
| Release build | 0 errors, 427 existing warnings; replacement code adds no warning |
| Native original motion/material/audio fixture | **960 assertions passed**: all 55 clip durations/identity, selected own PNG frames, native PCM decoding/readback for all 59 cues, actual frame progression, pause/seek/stop, stationary actors, bounded scale, no duplicate renderers, 11 supported materials, four original fills, 18 actual prefab layouts and child cleanup |
| Native Doctor/Deputy regressions | **33 assertions passed**: Doctor stationary presentation/medical appearance, Deputy normal/queued/duplicate callbacks, 1920×1080 and 1280×960 render captures, death/meeting/end/round cancellation, disabled/destroyed overlay, paused timescale and cleanup. Fixture events are not actual kills/meetings or multiplayer |
| Owned playback clock | **1,060 assertions passed** at 24/30/60/144/240 simulated FPS; duration, replay, pause, speed, loop, stop, invalid inputs |
| New return-to-lobby state | **32 assertions passed**: host/client/never mode matrix, deadlines, single action, cancellation and rearming; no live end-of-match click test |
| Existing Doctor/Deputy timelines | **5,670 assertions passed** on production timing and viewport math |
| Role rules | **998 assertions passed**, plus XML/locale/intro and 38 unique numeric feature RPC checks |
| Jester ballot/exile state | **231 assertions passed**, no actual voting/network transport |
| Plumber vent-spike state | **54 assertions passed**, no live vent/network test |
| Cosmetic store | **12 assertions passed**, synthetic files |
| Serialized resource audit | **2,623 assertions passed**: 1,511 frame hashes, both platform containers, zero old audio/controller/shader objects, cleared visual/packed tracks, zero-object format schemas, static-only legacy bundles, no standalone recordings and exact original duplicate icons |
| Frame regeneration | The supplied C# generator independently regenerated 1,511 PNGs; complete frame-manifest bytes/SHA matched |
| Player profile guard | Six real player/settings files unchanged after each native fixture; Unity display registry restored. No resolution-changing command line or real-profile skin edit |
| Archive rebuild | Separate source extraction/rebuild is recorded in artifacts/CORRESPONDENCE.json; do not infer cross-machine/public dependency reproducibility from a local match |

Native captures were visually inspected, including original shields, replaced meeting/character effects and prefab layouts. These are deliberately different artwork, not cloned Mira frames. The rebuilt surfaces use original fixed raster patterns instead of the inherited GPU effects.

Early experiments are not counted as passing: WAV-in-container decoding failed and was replaced by runtime PCM synthesis; stripped generic LoadAllAssets failed and was replaced with its native type-taking overload; startup asset unloading reset clips until explicit owned lifetime protection was added. An uninitialized Medium owner in the prefab-only fixture was a fixture setup error; final visual fixtures disable unrelated role behaviours and make no claim to exercise Medium gameplay.

## Not verified

Actual gameplay for every role, host plus client transport, attack/meeting/exile gameplay synchronization, public dependency clean install, Steam/Epic parity, Android/iOS runtime, official-server matchmaking, uninterrupted gameplay Mod Stamp and SUS Modder installation/acceptance. AutoRejoin's actual result-screen click remains unverified; its new pure state and compilation passed. Cached own frames/PCM intentionally live for the application; a long multi-match native memory soak was not run. Visual prototypes are not a stable-release certification.

Raw executed fixture reports are copied under docs/audit/runtime-motion-2026-11-18. Earlier dated reports elsewhere describe earlier DLLs and do not override these current boundaries.

