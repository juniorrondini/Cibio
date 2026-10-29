using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Cibio.RPG.Editor
{
    public static class RpgBuilder
    {
        public const string Root="Assets/Cibio/RPG";
        public const string Scene=Root+"/Scenes/03_VilaDaCentelha.unity";
        [MenuItem("Cibio/RPG/Abrir Vila da Centelha")]
        public static void Open(){if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())EditorSceneManager.OpenScene(Scene);}
        [MenuItem("Cibio/RPG/Criar cena RPG")]
        public static void Build()
        {
            Directory.CreateDirectory(Root+"/Scenes");AssetDatabase.Refresh();
            foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{Root+"/Resources/RpgArt"}))
            {
                var importer=AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid)) as TextureImporter;
                if(importer&&importer.npotScale!=TextureImporterNPOTScale.None){importer.npotScale=TextureImporterNPOTScale.None;importer.SaveAndReimport();}
            }
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var cameraObject=new GameObject("Câmera • pixel RPG");cameraObject.tag="MainCamera";var camera=cameraObject.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=7.03125f;camera.backgroundColor=RpgArt.Hex("243D34");camera.clearFlags=CameraClearFlags.SolidColor;camera.nearClipPlane=.1f;camera.farClipPlane=100;camera.transform.position=new Vector3(0,-2.2f,-10);camera.allowHDR=false;camera.allowMSAA=false;cameraObject.AddComponent<AudioListener>();
            var root=new GameObject("CIBIO • Vila da Centelha");var world=root.AddComponent<RpgWorld>();var game=root.AddComponent<RpgGame>();root.AddComponent<RpgInterface>();root.AddComponent<RpgVerification>();
            var mat=new Material(Shader.Find("Sprites/Default")){name="Pixel Energy"};string path=Root+"/Resources/RpgArt/Energy.mat";var existing=AssetDatabase.LoadAssetAtPath<Material>(path);if(existing){EditorUtility.CopySerialized(mat,existing);Object.DestroyImmediate(mat);mat=existing;}else AssetDatabase.CreateAsset(mat,path);game.effectsMaterial=mat;
            world.Build();Directory.CreateDirectory(Root+"/Art/Sprites");AssetDatabase.Refresh();
            foreach(var renderer in root.GetComponentsInChildren<SpriteRenderer>())
            {
                var s=renderer.sprite;var r=s.rect;string key=s.texture.name+"_"+(int)r.x+"_"+(int)r.y+"_"+(int)r.width+"_"+(int)r.height;string spritePath=Root+"/Art/Sprites/"+key+".asset";
                var asset=AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);if(!asset){s.name=key;AssetDatabase.CreateAsset(s,spritePath);asset=s;}renderer.sprite=asset;
            }
            game.world=world;game.lens=camera;
            EditorSceneManager.SaveScene(scene,Scene);EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Scene,true)};
            PlayerSettings.productName="Cibio - Vila da Centelha";PlayerSettings.companyName="Cibio";PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.runInBackground=true;
            QualitySettings.antiAliasing=0;QualitySettings.vSyncCount=1;
            AssetDatabase.SaveAssets();CaptureOverview(camera);Debug.Log("CIBIO_RPG_SCENE_OK");
        }
        public static void ValidateSaved()
        {
            EditorSceneManager.OpenScene(Scene);var world=Object.FindAnyObjectByType<RpgWorld>();var game=Object.FindAnyObjectByType<RpgGame>();
            if(!world||!game||!world.built||!world.hero||world.shrines.Count!=3||world.enemies.Count!=14||world.solids.Count<30)throw new System.Exception("Cena RPG incompleta");
            int renderers=0;
            foreach(var renderer in world.GetComponentsInChildren<SpriteRenderer>()){if(!renderer.sprite||!renderer.sprite.texture)throw new System.Exception("Sprite sem referência: "+renderer.name);renderers++;}
            foreach(var t in world.GetComponentsInChildren<Transform>())if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0)throw new System.Exception("Script ausente: "+t.name);
            if(!game.effectsMaterial||!game.lens)throw new System.Exception("Câmera ou material sem referência");
            foreach(string action in new[]{"walk","run","jump","slash","spellcast"}){var t=RpgArt.Texture("Hero_"+action);var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(t));if(importer.npotScale!=TextureImporterNPOTScale.None||importer.filterMode!=FilterMode.Point)throw new System.Exception("Importação incorreta: "+action);}
            Directory.CreateDirectory("RpgReview");File.WriteAllText("RpgReview/Validacao-Cena.txt","PASS cena persistida, "+renderers+" sprites, "+world.solids.Count+" obstáculos, câmera, material, scripts, 3 altares, 14 inimigos e importação sem redimensionamento.\n");Debug.Log("CIBIO_RPG_SAVED_SCENE_OK "+renderers);
        }
        static void CaptureOverview(Camera camera)
        {
            Directory.CreateDirectory("RpgReview");float size=camera.orthographicSize;Vector3 position=camera.transform.position;var rt=new RenderTexture(1800,1400,24);camera.orthographicSize=19;camera.transform.position=new Vector3(0,0,-10);camera.targetTexture=rt;camera.Render();var previous=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(1800,1400,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1800,1400),0,0);image.Apply();File.WriteAllBytes("RpgReview/Mapa-Completo.png",image.EncodeToPNG());RenderTexture.active=previous;camera.targetTexture=null;camera.orthographicSize=size;camera.transform.position=position;Object.DestroyImmediate(image);Object.DestroyImmediate(rt);
        }
        public static void BuildPlayer()
        {
            Build();string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../../outputs/Cibio_RPG_03/Cibio.exe"));
            // An explicit argument keeps builds independent of the source project's folder depth.
            var args=System.Environment.GetCommandLineArgs();for(int i=0;i<args.Length-1;i++)if(args[i]=="-cibio-build")output=args[i+1];
            Directory.CreateDirectory(Path.GetDirectoryName(output));var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Scene},locationPathName=output,target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Falha no build: "+report.summary.result);Debug.Log("CIBIO_RPG_PLAYER_OK "+output);
        }
        public static void BuildUpdatedPlayer()
        {
            // Preserve the authored scene and every existing GUID; this update changes runtime art and behavior.
            AssetDatabase.Refresh();
            var volumeShader=AssetDatabase.LoadAssetAtPath<Shader>(Root+"/Resources/RpgArt/EnergyVolume06.shader");if(!volumeShader)throw new System.Exception("Shader volumétrico ausente");
            string volumePath=Root+"/Resources/RpgArt/EnergyVolume06.mat";var volume=AssetDatabase.LoadAssetAtPath<Material>(volumePath);if(!volume)AssetDatabase.CreateAsset(new Material(volumeShader),volumePath);else{volume.shader=volumeShader;EditorUtility.SetDirty(volume);}
            var shader=AssetDatabase.LoadAssetAtPath<Shader>(Root+"/Resources/RpgArt/OrganicEnergy.shader");if(!shader)throw new System.Exception("Shader orgânico ausente");
            string materialPath=Root+"/Resources/RpgArt/OrganicEnergy.mat";var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if(!material)AssetDatabase.CreateAsset(new Material(shader),materialPath);else {material.shader=shader;EditorUtility.SetDirty(material);}
            foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{Root+"/Resources/RpgArt"}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);var importer=AssetImporter.GetAtPath(path) as TextureImporter;var filter=path.Contains("Vfx_")||path.Contains("Title_")||path.Contains("Terrain_")||path.Contains("UI_")?FilterMode.Bilinear:FilterMode.Point;
                if(importer&&(importer.npotScale!=TextureImporterNPOTScale.None||importer.filterMode!=filter)){importer.npotScale=TextureImporterNPOTScale.None;importer.filterMode=filter;importer.SaveAndReimport();}
            }
            AssetDatabase.SaveAssets();ValidateSaved();var world=Object.FindAnyObjectByType<RpgWorld>();RpgTerrain.Ensure(world);CaptureOverview(Camera.main);
            string output=Path.GetFullPath("Cibio04/Cibio.exe");var args=System.Environment.GetCommandLineArgs();for(int i=0;i<args.Length-1;i++)if(args[i]=="-cibio-build")output=args[i+1];Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Scene},locationPathName=output,target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Falha: "+report.summary.result);Debug.Log("CIBIO_12_BUILD_OK");
        }
    }
    public sealed class RpgTextureImport:AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if(!assetPath.StartsWith(RpgBuilder.Root+"/Resources/RpgArt/"))return;
            var t=(TextureImporter)assetImporter;t.textureType=TextureImporterType.Default;t.npotScale=TextureImporterNPOTScale.None;t.alphaIsTransparency=true;t.mipmapEnabled=false;t.filterMode=assetPath.Contains("Title_")||assetPath.Contains("Vfx_")||assetPath.Contains("Terrain_")||assetPath.Contains("UI_")?FilterMode.Bilinear:FilterMode.Point;t.wrapMode=TextureWrapMode.Clamp;t.textureCompression=TextureImporterCompression.Uncompressed;t.maxTextureSize=4096;t.sRGBTexture=!assetPath.Contains("Terrain_Mask");if(assetPath.Contains("Terrain_")&&!assetPath.Contains("Terrain_Mask"))t.wrapMode=TextureWrapMode.Repeat;t.isReadable=false;
        }
    }
}
