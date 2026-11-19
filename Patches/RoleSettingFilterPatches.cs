using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Options;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TouMarmo.Patches;

/// <summary>
/// Role-chances toolbar: labelled search box + origin segregation (All Role / ToU / MToU).
///
/// Segregation modes:
///  - All Role : every role, in the original grouped order (Crewmate / Impostor / Neutral ...).
///  - ToU      : only Town of Us Mira roles, all pushed to the top.
///  - MToU     : Marmo Town of Us roles grouped first (with their own header), the rest below.
///
/// Typing in the search box moves every matching role to the very top of the list and
/// highlights it; the remaining roles stay below in their normal groups.
///
/// The toolbar is a small ScreenSpaceOverlay canvas: the game has no TMP_InputField prefab,
/// which is why the previous clone-based attempt produced no search box at all.
/// </summary>
[HarmonyPatch]
public static class RoleSettingFilterPatches
{
    private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("TOU-Marmo.RoleFilter");

    private enum RoleOriginFilter
    {
        All,
        TownOfUsMira,
        MarmoTownOfUs,
    }

    // Layout constants mirroring MiraAPI's role-tab builder (RoleSettingMenuPatches).
    private const float HeaderStartY = 0.522f;
    private const float HeaderStep = 0.422f;
    private const float RoleStep = 0.43f;
    private const float GroupGap = 0.4f;
    private const float HeaderX = -0.44f;
    private const float RoleX = -0.1f;
    private const string MarmoHeaderName = "MarmoGroupHeader";

    private static readonly Color MarmoAccent = new(0.20f, 0.58f, 0.51f, 1f);
    private static readonly Color MatchColor = new(1f, 0.93f, 0.55f, 1f);

    private static readonly Color BarColor = new(0.09f, 0.10f, 0.12f, 0.96f);
    private static readonly Color PillColor = new(0.19f, 0.21f, 0.24f, 1f);
    private static readonly Color PillSelectedColor = new(0.20f, 0.58f, 0.51f, 1f);
    private static readonly Color FieldColor = new(0.16f, 0.17f, 0.20f, 1f);
    private static readonly Color TextColor = new(0.95f, 0.96f, 0.97f, 1f);
    private static readonly Color MutedColor = new(0.62f, 0.64f, 0.68f, 1f);

    private static RoleOriginFilter _filter = RoleOriginFilter.All;
    private static RoleOriginFilter _modifierFilter = RoleOriginFilter.All;
    private static RoleOriginFilter ActiveFilter => _modifierMode ? _modifierFilter : _filter;

    // The toolbar serves two lists, so each one keeps its own query.
    private static string _roleSearch = string.Empty;
    private static string _modifierSearch = string.Empty;

    // Each tab keeps its own query and origin choice on the shared toolbar.
    private static bool _modifierMode;
    private static bool _needsRoleFilterApply;
    private static bool _applyingRoleFilter;
    private static bool _modifierSearchActive;

    private static RolesSettingsMenu? _menu;
    private static GameOptionsMenu? _optionsMenu;
    private static GameObject? _canvasGo;
    private static GameObject? _eventSystemGo;
    private static GameObject? _bar;
    private static TMP_InputField? _searchInput;
    private static TMP_InputField.OnChangeEvent? _searchChanged;
    private static TextMeshProUGUI? _searchText;
    private static TextMeshProUGUI? _searchPlaceholder;
    private static TextMeshProUGUI? _status;
    private static TextMeshProUGUI? _filterLabel;
    private static TextMeshProUGUI? _searchLabel;

    private static int _lastContainerId;
    private static int _lastChildCount = -1;

    private static readonly List<(RoleOriginFilter Value, Image Background, TextMeshProUGUI Label)> FilterPills = [];
    private static readonly Dictionary<int, Color> OriginalTitleColors = [];

    // Options whose Visible() we wrapped once, keyed by their MiraAPI id.
    private static readonly HashSet<uint> WrappedModifierOptions = [];

    // Header is internal on AbstractOptionGroup, so it is read through reflection (cached).
    private static System.Reflection.PropertyInfo? _groupHeaderProperty;

    private sealed class RoleEntry
    {
        public RoleOptionSetting Setting = null!;
        public CategoryHeaderMasked? Header;
        public string Name = string.Empty;
        public bool Marmo;
        public bool Tou;
        public int Order;
        public bool Match;
    }

