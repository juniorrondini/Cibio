using UnityEngine;
namespace Cibio.RPG
{
 public sealed class RpgFlightWake09:MonoBehaviour
 {
  float nextWake;
  public static void Boom(RpgActor actor)
  {
   RpgAudio.Play("sonic_boom",actor.Position,1);var g=RpgGame.Instance;if(g){g.shake=Mathf.Max(g.shake,1.45f);g.GetComponent<RpgFeedback09>().Flash(.12f);}
   Cloud(actor,7.4f,.85f);RpgEnergyParticles.Eruption(actor.Position+Vector2.up*actor.flightHeight,actor.EnergyColor,3.8f,36);
  }
  void Update()
  {
   var g=RpgGame.Instance;if(!g||!g.Playing)return;var h=g.world.hero;if(!h.SonicActive)return;nextWake-=Time.deltaTime;if(nextWake>0)return;nextWake=.035f;Cloud(h,2.4f,.28f);
   var ghost=new GameObject("Rastro • forma ascendente");ghost.transform.position=h.body.transform.position;ghost.transform.localScale=h.body.transform.lossyScale;var r=ghost.AddComponent<SpriteRenderer>();r.sprite=h.body.sprite;r.sortingOrder=h.body.sortingOrder-2;r.color=new Color(.64f,.85f,1,.32f);var fade=ghost.AddComponent<RpgWakeFade09>();fade.SetGhost(r,.22f);
  }
  static void Cloud(RpgActor h,float size,float life)
  {
   var r=RpgEffects.Create("Sonic boom • condensação",null);r.transform.position=(Vector3)h.Position+Vector3.up*(h.flightHeight+.75f)-(Vector3)h.facing*.55f;r.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(h.facing.y,h.facing.x)*Mathf.Rad2Deg);r.transform.localScale=new Vector3(size,size*.8f,1);r.sortingOrder=h.body.sortingOrder-1;var fade=r.gameObject.AddComponent<RpgWakeFade09>();fade.SetCloud(r,life,h.EnergyColor,-h.facing*1.6f,size);
  }
 }
 public sealed class RpgWakeFade09:MonoBehaviour
 {
  SpriteRenderer image;float age,life,size;Color color;Vector2 motion;bool ghost;
  public void SetGhost(SpriteRenderer r,float duration){image=r;life=duration;ghost=true;color=r.color;}
  public void SetCloud(SpriteRenderer r,float duration,Color tint,Vector2 velocity,float width){image=r;life=duration;color=tint;motion=velocity;size=width;RpgEffects.Paint(image,"Vfx_Sonic09",0,color,.9f);}
  void Update()
  {
   if(!image||!RpgGame.Instance||!RpgGame.Instance.Playing)return;age+=Time.deltaTime;float t=age/life;if(t>=1){Destroy(gameObject);return;}var c=color;c.a*=1-t;if(ghost)image.color=c;else{transform.position+=(Vector3)(motion*Time.deltaTime);float s=size*Mathf.Lerp(.8f,1.25f,t);transform.localScale=new Vector3(s,s*.8f,1);RpgEffects.Paint(image,"Vfx_Sonic09",t*15,c,.8f);}
  }
 }
}
