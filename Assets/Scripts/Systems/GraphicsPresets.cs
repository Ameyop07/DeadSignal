using UnityEngine;
namespace CompoundFPS
{
    public static class GraphicsPresets
    {
        public static readonly string[] Names={"LOW","MEDIUM","HIGH"};
        public static int Current=>Mathf.Clamp(PlayerPrefs.GetInt("FPS.GraphicsPreset",1),0,2);
        public static void Apply(int index,CampaignCatalog catalog,bool save=true)
        {
            index=Mathf.Clamp(index,0,2);if(catalog && catalog.graphicsPresets!=null && catalog.graphicsPresets.Length>index)QualitySettings.renderPipeline=catalog.graphicsPresets[index];
            QualitySettings.vSyncCount=0;QualitySettings.lodBias=index==0?.65f:index==1?1:1.5f;QualitySettings.globalTextureMipmapLimit=index==0?1:0;Application.targetFrameRate=60;
            if(save){PlayerPrefs.SetInt("FPS.GraphicsPreset",index);PlayerPrefs.Save();}
        }
    }
}
