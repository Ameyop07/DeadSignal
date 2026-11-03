using UnityEngine;
namespace CompoundFPS
{
    public sealed class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance {get;private set;}
        public AudioPalette palette;
        public int voiceCount=24;
        AudioSource[] voices;AudioSource ambience;int next,lastStep=-1;
        public int FootstepsPlayed {get;private set;}
        public int LandingsPlayed {get;private set;}
        public SurfaceKind LastSurface {get;private set;}
        void Awake()
        {
            Instance=this;voices=new AudioSource[voiceCount];
            for(int i=0;i<voiceCount;i++){var g=new GameObject("Pooled voice "+i);g.transform.SetParent(transform);var s=voices[i]=g.AddComponent<AudioSource>();s.playOnAwake=false;s.minDistance=3;s.maxDistance=50;s.rolloffMode=AudioRolloffMode.Linear;}
            ambience=gameObject.AddComponent<AudioSource>();ambience.playOnAwake=false;ambience.loop=true;if(palette){ambience.clip=palette.ambience;ambience.Play();}
        }
        void OnEnable(){CombatEvents.Sound+=Play;}
        void OnDisable(){CombatEvents.Sound-=Play;}
        void OnDestroy(){if(Instance==this)Instance=null;}
        void Update(){ambience.volume=GameSettings.Music*.45f;}
        public void PlayClip(AudioClip clip,Vector3 pos,bool spatial=false,float volume=.6f,float pitch=1)
        {
            if(!clip || voices==null)return;var s=voices[next++%voices.Length];s.Stop();s.transform.position=pos;s.clip=clip;s.spatialBlend=spatial?1:0;s.pitch=pitch;s.volume=volume*GameSettings.SFX;s.Play();
        }
        public void PlayWeapon(WeaponDefinition w,WeaponSound sound,Vector3 pos)
        {
            var clip=sound==WeaponSound.Fire?w.shootingSound:sound==WeaponSound.Reload?w.reloadSound:sound==WeaponSound.Empty?w.emptySound:w.switchSound;
            PlayClip(clip,pos,false,sound==WeaponSound.Fire?.65f:.5f,Random.Range(.97f,1.03f));
        }
        public void PlayFootstep(SurfaceKind surface,Vector3 position,bool sprint=false,bool crouched=false)
        {
            if(!palette)return;var clips=surface==SurfaceKind.Grass?palette.grassSteps:surface==SurfaceKind.Metal?palette.metalSteps:palette.concreteSteps;
            if(clips==null || clips.Length==0)return;int i=Random.Range(0,clips.Length);if(clips.Length>1 && i==lastStep)i=(i+1)%clips.Length;lastStep=i;LastSurface=surface;FootstepsPlayed++;
            PlayClip(clips[i],position,false,crouched?.16f:sprint?.48f:.32f,Random.Range(.93f,1.07f));
        }
        public void PlayLanding(SurfaceKind surface,Vector3 position){LandingsPlayed++;PlayClip(palette.landing,position,false,.5f);PlayFootstep(surface,position);}
        public void PlayImpact(RaycastHit hit,bool flesh)
        {
            var marker=hit.collider.GetComponentInParent<SurfaceAudio>();var kind=marker?marker.surface:SurfaceKind.Concrete;
            PlayClip(flesh?palette.fleshImpact:kind==SurfaceKind.Metal?palette.metalImpact:kind==SurfaceKind.Grass?palette.grassImpact:palette.concreteImpact,hit.point,true,.5f,Random.Range(.9f,1.1f));
        }
        void Play(SoundCue cue,Vector3 p)
        {
            if(!palette)return;
            if(cue==SoundCue.Footstep){PlayFootstep(SurfaceKind.Concrete,p);return;}
            if(cue==SoundCue.EnemyShot){var g=GameManager.Instance;if(g && g.catalog)PlayClip(g.catalog.weapons[0].shootingSound,p,true,.45f);return;}
            PlayClip(cue==SoundCue.Hurt?palette.hurt:cue==SoundCue.EnemyDeath?palette.enemyDeath:palette.button,p,cue==SoundCue.EnemyDeath,.3f);
        }
    }
}
