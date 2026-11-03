using System.Collections.Generic;
using UnityEngine;
namespace CompoundFPS
{
    public sealed class AudioManager : MonoBehaviour
    {
        public int voiceCount=24;
        public string defaultFootstepSurface="Concrete";
        readonly Dictionary<SoundCue,AudioClip> clips=new Dictionary<SoundCue,AudioClip>();
        readonly Dictionary<string,AudioClip> footsteps=new Dictionary<string,AudioClip>();
        AudioSource[] voices;
        AudioSource ambience;
        int next;
        void Awake()
        {
            foreach(SoundCue cue in System.Enum.GetValues(typeof(SoundCue)))clips[cue]=Synthesize(cue);
            footsteps[defaultFootstepSurface]=clips[SoundCue.Footstep];
            voices=new AudioSource[voiceCount];
            // Voices need independent positions for concurrent spatial sounds.
            for(int i=0;i<voiceCount;i++){var node=new GameObject("Audio Voice "+i);node.transform.SetParent(transform);voices[i]=node.AddComponent<AudioSource>();voices[i].playOnAwake=false;voices[i].minDistance=3;voices[i].maxDistance=45;voices[i].rolloffMode=AudioRolloffMode.Linear;}
            ambience=gameObject.AddComponent<AudioSource>();ambience.playOnAwake=false;ambience.loop=true;ambience.clip=Ambient();ambience.Play();
        }
        void OnEnable(){CombatEvents.Sound+=Play;}
        void OnDisable(){CombatEvents.Sound-=Play;}
        void Update(){if(ambience)ambience.volume=GameSettings.Music*.25f;}
        public void RegisterFootstepSurface(string surface,AudioClip clip){footsteps[surface]=clip;}
        public void PlayFootstep(string surface,Vector3 position){PlayClip(footsteps.TryGetValue(surface,out var clip)?clip:clips[SoundCue.Footstep],position,false,.25f);}
        void Play(SoundCue cue,Vector3 position)
        {
            if(voices==null)return;
            if(cue==SoundCue.Footstep){PlayFootstep(defaultFootstepSurface,position);return;}
            bool spatial=cue==SoundCue.EnemyShot || cue==SoundCue.EnemyDeath;
            PlayClip(clips[cue],position,spatial,cue==SoundCue.Rifle?.5f:cue==SoundCue.Pistol?.55f:cue==SoundCue.EnemyShot?.4f:.3f);
        }
        void PlayClip(AudioClip clip,Vector3 position,bool spatial,float volume)
        {
            var source=voices[next++%voices.Length];source.Stop();source.transform.position=position;source.spatialBlend=spatial?1:0;source.pitch=Random.Range(.96f,1.04f);source.clip=clip;source.volume=volume*GameSettings.SFX;source.Play();
        }
        AudioClip Synthesize(SoundCue cue)
        {
            const int rate=22050;float duration=cue==SoundCue.Reload?.65f:cue==SoundCue.EnemyDeath?.35f:cue==SoundCue.Rifle?.2f:.14f;
            var samples=new float[(int)(rate*duration)];var rng=new System.Random(91+(int)cue);
            for(int i=0;i<samples.Length;i++)
            {
                float t=(float)i/rate;float n=(float)rng.NextDouble()*2-1;float env=Mathf.Exp(-t*(cue==SoundCue.Reload?7:30));float tone=0;
                switch(cue)
                {
                    case SoundCue.Rifle:case SoundCue.EnemyShot: tone=(n*.8f+Mathf.Sin(t*2*Mathf.PI*85)*.4f)*env;break;
                    case SoundCue.Pistol:tone=(n*.7f+Mathf.Sin(t*2*Mathf.PI*140)*.5f)*env;break;
                    case SoundCue.Reload:tone=n*.25f*(Mathf.Exp(-t*40)+Mathf.Exp(-Mathf.Abs(t-.35f)*90));break;
                    case SoundCue.Empty:tone=n*.5f*Mathf.Exp(-t*90);break;
                    case SoundCue.Footstep:tone=(n*.3f+Mathf.Sin(t*2*Mathf.PI*70)*.6f)*env;break;
                    case SoundCue.EnemyDeath:case SoundCue.Hurt:tone=(Mathf.Sin(t*2*Mathf.PI*(110-t*80))*.65f+n*.25f)*env;break;
                    default:tone=Mathf.Sin(t*2*Mathf.PI*660)*env*.35f;break;
                }
                samples[i]=Mathf.Clamp(tone,-.9f,.9f)*Mathf.Min(1,t*1000);
            }
            var clip=AudioClip.Create("Original Synth "+cue,samples.Length,1,rate,false);clip.SetData(samples,0);return clip;
        }
        AudioClip Ambient()
        {
            const int rate=22050;var data=new float[rate*8];
            for(int i=0;i<data.Length;i++){float t=(float)i/rate;data[i]=.12f*Mathf.Sin(t*2*Mathf.PI*55)+.055f*Mathf.Sin(t*2*Mathf.PI*82.5f)+.025f*Mathf.Sin(t*2*Mathf.PI*110);}
            var clip=AudioClip.Create("Original relay ambience",data.Length,1,rate,false);clip.SetData(data,0);return clip;
        }
        void OnDestroy(){foreach(var c in clips.Values)Destroy(c);if(ambience && ambience.clip)Destroy(ambience.clip);}
    }
}
