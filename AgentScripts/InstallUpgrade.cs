using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.AI;
using UnityEditor;
using UnityEditor.SceneManagement;
using CompoundFPS;
using Object=UnityEngine.Object;
public static class InstallUpgrade
{
    static T Asset<T>(string path,Func<T> create) where T:Object {var a=AssetDatabase.LoadAssetAtPath<T>(path);if(!a){a=create();AssetDatabase.CreateAsset(a,path);}return a;}
    static AudioClip Clip(string name)=>AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Designed/"+name+".wav");
    static Material Mat(string name,Color color){var m=Asset("Assets/Materials/"+name+".mat",()=>new Material(Shader.Find("Universal Render Pipeline/Lit")));m.color=color;m.SetFloat("_Smoothness",.15f);EditorUtility.SetDirty(m);return m;}
    static GameObject Box(string name,Transform parent,Vector3 pos,Vector3 size,Material mat){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent);g.transform.position=pos;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=mat;return g;}
    public static string Main()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop play first");
        foreach(string folder in new[]{"Assets/Configuration","Assets/Meshes/Soldier","Assets/Art/WeaponPreviews"})Directory.CreateDirectory(folder);
        AssetDatabase.Refresh();
        var catalog=Asset("Assets/Configuration/Campaign.asset",()=>ScriptableObject.CreateInstance<CampaignCatalog>());
        var rifle=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Prefabs/Weapons/CarbineDefinition.asset");
        var pistol=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Prefabs/Weapons/PistolDefinition.asset");
        var smg=Asset("Assets/Prefabs/Weapons/SMGDefinition.asset",()=>Object.Instantiate(rifle));
        var dmr=Asset("Assets/Prefabs/Weapons/DMRDefinition.asset",()=>Object.Instantiate(rifle));
        var defs=new[]{rifle,pistol,smg,dmr};string[] ids={"rifle","pistol","smg","dmr"},audio={"Rifle","Pistol","SMG","DMR"};
        rifle.displayName="AR-41 CARBINE";pistol.displayName="P9 SIDEARM";smg.displayName="VX-9 COMPACT";dmr.displayName="M12 MARKSMAN";
        for(int i=0;i<4;i++){var w=defs[i];w.id=ids[i];w.price=i<2?0:i==2?350:700;w.initiallyOwned=i<2;w.shootingSound=Clip(audio[i]+"Fire");w.reloadSound=Clip(audio[i]+"Reload");w.emptySound=Clip(audio[i]+"Empty");w.switchSound=Clip(audio[i]+"Switch");EditorUtility.SetDirty(w);}
        smg.damage=19;smg.fireRate=13;smg.magazineCapacity=36;smg.reserveAmmunition=180;smg.reloadDuration=1.65f;smg.hasScope=false;smg.recoil=.4f;
        dmr.damage=58;dmr.fireRate=2.7f;dmr.automatic=false;dmr.magazineCapacity=12;dmr.reserveAmmunition=72;dmr.reloadDuration=2.5f;dmr.hasScope=true;dmr.scopedFOV=23;dmr.recoil=1.4f;dmr.spread=.003f;
        pistol.reloadDuration=1.45f;rifle.reloadDuration=2;
        catalog.weapons=defs;
        string[] paths={"Assets/Scenes/Compound.unity","Assets/Scenes/FreightDepot.unity","Assets/Scenes/RelayStation.unity"};
        string[] titles={"OUTPOST 17","FREIGHT DEPOT","RELAY STATION"};
        catalog.levels=new LevelConfiguration[3];
        for(int i=0;i<3;i++){var l=Asset("Assets/Configuration/Level"+(i+1)+".asset",()=>ScriptableObject.CreateInstance<LevelConfiguration>());l.title=titles[i];l.briefing=i==0?"Breach the outpost and secure the northern extraction.":i==1?"Clear the freight lanes. Use cover to cross the yard.":"Silence the relay defenders and secure the station.";l.buildIndex=i;l.defaultWeapon=i==1?pistol:rifle;l.killReward=50+i*10;l.completionReward=250+i*100;catalog.levels[i]=l;EditorUtility.SetDirty(l);}
        var palette=Asset("Assets/Configuration/AudioPalette.asset",()=>ScriptableObject.CreateInstance<AudioPalette>());
        palette.concreteSteps=Enumerable.Range(0,5).Select(i=>Clip("ConcreteStep"+i)).ToArray();palette.grassSteps=Enumerable.Range(0,5).Select(i=>Clip("GrassStep"+i)).ToArray();palette.metalSteps=Enumerable.Range(0,5).Select(i=>Clip("MetalStep"+i)).ToArray();
        palette.concreteImpact=Clip("ConcreteImpact");palette.grassImpact=Clip("GrassImpact");palette.metalImpact=Clip("MetalImpact");palette.fleshImpact=Clip("FleshImpact");palette.landing=Clip("Landing");palette.ambience=Clip("Ambience");palette.hurt=Clip("Hurt");palette.enemyDeath=Clip("EnemyDeath");palette.button=Clip("Button");EditorUtility.SetDirty(palette);
        var source=GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;catalog.graphicsPresets=new UniversalRenderPipelineAsset[3];
        for(int i=0;i<3;i++){var rp=Asset("Assets/Configuration/Graphics"+GraphicsPresets.Names[i]+".asset",()=>Object.Instantiate(source));rp.renderScale=i==0?.7f:i==1?.85f:1;rp.shadowDistance=i==0?20:i==1?35:55;rp.msaaSampleCount=i==2?4:2;rp.mainLightShadowmapResolution=i==0?1024:2048;rp.supportsHDR=i>0;catalog.graphicsPresets[i]=rp;EditorUtility.SetDirty(rp);}
        var sky=Asset("Assets/Materials/Atmospheric sky.mat",()=>new Material(Shader.Find("Skybox/Procedural")));sky.SetColor("_SkyTint",new Color(.5f,.61f,.72f));sky.SetColor("_GroundColor",new Color(.25f,.28f,.31f));sky.SetFloat("_AtmosphereThickness",.8f);sky.SetFloat("_SunSize",.025f);sky.SetFloat("_Exposure",1.15f);EditorUtility.SetDirty(sky);
        var grass=Mat("Dry grass",new Color(.27f,.32f,.17f));var metal=Mat("Brushed steel",new Color(.27f,.31f,.34f));var backdrop=Mat("Distant hills",new Color(.21f,.27f,.28f));var soil=Mat("Surrounding earth",new Color(.35f,.32f,.25f));
        EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();int combined=0;
        for(int i=0;i<3;i++)
        {
            var scene=EditorSceneManager.OpenScene(paths[i]);var game=Object.FindAnyObjectByType<GameManager>();game.catalog=catalog;game.level=catalog.levels[i];game.missionName=titles[i];
            Object.FindAnyObjectByType<AudioManager>().palette=palette;
            var ui=Object.FindAnyObjectByType<MainMenu>();if(!ui.GetComponent<ArmoryScreen>())ui.gameObject.AddComponent<ArmoryScreen>();if(!ui.GetComponent<ResponsiveCanvas>())ui.gameObject.AddComponent<ResponsiveCanvas>();
            var manager=game.weapons;var weapons=manager.weapons.ToList();
            for(int j=2;j<4;j++)if(!weapons.Any(w=>w.definition==defs[j])){var gun=Object.Instantiate(weapons[0],weapons[0].transform.parent);gun.name=defs[j].displayName;gun.definition=defs[j];gun.aimCamera=game.playerCamera;gun.playerLook=game.player.GetComponentInChildren<PlayerLook>();gun.transform.localScale=j==2?new Vector3(.88f,.92f,.72f):new Vector3(1,1,1.25f);if(j==2)foreach(var t in gun.GetComponentsInChildren<Transform>(true))if(t.name.Contains("scope") || t.name.Contains("Scope"))t.gameObject.SetActive(false);gun.gameObject.SetActive(false);weapons.Add(gun);}
            manager.weapons=weapons.ToArray();
            var world=GameObject.Find("Industrial Compound").transform;
            foreach(var col in world.GetComponentsInChildren<Collider>()){var marker=col.GetComponent<SurfaceAudio>()??col.gameObject.AddComponent<SurfaceAudio>();string n=col.name.ToLowerInvariant();marker.surface=n.Contains("grass")?SurfaceKind.Grass:n.Contains("steel") || n.Contains("container") || n.Contains("deck") || n.Contains("ramp") || n.Contains("pipe")?SurfaceKind.Metal:SurfaceKind.Concrete;}
            if(!GameObject.Find("Surface practice grass")){var patch=Box("Surface practice grass",world,new Vector3(-6,.015f,-25),new Vector3(5,.03f,8),grass);patch.AddComponent<SurfaceAudio>().surface=SurfaceKind.Grass;var plate=Box("Surface practice steel",world,new Vector3(6,.025f,-25),new Vector3(5,.05f,8),metal);plate.AddComponent<SurfaceAudio>().surface=SurfaceKind.Metal;}
            if(!GameObject.Find("Landscape"))
            {
                var landscape=new GameObject("Landscape");var ground=Box("Distant ground",landscape.transform,new Vector3(0,-.55f,0),new Vector3(500,1,500),soil);Object.DestroyImmediate(ground.GetComponent<Collider>());
                for(int n=0;n<12;n++){float angle=n*Mathf.PI*2/12;var hill=GameObject.CreatePrimitive(PrimitiveType.Sphere);hill.name="Distant ridge";hill.transform.SetParent(landscape.transform);hill.transform.position=new Vector3(Mathf.Sin(angle)*150,7,Mathf.Cos(angle)*150);hill.transform.localScale=new Vector3(100,30+(n%3)*20,70);hill.GetComponent<Renderer>().sharedMaterial=backdrop;hill.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;Object.DestroyImmediate(hill.GetComponent<Collider>());}
            }
            RenderSettings.skybox=sky;RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.48f,.59f,.70f);RenderSettings.ambientEquatorColor=new Color(.32f,.36f,.4f);RenderSettings.ambientGroundColor=new Color(.17f,.15f,.12f);RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.003f;RenderSettings.fogColor=new Color(.56f,.65f,.71f);
            var sun=GameObject.Find("Sun").GetComponent<Light>();sun.intensity=1.5f;sun.color=new Color(1,.9f,.74f);sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(38+i*7,-35+i*20,0);RenderSettings.sun=sun;
            foreach(var cam in Object.FindObjectsByType<Camera>()){cam.clearFlags=CameraClearFlags.Skybox;cam.farClipPlane=350;}
            foreach(var enemy in Object.FindObjectsByType<EnemyHealth>()){var anim=enemy.GetComponent<SoldierAnimation>();foreach(var part in new[]{enemy.visual,anim.chest,anim.leftLeg,anim.rightLeg})combined+=Combine(part,enemy.visual,anim);}
            if(i==0){PrefabUtility.SaveAsPrefabAsset(game.player.gameObject,"Assets/Prefabs/Characters/Player.prefab");PrefabUtility.SaveAsPrefabAsset(ui.gameObject,"Assets/Prefabs/UI/Interface.prefab");PrefabUtility.SaveAsPrefabAsset(Object.FindObjectsByType<EnemyHealth>()[0].gameObject,"Assets/Prefabs/Enemies/Guard.prefab");for(int j=0;j<4;j++){Preview(weapons[j],defs[j]);PrefabUtility.SaveAsPrefabAsset(weapons[j].gameObject,"Assets/Prefabs/Weapons/"+audio[j]+".prefab");}}
            EditorSceneManager.SaveScene(scene);
        }
        EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();EditorSceneManager.OpenScene(paths[0]);return "Installed catalog, persistent store UI, 4 weapons, 40 audio clips, surface zones, sky/lighting, 3 graphics presets. Combined "+combined+" renderer groups.";
    }
    static int Combine(Transform part,Transform visual,SoldierAnimation anim)
    {
        var renderers=part.GetComponentsInChildren<MeshRenderer>().Where(r=>r.enabled && !r.name.StartsWith("Combined") && (part!=visual || (!r.transform.IsChildOf(anim.chest) && !r.transform.IsChildOf(anim.leftLeg) && !r.transform.IsChildOf(anim.rightLeg)))).ToArray();int count=0;
        foreach(var group in renderers.GroupBy(r=>r.sharedMaterial))
        {
            if(group.Count()<2)continue;string path="Assets/Meshes/Soldier/"+part.name+"_"+group.Key.name+".asset";
            var mesh=Asset(path,()=>{var m=new Mesh();m.indexFormat=IndexFormat.UInt32;m.CombineMeshes(group.Select(r=>new CombineInstance{mesh=r.GetComponent<MeshFilter>().sharedMesh,transform=part.worldToLocalMatrix*r.transform.localToWorldMatrix}).ToArray(),true,true);return m;});
            var node=new GameObject("Combined "+group.Key.name);node.transform.SetParent(part,false);node.AddComponent<MeshFilter>().sharedMesh=mesh;node.AddComponent<MeshRenderer>().sharedMaterial=group.Key;foreach(var r in group)r.enabled=false;count++;
        }
        return count;
    }
    static void Preview(Weapon weapon,WeaponDefinition definition)
    {
        var clone=Object.Instantiate(weapon.gameObject);clone.SetActive(true);clone.transform.SetPositionAndRotation(new Vector3(800,0,0),Quaternion.identity);foreach(var t in clone.GetComponentsInChildren<Transform>(true))t.gameObject.layer=31;
        var renderers=clone.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
        var cg=new GameObject("Preview camera");var camera=cg.AddComponent<Camera>();camera.cullingMask=1<<31;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.07f,.11f,.15f,1);camera.orthographic=true;camera.orthographicSize=Mathf.Max(bounds.extents.y,bounds.extents.z*.6f)*1.4f;camera.nearClipPlane=.01f;camera.farClipPlane=10;camera.transform.position=bounds.center+new Vector3(2,.6f,-.6f);camera.transform.LookAt(bounds.center);
        var rt=new RenderTexture(640,320,24);camera.targetTexture=rt;camera.Render();var prev=RenderTexture.active;RenderTexture.active=rt;var texture=new Texture2D(640,320,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,640,320),0,0);texture.Apply();RenderTexture.active=prev;
        string path="Assets/Art/WeaponPreviews/"+definition.id+".png";File.WriteAllBytes(path,texture.EncodeToPNG());camera.targetTexture=null;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(texture);Object.DestroyImmediate(cg);Object.DestroyImmediate(clone);
        AssetDatabase.ImportAsset(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.mipmapEnabled=false;importer.SaveAndReimport();definition.preview=AssetDatabase.LoadAssetAtPath<Sprite>(path);EditorUtility.SetDirty(definition);
    }
}



