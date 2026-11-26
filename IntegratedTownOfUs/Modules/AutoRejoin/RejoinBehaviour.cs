// Original MTOU replacement, GPL-3.0, 2026-11-18. Historical feature attribution retained in CREDITS.md.
using AmongUs.Data;
using InnerNet;
using Reactor.Utilities.Attributes;
using TouMarmo.Modules.OriginalMotion;
using UnityEngine;

namespace TownOfUs.Modules.AutoRejoin;

[RegisterInIl2Cpp]
public sealed class RejoinBehaviour(IntPtr ptr) : MonoBehaviour(ptr)
{
    public static bool PendingRejoin;
    public static int SavedGameId = -1;
    public static EndGameManager? CurrentEndGameManager;
    public static string ScreenText = "";
    public static CountdownGui? GuiObject;
    public static RejoinBehaviour? Instance;
    private readonly RejoinSchedule schedule = new();
    private double retryUntil;

    public static void TriggerRejoin()
    {
        if (!Instance)
        {
            var root = new GameObject("MTOU return-to-lobby controller");
            DontDestroyOnLoad(root);
            Instance = root.AddComponent<RejoinBehaviour>();
            GuiObject = root.AddComponent<CountdownGui>();
        }
        Instance!.StartRejoin();
    }
    public void StartRejoin()
    {
        var delaySetting = LocalSettingsTabSingleton<TouLocalTabPreferences>.Instance.AutoRejoinDelay;
        if (delaySetting.Value == 4f) delaySetting.Value = 5f; // Migrate previous default.
        schedule.Begin(Time.realtimeSinceStartupAsDouble, delaySetting.Value);
        retryUntil = Time.realtimeSinceStartupAsDouble + Math.Max(0, delaySetting.Value) + 15;
        PendingRejoin = true;
    }
    public void Cancel() { schedule.Cancel(); ScreenText = ""; }
    public static void CancelRejoin()
    {
        if (Instance) Instance!.Cancel();
        PendingRejoin = false; SavedGameId = -1; CurrentEndGameManager = null; ScreenText = "";
    }
    private void Update()
    {
        if (!PendingRejoin || !schedule.Active) return;
        double now = Time.realtimeSinceStartupAsDouble;
        if (!CurrentEndGameManager) { if(now>=retryUntil) CancelRejoin(); return; }
        int seconds = schedule.Seconds(now);
        ScreenText = "[AutoRejoin] " + MiraLocaleManager.Get("TouAutoRejoinCountdown", "Rejoining in <seconds>")
            .Replace("<seconds>", seconds.ToString(TownOfUsPlugin.Culture)) + "s";
        if (!DataManager.Settings.Gameplay.StreamerMode) ScreenText += " (" + GameCode.IntToGameName(SavedGameId) + ")";
        if (!schedule.Take(now)) return;
        ScreenText = "";
        // Resolve within the current result screen, not every button in every loaded scene.
        // Navigation is a separate scene object, not a child of EndGameManager.
        var navigation = UnityEngine.Object.FindObjectOfType<EndGameNavigation>();
        if (!navigation || !navigation.PlayAgainButton || !navigation.PlayAgainButton.gameObject.activeInHierarchy)
        {
            if (now < retryUntil) { schedule.Begin(now, .25); return; }
            Warning("[MTOU AutoRejoin] Result screen has no active Play Again button."); CancelRejoin(); return;
        }
        PendingRejoin = false;
        try { navigation!.NextGame(); }
        catch (Exception ex) { Warning("[MTOU AutoRejoin] Could not invoke Play Again: " + ex.GetType().Name); CancelRejoin(); }
    }
    private void OnDestroy() { if (Instance == this) { CancelRejoin(); Instance = null; GuiObject = null; } }
}
