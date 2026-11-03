using UnityEngine;
using UnityEngine.InputSystem;

namespace CompoundFPS
{
    public sealed class PlayerLook : MonoBehaviour
    {
        public Transform body;
        public float mouseSensitivity = 1f, adsSensitivity = .68f;
        public bool Aiming { get; set; }
        public float Pitch { get; private set; }
        Vector2 recoil;
        void LateUpdate()
        {
            if (!CombatEvents.Playing || Mouse.current == null) return;
            Vector2 delta = Mouse.current.delta.ReadValue() * GameSettings.Sensitivity * mouseSensitivity * (Aiming ? adsSensitivity : 1);
            Pitch = Mathf.Clamp(Pitch - delta.y, -85, 85);
            body.Rotate(Vector3.up, delta.x);
            recoil = Vector2.Lerp(recoil, Vector2.zero, Time.deltaTime * 7);
            transform.localRotation = Quaternion.Euler(Mathf.Clamp(Pitch - recoil.x, -88, 88), recoil.y, 0);
        }
        public void AddRecoil(float vertical, float horizontal) { recoil += new Vector2(vertical, horizontal); }
    }
}
