using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering.Universal;
using CompoundFPS;
using TMPro;

public static class WireMission
{
    public static string Main()
    {
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if(scene.path!="Assets/Scenes/Compound.unity")throw new System.Exception("Open Compound first");
        var player=Object.FindFirstObjectByType<PlayerController>();
        var root=new GameObject("Mission Systems");
        var objectives=root.AddComponent<ObjectiveManager>();objectives.player=player.transform;objectives.extraction=GameObject.Find("Extraction Zone").transform;
        var game=root.AddComponent<GameManager>();game.player=player;game.playerHealth=player.GetComponent<PlayerHealth>();game.weapons=player.GetComponent<WeaponManager>();game.playerCamera=player.GetComponentInChildren<Camera>();game.menuCamera=GameObject.Find("Menu Camera").GetComponent<Camera>();game.spawn=GameObject.Find("Player Spawn").transform;game.objectives=objectives;
        root.AddComponent<AudioManager>();root.AddComponent<ImpactPool>();
        var ui=new GameObject("Interface",typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster));
        ui.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=ui.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
        var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if(!font)throw new System.Exception("TMP font missing");
        ui.AddComponent<HUDManager>().font=font;ui.AddComponent<MainMenu>().font=font;
        PrefabUtility.SaveAsPrefabAsset(ui,"Assets/Prefabs/UI/Interface.prefab");
        Label("OUTPOST 17",new Vector3(0,4.25f,-18.56f),1.9f,font);
        Label("SECTOR A",new Vector3(-15.5f,3.55f,-9.3f),1.1f,font);
        Label("EXTRACTION",new Vector3(0,2.5f,32.9f),1.8f,font);
        Label("LOADING  /  02",new Vector3(-15,3.5f,27.6f),1.1f,font);
        var pipeline=UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if(pipeline){pipeline.shadowDistance=45;pipeline.msaaSampleCount=2;EditorUtility.SetDirty(pipeline);}
        foreach(var c in Object.FindObjectsByType<Camera>()){c.allowHDR=false;c.allowMSAA=true;}
        game.menuCamera.enabled=true;game.playerCamera.enabled=false;
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(scene.path,true)};
        PlayerSettings.productName="Dead Signal";PlayerSettings.companyName="Independent Prototype";PlayerSettings.defaultScreenWidth=1920;PlayerSettings.defaultScreenHeight=1080;PlayerSettings.fullScreenMode=FullScreenMode.FullScreenWindow;
        EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        return "Mission, cameras, UI prefab, objectives, audio and effects wired. Build starts at Compound main menu.";
    }
    static void Label(string text,Vector3 position,float size,TMP_FontAsset font)
    {
        var g=new GameObject(text);g.transform.position=position;var t=g.AddComponent<TextMeshPro>();t.font=font;t.text=text;t.fontSize=size;t.alignment=TextAlignmentOptions.Center;t.color=new Color(.98f,.76f,.3f);t.rectTransform.sizeDelta=new Vector2(12,2);t.enableAutoSizing=false;
    }
}
