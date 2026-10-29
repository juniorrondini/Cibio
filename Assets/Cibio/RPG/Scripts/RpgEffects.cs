using UnityEngine;
namespace Cibio.RPG
{
 public sealed class RpgEffects:MonoBehaviour
 {
  float age,duration,startSize,endSize,phase;Vector2 motion;SpriteRenderer image;Color tint;bool wisp;
  static Material material;static MaterialPropertyBlock properties;
  public static SpriteRenderer Create(string name,Transform parent)
  {
   var o=new GameObject(name);o.layer=RpgEnergyParticles.Layer;if(parent)o.transform.SetParent(parent,false);var r=o.AddComponent<SpriteRenderer>();r.sprite=RpgArt.Pixel;
   if(!material)material=Resources.Load<Material>("RpgArt/EnergyVolume06");r.sharedMaterial=material;r.enabled=false;return r;
  }
  public static void Paint(SpriteRenderer r,string atlas,float frame,Color color,float intensity,float veil=0)
  {
   properties??=new MaterialPropertyBlock();properties.Clear();properties.SetTexture("_Atlas",RpgArt.Texture(atlas));properties.SetFloat("_Frame",frame);properties.SetVector("_Grid",(atlas.EndsWith("06")||atlas.EndsWith("09"))?new Vector4(4,4,16,0):new Vector4(4,2,8,0));properties.SetFloat("_Veil",veil);properties.SetFloat("_Filament",atlas.Contains("Arc06")?1:0);properties.SetColor("_Tint",color);properties.SetFloat("_Intensity",intensity);properties.SetFloat("_Phase",Mathf.Abs(r.transform.position.x*7.7f+r.transform.position.y*11.3f)%47);r.SetPropertyBlock(properties);r.enabled=true;
  }
  public static void Spark(Vector2 p,Color c,float intensity)
  {
   RpgEnergyParticles.Spark(p,c,intensity);
  }
  public static RpgEffects Burst(Vector2 p,Color c,float radius,float duration)
  {
   var r=Create("Poder • explosão de energia",null);r.transform.position=p+Vector2.up*.55f;var fx=r.gameObject.AddComponent<RpgEffects>();fx.image=r;fx.duration=duration;fx.tint=c;fx.startSize=radius*1.15f;fx.endSize=radius*1.85f;r.sortingOrder=RpgWorld.Order(p.y)+3;if(radius>1.1f)RpgEnergyParticles.Eruption(p,c,radius,Mathf.Clamp(Mathf.RoundToInt(radius*6),8,30));return fx;
  }
  public static void Strike(Vector2 p,Vector2 facing,int combo,Color color){var fx=Burst(p,color,.7f+combo*.15f,.26f+combo*.04f);fx.motion=facing*.45f;fx.phase=combo*.23f;}
  public static void PowerStrike(Vector2 p,Vector2 facing,int combo,Color? energy=null)
  {
   for(int i=0;i<2+combo;i++){var fx=Burst(p+facing*(i*.22f),energy??RpgArt.Hex(i%2==0?"FFE7A6":"FFD377"),.7f+combo*.25f,.22f+i*.045f);fx.motion=facing*(1.8f+i*.4f);}
   RpgEnergyParticles.Eruption(p,energy??RpgArt.Hex("FFF0CA"),1.8f+combo*.5f,12+combo*5);
  }
  void Update()
  {
   if(!RpgGame.Instance||!RpgGame.Instance.Playing)return;age+=Time.deltaTime;float t=age/duration;if(t>=1){Destroy(gameObject);return;}transform.position+=(Vector3)(motion*Time.deltaTime);
   float size=Mathf.Lerp(startSize,endSize,wisp?t:Mathf.Sqrt(t));transform.localScale=wisp?new Vector3(size*.65f,size*1.7f,1):new Vector3(size,size*.72f,1);
   var c=tint;c.a*=Mathf.Pow(1-t,.5f);Paint(image,"Vfx_Surge06",t*15,c,.95f);
  }
 }
}


