using UnityEngine;
using TMPro;
namespace CompoundFPS
{
    public sealed class ScopeOverlay : MonoBehaviour
    {
        GameObject overlay;Texture2D mask;RectTransform center,left,right;
        Renderer[] gunRenderers;Weapon cachedWeapon;GameManager game;bool scoped;
        void Start()
        {
            game=GameManager.Instance;
            overlay=UIFactory.Rect("Rifle Scope",transform,Vector2.zero,Vector2.zero,Vector2.zero).gameObject;UIFactory.Fill((RectTransform)overlay.transform);
            overlay.transform.SetAsFirstSibling();
            // A circular transparent aperture plus opaque side wings works at every aspect ratio.
            mask=new Texture2D(512,512,TextureFormat.RGBA32,false);mask.name="Scope aperture";mask.wrapMode=TextureWrapMode.Clamp;
            var pixels=new Color32[512*512];for(int y=0;y<512;y++)for(int x=0;x<512;x++){float d=Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(256,256));byte a=(byte)(Mathf.Clamp01((d-238)/3)*255);pixels[y*512+x]=new Color32(3,6,7,a);}mask.SetPixels32(pixels);mask.Apply();
            center=UIFactory.Rect("Optic aperture",overlay.transform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(1080,1080));var image=center.gameObject.AddComponent<UnityEngine.UI.RawImage>();image.texture=mask;image.raycastTarget=false;
            left=UIFactory.Panel("Left mask",overlay.transform,new Vector2(0,.5f),Vector2.zero,Vector2.zero,new Color(.012f,.024f,.028f)).rectTransform;
            right=UIFactory.Panel("Right mask",overlay.transform,new Vector2(1,.5f),Vector2.zero,Vector2.zero,new Color(.012f,.024f,.028f)).rectTransform;
            UIFactory.Panel("Reticle horizontal",center,new Vector2(.5f,.5f),Vector2.zero,new Vector2(600,2),new Color(0,0,0,.85f));
            UIFactory.Panel("Reticle vertical",center,new Vector2(.5f,.5f),Vector2.zero,new Vector2(2,600),new Color(0,0,0,.85f));
            UIFactory.Panel("Aim dot",center,new Vector2(.5f,.5f),Vector2.zero,new Vector2(4,4),UIFactory.Accent);
            for(int i=-3;i<=3;i++)if(i!=0){UIFactory.Panel("Range hash",center,new Vector2(.5f,.5f),new Vector2(0,i*65),new Vector2(16,2),Color.black);UIFactory.Panel("Windage hash",center,new Vector2(.5f,.5f),new Vector2(i*65,0),new Vector2(2,16),Color.black);}
            var label=UIFactory.Text("Optic label",center,"AR-41  /  3x OPTIC",18,new Vector2(.5f,.5f),new Vector2(0,-350),new Vector2(300,35),UIFactory.Accent);label.alignment=TextAlignmentOptions.Center;
            overlay.SetActive(false);
        }
        void LateUpdate()
        {
            if(!game || !overlay)return;var w=game.weapons.Current;
            if(w!=cachedWeapon){if(gunRenderers!=null)foreach(var r in gunRenderers)if(r)r.enabled=true;cachedWeapon=w;gunRenderers=w?w.GetComponentsInChildren<Renderer>(true):null;}
            scoped=CombatEvents.Playing && w && w.Aiming && w.definition.hasScope;
            overlay.SetActive(scoped);if(gunRenderers!=null)foreach(var r in gunRenderers)if(r)r.enabled=!scoped;
            if(scoped){var size=((RectTransform)overlay.transform).rect.size;center.sizeDelta=Vector2.one*size.y;left.sizeDelta=right.sizeDelta=new Vector2(Mathf.Max(0,(size.x-size.y)/2)+1,size.y);}
        }
        void OnDestroy(){if(mask)Destroy(mask);if(gunRenderers!=null)foreach(var r in gunRenderers)if(r)r.enabled=true;}
    }
}
