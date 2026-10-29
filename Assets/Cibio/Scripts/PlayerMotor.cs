using UnityEngine;

namespace Cibio
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        public MotionSettings settings = new MotionSettings();
        public Transform visual;
        public float planeZ;
        public float killHeight = -7f;
        public Vector3 respawnPoint = new Vector3(0, .08f, 0);
        public MotionModel Motion { get; } = new MotionModel();
        public bool Grounded { get; private set; }
        public bool Paused { get; private set; }
        public int Respawns { get; private set; }
        public float FurthestX { get; private set; }
        public string CheckpointName { get; private set; } = "Praça do começo";
        public bool Completed { get; private set; }
        private CharacterController controller;
        private float lastCheckpoint = -1f;
#if UNITY_EDITOR
        public bool AutomatedInput { get; set; }
        public MotionInput TestInput { get; set; }
#endif

        private void Awake() { controller = GetComponent<CharacterController>(); }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) Paused = !Paused;
            if (Input.GetKeyDown(KeyCode.R))
            {
                Paused = false;
                if (Completed)
                {
                    Completed = false; FurthestX = 0; lastCheckpoint = -1;
                    respawnPoint = new Vector3(0, .08f, planeZ);
                    CheckpointName = "Praça do começo";
                }
                Respawn(); return;
            }
            if (Paused) return;
            var input = new MotionInput
            {
                horizontal = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1f : 0f)
                    - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f),
                sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift),
                jumpPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow),
                jumpHeld = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)
            };
#if UNITY_EDITOR
            if (AutomatedInput) input = TestInput;
#endif
            float dt = Mathf.Min(Time.deltaTime, .05f);
            Grounded = controller.isGrounded;
            Motion.Step(dt, input, Grounded, settings);
            var flags = controller.Move(new Vector3(Motion.HorizontalSpeed, Motion.VerticalSpeed, 0f) * dt);
            if ((flags & CollisionFlags.Above) != 0) Motion.HitCeiling();
            Grounded = (flags & CollisionFlags.Below) != 0;
            var position = transform.position;
            position.z = planeZ;
            transform.position = position;
            FurthestX = Mathf.Max(FurthestX, position.x);
            if (Grounded && position.x >= 18f && lastCheckpoint < 18f)
                SetCheckpoint(new Vector3(19f, .08f, planeZ), 18f, "Ponto de respiro");
            if (Grounded && position.x >= 36f && lastCheckpoint < 36f)
                SetCheckpoint(new Vector3(37f, .08f, planeZ), 36f, "Depois da papelada");
            if (Grounded && position.x >= 52f) Completed = true;
            if (position.y < killHeight) Respawn();
        }

        private void SetCheckpoint(Vector3 point, float x, string title)
        { respawnPoint = point; lastCheckpoint = x; CheckpointName = title; }

        public void Respawn()
        {
            controller.enabled = false;
            transform.position = respawnPoint;
            controller.enabled = true;
            Motion.Reset();
            Grounded = false;
            Respawns++;
            var cameraRig = FindAnyObjectByType<SideCamera>();
            if (cameraRig != null) cameraRig.Snap();
        }
    }
}
