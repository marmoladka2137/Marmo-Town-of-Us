using PowerTools;
using UnityEngine;
using Object = UnityEngine.Object;
namespace TouMarmo.Modules.Necromancer;

// This object contains cosmetics only. The deceased player keeps their ghost and controls.
public sealed class NecromancerPuppet : IDisposable
{
    private readonly CosmeticsLayer cosmetics;
    private readonly SpriteAnim animator;
    private readonly GameObject bodyImage;
    private readonly AnimationClip idle,run;
    private readonly SpriteRenderer[] renderers;
    private readonly Material bodyMaterial=null!;
    private Vector2 previous;
    private readonly bool controlledHere;
    public NecromancerPuppet(NetworkedPlayerInfo.PlayerOutfit outfit, Transform body, SpriteRenderer? reference = null, bool controlledHere = false)
    {
        this.controlledHere=controlledHere;
        cosmetics = Object.Instantiate(AmongUsClient.Instance.PlayerPrefab.cosmetics, body);
        bodyImage=new GameObject("Necromancer animated body");
        var stage="body setup";
        try
        {
            bodyImage.transform.SetParent(body,false);
            bodyImage.transform.localPosition=new Vector3(0,0.2f,-0.02f);
            // Zwiększono skalę body image z 0.7f na 1.0f, żeby outfit był większy.
            bodyImage.transform.localScale=Vector3.one*1.0f;
            var image=bodyImage.AddComponent<SpriteRenderer>();
            var source=reference ?? AmongUsClient.Instance.PlayerPrefab.GetComponentsInChildren<SpriteRenderer>(true).First();
            // A prefab/ghost renderer can use a plain sprite or masked material. Use the
            // live-player shader which decodes AU's red/blue/green body color channels.
            bodyMaterial=new Material(HatManager.Instance.PlayerMaterial);
            image.sharedMaterial=bodyMaterial;
            bodyImage.layer=LayerMask.NameToLayer("Players");
            image.sortingOrder=source.sortingOrder;
            cosmetics.currentBodySprite=new PlayerBodySprite
            {
                BodySprite=image,
                Type=PlayerBodyTypes.Classic,
                LongModeParts=new SpriteRenderer[0]
            };
            cosmetics.hat.Parent=image;
            cosmetics.hat.transform.localPosition=new Vector2(-0.045f,0.575f);
            cosmetics.visor.transform.localPosition=new Vector2(-0.045f,0.575f);
            // Zwiększono skalę cosmetics z 0.35f na 0.5f, żeby outfit był czytelniejszy.
            cosmetics.transform.localScale=Vector3.one*0.5f;
            cosmetics.petParent=body;
            stage="set living cosmetics";
            cosmetics.ResetCosmetics();
            cosmetics.name = "Necromancer living puppet";
            cosmetics.transform.localPosition = new Vector3(0,0.2f,-0.01f);
            cosmetics.gameObject.SetActive(true);
            cosmetics.SetHat(outfit.HatId,outfit.ColorId);
            cosmetics.SetVisor(outfit.VisorId,outfit.ColorId);
            cosmetics.SetSkin(outfit.SkinId,outfit.ColorId);
            cosmetics.SetMaskType(PlayerMaterial.MaskType.None);
            image.sharedMaterial=bodyMaterial;
            if (cosmetics.nameText) cosmetics.nameText.gameObject.SetActive(false);
            if (cosmetics.colorBlindText) cosmetics.colorBlindText.gameObject.SetActive(false);
            foreach (var part in cosmetics.GetComponentsInChildren<Transform>(true))
                part.gameObject.layer=LayerMask.NameToLayer("Players");
            stage="collect renderers";
            renderers = cosmetics.GetComponentsInChildren<SpriteRenderer>(true).Concat(new[]{image}).ToArray();
            stage="color renderers";
            foreach (var renderer in renderers)
            {
                if (!renderer) continue;
                renderer.color=Color.white;
                PlayerMaterial.SetColors(outfit.ColorId,renderer);
            }
            stage="create animator";
            animator = cosmetics.currentBodySprite.BodySprite.GetComponent<SpriteAnim>() ??
                cosmetics.currentBodySprite.BodySprite.gameObject.AddComponent<SpriteAnim>();
            previous=body.position;
            stage="find animation clips";
            var animations=AmongUsClient.Instance.PlayerPrefab.GetComponent<PlayerPhysics>().Animations;
            var group=animations.group;
            if (group == null)
                for(int i=0;i<animations.animationGroups.Count;i++)
                {
                    var candidate=animations.animationGroups[i];
                    if (candidate != null && candidate.IdleAnim && candidate.RunAnim) { group=candidate; break; }
                }
            if (group == null) throw new InvalidOperationException("Living player animations are unavailable.");
            idle=group.IdleAnim; run=group.RunAnim;
            stage="play idle";
            animator.Play(idle);
        }
        catch (Exception error)
        {
            if(bodyMaterial) Object.Destroy(bodyMaterial);
            Object.Destroy(cosmetics.gameObject);
            Object.Destroy(bodyImage);
            throw new InvalidOperationException($"Necromancer model failed during {stage}.",error);
        }
    }
    public void Tick(Vector2 position)
    {
        var delta=position-previous;
        previous=position;
        var clip=delta.sqrMagnitude>0.000001f ? run : idle;
        if (animator.GetCurrentAnimation()!=clip) animator.Play(clip);
        if (Mathf.Abs(delta.x)>0.0001f)
        {
            cosmetics.SetFlipX(delta.x<0);
            bodyImage.GetComponent<SpriteRenderer>().flipX=delta.x<0;
        }
        var local=PlayerControl.LocalPlayer;
        bool visible=local && local.Data && ShipStatus.Instance;
        if (visible && !controlledHere && !local!.HasDied())
        {
            visible = Vector2.Distance(local.GetTruePosition(), position) <= ShipStatus.Instance.CalculateLightRadius(local.Data) &&
                FurasRuntime.ClearPath(local.GetTruePosition(),position);
        }
        foreach(var renderer in renderers) if(renderer) renderer.enabled=visible;
    }
    public void Dispose() { if(cosmetics) Object.Destroy(cosmetics.gameObject); if(bodyImage) Object.Destroy(bodyImage); if(bodyMaterial) Object.Destroy(bodyMaterial); }
}
