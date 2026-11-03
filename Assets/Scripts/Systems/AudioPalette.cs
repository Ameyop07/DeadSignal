using UnityEngine;
namespace CompoundFPS
{
    public enum SurfaceKind { Concrete, Grass, Metal }
    public enum WeaponSound { Fire, Reload, Empty, Switch }
    [CreateAssetMenu(menuName="Compound FPS/Audio palette")]
    public sealed class AudioPalette : ScriptableObject
    {
        public AudioClip[] concreteSteps, grassSteps, metalSteps;
        public AudioClip concreteImpact, grassImpact, metalImpact, fleshImpact, landing, ambience, button, hurt, enemyDeath;
    }
}
