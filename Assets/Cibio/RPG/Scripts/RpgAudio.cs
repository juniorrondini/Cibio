using UnityEngine;
namespace Cibio.RPG
{
 public sealed class RpgAudio:MonoBehaviour
 {
  public static RpgAudio Instance;AudioSource ambient,bass,air,electric,sustain,flight;float stepTimer;bool wasCharging;public int played;public bool suppressOneShots;
  public static AudioClip Clip(string name)=>Resources.Load<AudioClip>("RpgAudio/Designed07/"+name)??Resources.Load<AudioClip>("RpgAudio/"+name)??Resources.Load<AudioClip>("RpgAudio/Designed05/"+name);
  AudioSource Loop(string name,float volume=0){var s=gameObject.AddComponent<AudioSource>();s.clip=Clip(name);s.loop=true;s.volume=volume;s.Play();return s;}
  void Awake(){Instance=this;ambient=Loop("Forest_Ambience",.15f);bass=Loop("charge_bass");air=Loop("charge_air");electric=Loop("charge_electric");sustain=Loop("aura_sustain");flight=Loop("flight_wind");}
  public static void Play(string name,Vector2 at,float volume=.5f,float pitch=1)
  {
   if(!Instance||Instance.suppressOneShots||Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Length>32)return;var clip=Clip(name);if(!clip)return;var o=new GameObject("Som • "+name);o.transform.position=at;var source=o.AddComponent<AudioSource>();source.clip=clip;source.pitch=pitch;source.volume=volume*.68f;source.priority=name.Contains("hit")||name.Contains("transform")?64:128;source.spatialBlend=.15f;source.rolloffMode=AudioRolloffMode.Linear;source.minDistance=3;source.maxDistance=20;source.Play();Instance.played++;Object.Destroy(o,clip.length/Mathf.Max(.1f,pitch)+.1f);
  }
  public static void Punch(bool power,int stage,Vector2 at){Play((power?"power_hit":"punch_hit")+Mathf.Clamp(stage,0,2),at,power?.9f:.78f,Random.Range(.96f,1.04f));}
  void Update()
  {
   var g=RpgGame.Instance;if(!g||!g.ready)return;var h=g.world.hero;bool active=g.Playing&&h.charging&&!h.dead;float q=h.charge/100,dt=Time.unscaledDeltaTime;
   if(active&&!wasCharging){bass.time=air.time=electric.time=0;}
   if(!active&&wasCharging&&h.buff<=0)Play("charge_release",h.Position,.3f);wasCharging=active;
   Fade(bass,active?Mathf.Lerp(.12f,.32f,q):0,dt);Fade(air,active?Mathf.Lerp(.04f,.25f,q):0,dt);Fade(electric,active?Mathf.SmoothStep(0,.21f,q):0,dt);
   bass.pitch=Mathf.Lerp(.86f,1.08f,q);air.pitch=Mathf.Lerp(.75f,1.15f,q);electric.pitch=Mathf.Lerp(.8f,1.12f,q);
   Fade(sustain,g.Playing&&h.buff>0&&!h.dead?(h.FormStage==2?.19f:.14f):0,dt);Fade(ambient,h.transforming>0?.035f:g.Playing?.15f:.10f,dt);
   Fade(flight,g.Playing&&h.flying?Mathf.Lerp(.08f,.23f,h.velocity.magnitude/8):0,dt);flight.pitch=Mathf.Lerp(.85f,h.FormStage==2?1.35f:1.2f,h.velocity.magnitude/15);sustain.pitch=h.FormStage==2?.82f:1;
   stepTimer-=Time.deltaTime;if(g.Playing&&!h.dead&&h.velocity.magnitude>.5f&&h.jumpTimer<=0&&h.flightHeight<=0&&stepTimer<=0){stepTimer=h.running?.22f:.34f;Play("footstep0"+Random.Range(0,4),h.Position,.14f,Random.Range(.94f,1.07f));}
   if(Input.GetKeyDown(KeyCode.M))AudioListener.volume=AudioListener.volume>.01f?0:1;
  }
  static void Fade(AudioSource source,float target,float dt){source.volume=Mathf.MoveTowards(source.volume,target,dt*1.8f);}
 }
}
