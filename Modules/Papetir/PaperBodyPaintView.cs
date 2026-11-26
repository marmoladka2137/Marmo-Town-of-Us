using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using UnityEngine;
namespace TouMarmo.Modules.Papetir;

// The sprite/pivot changes during the native falling animation. Refresh after
// animation updates instead of retaining a decal cut from the initial frame.
[RegisterInIl2Cpp]
public sealed class PaperBodyPaintView(IntPtr ptr) : MonoBehaviour(ptr)
{
    private SpriteRenderer source=null!,display=null!;
    private Sprite? frame;
    [HideFromIl2Cpp]
    public void Configure(SpriteRenderer original,SpriteRenderer overlay)
    {
        source=original;display=overlay;Sync();
    }
    private void LateUpdate()=>Sync();
    [HideFromIl2Cpp]
    public void Sync()
    {
        if(!display) return;
        if(!source || !source.sprite || !source.enabled || source.forceRenderingOff)
        {
            display.enabled=false;return;
        }
        if(frame!=source.sprite)
        {
            // Never show the old frame's pixels while preparing the new one.
            display.enabled=false;
            display.sprite=PaperBodyAppearance.GetChips(source.sprite!);
            frame=source.sprite;
        }
        display.flipX=source.flipX;display.flipY=source.flipY;
        display.sortingLayerID=source.sortingLayerID;display.sortingOrder=source.sortingOrder+1;
        display.color=new Color(1,1,1,source.color.a);
        display.enabled=display.sprite;
    }
}
