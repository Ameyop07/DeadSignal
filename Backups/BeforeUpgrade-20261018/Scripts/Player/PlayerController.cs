using UnityEngine;
using UnityEngine.InputSystem;

namespace CompoundFPS
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        public float walkSpeed = 4.5f, sprintSpeed = 7.2f, crouchSpeed = 2.3f;
        public float jumpHeight = 1.15f, gravity = -24f, acceleration = 16f;
        public Transform cameraPivot;
        public bool Aiming { get; set; }
        public bool Crouched { get; private set; }
        public bool Sprinting { get; private set; }
        public float Speed => planarVelocity.magnitude;
        public bool Grounded => controller != null && controller.isGrounded;
        public float CameraOffset { get; private set; }
        public string Surface = "Concrete"; // Surface-specific footstep banks can use this key.
        CharacterController controller;
        Vector3 planarVelocity;
        float verticalVelocity, stepDistance, bobTime, landing;
        bool wasGrounded;
        void Awake() { controller = GetComponent<CharacterController>(); }
        void Update()
        {
            if (!CombatEvents.Playing || Keyboard.current == null) return;
            var k = Keyboard.current;
            bool wantsCrouch = k.leftCtrlKey.isPressed || k.cKey.isPressed;
            bool blocked = Physics.SphereCast(transform.position + Vector3.up * .65f, .29f, Vector3.up, out _, 1.2f, ~ (1 << 2), QueryTriggerInteraction.Ignore);
            Crouched = wantsCrouch || (Crouched && blocked);
            float targetHeight = Crouched ? 1.15f : 1.8f;
            controller.height = Mathf.MoveTowards(controller.height, targetHeight, Time.deltaTime * 5);
            controller.center = Vector3.up * controller.height * .5f;
            var input = new Vector2((k.dKey.isPressed ? 1 : 0) - (k.aKey.isPressed ? 1 : 0), (k.wKey.isPressed ? 1 : 0) - (k.sKey.isPressed ? 1 : 0));
            input = Vector2.ClampMagnitude(input, 1);
            Sprinting = k.leftShiftKey.isPressed && input.y > .1f && !Crouched && !Aiming;
            float speed = Crouched ? crouchSpeed : Sprinting ? sprintSpeed : walkSpeed;
            if (Aiming) speed *= .7f;
            var target = (transform.right * input.x + transform.forward * input.y) * speed;
            planarVelocity = Vector3.MoveTowards(planarVelocity, target, acceleration * Time.deltaTime);
            if (controller.isGrounded && verticalVelocity < 0) verticalVelocity = -2;
            if (controller.isGrounded && k.spaceKey.wasPressedThisFrame && !Crouched) verticalVelocity = Mathf.Sqrt(jumpHeight * -2 * gravity);
            verticalVelocity += gravity * Time.deltaTime;
            controller.Move((planarVelocity + Vector3.up * verticalVelocity) * Time.deltaTime);
            if (controller.isGrounded && !wasGrounded) landing = -.055f;
            wasGrounded = controller.isGrounded;
            landing = Mathf.Lerp(landing, 0, Time.deltaTime * 9);
            bobTime += Time.deltaTime * (Sprinting ? 13 : 9);
            CameraOffset = (Grounded && Speed > .2f ? Mathf.Sin(bobTime) * (Sprinting ? .032f : .018f) : 0) + landing;
            if (cameraPivot) cameraPivot.localPosition = Vector3.Lerp(cameraPivot.localPosition, Vector3.up * (controller.height - .15f + CameraOffset), Time.deltaTime * 15);
            if (Grounded && Speed > .3f)
            {
                stepDistance += Speed * Time.deltaTime;
                if (stepDistance > (Crouched ? 2.2f : 1.8f)) { stepDistance = 0; CombatEvents.Play(SoundCue.Footstep, transform.position); CombatEvents.EmitNoise(transform.position, Crouched ? 3 : Sprinting ? 14 : 7); }
            }
        }
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            controller.enabled = false; transform.SetPositionAndRotation(position, rotation); controller.enabled = true;
            planarVelocity = Vector3.zero; verticalVelocity = 0;
        }
    }
}
