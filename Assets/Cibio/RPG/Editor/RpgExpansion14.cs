using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Cibio.RPG.Editor
{
 public static class RpgExpansion14
 {
  static Material shadow;
  static readonly Vector2[][] paths={new[]{new Vector2(8,46),new Vector2(9,56)},new[]{new Vector2(9,56),new Vector2(0,63)},new[]{new Vector2(-8,46),new Vector2(-9,56)},new[]{new Vector2(-9,56),new Vector2(0,63)},new[]{new Vector2(0,63),new Vector2(25,68)},new[]{new Vector2(0,63),new Vector2(-30,68)},new[]{new Vector2(-30,68),new Vector2(-35,52)},new[]{new Vector2(29,29),new Vector2(36,29)},new[]{new Vector2(36,29),new Vector2(39,50)},new[]{new Vector2(39,50),new Vector2(28,68)},new[]{new Vector2(29,16),new Vector2(38,16)},new[]{new Vector2(38,16),new Vector2(38,10)},new[]{new Vector2(-29,16),new Vector2(-37,16)},new[]{new Vector2(-37,16),new Vector2(-39,42)},new[]{new Vector2(0,-16),new Vector2(0,-21)}};
  static float Dist(Vector2 p,Vector2 a,Vector2 b){Vector2 d=b-a;return Vector2.Distance(p,a+d*Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude));}
  static bool Road(Vector2 p,float radius=2.3f)=>paths.Any(r=>Dist(p,r[0],r[1])<radius)||RpgIsland14.OnPier(p,-1);
  static readonly Vector2[] homes={new(-31,61),new(-19,65),new(12,62),new(31,54),new(35,27),new(-36,20),new(25,65),new(-35,41)},caches={new(-33,64),new(30,70),new(44.3f,10)};
  static bool Reserved(Vector2 p)=>Vector2.Distance(p,new Vector2(25,69))<5.2f||Vector2.Distance(p,new Vector2(35,21))<4.4f||homes.Any(a=>Vector2.Distance(p,a)<2.8f)||caches.Any(a=>Vector2.Distance(p,a)<2.2f);
  [MenuItem("Cibio/RPG/Expandir ilha e Costa da Brisa")]
  public static void Append()
  {
   AssetDatabase.Refresh();EditorSceneManager.OpenScene(RpgBuilder.Scene);var w=Object.FindAnyObjectByType<RpgWorld>();if(GameObject.Find("Costa da Brisa • expansão14"))return;
   shadow=Resources.Load<Material>("RpgArt/SoftShadow11");w.bounds=RpgIsland14.Bounds;w.coastal=true;var root=new GameObject("Costa da Brisa • expansão14").transform;root.SetParent(w.scenery,false);
   int cleared=0;foreach(var s in w.solids)if(s&&s.enabled&&(s.name.StartsWith("Tree")||s.name=="Rock")&&(Road(s.transform.position,2.3f)||homes.Any(p=>Vector2.Distance(s.transform.position,p)<1.4f)||caches.Any(p=>Vector2.Distance(s.transform.position,p)<1.4f)))
   {
    s.enabled=false;var r=s.GetComponent<SpriteRenderer>();if(r)r.enabled=false;foreach(var sh in s.GetComponentsInChildren<SpriteRenderer>())if(sh.name.StartsWith("Sombra"))sh.enabled=false;cleared++;
   }
   var farol=Prop(root,"Lighthouse14",new Vector2(25,69));Solid(w,farol.gameObject,new Vector2(3.5f,2.8f),Vector2.up*1.35f,false);Shadow(farol.transform,4.4f,.6f,.23f);
   var cabin=Prop(root,"Cabin14",new Vector2(35,21));Solid(w,cabin.gameObject,new Vector2(5,3.3f),Vector2.up*1.55f,false);Shadow(cabin.transform,4.8f,.55f,.2f);
   var pier=Prop(root,"Pier14",new Vector2(40.5f,7.5f));pier.sortingOrder=-3950;Prop(root,"Boat14",new Vector2(43.6f,4.6f));var boat=Prop(root,"Boat14",new Vector2(-43.8f,26));boat.transform.localScale=new Vector3(.8f,.8f,1);
   for(int i=0;i<3;i++){var o=new GameObject("Esconderijo da ilha "+(i+1));o.transform.SetParent(root,false);o.transform.position=caches[i];var c=o.AddComponent<RpgCache14>();c.id=i;c.image=RpgArt.Put("Baú da Brisa",RpgArt.Image("Cache14_closed"),caches[i],RpgWorld.Order(caches[i].y),o.transform);Solid(w,o,new Vector2(.85f,.45f),Vector2.up*.12f,true);Shadow(o.transform,1.05f,.21f,.24f);}
   int[] kinds={0,2,3,0,1,3,1,2};for(int n=0;n<homes.Length;n++){var enemy=w.CreateActor("Guardião da costa",homes[n],true);enemy.ConfigureEnemy(kinds[n]);w.enemies.Add(enemy);}
   var random=new System.Random(1414);int trees=0;for(int n=0;n<2500&&trees<102;n++)
   {
    Vector2 p=new((float)random.NextDouble()*84-42,(float)random.NextDouble()*99-22);if(Mathf.Abs(p.x)<31.5f&&p.y<54&&p.y>-18)continue;if(Road(p,2.6f)||Reserved(p)||RpgIsland14.CoastDistance(p)>-2.7f||!w.CanStand(p,.85f))continue;
    bool palm=RpgIsland14.CoastDistance(p)>-7||trees%4==0;var r=Prop(root,palm?"Palm14":"Tree11_"+(trees%3),p);r.transform.localScale=Vector3.one*Mathf.Lerp(.83f,1.08f,(float)random.NextDouble());Solid(w,r.gameObject,new Vector2(.65f,.45f),Vector2.up*.06f,true);Shadow(r.transform,palm?1.6f:2.1f,.42f,.16f);trees++;
   }
   for(int n=0;n<95;n++)
   {
    Vector2 p=new((float)random.NextDouble()*84-42,(float)random.NextDouble()*99-22);if(Mathf.Abs(p.x)<31.5f&&p.y<54&&p.y>-18||Road(p,1.9f)||Reserved(p)||!w.CanStand(p,.7f))continue;var r=Prop(root,n%4==0?"CoastRock14":n%3==0?"Reed11":"Shrub11",p);r.transform.localScale=Vector3.one*(n%4==0?.65f:.8f);if(n%4==0)Solid(w,r.gameObject,new Vector2(.6f,.4f),Vector2.up*.07f,true);
   }
   var lamp=Prop(root,"Lamp11",new Vector2(34,15));Solid(w,lamp.gameObject,new Vector2(.32f,.25f),Vector2.zero,true);Prop(root,"Barrel11",new Vector2(32.4f,21));Prop(root,"Bench11",new Vector2(21,68));
   foreach(var r in w.GetComponentsInChildren<SpriteRenderer>())Persist(r);EditorUtility.SetDirty(w);EditorSceneManager.MarkSceneDirty(w.gameObject.scene);EditorSceneManager.SaveScene(w.gameObject.scene);AssetDatabase.SaveAssets();Directory.CreateDirectory("RpgReview");File.WriteAllText("RpgReview/Expansion14.txt",$"Bounds96x112; trees={trees}; former route obstacles disabled={cleared}; new enemies8; caches3; original scene objects retained.\n");Debug.Log("CIBIO_14_EXPANSION_OK trees="+trees+" cleared="+cleared);
  }
  static SpriteRenderer Prop(Transform parent,string image,Vector2 p)=>RpgArt.Put(image,RpgArt.Image(image),p,RpgWorld.Order(p.y),parent);
  static void Solid(RpgWorld w,GameObject o,Vector2 size,Vector2 offset,bool ellipse){var s=o.AddComponent<RpgSolid>();s.size=size;s.offset=offset;s.elliptical=ellipse;w.solids.Add(s);}
  static void Shadow(Transform parent,float width,float height,float opacity){var r=RpgArt.Put("Sombra de contato14",RpgArt.Pixel,(Vector2)parent.position+new Vector2(.1f,.04f),-3900,parent,new Color(.025f,.045f,.025f,opacity));r.transform.localScale=new Vector3(width,height,1);r.sharedMaterial=shadow;}
  static void Persist(SpriteRenderer r){if(!r.sprite||AssetDatabase.Contains(r.sprite))return;var s=r.sprite;var q=s.rect;string path=RpgBuilder.Root+"/Art/Sprites/"+s.texture.name+"_"+(int)q.x+"_"+(int)q.y+"_"+(int)q.width+"_"+(int)q.height+"_ppu"+(int)s.pixelsPerUnit+".asset";var existing=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(!existing){AssetDatabase.CreateAsset(s,path);existing=s;}r.sprite=existing;}
  public static void BuildPlayer(){Append();RpgBuilder.BuildUpdatedPlayer();}
 }
}
