using UnityEngine;

namespace Cibio.City
{
    [RequireComponent(typeof(CharacterController), typeof(Vitality))]
    public sealed class StreetEnemy : MonoBehaviour
    {
        public CityMotor target;
        public Transform visual;
        public Animator animator;
        public float detectionRange = 7, speed = 1.7f, attackRange = 1.25f;
        public Vector3 Velocity { get; private set; }
        public bool Attacking => attackTimer > 0;
        private CharacterController controller;
        private Vitality life;
        private Vector3 home, push;
        private float attackTimer, ready, vertical;
        private bool hit;
        private void Awake()
        {
            controller = GetComponent<CharacterController>(); life = GetComponent<Vitality>(); home = transform.position;
            life.Hit += source => { push = Vector3.ProjectOnPlane(transform.position - source, Vector3.up).normalized * 3.5f; };
            life.Died += () => { MeleeCombat.RegisterDefeat(); controller.enabled = false; if (animator) animator.SetTrigger("Death"); Destroy(gameObject, 2.3f); };
        }
        private void Update()
        {
            if (!target || target.Paused || life.IsDead) { Velocity = Vector3.zero; return; }
            float dt = Mathf.Min(Time.deltaTime, .04f);
            Vector3 delta = Vector3.ProjectOnPlane(target.transform.position - transform.position, Vector3.up);
            bool chase = delta.magnitude < detectionRange && !target.GetComponent<Vitality>().IsDead;
            Vector3 move = Vector3.zero;
            if (chase && delta.sqrMagnitude > .05f && visual)
                visual.rotation = Quaternion.Slerp(visual.rotation, Quaternion.LookRotation(delta), 1 - Mathf.Exp(-8 * dt));
            if (attackTimer > 0)
            {
                attackTimer -= dt;
                if (!hit && attackTimer < .82f)
                {
                    hit = true;
                    if (delta.magnitude <= attackRange + .25f && Mathf.Abs(target.transform.position.y - transform.position.y) < .6f)
                    {
                        if (target.GetComponent<Vitality>().Damage(12, transform.position)) target.Push(delta);
                    }
                }
            }
            else if (chase)
            {
                if (delta.magnitude > attackRange) move = delta.normalized * speed;
                else if (Time.time >= ready)
                { attackTimer = 1.25f; ready = Time.time + 1.85f; hit = false; if (animator) animator.SetTrigger("Attack"); }
            }
            else
            {
                Vector3 point = home + new Vector3(Mathf.Sin(Time.time * .35f + home.x), 0, Mathf.Cos(Time.time * .35f + home.z)) * 1.1f;
                Vector3 patrol = Vector3.ProjectOnPlane(point - transform.position, Vector3.up);
                if (patrol.magnitude > .15f) { move = patrol.normalized * .65f; visual.rotation = Quaternion.Slerp(visual.rotation, Quaternion.LookRotation(move), dt * 4); }
            }
            push = Vector3.MoveTowards(push, Vector3.zero, dt * 8);
            vertical = controller.isGrounded ? -2 : Mathf.Max(-20, vertical - dt * 23);
            Vector3 before = transform.position;
            // Small obstacle probes steer around street furniture; collisions remain authoritative.
            if (move.sqrMagnitude > .1f && Physics.SphereCast(transform.position + Vector3.up * .8f, .25f,
                move.normalized, out var obstacle, .8f, 1 << 0, QueryTriggerInteraction.Ignore))
            {
                if (!obstacle.collider.GetComponentInParent<CityMotor>())
                {
                    Vector3 tangent = Vector3.Cross(Vector3.up, obstacle.normal).normalized;
                    if (Vector3.Dot(tangent, delta) < 0) tangent = -tangent;
                    move = tangent * move.magnitude;
                }
            }
            controller.Move((move + push + Vector3.up * vertical) * dt);
            Velocity = (transform.position - before) / Mathf.Max(dt, .001f);
        }
    }
}
