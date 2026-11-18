using System.Text;
using HarmonyLib;
using MiraAPI.Roles;

namespace TouMarmo.Modules;

// TOU's role tab has no useful word wrap on this build. Break our descriptions
// into shorter lines without changing the text used in the role wiki.
[HarmonyPatch(typeof(TownOfUs.Utilities.TouRoleUtils), nameof(TownOfUs.Utilities.TouRoleUtils.SetTabText))]
public static class MarmoTabTextPatch
{
    [HarmonyPostfix]
    public static void Postfix(ICustomRole role, ref StringBuilder __result)
    {
        if (role.GetType().Assembly != typeof(TouMarmoPlugin).Assembly || __result == null) return;
        var description = role.RoleLongDescription;
        if (string.IsNullOrWhiteSpace(description)) return;

        var wrapped = new StringBuilder();
        var column = 0;
        foreach (var word in description.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (column > 0 && column + word.Length + 1 > 60)
            {
                wrapped.Append('\n');
                column = 0;
            }
            else if (column > 0)
            {
                wrapped.Append(' ');
                column++;
            }
            wrapped.Append(word);
            column += word.Length;
        }
        __result.Replace(description, wrapped.ToString());
    }
}
