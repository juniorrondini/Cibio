using UnityEngine;
namespace Cibio.RPG
{
 [ExecuteAlways,DisallowMultipleComponent]
 public sealed class RpgTerrain:MonoBehaviour
 {
  GameObject floor; Material material;Mesh mesh;
  public static RpgTerrain Ensure(RpgWorld world){var t=world.GetComponent<RpgTerrain>();return t?t:world.gameObject.AddComponent<RpgTerrain>();}
  void OnEnable()
  {
   var w=GetComponent<RpgWorld>();if(!w||!w.scenery)return;
   foreach(var r in w.scenery.GetComponentsInChildren<SpriteRenderer>())if(IsGround(r.name))r.enabled=false;
   floor=new GameObject("Terreno orgânico • caminhos e praça"){hideFlags=HideFlags.DontSave};floor.transform.SetParent(w.scenery,false);
   mesh=new Mesh{name="Solo contínuo"};mesh.vertices=new[]{new Vector3(w.bounds.xMin,w.bounds.yMin,0),new Vector3(w.bounds.xMax,w.bounds.yMin,0),new Vector3(w.bounds.xMax,w.bounds.yMax,0),new Vector3(w.bounds.xMin,w.bounds.yMax,0)};mesh.uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up};mesh.triangles=new[]{0,2,1,0,3,2};mesh.RecalculateBounds();floor.AddComponent<MeshFilter>().sharedMesh=mesh;
   material=new Material(Resources.Load<Shader>("RpgArt/OrganicGround")){hideFlags=HideFlags.DontSave};material.SetVector("_WorldBounds",new Vector4(w.bounds.xMin,w.bounds.yMin,w.bounds.width,w.bounds.height));material.SetTexture("_Grass",Resources.Load<Texture2D>("RpgArt/Terrain_Grass11"));material.SetTexture("_Stone",Resources.Load<Texture2D>("RpgArt/Terrain_Stone11"));material.SetTexture("_Earth",Resources.Load<Texture2D>("RpgArt/Terrain_Earth11"));material.SetTexture("_Damp",Resources.Load<Texture2D>("RpgArt/Terrain_Damp11"));material.SetTexture("_Mask",Resources.Load<Texture2D>("RpgArt/Terrain_Mask05"));material.SetFloat("_Coastal",w.coastal?1:0);foreach(var name in new[]{"Sea","Sand","Wood","CoastRock"})material.SetTexture("_"+name,Resources.Load<Texture2D>("RpgArt/Terrain_"+name+"14"));var renderer=floor.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.sortingOrder=-4000;
  }
  static bool IsGround(string n)=>n=="Relva"||n=="Praça de pedra"||n=="Caminho";
  void OnDisable(){var w=GetComponent<RpgWorld>();if(w&&w.scenery)foreach(var r in w.scenery.GetComponentsInChildren<SpriteRenderer>())if(IsGround(r.name))r.enabled=true;Clean(floor);Clean(material);Clean(mesh);}
  static void Clean(Object o){if(!o)return;if(Application.isPlaying)Destroy(o);else DestroyImmediate(o);}
 }
}
