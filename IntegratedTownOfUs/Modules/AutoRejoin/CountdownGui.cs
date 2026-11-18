// Original MTOU result-screen label, GPL-3.0, 2026-11-18.
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace TownOfUs.Modules.AutoRejoin;

[RegisterInIl2Cpp]
public sealed class CountdownGui(IntPtr ptr) : MonoBehaviour(ptr)
{
    private GUIStyle? label;
    private void OnGUI()
    {
        if (!RejoinBehaviour.PendingRejoin || string.IsNullOrEmpty(RejoinBehaviour.ScreenText)) return;
        label ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 22, fontStyle = FontStyle.Bold };
        label.normal.textColor = new Color(.2f, .85f, 1);
        float width = Math.Min(720, Screen.width * .7f);
        GUI.Label(new Rect((Screen.width - width) * .5f, Screen.height - 80, width, 44), RejoinBehaviour.ScreenText, label);
    }
}
