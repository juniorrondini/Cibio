using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Cibio.City.Editor
{
    public static class CityPreview
    {
        public static void InspectSkin()
        {
            EditorSceneManager.OpenScene(CityBuilder.Scene);
            var player=Object.FindAnyObjectByType<CityMotor>();var skin=player.visual.GetComponent<SkinnedMeshRenderer>();
            var mesh=skin.sharedMesh;var weights=mesh.boneWeights;var vertices=mesh.vertices;var bind=mesh.bindposes;
            var baked=new Mesh();skin.BakeMesh(baked);var actual=baked.vertices;float maximum=0;int bad=0;
            for(int i=0;i<vertices.Length;i++)
            {
                var w=weights[i];Vector3 expected=Vector3.zero;
                for(int j=0;j<4;j++){int index=j==0?w.boneIndex0:j==1?w.boneIndex1:j==2?w.boneIndex2:w.boneIndex3;float weight=j==0?w.weight0:j==1?w.weight1:j==2?w.weight2:w.weight3;if(weight>0)expected+=skin.transform.worldToLocalMatrix.MultiplyPoint3x4(skin.bones[index].localToWorldMatrix.MultiplyPoint3x4(bind[index].MultiplyPoint3x4(vertices[i])))*weight;}
                float difference=Vector3.Distance(expected,actual[i]);maximum=Mathf.Max(maximum,difference);if(difference>.01f)bad++;
                if(i%4000==0)Debug.Log("SKIN_SAMPLE "+i+" "+vertices[i]+" expected "+expected+" bake "+actual[i]+" bone "+w.boneIndex0+"/"+w.weight0+" "+w.boneIndex1+"/"+w.weight1);
            }
            Debug.Log("SKIN_INSPECT "+vertices.Length+" bad "+bad+" max "+maximum+" bones "+skin.bones.Length);
        }
        private static string Output=>Path.Combine(Path.GetDirectoryName(Application.dataPath),"CityReview");
        public static void CaptureAll()
        {
            Directory.CreateDirectory(Output);
            EditorSceneManager.OpenScene(CityBuilder.Scene);
            var rig=Object.FindAnyObjectByType<CityCamera>();rig.Snap();
            var lens=rig.GetComponent<Camera>();Render(lens,"City-Gameplay.png",1600,1000);
            Vector3 oldPosition=lens.transform.position;Quaternion oldRotation=lens.transform.rotation;float oldSize=lens.orthographicSize;
            lens.transform.position=new Vector3(19,34,-28);lens.transform.LookAt(new Vector3(0,0,1));lens.orthographicSize=25;
            Render(lens,"City-Overview.png",1800,1500);
            lens.transform.position=oldPosition;lens.transform.rotation=oldRotation;lens.orthographicSize=oldSize;
            CaptureCharacter();
            Debug.Log("CIBIO_CITY_PREVIEWS: "+Output);
        }
        private static void CaptureCharacter()
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(CityBuilder.Root+"/Prefabs/Brasileiro_Refinado.prefab");
            var actor=(GameObject)PrefabUtility.InstantiatePrefab(prefab);actor.transform.position=new Vector3(0,60,0);
            var visual=actor.transform.Find("Visual");visual.localRotation=Quaternion.identity;
            visual.GetComponent<Animator>().enabled=false;
            HeroAnimations.ApplyPose(visual,"Idle",0);
            var skin=visual.GetComponent<SkinnedMeshRenderer>();
            var bakedMesh=new Mesh();skin.BakeMesh(bakedMesh);skin.enabled=false;
            var bakedObject=new GameObject("Baked studio pose");bakedObject.transform.SetParent(visual,false);
            var bakedFilter=bakedObject.AddComponent<MeshFilter>();bakedFilter.sharedMesh=bakedMesh;
            bakedObject.AddComponent<MeshRenderer>().sharedMaterials=skin.sharedMaterials;
            var studioKey=new GameObject("Studio softbox").AddComponent<Light>();studioKey.type=LightType.Directional;studioKey.intensity=.95f;studioKey.color=CityPalette.Hex("E9E9DD");studioKey.transform.rotation=Quaternion.Euler(28,165,0);
            var floor=TownArt.Box("Studio floor",null,new Vector3(0,59.92f,0),new Vector3(200,.12f,200),"Ivory",.02f);
            var cameraObject=new GameObject("Studio camera");var camera=cameraObject.AddComponent<Camera>();
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=CityPalette.Hex("DDD7C8");camera.orthographic=true;camera.orthographicSize=1.14f;
            camera.nearClipPlane=.1f;camera.farClipPlane=20;camera.allowHDR=true;
            cameraObject.AddComponent<CityColorGrade>().grading=AssetDatabase.LoadAssetAtPath<Material>(CityBuilder.Root+"/Art/Materials/ColorGrade.mat");
            var views=new[]{("Front",new Vector3(0,61.15f,4)),("Back",new Vector3(0,61.15f,-4)),("Left",new Vector3(-4,61.15f,0)),("Right",new Vector3(4,61.15f,0)),("Oblique",new Vector3(2.7f,62.7f,3.8f))};
            foreach(var view in views)
            {camera.transform.position=view.Item2;camera.transform.LookAt(new Vector3(0,60.99f,0));Render(camera,"Hero-"+view.Item1+".png",900,1200);}
            camera.orthographicSize=.34f;camera.transform.position=new Vector3(.16f,61.76f,3);camera.transform.LookAt(new Vector3(0,61.74f,.02f));
            Render(camera,"Hero-Face.png",1000,1000);
            camera.orthographicSize=1.6f;camera.transform.position=new Vector3(2.9f,61.85f,4);camera.transform.LookAt(new Vector3(0,61.28f,0));
            Directory.CreateDirectory(Path.Combine(Output,"Frames"));
            int frame=0;
            foreach(string animation in new[]{"Idle","Walk","Run","Jump","Land"})
            {
                var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(HeroAnimations.Folder+"/"+animation+".anim");
                int count=animation=="Idle"?12:animation=="Land"?8:20;
                for(int i=0;i<count;i++)
                {
                    float phase=i/(float)count;
                    var sampled=animation=="Jump"&&phase>.5f?AssetDatabase.LoadAssetAtPath<AnimationClip>(HeroAnimations.Folder+"/Fall.anim"):clip;
                    sampled.SampleAnimation(visual.gameObject,phase*sampled.length);
                    actor.transform.position=new Vector3(0,60+(animation=="Jump"?Mathf.Sin(phase*Mathf.PI)*.85f:0),0);
                    skin.BakeMesh(bakedMesh);
                    Render(camera,"Frames/"+frame.ToString("000")+".png",600,760);frame++;
                }
            }
            Object.DestroyImmediate(cameraObject);Object.DestroyImmediate(actor);Object.DestroyImmediate(floor);Object.DestroyImmediate(studioKey.gameObject);Object.DestroyImmediate(bakedMesh);
        }
        public static void Render(Camera view,string name,int width,int height)
        {
            var render=new RenderTexture(width,height,24,RenderTextureFormat.ARGBHalf);render.antiAliasing=4;
            var previous=RenderTexture.active;var previousTarget=view.targetTexture;
            view.targetTexture=render;view.Render();RenderTexture.active=render;
            var texture=new Texture2D(width,height,TextureFormat.RGB24,false);
            texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();
            File.WriteAllBytes(Path.Combine(Output,name),texture.EncodeToPNG());
            RenderTexture.active=previous;view.targetTexture=previousTarget;Object.DestroyImmediate(texture);Object.DestroyImmediate(render);
        }
    }
}
