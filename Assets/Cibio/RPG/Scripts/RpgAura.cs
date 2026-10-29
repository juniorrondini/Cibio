using UnityEngine;
namespace Cibio.RPG
{
 public sealed class RpgAura:MonoBehaviour
 {
  RpgActor actor;SpriteRenderer shell,haze,veil,arcs,ground;ParticleSystem rearParticles,frontParticles;Material groundMaterial;float time,strength,rearEmission,frontEmission;bool lastPlaying;
  public Color CurrentColor=>actor?actor.EnergyColor:Color.white;
  public bool Visible=>shell&&shell.enabled;
  public int LiveParticles=>(rearParticles?rearParticles.particleCount:0)+(frontParticles?frontParticles.particleCount:0);
  public int LayerCount=>5;
  void Start()
  {
   actor=GetComponent<RpgActor>();shell=RpgEffects.Create("Aura • volume ascendente",transform);haze=RpgEffects.Create("Aura • vapor externo",transform);veil=RpgEffects.Create("Aura • fios diante do corpo",transform);arcs=RpgEffects.Create("Aura • descargas intermitentes",transform);
   ground=RpgEffects.Create("Aura • luz difusa no chão",transform);groundMaterial=new Material(Resources.Load<Shader>("RpgArt/EnergyGroundLight06"));ground.sharedMaterial=groundMaterial;
   rearParticles=RpgEnergyParticles.Create("Aura • partículas atrás do corpo",transform,100,false);frontParticles=RpgEnergyParticles.Create("Aura • partículas à frente",transform,70,true);
  }
  void LateUpdate(){Refresh(Time.deltaTime);}
  public void Refresh(float dt)
  {
   if(!actor||!shell||!RpgGame.Instance)return;
   bool playing=RpgGame.Instance.Playing,display=playing||RpgGame.Instance.mode==2,show=display&&!actor.dead&&(actor.charging||actor.buff>0||actor.flying);
   if(playing!=lastPlaying){if(playing){rearParticles.Play();frontParticles.Play();}else{rearParticles.Pause();frontParticles.Pause();}lastPlaying=playing;}
   if(playing){time+=dt;strength=Mathf.MoveTowards(strength,show?1:0,dt*(show?3:4));}
   bool visible=display&&strength>.005f;shell.enabled=haze.enabled=veil.enabled=arcs.enabled=ground.enabled=visible;
   if(!visible)return;
   bool crimson=actor.FormStage==2;Color palette=crimson?new Color(1,.22f,.31f):new Color(1,.79f,.36f);bool transformed=actor.buff>0;float q=transformed?1:actor.flying?.42f:actor.charge/100,amount=Mathf.Lerp(.25f,1,q),breathe=1+Mathf.Sin(time*3.2f)*.018f;
   float form=Mathf.InverseLerp(1,1.18f,actor.body.transform.localScale.y),width=Mathf.Lerp(2.55f,3.15f,form)*Mathf.Lerp(.65f,1,q)*breathe,height=Mathf.Lerp(3.4f,crimson?4.5f:4.05f,form)*Mathf.Lerp(.55f,1,q)*breathe,lift=actor.body.transform.localPosition.y,lean=-actor.velocity.x*.025f;int order=RpgWorld.Order(actor.Position.y);
   shell.transform.localPosition=new Vector3(lean,1.27f+lift,0);shell.transform.localScale=new Vector3(width,height,1);shell.transform.localRotation=Quaternion.Euler(0,0,actor.velocity.x*-1.4f);shell.sortingOrder=order-3;
   RpgEffects.Paint(shell,"Vfx_AuraBody06",time*12,new Color(palette.r,palette.g,palette.b,strength),amount*.92f);
   haze.transform.localPosition=shell.transform.localPosition;haze.transform.localScale=new Vector3(width*1.18f,height*1.08f,1);haze.sortingOrder=order-4;RpgEffects.Paint(haze,"Vfx_AuraBody06",time*8+7,new Color(1,crimson?.08f:.64f,crimson?.18f:.22f,strength*.18f),amount);
   veil.transform.localPosition=new Vector3(-lean*.5f,1.24f+lift,0);veil.transform.localScale=new Vector3(width*.96f,height*.97f,1);veil.sortingOrder=order+2;RpgEffects.Paint(veil,"Vfx_AuraVeil06",time*13+4,new Color(1,crimson?.30f:.88f,crimson?.40f:.57f,strength*.70f),amount*.66f,1);
   float electric=(q>.7f?Mathf.Pow(Mathf.Max(0,Mathf.Sin(time*4.8f)),14):0)*Mathf.InverseLerp(.7f,1,q);
   arcs.transform.localPosition=new Vector3(0,1.3f+lift,0);arcs.transform.localScale=new Vector3(width*.93f,height*.96f,1);arcs.sortingOrder=order+3;RpgEffects.Paint(arcs,"Vfx_Arc06",time*19,new Color(1,crimson?.6f:.96f,crimson?.74f:.82f,strength*electric*.55f),.8f,1);arcs.enabled=electric>.01f;
   ground.transform.localPosition=new Vector3(0,-.18f,0);ground.transform.localScale=new Vector3(width*1.3f,.9f,1);ground.sortingOrder=-3980;groundMaterial.SetColor("_Tint",new Color(1,crimson?.12f:.63f,crimson?.22f:.14f,strength*amount*.17f));
   rearParticles.GetComponent<ParticleSystemRenderer>().sortingOrder=order-2;frontParticles.GetComponent<ParticleSystemRenderer>().sortingOrder=order+1;
   if(show&&playing){Emit(rearParticles,ref rearEmission,dt*(18+q*20),false,amount,lift);Emit(frontParticles,ref frontEmission,dt*(7+q*9),true,amount,lift);}
  }
  void Emit(ParticleSystem ps,ref float accumulated,float count,bool front,float amount,float lift)
  {
   accumulated+=count;int number=Mathf.Min(8,(int)accumulated);accumulated-=number;
   for(int i=0;i<number;i++)
   {
    float y=Random.Range(-.14f,front?.95f:1.65f),side=Random.value<.5f?-1:1;float x=side*Random.Range(.35f,.85f)*(1-y*.16f);
    var emit=new ParticleSystem.EmitParams{position=(Vector3)actor.Position+new Vector3(x,y+lift,0),velocity=new Vector3(-x*.12f,Random.Range(.3f,.85f),0),startColor=new Color(1,actor.FormStage==2?.24f:.84f,actor.FormStage==2?.35f:.43f,strength*(front?.50f:.75f)),startLifetime=Random.Range(.65f,1.4f),startSize3D=new Vector3(Random.Range(.12f,.28f),Random.Range(.26f,.65f)*amount,1)};ps.Emit(emit,1);
   }
  }
  void OnDestroy(){if(groundMaterial)Destroy(groundMaterial);}
 }
}
