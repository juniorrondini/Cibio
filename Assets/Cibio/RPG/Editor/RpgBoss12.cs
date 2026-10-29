using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Cibio.RPG.Editor
{
 public static class RpgBoss12
 {
  [MenuItem("Cibio/RPG/Atualizar Companheiro e Seu Bonero")]
  public static void UpdateScene()
  {
   AssetDatabase.Refresh();EditorSceneManager.OpenScene(RpgBuilder.Scene);
   var w=Object.FindAnyObjectByType<RpgWorld>();var q=w.GetComponent<RpgQuest08>();
   if(!q.giver||!q.giver.GetComponent<RpgNpcDavi11>())throw new System.Exception("NPC original ausente");
   q.giver.name="Seu Bonero • missão";q.giver.GetComponent<RpgNpcDavi11>().body.name="Seu Bonero";
   var boss=w.enemies.Single(e=>e.enemyKind==4);boss.name="Companheiro";
   // Clear the amplifier silhouette without replacing either authored object.
   if(Vector2.Distance(boss.Position,new Vector2(0,38.5f))<.01f)
   {
    Vector2 p=new(2.6f,39);if(!w.CanStand(p))throw new System.Exception("Nova ancora do chefe bloqueada");
    boss.transform.position=p;boss.SetHome(p);
   }
   var s=RpgArt.Frame("Companheiro12","idle",2,0);string path=RpgBuilder.Root+"/Art/Sprites/Companheiro12_idle_south_ppu64.asset";
   var saved=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(!saved){AssetDatabase.CreateAsset(s,path);saved=s;}
   boss.body.sprite=saved;boss.body.color=Color.white;
   EditorUtility.SetDirty(q.giver.gameObject);EditorUtility.SetDirty(boss);EditorUtility.SetDirty(boss.body);
   EditorSceneManager.MarkSceneDirty(w.gameObject.scene);EditorSceneManager.SaveScene(w.gameObject.scene);AssetDatabase.SaveAssets();
   Debug.Log("CIBIO_12_SCENE_OK existing boss and NPC updated in place");
  }
  public static void BuildPlayer(){UpdateScene();RpgBuilder.BuildUpdatedPlayer();}
 }
}
