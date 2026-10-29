using System.Collections.Generic;
using UnityEngine;

namespace Cibio.City
{
    public sealed class Occludable : MonoBehaviour
    {
        public static readonly HashSet<Occludable> Active = new HashSet<Occludable>();
        public bool obstructing;
        private Renderer[] surfaces;
        private MaterialPropertyBlock properties;
        private float fade = 1;
        private void OnEnable() { Active.Add(this); surfaces = GetComponentsInChildren<Renderer>(); properties = new MaterialPropertyBlock(); }
        private void OnDisable() { Active.Remove(this); }
        private void Update()
        {
            float goal = obstructing ? .22f : 1f;
            if (Mathf.Abs(goal - fade) < .002f) return;
            fade = Mathf.MoveTowards(fade, goal, Time.deltaTime * 4);
            properties.Clear(); properties.SetFloat("_Fade", fade);
            foreach (var surface in surfaces) if (surface) surface.SetPropertyBlock(properties);
        }
    }
}
