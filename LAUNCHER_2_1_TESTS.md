# Launcher 2.1.0 verification — 2026-11-19

- Build: 0 compiler errors/warnings; current-user execution manifest, no requested elevation.
- 26 discovery/path/process assertions passed, including executable selection, invalid-folder rejection, collision avoidance and separate store copies.
- 11 isolated form-state assertions passed, including automatic Steam/Epic selection, Epic-only discovery, remembered destinations and disabled Play/Install states.
- 29 update/Epic assertions passed. Existing verified download, manifest, rollback and configuration-preservation behavior remains.
- Native Windows UI: actual Steam path detected; modern folder dialog opened with address bar/search/Quick Access; selecting a parent through the address bar updated the destination correctly; platform switching restored separate destinations; executable picker opened directly in the detected Steam game folder, filtered to Among Us.exe; cancellation returned to the form. These checks used a private profile with no game installation or launch.
- Defender custom scan of the final executable completed; no matching detections were reported. Real-time protection remained enabled. Authenticode status: NotSigned. No valid signing certificate was available; no self-signed trust change or antivirus exclusion was applied.
- Bootstrap refresh retains the exact original mod manifest, plugin payload pin and runtime dependency pins. Only launcher files, instructions and its own source/pin metadata change.

Limits: actual Epic game launch was unavailable; no multiplayer or new gameplay test is claimed. SmartScreen reputation is separate from antivirus results. A trusted code signature and reputation are needed to address publisher warnings; this build is not claimed to suppress them.

Microsoft documentation: https://learn.microsoft.com/windows/apps/package-and-deploy/smartscreen-reputation
Optional developer signing: tools/Launcher/Build.ps1 -SigningCertificateThumbprint <existing trusted certificate>
