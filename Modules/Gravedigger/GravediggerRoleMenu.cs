using MiraAPI.Hud;
using UnityEngine;
namespace TouMarmo.Modules.Gravedigger;

// MiraAPI's existing paged phone menu supports role entries without retaining dead PlayerControls.
public sealed record BuriedRoleEntry(RoleBehaviour Role, string PlayerName);
public sealed class GravediggerRoleMenu : CustomMultiSelectMenu<BuriedRoleEntry>
{
    public static GravediggerRoleMenu CreateMenu() => Create<GravediggerRoleMenu>();
    public void Open(IEnumerable<BuriedRoleEntry> roles, Action<BuriedRoleEntry?> onClick) => Begin(roles, onClick, false);
    protected override void SetupPanelEntry(ShapeshifterPanel panel, int i, BuriedRoleEntry entry, Action onClick)
    {
        // The same inline TMP role icon used on revealed meeting nameplates.
        // Do not clone Background: its sliced renderer stretches role sprites.
        panel.SetPlayer(i, PlayerControl.LocalPlayer.Data, (Il2CppSystem.Action)onClick);
        panel.PlayerIcon.gameObject.SetActive(false);
        panel.LevelNumberText.transform.parent.gameObject.SetActive(false);
        if (panel.ColorBlindName) panel.ColorBlindName.gameObject.SetActive(false);
        panel.NameText.richText = true;
        panel.NameText.text = Label(MiscUtils.GetRoleTmpIcon(entry.Role), entry.Role.GetRoleName(), entry.PlayerName);
        panel.NameText.color = entry.Role is MiraAPI.Roles.ICustomRole custom ? custom.RoleColor : entry.Role.TeamColor;
        panel.NameText.alignment = TMPro.TextAlignmentOptions.Center;
        var position = panel.NameText.transform.localPosition;
        panel.NameText.transform.localPosition = new Vector3(0f, 0f, position.z);
        panel.NameText.enableWordWrapping = false;
        panel.NameText.overflowMode = TMPro.TextOverflowModes.Ellipsis;
        panel.NameText.enableAutoSizing = true;
        panel.NameText.fontSizeMin = 1f;
        panel.NameText.fontSizeMax = 2f;
    }
    internal static string Label(string icon, string role, string player) =>
        icon + Escape(role) + "\n<size=85%><color=white>" + Escape(player) + "</color></size>";
    internal static string Escape(string text) => text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}
