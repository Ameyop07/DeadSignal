using System;
using UnityEngine;
namespace CompoundFPS
{
    public class Damageable : MonoBehaviour, IDamageable
    {
        public float maximumHealth=100;
        public float Health {get;private set;}
        public bool Dead => Health<=0;
        public event Action<float,Vector3> Damaged;
        public event Action Died;
        protected virtual void Awake(){Health=maximumHealth;}
        public virtual bool TakeDamage(float amount,Vector3 origin)
        {
            if(Dead || amount<=0)return false;
            Health=Mathf.Max(0,Health-amount); Damaged?.Invoke(amount,origin);
            if(Dead){OnDeath();Died?.Invoke();return true;}return false;
        }
        protected virtual void OnDeath(){}
        public void Restore(){Health=maximumHealth;}
    }
}
