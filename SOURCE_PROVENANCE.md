# Source provenance

Current MTOU publication: **1.1.0 Beta**, file revision **1.1.0.19**. Plugin ID: pl.marmo.toumarmo. The launcher and mod payload each record their corresponding source commit in installer-manifest.json; packaging-only commits do not change the gameplay DLL.

## Upstream base

[AU-Avengers/TOU-Mira](https://github.com/AU-Avengers/TOU-Mira), exact local checkout HEAD [b56f24a0e5fed86ce7decddf227e292672ebd857](https://github.com/AU-Avengers/TOU-Mira/commit/b56f24a0e5fed86ce7decddf227e292672ebd857), dated 2026-10-18, ToU Mira 1.7.9 Beta 1. This identifies the available upstream checkout, not proof that every integrated file originated at exactly that commit.

`IntegratedTownOfUs/` is compared by relative path and SHA-256 against `TownOfUs/` at that exact commit. See [source-provenance.csv](docs/audit/source-provenance.csv) for unchanged, modified and added files. New MTOU classes live in Roles, Modifiers, Buttons, Modules, Options, Events, Patches and Resources. Original namespaces and resource logical names are retained for compatibility. Modified integrated C# files carry a dated publication notice; original notices are retained.

The upstream working checkout had uncommitted changes, including Discord patches, locales, Jester and plugin/RPC code, plus untracked Composter additions. It was preserved, not reset. This preparation compares against Git HEAD rather than treating the dirty checkout as a pristine release.

## Why external is omitted

The main project already excludes `external/**/*.cs`; it compiles the integrated tree. A second full TOU source checkout and its binaries are unnecessary for this build. The exact upstream commit, original README, GPL text and per-file diff inventory preserve its provenance without compiling duplicate roles. Seven older integrated Composter files remain explicitly excluded by the project; the active Composter is `TouMarmo.Roles.ComposterRole`.

Submodule history: MiraAPI `7a4e8913677815c3a6fbd632fad3a05bb361ae3b` had a local plugin-credit change; AchievementsAPI checkout was `89448b6019338c524e7b2227f2204da485a49fe6`. The installed Achievements binary instead identifies `e62c9c9032d293187d170f2e4ac37d05c1c11a63`. The installed MiraAPI binary has no proven matching commit. These dependencies are **not** bundled in the plugin-only archive; do not substitute the submodule revisions as certified corresponding sources for unrelated binaries.

## Publication changes and limitations

Build configuration, version/metadata, documented all-client registration helper, stamp activation, portable test entry point and publication documents were changed in this copy. No gameplay mechanism was removed to hide a defect. Existing source-reference links are inventoried in docs/audit/source-references.csv; original author/license verification for all historic copied snippets remains incomplete. Filesystem dates are not asserted as development history.
