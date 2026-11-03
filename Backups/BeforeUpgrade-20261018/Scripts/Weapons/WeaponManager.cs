using UnityEngine;
using UnityEngine.InputSystem;
namespace CompoundFPS
{
    public sealed class WeaponManager : MonoBehaviour
    {
        public Weapon[] weapons;
        public Weapon Current => weapons != null && weapons.Length>0 ? weapons[index] : null;
        int index;
        void Start(){Equip(0);}
        void Update()
        {
            if(!CombatEvents.Playing || Keyboard.current==null || Mouse.current==null)return;
            var k=Keyboard.current;
            if(k.digit1Key.wasPressedThisFrame)Equip(0);
            if(k.digit2Key.wasPressedThisFrame)Equip(1);
            if(k.rKey.wasPressedThisFrame)Current.BeginReload();
            if(Current.definition.automatic?Mouse.current.leftButton.isPressed:Mouse.current.leftButton.wasPressedThisFrame)Current.TryFire();
        }
        public void Equip(int slot)
        {
            if(slot<0 || slot>=weapons.Length)return;
            index=slot;
            for(int i=0;i<weapons.Length;i++)weapons[i].gameObject.SetActive(i==index);
        }
    }
}
