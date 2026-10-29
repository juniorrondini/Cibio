using UnityEngine;
namespace Cibio.RPG
{
 public static class RpgEnergyParticles
 {
  public const int Layer=29;static Material material;static ParticleSystem shared;
  public static Material Material
  {
   get{if(!material){material=new Material(Resources.Load<Shader>("RpgArt/EnergyParticle06"));material.mainTexture=RpgArt.Texture("Vfx_Wisp06");}return material;}
  }
  public static ParticleSystem Create(string name,Transform parent,int maxParticles,bool front)
  {
   var root=new GameObject(name);root.layer=Layer;if(parent)root.transform.SetParent(parent,false);
   var ps=root.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
   var main=ps.main;main.loop=true;main.playOnAwake=false;main.simulationSpace=ParticleSystemSimulationSpace.World;main.maxParticles=maxParticles;main.startLifetime=new ParticleSystem.MinMaxCurve(.6f,1.45f);main.startSpeed=0;main.startSize3D=true;main.startSizeX=new ParticleSystem.MinMaxCurve(.07f,.18f);main.startSizeY=new ParticleSystem.MinMaxCurve(.18f,.5f);main.startSizeZ=1;main.startColor=new Color(1,.79f,.35f,.65f);main.gravityModifier=-.035f;
   var emission=ps.emission;emission.enabled=true;emission.rateOverTime=0;var shape=ps.shape;shape.enabled=false;
   var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.World;velocity.x=new ParticleSystem.MinMaxCurve(-.18f,.18f);velocity.y=new ParticleSystem.MinMaxCurve(.65f,1.35f);velocity.z=new ParticleSystem.MinMaxCurve(0,0);
   var noise=ps.noise;noise.enabled=true;noise.separateAxes=true;noise.strengthX=.18f;noise.strengthY=.13f;noise.strengthZ=0;noise.frequency=.7f;noise.scrollSpeed=.5f;noise.damping=true;noise.quality=ParticleSystemNoiseQuality.Low;
   var color=ps.colorOverLifetime;color.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(new Color(1,.98f,.9f),0),new GradientColorKey(new Color(.9f,.94f,1),1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.8f,.12f),new GradientAlphaKey(.6f,.45f),new GradientAlphaKey(0,1)});color.color=gradient;
   var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.3f),new Keyframe(.25f,1),new Keyframe(1,.08f)));
   var rotation=ps.rotationOverLifetime;rotation.enabled=true;rotation.z=new ParticleSystem.MinMaxCurve(-.25f,.25f);
   var sheet=ps.textureSheetAnimation;sheet.enabled=true;sheet.mode=ParticleSystemAnimationMode.Grid;sheet.numTilesX=4;sheet.numTilesY=4;sheet.animation=ParticleSystemAnimationType.WholeSheet;sheet.frameOverTime=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,0,1,.999f));sheet.startFrame=new ParticleSystem.MinMaxCurve(0,.35f);sheet.cycleCount=1;
   var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=Material;renderer.renderMode=ParticleSystemRenderMode.Billboard;renderer.sortMode=ParticleSystemSortMode.OldestInFront;renderer.minParticleSize=0;renderer.maxParticleSize=.12f;renderer.sortingOrder=front?2:-2;
   ps.useAutoRandomSeed=false;ps.randomSeed=front?61029u:61030u;ps.Play();root.AddComponent<RpgParticleClock>();return ps;
  }
  public static void Spark(Vector2 p,Color tint,float intensity)
  {
   if(!shared)shared=Create("Energia • partículas compartilhadas",RpgGame.Instance?RpgGame.Instance.world.transform:null,180,true);
   var renderer=shared.GetComponent<ParticleSystemRenderer>();renderer.sortingOrder=RpgWorld.Order(p.y)+3;
   var emit=new ParticleSystem.EmitParams{position=p+Random.insideUnitCircle*.38f,velocity=new Vector3(Random.Range(-.25f,.25f),Random.Range(.25f,.8f)*intensity,0),startColor=tint,startLifetime=Random.Range(.5f,1.2f),startSize3D=new Vector3(Random.Range(.08f,.17f),Random.Range(.18f,.42f),1)};shared.Emit(emit,1);
  }
  public static void Eruption(Vector2 p,Color tint,float radius,int count)
  {
   for(int i=0;i<count;i++){float angle=Random.Range(0,Mathf.PI*2);Vector2 direction=new(Mathf.Cos(angle),Mathf.Sin(angle)*.65f);Spark(p+direction*radius*.35f,tint,Random.Range(.7f,1.8f));}
  }
 }
}

