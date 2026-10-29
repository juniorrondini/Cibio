using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Cibio.City.Editor
{
    public static class CityVerification
    {
        public static void ValidateAssets()
        {
            EditorSceneManager.OpenScene(CityBuilder.Scene);
            var player=UnityEngine.Object.FindAnyObjectByType<CityMotor>();var camera=UnityEngine.Object.FindAnyObjectByType<CityCamera>();
            Require(player&&camera&&camera.target==player,"Vinculo entre camera e jogador");
            Require(player.view==camera.GetComponent<Camera>(),"Movimento relativo a camera");
            var skin=player.visual.GetComponent<SkinnedMeshRenderer>();
            Require(skin&&skin.bones.Length==17,"Esqueleto com 17 articulacoes");
            var mesh=skin.sharedMesh;Require(mesh.vertexCount>20000,"Malha refinada com mais de 20 mil vertices");
            Require(mesh.boneWeights.Length==mesh.vertexCount&&mesh.bindposes.Length==17,"Pesos e poses de referencia completos");
            foreach(var w in mesh.boneWeights)
                Require(Mathf.Abs(w.weight0+w.weight1+w.weight2+w.weight3-1)<.001f,"Soma dos pesos da malha");
            foreach(string name in new[]{"Idle","Walk","Run","Jump","Fall","Land","Attack","Death"})
            {
                var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(HeroAnimations.Folder+"/"+name+".anim");
                Require(clip&&clip.length>.1f&&AnimationUtility.GetCurveBindings(clip).Length>60,"Clip valido: "+name);
            }
            foreach(var root in player.gameObject.scene.GetRootGameObjects())
            foreach(var child in root.GetComponentsInChildren<Transform>(true))
                Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject)==0,"Scripts completos: "+child.name);
            foreach(var renderer in UnityEngine.Object.FindObjectsByType<Renderer>())
            {
                if(renderer.GetComponent<TextMesh>())continue;
                foreach(var material in renderer.sharedMaterials)Require(material&&material.shader&&material.shader.isSupported,"Material e shader: "+renderer.name);
            }
            Directory.CreateDirectory("CityReview");
            File.WriteAllText("CityReview/AssetValidation.txt","PASS Links, skeleton, skin weights, 8 animation clips, scene scripts and materials.\nVertices: "+mesh.vertexCount+"\nBones: "+skin.bones.Length+"\n");
            Debug.Log("CIBIO_CITY_ASSETS_OK: "+mesh.vertexCount+" vertices; 17 bones; 8 clips.");
        }
        public static void Run()
        {
            ValidateAssets();
            SessionState.SetBool("Cibio.CityVerify",true);EditorApplication.isPlaying=true;
        }
        private static void Require(bool condition,string context){if(!condition)throw new InvalidOperationException(context);}
    }
}
