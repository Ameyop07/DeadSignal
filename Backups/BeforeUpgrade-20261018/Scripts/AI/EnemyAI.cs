using UnityEngine;
using UnityEngine.AI;
namespace CompoundFPS
{
    [RequireComponent(typeof(NavMeshAgent),typeof(EnemyHealth),typeof(EnemyWeapon))]
    public sealed class EnemyAI : MonoBehaviour
    {
        public enum State { Patrol, Alert, Chase, Attack, Search, Dead }
        public State CurrentState {get;private set;}
        public PlayerHealth player;
        public PlayerController playerController;
        public Vector3[] waypoints;
        public float detectionDistance=24, fieldOfView=110, hearingDistance=28, movementSpeed=3.2f, attackRange=18;
        NavMeshAgent agent;
        EnemyHealth health;
        EnemyWeapon weapon;
        Vector3 lastKnown;
        float thinkAt, stateSince, lastSeen;
        int waypoint;
        void Awake(){agent=GetComponent<NavMeshAgent>();health=GetComponent<EnemyHealth>();weapon=GetComponent<EnemyWeapon>();}
        void OnEnable(){CombatEvents.Noise+=Hear;}
        void OnDisable(){CombatEvents.Noise-=Hear;}
        void Start(){agent.enabled=true;agent.speed=movementSpeed;agent.updateRotation=false;health.Damaged+=OnHurt;thinkAt=Time.time+Random.Range(0,.2f);}
        void OnDestroy(){if(health)health.Damaged-=OnHurt;}
        void OnHurt(float damage,Vector3 source){if(!health.Dead){lastKnown=source;Change(State.Alert);}}
        void Hear(Vector3 position,float radius)
        {
            if(health.Dead || !CombatEvents.Playing || Vector3.Distance(position,transform.position)>Mathf.Min(radius,hearingDistance))return;
            lastKnown=position;
            if(CurrentState==State.Patrol || CurrentState==State.Search)Change(State.Alert);
        }
        public bool CanSeePlayer()
        {
            if(!player || player.Dead)return false;
            Vector3 eye=transform.position+Vector3.up*1.55f;
            Vector3 target=player.transform.position+Vector3.up*(playerController && playerController.Crouched?.85f:1.35f);
            Vector3 delta=target-eye;
            if(delta.magnitude>detectionDistance || Vector3.Angle(transform.forward,delta)>fieldOfView*.5f)return false;
            return Physics.Raycast(eye,delta.normalized,out var hit,delta.magnitude+.3f,~0,QueryTriggerInteraction.Ignore) && hit.collider.GetComponentInParent<PlayerHealth>()==player;
        }
        void Update()
        {
            if(!agent.isOnNavMesh)return;
            if(health.Dead){CurrentState=State.Dead;agent.isStopped=true;return;}
            if(!CombatEvents.Playing){agent.isStopped=true;return;}
            if(Time.time<thinkAt)return;
            thinkAt=Time.time+.18f;
            bool sees=CanSeePlayer();
            if(sees)
            {
                lastKnown=player.transform.position;lastSeen=Time.time;
                if(CurrentState==State.Patrol)Change(State.Alert);
                else if(CurrentState!=State.Alert || Time.time-stateSince>.6f)
                    Change(Vector3.Distance(transform.position,lastKnown)<=attackRange?State.Attack:State.Chase);
            }
            switch(CurrentState)
            {
                case State.Patrol:
                    if(waypoints==null || waypoints.Length==0)break;
                    if(!agent.hasPath || (!agent.pathPending && agent.remainingDistance<.6f))waypoint=(waypoint+1)%waypoints.Length;
                    Go(waypoints[waypoint]); Face(agent.steeringTarget);break;
                case State.Alert:
                    agent.isStopped=true;Face(lastKnown);
                    if(Time.time-stateSince>.7f)Change(State.Chase);break;
                case State.Chase:
                    Go(lastKnown);Face(agent.steeringTarget);
                    if(!sees && Vector3.Distance(transform.position,lastKnown)<2)Change(State.Search);
                    else if(!sees && Time.time-lastSeen>8)Change(State.Search);break;
                case State.Attack:
                    agent.isStopped=true;Face(lastKnown);
                    if(!sees){Change(State.Chase);break;}
                    if(Vector3.Distance(transform.position,lastKnown)<5)
                    {
                        Vector3 retreat=transform.position+(transform.position-lastKnown).normalized*3;
                        if(NavMesh.SamplePosition(retreat,out var nav,2,NavMesh.AllAreas))Go(nav.position);
                    }
                    weapon.Fire(player,player.transform.position+Vector3.up*(playerController.Crouched?.75f:1.15f));break;
                case State.Search:
                    agent.isStopped=true;transform.Rotate(0,25*.18f,0);
                    if(Time.time-stateSince>6)Change(State.Patrol);break;
            }
        }
        void Go(Vector3 p){agent.isStopped=false;agent.stoppingDistance=.45f;agent.SetDestination(p);}
        void Face(Vector3 p){Vector3 d=p-transform.position;d.y=0;if(d.sqrMagnitude>.01f)transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(d),45);}
        void Change(State state){if(CurrentState==state)return;CurrentState=state;stateSince=Time.time;}
    }
}
