using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Cibio.Editor
{
    public static class PrototypeBuilder
    {
        private const string Root = "Assets/Cibio";
        private static readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();
        private static readonly List<WorldLabels.Label> labels = new List<WorldLabels.Label>();

        public static void BuildAndPreview()
        {
            Build();
            PrototypeTools.CapturePreview();
        }

        [MenuItem("Cibio/Recriar prototipo inicial")]
        public static void RebuildFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!EditorUtility.DisplayDialog("Recriar protótipo", "Isso recria a cena inicial e os assets gerados. Faça uma cópia de suas alterações antes de continuar.", "Recriar", "Cancelar")) return;
            Build();
        }

        public static void Build()
        {
            foreach (string folder in new[] { "Scenes", "Prefabs", "Art/Materials", "Art/Meshes" })
                Directory.CreateDirectory(Root + "/" + folder);
            AssetDatabase.Refresh();
            materials.Clear(); labels.Clear();
            Mat("Canarinho", "F5C62D"); Mat("Verde", "16806A"); Mat("Azul", "254B83");
            Mat("Pele", "B87950"); Mat("Cabelo", "302620"); Mat("Tenis", "F4EFE1");
            Mat("Asfalto", "344845"); Mat("Calcada", "E7D9B7"); Mat("Solo", "AD7146");
            Mat("Papel", "F5ECD5"); Mat("Coral", "D7795E"); Mat("Menta", "78A494");
            Mat("Sombra", "304D4D"); Mat("Janela", "476768"); Mat("Laranja", "E99A49");
            Mat("Nuvem", "F1E4C7"); Mat("Distante", "A5B5A0");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.43f, .48f, .45f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = Hex("C8D6BF");
            RenderSettings.fogStartDistance = 30f; RenderSettings.fogEndDistance = 95f;
            RenderSettings.skybox = null;
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowDistance = 55;
            QualitySettings.antiAliasing = 4;
            var sun = new GameObject("Sol • fim de tarde").AddComponent<Light>();
            sun.type = LightType.Directional; sun.intensity = .85f; sun.color = Hex("FFE6D0");
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(45, -35, 0);

            CreatePlatformPrefab();
            var player = BuildCharacter();
            var cam = new GameObject("Main Camera"); cam.tag = "MainCamera";
            var lens = cam.AddComponent<Camera>();
            lens.orthographic = true; lens.orthographicSize = 5.2f;
            lens.clearFlags = CameraClearFlags.SolidColor; lens.backgroundColor = Hex("C8D6BF");
            lens.nearClipPlane = .1f; lens.farClipPlane = 160;
            lens.allowHDR = false; lens.allowMSAA = true;
            cam.AddComponent<AudioListener>();
            var camera = cam.AddComponent<SideCamera>(); camera.target = player; camera.Snap();
            var hud = new GameObject("Interface • controles").AddComponent<PrototypeHUD>(); hud.player = player;
            BuildCourse();
            BuildNeighborhood();
            var lettering = new GameObject("Letreiros").AddComponent<WorldLabels>();
            lettering.labels = labels.ToArray(); lettering.enabled = false; lettering.enabled = true;

            PlayerSettings.companyName = "Cibio";
            PlayerSettings.productName = "O Brasil virou uma Vuvuzela";
            PlayerSettings.defaultScreenWidth = 1440; PlayerSettings.defaultScreenHeight = 810;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.runInBackground = true;
            EditorSettings.serializationMode = SerializationMode.ForceText;
            // No downloadable Input System dependency: use the built-in keyboard backend.
            var playerSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            var inputMode = playerSettings.FindProperty("activeInputHandler");
            if (inputMode != null) { inputMode.intValue = 0; playerSettings.ApplyModifiedPropertiesWithoutUndo(); }
            EditorSceneManager.SaveScene(scene, PrototypeTools.ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(PrototypeTools.ScenePath, true) };
            AssetDatabase.SaveAssets();
            File.WriteAllText("ProjectSettings/CibioBuild.txt", "Prototype 01 generated successfully.\nUnity " + Application.unityVersion + "\n");
            Debug.Log("CIBIO_BUILD_OK: cena, personagem e assets salvos.");
        }

        private static PlayerMotor BuildCharacter()
        {
            var root = new GameObject("Brasileiro");
            root.transform.position = new Vector3(0, .08f, 0);
            var capsule = root.AddComponent<CharacterController>();
            capsule.height = 2.18f; capsule.radius = .32f; capsule.center = new Vector3(0, 1.09f, 0);
            capsule.skinWidth = .025f; capsule.stepOffset = .2f; capsule.slopeLimit = 45;
            capsule.minMoveDistance = 0;
            var motor = root.AddComponent<PlayerMotor>();
            var visual = Group("Visual articulado", root.transform);
            visual.localRotation = Quaternion.Euler(0, -20, 0); motor.visual = visual;
            var torso = Group("Tronco", visual, new Vector3(0, 1.15f, 0));
            Shape("Camiseta do Brasil", PrimitiveType.Cube, torso, new Vector3(0, .14f, 0), new Vector3(.67f, .65f, .37f), "Canarinho");
            Shape("Gola verde", PrimitiveType.Cube, torso, new Vector3(0, .47f, -.015f), new Vector3(.3f, .065f, .39f), "Verde");
            Shape("Barra da camisa", PrimitiveType.Cube, torso, new Vector3(0, -.16f, 0), new Vector3(.68f, .065f, .38f), "Verde");
            Shape("Escudo original", PrimitiveType.Cube, torso, new Vector3(.18f, .25f, -.196f), new Vector3(.115f, .15f, .025f), "Verde");
            Shape("Centro do escudo", PrimitiveType.Sphere, torso, new Vector3(.18f, .26f, -.216f), new Vector3(.068f, .068f, .02f), "Azul");
            // A small original jersey emblem; no official team branding or paid assets.
            Shape("Shorts", PrimitiveType.Cube, visual, new Vector3(0, .88f, 0), new Vector3(.60f, .28f, .35f), "Azul");
            Shape("Pescoco", PrimitiveType.Cylinder, visual, new Vector3(0, 1.65f, 0), new Vector3(.19f, .105f, .19f), "Pele");
            var head = Group("Cabeca", visual, new Vector3(0, 1.91f, 0));
            Shape("Rosto", PrimitiveType.Sphere, head, Vector3.zero, new Vector3(.60f, .64f, .52f), "Pele");
            Shape("Cabelo", PrimitiveType.Sphere, head, new Vector3(0, .20f, .035f), new Vector3(.62f, .31f, .54f), "Cabelo");
            for (int i = 0; i < 6; i++)
                Shape("Cacho", PrimitiveType.Sphere, head, new Vector3(-.22f + i * .088f, .27f + Mathf.Sin(i * 2f) * .025f, -.06f), new Vector3(.17f, .16f, .28f), "Cabelo");
            foreach (int side in new[] { -1, 1 })
            {
                Shape("Orelha", PrimitiveType.Sphere, head, new Vector3(side * .29f, -.015f, 0), new Vector3(.12f, .18f, .14f), "Pele");
                Shape("Olho", PrimitiveType.Sphere, head, new Vector3(side * .105f, .005f, -.248f), new Vector3(.046f, .063f, .025f), "Cabelo");
                var brow = Shape("Sobrancelha cansada", PrimitiveType.Cube, head, new Vector3(side * .107f, .083f, -.243f), new Vector3(.10f, .024f, .026f), "Cabelo");
                brow.transform.localRotation = Quaternion.Euler(0, 0, side * 10f);
            }
            Shape("Nariz", PrimitiveType.Sphere, head, new Vector3(0, -.065f, -.274f), new Vector3(.09f, .10f, .09f), "Pele");
            Shape("Boca", PrimitiveType.Cube, head, new Vector3(0, -.17f, -.222f), new Vector3(.13f, .023f, .02f), "Cabelo");
            var leftArm = Limb("Braco esquerdo", visual, new Vector3(-.445f, 1.53f, 0), true);
            var rightArm = Limb("Braco direito", visual, new Vector3(.445f, 1.53f, 0), true);
            var leftLeg = Limb("Perna esquerda", visual, new Vector3(-.18f, .84f, 0), false);
            var rightLeg = Limb("Perna direita", visual, new Vector3(.18f, .84f, 0), false);
            var rig = visual.gameObject.AddComponent<CharacterRig>(); rig.motor = motor;
            rig.torso = torso; rig.head = head; rig.leftArm = leftArm; rig.rightArm = rightArm; rig.leftLeg = leftLeg; rig.rightLeg = rightLeg;
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, Root + "/Prefabs/Brasileiro.prefab");
            UnityEngine.Object.DestroyImmediate(root);
            return ((GameObject)PrefabUtility.InstantiatePrefab(prefab)).GetComponent<PlayerMotor>();
        }

        private static Transform Limb(string name, Transform parent, Vector3 position, bool arm)
        {
            var joint = Group(name, parent, position);
            if (arm)
            {
                Shape("Manga", PrimitiveType.Cube, joint, new Vector3(0, -.12f, 0), new Vector3(.22f, .27f, .36f), "Canarinho");
                Shape("Punho verde", PrimitiveType.Cube, joint, new Vector3(0, -.25f, 0), new Vector3(.23f, .055f, .37f), "Verde");
                Shape("Antebraco", PrimitiveType.Capsule, joint, new Vector3(0, -.40f, 0), new Vector3(.16f, .15f, .18f), "Pele");
                Shape("Mao", PrimitiveType.Sphere, joint, new Vector3(0, -.59f, -.01f), new Vector3(.20f, .23f, .20f), "Pele");
            }
            else
            {
                Shape("Barra do short", PrimitiveType.Cube, joint, new Vector3(0, -.09f, 0), new Vector3(.25f, .24f, .33f), "Azul");
                Shape("Canela", PrimitiveType.Capsule, joint, new Vector3(0, -.35f, 0), new Vector3(.19f, .18f, .21f), "Pele");
                Shape("Meia", PrimitiveType.Cylinder, joint, new Vector3(0, -.57f, 0), new Vector3(.21f, .09f, .22f), "Tenis");
                Shape("Tenis", PrimitiveType.Cube, joint, new Vector3(0, -.73f, -.08f), new Vector3(.27f, .18f, .44f), "Tenis");
                Shape("Solado", PrimitiveType.Cube, joint, new Vector3(0, -.81f, -.08f), new Vector3(.28f, .04f, .45f), "Verde");
            }
            return joint;
        }

        private static void CreatePlatformPrefab()
        {
            var root = new GameObject("Modulo de calcada");
            Shape("Base", PrimitiveType.Cube, root.transform, new Vector3(0, -.45f, 0), new Vector3(4, .9f, 3), "Solo", true);
            Shape("Piso", PrimitiveType.Cube, root.transform, new Vector3(0, -.04f, 0), new Vector3(4, .08f, 3.05f), "Calcada");
            PrefabUtility.SaveAsPrefabAsset(root, Root + "/Prefabs/Calcada.prefab");
            UnityEngine.Object.DestroyImmediate(root);
        }

        private static void BuildCourse()
        {
            var course = Group("Percurso • plano XY", null);
            foreach (var span in new[] { new Vector2(-9, 8), new Vector2(10.5f, 24), new Vector2(27, 41), new Vector2(43.5f, 60) })
            {
                float width = span.y - span.x;
                var block = Group("Trecho de calcada", course, new Vector3((span.x + span.y) / 2, 0, 0));
                Shape("Fundacao", PrimitiveType.Cube, block, new Vector3(0, -1.15f, 0), new Vector3(width, 2.3f, 3.4f), "Solo", true);
                Shape("Calcada", PrimitiveType.Cube, block, new Vector3(0, -.06f, 0), new Vector3(width, .12f, 3.5f), "Calcada");
                Shape("Faixa verde", PrimitiveType.Cube, block, new Vector3(0, -.29f, -1.72f), new Vector3(width, .12f, .05f), "Verde");
                for (float x = span.x + .7f; x < span.y; x += 1.4f)
                    Shape("Junta de piso", PrimitiveType.Cube, course, new Vector3(x, .003f, 0), new Vector3(.018f, .007f, 3.4f), "Solo");
            }
            Platform(4.6f, .65f, 2.2f, "Degrau 01");
            Platform(7.4f, 1.35f, 1.7f, "Degrau 02");
            Platform(11.6f, 1.6f, 2.2f, "Degrau 03");
            Platform(22.9f, .8f, 1.8f, "Antes do salto");
            Platform(26, 1.3f, 1.7f, "Ilha da papelada");
            Platform(29, 1.8f, 2.2f, "Depois do salto");
            Platform(40, .8f, 1.8f, "Ultimo salto");
            Platform(44.5f, .8f, 2.2f, "Aterrissagem");
            foreach (float x in new[] { 19f, 37f })
            {
                var post = Group("Ponto de respiro", course, new Vector3(x, 0, .9f));
                Shape("Haste", PrimitiveType.Cylinder, post, new Vector3(0, 1.4f, 0), new Vector3(.08f, 1.4f, .08f), "Verde");
                Shape("Bandeira", PrimitiveType.Cube, post, new Vector3(.4f, 2.5f, 0), new Vector3(.8f, .48f, .035f), "Canarinho");
                Shape("Marca no piso", PrimitiveType.Cube, post, new Vector3(0, .012f, -.9f), new Vector3(1.4f, .025f, 3.3f), "Menta");
            }
            // A paper stack is an obstacle, with its own editable prefab.
            var paperwork = new GameObject("Pilha de boletos");
            for (int i = 0; i < 9; i++)
            {
                var sheet = Shape("Papel", PrimitiveType.Cube, paperwork.transform, new Vector3(0, .07f + i * .13f, 0), new Vector3(1.15f, .115f, .75f), i % 3 == 0 ? "Calcada" : "Papel", i == 0);
                sheet.transform.localRotation = Quaternion.Euler(0, i % 2 == 0 ? -6f : 6f, 0);
            }
            var paperCollider = paperwork.AddComponent<BoxCollider>(); paperCollider.center = new Vector3(0, .61f, 0); paperCollider.size = new Vector3(1.2f, 1.22f, .9f);
            var paperPrefab = PrefabUtility.SaveAsPrefabAsset(paperwork, Root + "/Prefabs/PilhaDeBoletos.prefab");
            paperwork.transform.position = new Vector3(33, 0, 0); paperwork.transform.SetParent(course);
            Sign("01 / PRACA DO COMECO", new Vector3(-3, 3.6f, 2), 4.6f, .24f);
            Sign("02 / A CONTA CHEGOU", new Vector3(17, 3.5f, 2.5f), 4.4f, .23f);
            Sign("03 / HAJA PAPELADA", new Vector3(34, 3.8f, 2.5f), 4.4f, .23f);
            Sign("UM PULO DE CADA VEZ.", new Vector3(53, 3.2f, 1), 5, .27f);
            Label("A / D  MOVER     ESPACO  PULAR", new Vector3(1.7f, -.72f, -1.77f), .16f, "Papel");
            Label("SEGURE SHIFT PARA GANHAR DISTANCIA", new Vector3(20.5f, -.72f, -1.77f), .13f, "Papel");
        }

        private static void Platform(float x, float top, float width, string name)
        {
            var parent = Group(name, null, new Vector3(x, top, 0));
            Shape("Plataforma", PrimitiveType.Cube, parent, new Vector3(0, -.22f, 0), new Vector3(width, .44f, 2.3f), "Verde", true);
            Shape("Borda amarela", PrimitiveType.Cube, parent, new Vector3(0, -.03f, 0), new Vector3(width + .04f, .06f, 2.35f), "Canarinho");
        }

        private static void BuildNeighborhood()
        {
            var city = Group("Bairro • cenario em profundidade", null);
            var random = new System.Random(72);
            string[] colors = { "Coral", "Menta", "Laranja", "Calcada" };
            for (int i = 0; i < 17; i++)
            {
                float x = -14 + i * 4.8f, h = 2.4f + (float)random.NextDouble() * 3.4f;
                var house = Group("Casa " + (i + 1), city, new Vector3(x, 0, 6 + (i % 2) * 1.5f));
                Shape("Fachada", PrimitiveType.Cube, house, new Vector3(0, h / 2, 0), new Vector3(4.35f, h, 3), colors[i % colors.Length]);
                Shape("Platibanda", PrimitiveType.Cube, house, new Vector3(0, h + .10f, 0), new Vector3(4.55f, .20f, 3.2f), "Calcada");
                Shape("Porta", PrimitiveType.Cube, house, new Vector3(-1.05f, .95f, -1.53f), new Vector3(.75f, 1.9f, .08f), "Sombra");
                for (int row = 0; row < (h > 4 ? 2 : 1); row++)
                foreach (float wx in new[] { -.85f, .85f })
                {
                    float wy = 2f + row * 1.5f;
                    Shape("Moldura", PrimitiveType.Cube, house, new Vector3(wx, wy, -1.53f), new Vector3(.92f, .95f, .10f), "Calcada");
                    Shape("Janela", PrimitiveType.Cube, house, new Vector3(wx, wy, -1.60f), new Vector3(.72f, .75f, .06f), "Janela");
                    Shape("Divisoria", PrimitiveType.Cube, house, new Vector3(wx, wy, -1.64f), new Vector3(.045f, .75f, .035f), "Calcada");
                }
                if (i % 3 == 0)
                    Shape("Caixa d'agua", PrimitiveType.Cylinder, house, new Vector3(.8f, h + .55f, .2f), new Vector3(1, .45f, 1), "Azul");
            }
            for (int i = 0; i < 13; i++)
            {
                float x = -25 + i * 8f;
                Shape("Silhueta da cidade", PrimitiveType.Cube, city, new Vector3(x, 2.5f, 19), new Vector3(6, 5 + i % 4, 4), "Distante");
                Shape("Nuvem", PrimitiveType.Sphere, city, new Vector3(x + 2, 10 + i % 3, 25), new Vector3(8, 1.2f, 2), "Nuvem");
            }
            foreach (float x in new[] { -5f, 15f, 35f, 51f })
            {
                var tree = Group("Arvore da praca", city, new Vector3(x, 0, 3.2f));
                Shape("Tronco", PrimitiveType.Cylinder, tree, new Vector3(0, 1.35f, 0), new Vector3(.25f, 1.35f, .25f), "Solo");
                Shape("Copa", PrimitiveType.Sphere, tree, new Vector3(0, 3f, 0), new Vector3(2.6f, 2.5f, 1.8f), "Verde");
                Shape("Copa clara", PrimitiveType.Sphere, tree, new Vector3(-.65f, 3.35f, -.15f), new Vector3(1.5f, 1.6f, 1.4f), "Menta");
            }
            BuildHorn(city);
            var bench = Group("Banco da praca", city, new Vector3(-1.5f, 0, 2.8f));
            Shape("Assento", PrimitiveType.Cube, bench, new Vector3(0, .65f, 0), new Vector3(2.3f, .15f, .6f), "Coral");
            Shape("Encosto", PrimitiveType.Cube, bench, new Vector3(0, 1.05f, .25f), new Vector3(2.3f, .55f, .12f), "Coral");
            foreach (float x in new[] { -.8f, .8f })
                Shape("Pe do banco", PrimitiveType.Cube, bench, new Vector3(x, .3f, 0), new Vector3(.12f, .6f, .5f), "Sombra");
        }

        private static void BuildHorn(Transform city)
        {
            var horn = new GameObject("Vuvuzela");
            horn.AddComponent<MeshFilter>(); horn.AddComponent<MeshRenderer>().sharedMaterial = materials["Canarinho"];
            var generator = horn.AddComponent<VuvuzelaProp>();
            var mesh = UnityEngine.Object.Instantiate(horn.GetComponent<MeshFilter>().sharedMesh);
            string path = Root + "/Art/Meshes/Vuvuzela.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null) AssetDatabase.CreateAsset(mesh, path);
            else { EditorUtility.CopySerialized(mesh, existing); UnityEngine.Object.DestroyImmediate(mesh); mesh = existing; }
            horn.GetComponent<MeshFilter>().sharedMesh = mesh;
            UnityEngine.Object.DestroyImmediate(generator);
            Shape("Bocal", PrimitiveType.Cylinder, horn.transform, new Vector3(0, .07f, 0), new Vector3(.20f, .09f, .20f), "Verde");
            var prefab = PrefabUtility.SaveAsPrefabAsset(horn, Root + "/Prefabs/Vuvuzela.prefab");
            UnityEngine.Object.DestroyImmediate(horn);
            foreach (float x in new[] { 12f, 31f, 49f })
            {
                var prop = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                prop.transform.SetParent(city); prop.transform.position = new Vector3(x, .8f, 4);
                prop.transform.rotation = Quaternion.Euler(-20, 0, -35);
                prop.transform.localScale = Vector3.one * 1.5f;
                Shape("Pedestal", PrimitiveType.Cube, city, new Vector3(x, .4f, 4), new Vector3(1.6f, .8f, 1.4f), "Calcada");
            }
        }

        private static void Sign(string text, Vector3 position, float width, float size)
        {
            var sign = Group(text, null, position);
            Shape("Placa", PrimitiveType.Cube, sign, Vector3.zero, new Vector3(width, .72f, .14f), "Verde");
            Shape("Filete", PrimitiveType.Cube, sign, new Vector3(0, -.32f, -.08f), new Vector3(width, .055f, .04f), "Canarinho");
            foreach (float side in new[] { -1, 1 })
                Shape("Poste", PrimitiveType.Cube, sign, new Vector3(side * (width / 2 - .3f), -position.y / 2, .05f), new Vector3(.08f, position.y, .08f), "Sombra");
            Label(text, position + new Vector3(0, .04f, -.09f), size, "Papel");
        }

        private static void Label(string text, Vector3 position, float size, string color)
        { labels.Add(new WorldLabels.Label { text = text, position = position, size = size, color = materials[color].color }); }

        private static Transform Group(string name, Transform parent, Vector3 position = default)
        { var obj = new GameObject(name); obj.transform.SetParent(parent, false); obj.transform.localPosition = position; return obj.transform; }

        private static GameObject Shape(string name, PrimitiveType type, Transform parent, Vector3 position, Vector3 size, string material, bool collision = false)
        {
            var obj = GameObject.CreatePrimitive(type); obj.name = name;
            obj.transform.SetParent(parent, false); obj.transform.localPosition = position; obj.transform.localScale = size;
            obj.GetComponent<Renderer>().sharedMaterial = materials[material];
            if (!collision) UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());
            return obj;
        }

        private static Material Mat(string name, string hex)
        {
            string path = Root + "/Art/Materials/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) { mat = new Material(Shader.Find("Standard")); AssetDatabase.CreateAsset(mat, path); }
            mat.color = Hex(hex); mat.SetFloat("_Glossiness", .07f); mat.SetFloat("_Metallic", 0);
            materials[name] = mat; return mat;
        }
        private static Color Hex(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out Color color); return color; }
    }
}
