using UnityEngine;
namespace CompoundFPS
{
    public sealed class Weapon : MonoBehaviour
    {
        public WeaponDefinition definition;
        public Camera aimCamera;
        public GameObject muzzleFlash;
        public PlayerLook playerLook;
        public AmmoState Ammo { get; private set; }
        public bool Reloading { get; private set; }
        public bool Aiming { get; set; }
        public float ShotBloom { get; private set; }
        public float ReloadProgress => Reloading ? Mathf.Clamp01(1-(reloadEnd-Time.time)/definition.reloadDuration) : 0;
        float nextShot, reloadEnd, flashEnd, dryClick;
        void Awake() { Ammo = new AmmoState(definition.magazineCapacity,definition.reserveAmmunition); }
        void OnDisable() { Reloading=false; Aiming=false; if(muzzleFlash)muzzleFlash.SetActive(false); }
        void Update()
        {
            if(!CombatEvents.Playing)return;
            if(Reloading && Time.time>=reloadEnd) { Ammo.Reload(); Reloading=false; }
            ShotBloom=Mathf.MoveTowards(ShotBloom,0,Time.deltaTime*1.8f);
            if(muzzleFlash && Time.time>flashEnd)muzzleFlash.SetActive(false);
        }
        public bool TryFire()
        {
            if(!CombatEvents.Playing || Reloading || Time.time<nextShot)return false;
            if(!Ammo.Spend()) { if(Time.time>dryClick){ Play(definition.emptySound,SoundCue.Empty); dryClick=Time.time+.3f; } return false; }
            nextShot=Time.time+1/definition.fireRate;
            float spread=definition.spread*(Aiming?.2f:1)*(1+ShotBloom*.5f);
            Vector2 offset=Random.insideUnitCircle*spread;
            var direction=(aimCamera.transform.forward+aimCamera.transform.right*offset.x+aimCamera.transform.up*offset.y).normalized;
            if(Physics.Raycast(aimCamera.transform.position,direction,out var hit,definition.effectiveRange,~(1<<2),QueryTriggerInteraction.Ignore))
            {
                var target=hit.collider.GetComponentInParent<IDamageable>();
                if(target!=null) CombatEvents.Confirm(target.TakeDamage(definition.damage,aimCamera.transform.position));
                CombatEvents.Spark(hit.point,hit.normal,target!=null);
                if(AudioManager.Instance)AudioManager.Instance.PlayImpact(hit,target!=null);
            }
            Play(definition.shootingSound,definition.automatic?SoundCue.Rifle:SoundCue.Pistol);
            CombatEvents.EmitNoise(aimCamera.transform.position,35);
            if(muzzleFlash){muzzleFlash.SetActive(true);flashEnd=Time.time+.045f;}
            ShotBloom=Mathf.Min(2,ShotBloom+.3f);
            if(playerLook)playerLook.AddRecoil(definition.recoil*(1+ShotBloom*.22f),Random.Range(-definition.horizontalRecoil,definition.horizontalRecoil));
            return true;
        }
        public bool BeginReload()
        {
            if(!CombatEvents.Playing || Reloading || !Ammo.CanReload)return false;
            Reloading=true; reloadEnd=Time.time+definition.reloadDuration; Play(definition.reloadSound,SoundCue.Reload); return true;
        }
        void Play(AudioClip clip,SoundCue fallback)
        {
            if(AudioManager.Instance)AudioManager.Instance.PlayWeapon(definition,fallback==SoundCue.Reload?WeaponSound.Reload:fallback==SoundCue.Empty?WeaponSound.Empty:WeaponSound.Fire,transform.position);
        }
    }
}
