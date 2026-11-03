using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using CompoundFPS;
public static class FixSurfaceAssignments
{
    public static string Main()
    {
        if(EditorApplication.isPlaying)throw new System.Exception("Stop play first");
        foreach(var entry in EditorBuildSettings.scenes){var scene=EditorSceneManager.OpenScene(entry.path);GameObject.Find("Surface practice grass").GetComponent<SurfaceAudio>().surface=SurfaceKind.Grass;GameObject.Find("Surface practice steel").GetComponent<SurfaceAudio>().surface=SurfaceKind.Metal;EditorSceneManager.SaveScene(scene);}
        EditorSceneManager.OpenScene("Assets/Scenes/Compound.unity");return "Grass and steel surface assignments corrected in all three levels.";
    }
}
