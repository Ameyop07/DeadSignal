using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using CompoundFPS;
public static class FixUpgradeReferences
{
    public static string Main()
    {
        foreach(var entry in EditorBuildSettings.scenes){var scene=EditorSceneManager.OpenScene(entry.path);var g=Object.FindAnyObjectByType<GameManager>();foreach(var w in g.weapons.weapons){w.playerLook=g.player.GetComponentInChildren<PlayerLook>();w.aimCamera=g.playerCamera;}if(g.LevelNumber==1)PrefabUtility.SaveAsPrefabAsset(g.player.gameObject,"Assets/Prefabs/Characters/Player.prefab");EditorSceneManager.SaveScene(scene);}
        AssetDatabase.SaveAssets();EditorSceneManager.OpenScene("Assets/Scenes/Compound.unity");return "All four weapon camera/recoil references fixed across three missions and player prefab.";
    }
}
