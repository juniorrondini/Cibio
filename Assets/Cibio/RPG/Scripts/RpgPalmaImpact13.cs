using UnityEngine;
namespace Cibio.RPG
{
 public sealed class RpgPalmaImpact13:MonoBehaviour
 {
  public const float Duration=.5f;float age;SpriteRenderer image;Color tint;
  public static RpgPalmaImpact13 Emit(Vector2 ground,float lift,Color color,float size)
  {
   var r=RpgEffects.Create("Palma • impacto orgânico",null);r.transform.position=ground+Vector2.up*lift;r.transform.localScale=new Vector3(size,size*.82f,1);r.sortingOrder=RpgWorld.Order(ground.y)+4;
   var fx=r.gameObject.AddComponent<RpgPalmaImpact13>();fx.image=r;fx.tint=color;fx.Advance(0);return fx;
  }
  void Update(){if(RpgGame.Instance&&RpgGame.Instance.Playing)Advance(Time.deltaTime);}
  public void Advance(float dt)
  {
   age+=dt;if(age>=Duration){image.enabled=false;Destroy(gameObject);return;}
   var c=tint;c.a*=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.3f,Duration,age));RpgEffects.Paint(image,"Vfx_PalmaImpact13",Mathf.Min(15,age/Duration*16),c,1.55f);
  }
 }
}
