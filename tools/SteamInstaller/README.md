# Installation engine

Player download and instructions: [INSTALLATION.md](../../INSTALLATION.md). This directory contains the installation engine used by the current Steam/Epic launcher, not a separate player download.

The engine copies the user's installed game into a separate destination, validates its expected build, obtains pinned dependencies and verifies payload hashes. It rejects path traversal and unsafe destinations. It does not distribute Among Us or install a separate TownOfUsMira plugin.

Build and test entry points: Build.ps1 and Test.ps1, using Windows .NET Framework 4.8. Package.ps1 validates its manifest inputs. The launcher build consumes InstallerCore.cs directly. Current verification and limitations: [LAUNCHER_RELEASE_TESTS.md](../../LAUNCHER_RELEASE_TESTS.md).

Code GPLv3; Marmo artwork CC BY 4.0. Dependencies retain independent licenses and source obligations; see THIRD_PARTY_NOTICES.md and DEPENDENCIES.md. Do not substitute unpinned libraries.
