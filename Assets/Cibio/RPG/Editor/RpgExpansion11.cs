using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Cibio.RPG.Editor
{
 public static class RpgExpansion11
 {
  static Material shadow;static int cleared,refined;
  static readonly Vector2[][] roads={new[]{new Vector2(-30,-.5f),new Vector2(30,-.5f)},new[]{new Vector2(18,16),new Vector2(29,16)},new[]{new Vector2(29,-6),new Vector2(29,37)},new[]{new Vector2(-18,29),new Vector2(-29,29)},new[]{new Vector2(-29,-6),new Vector2(-29,34)},new[]{new Vector2(-29,34),new Vector2(-20,39)},new[]{new Vector2(-20,39),new Vector2(-10,46)},new[]{new Vector2(29,36),new Vector2(17,45)},new[]{new Vector2(17,45),new Vector2(8,46)},new[]{new Vector2(0,37),new Vector2(0,48)}};
  static float Distance(Vector2 p,Vector2 a,Vector2 b){var d=b-a;return Vector2.Distance(p,a+d*Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude));}
  static bool OnRoad(Vector2 p,float padding=2)=>roads.Any(r=>Distance(p,r[0],r[1])<padding)||Mathf.Pow(p.x/8.5f,2)+Mathf.Pow((p.y-46)/4.8f,2)<1;
  [MenuItem("Cibio/RPG/Adicionar Largo das Palmeiras e refinar vila")]
  public static void Append()
  {
   EditorSceneManager.OpenScene(RpgBuilder.Scene);var w=Object.FindAnyObjectByType<RpgWorld>();if(GameObject.Find("Largo das Palmeiras • expansão 11"))return;AssetDatabase.Refresh();var shader=Resources.Load<Shader>("RpgArt/SoftShadow11");string matPath=RpgBuilder.Root+"/Resources/RpgArt/SoftShadow11.mat";shadow=AssetDatabase.LoadAssetAtPath<Material>(matPath);if(!shadow){shadow=new Material(shader);AssetDatabase.CreateAsset(shadow,matPath);}w.bounds=new Rect(-32,-18,64,72);
   var root=new GameObject("Largo das Palmeiras • expansão 11").transform;root.SetParent(w.scenery,false);
   foreach(var r in w.scenery.GetComponentsInChildren<SpriteRenderer>())
   {
    if(!r.enabled)continue;string n=r.name;var solid=r.GetComponent<RpgSolid>();
    if((n.StartsWith("Tree_")||n=="Rock")&&OnRoad(r.transform.position)){r.enabled=false;if(solid)solid.enabled=false;cleared++;continue;}
    if(n.StartsWith("House_")){int variant=n.EndsWith("1")?1:n.EndsWith("2")?2:0;if(Vector2.Distance(r.transform.position,new Vector2(0,7))<.1f)r.transform.position=new Vector2(-5,10);r.sprite=RpgArt.Image("House11_"+variant);refined++;Shadow(r.transform,5.3f,.65f,.18f);}
    else if(n.StartsWith("Tree_")){r.sprite=RpgArt.Image("Tree11_"+(n.EndsWith("1")?1:0));if(solid)solid.elliptical=true;refined++;Shadow(r.transform,2.2f,.55f,.18f);}
    else if(n=="Bush"){r.sprite=RpgArt.Image("Shrub11");refined++;}
    else if(n=="Rock"){r.sprite=RpgArt.Image("Stone11");if(solid)solid.elliptical=true;refined++;}
    else if(n=="Pond"){r.sprite=RpgArt.Image("Pond11");r.transform.localScale=Vector3.one;if(solid){solid.elliptical=true;solid.size=new Vector2(6.2f,4.2f);solid.offset=Vector2.up*2;}refined++;}
   }
   House(w,root,new Vector2(25,6),0);House(w,root,new Vector2(24,26),1);House(w,root,new Vector2(-25,16),2);House(w,root,new Vector2(0,48),3);
   var pond=Prop(w,root,"Pond11",new Vector2(-26,42),Vector2.one);Solid(w,pond.gameObject,new Vector2(6.2f,4.2f),Vector2.up*2,true);
   var random=new System.Random(1111);int trees=0;for(int n=0;n<700&&trees<64;n++)
   {
    float x=(float)random.NextDouble()*60-30,y=(float)random.NextDouble()*69-16;if(Mathf.Abs(x)<24&&y<42)continue;Vector2 p=new(x,y);if(OnRoad(p,2.1f)||!w.CanStand(p,1.4f))continue;
    var r=Prop(w,root,"Tree11_"+(trees%8==0?2:trees%4==0?0:1),p,Vector2.one*Mathf.Lerp(.84f,1.1f,(float)random.NextDouble()));Solid(w,r.gameObject,new Vector2(.72f,.5f),Vector2.up*.07f,true);Shadow(r.transform,2.2f,.55f,.18f);trees++;
   }
   for(int n=0;n<62;n++){Vector2 p=new((float)random.NextDouble()*60-30,(float)random.NextDouble()*68-15);if((Mathf.Abs(p.x)<23&&p.y<42)||OnRoad(p,1.3f)||!w.CanStand(p,.5f))continue;Prop(w,root,n%5==0?"Stone11":n%3==0?"Reed11":"Shrub11",p,Vector2.one*Mathf.Lerp(.65f,1,(float)random.NextDouble()));}
   var quest=w.GetComponent<RpgQuest08>();var npc=quest.giver;var actor=npc.GetComponent<RpgNpcDavi11>();if(!actor)actor=npc.gameObject.AddComponent<RpgNpcDavi11>();actor.body=npc.GetComponentInChildren<SpriteRenderer>();actor.Pose(2,0);Shadow(npc,.7f,.16f,.3f);
   foreach(var r in w.GetComponentsInChildren<SpriteRenderer>())Persist(r);EditorUtility.SetDirty(w);EditorUtility.SetDirty(quest);EditorSceneManager.MarkSceneDirty(w.gameObject.scene);EditorSceneManager.SaveScene(w.gameObject.scene);AssetDatabase.SaveAssets();Directory.CreateDirectory("RpgReview");File.WriteAllText("RpgReview/Expansion11.txt",$"area64x72; old scenery refined={refined}; route obstacles disabled={cleared}; new trees={trees}; NPC unique32frames; original objects retained.\n");Debug.Log($"CIBIO_11_EXPANSION_OK refined={refined} cleared={cleared} trees={trees}");
  }
  static SpriteRenderer Prop(RpgWorld w,Transform root,string image,Vector2 p,Vector2 scale){var r=RpgArt.Put(image,RpgArt.Image(image),p,RpgWorld.Order(p.y),root);r.transform.localScale=scale;return r;}
  static void House(RpgWorld w,Transform root,Vector2 p,int variant)
  {
   var r=Prop(w,root,"House11_"+variant,p,Vector2.one);Solid(w,r.gameObject,new Vector2(5,3.5f),new Vector2(0,1.7f),false);Shadow(r.transform,5.3f,.65f,.18f);
   foreach(var s in w.solids)if(s&&s!=r.GetComponent<RpgSolid>()&&s.name.StartsWith("Tree")&&s.Bounds.Overlaps(r.GetComponent<RpgSolid>().Bounds)){s.enabled=false;s.GetComponent<SpriteRenderer>().enabled=false;var sh=s.transform.Find("Sombra de contato11");if(sh)sh.gameObject.SetActive(false);cleared++;}
  }
  static void Solid(RpgWorld w,GameObject o,Vector2 size,Vector2 offset,bool rounded){var s=o.AddComponent<RpgSolid>();s.size=size;s.offset=offset;s.elliptical=rounded;w.solids.Add(s);}
  static void Shadow(Transform parent,float width,float height,float opacity){var r=RpgArt.Put("Sombra de contato11",RpgArt.Pixel,(Vector2)parent.position+new Vector2(.12f,.06f),-3900,parent,new Color(.025f,.045f,.025f,opacity));r.transform.localScale=new Vector3(width,height,1);r.sharedMaterial=shadow;}
  static void Persist(SpriteRenderer r)
  {
   if(!r.sprite||AssetDatabase.Contains(r.sprite))return;var s=r.sprite;var q=s.rect;string path=RpgBuilder.Root+"/Art/Sprites/"+s.texture.name+"_"+(int)q.x+"_"+(int)q.y+"_"+(int)q.width+"_"+(int)q.height+"_ppu"+(int)s.pixelsPerUnit+".asset";var existing=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(!existing){AssetDatabase.CreateAsset(s,path);existing=s;}r.sprite=existing;
  }
  static void Details()
  {
   var w=Object.FindAnyObjectByType<RpgWorld>();if(GameObject.Find("Detalhes urbanos11"))return;shadow=Resources.Load<Material>("RpgArt/SoftShadow11");var root=new GameObject("Detalhes urbanos11").transform;root.SetParent(w.scenery,false);
   foreach(var r in w.scenery.GetComponentsInChildren<SpriteRenderer>())
   {
    if(!r.enabled)continue;if(r.name=="House11_3")r.transform.position=new Vector2(0,48);
    if(r.name.StartsWith("Tree_")||r.name.StartsWith("Tree11_")){var p=(Vector2)r.transform.position;int key=Mathf.Abs(Mathf.RoundToInt(p.x*23+p.y*31));r.sprite=RpgArt.Image("Tree11_"+(key%9==0?2:key%4==0?0:1));if(r.name.StartsWith("Tree_")&&Mathf.Abs(p.y-41)<.1f){var next=p+new Vector2(Mathf.Sin(p.x*1.7f)*.45f,Mathf.Sin(p.x*.8f)*1.2f);if(!OnRoad(next,1.6f))r.transform.position=next;r.transform.localScale=Vector3.one*(.88f+Mathf.Abs(Mathf.Sin(p.x))*.18f);}}
    else if(r.name=="Barrel"){r.sprite=RpgArt.Image("Barrel11");Shadow(r.transform,.95f,.2f,.2f);}
    if(r.name.StartsWith("House")||r.name.StartsWith("Tree"))r.sortingOrder=RpgWorld.Order(r.transform.position.y);
   }
   var giver=w.GetComponent<RpgQuest08>().giver;if(!giver.GetComponent<RpgSolid>())Solid(w,giver.gameObject,new Vector2(.5f,.3f),Vector2.zero,true);
   Detail(w,root,"Stall11",new Vector2(-6,46),new Vector2(2.8f,1.2f),Vector2.up*.55f);Detail(w,root,"Bench11",new Vector2(5.8f,45),new Vector2(1.85f,.45f),Vector2.up*.15f);Detail(w,root,"Bench11",new Vector2(-4,-4),new Vector2(1.85f,.45f),Vector2.up*.15f);Detail(w,root,"Well11",new Vector2(6,48.3f),new Vector2(1.65f,1.1f),Vector2.up*.4f);
   foreach(var p in new[]{new Vector2(-8,45),new Vector2(8,45),new Vector2(-3.8f,49.5f),new Vector2(3.8f,49.5f)})Detail(w,root,"Lamp11",p,new Vector2(.35f,.25f),Vector2.up*.05f);
   foreach(var p in new[]{new Vector2(-3,48),new Vector2(3,48),new Vector2(-5.4f,-4),new Vector2(7.3f,45)})Detail(w,root,"Planter11",p,new Vector2(.55f,.35f),Vector2.up*.1f);
   foreach(var r in w.GetComponentsInChildren<SpriteRenderer>())Persist(r);EditorUtility.SetDirty(w);EditorSceneManager.MarkSceneDirty(w.gameObject.scene);EditorSceneManager.SaveScene(w.gameObject.scene);AssetDatabase.SaveAssets();Debug.Log("CIBIO_11_DETAILS_OK");
  }
  static void Detail(RpgWorld w,Transform root,string image,Vector2 p,Vector2 size,Vector2 offset)
  {
   foreach(var s in w.solids)if(s&&s.enabled&&(s.name.StartsWith("Tree")||s.name=="Rock")&&Vector2.Distance(s.transform.position,p)<1.7f){s.enabled=false;s.GetComponent<SpriteRenderer>().enabled=false;var sh=s.transform.Find("Sombra de contato11");if(sh)sh.gameObject.SetActive(false);}
   var r=Prop(w,root,image,p,Vector2.one);Solid(w,r.gameObject,size,offset,image!="Stall11"&&image!="Bench11");Shadow(r.transform,size.x*1.1f,.3f,.2f);
  }
  static void PropSpacing()
  {
   var root=GameObject.Find("Detalhes urbanos11");if(!root)return;bool changed=false;foreach(var r in root.GetComponentsInChildren<SpriteRenderer>())if(r.name=="Planter11"){Vector2 p=r.transform.position;if(Vector2.Distance(p,new Vector2(-4.8f,-4))<.05f){r.transform.position=new Vector2(-5.4f,-4);changed=true;}else if(Vector2.Distance(p,new Vector2(6.7f,45))<.05f){r.transform.position=new Vector2(7.3f,45);changed=true;}}if(changed){EditorSceneManager.MarkSceneDirty(root.scene);EditorSceneManager.SaveScene(root.scene);}
  }
  public static void BuildPlayer(){Append();Details();PropSpacing();RpgBuilder.BuildUpdatedPlayer();}
 }
}
