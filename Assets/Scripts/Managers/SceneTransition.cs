using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace CompoundFPS
{
    public sealed class SceneTransition : MonoBehaviour
    {
        public static bool Busy {get;private set;}
        public static void Load(int index)
        {
            if(Busy)return;Busy=true;
            var root=new GameObject("Loading transition");DontDestroyOnLoad(root);root.AddComponent<SceneTransition>().StartCoroutine(LoadRoutine(index,root));
        }
        static IEnumerator LoadRoutine(int index,GameObject root)
        {
            CombatEvents.State=GameState.Paused;Time.timeScale=1;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
            var canvas=root.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=1000;
            var scale=root.AddComponent<UnityEngine.UI.CanvasScaler>();scale.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scale.referenceResolution=new Vector2(1920,1080);scale.matchWidthOrHeight=1;
            root.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            var bg=UIFactory.Panel("Loading",root.transform,Vector2.zero,Vector2.zero,Vector2.zero,UIFactory.Ink);UIFactory.Fill(bg.rectTransform);bg.raycastTarget=true;
            var title=UIFactory.Text("Loading title",bg.transform,"DEPLOYING",54,new Vector2(.5f,.5f),new Vector2(0,70),new Vector2(800,90));title.alignment=TMPro.TextAlignmentOptions.Center;
            var label=UIFactory.Text("Progress",bg.transform,"Preparing mission...",24,new Vector2(.5f,.5f),new Vector2(0,-60),new Vector2(800,50),UIFactory.Accent);label.alignment=TMPro.TextAlignmentOptions.Center;
            var track=UIFactory.Panel("Track",bg.transform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(700,5),new Color(.15f,.22f,.25f));
            var bar=UIFactory.Panel("Progress bar",track.transform,new Vector2(0,.5f),Vector2.zero,new Vector2(0,5),UIFactory.Accent);
            yield return null;yield return new WaitForEndOfFrame();
            var op=SceneManager.LoadSceneAsync(index);op.allowSceneActivation=false;
            while(op.progress<.9f){float p=op.progress/.9f;bar.rectTransform.sizeDelta=new Vector2(700*p,5);label.SetText("LOADING  {0:0}%",p*100);yield return null;}
            bar.rectTransform.sizeDelta=new Vector2(700,5);label.text="READY";yield return null;op.allowSceneActivation=true;
            while(!op.isDone)yield return null;
            yield return null;Busy=false;Destroy(root);
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void Reset(){Busy=false;}
    }
}
