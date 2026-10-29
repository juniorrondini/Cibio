using System.Collections;
using System.IO;
using UnityEngine;
namespace Cibio.RPG
{
 public sealed class RpgReview07:MonoBehaviour
 {
  RpgGame g;RpgActor h;string folder;int frame;
  public IEnumerator Run(string directory)
  {
   g=GetComponent<RpgGame>();folder=directory;Directory.CreateDirectory(folder);yield return new WaitForSecondsRealtime(.8f);
   g.StartGame(false);g.reviewPose=true;GetComponent<RpgVerification>().RunChecks(folder);yield return new WaitForSecondsRealtime(.4f);
   g.StartGame(false);g.reviewPose=true;h=g.world.hero;h.GetComponent<RpgPowerForm>().StopAllCoroutines();
   foreach(var e in g.world.enemies)e.transform.position=new Vector2(18,12);foreach(var fx in Object.FindObjectsByType<RpgEffects>())Destroy(fx.gameObject);
   h.transform.position=new Vector2(0,-3);g.progress.perks=7;g.messageUntil=0;g.SnapCamera();
   // A real-time, independently recorded mix; fixed-step visual capture starts later.
   var sound=g.lens.gameObject.AddComponent<RpgAudioCapture>();g.SetChargeHeld(true);yield return Realtime(3.35f);g.SetChargeHeld(false);yield return Realtime(1.6f);
   for(int i=0;i<3;i++){var e=g.world.enemies[0];e.dead=false;e.health=500;e.invincible=0;e.transform.position=h.Position+Vector2.down*.8f;h.facing=Vector2.down;h.Attack();yield return Realtime(.8f);}
   h.energy=100;g.dashCooldown=0;g.ToggleFlight();yield return Realtime(1.5f,Vector2.right);h.Land();yield return Realtime(.7f);h.energy=100;g.waveCooldown=0;g.Wave();yield return Realtime(1.3f);
   sound.Save(Path.Combine(folder,"Som-07.wav"));File.WriteAllText(Path.Combine(folder,"Audio-07.txt"),"Captura nativa em tempo real. Pico de saida: "+sound.peak+"; eventos: "+RpgAudio.Instance.played);Destroy(sound);
   g.StartGame(false);g.reviewPose=true;g.progress.perks=7;g.messageUntil=0;h.transform.position=new Vector2(0,-3);foreach(var e in g.world.enemies)e.transform.position=new Vector2(18,12);g.SnapCamera();Time.captureFramerate=24;
   for(int d=0;d<4;d++){Vector2 v=d==0?Vector2.up:d==1?Vector2.left:d==2?Vector2.down:Vector2.right;yield return Frames(14,v,2.8f);yield return Frames(14,-v,4.5f,true);}
   h.velocity=Vector2.zero;h.facing=Vector2.down;yield return Combo(false);
   h.ResetMotion();h.attackTimer=h.powerPose=0;g.SetChargeHeld(true);yield return Frames(80,Vector2.zero,0);g.SetChargeHeld(false);yield return Frames(32,Vector2.zero,0);
   h.transforming=0;yield return Frames(18,Vector2.right,3.64f);yield return Frames(18,Vector2.left,5.85f,true);yield return Combo(true);
   h.energy=100;h.attackTimer=h.powerPose=0;g.dashCooldown=0;g.ToggleFlight();yield return Frames(16,Vector2.zero,0);yield return Frames(28,Vector2.right,8);yield return Frames(20,Vector2.left,8);h.Land();yield return Frames(18,Vector2.zero,0);
   h.transform.position=new Vector2(0,-3);g.SnapCamera();h.energy=100;g.waveCooldown=0;var target=g.world.enemies[0];target.dead=false;target.health=1000;target.invincible=0;target.transform.position=h.Position+Vector2.right*4.6f;
   bool cast=g.Wave();float before=target.health;yield return Frames(24,Vector2.zero,0);bool poweredRange=cast&&target.health<before&&Vector2.Distance(h.Position,target.Position)>4.6f;
   File.WriteAllText(Path.Combine(folder,"Poderes-07.txt"),(poweredRange?"PASS":"FAIL")+" Onda desperta libera dano depois da pose, atinge a 4,6 unidades e empurra o alvo.\n");
   if(!poweredRange){Debug.LogError("Onda desperta falhou na revisao nativa");Application.Quit(2);yield break;}
   Time.captureFramerate=0;h.buff=12;h.flying=true;yield return Realtime(.5f);float start=Time.realtimeSinceStartup;
   for(int n=0;n<120;n++){h.Tick(Time.deltaTime);yield return new WaitForEndOfFrame();}float seconds=Time.realtimeSinceStartup-start;
   File.WriteAllText(Path.Combine(folder,"Desempenho-07.txt"),"120 quadros sem captura: "+seconds.ToString("F3")+"s; "+(120/seconds).ToString("F1")+" FPS. Visual: 24fps fixos; audio: captura independente em tempo real.\n");
   Debug.Log("CIBIO_07_REVIEW_OK");Application.Quit();
  }
  IEnumerator Realtime(float seconds,Vector2 move=default){float end=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<end){h.Walk(move,h.buff>0?8:6,Time.deltaTime);h.Tick(Time.deltaTime);yield return null;}}
  IEnumerator Combo(bool power){h.facing=Vector2.down;h.direction=2;for(int stage=0;stage<3;stage++){var e=g.world.enemies[0];e.dead=false;e.health=500;e.invincible=0;e.transform.position=h.Position+Vector2.down*.9f;h.attackTimer=h.powerPose=0;h.Attack();yield return Frames(18,Vector2.zero,0);}g.world.enemies[0].transform.position=new Vector2(18,12);}
  IEnumerator Frames(int count,Vector2 move,float speed,bool run=false){for(int i=0;i<count;i++){h.Walk(move,speed,1/24f,run);h.Tick(1/24f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"Frame_"+(frame++).ToString("D4")+".png"));}}
  void OnDisable(){Time.captureFramerate=0;}
 }
}
