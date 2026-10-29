using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Cibio.RPG.Editor
{
 [InitializeOnLoad]
 public static class RpgTerrainPreview
 {
  static RpgTerrainPreview(){EditorApplication.delayCall+=Preview;EditorSceneManager.sceneOpened+=(scene,mode)=>EditorApplication.delayCall+=Preview;}
  [MenuItem("Cibio/RPG/Atualizar prévia do terreno refinado")]
  static void Preview()
  {
   if(Application.isBatchMode||EditorApplication.isPlayingOrWillChangePlaymode)return;
   foreach(var world in Object.FindObjectsByType<RpgWorld>())if(world.built&&world.scenery){var t=RpgTerrain.Ensure(world);t.hideFlags=HideFlags.DontSaveInEditor|HideFlags.DontSaveInBuild;}
   SceneView.RepaintAll();
  }
 }
}

