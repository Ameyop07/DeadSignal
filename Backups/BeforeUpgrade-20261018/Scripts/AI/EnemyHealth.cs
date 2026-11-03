using UnityEngine;
namespace CompoundFPS
{
    public sealed class EnemyHealth : Damageable
    {
        public Transform visual;
        float deathTime;
        protected override void OnDeath()
        {
            deathTime=Time.time;
            foreach(var c in GetComponentsInChildren<Collider>())c.enabled=false;
            CombatEvents.Play(SoundCue.EnemyDeath,transform.position);
            CombatEvents.Kill();
        }
        void Update()
        {
            if(!Dead || !visual)return;
            visual.localRotation=Quaternion.Slerp(visual.localRotation,Quaternion.Euler(85,0,15),Time.deltaTime*5);
            visual.localPosition=Vector3.Lerp(visual.localPosition,Vector3.up*.2f,Time.deltaTime*3);
            if(Time.time-deathTime>15)visual.gameObject.SetActive(false);
        }
    }
}
