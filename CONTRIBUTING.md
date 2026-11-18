# Contributing

Build using BUILDING.md and keep changes scoped. Preserve upstream author notices and resource names. Explain which MTOU or TOU Mira behavior changes, why, and which paths were tested.

Bug reports should contain: exact mod/game/dependency versions, Steam or Epic, host/client perspective, enabled role options, reproduction steps, expected versus actual result, and a redacted relevant log. Multiplayer claims require at least two actual clients; a single-client fixture is insufficient.

Before a pull request, build Release and run relevant deterministic tests. Record remaining runtime cases. Do not add DLLs, base-game files, tokens, generated interop caches, private configuration or artwork with uncertain permission. New assets need an author/source/license entry in ASSET_CREDITS.md. Gameplay/RPC changes need death, meeting, disconnect and reset checks.

Contributions to the combined code are intended for GPLv3 distribution. Confirm you have the necessary rights; third-party license obligations and attribution remain intact. No contributor agreement or official upstream partnership is implied.
