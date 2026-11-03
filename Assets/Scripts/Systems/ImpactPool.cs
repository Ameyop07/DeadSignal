using UnityEngine;
namespace CompoundFPS
{
    public sealed class ImpactPool : MonoBehaviour
    {
        public int capacity=96, bloodMarkCapacity=24;
        public Material impactMaterial;
        GameObject[] particles,marks;float[] until,markUntil;Vector3[] velocity;
        Material spark,blood;Mesh splatter;int next,markNext;
        void Awake()
        {
            spark=new Material(impactMaterial);spark.color=new Color(1,.7f,.2f);
            blood=new Material(impactMaterial);blood.color=new Color(.38f,.018f,.012f);blood.SetFloat("_Cull",0);
            particles=new GameObject[capacity];until=new float[capacity];velocity=new Vector3[capacity];
            for(int i=0;i<capacity;i++)particles[i]=Create(PrimitiveType.Sphere,"Impact particle",spark);
            marks=new GameObject[bloodMarkCapacity];markUntil=new float[bloodMarkCapacity];
            splatter=new Mesh{name="Irregular blood splatter"};
            var vertices=new Vector3[19];var triangles=new int[54];
            for(int i=0;i<18;i++){float angle=i*Mathf.PI*2/18;float radius=i%3==0?.65f:(i%2==0?.42f:.29f);vertices[i+1]=new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),0)*radius;triangles[i*3]=0;triangles[i*3+1]=i+1;triangles[i*3+2]=(i+1)%18+1;}
            splatter.vertices=vertices;splatter.triangles=triangles;splatter.RecalculateNormals();splatter.RecalculateBounds();
            for(int i=0;i<marks.Length;i++){marks[i]=Create(PrimitiveType.Quad,"Blood mark",blood);marks[i].GetComponent<MeshFilter>().sharedMesh=splatter;}
        }
        GameObject Create(PrimitiveType primitive,string name,Material mat)
        {
            var g=GameObject.CreatePrimitive(primitive);g.name=name;var c=g.GetComponent<Collider>();c.enabled=false;Destroy(c);g.transform.SetParent(transform);g.layer=2;g.GetComponent<Renderer>().sharedMaterial=mat;g.SetActive(false);return g;
        }
        void OnEnable(){CombatEvents.Impact+=Show;}
        void OnDisable(){CombatEvents.Impact-=Show;}
        void Show(Vector3 point,Vector3 normal,bool flesh)
        {
            for(int j=0;j<(flesh?7:3);j++)
            {
                int i=next++%capacity;var g=particles[i];g.transform.position=point+normal*.04f;g.transform.localScale=Vector3.one*(flesh?Random.Range(.035f,.085f):.035f);g.GetComponent<Renderer>().sharedMaterial=flesh?blood:spark;
                velocity[i]=normal*Random.Range(.5f,1.5f)+Random.insideUnitSphere*(flesh?1.3f:2);until[i]=Time.time+(flesh?.55f:.16f);g.SetActive(true);
            }
            if(!flesh)return;
            var hits=Physics.RaycastAll(point+normal*.45f+Vector3.up*.1f,Vector3.down,4,~(1<<2),QueryTriggerInteraction.Ignore);
            foreach(var hit in hits)
            {
                if(hit.collider.GetComponentInParent<IDamageable>()!=null)continue;
                int i=markNext++%marks.Length;var g=marks[i];g.transform.position=hit.point+hit.normal*.012f;g.transform.rotation=Quaternion.LookRotation(hit.normal)*Quaternion.Euler(0,0,Random.Range(0,360));g.transform.localScale=new Vector3(Random.Range(.18f,.35f),Random.Range(.15f,.3f),1);markUntil[i]=Time.time+12;g.SetActive(true);break;
            }
        }
        void Update()
        {
            for(int i=0;i<particles.Length;i++)if(particles[i].activeSelf){if(Time.time>until[i])particles[i].SetActive(false);else{velocity[i]+=Vector3.down*3*Time.deltaTime;particles[i].transform.position+=velocity[i]*Time.deltaTime;}}
            for(int i=0;i<marks.Length;i++)if(marks[i].activeSelf && Time.time>markUntil[i])marks[i].SetActive(false);
        }
        void OnDestroy(){Destroy(spark);Destroy(blood);Destroy(splatter);}
    }
}
