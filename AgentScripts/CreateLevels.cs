using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.AI.Navigation;
using CompoundFPS;
using System.Linq;
using System.Collections.Generic;
public static class CreateLevels
{
    static void Block(string name,Vector3 p,Vector3 size,Material m,Transform parent)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent);g.transform.position=p;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=m;
    }
    public static string Main()
    {
        if(EditorApplication.isPlaying)throw new System.Exception("Stop play mode first");
        var paths=new[]{"Assets/Scenes/Compound.unity","Assets/Scenes/FreightDepot.unity","Assets/Scenes/RelayStation.unity"};
        var names=new[]{"OUTPOST 17","FREIGHT DEPOT","RELAY STATION"};
        var report=new List<string>();
        for(int level=1;level<=2;level++)
        {
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(paths[level]))throw new System.Exception("Level already exists: "+paths[level]);
            var scene=EditorSceneManager.OpenScene(paths[0]);EditorSceneManager.SaveScene(scene,paths[level]);
            var game=Object.FindAnyObjectByType<GameManager>();game.missionName=names[level];
            var world=GameObject.Find("Industrial Compound").transform;
            var concrete=AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Concrete.mat");var dark=AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Graphite.mat");var ochre=AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Safety Ochre.mat");
            // Reconfigure the main lane and add distinct routes while keeping a reachable perimeter.
            if(level==1)
            {
                Block("Depot lane divider",new Vector3(0,1.4f,-4),new Vector3(.7f,2.8f,18),concrete,world);
                Block("Depot west checkpoint",new Vector3(-4,1.2f,10),new Vector3(7,2.4f,.8f),dark,world);
                Block("Depot east checkpoint",new Vector3(7,1.2f,21),new Vector3(10,2.4f,.8f),dark,world);
                for(int z=-12;z<=18;z+=10){Block("Freight stack",new Vector3(21,1.5f,z),new Vector3(4,3,4),ochre,world);}
                RenderSettings.fogColor=new Color(.65f,.54f,.40f);RenderSettings.ambientLight=new Color(.55f,.47f,.37f);
                GameObject.Find("Sun").transform.rotation=Quaternion.Euler(25,40,0);
            }
            else
            {
                Block("Relay bunker west",new Vector3(-3.5f,1.8f,8),new Vector3(.7f,3.6f,9),dark,world);
                Block("Relay bunker east",new Vector3(3.5f,1.8f,8),new Vector3(.7f,3.6f,9),dark,world);
                Block("Relay bunker roof",new Vector3(0,3.8f,8),new Vector3(8,.4f,10),concrete,world);
                Block("Relay control bank",new Vector3(0,.7f,11),new Vector3(4,1.4f,1),ochre,world);
                for(int x=-1;x<=1;x+=2){Block("Antenna mast",new Vector3(x*7,6,28),new Vector3(.3f,12,.3f),dark,world);Block("Antenna array",new Vector3(x*7,10,28),new Vector3(4,.2f,.2f),ochre,world);}
                RenderSettings.fogColor=new Color(.35f,.43f,.51f);RenderSettings.ambientLight=new Color(.35f,.44f,.54f);
                GameObject.Find("Sun").transform.rotation=Quaternion.Euler(18,-70,0);GameObject.Find("Sun").GetComponent<Light>().color=new Color(.66f,.8f,1);
            }
            var surface=world.GetComponent<NavMeshSurface>();surface.RemoveData();surface.navMeshData=null;surface.BuildNavMesh();AssetDatabase.CreateAsset(surface.navMeshData,"Assets/Scenes/"+names[level].Replace(" ","")+"NavMesh.asset");
            var extra=level==1?new[]{new Vector3(6,0,-13),new Vector3(-7,0,22)}:new[]{new Vector3(-7,0,-13),new Vector3(7,0,7),new Vector3(0,0,6),new Vector3(-7,0,29)};
            int n=8;
            foreach(var p in extra)
            {
                var guard=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Guard.prefab"));guard.name="Guard "+(++n);guard.transform.position=p;
                var ai=guard.GetComponent<EnemyAI>();ai.player=game.playerHealth;ai.playerController=game.player;ai.waypoints=new[]{p,p+Vector3.forward*2};
            }
            foreach(var ai in Object.FindObjectsByType<EnemyAI>())
            {
                if(!NavMesh.SamplePosition(ai.transform.position,out var nav,4,NavMesh.AllAreas))throw new System.Exception("No spawn for "+ai.name);
                ai.transform.position=nav.position;ai.GetComponent<EnemyWeapon>().accuracy=.32f+level*.035f;ai.GetComponent<EnemyWeapon>().fireRate=1.1f+level*.1f;
                for(int i=0;i<ai.waypoints.Length;i++)if(NavMesh.SamplePosition(ai.waypoints[i],out var wp,4,NavMesh.AllAreas))ai.waypoints[i]=wp.position;
            }
            game.objectives.requiredEnemies=n;
            foreach(var t in Object.FindObjectsByType<TMPro.TextMeshPro>())if(t.text=="OUTPOST 17")t.text=names[level];
            Physics.SyncTransforms();var path=new NavMeshPath();NavMesh.SamplePosition(game.spawn.position,out var start,2,NavMesh.AllAreas);NavMesh.SamplePosition(game.objectives.extraction.position,out var end,2,NavMesh.AllAreas);
            if(!NavMesh.CalculatePath(start.position,end.position,NavMesh.AllAreas,path)||path.status!=NavMeshPathStatus.PathComplete)throw new System.Exception("Extraction unreachable "+names[level]);
            EditorSceneManager.SaveScene(scene);report.Add(names[level]+": "+n+" guards; extraction reachable");
        }
        EditorBuildSettings.scenes=paths.Select(p=>new EditorBuildSettingsScene(p,true)).ToArray();AssetDatabase.SaveAssets();EditorSceneManager.OpenScene(paths[0]);
        return string.Join("; ",report);
    }
}
