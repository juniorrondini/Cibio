using UnityEngine;

namespace Cibio.City
{
    public struct CityInput
    {
        public Vector2 move;
        public bool run, jumpPressed, jumpHeld, attack;
    }

    [RequireComponent(typeof(CharacterController), typeof(Vitality))]
    public sealed class CityMotor : MonoBehaviour
    {
        public Transform visual;
        public Camera view;
        public float walkSpeed = 2.3f, runSpeed = 4.6f;
        public float acceleration = 16f, braking = 21f, rotationSpeed = 13f;
        public float jumpHeight = .85f, gravity = 23f;
        public Bounds mapBounds = new Bounds(Vector3.zero, new Vector3(44, 20, 38));
        public Vector3 spawn = new Vector3(0, .08f, -4);
        public Vector3 Velocity { get; private set; }
        public float Speed => new Vector2(Velocity.x, Velocity.z).magnitude;
        public bool Grounded { get; private set; }
        public bool Paused { get; private set; }
        public float VerticalSpeed { get; private set; }
        public int Respawns { get; private set; }
        private CharacterController controller;
        private Vitality vitality;
        private MeleeCombat combat;
        private Vector3 horizontal, knockback;
        private float coyote, buffer;
        private bool consumedJump, wasHolding;
#if UNITY_EDITOR
        public bool AutomatedInput { get; set; }
        public CityInput TestInput { get; set; }
#endif
        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            vitality = GetComponent<Vitality>(); combat = GetComponent<MeleeCombat>();
            vitality.Died+=()=>{if(visual)visual.GetComponent<Animator>()?.SetTrigger("Death");};
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) SetPaused(!Paused);
            if (Input.GetKeyDown(KeyCode.R)) { Paused = false; Respawn(); return; }
            if (Paused) return;
            var input = new CityInput
            {
                move = new Vector2((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0) -
                    (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0),
                    (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 : 0) -
                    (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1 : 0)),
                run = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift),
                jumpPressed = Input.GetKeyDown(KeyCode.Space), jumpHeld = Input.GetKey(KeyCode.Space),
                attack = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.J)
            };
#if UNITY_EDITOR
            if (AutomatedInput) input = TestInput;
#endif
            float dt = Mathf.Min(Time.deltaTime, .04f);
            if (vitality.IsDead) { Move(Vector3.zero, dt); return; }
            Vector2 axis = Vector2.ClampMagnitude(input.move, 1);
            Vector3 forward = view ? Vector3.ProjectOnPlane(view.transform.forward, Vector3.up).normalized : Vector3.forward;
            Vector3 right = view ? Vector3.ProjectOnPlane(view.transform.right, Vector3.up).normalized : Vector3.right;
            Vector3 direction = forward * axis.y + right * axis.x;
            float attackSlow = combat != null && combat.IsAttacking ? .3f : 1f;
            Vector3 desired = direction * (input.run ? runSpeed : walkSpeed) * attackSlow;
            horizontal = Vector3.MoveTowards(horizontal, desired, (axis.sqrMagnitude > .01f ? acceleration : braking) * dt);
            if (direction.sqrMagnitude > .01f && visual)
                visual.rotation = Quaternion.Slerp(visual.rotation, Quaternion.LookRotation(direction), 1f - Mathf.Exp(-rotationSpeed * dt));
            buffer = input.jumpPressed ? .14f : Mathf.Max(0, buffer - dt);
            Grounded = controller.isGrounded;
            if (Grounded && VerticalSpeed <= 0)
            { coyote = .1f; consumedJump = false; VerticalSpeed = -2f; }
            else coyote = Mathf.Max(0, coyote - dt);
            if (buffer > 0 && coyote > 0 && !consumedJump && !(combat && combat.IsAttacking))
            { VerticalSpeed = Mathf.Sqrt(2 * gravity * jumpHeight); coyote = buffer = 0; consumedJump = true; }
            if (VerticalSpeed > 0 && wasHolding && !input.jumpHeld) VerticalSpeed *= .48f;
            wasHolding = input.jumpHeld;
            if (input.attack && Grounded && combat) combat.TryAttack();
            Move(horizontal, dt);
            if (transform.position.y < -5) Respawn();
        }
        private void Move(Vector3 movement, float dt)
        {
            VerticalSpeed = Mathf.Max(-23, VerticalSpeed - gravity * (VerticalSpeed < 0 ? 1.45f : 1f) * dt);
            knockback = Vector3.MoveTowards(knockback, Vector3.zero, 15f * dt);
            Vector3 before = transform.position;
            var flags = controller.Move((movement + knockback + Vector3.up * VerticalSpeed) * dt);
            if ((flags & CollisionFlags.Above) != 0 && VerticalSpeed > 0) VerticalSpeed = 0;
            Grounded = (flags & CollisionFlags.Below) != 0;
            Vector3 position = transform.position;
            position.x = Mathf.Clamp(position.x, mapBounds.min.x, mapBounds.max.x);
            position.z = Mathf.Clamp(position.z, mapBounds.min.z, mapBounds.max.z);
            transform.position = position;
            Velocity = (position - before) / Mathf.Max(dt, .001f);
        }
        public void Push(Vector3 direction) { knockback = Vector3.ProjectOnPlane(direction, Vector3.up).normalized * 4; }
        public void SetPaused(bool paused){Paused=paused;}
        public void Respawn()
        {
            controller.enabled = false; transform.position = spawn; controller.enabled = true;
            horizontal = knockback = Velocity = Vector3.zero; VerticalSpeed = 0;
            coyote = buffer = 0; consumedJump = wasHolding = Grounded = false;
            vitality.Restore(); Respawns++;
            if(visual){var animator=visual.GetComponent<Animator>();if(animator){animator.Rebind();animator.Update(0);}}
            FindAnyObjectByType<CityCamera>()?.Snap();
        }
    }
}
