# Source provenance

Audit snapshot: 2026-11-17. The main local MTOU root was **not a Git repository**; no MTOU commit history or clean/dirty Git claim can be reconstructed from it. Its external TOU checkout does have Git history. The original working directory, original DLL and old documentation were backed up outside this publication tree before editing.

## Chosen baseline

The disk project had internal version 1.1.5-beta and includes the Jester/Deputy changes. The supplied Downloads archive had 1.1.0-beta: 3,303 matching files, 78 differing files, 18 disk-only and one archive-only file in the comparison. The archive was not used to roll back disk code.

The installed DLL was internally 1.1.4.0, SHA-256 `C0118124B42C04EC9D38E2C91C4BF8AFB8417EDEEF7FF785BD454A3835DBB607`. The pre-preparation source build was 1.1.5.0, SHA-256 `1D243575E0162DDE6CB325A36DA00EF7D5A97605640B0988CAB550E14D44D9CE`. Both displayed 1.1 BETA. They are not interchangeable evidence of tested behavior.

The publication copy intentionally normalizes its version to **1.1.0-beta** as requested, retaining source fixes and the existing `pl.marmo.toumarmo` plugin ID. This is a naming normalization, not a version downgrade of source content. Version handshakes require participants to use this exact publication build.

## Upstream base

[AU-Avengers/TOU-Mira](https://github.com/AU-Avengers/TOU-Mira), exact local checkout HEAD [b56f24a0e5fed86ce7decddf227e292672ebd857](https://github.com/AU-Avengers/TOU-Mira/commit/b56f24a0e5fed86ce7decddf227e292672ebd857), dated 2026-10-18, ToU Mira 1.7.9 Beta 1. This identifies the available upstream checkout, not proof that every integrated file originated at exactly that commit.

`IntegratedTownOfUs/` is compared by relative path and SHA-256 against `TownOfUs/` at that exact commit. See [source-provenance.csv](docs/audit/source-provenance.csv) for unchanged, modified and added files. New MTOU classes live in Roles, Modifiers, Buttons, Modules, Options, Events, Patches and Resources. Original namespaces and resource logical names are retained for compatibility. Modified integrated C# files carry a dated publication notice; original notices are retained.

The upstream working checkout had uncommitted changes, including Discord patches, locales, Jester and plugin/RPC code, plus untracked Composter additions. It was preserved, not reset. This preparation compares against Git HEAD rather than treating the dirty checkout as a pristine release.

## Why external is omitted

The main project already excludes `external/**/*.cs`; it compiles the integrated tree. A second full TOU source checkout and its binaries are unnecessary for this build. The exact upstream commit, original README, GPL text and per-file diff inventory preserve its provenance without compiling duplicate roles. Seven older integrated Composter files remain explicitly excluded by the project; the active Composter is `TouMarmo.Roles.ComposterRole`.

Submodule history: MiraAPI `7a4e8913677815c3a6fbd632fad3a05bb361ae3b` had a local plugin-credit change; AchievementsAPI checkout was `89448b6019338c524e7b2227f2204da485a49fe6`. The installed Achievements binary instead identifies `e62c9c9032d293187d170f2e4ac37d05c1c11a63`. The installed MiraAPI binary has no proven matching commit. These dependencies are **not** bundled in the plugin-only archive; do not substitute the submodule revisions as certified corresponding sources for unrelated binaries.

## Publication changes and limitations

Build configuration, version/metadata, documented all-client registration helper, stamp activation, portable test entry point and publication documents were changed in this copy. No gameplay mechanism was removed to hide a defect. Existing source-reference links are inventoried in docs/audit/source-references.csv; original author/license verification for all historic copied snippets remains incomplete. Filesystem dates are not asserted as development history.

## Asset/license audit supersession — 2026-11-17

The initial per-file inventory and broad unknown-rights wording are superseded by LICENSE_AUDIT.md and docs/audit/recheck-2026-11-17. This recheck identifies the embedded OFL font, mixed version-specific Il2CppInterop licenses, 17 exact cosmetic blob matches and concrete asset origins. Original source provenance and preserved historical evidence are not removed. See LICENSE_RECHECK_PL.md for the Polish report.

## 2026-11-18 owner artwork grant

Marmo confirmed ownership/public-sharing rights for original MTOU graphics and selected CC BY 4.0. See MTOU_ARTWORK_MANIFEST.csv and MTOU_ARTWORK_LICENSE.md. Previous missing-origin statements for those exact 38 PNGs and the logo are superseded. Original author credits and separate third-party terms remain. The official Unity asset project and its separate animation condition were located; see LICENSE_AUDIT.md. No runtime C# or asset bytes changed.

## Original runtime resource replacement — 2026-11-18, revision 1.1.0.12

Modules/OriginalMotion, tools/OriginalMotionGenerator, tools/OriginalResources, Resources/OriginalMotion and the rebuilt main bundles are the new source-controlled replacement pipeline. TouAssets registers the owned catalog; TouAudio uses owned synthesis. The two integrated Composter icon files are exact verified MTOU copies. Four unused standalone recordings are excluded. No runtime DLL patching or installed-game overwrite was used.

The three AutoRejoin integration/implementation files were newly replaced, preserving integration class names and behavior settings. Original attribution remains historical; no formal clean-room claim is made. Private earlier snapshots remain outside publication. Current manifests and LICENSE_AUDIT.md supersede older resource questions for this exact candidate.