    // ------------------------------------------------------------------ harmony hooks

    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(RolesSettingsMenu), nameof(RolesSettingsMenu.OpenChancesTab))]
    public static void OpenChancesTabPrefix(RolesSettingsMenu __instance)
    {
        // MiraAPI rebuilds the tab by destroying the role rows it finds **active**, and our
        // filter hides the rows it excludes. Restore them first (our prefix runs before
        // MiraAPI's) so a rebuild never leaves orphaned rows behind.
        try
        {
            if (!__instance || !__instance.RoleChancesSettings) return;

            foreach (var setting in __instance.RoleChancesSettings.GetComponentsInChildren<RoleOptionSetting>(true))
            {
                if (setting == null || !setting) continue;
                if (!setting.gameObject.activeSelf) setting.gameObject.SetActive(true);
            }
        }
        catch
        {
            /* rows already gone */
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(RolesSettingsMenu), nameof(RolesSettingsMenu.OpenChancesTab))]
    public static void OpenChancesTabPostfix(RolesSettingsMenu __instance)
    {
        _menu = __instance;
        _needsRoleFilterApply = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(RolesSettingsMenu), nameof(RolesSettingsMenu.OpenMenu))]
    public static void OpenMenuPostfix(RolesSettingsMenu __instance)
    {
        _menu = __instance;
        _needsRoleFilterApply = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(RolesSettingsMenu), nameof(RolesSettingsMenu.Update))]
    public static void UpdatePostfix(RolesSettingsMenu __instance)
    {
        try
        {
            if (!__instance) return;

            if (!ShouldShow(__instance))
            {
                // Force leave modifier mode when not in modifiers tab
                if (MenuState.Instance?.CurrentMenu != MenuCategory.Modifiers && _modifierMode)
                {
                    LeaveModifierMode();
                }
                if (MenuState.Instance?.CurrentMenu != MenuCategory.Modifiers) HideBar();
                return;
            }

            _menu = __instance;
            // Leaving the modifiers tab: restore the role pills and swap the query back.
            if (_modifierMode)
            {
                LeaveModifierMode();
                _needsRoleFilterApply = true;
            }
            // One stable parent for the whole settings menu, so switching tabs never
            // destroys/rebuilds the toolbar (that churn broke input and duplicated bars).
            EnsureBar(StableParent() ?? __instance.transform);
            if (_bar != null && _bar && !_bar.activeSelf) _bar.SetActive(true);
            // Don't sync input text if user is actively typing
            if (_searchInput == null || !_searchInput || !_searchInput.isFocused)
            {
                SyncInputText();
            }

            // MiraAPI builds a mod tab's role list over several frames, so re-apply whenever the
            // container changed (rebuild finished, a group was collapsed, another tab opened).
            var container = __instance.RoleChancesSettings.transform;
            var containerId = container != null ? container.GetInstanceID() : 0;
            var childCount = container != null ? container.childCount : 0;

            if (_needsRoleFilterApply || containerId != _lastContainerId || childCount != _lastChildCount)
            {
                _needsRoleFilterApply = false;
                _lastContainerId = containerId;
                _lastChildCount = childCount;
                ApplyFilter(__instance);
            }
        }
        catch (Exception e)
        {
            Log.LogWarning($"Role filter visibility update failed: {e}");
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
    public static void OptionsUpdatePostfix(GameOptionsMenu __instance)
    {
        try
        {
            if (!__instance) return;

            var state = MenuState.Instance;
            if (state == null)
            {
                HideBar();
                return;
            }

            if (state.CurrentMenu != MenuCategory.Modifiers || state.CurrentModIdx == 0)
            {
                if (_modifierMode) LeaveModifierMode();
                if (state.CurrentMenu != MenuCategory.Roles) HideBar();
                return;
            }

            _optionsMenu = __instance;
            EnterModifierMode();
            EnsureBar(StableParent() ?? __instance.transform);
            if (_bar != null && _bar && !_bar.activeSelf) _bar.SetActive(true);

            UpdateModifierFilter();
        }
        catch (Exception e)
        {
            Log.LogWarning($"Modifier filter update failed: {e}");
        }
    }

    // ------------------------------------------------------------------ modifier mode

    private static void EnterModifierMode()
    {
        if (!_modifierMode)
        {
            _modifierMode = true;
            SetPillsVisible(true);
            UpdatePillVisuals();
            SyncInputText();
        }
    }

    private static void LeaveModifierMode()
    {
        _modifierMode = false;
        _modifierSearchActive = false;
        SetPillsVisible(true);
        UpdatePillVisuals();
        SyncInputText();
    }

    private static void SetPillsVisible(bool visible)
    {
        try
        {
            if (_filterLabel != null && _filterLabel) _filterLabel.gameObject.SetActive(visible);

            foreach (var (_, background, _) in FilterPills)
            {
                if (background == null || !background) continue;
                if (background.gameObject.activeSelf != visible) background.gameObject.SetActive(visible);
            }
        }
        catch
        {
            /* bar in teardown */
        }
    }

    /// <summary>
    /// Makes the search box show the query that belongs to the list currently on screen, so a
    /// role query and a modifier query never overwrite each other when switching tabs.
    /// </summary>
    private static void SyncInputText()
    {
        try
        {
            if (_searchInput == null || !_searchInput) return;

            var value = _modifierMode ? _modifierSearch : _roleSearch;
            SetInputTextSilently(value);

            if (_searchPlaceholder != null && _searchPlaceholder)
            {
                _searchPlaceholder.text = _modifierMode
                    ? "Search modifiers..."
                    : "Search roles...";
            }

            if (_searchLabel != null && _searchLabel)
            {
                _searchLabel.text = _modifierMode ? "MODIFIERS:" : "ROLES:";
            }
        }
        catch
        {
            /* input already gone */
        }
    }

    /// <summary>
    /// Rewrites the search box without firing its change event (mode switches would otherwise feed
    /// the other list's query into the wrong field).
    /// </summary>
    private static void SetInputTextSilently(string? value)
    {
        try
        {
            if (_searchInput == null || !_searchInput) return;
            if (_searchInput.text == value) return;

            _searchInput.SetTextWithoutNotify(value ?? string.Empty);
        }
        catch
        {
            /* input already gone */
        }
    }

    private static void UpdateModifierFilter()
    {
        var state = MenuState.Instance;
        if (state == null || state.CurrentModIdx == 0)
        {
            _modifierSearchActive = false;
            return;
        }

        var groups = GetModifierGroups(state);
        var query = (_modifierSearch ?? string.Empty).Trim();
        var hasQuery = query.Length > 0;
        _modifierSearchActive = hasQuery || _modifierFilter != RoleOriginFilter.All;

        var total = 0;
        var shown = 0;

        foreach (var group in groups)
        {
            foreach (var option in group.Children)
            {
                total++;
                EnsureModifierWrapper(option);
                if (ModifierPassesSearch(option)) shown++;
            }
        }

        UpdateStatus(shown, total, "MOD");
    }

    private static List<AbstractOptionGroup> GetModifierGroups(MenuState state)
    {
        try
        {
            var mod = state.CurrentMod;
            if (mod == null) return [];

            return mod.OptionGroups
                .Where(x => x.ParentMenu is MenuCategory.Modifiers ||
                            x.OptionableType?.IsAssignableTo(typeof(BaseModifier)) == true)
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    /// <summary>
    /// Wraps <see cref="IModdedOption.Visible"/> once so MiraAPI's own layout hides modifiers that
    /// do not match the query - this keeps every group segregated exactly as MiraAPI built it.
    /// </summary>
    private static void EnsureModifierWrapper(IModdedOption option)
    {
        if (option == null) return;

        var original = option.Visible;
        if (original == null) return;
        if (!WrappedModifierOptions.Add(option.Id)) return;

        option.Visible = () => original() && ModifierPassesSearch(option);
    }

    private static bool ModifierPassesSearch(IModdedOption option)
    {
        if (!_modifierSearchActive) return true;
        if (!ModifierPassesOrigin(option.ParentGroup)) return false;

        var query = (_modifierSearch ?? string.Empty).Trim();
        if (query.Length == 0) return true;

        try
        {
            var title = option.Title ?? string.Empty;
            if (title.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) return true;

            var groupKey = option.ParentGroup?.GroupName ?? string.Empty;
            return groupKey.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }
        catch
        {
            return true;
        }
    }

    private static bool ModifierPassesOrigin(AbstractOptionGroup? group)
    {
        if (_modifierFilter == RoleOriginFilter.All) return true;
        if (group == null) return false;
        // Per-modifier generated groups point to the modifier type; category/chance
        // groups have no OptionableType, so their own declared type identifies origin.
        var ns = (group.OptionableType ?? group.GetType()).Namespace ?? string.Empty;
        return _modifierFilter == RoleOriginFilter.MarmoTownOfUs
            ? ns.StartsWith("TouMarmo", StringComparison.Ordinal)
            : ns.StartsWith("TownOfUs", StringComparison.Ordinal);
    }

    /// <summary>
    /// Patched manually onto MiraAPI's internal option-group layout (see TouMarmoPlugin): a group
    /// without a single hit is skipped completely, so the search does not leave empty headers behind.
    /// </summary>
    public static bool ModifierGroupPrefix(AbstractOptionGroup? group, ref float num)
    {
        if (!_modifierSearchActive || group == null) return true;

        try
        {
            if (MenuState.Instance?.CurrentMenu != MenuCategory.Modifiers) return true;

            var anyVisible = false;
            foreach (var option in group.Children)
            {
                try
                {
                    if (!option.Visible()) continue;
                    anyVisible = true;
                    break;
                }
                catch
                {
                    /* broken visibility lambda */
                }
            }

            if (anyVisible) return true;

            var header = GetGroupHeader(group);
            if (header != null && header) header.gameObject.SetActive(false);

            foreach (var option in group.Children)
            {
                var behaviour = option.OptionBehaviour;
                if (behaviour != null) behaviour.gameObject.SetActive(false);
            }

            return false;
        }
        catch
        {
            return true;
        }
    }

    private static CategoryHeaderMasked? GetGroupHeader(AbstractOptionGroup group)
    {
        try
        {
            if (_groupHeaderProperty == null)
            {
                _groupHeaderProperty = AccessTools.Property(typeof(AbstractOptionGroup), "Header");
                if (_groupHeaderProperty == null) return null;
            }

            return (CategoryHeaderMasked?)_groupHeaderProperty.GetValue(group);
        }
        catch
        {
            return null;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.ChangeTab))]
    public static void GameSettingMenuChangeTabPostfix(bool previewOnly)
    {
        // Hover previews must not change the toolbar; only a real tab switch does.
        if (previewOnly) return;

        try
        {
            var state = MenuState.Instance;
            var wantsBar = state != null &&
                           (state.CurrentMenu == MenuCategory.Roles ||
                            (state.CurrentMenu == MenuCategory.Modifiers && state.CurrentModIdx != 0));
            // The active tab's own Update re-shows and re-modes the toolbar next frame.
            if (wantsBar) return;
        }
        catch
        {
            /* fall through to hiding */
        }

        // Leaving the toolbar tabs entirely (e.g. Modifiers -> Game). Restore the origin pills
        // before hiding, otherwise coming back to Roles would show a toolbar without its filters.
        if (_modifierMode) LeaveModifierMode();
        _modifierSearchActive = false;
        HideBar();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Close))]
    public static void GameSettingMenuClosePostfix()
    {
        try
        {
            if (_searchInput != null && _searchInput && _searchInput.isFocused) _searchInput.DeactivateInputField();
        }
        catch { /* input already gone */ }

        // The canvas lives under GameSettingMenu, which survives a close, so destroy it
        // explicitly to leave nothing behind in the lobby.
        DestroyBar();
        _needsRoleFilterApply = false;
    }

    private static void HideBar()
    {
        try
        {
            if (_searchInput != null && _searchInput && _searchInput.isFocused) _searchInput.DeactivateInputField();
        }
        catch { /* input already gone */ }

        try
        {
            if (_bar != null && _bar && _bar.activeSelf) _bar.SetActive(false);
        }
        catch { /* bar already destroyed */ }
    }

    private static void DestroyBar()
    {
        try
        {
            if (_canvasGo != null && _canvasGo) Object.Destroy(_canvasGo);
            if (_eventSystemGo != null && _eventSystemGo) Object.Destroy(_eventSystemGo);
            _eventSystemGo = null;
        }
        catch { /* already destroyed */ }

        _modifierMode = false;
        _modifierSearchActive = false;
        _optionsMenu = null;

        ResetBarState();
        OriginalTitleColors.Clear();
        _menu = null;
    }

    private static void ResetBarState()
    {
        _canvasGo = null;
        _bar = null;
        _searchInput = null;
        _searchChanged = null;
        _searchText = null;
        _searchPlaceholder = null;
        _status = null;
        _filterLabel = null;
        _searchLabel = null;
        _lastContainerId = 0;
        _lastChildCount = -1;
        FilterPills.Clear();
    }

    private static bool ShouldShow(RolesSettingsMenu menu)
    {
        try
        {
            if (!menu || !menu.RoleChancesSettings) return false;
            if (!menu.RoleChancesSettings.activeInHierarchy) return false;

            var state = MenuState.Instance;
            if (state == null) return false;
            if (state.CurrentMenu != MenuCategory.Roles) return false;

            // The vanilla "All Role" tab (mod index 0) is built by the game itself: its rows sit
            // in a different layout and contain vanilla roles only. Filtering there would simply
            // wipe the tab, so the toolbar is shown on the MiraAPI mod tab only.
            return CanReorder();
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Reordering only makes sense in a MiraAPI mod tab, where every role is a full-width
    /// row laid out with ScrollerNum. The vanilla "All Roles" tab uses its own two-column
    /// grid, so nothing is filtered or moved there.
    /// </summary>
    private static bool CanReorder()
    {
        try
        {
            var state = MenuState.Instance;
            return state != null && state.CurrentModIdx != 0;
        }
        catch
        {
            return false;
        }
    }

    // ------------------------------------------------------------------ toolbar

    /// <summary>
    /// The one parent the toolbar is built under for the whole settings menu. Using the
    /// top-level <see cref="GameSettingMenu"/> (instead of the per-tab containers) means a
    /// Roles &lt;-&gt; Modifiers switch never reparents/destroys the toolbar, which is what made
    /// the search box vanish, lose focus and duplicate before.
    /// </summary>
    private static Transform? StableParent()
    {
        try
        {
            var gsm = GameSettingMenu.Instance;
            if (gsm != null && gsm) return gsm.transform;
        }
        catch
        {
            /* menu not ready yet */
        }

        return null;
    }

    private static void EnsureBar(Transform? parent = null)
    {
        // If the bar exists but its parent changed (e.g. a brand new settings menu), destroy
        // and recreate it under the new parent. Under the stable parent this never happens
        // during ordinary tab switches.
        if (_canvasGo != null && _canvasGo)
        {
            try
            {
                if (parent != null && _canvasGo.transform.parent != parent)
                {
                    DestroyBar();
                }
            }
            catch
            {
                // Bar was destroyed - reset references
                ResetBarState();
            }
        }

        if (BarAlive()) return;

        // A stale reference (for example a canvas destroyed during a scene change) must not
        // block rebuilding the toolbar.
        ResetBarState();

        try
        {
            BuildBar(parent);
        }
        catch (Exception e)
        {
            Log.LogWarning($"Could not build the role filter toolbar: {e}");
            ResetBarState();
        }
    }

    private static bool BarAlive()
    {
        try
        {
            return _canvasGo != null && _canvasGo && _bar != null && _bar && _bar.transform != null;
        }
        catch
        {
            return false;
        }
    }

    private static void EnsureEventSystem()
    {
        try
        {
            if (EventSystem.current != null) return;
            if (_eventSystemGo != null && _eventSystemGo) return;

            _eventSystemGo = new GameObject("MarmoTouEventSystem");
            _eventSystemGo.AddComponent<EventSystem>();
            _eventSystemGo.AddComponent<StandaloneInputModule>();
        }
        catch
        {
            /* keep relying on the game's EventSystem */
        }
    }

    private static void BuildBar(Transform? parent)
    {
        FilterPills.Clear();
        _canvasGo = new GameObject("MarmoTouRoleFilterCanvas");
        if (parent != null)
        {
            _canvasGo.transform.SetParent(parent, false);
        }
        var canvas = _canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30500;
        var scaler = _canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        _canvasGo.AddComponent<GraphicRaycaster>();

        // The game always has its own EventSystem while the settings menu is open. Only if it
        // is missing do we add one - as a ROOT object, so a canvas rebuild can never take the
        // EventSystem down with it (that race is what made the search box unclickable).
        EnsureEventSystem();

        var barImage = CreateImage(_canvasGo.transform, "MarmoRoleFilterBar", BarColor);
        _bar = barImage.gameObject;
        var barRect = barImage.rectTransform;
        barRect.anchorMin = barRect.anchorMax = new Vector2(0.5f, 0f);
        barRect.pivot = new Vector2(0.5f, 0.5f);
        barRect.anchoredPosition = new Vector2(0f, 34f);
        barRect.sizeDelta = new Vector2(1360f, 64f);

        var outline = _bar.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.18f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        var searchLabel = CreateLabel(barRect, "SearchLabel", "ROLES:", 21f, TextColor,
            FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
        SetRect(searchLabel.rectTransform, new Vector2(-520f, 0f), new Vector2(250f, 44f));
        _searchLabel = searchLabel;

        CreateSearchField(barRect, new Vector2(-200f, 0f));
        CreateClearButton(barRect, new Vector2(55f, 0f));

        var filterLabel = CreateLabel(barRect, "FilterLabel", "FILTER:", 21f, TextColor,
            FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
        SetRect(filterLabel.rectTransform, new Vector2(175f, 0f), new Vector2(70f, 44f));
        _filterLabel = filterLabel;

        CreateFilterPill(barRect, RoleOriginFilter.All, "All", 280f, 120f);
        CreateFilterPill(barRect, RoleOriginFilter.TownOfUsMira, "ToU", 385f, 90f);
        CreateFilterPill(barRect, RoleOriginFilter.MarmoTownOfUs, "MToU", 490f, 100f);

        _status = CreateLabel(barRect, "Status", string.Empty, 19f, MutedColor,
            FontStyles.Normal, TextAlignmentOptions.MidlineRight);
        SetRect(_status.rectTransform, new Vector2(610f, 0f), new Vector2(130f, 44f));

        UpdatePillVisuals();
        SetPillsVisible(true);
        SyncInputText();
        Log.LogInfo("Role filter toolbar created (labelled search + All Role/ToU/MToU segregation).");
    }

    private static void CreateFilterPill(RectTransform parent, RoleOriginFilter value, string label, float x, float width)
    {
        var image = CreateImage(parent, $"Filter_{value}", ActiveFilter == value ? PillSelectedColor : PillColor);
        var rect = image.rectTransform;
        SetRect(rect, new Vector2(x, 0f), new Vector2(width, 44f));

        var button = image.gameObject.AddComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener((UnityAction)(() =>
        {
            if (_modifierMode) _modifierFilter = value;
            else _filter = value;
            UpdatePillVisuals();
            ApplySearchChange();
        }));

        var text = CreateLabel(rect, "Label", label, 20f, TextColor, FontStyles.Bold, TextAlignmentOptions.Center);
        Stretch(text.rectTransform);

        FilterPills.Add((value, image, text));
    }

    private static void CreateSearchField(RectTransform parent, Vector2 position)
    {
        var fieldImage = CreateImage(parent, "SearchField", FieldColor);
        var fieldRect = fieldImage.rectTransform;
        SetRect(fieldRect, position, new Vector2(360f, 46f));

        var outline = fieldImage.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.25f);
        outline.effectDistance = new Vector2(1f, -1f);

        var viewportGo = new GameObject("Text Area");
        var viewportRect = viewportGo.AddComponent<RectTransform>();
        viewportRect.SetParent(fieldRect, false);
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(14f, 3f);
        viewportRect.offsetMax = new Vector2(-14f, -3f);
        viewportGo.AddComponent<RectMask2D>();

        _searchText = CreateLabel(viewportRect, "Text", string.Empty, 22f, TextColor, FontStyles.Normal,
            TextAlignmentOptions.MidlineLeft);
        _searchText.overflowMode = TextOverflowModes.Overflow;
        _searchText.enableWordWrapping = false;
        _searchText.richText = false;
        _searchText.enableAutoSizing = false;
        _searchText.fontSizeMin = 22f;
        _searchText.fontSizeMax = 22f;
        SetTextRect(_searchText.rectTransform);

        _searchPlaceholder = CreateLabel(viewportRect, "Placeholder", "Search roles...", 22f, MutedColor,
            FontStyles.Italic, TextAlignmentOptions.MidlineLeft);
        _searchPlaceholder.overflowMode = TextOverflowModes.Overflow;
        _searchPlaceholder.enableWordWrapping = false;
        _searchPlaceholder.enableAutoSizing = false;
        SetTextRect(_searchPlaceholder.rectTransform);

        // Deactivate before adding TMP_InputField: its OnEnable builds the caret and needs
        // textComponent/textViewport already assigned.
        fieldImage.gameObject.SetActive(false);
        var input = fieldImage.gameObject.AddComponent<TMP_InputField>();
        input.textViewport = viewportRect;
        input.textComponent = _searchText;
        input.placeholder = _searchPlaceholder;
        input.lineType = TMP_InputField.LineType.SingleLine;
        input.characterLimit = 32;
        input.richText = false;
        input.customCaretColor = true;
        input.caretColor = Color.white;
        input.selectionColor = new Color(0.35f, 0.75f, 0.68f, 0.5f);
        input.caretWidth = 2;
        input.restoreOriginalTextOnEscape = false;
        input.transition = Selectable.Transition.None;
        input.interactable = true;
        input.readOnly = false;
        input.shouldHideMobileInput = false;
        input.onFocusSelectAll = true;
        input.text = _modifierMode ? _modifierSearch : _roleSearch;

        _searchChanged = new TMP_InputField.OnChangeEvent();
        _searchChanged.AddListener((UnityAction<string>)(value =>
        {
            if (_modifierMode) _modifierSearch = value ?? string.Empty;
            else _roleSearch = value ?? string.Empty;

            ApplySearchChange();
        }));

        input.onValueChanged = _searchChanged;

        _searchInput = input;
        fieldImage.gameObject.SetActive(true);
    }

    private static void CreateClearButton(RectTransform parent, Vector2 position)
    {
        var image = CreateImage(parent, "ClearFilters", PillColor);
        var rect = image.rectTransform;
        SetRect(rect, position, new Vector2(110f, 44f));

        var button = image.gameObject.AddComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener((UnityAction)(() =>
        {
            if (_modifierMode) { _modifierFilter = RoleOriginFilter.All; _modifierSearch = string.Empty; }
            else { _filter = RoleOriginFilter.All; _roleSearch = string.Empty; }

            SetInputTextSilently(string.Empty);

            UpdatePillVisuals();
            ApplySearchChange();
        }));

        var text = CreateLabel(rect, "Label", "Clear", 19f, TextColor, FontStyles.Bold, TextAlignmentOptions.Center);
        Stretch(text.rectTransform);
    }

    private static void UpdatePillVisuals()
    {
        foreach (var (value, background, label) in FilterPills)
        {
            if (background == null || !background) continue;
            background.color = ActiveFilter == value ? PillSelectedColor : PillColor;
            if (label != null && label) label.color = ActiveFilter == value ? Color.white : MutedColor;
        }
    }

    /// <summary>
    /// Called whenever the search box or a pill changes: routes the change to whichever list
    /// currently owns the toolbar (roles or modifiers) and scrolls that list to the top.
    /// </summary>
    private static void ApplySearchChange()
    {
        if (_modifierMode)
        {
            UpdateModifierFilter();

            try
            {
                if (_optionsMenu != null && _optionsMenu && _optionsMenu.scrollBar != null)
                    _optionsMenu.scrollBar.ScrollToTop();
            }
            catch { /* scroller unavailable */ }

            return;
        }

        if (_menu != null && _menu)
        {
            _needsRoleFilterApply = true;
            if (_menu.scrollBar) _menu.scrollBar.ScrollToTop();
        }
    }

    // ------------------------------------------------------------------ filtering / layout

    private static void ApplyFilter(RolesSettingsMenu menu)
    {
        if (_applyingRoleFilter) return;
        _applyingRoleFilter = true;
        try
        {
            if (!menu || !menu.RoleChancesSettings) return;

            var container = menu.RoleChancesSettings.transform;
            if (container == null) return;

            var entries = CollectEntries(container);
            if (entries.Count == 0) return;

            if (!CanReorder()) return;

            var search = (_roleSearch ?? string.Empty).Trim();
            var hasSearch = search.Length > 0;

            foreach (var entry in entries)
            {
                entry.Match = hasSearch && entry.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
                RestoreTitleColor(entry.Setting);
            }

            var filtered = entries.Where(PassesOrigin).ToList();

            var rows = new List<object>();

            // Search results retain their original faction/alignment category headers.
            rows.AddRange(OrderGrouped(filtered.Where(x => !hasSearch || x.Match).ToList(), container));

            var visibleIds = rows.OfType<Component>().Select(row => row.gameObject.GetInstanceID()).ToHashSet();
            foreach (var entry in entries)
                SetRowVisible(entry.Setting.gameObject, visibleIds.Contains(entry.Setting.gameObject.GetInstanceID()));
            foreach (var header in container.GetComponentsInChildren<CategoryHeaderMasked>(true))
                SetRowVisible(header.gameObject, visibleIds.Contains(header.gameObject.GetInstanceID()));

            Layout(rows);

            var shown = rows.Count(row => row is RoleOptionSetting);
            ApplyHighlights(filtered.Where(x => x.Match || !hasSearch).ToList());
            UpdateStatus(shown, entries.Count, "ROLE");
        }
        catch (Exception e)
        {
            Log.LogWarning($"Role filter apply failed: {e}");
        }
        finally
        {
            _applyingRoleFilter = false;
        }
    }

    private static void SetRowVisible(GameObject row, bool visible)
    {
        if (row && row.activeSelf != visible) row.SetActive(visible);
    }

    /// <summary>
    /// Builds the ordered row sequence (headers + roles) for the active origin filter.
    /// Roles are always grouped by their original MiraAPI category headers (Crewmate Killing,
    /// Impostor Power, Neutral Evil, ...). In MToU mode the categories that contain at least
    /// one Marmo role are moved to the very top, and inside each category Marmo roles come
    /// first - but the category headers themselves are preserved.
    /// </summary>
    private static List<object> OrderGrouped(List<RoleEntry> filtered, Transform container)
    {
        var rows = new List<object>();
        if (filtered.Count == 0) return rows;

        var remaining = new List<RoleEntry>(filtered);

        // Group by the original category header, preserving MiraAPI's creation order.
        var categories = remaining
            .Where(x => x.Header != null)
            .Select(x => x.Header!)
            .GroupBy(h => h.GetInstanceID())
            .Select(g => g.First())
            .OrderBy(h => h.transform.GetSiblingIndex())
            .ToList();

        // In MToU mode, categories that contain at least one Marmo role go first.
        if (_filter == RoleOriginFilter.MarmoTownOfUs)
        {
            var marmoCategories = categories.Where(c => remaining.Any(x => x.Header != null && x.Header.GetInstanceID() == c.GetInstanceID() && x.Marmo)).ToList();
            var otherCategories = categories.Where(c => !marmoCategories.Contains(c)).ToList();
            categories = marmoCategories.Concat(otherCategories).ToList();
        }

        foreach (var header in categories)
        {
            var roles = remaining
                .Where(x => x.Header != null && x.Header.GetInstanceID() == header.GetInstanceID())
                .OrderBy(x => _filter == RoleOriginFilter.MarmoTownOfUs ? (x.Marmo ? 0 : 1) : 0)
                .ThenBy(x => x.Order)
                .ToList();

            if (roles.Count == 0) continue;

            rows.Add(header);
            foreach (var role in roles) rows.Add(role.Setting);
        }

        // Roles without any owning header (unexpected) stay visible at the end.
        foreach (var entry in remaining.Where(x => x.Header == null).OrderBy(x => x.Order))
        {
            rows.Add(entry.Setting);
        }

        return rows;
    }

    private static List<RoleEntry> CollectEntries(Transform container)
    {
        var entries = new List<RoleEntry>();
        CategoryHeaderMasked? currentHeader = null;
        var order = 0;

        for (var i = 0; i < container.childCount; i++)
        {
            var child = container.GetChild(i);
            if (child == null) continue;
            if (child.name.StartsWith(MarmoHeaderName, StringComparison.Ordinal)) continue;

            var header = child.GetComponent<CategoryHeaderMasked>();
            if (header != null)
            {
                currentHeader = header;
                continue;
            }

            var setting = child.GetComponent<RoleOptionSetting>();
            if (setting == null) continue;

            var role = TryGetRole(setting);
            entries.Add(new RoleEntry
            {
                Setting = setting,
                Header = currentHeader,
                Name = SafeRoleName(role, setting),
                Order = order++,
                Marmo = role != null && IsMarmoRole(role),
                Tou = role != null && IsTownOfUsMiraRole(role),
            });
        }

        return entries;
    }

    private static void Layout(List<object> rows)
    {
        var y = HeaderStartY;
        var firstRow = true;

        foreach (var row in rows)
        {
            if (row is CategoryHeaderMasked header)
            {
                if (!firstRow) y -= GroupGap;

                if (header.name.StartsWith(MarmoHeaderName, StringComparison.Ordinal)) StyleMarmoHeader(header);
                header.transform.localPosition = new Vector3(HeaderX, y, -2f);
                y -= HeaderStep;
            }
            else if (row is RoleOptionSetting setting)
            {
                setting.transform.localPosition = new Vector3(RoleX, y, -2f);
                y -= RoleStep;
            }

            firstRow = false;
        }
    }

    private static void StyleMarmoHeader(CategoryHeaderMasked header)
    {
        try
        {
            // Keep the label stable even if the cloned header tries to re-translate its title.
            if (header.Title != null)
            {
                header.Title.text = "Marmo Town of Us";
                header.Title.color = Color.white;
            }

            if (header.Background != null) header.Background.color = MarmoAccent;
            if (header.Divider != null) header.Divider.color = MarmoAccent;
        }
        catch
        {
            /* header already in teardown */
        }
    }

    /// <summary>
    /// Returns our extra section header, cloning an existing group header the first time.
    /// Lookup is by name, so a rebuilt role list can never leave a destroyed reference behind.
    /// </summary>
    private static CategoryHeaderMasked? EnsureMarmoHeader(Transform container)
    {
        for (var i = 0; i < container.childCount; i++)
        {
            var child = container.GetChild(i);
            if (child == null) continue;
            if (!child.name.StartsWith(MarmoHeaderName, StringComparison.Ordinal)) continue;

            var existing = child.GetComponent<CategoryHeaderMasked>();
            if (existing != null) return existing;
        }

        try
        {
            CategoryHeaderMasked? template = null;
            for (var i = 0; i < container.childCount && template == null; i++)
            {
                var child = container.GetChild(i);
                if (child == null) continue;
                if (child.name.StartsWith(MarmoHeaderName, StringComparison.Ordinal)) continue;
                template = child.GetComponent<CategoryHeaderMasked>();
            }

            if (template == null) return null;

            var go = Object.Instantiate(template.gameObject, container);
            go.name = MarmoHeaderName;

            var header = go.GetComponent<CategoryHeaderMasked>();
            if (header == null)
            {
                Object.Destroy(go);
                return null;
            }

            // Strip interactivity: clicking one of these headers rebuilds the whole role list.
            foreach (var component in go.GetComponents<PassiveButton>()) Object.Destroy(component);
            foreach (var component in go.GetComponents<BoxCollider2D>()) Object.Destroy(component);

            // Keep the three visuals the header needs (they may be nested), hide anything else -
            // most notably the cloned "Click to close" hint.
            var keep = new List<Transform>(3);
            if (header.Title != null) keep.Add(header.Title.transform);
            if (header.Background != null) keep.Add(header.Background.transform);
            if (header.Divider != null) keep.Add(header.Divider.transform);

            for (var i = go.transform.childCount - 1; i >= 0; i--)
            {
                var child = go.transform.GetChild(i);
                if (child == null) continue;

                var needed = keep.Any(t => t == child || (t != null && t.IsChildOf(child)));
                if (!needed) child.gameObject.SetActive(false);
            }

            if (header.Title != null)
            {
                var translator = header.Title.GetComponent<TextTranslatorTMP>();
                if (translator != null) Object.Destroy(translator);
            }

            go.transform.localScale = Vector3.one * 0.63f;
            go.SetActive(false);
            StyleMarmoHeader(header);

            Log.LogInfo("Marmo section header created for the MToU view.");
            return header;
        }
        catch (Exception e)
        {
            Log.LogWarning($"Could not create the Marmo section header: {e}");
            return null;
        }
    }

    private static void ApplyHighlights(List<RoleEntry> entries)
    {
        foreach (var entry in entries)
        {
            var setting = entry.Setting;
            if (setting == null || !setting) continue;
            if (!setting.gameObject.activeSelf) continue;

            var title = setting.titleText;
            if (title == null) continue;

            var id = setting.GetInstanceID();
            if (!OriginalTitleColors.ContainsKey(id)) OriginalTitleColors[id] = title.color;

            title.color = entry.Match ? MatchColor : OriginalTitleColors[id];
        }
    }

    private static void RestoreTitleColor(RoleOptionSetting setting)
    {
        try
        {
            if (setting == null || !setting || setting.titleText == null) return;
            var id = setting.GetInstanceID();
            if (OriginalTitleColors.TryGetValue(id, out var color) && setting.titleText.color != color)
            {
                setting.titleText.color = color;
            }
        }
        catch { /* setting destroyed */ }
    }

    private static void UpdateStatus(int shown, int total, string prefix)
    {
        // Don't show the count (user request: no role/modifier counts during search).
        if (_status == null || !_status) return;
        if (_status.text != string.Empty) _status.text = string.Empty;
    }

    // ------------------------------------------------------------------ origin helpers

    private static bool PassesOrigin(RoleEntry entry)
    {
        return _filter switch
        {
            RoleOriginFilter.TownOfUsMira => entry.Tou,
            RoleOriginFilter.MarmoTownOfUs => entry.Marmo,
            _ => true,
        };
    }

    private static bool IsMarmoRole(RoleBehaviour role)
    {
        try
        {
            var ns = role.GetType().Namespace ?? string.Empty;
            return ns.StartsWith("TouMarmo", StringComparison.Ordinal);
        }
        catch
        {
            return false;
        }
    }

    private static bool IsTownOfUsMiraRole(RoleBehaviour role)
    {
        try
        {
            var ns = role.GetType().Namespace ?? string.Empty;
            return ns.StartsWith("TownOfUs", StringComparison.Ordinal);
        }
        catch
        {
            return false;
        }
    }

    private static RoleBehaviour? TryGetRole(RoleOptionSetting setting)
    {
        try { return setting.Role; } catch { return null; }
    }

    private static string SafeRoleName(RoleBehaviour? role, RoleOptionSetting setting)
    {
        try
        {
            var title = setting.titleText != null ? setting.titleText.text : null;
            if (!string.IsNullOrWhiteSpace(title)) return title!.Trim();

            if (role != null)
            {
                var name = role.GetRoleName();
                if (!string.IsNullOrWhiteSpace(name)) return name.Trim();
                if (!string.IsNullOrWhiteSpace(role.NiceName)) return role.NiceName.Trim();
            }
        }
        catch { }

        return setting.name ?? string.Empty;
    }

    // ------------------------------------------------------------------ ui helpers

    private static Image CreateImage(Transform parent, string name, Color color)
    {
        var go = new GameObject(name);
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        var image = go.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private static TextMeshProUGUI CreateLabel(Transform parent, string name, string text, float size,
        Color color, FontStyles style, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name);
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        ApplyGameFont(tmp);
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static void ApplyGameFont(TMP_Text target)
    {
        try
        {
            if (_menu != null && _menu)
            {
                var source = _menu.GetComponentsInChildren<TextMeshProUGUI>(true)
                    .FirstOrDefault(x => x != null && x.font != null);
                if (source != null)
                {
                    target.font = source.font;
                    if (source.fontSharedMaterial != null) target.fontSharedMaterial = source.fontSharedMaterial;
                    return;
                }
            }

            if (TMP_Settings.defaultFontAsset != null) target.font = TMP_Settings.defaultFontAsset;
        }
        catch
        {
            /* keep TMP's default font */
        }
    }

    private static void SetRect(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void SetTextRect(RectTransform rect)
    {
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(2000f, 36f);
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
