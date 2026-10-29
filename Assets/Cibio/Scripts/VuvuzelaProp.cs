using UnityEngine;

namespace Cibio
{
    [ExecuteAlways, RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class VuvuzelaProp : MonoBehaviour
    {
        public float length = 2.4f;
        public float bellRadius = .65f;
        public int sides = 24;
        private Mesh generated;
        private void OnEnable()
        {
            if (GetComponent<MeshFilter>().sharedMesh != null) return;
            // Open bell with an inner wall. Local +Y is the horn's axis.
            int rings = 12;
            var vertices = new Vector3[(rings + 1) * sides * 2];
            var triangles = new int[(rings * 2 + 1) * sides * 6];
            int index = 0;
            for (int wall = 0; wall < 2; wall++)
            for (int ring = 0; ring <= rings; ring++)
            {
                float t = ring / (float)rings;
                float radius = Mathf.Lerp(.08f, bellRadius, Mathf.Pow(t, 3.2f));
                if (wall == 1) radius = Mathf.Max(.025f, radius - .035f);
                for (int side = 0; side < sides; side++)
                {
                    float angle = side * Mathf.PI * 2 / sides;
                    vertices[(wall * (rings + 1) + ring) * sides + side] =
                        new Vector3(Mathf.Cos(angle) * radius, t * length, Mathf.Sin(angle) * radius);
                    if (ring == rings) continue;
                    int a = (wall * (rings + 1) + ring) * sides + side;
                    int b = (wall * (rings + 1) + ring) * sides + (side + 1) % sides;
                    int c = a + sides, d = b + sides;
                    if (wall == 0) { Add(a,c,b); Add(b,c,d); }
                    else { Add(a,b,c); Add(b,d,c); }
                }
            }
            for (int side = 0; side < sides; side++)
            {
                int a = rings * sides + side, b = rings * sides + (side + 1) % sides;
                int c = a + (rings + 1) * sides, d = b + (rings + 1) * sides;
                Add(a,b,c); Add(b,d,c);
            }
            generated = new Mesh { name = "Vuvuzela • original mesh" };
            generated.vertices = vertices; generated.triangles = triangles;
            generated.RecalculateNormals(); generated.RecalculateBounds();
            GetComponent<MeshFilter>().sharedMesh = generated;
            void Add(int a, int b, int c) { triangles[index++] = a; triangles[index++] = b; triangles[index++] = c; }
        }
        private void OnDestroy()
        {
            if (generated == null) return;
            if (Application.isPlaying) Destroy(generated); else DestroyImmediate(generated);
        }
    }
}
