using UnityEngine;
using TMPro;
using UnityEngine.Events;
namespace CompoundFPS
{
    public static class UIFactory
    {
        public static TMP_FontAsset Font;
        public static readonly Color Ink=new Color(.025f,.044f,.062f,.96f), Accent=new Color(.25f,.88f,.77f), White=new Color(.91f,.95f,.98f);
        public static RectTransform Rect(string name,Transform parent,Vector2 anchor,Vector2 pos,Vector2 size)
        {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);
            r.anchorMin=r.anchorMax=anchor;r.pivot=anchor;r.anchoredPosition=pos;r.sizeDelta=size;return r;
        }
        public static UnityEngine.UI.Image Panel(string name,Transform parent,Vector2 anchor,Vector2 pos,Vector2 size,Color color)
        {
            var r=Rect(name,parent,anchor,pos,size);var image=r.gameObject.AddComponent<UnityEngine.UI.Image>();image.color=color;image.raycastTarget=false;return image;
        }
        public static TextMeshProUGUI Text(string name,Transform parent,string text,int size,Vector2 anchor,Vector2 pos,Vector2 bounds,Color? color=null)
        {
            var r=Rect(name,parent,anchor,pos,bounds);var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=Font;t.text=text;t.fontSize=size;t.color=color??White;t.raycastTarget=false;t.enableAutoSizing=false;t.textWrappingMode=TextWrappingModes.Normal;return t;
        }
        public static UnityEngine.UI.Button Button(Transform parent,string label,UnityAction action)
        {
            var image=Panel(label,parent,new Vector2(.5f,.5f),Vector2.zero,new Vector2(550,66),new Color(.13f,.19f,.20f,.95f));image.raycastTarget=true;
            var layout=image.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();layout.preferredHeight=66;layout.minHeight=66;
            var b=image.gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=image;
            image.gameObject.AddComponent<UIFeedback>();
            var c=b.colors;c.normalColor=Color.white;c.highlightedColor=new Color(1.3f,1.15f,.85f);c.pressedColor=Accent;b.colors=c;
            var text=Text("Label",image.transform,label,24,new Vector2(.5f,.5f),Vector2.zero,new Vector2(510,48));Fill(text.rectTransform);text.margin=new Vector4(22,5,15,5);text.alignment=TextAlignmentOptions.MidlineLeft;
            b.onClick.AddListener(()=>CombatEvents.Play(SoundCue.Button,Vector3.zero));b.onClick.AddListener(action);return b;
        }
        public static RectTransform Stack(Transform parent,string name,Vector2 anchor,Vector2 pos,Vector2 size,float spacing=12)
        {
            var r=Rect(name,parent,anchor,pos,size);var l=r.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();l.spacing=spacing;l.childControlWidth=true;l.childControlHeight=true;l.childForceExpandWidth=true;l.childForceExpandHeight=false;return r;
        }
        public static void Fill(RectTransform rect){rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;}
    }
}
