using UnityEngine;
using UnityEngine.EventSystems;
namespace CompoundFPS
{
    public sealed class UIFeedback : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler,IPointerUpHandler
    {
        float target=1;
        public void OnPointerEnter(PointerEventData e){target=1.018f;}
        public void OnPointerExit(PointerEventData e){target=1;}
        public void OnPointerDown(PointerEventData e){target=.98f;}
        public void OnPointerUp(PointerEventData e){target=1.018f;}
        void OnDisable(){target=1;transform.localScale=Vector3.one;}
        void Update(){if(Mathf.Abs(transform.localScale.x-target)>.0001f)transform.localScale=Vector3.one*Mathf.Lerp(transform.localScale.x,target,1-Mathf.Exp(-20*Time.unscaledDeltaTime));}
    }
}
