using UnityEngine;

namespace Cibio.City
{
    [RequireComponent(typeof(CityCamera))]
    public sealed class CameraObstruction : MonoBehaviour
    {
        private readonly RaycastHit[] hits = new RaycastHit[24];
        private CityCamera rig;
        private void Awake() { rig = GetComponent<CityCamera>(); }
        private void LateUpdate()
        {
            foreach (var item in Occludable.Active) item.obstructing = false;
            if (!rig.target) return;
            var delta = rig.target.transform.position + Vector3.up * 1.2f - transform.position;
            int count = Physics.SphereCastNonAlloc(transform.position, .32f, delta.normalized, hits, delta.magnitude, 1, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var item = hits[i].collider.GetComponentInParent<Occludable>();
                if (item) item.obstructing = true;
            }
        }
    }
}
