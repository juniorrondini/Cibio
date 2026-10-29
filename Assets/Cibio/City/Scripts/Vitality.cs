using System;
using UnityEngine;

namespace Cibio.City
{
    public sealed class Vitality : MonoBehaviour
    {
        public float maximum = 100;
        public int team;
        public float Current { get; private set; }
        public bool IsDead => Current <= 0;
        public event Action Died;
        public event Action<Vector3> Hit;
        private float immunity, flash;
        private Renderer[] renderers;
        private MaterialPropertyBlock block;
        private void Awake() { Current = maximum; renderers = GetComponentsInChildren<Renderer>(); block = new MaterialPropertyBlock(); }
        public bool Damage(float amount, Vector3 from)
        {
            if (IsDead || Time.time < immunity || amount <= 0) return false;
            Current = Mathf.Max(0, Current - amount); immunity = Time.time + (team == 0 ? .7f : .13f);
            flash = .12f; Hit?.Invoke(from);
            if (Current <= 0) Died?.Invoke();
            return true;
        }
        public void Restore()
        {
            Current = maximum; immunity = Time.time + 1; flash = 0;
            if(block!=null){block.Clear();foreach(var surface in renderers)if(surface)surface.SetPropertyBlock(block);}
        }
        private void Update()
        {
            if (flash <= 0) return;
            flash -= Time.deltaTime;
            block.Clear();
            if (flash > 0) block.SetColor("_EmissionColor", new Color(.85f, .45f, .12f));
            foreach (var renderer in renderers) if (renderer) renderer.SetPropertyBlock(block);
        }
    }
}
