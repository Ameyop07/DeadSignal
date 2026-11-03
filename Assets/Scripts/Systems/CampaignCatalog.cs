using UnityEngine;
namespace CompoundFPS
{
    [CreateAssetMenu(menuName="Compound FPS/Campaign catalog")]
    public sealed class CampaignCatalog : ScriptableObject
    {
        public WeaponDefinition[] weapons;
        public LevelConfiguration[] levels;
        public UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset[] graphicsPresets;
    }
}
