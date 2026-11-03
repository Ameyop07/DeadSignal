using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Linq;
public static class PhaseOne
{
    public static string Main()
    {
        string[] folders = { "Animations", "Audio", "Materials", "Models", "Prefabs", "Textures", "VFX" };
        foreach(var f in folders) if(!AssetDatabase.IsValidFolder("Assets/"+f)) AssetDatabase.CreateFolder("Assets",f);
        foreach(var f in new[]{"Characters","Enemies","Weapons","Environment","UI"}) if(!AssetDatabase.IsValidFolder("Assets/Prefabs/"+f)) AssetDatabase.CreateFolder("Assets/Prefabs",f);
        var nav = UnityEngine.Object.FindObjectsByType<Unity.AI.Navigation.NavMeshSurface>(FindObjectsSortMode.None).Length;
        var agents = UnityEngine.Object.FindObjectsByType<UnityEngine.AI.NavMeshAgent>(FindObjectsSortMode.None).Length;
        var old = Enumerable.Range(0,SceneManager.sceneCount).Select(SceneManager.GetSceneAt).ToArray();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube); floor.name="Test Floor"; floor.transform.position=new Vector3(0,-.5f,0); floor.transform.localScale=new Vector3(40,1,40);
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/FPS_Test.unity");
        foreach(var s in old) if(!s.isDirty) EditorSceneManager.CloseScene(s,true);
        AssetDatabase.Refresh();
        return "FPS test scene saved. Existing navigation surfaces="+nav+", agents="+agents;
    }
}
