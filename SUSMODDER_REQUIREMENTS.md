# SUS Modder requirements and integration evidence

Rechecked 2026-11-18. Public sources: [website](https://susmodder.app/), [repository](https://github.com/boratsc/SUSModder), inspected commit [68ba478ac02c14efb091b21d502f8d695d3255e9](https://github.com/boratsc/SUSModder/tree/68ba478ac02c14efb091b21d502f8d695d3255e9). The website exposed no readable submission specification through the available fetch; the following technical findings come from actual client source, not an invented intake policy.

## Requirements communicated directly by the moderator (via project owner)

- A public GitHub repository.
- Open source code.
- A repository link.
- A description of the mod.
- A full role list.

Documents/source are prepared locally, but the repository/link requirements are not satisfied until publication completes (owner authorization was provided on 2026-11-17). No message or submission was sent.

## Confirmed public implementation facts

- `SUSModder.Core/Api/Models/CatalogModels.cs` represents versions, beta/stable selection, compatible Among Us version and platform/architecture variants.
- Variants can carry version, downloadUrl, SHA-256 and fileSizeBytes. `CatalogMapper.cs` maps role availability, icons, description and version metadata from the backend.
- `SUSModderApiClient.BuildModDownloadUrl` builds backend download URLs. The catalog, rather than arbitrary local repository names, supplies installation paths.
- The Epic installer downloads a ZIP, extracts it to a temporary staging area and installs against a resolved game path. Steam and Epic are separately represented in the codebase. This does not prove an identical archive root is accepted by every install route.
- The codebase includes role/catalog browsing and multiple release variants. It does not establish that a Markdown role list can be imported without moderator-side conversion.

These are fields supported by a particular client revision, **not proof that every field is mandatory for submissions**. A default x86 argument in an API method is not proof MTOU must be x86: the inspected game executable was x64, and the observed startup environment was Windows game 2026.9.29.

## Prepared good practices

Neutral plugin-only ZIP rooted at `BepInEx/plugins/TOU-Marmo.dll`; matching source ZIP; SHA-256 and file size; English README and concise role catalog; transparent beta, compatibility and licensing status; dependency requirements and clear all-client installation instructions. These are preparation choices, not a claim of a mandatory SUS Modder manifest standard.

## Must be confirmed with the moderator

- Accepted package root/layout for MTOU's Steam and Epic install routes; whether the backend installs dependencies or expects a complete approved loader pack.
- Supported game version/store/architecture tuple after real testing.
- How to provide download URL, release channel and update metadata, and whether GitHub Releases is the accepted source.
- Exact role data/import schema, category identifiers and expected image sizes/URLs.
- Permissions for any promotional art and whether screenshots are requested.
- Submission procedure, review contact and any requirements not documented publicly.

No fake SUS Modder manifest was created. The local draft in SUSMODDER_SUBMISSION.md is review material. The candidate was not installed through SUS Modder or approved by its moderators. Original MTOU graphics now have owner-selected CC BY 4.0 terms; inherited animation/media clearance is a separate pending issue. Acceptance is their decision.



The current public SUSModder main commit was fetched on 2026-11-18 and remains 68ba478ac02c14efb091b21d502f8d695d3255e9, matching the source revision inspected for the implementation facts above. No additional mandatory intake format was established. Public repository lookup for marmoladka2137/Marmo-Town-of-Us returned 404 to the connected account; a verified public link remains missing.
