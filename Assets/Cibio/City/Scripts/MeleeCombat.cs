using System.Collections.Generic;
using UnityEngine;

namespace Cibio.City
{
    public sealed class MeleeCombat : MonoBehaviour
    {
        public Transform facing;
        public Animator animator;
        public float damage = 20, range = 1.5f, cooldown = .55f;
        public bool IsAttacking => timer > 0;
        public int ConfirmedHits { get; private set; }
        public static int DefeatedEnemies { get; private set; }
        private float timer, ready;
        private bool struck;
        private CityMotor motor;
        private Vitality life;
        private readonly Collider[] overlaps = new Collider[24];
        private readonly HashSet<Vitality> visited = new HashSet<Vitality>();
        [RuntimeInitializeOnLoadMethod] private static void ResetScore() { DefeatedEnemies = 0; }
        public static void RegisterDefeat() { DefeatedEnemies++; }
        private void Awake() { motor = GetComponent<CityMotor>(); life = GetComponent<Vitality>(); }
        public bool TryAttack()
        {
            if (Time.time < ready || life.IsDead || (motor && motor.Paused)) return false;
            ready = Time.time + cooldown; timer = .48f; struck = false;
            if (animator) animator.SetTrigger("Attack");
            return true;
        }
        private void Update()
        {
            if (motor && motor.Paused) return;
            if (timer <= 0) return;
            timer -= Mathf.Min(Time.deltaTime, .04f);
            if (struck || timer > .29f) return;
            struck = true; visited.Clear();
            Vector3 forward = facing ? facing.forward : transform.forward;
            int count = Physics.OverlapSphereNonAlloc(transform.position + Vector3.up * .8f, range, overlaps, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var target = overlaps[i].GetComponentInParent<Vitality>();
                if (!target || target.team == life.team || !visited.Add(target)) continue;
                Vector3 to = Vector3.ProjectOnPlane(target.transform.position - transform.position, Vector3.up);
                if (to.sqrMagnitude > .04f && Vector3.Dot(forward, to.normalized) < .15f) continue;
                if (Mathf.Abs(target.transform.position.y - transform.position.y) > 1.3f) continue;
                if (target.Damage(damage, transform.position)) ConfirmedHits++;
            }
        }
    }
}
