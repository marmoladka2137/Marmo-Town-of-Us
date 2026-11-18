# Known issues and unverified cases — 1.1.0-beta

- Full gameplay for every role/inter-role interaction, actual host/client RPC transport, attack/meeting/exile synchronization and several consecutive matches have not been verified by the local fixtures.
- Clean installation using only publicly available dependency builds, Steam/Epic parity, Android/iOS runtime and official-server behavior remain unverified. The recorded MiraAPI development build does not have a proven exact source/binary match. No external implementation DLL is distributed here.
- Double Shot and Triple Shot remain registered, and Hugger retains its current Double Shot interaction. The owner requested preserving the supplied code while recording retirement as unresolved; no silent retirement change was made.
- The original resource pipeline intentionally changes appearance/audio style. It passed local material, clip, cue and prefab fixtures; no long multi-match memory soak was run.
- AutoRejoin's new deadline/mode/cancellation state passed deterministic tests; its real end-of-match click remains unverified.
- SUS Modder intake format, installation and moderator acceptance are not confirmed. No submission was sent.

See RUNTIME_MOTION_TESTS.md, DEPENDENCIES.md, LICENSE_AUDIT.md and RELEASE_CHECKLIST.md. Publication does not mean all roles are guaranteed bug-free. Reports should include exact settings and a reproducible scenario.
