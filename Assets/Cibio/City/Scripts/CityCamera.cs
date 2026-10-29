using UnityEngine;

namespace Cibio.City
{
    [RequireComponent(typeof(Camera))]
    public sealed class CityCamera : MonoBehaviour
    {
        public CityMotor target;
        public Vector3 offset = new Vector3(0, 14f, -10.8f);
        public float smoothTime = .22f, lookAhead = .35f;
        public Vector2 xBounds = new Vector2(-16, 16), zBounds = new Vector2(-12, 12);
        public Vector3 Focus { get; private set; }
        private Vector3 smoothing;
        private void Start() { Snap(); }
        private Vector3 Desired()
        {
            var position = target.transform.position;
            position.y = .65f;
            position += Vector3.ProjectOnPlane(target.Velocity, Vector3.up) * lookAhead;
            position.x = Mathf.Clamp(position.x, xBounds.x, xBounds.y);
            position.z = Mathf.Clamp(position.z, zBounds.x, zBounds.y);
            return position;
        }
        public void Snap()
        {
            if (!target) return;
            Focus = Desired(); smoothing = Vector3.zero;
            transform.position = Focus + offset;
            transform.rotation = Quaternion.LookRotation(-offset);
        }
        private void LateUpdate()
        {
            if (!target || target.Paused) return;
            var lens=GetComponent<Camera>();float wheel=Input.mouseScrollDelta.y;
            if(Mathf.Abs(wheel)>.01f)lens.orthographicSize=Mathf.Clamp(lens.orthographicSize-wheel*.5f,5.2f,10.5f);
            Focus = Vector3.SmoothDamp(Focus, Desired(), ref smoothing, smoothTime, 25, Mathf.Min(Time.deltaTime, .04f));
            transform.position = Focus + offset;
        }
    }
}
