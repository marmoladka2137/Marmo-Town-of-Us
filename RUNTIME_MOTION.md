# Original runtime motion architecture

Resources/OriginalMotion has 1,511 original frames for 55 named contracts. OriginalMotionCatalog preserves clip name/duration identities; original legacy timing curves contain no borrowed visual motion. OriginalMotionPlayer drives a dedicated child renderer, leaving actor transforms/colliders intact and copying source material, sorting, visibility, flip and mask state.

Harmony intercepts only the instance IDs loaded from MTOU's cleaned bundle. Base-game and other mod clips retain their native engine. Stop/pause/resume/speed/time/normalized-time queries route to the same owned MotionClock. Switching to a non-MTOU clip restores the source renderer and Animator state. Unity startup unload protection retains the owned runtime assets intentionally for the application's lifetime; presentation children are destroyed with their actors.

OriginalAudioAsset synthesizes each named cue without input recordings or random game state. OriginalMaterials uses installed-game shaders and new surface fills. Two platform bundles have zero old recordings/controller graphs/GPU programs; legacy bundles are static resources. Native registration uses the type-taking LoadAllAssets overload because the generic ConvertObjects helper is stripped from the tested game.

The generator uses names/durations/loop contracts, not source pixels or old curves. The reproducible compiler clears visual clip tracks/mappings, replaces silhouette meshes with rectangular layout interfaces, substitutes new frames, inlines permitted static texture pixels and strips old stream files. Names and durations are compatibility interfaces. Preferred modification source is included in tools/OriginalResources/Sources and tools/OriginalMotionGenerator.

These replacements alter visual/audio style, not role cooldowns, attack rules, RPC identities, protections or victory rules. Full multiplayer gameplay remains unverified. No formal clean-room or zero-bug claim is made.
