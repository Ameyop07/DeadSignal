using UnityEngine;
using UnityEngine.InputSystem;
namespace CompoundFPS
{
    public sealed class WeaponRecoil : MonoBehaviour
    {
        public WeaponManager manager;
        public PlayerController controller;
        public PlayerLook look;
        public Camera viewCamera;
        public float normalFOV=75, aimSmoothness=13, swayAmount=.0007f;
        void LateUpdate()
        {
            if(!CombatEvents.Playing || manager.Current==null || Mouse.current==null)return;
            var weapon=manager.Current;
            bool aim=Mouse.current.rightButton.isPressed && !weapon.Reloading;
            weapon.Aiming=aim; controller.Aiming=aim; look.Aiming=aim;
            viewCamera.fieldOfView=Mathf.Lerp(viewCamera.fieldOfView,aim?(weapon.definition.hasScope?weapon.definition.scopedFOV:weapon.definition.adsFOV):normalFOV,Time.deltaTime*aimSmoothness);
            var sway=Vector2.ClampMagnitude(Mouse.current.delta.ReadValue()*swayAmount,.025f)*(aim?.2f:1);
            var target=(aim?weapon.definition.adsPosition:weapon.definition.hipPosition)+new Vector3(-sway.x,-sway.y,-weapon.ShotBloom*.025f);
            target.y+=controller.CameraOffset*.35f;
            var rotation=Quaternion.Euler(weapon.ShotBloom*2,0,weapon.Reloading?Mathf.Sin(weapon.ReloadProgress*Mathf.PI)*-28:0);
            if(weapon.Reloading)target.y-=Mathf.Sin(weapon.ReloadProgress*Mathf.PI)*.15f;
            weapon.transform.localPosition=Vector3.Lerp(weapon.transform.localPosition,target,Time.deltaTime*aimSmoothness);
            weapon.transform.localRotation=Quaternion.Slerp(weapon.transform.localRotation,rotation,Time.deltaTime*12);
        }
    }
}
