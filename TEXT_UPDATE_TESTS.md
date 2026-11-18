# Text revision 1.1.0.13

Public version label remains **1.1.0 Beta**. This source update changes English descriptions, catalogs and version metadata; it changes no gameplay C#.

- Shorten 370 existing locale strings; add five missing One More Chance name/description entries.
- Preserve all existing locale keys and non-description strings; no formatting placeholders are removed.
- Validate both edited XML files, unique keys and intro limits. Updated descriptions contain at most 49 words; integrated ToU descriptions contain at most 24.
- Preserve 111 role variants and 51 assignable modifier variants, with valid links to their implementations.
- Run `Tests/Check-Rules.ps1`: 998 algorithm assertions, locale checks and 38 unique numeric RPC IDs pass.
- Release build: zero errors, 427 existing warnings, matching the previous build count.
- Keep informational version metadata independent of the local Git checkout, so an extracted source archive can reproduce the DLL.
- Rebuild the extracted source archive: the resulting DLL matches byte for byte. SHA-256: `2890EA25293C2EA3F318288DC4C46556568B00847F1FB44218469240AC28A97C`.

Gameplay and multiplayer were not run for this text-only update. Earlier native test results remain attached to their original revisions. The previously published `v1.1.0-beta` binaries and source archives are not replaced by this source commit.
