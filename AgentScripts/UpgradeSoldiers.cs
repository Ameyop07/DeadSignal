using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using CompoundFPS;
using System.Linq;

public static class UpgradeSoldiers
{
    static Material cloth,vest,skin,boots,metal,eyes;
    static Material Mat(string name,Color color)
    {
        string path="Assets/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.color=color;m.SetFloat("_Smoothness",.25f);AssetDatabase.CreateAsset(m,path);}return m;
    }
    static Transform Node(string name,Transform parent,Vector3 pos){var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=pos;return t;}
    static GameObject Shape(string name,PrimitiveType type,Transform parent,Vector3 pos,Vector3 size,Material mat)
    {
        var g=GameObject.CreatePrimitive(type);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=pos;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=mat;Object.DestroyImmediate(g.GetComponent<Collider>());return g;
    }
    static void Limb(string name,Transform parent,Vector3 from,Vector3 to,float radius,Material mat)
    {
        var g=Shape(name,PrimitiveType.Capsule,parent,(from+to)*.5f,new Vector3(radius*2,(to-from).magnitude*.5f,radius*2),mat);g.transform.localRotation=Quaternion.FromToRotation(Vector3.up,to-from);
    }
    public static string Main()
    {
        if(EditorApplication.isPlaying)throw new System.Exception("Stop play mode");
        cloth=Mat("Soldier fatigues",new Color(.24f,.28f,.19f));vest=Mat("Tactical webbing",new Color(.19f,.17f,.12f));skin=Mat("Skin",new Color(.57f,.36f,.24f));boots=Mat("Boot leather",new Color(.075f,.065f,.05f));metal=Mat("Gunmetal",new Color(.065f,.08f,.085f));eyes=Mat("Goggle glass",new Color(.09f,.16f,.18f));
        foreach(var enemy in Object.FindObjectsByType<EnemyHealth>())
        {
            if(enemy.visual)Object.DestroyImmediate(enemy.visual.gameObject);
            var visual=Node("Soldier",enemy.transform,Vector3.zero);enemy.visual=visual;
            var hips=Shape("Pelvis",PrimitiveType.Sphere,visual,new Vector3(0,.92f,0),new Vector3(.4f,.28f,.25f),cloth);
            var chest=Node("Chest",visual,new Vector3(0,1.2f,0));
            Shape("Torso",PrimitiveType.Capsule,chest,Vector3.zero,new Vector3(.43f,.26f,.27f),cloth);
            Shape("Plate carrier",PrimitiveType.Cube,chest,new Vector3(0,.035f,.02f),new Vector3(.41f,.37f,.32f),vest);
            Shape("Backpack",PrimitiveType.Capsule,chest,new Vector3(0,-.02f,-.23f),new Vector3(.31f,.22f,.19f),vest);
            for(int i=-1;i<=1;i++)Shape("Magazine pouch",PrimitiveType.Cube,chest,new Vector3(i*.115f,-.04f,.205f),new Vector3(.10f,.17f,.08f),cloth);
            for(int s=-1;s<=1;s+=2)
            {
                Shape("Shoulder strap",PrimitiveType.Cube,chest,new Vector3(s*.15f,.24f,.02f),new Vector3(.08f,.08f,.33f),vest);
                Shape("Belt pouch",PrimitiveType.Cube,visual,new Vector3(s*.23f,.94f,0),new Vector3(.12f,.18f,.16f),vest);
                Limb("Neck",visual,new Vector3(0,1.43f,0),new Vector3(0,1.58f,0),.065f,skin);
            }
            var head=Shape("Head",PrimitiveType.Sphere,visual,new Vector3(0,1.66f,.015f),new Vector3(.245f,.31f,.245f),skin);
            head.AddComponent<SphereCollider>();var zone=head.AddComponent<HitZone>();zone.owner=enemy;zone.multiplier=2;
            Shape("Jaw",PrimitiveType.Sphere,visual,new Vector3(0,1.565f,.055f),new Vector3(.17f,.12f,.17f),skin);
            Shape("Nose",PrimitiveType.Sphere,visual,new Vector3(0,1.65f,.141f),new Vector3(.043f,.065f,.055f),skin);
            Shape("Mouth",PrimitiveType.Cube,visual,new Vector3(0,1.59f,.137f),new Vector3(.065f,.012f,.008f),vest);
            Shape("Helmet shell",PrimitiveType.Sphere,visual,new Vector3(0,1.79f,-.005f),new Vector3(.29f,.20f,.3f),cloth);
            Shape("Helmet rim",PrimitiveType.Cylinder,visual,new Vector3(0,1.735f,.008f),new Vector3(.31f,.018f,.32f),vest);
            for(int s=-1;s<=1;s+=2)
            {
                Shape("Ballistic goggle",PrimitiveType.Sphere,visual,new Vector3(s*.055f,1.69f,.125f),new Vector3(.092f,.055f,.045f),eyes);
                Shape("Earpiece",PrimitiveType.Sphere,visual,new Vector3(s*.135f,1.67f,-.01f),new Vector3(.06f,.105f,.08f),vest);
            }
            Limb("Radio antenna",chest,new Vector3(-.19f,.1f,-.2f),new Vector3(-.19f,.53f,-.2f),.008f,metal);
            var anim=enemy.GetComponent<SoldierAnimation>()??enemy.gameObject.AddComponent<SoldierAnimation>();anim.chest=chest;
            for(int s=-1;s<=1;s+=2)
            {
                var leg=Node(s<0?"Left leg":"Right leg",visual,new Vector3(s*.12f,.9f,0));if(s<0)anim.leftLeg=leg;else anim.rightLeg=leg;
                Limb("Thigh",leg,Vector3.zero,new Vector3(0,-.37f,.01f),.105f,cloth);
                Limb("Shin",leg,new Vector3(0,-.37f,.01f),new Vector3(0,-.75f,-.01f),.077f,cloth);
                Shape("Knee pad",PrimitiveType.Sphere,leg,new Vector3(0,-.38f,.08f),new Vector3(.16f,.19f,.075f),vest);
                Shape("Boot",PrimitiveType.Capsule,leg,new Vector3(0,-.79f,.06f),new Vector3(.16f,.10f,.29f),boots);
                Vector3 shoulder=new Vector3(s*.27f,1.39f,0),elbow=new Vector3(s*.32f,1.12f,.12f),hand=new Vector3(s<0?.02f:.16f,1.2f,s<0?.45f:.28f);
                Limb("Sleeve",visual,shoulder,elbow,.085f,cloth);Limb("Forearm",visual,elbow,hand,.065f,cloth);
                Shape("Glove",PrimitiveType.Sphere,visual,hand,new Vector3(.09f,.10f,.12f),boots);
            }
            Shape("Carbine receiver",PrimitiveType.Cube,visual,new Vector3(.13f,1.23f,.35f),new Vector3(.09f,.12f,.38f),metal);
            Shape("Rifle stock",PrimitiveType.Cube,visual,new Vector3(.13f,1.23f,.1f),new Vector3(.08f,.12f,.2f),vest);
            Limb("Rifle barrel",visual,new Vector3(.13f,1.25f,.5f),new Vector3(.13f,1.25f,.8f),.022f,metal);
            Shape("Rifle magazine",PrimitiveType.Cube,visual,new Vector3(.13f,1.12f,.34f),new Vector3(.055f,.18f,.09f),metal);
            var body=Node("Body hitbox",enemy.transform,new Vector3(0,.87f,0)).gameObject;var capsule=body.AddComponent<CapsuleCollider>();capsule.radius=.25f;capsule.height=1.35f;
            enemy.GetComponent<EnemyWeapon>().muzzle.localPosition=new Vector3(.13f,1.25f,.84f);
        }
        var rifle=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Prefabs/Weapons/CarbineDefinition.asset");rifle.hasScope=true;rifle.scopedFOV=28;EditorUtility.SetDirty(rifle);
        var player=Object.FindAnyObjectByType<PlayerController>();var weapon=player.GetComponent<WeaponManager>().weapons[0];
        var optic=Shape("3x scope housing",PrimitiveType.Cylinder,weapon.transform,new Vector3(0,.145f,.08f),new Vector3(.09f,.14f,.09f),metal);optic.transform.localRotation=Quaternion.Euler(90,0,0);optic.layer=2;
        Shape("Scope objective lens",PrimitiveType.Sphere,weapon.transform,new Vector3(0,.145f,.225f),new Vector3(.077f,.077f,.012f),eyes).layer=2;
        foreach(var sight in weapon.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Sight")).ToArray())Object.DestroyImmediate(sight.gameObject);
        var ui=Object.FindAnyObjectByType<MainMenu>();if(!ui.GetComponent<ScopeOverlay>())ui.gameObject.AddComponent<ScopeOverlay>();
        Object.FindAnyObjectByType<ImpactPool>().capacity=96;
        foreach(var label in Object.FindObjectsByType<TMPro.TextMeshPro>())label.fontSize*=6;
        PrefabUtility.SaveAsPrefabAsset(player.gameObject,"Assets/Prefabs/Characters/Player.prefab");
        PrefabUtility.SaveAsPrefabAsset(weapon.gameObject,"Assets/Prefabs/Weapons/Carbine.prefab");
        PrefabUtility.SaveAsPrefabAsset(Object.FindObjectsByType<EnemyHealth>()[0].gameObject,"Assets/Prefabs/Enemies/Guard.prefab");
        PrefabUtility.SaveAsPrefabAsset(ui.gameObject,"Assets/Prefabs/UI/Interface.prefab");
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        return "Upgraded eight soldiers with anatomical silhouettes, face/helmet/gear, gait animation and head hitboxes. Added rifle optic and scope UI.";
    }
}
