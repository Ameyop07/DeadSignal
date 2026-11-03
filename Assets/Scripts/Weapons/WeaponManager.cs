using UnityEngine;
using UnityEngine.InputSystem;
namespace CompoundFPS
{
    public sealed class WeaponManager : MonoBehaviour
    {
        public Weapon[] weapons;
        public Weapon Current => weapons != null && weapons.Length>0 ? weapons[index] : null;
        int index;
        int primary;
        void Start(){EquipLoadout();}
        public void EquipLoadout()
        {
            primary=0;var game=GameManager.Instance;var selected=PlayerProgress.Data.equippedWeapon;
            for(int i=0;i<weapons.Length;i++)if((!string.IsNullOrEmpty(selected) && weapons[i].definition.id==selected && PlayerProgress.Data.Owns(weapons[i].definition)) || (string.IsNullOrEmpty(selected) && game && game.level && weapons[i].definition==game.level.defaultWeapon)){primary=i;break;}
            Equip(primary);
        }
        void Update()
        {
            if(!CombatEvents.Playing || Keyboard.current==null || Mouse.current==null)return;
            var k=Keyboard.current;
            if(k.digit1Key.wasPressedThisFrame)Equip(primary);
            if(k.digit2Key.wasPressedThisFrame)Equip(primary==1?0:1);
            if(k.rKey.wasPressedThisFrame)Current.BeginReload();
            if(Current.definition.automatic?Mouse.current.leftButton.isPressed:Mouse.current.leftButton.wasPressedThisFrame)Current.TryFire();
        }
        public void Equip(int slot)
        {
            if(slot<0 || slot>=weapons.Length)return;
            var game=GameManager.Instance;if(!PlayerProgress.Data.Owns(weapons[slot].definition) && !(game && game.level && game.level.defaultWeapon==weapons[slot].definition))return;
            index=slot;
            for(int i=0;i<weapons.Length;i++)weapons[i].gameObject.SetActive(i==index);
            if(CombatEvents.Playing && AudioManager.Instance)AudioManager.Instance.PlayWeapon(weapons[index].definition,WeaponSound.Switch,transform.position);
        }
    }
}
