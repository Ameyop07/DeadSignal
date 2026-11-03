using UnityEngine;
using UnityEngine.AI;
namespace CompoundFPS
{
    public sealed class SoldierAnimation : MonoBehaviour
    {
        public Transform leftLeg,rightLeg,chest;
        NavMeshAgent agent;EnemyHealth health;float cycle;
        void Awake(){agent=GetComponent<NavMeshAgent>();health=GetComponent<EnemyHealth>();}
        void LateUpdate()
        {
            if(!CombatEvents.Playing || health.Dead)return;
            float speed=agent.velocity.magnitude;cycle+=Time.deltaTime*speed*3.7f;
            float angle=Mathf.Sin(cycle)*Mathf.Min(25,speed*9);
            if(leftLeg)leftLeg.localRotation=Quaternion.Euler(angle,0,0);
            if(rightLeg)rightLeg.localRotation=Quaternion.Euler(-angle,0,0);
            if(chest)chest.localRotation=Quaternion.Euler(0,Mathf.Sin(cycle)*Mathf.Min(3,speed),0);
        }
    }
}
