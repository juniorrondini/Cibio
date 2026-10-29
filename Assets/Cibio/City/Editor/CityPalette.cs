using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Cibio.City.Editor
{
    public static class CityPalette
    {
        public static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();
        public static Material Get(string name)
        {
            if(Materials.TryGetValue(name,out var material))return material;
            material=AssetDatabase.LoadAssetAtPath<Material>(MeshWorkshop.Art+"/Materials/"+name+".mat");
            if(!material)throw new System.InvalidOperationException("Material ausente: "+name);
            Materials[name]=material;return material;
        }
        public static Color Hex(string color) { ColorUtility.TryParseHtmlString("#" + color, out var result); return result; }
        public static void Build()
        {
            Materials.Clear();
            Material("Skin", "B87650", .38f); Material("SkinWarm", "CE8B65", .32f);
            Material("Ear", "A85C4F", .25f); Material("Lips", "8B4D3D", .28f);
            Material("Hair", "201C19", .27f); Material("HairLight", "38302A", .29f);
            Material("Eye", "ECE5D9", .72f); Material("Iris", "4D3826", .64f); Material("Pupil", "111B1B", .64f);
            Material("Jersey", "E4BA2F", .2f, 1); Material("Trim", "17735C", .22f, 1);
            Material("Shorts", "234B77", .22f, 1); Material("ShortSeam", "36628B", .27f, 1);
            Material("Socks", "E8E5D7", .15f, 1); Material("Leather", "EEE9DD", .4f);
            Material("Sole", "C3C8BA", .18f); Material("Rubber", "36584C", .21f);
            Material("Brass", "CE9946", .48f, 0, .65f); Material("Metal", "263D3A", .34f, 0, .55f);
            Material("Glass", "597D7C", .76f, 0, .25f); Material("DarkGlass", "1D3B3C", .6f);
            Material("Cream", "DDCBA7", .23f, 2); Material("Peach", "C77D65", .22f, 2);
            Material("Teal", "528E84", .24f, 2); Material("Mustard", "CAA35E", .24f, 2);
            Material("Ivory", "E5DBC5", .2f, 2); Material("Blue", "547F8C", .23f, 2);
            Material("Roof", "A15E43", .22f, 2); Material("RoofLight", "C17C56", .24f, 2);
            Material("Wood", "775642", .27f, 2); Material("WoodLight", "B17A4D", .28f, 2);
            Material("Grass", "688765", .18f, 3); Material("Leaves", "3C7661", .23f);
            Material("LeavesLight", "638B65", .27f); Material("Flower", "CB6F79", .3f);
            Material("Stone", "ABA491", .18f, 2); Material("Pavers", "CEBE9F", .24f, 4);
            Material("PaversDark", "9FA492", .22f, 4); Material("Asphalt", "485B58", .18f, 3);
            Material("Paper", "EDE2C6", .18f, 1); Material("Red", "B64E43", .3f);
            Material("EnemyCoat", "8E5C58", .22f, 1); Material("EnemyPants", "373F50", .23f, 1);
            Material("Water", "5DADA3", .78f, 0, .25f);
            Material("Lamp", "F2D8A3", .5f);
            Get("Lamp").SetColor("_EmissionColor", Hex("EBC387") * .4f);
        }
        private static Material Material(string name, string color, float smooth, int texture = 0, float metal = 0)
        {
            string path = MeshWorkshop.Art + "/Materials/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!mat) { mat = new Material(Shader.Find("Cibio/Refined Surface")); AssetDatabase.CreateAsset(mat, path); }
            mat.shader = Shader.Find("Cibio/Refined Surface"); mat.color = Hex(color);
            mat.SetFloat("_Glossiness", smooth); mat.SetFloat("_Metallic", metal); mat.SetFloat("_Fade", 1);
            mat.SetColor("_EmissionColor", Color.black);
            if (texture != 0)
            {
                mat.SetTexture("_MainTex", MakeTexture(texture));
                mat.mainTextureScale = texture == 1 ? new Vector2(4, 4) : Vector2.one;
            }
            EditorUtility.SetDirty(mat); Materials[name] = mat; return mat;
        }
        private static Texture2D MakeTexture(int kind)
        {
            string path = MeshWorkshop.Art + "/Textures/Surface_" + kind + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing) return existing;
            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true, false) { name = "Surface detail " + kind, wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Trilinear, anisoLevel = 4 };
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                float noise = Mathf.PerlinNoise(x * .13f, y * .13f);
                float value = .92f + noise * .13f;
                if (kind == 1) value = .94f + .035f * Mathf.Sin(x * Mathf.PI) + .035f * Mathf.Sin(y * Mathf.PI * .5f) + noise * .035f;
                if (kind == 2) value = .86f + noise * .15f + Mathf.PerlinNoise(x * .8f, y * .8f) * .07f;
                if (kind == 3) value = .83f + noise * .22f + Mathf.PerlinNoise(x * 1.3f, y * 1.3f) * .05f;
                if (kind == 4)
                {
                    int row = y / 64, ox = (x + (row % 2) * 32) % 64;
                    float seam = ox < 2 || y % 64 < 2 ? .66f : .9f + noise * .1f;
                    value = seam + Mathf.PerlinNoise((x / 64) * 9, row * 6) * .04f;
                }
                pixels[y * size + x] = new Color(value, value, value, 1);
            }
            tex.SetPixels(pixels); tex.Apply(true, false); AssetDatabase.CreateAsset(tex, path); return tex;
        }
    }
}
