using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.GameOptions;
using UnityEngine;
namespace TouMarmo.Patches;

// Retain native options/serialization: the server must accept the capacity.
[HarmonyPatch(typeof(InnerNet.InnerNetClient),nameof(InnerNet.InnerNetClient.HostGame))]
public static class ExpandedLobbyHostPatch
{
    [HarmonyPrefix] public static void Prefix(IGameOptions __0)
    {
        var count=(int)OptionGroupSingleton<ExpandedLobbyOptions>.Instance.Capacity.Value;
        if(count<=0) return;
        var normal=__0.TryCast<NormalGameOptionsV12>();
        if(normal!=null) normal.MaxPlayers=Math.Clamp(count,4,24);
        else __0.SetInt(Int32OptionNames.MaxPlayers,Math.Clamp(count,4,24));
    }
}
[HarmonyPatch(typeof(MeetingHud),nameof(MeetingHud.Start))]
public static class ExpandedMeetingLayoutPatch
{
    [HarmonyPostfix,HarmonyPriority(Priority.Last)] public static void Postfix(MeetingHud __instance)
    {
        var areas=__instance.playerStates;
        if(areas.Length<=15) return;
        var columns=areas.Take(15).Select(a=>a.transform.localPosition.x).Distinct().OrderBy(x=>x).ToArray();
        var rows=areas.Take(15).Select(a=>a.transform.localPosition.y).Distinct().OrderByDescending(y=>y).ToArray();
        if(columns.Length<2 || rows.Length<2) return;
        var needed=(areas.Length+columns.Length-1)/columns.Length;
        var scale=Mathf.Min(1f,(rows.Length-1f)/Math.Max(1,needed-1));
        var step=Mathf.Abs(rows[0]-rows[1])*scale;
        for(var i=0;i<areas.Length;i++)
        {
            var t=areas[i].transform;var pos=t.localPosition;
            t.localScale=t.localScale*scale;
            t.localPosition=new Vector3(columns[i%columns.Length],rows[0]-(i/columns.Length)*step,pos.z);
        }
    }
}

// Once the palette is exhausted, reuse a valid colour rather than letting the
// stock unique-colour search run out of free entries. Player IDs stay unique.
[HarmonyPatch(typeof(PlayerControl),nameof(PlayerControl.CheckColor))]
public static class ExpandedLobbyColorPatch
{
    [HarmonyPrefix,HarmonyPriority(Priority.First)]
    public static bool Prefix(PlayerControl __instance,byte __0)
    {
        if(!MarmoSession.IsHost || Palette.PlayerColors.Length==0 ||
            PlayerControl.AllPlayerControls.Count<Palette.PlayerColors.Length) return true;
        __instance.RpcSetColor((byte)(__0 % Palette.PlayerColors.Length));
        return false;
    }
}
