using UnityEngine;
namespace CompoundFPS
{
    public sealed class EnemyWeapon : MonoBehaviour
    {
        public float fireRate=1.1f, accuracy=.32f, damage=8;
        public Transform muzzle;
        float nextFire;
        public void Fire(PlayerHealth target,Vector3 aimPoint)
        {
            if(!CombatEvents.Playing || Time.time<nextFire || target.Dead)return;
            nextFire=Time.time+1/fireRate;
            Vector3 start=muzzle?muzzle.position:transform.position+Vector3.up*1.5f;
            Vector3 direction=aimPoint-start;
            // A second LOS test at the muzzle prevents firing through low cover.
            if(Physics.Raycast(start,direction.normalized,out var hit,direction.magnitude+0.2f,~0,QueryTriggerInteraction.Ignore) && hit.collider.GetComponentInParent<PlayerHealth>()==target)
            {
                CombatEvents.Play(SoundCue.EnemyShot,start);
                CombatEvents.Spark(start,-direction.normalized,false);
                float chance=accuracy*Mathf.Lerp(1.2f,.6f,Mathf.Clamp01(direction.magnitude/25));
                if(Random.value<chance) target.TakeDamage(damage,start);
            }
        }
    }
}
