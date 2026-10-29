using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using static Cibio.City.Editor.TownArt;
using static Cibio.City.Editor.MeshWorkshop;

namespace Cibio.City.Editor
{
    public static class CityBuilder
    {
        public const string Root="Assets/Cibio/City";
        public const string Scene="Assets/Cibio/City/Scenes/02_PracaDoRespiro.unity";
        public static void BuildAndCapture(){Build();CityPreview.CaptureAll();}
        public static void Build()
        {
            foreach(string folder in new[]{"Art/Materials","Art/Meshes","Art/Textures","Animations","Prefabs","Scenes"})Directory.CreateDirectory(Root+"/"+folder);
            AssetDatabase.Refresh();
            CityPalette.Build();TownArt.Reset();
            var animations=HeroAnimations.Build();
            var hero=HeroArt.Build(false,animations);
            var heroPrefab=PrefabUtility.SaveAsPrefabAsset(hero,Root+"/Prefabs/Brasileiro_Refinado.prefab");UnityEngine.Object.DestroyImmediate(hero);
            var enemy=HeroArt.Build(true,animations);
            var enemyPrefab=PrefabUtility.SaveAsPrefabAsset(enemy,Root+"/Prefabs/Cobrador_do_Caos.prefab");UnityEngine.Object.DestroyImmediate(enemy);
            CreateStreetFurniture();CreateTree();CreateFountain();
            CreateBuilding("Cafe do Respiro","Peach",true);
            CreateBuilding("Armazem da Esquina","Teal",true);
            CreateBuilding("Casa Ocre","Mustard",false);
            CreateBuilding("Sobrado Azul","Blue",false);
            CreateFlowerPot();CreatePaperStack();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            SetupLight();
            var map=Group("Bairro • exploracao XZ");BuildGround(map);BuildDistrict(map);
            var player=(GameObject)PrefabUtility.InstantiatePrefab(heroPrefab);player.name="Brasileiro • jogador";
            player.transform.position=new Vector3(0,.18f,-6);
            var motor=player.GetComponent<CityMotor>();motor.spawn=player.transform.position;
            SetActorLayer(player);
            var cameraObject=new GameObject("Main Camera");cameraObject.tag="MainCamera";
            var camera=cameraObject.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=7.4f;
            camera.nearClipPlane=.2f;camera.farClipPlane=110;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=CityPalette.Hex("BCD0C2");
            camera.allowHDR=true;camera.allowMSAA=true;cameraObject.AddComponent<AudioListener>();
            var rig=cameraObject.AddComponent<CityCamera>();rig.target=motor;rig.Snap();motor.view=camera;
            cameraObject.AddComponent<CameraObstruction>();
            var grade=cameraObject.AddComponent<CityColorGrade>();
            string gradePath=Root+"/Art/Materials/ColorGrade.mat";
            grade.grading=AssetDatabase.LoadAssetAtPath<Material>(gradePath);
            if(!grade.grading){grade.grading=new Material(Shader.Find("Hidden/Cibio/ColorGrade"));AssetDatabase.CreateAsset(grade.grading,gradePath);}
            var hud=new GameObject("Interface • bairro").AddComponent<CityHUD>();hud.player=motor;
            Vector3[] patrols={new Vector3(7,.18f,5),new Vector3(-6,.18f,8),new Vector3(13,.04f,-6)};
            for(int i=0;i<patrols.Length;i++)
            {
                var actor=(GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab);actor.name="Cobrador do caos "+(i+1);actor.transform.position=patrols[i];
                var ai=actor.GetComponent<StreetEnemy>();ai.target=motor;ai.detectionRange=5.8f;SetActorLayer(actor);
            }
            PlayerSettings.colorSpace=ColorSpace.Linear;
            PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;
            PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.runInBackground=true;
            PlayerSettings.productName="Cibio • O Brasil virou uma Vuvuzela";
            QualitySettings.antiAliasing=4;QualitySettings.shadowCascades=4;QualitySettings.shadowDistance=65;
            QualitySettings.shadowResolution=ShadowResolution.VeryHigh;QualitySettings.shadows=ShadowQuality.All;
            var tags=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            tags.FindProperty("layers").GetArrayElementAtIndex(8).stringValue="Actors";tags.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene,Scene);
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Scene,true),new EditorBuildSettingsScene("Assets/Cibio/Scenes/01_PracaDoComeco.unity",false)};
            AssetDatabase.SaveAssets();
            Debug.Log("CIBIO_CITY_BUILD_OK: personagem refinado, Animator e bairro salvos.");
        }
        private static void SetActorLayer(GameObject root){foreach(var node in root.GetComponentsInChildren<Transform>(true))node.gameObject.layer=8;}
        private static void SetupLight()
        {
            RenderSettings.skybox=null;RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=CityPalette.Hex("AABFC9");RenderSettings.ambientEquatorColor=CityPalette.Hex("829385");RenderSettings.ambientGroundColor=CityPalette.Hex("665949");
            RenderSettings.ambientIntensity=.75f;RenderSettings.fog=false;
            var sun=new GameObject("Sol • luz da tarde").AddComponent<Light>();sun.type=LightType.Directional;
            sun.transform.rotation=Quaternion.Euler(48,-38,0);sun.color=CityPalette.Hex("FFE5C3");sun.intensity=1.15f;
            sun.shadows=LightShadows.Soft;sun.shadowStrength=.8f;sun.shadowBias=.025f;sun.shadowNormalBias=.25f;
            var fill=new GameObject("Luz difusa do ceu").AddComponent<Light>();fill.type=LightType.Directional;
            fill.transform.rotation=Quaternion.Euler(30,145,0);fill.color=CityPalette.Hex("C7DBEC");fill.intensity=.38f;fill.shadows=LightShadows.None;
            DynamicGI.UpdateEnvironment();
        }
        private static void BuildGround(Transform map)
        {
            var ground=Box("Solo continuo",map,new Vector3(0,-.35f,0),new Vector3(54,.7f,48),"Asphalt",.1f,true);
            ground.GetComponent<Renderer>().sharedMaterial.mainTextureScale=new Vector2(32,32);
            var plaza=Box("Praca pavimentada",map,new Vector3(0,.02f,0),new Vector3(20,.20f,21),"Pavers",.09f,true);
            plaza.GetComponent<Renderer>().sharedMaterial.mainTextureScale=new Vector2(9,9);
            // Continuous ground with low curbs: no side-scrolling gaps or suspended platforms.
            foreach(float x in new[]{-10f,10f})
            {
                Box("Meio-fio",map,new Vector3(x,.10f,0),new Vector3(.22f,.22f,21.2f),"Ivory",.04f,true);
                Box("Faixa lateral",map,new Vector3(x*.88f,.127f,0),new Vector3(.38f,.018f,20.6f),"PaversDark",.01f);
            }
            foreach(float z in new[]{-10.5f,10.5f})Box("Borda da praca",map,new Vector3(0,.10f,z),new Vector3(20,.22f,.22f),"Ivory",.04f,true);
            for(int i=0;i<7;i++)Box("Faixa de pedestres",map,new Vector3(-2.7f+i*.9f,.006f,-13),new Vector3(.52f,.012f,3.5f),"Ivory",.02f);
            for(int i=0;i<11;i++)
            {
                float x=-21+i*4.2f;Box("Sinalizacao de rua",map,new Vector3(x,.008f,-17),new Vector3(2.2f,.015f,.08f),"Jersey",.012f);
            }
            // Broad sidewalks wrap the surrounding blocks.
            Box("Passeio norte",map,new Vector3(0,.04f,14),new Vector3(47,.24f,3),"Pavers",.055f,true);
            foreach(float x in new[]{-19f,19f})Box("Passeio lateral",map,new Vector3(x,.04f,0),new Vector3(3,.24f,31),"Pavers",.055f,true);
            // World boundary walls are low enough to leave the street readable.
            foreach(float x in new[]{-23f,23f})Box("Limite do bairro",map,new Vector3(x,.55f,0),new Vector3(.35f,1.1f,42),"Cream",.055f,true);
            Box("Limite norte",map,new Vector3(0,.55f,20),new Vector3(46,1.1f,.35f),"Cream",.055f,true);
            Box("Limite sul",map,new Vector3(0,.45f,-20),new Vector3(46,.9f,.35f),"Cream",.055f,true);
        }
        private static void BuildDistrict(Transform map)
        {
            foreach(var building in new[]{
                ("Cafe_do_Respiro",new Vector3(-6.6f,.16f,16.7f),0f),
                ("Casa_Ocre",new Vector3(0,.16f,16.7f),0f),
                ("Armazem_da_Esquina",new Vector3(6.6f,.16f,16.7f),0f),
                ("Sobrado_Azul",new Vector3(-21,.16f,8.5f),-90f),
                ("Casa_Ocre",new Vector3(-21,.16f,1.7f),-90f),
                ("Armazem_da_Esquina",new Vector3(-21,.16f,-5.2f),-90f),
                ("Casa_Ocre",new Vector3(21,.16f,7.4f),90f),
                ("Sobrado_Azul",new Vector3(21,.16f,.5f),90f),
                ("Cafe_do_Respiro",new Vector3(21,.16f,-6.4f),90f)})
            {
                var obj=Place(building.Item1,map,building.Item2,building.Item3);obj.AddComponent<Occludable>();
            }
            Place("Fonte_de_Pedra",map,new Vector3(0,.12f,2));
            foreach(var position in new[]{new Vector3(-6.5f,0,-6),new Vector3(6.5f,0,-6),new Vector3(-6.5f,0,6.5f),new Vector3(6.5f,0,6.5f)})
            {
                var planter=Group("Canteiro",map,position);
                Box("Mureta de pedra",planter,new Vector3(0,.28f,0),new Vector3(2.9f,.56f,2.9f),"Stone",.17f,true);
                Box("Terra",planter,new Vector3(0,.56f,0),new Vector3(2.62f,.06f,2.62f),"Wood",.12f);
                Place("Ipe_Detalhado",map,position+Vector3.up*.59f,position.x*6,.91f).AddComponent<Occludable>();
                for(int i=0;i<7;i++)
                {float a=i/7f*Mathf.PI*2;Place("Vaso_de_Flores",planter,new Vector3(Mathf.Cos(a)*1.19f,.54f,Mathf.Sin(a)*1.19f),i*51,.47f);}
            }
            foreach(float x in new[]{-4f,4f})
            foreach(float z in new[]{-.7f,4.7f})Place("Banco_Ripado",map,new Vector3(x,.12f,z),x<0?-90:90);
            foreach(float x in new[]{-9.1f,9.1f})foreach(float z in new[]{-8.9f,-.5f,8.8f})Place("Poste_Colonial",map,new Vector3(x,.12f,z),x<0?90:-90);
            Place("Lixeira_Urbana",map,new Vector3(-3,.12f,-1.8f));Place("Lixeira_Urbana",map,new Vector3(4.7f,.12f,6));
            Place("Papelada_Detalhada",map,new Vector3(11.8f,.02f,3.6f),30);
            Place("Papelada_Detalhada",map,new Vector3(-11.8f,.02f,4.4f),-15);
            foreach(float x in new[]{-4.9f,4.9f})
            {
                Box("Bicicletario",map,new Vector3(x,.5f,-9.5f),new Vector3(.06f,.8f,.9f),"Metal",.025f);
                Place("Vaso_de_Flores",map,new Vector3(x,.12f,-8.8f),0,1.1f);
            }
            // A small neighborhood monument carries the vuvuzela motif into the new view.
            var statue=Group("Monumento ao barulho",map,new Vector3(-3.8f,.12f,8.2f));
            Box("Pedestal",statue,new Vector3(0,.35f,0),new Vector3(1.1f,.7f,1.1f),"Cream",.07f,true);
            var hornObject=new GameObject("Vuvuzela polida");hornObject.transform.SetParent(statue,false);hornObject.transform.localPosition=new Vector3(0,.74f,0);hornObject.transform.localRotation=Quaternion.Euler(12,20,-25);
            hornObject.AddComponent<MeshFilter>();hornObject.AddComponent<MeshRenderer>().sharedMaterial=CityPalette.Get("Brass");var horn=hornObject.AddComponent<Cibio.VuvuzelaProp>();
            var hornMesh=UnityEngine.Object.Instantiate(hornObject.GetComponent<MeshFilter>().sharedMesh);hornObject.GetComponent<MeshFilter>().sharedMesh=Save(hornMesh,"Monumento_Vuvuzela");UnityEngine.Object.DestroyImmediate(horn);
            var marker=Group("Placa da praca",map,new Vector3(0,.12f,-9.3f));
            foreach(float x in new[]{-1.3f,1.3f})Box("Suporte",marker,new Vector3(x,.39f,0),new Vector3(.045f,.78f,.045f),"Metal",.012f);
            Box("Placa esmaltada",marker,new Vector3(0,.87f,0),new Vector3(3.1f,.46f,.07f),"Trim",.055f);
            Label("PRACA DO RESPIRO",marker,new Vector3(0,.87f,-.041f),.205f,"Paper");
        }
        private static void CreateFlowerPot()
        {
            var pot=Group("Vaso de flores");
            Lathe("Ceramica",pot,Vector3.zero,new[]{new Ring(0,.16f,.16f),new Ring(.03f,.17f,.17f),new Ring(.36f,.25f,.25f),new Ring(.40f,.28f,.28f),new Ring(.46f,.28f,.28f),new Ring(.46f,.235f,.235f),new Ring(.37f,.23f,.23f)},"RoofLight",40);
            Oval("Terra",pot,new Vector3(0,.39f,0),new Vector3(.23f,.025f,.23f),"Wood");
            for(int i=0;i<12;i++)
            {
                float a=i*2.4f;Vector3 tip=new Vector3(Mathf.Cos(a)*.17f,.55f+Mathf.Sin(i*4)*.12f,Mathf.Sin(a)*.17f);
                Pipe("Haste",pot,new[]{new Vector3(0,.39f,0),tip},.008f,"Leaves",6);
                for(int petal=0;petal<5;petal++)
                {float p=petal*Mathf.PI*.4f;Oval("Petala",pot,tip+new Vector3(Mathf.Cos(p)*.035f,.025f,Mathf.Sin(p)*.035f),new Vector3(.038f,.014f,.025f),i%3==0?"Jersey":"Flower");}
                Oval("Miolo",pot,tip+Vector3.up*.04f,Vector3.one*.015f,"Brass");
            }
            SavePrefab(pot,"Vaso_de_Flores");
        }
        private static void CreatePaperStack()
        {
            var stack=Group("Papelada refinada");
            Box("Mesa",stack,new Vector3(0,.74f,0),new Vector3(1.3f,.09f,.85f),"WoodLight",.035f,true);
            foreach(float x in new[]{-.5f,.5f})foreach(float z in new[]{-.3f,.3f})Box("Pe",stack,new Vector3(x,.35f,z),new Vector3(.045f,.70f,.045f),"Metal",.012f,true);
            for(int i=0;i<14;i++)
            {
                var sheet=Box("Papel",stack,new Vector3(.15f,.81f+i*.014f,0),new Vector3(.48f,.011f,.62f),i%3==0?"Ivory":"Paper",.004f);
                sheet.transform.localRotation=Quaternion.Euler(0,Mathf.Sin(i*7)*7,0);
            }
            Box("Pasta",stack,new Vector3(-.34f,.82f,-.08f),new Vector3(.35f,.055f,.56f),"Red",.015f);
            SavePrefab(stack,"Papelada_Detalhada");
        }
        [MenuItem("Cibio/Abrir bairro refinado")]
        public static void OpenCity()
        {
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            EditorSceneManager.OpenScene(Scene);var player=UnityEngine.Object.FindAnyObjectByType<CityMotor>();
            SceneView.lastActiveSceneView?.LookAt(player.transform.position+Vector3.up,Quaternion.Euler(52,0,0),12);
        }
    }
}
