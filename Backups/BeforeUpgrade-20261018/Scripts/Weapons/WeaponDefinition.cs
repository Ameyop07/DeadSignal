using UnityEngine;
namespace CompoundFPS
{
    [CreateAssetMenu(menuName="Compound FPS/Weapon")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        public string displayName = "AR-41";
        public bool automatic = true;
        public float damage = 26, fireRate = 9, reloadDuration = 2, effectiveRange = 100;
        public int magazineCapacity = 30, reserveAmmunition = 150;
        public float recoil = .65f, horizontalRecoil = .18f, spread = .009f, adsFOV = 55;
        public bool hasScope;
        public float scopedFOV = 28;
        public Vector3 hipPosition = new Vector3(.25f,-.23f,.45f);
        public Vector3 adsPosition = new Vector3(0,-.13f,.38f);
        public AudioClip shootingSound, reloadSound, emptySound;
    }
    [System.Serializable]
    public sealed class AmmoState
    {
        public int magazine, reserve, capacity;
        public AmmoState(int capacity, int reserve) { this.capacity=capacity; magazine=capacity; this.reserve=reserve; }
        public bool Spend() { if(magazine<=0) return false; magazine--; return true; }
        public int Reload() { int transfer=Mathf.Min(capacity-magazine,reserve); magazine+=transfer; reserve-=transfer; return transfer; }
        public bool CanReload => magazine < capacity && reserve > 0;
    }
}
