using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using UnityEditor.SceneManagement;
using CompoundFPS;
public static class FixBuildReferences
{
    public static string Main()
    {
        var mat=new Material(Shader.Find("Universal Render Pipeline/Unlit"));mat.name="Impact template";AssetDatabase.CreateAsset(mat,"Assets/Materials/Impact template.mat");
        foreach(var entry in EditorBuildSettings.scenes)
        {
            var scene=EditorSceneManager.OpenScene(entry.path);
            Object.FindAnyObjectByType<ImpactPool>().impactMaterial=mat;
            foreach(var agent in Object.FindObjectsByType<NavMeshAgent>())agent.enabled=false;
            EditorSceneManager.SaveScene(scene);
        }
        var prefab=PrefabUtility.LoadPrefabContents("Assets/Prefabs/Enemies/Guard.prefab");prefab.GetComponent<NavMeshAgent>().enabled=false;PrefabUtility.SaveAsPrefabAsset(prefab,"Assets/Prefabs/Enemies/Guard.prefab");PrefabUtility.UnloadPrefabContents(prefab);
        AssetDatabase.SaveAssets();EditorSceneManager.OpenScene("Assets/Scenes/Compound.unity");return "Impact material is explicitly included; agents activate in Start after surfaces register.";
    }
}
