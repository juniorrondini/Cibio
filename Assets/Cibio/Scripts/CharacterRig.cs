using UnityEngine;

namespace Cibio
{
    public sealed class CharacterRig : MonoBehaviour
    {
        public PlayerMotor motor;
        public Transform torso, head, leftArm, rightArm, leftLeg, rightLeg;
        private float phase;

        private void LateUpdate()
        {
            if (motor == null || motor.Paused) return;
            float speed = Mathf.Abs(motor.Motion.HorizontalSpeed);
            float blend = Mathf.Clamp01(speed / motor.settings.walkSpeed);
            phase += Time.deltaTime * (5f + speed * 1.6f);
            float swing = motor.Grounded ? Mathf.Sin(phase) * 32f * blend : -14f;
            if (leftLeg) leftLeg.localRotation = Quaternion.Euler(swing, 0, 0);
            if (rightLeg) rightLeg.localRotation = Quaternion.Euler(-swing, 0, 0);
            if (leftArm) leftArm.localRotation = Quaternion.Euler(-swing * .7f, 0, -7f);
            if (rightArm) rightArm.localRotation = Quaternion.Euler(swing * .7f, 0, 7f);
            if (torso) torso.localPosition = new Vector3(0, 1.15f +
                (motor.Grounded ? Mathf.Abs(Mathf.Sin(phase)) * .045f * blend : .035f), 0);
            // Face the camera at rest, turn three quarters toward travel when moving.
            var turn = Quaternion.Euler(0, -motor.Motion.Facing * (speed > .1f ? 48f : 20f), 0);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, turn, 12f * Time.deltaTime);
        }
    }
}
