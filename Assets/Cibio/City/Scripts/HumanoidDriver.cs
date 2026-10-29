using UnityEngine;

namespace Cibio.City
{
    [RequireComponent(typeof(Animator))]
    public sealed class HumanoidDriver : MonoBehaviour
    {
        public CityMotor motor;
        public StreetEnemy enemy;
        public Transform leftEye, rightEye;
        private Animator animator;
        private Vector3 eyeScale;
        private float blinkClock;
        private void Awake()
        {
            animator = GetComponent<Animator>();
            animator.SetFloat("AttackRate",enemy?.45f:1f);
            if (leftEye) eyeScale = leftEye.localScale;
            blinkClock = 2.5f;
        }
        private void Update()
        {
            bool paused = motor && motor.Paused || enemy && enemy.target && enemy.target.Paused;
            animator.speed = paused ? 0 : 1;
            if (paused) return;
            float speed = motor ? motor.Speed : enemy ? Vector3.ProjectOnPlane(enemy.Velocity, Vector3.up).magnitude : 0;
            animator.SetFloat("Speed", speed, .12f, Time.deltaTime);
            animator.SetBool("Grounded", !motor || motor.Grounded);
            animator.SetFloat("Vertical", motor ? motor.VerticalSpeed : 0);
            blinkClock -= Time.deltaTime;
            float blink = blinkClock <= 0 ? Mathf.Clamp01(Mathf.Abs(blinkClock + .085f) / .085f) : 1;
            if (leftEye) leftEye.localScale = new Vector3(eyeScale.x, eyeScale.y * Mathf.Max(.07f, blink), eyeScale.z);
            if (rightEye) rightEye.localScale = new Vector3(eyeScale.x, eyeScale.y * Mathf.Max(.07f, blink), eyeScale.z);
            if (blinkClock < -.17f) blinkClock = 2.8f + Mathf.Repeat(Time.time * 1.37f, 2.4f);
        }
    }
}
