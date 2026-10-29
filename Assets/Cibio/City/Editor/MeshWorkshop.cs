using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Cibio.City.Editor
{
    public static class MeshWorkshop
    {
        public const string Art = "Assets/Cibio/City/Art";
        public struct Ring
        {
            public Vector3 center;
            public float x, z;
            public Ring(float y, float x, float z, float centerX = 0, float centerZ = 0)
            { center = new Vector3(centerX, y, centerZ); this.x = x; this.z = z; }
        }

        public static Mesh Loft(string name, Ring[] rings, int sides = 48)
        {
            var vertices = new List<Vector3>(); var uv = new List<Vector2>(); var triangles = new List<int>();
            for (int ring = 0; ring < rings.Length; ring++)
            for (int side = 0; side <= sides; side++)
            {
                float angle = side * Mathf.PI * 2 / sides;
                vertices.Add(rings[ring].center + new Vector3(Mathf.Cos(angle) * rings[ring].x, 0, Mathf.Sin(angle) * rings[ring].z));
                uv.Add(new Vector2(side / (float)sides, ring / (float)(rings.Length - 1)));
                if (ring == rings.Length - 1 || side == sides) continue;
                int a = ring * (sides + 1) + side, b = a + 1, c = a + sides + 1, d = c + 1;
                triangles.AddRange(new[] { a, c, b, b, c, d });
            }
            var mesh = Complete(name, vertices, uv, triangles);
            var normals = mesh.normals;
            for (int ring = 0; ring < rings.Length; ring++)
            {
                int a = ring * (sides + 1), b = a + sides;
                Vector3 average = (normals[a] + normals[b]).normalized;
                normals[a] = normals[b] = average;
            }
            mesh.normals = normals; mesh.RecalculateTangents(); return mesh;
        }

        public static Mesh Ellipsoid(string name, Vector3 center, Vector3 size, int sides = 40, int rings = 24)
        {
            var vertices = new List<Vector3>(); var uv = new List<Vector2>(); var tris = new List<int>(); var normals = new List<Vector3>();
            for (int r = 0; r <= rings; r++)
            for (int s = 0; s <= sides; s++)
            {
                float latitude = r * Mathf.PI / rings, longitude = s * Mathf.PI * 2 / sides;
                Vector3 p = new Vector3(Mathf.Sin(latitude) * Mathf.Cos(longitude), Mathf.Cos(latitude), Mathf.Sin(latitude) * Mathf.Sin(longitude));
                vertices.Add(center + Vector3.Scale(p, size));
                normals.Add(new Vector3(p.x / Mathf.Max(.0001f, size.x), p.y / Mathf.Max(.0001f, size.y), p.z / Mathf.Max(.0001f, size.z)).normalized);
                uv.Add(new Vector2(s / (float)sides, r / (float)rings));
                if (r == rings || s == sides) continue;
                int a = r * (sides + 1) + s, b = a + 1, c = a + sides + 1;
                tris.AddRange(new[] { a, b, c, b, c + 1, c });
            }
            var mesh = Complete(name, vertices, uv, tris); mesh.SetNormals(normals); mesh.RecalculateTangents(); return mesh;
        }

        public static Mesh RoundedBox(string name, Vector3 size, float radius, int segments = 6)
        {
            Vector3 half = size * .5f;
            radius = Mathf.Min(radius, Mathf.Min(half.x, Mathf.Min(half.y, half.z)) * .98f);
            var verts = new List<Vector3>(); var normals = new List<Vector3>(); var uv = new List<Vector2>(); var tris = new List<int>();
            Vector3[] directions = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
            foreach (Vector3 normal in directions)
            {
                Vector3 axisU = Mathf.Abs(normal.y) > .5f ? Vector3.right : Vector3.Cross(Vector3.up, normal);
                Vector3 axisV = Vector3.Cross(normal, axisU);
                int start = verts.Count;
                for (int y = 0; y <= segments; y++)
                for (int x = 0; x <= segments; x++)
                {
                    Vector3 cube = normal + axisU * (x / (float)segments * 2 - 1) + axisV * (y / (float)segments * 2 - 1);
                    Vector3 point = Vector3.Scale(cube, half);
                    Vector3 inner = half - Vector3.one * radius;
                    Vector3 clamped = new Vector3(Mathf.Clamp(point.x, -inner.x, inner.x), Mathf.Clamp(point.y, -inner.y, inner.y), Mathf.Clamp(point.z, -inner.z, inner.z));
                    Vector3 n = (point - clamped).normalized;
                    verts.Add(clamped + n * radius); normals.Add(n); uv.Add(new Vector2(x / (float)segments, y / (float)segments));
                    if (x == segments || y == segments) continue;
                    int a = start + y * (segments + 1) + x, b = a + 1, c = a + segments + 1;
                    tris.AddRange(new[] { a, b, c, b, c + 1, c });
                }
            }
            var mesh = Complete(name, verts, uv, tris); mesh.SetNormals(normals); mesh.RecalculateTangents(); return mesh;
        }

        public static Mesh Tube(string name, Vector3[] path, float radius, int sides = 10, float taper = 1f)
        {
            var v = new List<Vector3>(); var u = new List<Vector2>(); var t = new List<int>();
            for (int i = 0; i < path.Length; i++)
            {
                Vector3 tangent = (path[Mathf.Min(i + 1, path.Length - 1)] - path[Mathf.Max(i - 1, 0)]).normalized;
                Vector3 a = Vector3.Cross(tangent, Mathf.Abs(tangent.y) > .9f ? Vector3.forward : Vector3.up).normalized;
                Vector3 b = Vector3.Cross(tangent, a).normalized;
                float r = radius * Mathf.Lerp(1, taper, i / (float)(path.Length - 1));
                for (int j = 0; j <= sides; j++)
                {
                    float angle = j * Mathf.PI * 2 / sides;
                    v.Add(path[i] + (a * Mathf.Cos(angle) + b * Mathf.Sin(angle)) * r);
                    u.Add(new Vector2(j / (float)sides, i / (float)(path.Length - 1)));
                    if (i == path.Length - 1 || j == sides) continue;
                    int n = i * (sides + 1) + j;
                    t.AddRange(new[] { n, n + 1, n + sides + 1, n + 1, n + sides + 2, n + sides + 1 });
                }
            }
            return Complete(name, v, u, t);
        }

        public static Mesh Transform(Mesh source, Matrix4x4 matrix)
        {
            var vertices = source.vertices; var normals = source.normals;
            Matrix4x4 normalMatrix = matrix.inverse.transpose;
            for (int i = 0; i < vertices.Length; i++)
            { vertices[i] = matrix.MultiplyPoint3x4(vertices[i]); normals[i] = normalMatrix.MultiplyVector(normals[i]).normalized; }
            source.vertices = vertices; source.normals = normals; source.RecalculateBounds(); source.RecalculateTangents(); return source;
        }

        public static Mesh Save(Mesh mesh, string name)
        {
            string path = Art + "/Meshes/" + name + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing)
            {
                // Replacing native mesh buffers explicitly also invalidates GPU skinning caches.
                existing.Clear(false);existing.indexFormat=mesh.indexFormat;
                existing.vertices=mesh.vertices;existing.normals=mesh.normals;existing.uv=mesh.uv;existing.tangents=mesh.tangents;
                existing.boneWeights=mesh.boneWeights;existing.bindposes=mesh.bindposes;
                existing.subMeshCount=mesh.subMeshCount;
                for(int i=0;i<mesh.subMeshCount;i++)existing.SetIndices(mesh.GetIndices(i),mesh.GetTopology(i),i,false);
                existing.bounds=mesh.bounds;existing.UploadMeshData(false);EditorUtility.SetDirty(existing);
                UnityEngine.Object.DestroyImmediate(mesh);return existing;
            }
            AssetDatabase.CreateAsset(mesh, path); return mesh;
        }
        private static Mesh Complete(string name, List<Vector3> verts, List<Vector2> uv, List<int> tris)
        {
            var mesh = new Mesh { name = name };
            if (verts.Count > 65535) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(verts); mesh.SetUVs(0, uv); mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals(); mesh.RecalculateTangents(); mesh.RecalculateBounds(); return mesh;
        }
    }
}
