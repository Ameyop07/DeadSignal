using UnityEngine;
namespace CompoundFPS
{
    [CreateAssetMenu(menuName="Compound FPS/Level configuration")]
    public sealed class LevelConfiguration : ScriptableObject
    {
        public string title, briefing;
        public int buildIndex;
        public WeaponDefinition defaultWeapon;
        [Min(0)] public int killReward=50, completionReward=250;
    }
}
