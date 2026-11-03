using UnityEngine;
namespace CompoundFPS
{
    public sealed class UIFade : MonoBehaviour
    {
        CanvasGroup group;
        void Awake(){group=gameObject.AddComponent<CanvasGroup>();}
        void OnEnable(){if(group)group.alpha=0;}
        void Update(){if(group.alpha<1)group.alpha=Mathf.MoveTowards(group.alpha,1,Time.unscaledDeltaTime*6);}
    }
}
