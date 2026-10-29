using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Cibio.Editor
{
    public static class PrototypeTools
    {
        public const string ScenePath = "Assets/Cibio/Scenes/01_PracaDoComeco.unity";

        [MenuItem("Cibio/Abrir percurso de teste")]
        public static void OpenCourse()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(ScenePath);
            SceneView.lastActiveSceneView?.Frame(new Bounds(new Vector3(4, 2, 0), new Vector3(16, 9, 8)), false);
        }

        [MenuItem("Cibio/Salvar personagem como prefab")]
        public static void SaveCharacter()
        {
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerMotor>();
            if (player == null) throw new InvalidOperationException("Abra o percurso de teste primeiro.");
            PrefabUtility.SaveAsPrefabAsset(player.gameObject, "Assets/Cibio/Prefabs/Brasileiro.prefab");
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Cibio/Verificar referencias da cena")]
        public static void ValidateScene()
        {
            EditorSceneManager.OpenScene(ScenePath);
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerMotor>();
            var camera = UnityEngine.Object.FindAnyObjectByType<SideCamera>();
            var rig = UnityEngine.Object.FindAnyObjectByType<CharacterRig>();
            if (player == null || camera == null || rig == null) throw new InvalidOperationException("Personagem ou câmera ausente.");
            if (camera.target != player || rig.motor != player) throw new InvalidOperationException("Referencias do personagem incompletas.");
            if (rig.leftArm == null || rig.rightArm == null || rig.leftLeg == null || rig.rightLeg == null || rig.torso == null)
                throw new InvalidOperationException("Articulações incompletas.");
            foreach (var root in player.gameObject.scene.GetRootGameObjects())
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject) > 0)
                    throw new InvalidOperationException("Script ausente: " + transform.name);
            Debug.Log("CIBIO: referencias da cena verificadas.");
        }

        // Run with -executeMethod Cibio.Editor.PrototypeTools.CapturePreview.
        public static void CapturePreview()
        {
            ValidateScene();
            var camera = UnityEngine.Object.FindAnyObjectByType<SideCamera>();
            camera.Snap();
            var view = camera.GetComponent<Camera>();
            Capture(view, "Preview-Unity.png", 1600, 900);
            CaptureCharacter();
        }

        private static void CaptureCharacter()
        {
            var character = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Cibio/Prefabs/Brasileiro.prefab"));
            character.transform.position = new Vector3(0, 50, 0);
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = new Vector3(0, 49.91f, 0);
            floor.transform.localScale = new Vector3(100, .1f, 100);
            floor.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Cibio/Art/Materials/Calcada.mat");
            var cameraObject = new GameObject("Character preview camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true; camera.orthographicSize = 1.5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.12f, .22f, .21f);
            camera.transform.position = new Vector3(2.4f, 51.8f, -6f);
            camera.transform.LookAt(new Vector3(0, 51.1f, 0));
            camera.allowHDR = false;
            Capture(camera, "Personagem-Unity.png", 1000, 1100);
            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(character);
            UnityEngine.Object.DestroyImmediate(floor);
        }

        private static void Capture(Camera view, string file, int width, int height)
        {
            var render = new RenderTexture(width, height, 24);
            view.targetTexture = render;
            view.Render();
            var previous = RenderTexture.active;
            RenderTexture.active = render;
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();
            string path = Path.Combine(Path.GetDirectoryName(Application.dataPath), file);
            File.WriteAllBytes(path, texture.EncodeToPNG());
            RenderTexture.active = previous;
            view.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(texture);
            UnityEngine.Object.DestroyImmediate(render);
            Debug.Log("CIBIO: preview salvo em " + path);
        }
    }
}
