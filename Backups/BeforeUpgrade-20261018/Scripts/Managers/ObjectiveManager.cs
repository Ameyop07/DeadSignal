using UnityEngine;
namespace CompoundFPS
{
    public sealed class ObjectiveManager : MonoBehaviour
    {
        public Transform player, extraction;
        public int requiredEnemies=8;
        public int Eliminated {get;private set;}
        public bool Entered {get;private set;}
        public bool Complete {get;private set;}
        public string CurrentObjective => !Entered?"01 / ENTER THE COMPOUND":Eliminated<requiredEnemies?$"02 / ELIMINATE GUARDS   {Eliminated} / {requiredEnemies}":"03 / REACH EXTRACTION";
        void OnEnable(){CombatEvents.EnemyKilled+=OnKill;}
        void OnDisable(){CombatEvents.EnemyKilled-=OnKill;}
        void OnKill(){Eliminated=Mathf.Min(requiredEnemies,Eliminated+1);}
        void Update()
        {
            if(!CombatEvents.Playing || !player)return;
            if(player.position.z>-15)Entered=true;
            Vector3 delta=player.position-extraction.position;delta.y=0;
            if(Entered && Eliminated>=requiredEnemies && delta.magnitude<3.5f){Complete=true;CombatEvents.State=GameState.Won;}
        }
    }
}
