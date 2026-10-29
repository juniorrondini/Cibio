using UnityEngine;
namespace Cibio.RPG
{
 public sealed class RpgProjectile:MonoBehaviour
 {
  public Vector2 velocity;public RpgActor owner;float age,damage,visualLift;int visualStage;bool friendly,finished;Color color;SpriteRenderer sprite;
  public static RpgProjectile FireHero(RpgActor owner,Vector2 direction)
  {
   var p=Fire(owner,direction);p.friendly=true;p.color=owner.EnergyColor;p.visualStage=owner.FormStage;p.visualLift=owner.flightHeight+.85f;p.velocity=direction.normalized*(owner.FormStage==2?11:8);p.damage=owner.Damage*(owner.FormStage==2?1.8f:1.35f);
   Vector2 size=owner.FormStage==2?new Vector2(1.3f,2.5f):owner.FormStage==1?new Vector2(1.08f,2.1f):new Vector2(.92f,1.8f);p.sprite.transform.localScale=new Vector3(size.x,size.y,1);
   // Register the luminous head at the ground-plane collision position, plus visual height.
   p.sprite.transform.localPosition=Vector2.up*p.visualLift-direction.normalized*(size.y*.2f);p.Paint();return p;
  }
  public static RpgProjectile Fire(RpgActor owner,Vector2 direction)
  {
   var root=new GameObject("Palma • projétil de energia");var r=RpgEffects.Create("Fluxo de magia",root.transform);var p=root.AddComponent<RpgProjectile>();p.owner=owner;p.velocity=direction.normalized*(owner.enemyKind==4?5.1f:4.3f);p.sprite=r;p.color=owner.enemyKind==4?RpgArt.Hex("FF748A"):RpgArt.Hex("A6D8FF");root.transform.position=owner.Position+direction*.55f;r.transform.localPosition=Vector2.up*.8f;r.transform.localScale=new Vector3(.72f,.5f,1);RpgAudio.Play("spell_01",owner.Position,.25f,.9f);return p;
  }
  void Update(){if(RpgGame.Instance.Playing)Advance(Time.deltaTime);}
  public void Advance(float dt)
  {
   if(finished)return;age+=dt;Vector2 before=transform.position,next=before+velocity*dt;
   if(age>2.2f){finished=true;Destroy(gameObject);return;}
   int steps=Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(before,next)/.1f));for(int i=1;i<=steps;i++)if(!owner.world.CanStand(Vector2.Lerp(before,next,i/(float)steps),.12f)){if(friendly)Hit(Vector2.Lerp(before,next,(i-1)/(float)steps));else{finished=true;Destroy(gameObject);}return;}transform.position=next;
   Paint();if(Random.value<dt*16)RpgEffects.Spark(next+Vector2.up*(friendly?visualLift:.6f),color,.4f);
   if(friendly){foreach(var e in owner.world.enemies)if(!e.dead&&DistanceToSegment(e.Position,before,next)<.6f){e.TakeDamage(damage);Hit(e.Position);break;}}
   else{var h=owner.world.hero;if(!h.dead&&DistanceToSegment(h.Position,before,next)<.4f){h.TakeDamage(owner.enemyKind==4?16:10);Hit(next);}}
  }
  void Paint(){sprite.sortingOrder=RpgWorld.Order(transform.position.y)+1;sprite.transform.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(velocity.y,velocity.x)*Mathf.Rad2Deg-90);RpgEffects.Paint(sprite,friendly?"Vfx_PalmaFlight13":"Vfx_Wisp06",age*(friendly?20:18),color,friendly?1.65f:1.4f);}
  void Hit(Vector2 p){finished=true;sprite.enabled=false;if(friendly)RpgPalmaImpact13.Emit(p,visualLift,color,visualStage==2?2.8f:visualStage==1?2.35f:1.9f);else RpgEffects.Burst(p,color,.7f,.35f);RpgAudio.Play("power_hit0",p,.3f);Destroy(gameObject);}
  static float DistanceToSegment(Vector2 p,Vector2 a,Vector2 b){Vector2 d=b-a;float u=d.sqrMagnitude>.00001f?Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude):0;return Vector2.Distance(p,a+d*u);}
 }
}
