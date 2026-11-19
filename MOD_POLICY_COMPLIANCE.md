# Mod policy implementation notes

Official references: [Innersloth Mod Policy](https://www.innersloth.com/among-us-mod-policy/) and [technical modding information](https://github.com/Innersloth-LLC/AmongUsModdingInformation). This is an implementation record, not a new policy verification.

- All-client role/RPC architecture: every participant needs a compatible MTOU version.
- Mod Stamp uses ModManager.ShowModStamp. Native startup was checked; uninterrupted gameplay/meeting coverage remains unverified.
- Registration helper UUID: f3dc9545-e8f3-4a99-a95d-17ae396dd698. Real official-server hosting/filtering/joining remain unverified.
- Public packages contain no base game or private profiles. The launcher copies the user's installed game locally.
- Required Innersloth disclaimer is retained in README.
- Native Mira resources retain independent rights; see LICENSE_AUDIT.md for unresolved permissions.

Preserve plugin identity, source credits, notices and the registration UUID. No complete policy or legal clearance is claimed.
