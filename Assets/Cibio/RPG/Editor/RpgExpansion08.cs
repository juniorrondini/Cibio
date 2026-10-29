using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Cibio.RPG.Editor
{
 public static class RpgExpansion08
 {
  [MenuItem("Cibio/RPG/Adicionar Morro do Eco (preserva a vila)")]
  public static void Append()
  {
   EditorSceneManager.OpenScene(RpgBuilder.Scene);var w=Object.FindAnyObjectByType<RpgWorld>();if(GameObject.Find("Morro do Eco • expansão 08"))return;
   w.bounds=new Rect(-24,-18,48,60);var root=new GameObject("Morro do Eco • expansão 08").transform;root.SetParent(w.scenery,false);
   // Open only the existing northern entrance, retaining all other authored scenery.
   var corridor=new Rect(-2.8f,12,5.6f,8);int cleared=0;
   foreach(var solid in w.solids)if(solid&&corridor.Overlaps(solid.Bounds)&&solid.name.StartsWith("Tree")){solid.enabled=false;var r=solid.GetComponent<SpriteRenderer>();if(r)r.enabled=false;cleared++;}
   House(w,root,new Vector2(-12,21),0);House(w,root,new Vector2(13,22),1);House(w,root,new Vector2(-13,32),2);House(w,root,new Vector2(12,35),0);
   for(int y=19;y<42;y+=3){Prop(w,root,"Tree_0",new Vector2(-22,y),true);Prop(w,root,"Tree_1",new Vector2(22,y),true);}
   for(int x=-19;x<=19;x+=3)Prop(w,root,x%2==0?"Tree_0":"Tree_1",new Vector2(x,41),true);
   foreach(var p in new[]{new Vector2(-17,25),new Vector2(17,27),new Vector2(-16,37),new Vector2(7,39),new Vector2(16,18),new Vector2(-8,39)})Prop(w,root,"Bush",p,false);
   foreach(var p in new[]{new Vector2(-16,20),new Vector2(16,23),new Vector2(-15,33),new Vector2(15,35)})Prop(w,root,"Barrel",p,true);
   var npc=new GameObject("Seu Bonero • missão");npc.transform.SetParent(w.transform,false);npc.transform.position=new Vector2(-2,-6.3f);var image=RpgArt.Put("Seu Bonero",RpgArt.Frame("Hero","idle",2,0),npc.transform.position,RpgWorld.Order(-6.3f),npc.transform,new Color(.76f,.87f,1));
   var quest=w.GetComponent<RpgQuest08>();if(!quest)quest=w.gameObject.AddComponent<RpgQuest08>();quest.giver=npc.transform;
   Vector2[] positions={new(-9,24),new(9,29),new(0,37)};
   for(int i=0;i<3;i++){var node=new GameObject("Amplificador "+(i+1));node.transform.SetParent(root,false);node.transform.position=positions[i];node.AddComponent<RpgResonator08>().id=i;RpgArt.Put("Base do amplificador",RpgArt.Tile("terrain_atlas",544,384,32,64),positions[i],RpgWorld.Order(positions[i].y),node.transform);RpgArt.Put("Corpo do amplificador",RpgArt.Image("Barrel"),positions[i]+Vector2.up*.45f,RpgWorld.Order(positions[i].y)+1,node.transform);}
   Vector2[] spawn={new(-7,24),new(-10,26),new(7,29),new(10,31),new(-2,35),new(3,36),new(0,38.5f)};int[] kinds={3,0,2,3,3,1,4};
   for(int i=0;i<spawn.Length;i++){var e=w.CreateActor("Guarda do Morro",spawn[i],true);e.ConfigureEnemy(kinds[i]);e.SetHome(spawn[i]);w.enemies.Add(e);}
   foreach(var r in root.GetComponentsInChildren<SpriteRenderer>())Persist(r);Persist(image);foreach(var e in w.enemies)foreach(var r in e.GetComponentsInChildren<SpriteRenderer>())Persist(r);
   EditorUtility.SetDirty(w);EditorUtility.SetDirty(quest);EditorSceneManager.SaveScene(w.gameObject.scene);AssetDatabase.SaveAssets();Debug.Log("CIBIO_08_EXPANSION_OK area=48x60 enemies="+w.enemies.Count+" entrance="+cleared);
  }
  static void Persist(SpriteRenderer r)
  {
   var s=r.sprite;if(!s||AssetDatabase.Contains(s))return;var rect=s.rect;string path=RpgBuilder.Root+"/Art/Sprites/"+s.texture.name+"_"+(int)rect.x+"_"+(int)rect.y+"_"+(int)rect.width+"_"+(int)rect.height+".asset";var existing=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(!existing){AssetDatabase.CreateAsset(s,path);existing=s;}r.sprite=existing;
  }
  static SpriteRenderer Prop(RpgWorld w,Transform parent,string asset,Vector2 p,bool solid){var r=RpgArt.Put(asset,RpgArt.Image(asset),p,RpgWorld.Order(p.y),parent);if(solid)Solid(w,r.gameObject,new Vector2(.72f,.5f),Vector2.up*.07f);return r;}
  static void House(RpgWorld w,Transform parent,Vector2 p,int variant){var r=Prop(w,parent,"House_"+variant,p,false);Solid(w,r.gameObject,new Vector2(5,3.5f),new Vector2(0,1.7f));}
  static void Solid(RpgWorld w,GameObject o,Vector2 size,Vector2 offset){var s=o.AddComponent<RpgSolid>();s.size=size;s.offset=offset;w.solids.Add(s);}
  public static void BuildPlayer(){Append();RpgBuilder.BuildUpdatedPlayer();}
 }
}
