using System;

namespace Cibio
{
    // Engine-independent movement rules. The Unity adapter supplies collision results.
    [Serializable]
    public sealed class MotionSettings
    {
        public float walkSpeed = 5.2f;
        public float runSpeed = 8f;
        public float acceleration = 38f;
        public float braking = 48f;
        public float airAcceleration = 22f;
        public float jumpHeight = 2.6f;
        public float gravity = 26f;
        public float fallMultiplier = 1.65f;
        public float terminalSpeed = 24f;
        public float jumpCut = .45f;
        public float coyoteTime = .12f;
        public float jumpBuffer = .14f;
    }

    public struct MotionInput
    {
        public float horizontal;
        public bool sprint, jumpPressed, jumpHeld;
    }

    public sealed class MotionModel
    {
        public float HorizontalSpeed { get; private set; }
        public float VerticalSpeed { get; private set; }
        public bool JumpedThisStep { get; private set; }
        public int Facing { get; private set; } = 1;
        private float coyote, buffer;
        private bool consumedJump, wasHoldingJump;

        public void Step(float dt, MotionInput input, bool grounded, MotionSettings settings)
        {
            JumpedThisStep = false;
            if (dt <= 0f) return;
            float axis = Math.Max(-1f, Math.Min(1f, input.horizontal));
            if (Math.Abs(axis) > .01f) Facing = axis > 0 ? 1 : -1;
            buffer = input.jumpPressed ? settings.jumpBuffer : Math.Max(0f, buffer - dt);
            if (grounded && VerticalSpeed <= 0f)
            {
                coyote = settings.coyoteTime;
                consumedJump = false;
                VerticalSpeed = -2f;
            }
            else coyote = Math.Max(0f, coyote - dt);

            float target = axis * (input.sprint ? settings.runSpeed : settings.walkSpeed);
            float rate = grounded ? (Math.Abs(axis) < .01f ? settings.braking : settings.acceleration) : settings.airAcceleration;
            HorizontalSpeed = Towards(HorizontalSpeed, target, rate * dt);
            if (buffer > 0f && coyote > 0f && !consumedJump)
            {
                VerticalSpeed = (float)Math.Sqrt(2f * settings.gravity * settings.jumpHeight);
                coyote = buffer = 0f;
                consumedJump = true;
                JumpedThisStep = true;
            }
            // A queued tap released before landing must also produce a short hop.
            if (VerticalSpeed > 0f && !input.jumpHeld && (wasHoldingJump || JumpedThisStep))
                VerticalSpeed *= settings.jumpCut;
            VerticalSpeed = Math.Max(-settings.terminalSpeed,
                VerticalSpeed - settings.gravity * (VerticalSpeed < 0f ? settings.fallMultiplier : 1f) * dt);
            wasHoldingJump = input.jumpHeld;
        }

        public void HitCeiling() { if (VerticalSpeed > 0f) VerticalSpeed = 0f; }
        public void Reset()
        {
            HorizontalSpeed = VerticalSpeed = coyote = buffer = 0f;
            consumedJump = wasHoldingJump = JumpedThisStep = false;
        }
        private static float Towards(float value, float target, float delta)
        { return Math.Abs(target - value) <= delta ? target : value + Math.Sign(target - value) * delta; }
    }
}
