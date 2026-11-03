using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using System.Collections.Generic;
using CompoundFPS;
public static class PolishEnvironment
{
    public static string Main()
    {
        var mesh=new Mesh{name="Eroded ridge"};var vertices=new List<Vector3>();var triangles=new List<int>();const int side=13;
        for(int z=0;z<side;z++)for(int x=0;x<side;x++){float px=(float)x/(side-1)-.5f,pz=(float)z/(side-1)-.5f;float h=Mathf.Pow(Mathf.Max(0,1-Mathf.Abs(px)*2),1.5f)*Mathf.Pow(Mathf.Max(0,1-Mathf.Abs(pz)*2),1.2f);h*=.55f+.45f*Mathf.PerlinNoise(x*.48f,z*.61f);vertices.Add(new Vector3(px,h-.2f,pz));if(x<side-1 && z<side-1){int a=z*side+x;triangles.AddRange(new[]{a,a+side,a+1,a+1,a+side,a+side+1});}}
        mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,"Assets/Meshes/Ridge.asset");
        var texture=new Texture2D(128,128,TextureFormat.RGB24,true);texture.name="Concrete grain";texture.wrapMode=TextureWrapMode.Repeat;var colors=new Color[128*128];for(int y=0;y<128;y++)for(int x=0;x<128;x++){float n=.8f+Mathf.PerlinNoise(x*.7f,y*.7f)*.2f;colors[y*128+x]=new Color(n,n,n);}texture.SetPixels(colors);texture.Apply();AssetDatabase.CreateAsset(texture,"Assets/Materials/Concrete grain.asset");
        foreach(var sceneEntry in EditorBuildSettings.scenes)
        {
            var scene=EditorSceneManager.OpenScene(sceneEntry.path);int i=0;
            foreach(var t in GameObject.Find("Landscape").GetComponentsInChildren<Transform>())if(t.name=="Distant ridge"){t.GetComponent<MeshFilter>().sharedMesh=mesh;t.localScale=new Vector3(140,80+i%3*25,100);t.position=new Vector3(t.position.x*1.4f,0,t.position.z*1.4f);i++;}
            var world=GameObject.Find("Industrial Compound");foreach(var r in world.GetComponentsInChildren<MeshRenderer>()){r.sharedMaterial.SetTexture("_BaseMap",texture);r.sharedMaterial.SetTextureScale("_BaseMap",new Vector2(4,4));EditorUtility.SetDirty(r.sharedMaterial);}
            EditorSceneManager.SaveScene(scene);
        }
        PlayerSettings.enableFrameTimingStats=true;
        AssetDatabase.SaveAssets();EditorSceneManager.OpenScene("Assets/Scenes/Compound.unity");return "Mountain ridges and subtle surface grain applied; frame timing enabled.";
    }
}
