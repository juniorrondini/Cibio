using UnityEngine;
namespace Cibio.RPG
{
 public sealed class RpgProjectile:MonoBehaviour
 {
  public Vector2 velocity;public RpgActor owner;float age;SpriteRenderer sprite;
  public static RpgProjectile Fire(RpgActor owner,Vector2 direction)
  {
   var root=new GameObject("Arauto • projétil de energia");var r=RpgEffects.Create("Fluxo de magia",root.transform);var p=root.AddComponent<RpgProjectile>();p.owner=owner;p.velocity=direction.normalized*4.3f;p.sprite=r;root.transform.position=owner.Position+direction*.55f;r.transform.localPosition=Vector2.up*.8f;r.transform.localScale=new Vector3(.72f,.5f,1);RpgAudio.Play("spell_01",owner.Position,.25f,.9f);return p;
  }
  void Update(){if(RpgGame.Instance.Playing)Advance(Time.deltaTime);}
  public void Advance(float dt)
  {
   age+=dt;Vector2 next=(Vector2)transform.position+velocity*dt;
   if(age>2.2f||!owner.world.CanStand(next,.12f)){Destroy(gameObject);return;}transform.position=next;
   sprite.sortingOrder=RpgWorld.Order(next.y)+1;sprite.transform.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(velocity.y,velocity.x)*Mathf.Rad2Deg-90);RpgEffects.Paint(sprite,"Vfx_Wisp06",age*18,new Color(.65f,.85f,1,.9f),1.4f);if(Random.value<dt*16)RpgEffects.Spark(next+Vector2.up*.6f,new Color(.55f,.7f,1,.8f),.4f);
   var h=owner.world.hero;if(!h.dead&&Vector2.Distance(next,h.Position)<.4f){h.TakeDamage(10);RpgEffects.Burst(next,RpgArt.Hex("8AD8F2"),.7f,.4f);Destroy(gameObject);}
  }
 }
}
