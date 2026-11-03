using UnityEngine;
namespace CompoundFPS
{
    public sealed class PlayerHealth : Damageable
    {
        public float DamageFlash {get;private set;}
        public Vector3 LastDamageOrigin {get;private set;}
        public override bool TakeDamage(float amount,Vector3 origin)
        {
            if(!CombatEvents.Playing || Dead)return false;
            LastDamageOrigin=origin; DamageFlash=1; CombatEvents.Play(SoundCue.Hurt,transform.position);
            return base.TakeDamage(amount,origin);
        }
        void Update(){DamageFlash=Mathf.MoveTowards(DamageFlash,0,Time.deltaTime*2.5f);}
        protected override void OnDeath(){CombatEvents.State=GameState.Lost;}
    }
}
