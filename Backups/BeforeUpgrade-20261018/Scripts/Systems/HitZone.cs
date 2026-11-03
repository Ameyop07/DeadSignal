using UnityEngine;
namespace CompoundFPS
{
    public sealed class HitZone : MonoBehaviour, IDamageable
    {
        public Damageable owner;
        public float multiplier=2;
        public bool TakeDamage(float damage,Vector3 origin)=>owner && owner.TakeDamage(damage*multiplier,origin);
    }
}
