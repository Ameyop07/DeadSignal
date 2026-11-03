using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Unity.AI.Navigation;
using CompoundFPS;

public static class BuildCompound
{
    static Transform world;
    static Material concrete, dark, teal, rust, yellow, armor, red, glass;
    static Material Mat(string name,Color color,float metallic=0)
    {
        string path="Assets/Materials/"+name+".mat";
        var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
        m.color=color;m.SetFloat("_Metallic",metallic);m.SetFloat("_Smoothness",.25f);return m;
    }
    static GameObject Box(string name,Vector3 p,Vector3 scale,Material m,Transform parent=null,bool collider=true)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent?parent:world,false);g.transform.localPosition=p;g.transform.localScale=scale;
        g.GetComponent<Renderer>().sharedMaterial=m;if(!collider)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;
    }
    static GameObject Sphere(string name,Vector3 p,Vector3 scale,Material m,Transform parent)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Sphere);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=m;return g;
    }
    public static string Main()
    {
        if(AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/Compound.unity"))throw new Exception("Compound already exists; modify it instead of overwriting.");
        var old=Enumerable.Range(0,SceneManager.sceneCount).Select(SceneManager.GetSceneAt).ToArray();
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(scene);
        world=new GameObject("Industrial Compound").transform;
        concrete=Mat("Concrete",new Color(.39f,.43f,.43f)); dark=Mat("Graphite",new Color(.09f,.13f,.15f));
        teal=Mat("Container Teal",new Color(.12f,.34f,.36f),.3f);rust=Mat("Oxide",new Color(.48f,.22f,.12f),.25f);
        yellow=Mat("Safety Ochre",new Color(.95f,.64f,.17f));armor=Mat("Guard Armor",new Color(.25f,.30f,.23f));
        red=Mat("Guard Visor",new Color(.92f,.24f,.12f));glass=Mat("Signal Cyan",new Color(.2f,.85f,.8f));
        Box("Compound foundation",new Vector3(0,-.5f,0),new Vector3(52,1,70),concrete);
        Box("Road",new Vector3(0,.012f,0),new Vector3(8,.024f,65),dark,collider:false);
        for(int z=-28;z<30;z+=6)Box("Road dash",new Vector3(0,.035f,z),new Vector3(.15f,.025f,2.5f),yellow,collider:false);
        Box("West perimeter",new Vector3(-25,2,0),new Vector3(1,4,70),concrete);
        Box("East perimeter",new Vector3(25,2,0),new Vector3(1,4,70),concrete);
        Box("North perimeter",new Vector3(0,2,34),new Vector3(50,4,1),concrete);
        Box("South perimeter",new Vector3(0,2,-34),new Vector3(50,4,1),concrete);
        Box("Entrance West",new Vector3(-15,2,-18),new Vector3(20,4,.6f),dark);
        Box("Entrance East",new Vector3(15,2,-18),new Vector3(20,4,.6f),dark);
        Box("Gate lintel",new Vector3(0,4.3f,-18),new Vector3(11,.7f,1),yellow);
        for(int x=-5;x<=5;x+=10)Box("Gate upright",new Vector3(x,2.3f,-18),new Vector3(.6f,4.6f,1),yellow);
        // Walk-through warehouse with two entrances and an interior divider.
        Box("Warehouse west",new Vector3(-21,2,1),new Vector3(.5f,4,20),teal);
        Box("Warehouse east",new Vector3(-10,2,1),new Vector3(.5f,4,20),teal);
        for(int z=-9;z<=11;z+=20)
        { Box("Warehouse doorway left",new Vector3(-19,2,z),new Vector3(4,4,.5f),teal);Box("Warehouse doorway right",new Vector3(-12,2,z),new Vector3(4,4,.5f),teal);Box("Door lintel",new Vector3(-15.5f,3.7f,z),new Vector3(3,.6f,.5f),yellow); }
        Box("Warehouse roof",new Vector3(-15.5f,4.2f,1),new Vector3(12,.4f,21),dark);
        Box("Interior divider",new Vector3(-17,1.5f,1),new Vector3(7,3,.4f),concrete);
        Box("Warehouse crate",new Vector3(-12,.6f,5),new Vector3(2,1.2f,2),rust);
        Container(new Vector3(14,0,-8),teal);Container(new Vector3(17,0,5),rust);Container(new Vector3(11,0,16),teal);
        foreach(var p in new[]{new Vector3(-6,.6f,-9),new Vector3(6,.6f,-3),new Vector3(-5,.6f,5),new Vector3(4,.6f,11),new Vector3(0,.6f,20),new Vector3(17,.6f,23)})
        { Box("Concrete cover",p,new Vector3(3,1.2f,.8f),concrete);Box("Cover warning stripe",p+Vector3.up*.61f,new Vector3(3,.035f,.85f),yellow,collider:false); }
        Box("Loading deck",new Vector3(-15,2.75f,23),new Vector3(12,.5f,10),concrete);
        Box("Deck west support",new Vector3(-20,1.3f,23),new Vector3(.6f,2.6f,9),dark);
        Box("Deck north guardrail",new Vector3(-15,3.7f,27.7f),new Vector3(12,.12f,.12f),yellow);
        var ramp=Box("Access ramp",new Vector3(-15,1.5f,13),new Vector3(4,.35f,10.45f),concrete);ramp.transform.localRotation=Quaternion.Euler(-16.7f,0,0);
        for(int x=-23;x<=23;x+=46)for(int z=-20;z<=25;z+=15)
        { Box("Light pole",new Vector3(x,3,z),new Vector3(.18f,6,.18f),dark);Box("Lamp housing",new Vector3(x,6,z),new Vector3(1,.2f,.5f),yellow); }
        Box("Extraction pad",new Vector3(0,.05f,29),new Vector3(7,.1f,6),teal);
        for(int x=-3;x<=3;x+=6)Box("Extraction marker",new Vector3(x,.12f,29),new Vector3(.15f,.1f,6),glass,collider:false);
        var extract=new GameObject("Extraction Zone");extract.transform.position=new Vector3(0,0,29);
        var spawn=new GameObject("Player Spawn");spawn.transform.position=new Vector3(0,.1f,-28);
        var sun=new GameObject("Sun",typeof(Light));sun.transform.rotation=Quaternion.Euler(40,-28,0);var light=sun.GetComponent<Light>();light.type=LightType.Directional;light.intensity=1.6f;light.color=new Color(1,.87f,.7f);light.shadows=LightShadows.Soft;
        RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.45f,.53f,.59f);
        RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogColor=new Color(.56f,.64f,.66f);RenderSettings.fogDensity=.008f;
        var surface=world.gameObject.AddComponent<NavMeshSurface>();surface.collectObjects=CollectObjects.Children;surface.useGeometry=NavMeshCollectGeometry.PhysicsColliders;
        surface.BuildNavMesh();AssetDatabase.CreateAsset(surface.navMeshData,"Assets/Scenes/CompoundNavMesh.asset");
        var player=BuildPlayer(spawn.transform.position);
        var playerHealth=player.GetComponent<PlayerHealth>();var controller=player.GetComponent<PlayerController>();
        var positions=new[]{new Vector3(-7,0,-11),new Vector3(8,0,-10),new Vector3(-15,0,-5),new Vector3(10,0,2),new Vector3(-8,0,12),new Vector3(17,0,14),new Vector3(-3,0,24),new Vector3(9,0,26)};
        for(int i=0;i<positions.Length;i++)
        {
            var enemy=BuildEnemy(positions[i],i);enemy.player=playerHealth;enemy.playerController=controller;
            if(i==0)PrefabUtility.SaveAsPrefabAsset(enemy.gameObject,"Assets/Prefabs/Enemies/Guard.prefab");
        }
        var overview=new GameObject("Menu Camera",typeof(Camera));overview.transform.position=new Vector3(23,16,-25);overview.transform.LookAt(new Vector3(-2,0,4));overview.GetComponent<Camera>().enabled=false;
        var data=overview.AddComponent<UniversalAdditionalCameraData>();data.renderPostProcessing=false;
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/Compound.unity");AssetDatabase.SaveAssets();
        foreach(var s in old)if(!s.isDirty)EditorSceneManager.CloseScene(s,true);
        if(SceneView.lastActiveSceneView){SceneView.lastActiveSceneView.in2DMode=false;SceneView.lastActiveSceneView.LookAt(new Vector3(0,0,0),Quaternion.Euler(45,0,0),40);}
        int valid=0;foreach(var p in positions)if(NavMesh.SamplePosition(p,out _,2,NavMesh.AllAreas))valid++;
        return "Compound scene saved. NavMesh triangles="+NavMesh.CalculateTriangulation().indices.Length/3+"; valid guard spawns="+valid+"/8.";
    }
    static void Container(Vector3 p,Material m)
    {
        var c=Box("Freight container",p+Vector3.up*1.5f,new Vector3(6,3,3),m);
        for(int i=-2;i<=2;i++)Box("Container rib",p+new Vector3(i,1.5f,-1.53f),new Vector3(.08f,2.85f,.07f),dark,collider:false);
        if(!AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/Container.prefab"))PrefabUtility.SaveAsPrefabAsset(c,"Assets/Prefabs/Environment/Container.prefab");
    }
    static GameObject BuildPlayer(Vector3 p)
    {
        var player=new GameObject("Player");player.layer=2;player.transform.position=p;
        var cc=player.AddComponent<CharacterController>();cc.height=1.8f;cc.radius=.3f;cc.center=Vector3.up*.9f;cc.stepOffset=.3f;
        var controller=player.AddComponent<PlayerController>();player.AddComponent<PlayerHealth>();
        var pivot=new GameObject("View Pivot").transform;pivot.SetParent(player.transform,false);pivot.localPosition=Vector3.up*1.65f;controller.cameraPivot=pivot;
        var cam=pivot.gameObject.AddComponent<Camera>();cam.tag="MainCamera";cam.fieldOfView=75;cam.nearClipPlane=.05f;cam.farClipPlane=150;cam.backgroundColor=new Color(.56f,.64f,.66f);cam.clearFlags=CameraClearFlags.SolidColor;
        pivot.gameObject.AddComponent<AudioListener>();pivot.gameObject.AddComponent<UniversalAdditionalCameraData>();
        var look=pivot.gameObject.AddComponent<PlayerLook>();look.body=player.transform;
        var manager=player.AddComponent<WeaponManager>();manager.weapons=new Weapon[2];
        for(int i=0;i<2;i++)
        {
            var def=ScriptableObject.CreateInstance<WeaponDefinition>();def.displayName=i==0?"AR-41 / CARBINE":"P9 / SIDEARM";def.automatic=i==0;
            if(i==1){def.damage=38;def.fireRate=4;def.magazineCapacity=12;def.reserveAmmunition=60;def.reloadDuration=1.4f;def.recoil=1.1f;def.spread=.006f;def.hipPosition=new Vector3(.22f,-.22f,.4f);}
            AssetDatabase.CreateAsset(def,"Assets/Prefabs/Weapons/"+(i==0?"Carbine":"Pistol")+"Definition.asset");
            var gun=new GameObject(i==0?"Carbine":"Pistol");gun.transform.SetParent(pivot,false);gun.transform.localPosition=def.hipPosition;
            Box("Receiver",new Vector3(0,0,.08f),new Vector3(.13f,.14f,i==0?.5f:.24f),dark,gun.transform,false);
            Box("Barrel",new Vector3(0,.015f,i==0?.44f:.24f),new Vector3(.055f,.055f,i==0?.28f:.12f),concrete,gun.transform,false);
            Box("Grip",new Vector3(0,-.13f,.02f),new Vector3(.09f,.22f,.09f),dark,gun.transform,false);
            Box("Sight rear",new Vector3(0,.10f,-.08f),new Vector3(.08f,.055f,.035f),yellow,gun.transform,false);
            Box("Sight front",new Vector3(0,.10f,i==0?.28f:.14f),new Vector3(.025f,.055f,.025f),yellow,gun.transform,false);
            if(i==0)Box("Magazine",new Vector3(0,-.15f,.19f),new Vector3(.09f,.22f,.13f),teal,gun.transform,false);
            var flash=Sphere("Muzzle Flash",new Vector3(0,.015f,i==0?.61f:.32f),new Vector3(.08f,.08f,.17f),yellow,gun.transform);UnityEngine.Object.DestroyImmediate(flash.GetComponent<Collider>());flash.SetActive(false);
            foreach(var t in gun.GetComponentsInChildren<Transform>(true))t.gameObject.layer=2;
            var w=gun.AddComponent<Weapon>();w.definition=def;w.aimCamera=cam;w.playerLook=look;w.muzzleFlash=flash;manager.weapons[i]=w;
            PrefabUtility.SaveAsPrefabAsset(gun,"Assets/Prefabs/Weapons/"+gun.name+".prefab");
        }
        var recoil=player.AddComponent<WeaponRecoil>();recoil.manager=manager;recoil.controller=controller;recoil.look=look;recoil.viewCamera=cam;
        PrefabUtility.SaveAsPrefabAsset(player,"Assets/Prefabs/Characters/Player.prefab");return player;
    }
    static EnemyAI BuildEnemy(Vector3 p,int index)
    {
        var root=new GameObject("Guard "+(index+1));root.transform.position=p;root.transform.rotation=Quaternion.Euler(0,180,0);
        var agent=root.AddComponent<NavMeshAgent>();agent.radius=.35f;agent.height=1.8f;agent.speed=3.2f;agent.angularSpeed=180;
        var visual=new GameObject("Guard Visual").transform;visual.SetParent(root.transform,false);
        Box("Body",new Vector3(0,.95f,0),new Vector3(.65f,.9f,.4f),armor,visual);
        var head=Sphere("Head",new Vector3(0,1.65f,0),Vector3.one*.43f,armor,visual);
        Box("Visor",new Vector3(0,1.66f,.2f),new Vector3(.32f,.09f,.04f),red,visual,false);
        for(int x=-1;x<=1;x+=2){Box("Leg",new Vector3(x*.18f,.3f,0),new Vector3(.23f,.6f,.26f),dark,visual);Box("Arm",new Vector3(x*.42f,1.1f,.1f),new Vector3(.19f,.65f,.23f),armor,visual);}
        Box("Rifle",new Vector3(.32f,1.15f,.48f),new Vector3(.1f,.12f,.7f),dark,visual,false);
        var muzzle=new GameObject("Muzzle").transform;muzzle.SetParent(root.transform,false);muzzle.localPosition=new Vector3(.32f,1.15f,.87f);
        var health=root.AddComponent<EnemyHealth>();health.maximumHealth=75;health.visual=visual;
        var zone=head.AddComponent<HitZone>();zone.owner=health;
        var weapon=root.AddComponent<EnemyWeapon>();weapon.muzzle=muzzle;
        var ai=root.AddComponent<EnemyAI>();ai.waypoints=new[]{p,p+new Vector3(index%2==0?3:-3,0,3),p+new Vector3(0,0,-2)};
        return ai;
    }
}
