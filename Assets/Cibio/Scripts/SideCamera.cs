using UnityEngine;

namespace Cibio
{
    [RequireComponent(typeof(Camera))]
    public sealed class SideCamera : MonoBehaviour
    {
        public PlayerMotor target;
        public Vector3 offset = new Vector3(0, 3.6f, -17f);
        public float smoothTime = .22f;
        public float lookAhead = 1.9f;
        public float verticalDeadZone = .8f;
        public Vector2 horizontalBounds = new Vector2(-2f, 54f);
        private Vector3 focus, velocity;
        private float ahead;

        private void Start() { Snap(); }
        public void Snap()
        {
            if (target == null) return;
            ahead = 0f;
            focus = target.transform.position + Vector3.up * 1.25f;
            focus.x = Mathf.Clamp(focus.x, horizontalBounds.x, horizontalBounds.y);
            focus.y = Mathf.Max(1.65f, focus.y);
            focus.z = 0;
            velocity = Vector3.zero;
            transform.position = focus + offset;
            transform.rotation = Quaternion.LookRotation(-offset);
        }
        private void LateUpdate()
        {
            if (target == null || target.Paused) return;
            float dt = Mathf.Min(Time.deltaTime, .05f);
            float desiredAhead = Mathf.Clamp(target.Motion.HorizontalSpeed / target.settings.runSpeed, -1f, 1f) * lookAhead;
            ahead = Mathf.Lerp(ahead, desiredAhead, 1f - Mathf.Exp(-5f * dt));
            Vector3 desired = focus;
            desired.x = Mathf.Clamp(target.transform.position.x + ahead, horizontalBounds.x, horizontalBounds.y);
            float y = target.transform.position.y + 1.25f;
            if (Mathf.Abs(y - focus.y) > verticalDeadZone)
                desired.y = y - Mathf.Sign(y - focus.y) * verticalDeadZone;
            desired.y = Mathf.Clamp(desired.y, 1.65f, 5f);
            desired.z = 0f;
            focus = Vector3.SmoothDamp(focus, desired, ref velocity, smoothTime, Mathf.Infinity, dt);
            transform.position = focus + offset;
        }
    }
}
