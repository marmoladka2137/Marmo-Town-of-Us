# Asset credits — current runtime-motion candidate

## Launcher icon

The app uses the existing MTOU Discord application artwork `mtou_logo`, selected by Marmo for the launcher. PNG SHA-256: `ECF5A2A9811591013DE650A55235C255752C3B2B76216B01A275F12131293AC1`. Source: https://cdn.discordapp.com/app-assets/1571528434805510255/1571542621472423986.png . Preferred source: tools/Launcher/DiscordLogo.png; tools/Launcher/Build-Icon.ps1 converts it to a multi-resolution Windows ICO. This is separate from the red Marmo wordmark. Existing original MTOU artwork attribution/CC BY 4.0 terms and Innersloth IP notices remain applicable.

## Original MTOU graphics

**Original MTOU artwork — created by Marmo. CC BY 4.0.** Keep attribution, license reference and modification notices. The owner selected these terms; no foreign resource is relicensed by that choice.

- MTOU_ARTWORK_MANIFEST.csv: 40 exact paths / 38 unique PNGs, including logo, role, modifier, ability, pet and Composter trap art. The two integrated duplicate icons are exact copies of verified originals. Old ambiguous files are excluded.
- ORIGINAL_MOTION_MANIFEST.csv: 1,511 new original procedural frames, 55 functional animation series. Source: tools/OriginalMotionGenerator; no copied animation curves or pixels.  The resource source's animated-texture aliases are byte copies of these frames, not further upstream art.
- Original C# Doctor/Deputy presentation, new playback, surface-fill generation and synthesized audio algorithms: GPLv3 code. Native game avatars/clothing and game shaders remain their rightsholders' work and are referenced at runtime.

## Retained upstream materials

Permitted static TOU Mira sprites retain AU-Avengers, AtonyGit and existing artist credits. The original source and notice boundary is [TownOfUs-Mira-Unity](https://github.com/AU-Avengers/TownOfUs-Mira-Unity/blob/0224bd7a0a98e3aebb70ebfe1f7c388ae6b5d3ec/README.md). Remaining upstream static textures are exported losslessly as editable PNGs under tools/OriginalResources/Sources/Textures. They are not called Marmo artwork.

Liberation Sans Regular 2.00.1 remains OFL-1.1: digitized data © 2010 Google Corporation; © 2012 Red Hat, Inc.; designer Steve Matteson. Full required notice: LICENSES/LiberationSans-2.00.1-OFL-1.1.txt. Font textures/native font metadata remain part of this separately licensed material.

## Replaced / omitted

Inherited motion curves, animator controllers, animated pixel sheets/silhouettes, 59 audio recordings/streams and four GPU programs were physically removed from each rebuilt main bundle. Names/durations and layout interfaces are preserved for compatibility. Four unused standalone WAV/MP3 files are omitted. No permission is claimed for omitted recordings or older animation files.

Both legacy-assets bundles contain only static image resources and retain upstream credits. Optional TownOfUs.*.ccb cosmetics are remote runtime downloads, not release ZIP content; artist/sub-asset and source concerns are not silently cleared. Innersloth game IP, fonts, Unity, trademarks and service materials retain independent rights.

Historical attribution is preserved in CREDITS.md, SOURCE_PROVENANCE.md and dated audits. The current package does not include the private upstream Unity snapshots, old recordings, installers, game files or failed prototype bundles.
