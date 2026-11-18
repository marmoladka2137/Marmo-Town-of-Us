using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using TouMarmo.Modules.OriginalAnimations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TouMarmo.Modules.Deputy;

/// <summary>Original graphic shot cue. Does not clone or play a Mira/native kill-animation prefab.</summary>
[RegisterInIl2Cpp]
public sealed class DeputyShotEffect(IntPtr ptr) : MonoBehaviour(ptr)
{
    public const float Duration = AnimationTiming.DeputyDuration;
    private DeputyShotPresentation.Shot? shot;
    private ProceduralStage? stage;
    private Transform canvas = null!, weapon = null!, flash = null!, pulse = null!;
    private Transform actor = null!;
    private SpriteRenderer shade = null!;
    private float began;
    private bool configured, finished, sounded;
    internal byte OwnerId => shot?.Owner.PlayerId ?? byte.MaxValue;

    [HideFromIl2Cpp]
    internal void Configure(DeputyShotPresentation.Shot request)
    {
        shot = request;
        stage = new ProceduralStage(transform, 200);
        canvas = stage.Group("Original shot composition");
        shade = stage.Quad("Dark panel", Vector2.zero, new Vector2(8, 4.5f), new Color(.03f,.08f,.12f,.85f), canvas);
        shade.sortingOrder = 180;
        var stripe = stage.Quad("Gold rule", new Vector2(0,-1.65f), new Vector2(6.3f,.035f), new Color(1,.76f,.28f), canvas);
        stripe.sortingOrder = 181;

        // A live meeting icon owns loaded cosmetic handles; cloning it can share stale loading state.
        var player = Object.Instantiate(HudManager.Instance.IntroPrefab.PlayerPrefab, canvas);
        actor = player.transform; actor.name = "Native shooter idle portrait";
        actor.localPosition = new Vector3(-1.25f, -.75f, -.1f);
        actor.localRotation = Quaternion.identity; actor.localScale = Vector3.one * 1.25f;
        player.gameObject.SetActive(true);
        // The shooter is a plain native crewmate with this effect's own gold insignia.
        // No custom costume/addressable needs to finish loading before this short cue can play.
        player.SetBodyType(PlayerBodyTypes.Normal);
        player.SetBodyColor(request.DisplayKiller.DefaultOutfit.ColorId);
        player.SetHatAndVisorMaskType(PlayerMaterial.MaskType.None);
        if (player.cosmetics.skin) player.cosmetics.skin.gameObject.SetActive(false);
        if (player.cosmetics.hat) player.cosmetics.hat.gameObject.SetActive(false);
        if (player.cosmetics.visor) player.cosmetics.visor.gameObject.SetActive(false);
        player.ToggleName(false); player.TogglePet(false); player.SetFlipX(false);
        foreach (var r in player.GetComponentsInChildren<SpriteRenderer>(true)) r.sortingOrder += 185;
        foreach (var t in player.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = gameObject.layer;
        var badge = stage.Ring("Original gold insignia", .12f, .035f, new Color(1,.76f,.28f), canvas);
        badge.localPosition = new Vector3(-1.32f,-.60f,-.2f);

        weapon = stage.Group("Original raised sidearm", canvas);
        stage.Quad("Grip outline", new Vector2(-.18f,-.17f), new Vector2(.22f,.49f), new Color(.07f,.09f,.12f), weapon);
        stage.Quad("Grip", new Vector2(-.18f,-.17f), new Vector2(.16f,.43f), new Color(.25f,.21f,.17f), weapon);
        stage.Quad("Barrel outline", new Vector2(.02f,.05f), new Vector2(.79f,.25f), new Color(.07f,.09f,.12f), weapon);
        stage.Quad("Barrel", new Vector2(.02f,.05f), new Vector2(.73f,.19f), new Color(.78f,.83f,.88f), weapon);
        stage.Quad("Barrel highlight", new Vector2(.02f,.1f), new Vector2(.65f,.025f), Color.white, weapon);
        var chamber = stage.Ring("Cylinder", .07f,.025f,new Color(.28f,.34f,.4f),weapon);
        chamber.localPosition = new Vector3(-.15f,.05f,-.01f);
        stage.Quad("Sight", new Vector2(.25f,.17f), new Vector2(.07f,.08f), Color.white, weapon);
        pulse = stage.Ring("Expanding shot ring", .5f,.035f,new Color(1,.79f,.27f),canvas);
        pulse.localPosition = new Vector3(.9f,.05f,-.2f);
        flash = stage.Group("Muzzle rays", canvas);
        flash.localPosition = new Vector3(.15f,.05f,-.3f);
        for (int i=0;i<7;i++)
        {
            float angle = (i-3)*17;
            var ray = stage.Quad("Ray",new Vector2(.23f,0),new Vector2(.55f,.045f),new Color(1,.91f,.55f),flash);
            ray.transform.localEulerAngles = new Vector3(0,0,angle);
        }
        ProceduralStage.NonInteractive(gameObject);
        Fit();
        began = Time.unscaledTime; configured = true;
        Draw(0);
        DeputyShotPresentation.Log.LogDebug($"Original effect {GetInstanceID()} began, source={OwnerId}, target={request.Victim.PlayerId}");
    }

    private void Fit()
    {
        var camera = Camera.main;
        float height = camera && camera.orthographic ? camera.orthographicSize * 2 : 6;
        float aspect = camera ? camera.aspect : (float)Math.Max(1,Screen.width)/Math.Max(1,Screen.height);
        var inherited = transform.lossyScale;
        float scale = AnimationTiming.Fit(height,aspect,inherited.x,inherited.y);
        canvas.localScale = new Vector3(scale,scale,1);
    }
    private void Draw(float t)
    {
        float alpha = AnimationTiming.DeputyOpacity(t);
        ProceduralStage.Alpha(canvas,alpha);
        shade.color = new Color(.03f,.08f,.12f,alpha*.85f);
        weapon.localPosition = new Vector3(-.25f-.16f*AnimationTiming.Recoil(t),0,-.2f);
        weapon.localEulerAngles = new Vector3(0,0,-28*(1-AnimationTiming.Ramp(t,0,.26f))+12*AnimationTiming.Recoil(t));
        actor.localPosition = new Vector3(-1.25f-.07f*AnimationTiming.Recoil(t),-.75f,-.1f);
        ProceduralStage.Alpha(flash,AnimationTiming.Flash(t));
        pulse.localScale = Vector3.one*(.3f+1.4f*AnimationTiming.Ramp(t,.30f,.73f));
        ProceduralStage.Alpha(pulse,AnimationTiming.Window(t,.30f,.37f,.75f));
        if(t>=.30f&&!sounded){sounded=true;PresentationSound.Play(false);}
    }
    private void Update()
    {
        if (!configured || finished || shot == null) return;
        try
        {
            if (!shot.Overlay || !shot.Area || shot.Owner.Disconnected || shot.Owner.IsDead ||
                (shot.HadMeeting && (!shot.Meeting || shot.Meeting != MeetingHud.Instance))) { Finish(false); return; }
            float elapsed = Time.unscaledTime-began;
            if(AnimationTiming.Finished(elapsed,Duration)){Finish(true);return;}
            Fit(); Draw(elapsed);
        }
        catch(Exception error)
        { DeputyShotPresentation.Log.LogError($"Original Deputy effect aborted: {error}"); Finish(false); }
    }
    [HideFromIl2Cpp]
    internal void Finish(bool completed)
    {
        if(finished)return;
        finished=true;
        gameObject.SetActive(false);
        stage?.Dispose(); stage=null;
        Object.Destroy(gameObject);
        if(shot!=null)DeputyShotPresentation.Closed(this,shot,completed);
    }
    private void OnDisable(){if(configured&&!finished)Finish(false);}
    private void OnDestroy(){if(!finished)Finish(false);}
}
