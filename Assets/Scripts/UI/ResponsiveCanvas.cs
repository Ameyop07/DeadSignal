using UnityEngine;
namespace CompoundFPS
{
    [RequireComponent(typeof(UnityEngine.UI.CanvasScaler))]
    public sealed class ResponsiveCanvas : MonoBehaviour
    {
        UnityEngine.UI.CanvasScaler scaler;int width,height;
        void Awake(){scaler=GetComponent<UnityEngine.UI.CanvasScaler>();}
        void Update(){if(width==Screen.width && height==Screen.height)return;width=Screen.width;height=Screen.height;scaler.matchWidthOrHeight=(float)width/Mathf.Max(1,height)<16f/9?0:1;}
    }
}
