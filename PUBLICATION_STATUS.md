# Publication status — 1.1.0-beta

Public repository: https://github.com/marmoladka2137/Marmo-Town-of-Us

Steam installer revision: a separate installer creates a local game copy, downloads pinned official dependencies and installs the unchanged 1.1.0.13 plugin. Installation and native startup with public MiraAPI 0.6.0 passed; full match/multiplayer testing remains unverified. No Among Us game files or dependency implementations are included in the installer archive. See [installer evidence](STEAM_INSTALLER_TESTS.md).

Current source adds the text-only revision 1.1.0.13: concise English role/modifier descriptions and compact catalogs. Gameplay C# is unchanged. The existing `v1.1.0-beta` download remains revision 1.1.0.12; its matching tagged source and archives are preserved. See [text verification](TEXT_UPDATE_TESTS.md).

The 1.1.0-beta publication is a public beta with complete code, original-resource sources, documentation and plugin-only artifacts. Public label: 1.1.0 Beta; internal assembly revision: 1.1.0.12. Plugin identity is unchanged. The Source Code menu link now points to this repository.

The former animation/audio/shader-content hold was resolved for this candidate through original replacements. See LICENSE_AUDIT.md, ASSET_CREDITS.md and the exact manifests. Code GPLv3, original MTOU graphics CC BY 4.0, retained font OFL-1.1; third-party notices remain separate.

Local Release compilation, native presentation/resource fixtures and deterministic state tests passed. Full gameplay, host/client transport, clean public dependency installation, additional platforms, official-server behavior and SUS Modder installation/acceptance remain unverified. A public beta is not certification of every role or a legal/moderator approval.

The release does not contain a complete game or external loader/dependency implementation DLLs. Do not add unreviewed third-party payload or older resource-bearing DLLs to it. The original local working installation was not overwritten.
