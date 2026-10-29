using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Cibio.RPG
{
 public sealed class RpgSonicReview10:MonoBehaviour
 {
  RpgGame g;RpgActor h;string folder;int frame;readonly List<string> checks=new();
  void Check(bool ok,string text){checks.Add((ok?"PASS ":"FAIL ")+text);File.WriteAllLines(Path.Combine(folder,"Sonic10.txt"),checks);if(!ok)throw new InvalidOperationException(text);}
  public IEnumerator Run(string path)
  {
   folder=path;Directory.CreateDirectory(folder);g=GetComponent<RpgGame>();yield return new WaitForSecondsRealtime(.7f);g.StartGame(false);g.reviewPose=true;h=g.world.hero;
   try{GetComponent<RpgVerification>().RunChecks(folder);gameObject.AddComponent<RpgReview09>().ValidateMechanics(folder);RpgAudio.Instance.suppressOneShots=true;Tests();}catch(Exception ex){Debug.LogException(ex);Application.Quit(2);yield break;}finally{RpgAudio.Instance.suppressOneShots=false;}
   foreach(var p in UnityEngine.Object.FindObjectsByType<RpgEffects>())Destroy(p.gameObject);h.GetComponent<RpgPowerForm>().StopAllCoroutines();yield return null;Reset();Time.captureFramerate=24;yield return Frames(30,false);yield return Frames(5,false);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"Boom10.png"));yield return new WaitForEndOfFrame();yield return Frames(4,false);yield return Frames(10,true);h.Land();yield return Frames(18,false);
   Time.captureFramerate=0;Reset();h.SetFlightControls(false,false);yield return new WaitForSecondsRealtime(.5f);var capture=g.lens.gameObject.AddComponent<RpgAudioCapture>();float remain=4;float maximum=0;while(remain>0){float dt=Time.deltaTime;h.SetFlightControls(true,false);h.Walk(Vector2.right,h.FlightSpeed,dt);h.Tick(dt);maximum=Mathf.Max(maximum,h.velocity.magnitude);remain-=dt;yield return null;}h.Land();yield return new WaitForSecondsRealtime(.4f);capture.Save(Path.Combine(folder,"Boom10-Audio.wav"));Check(capture.peak<.985&&capture.peak>.60,"Mixagem nativa forte sem clipping: peak="+capture.peak);Check(maximum>57,"Velocidade real supera57u/s na vila: peak="+maximum);File.WriteAllText(Path.Combine(folder,"Velocidade10.txt"),"Impulso15; arrancada42; teto60 u/s. Pico real="+maximum.ToString("F2")+" u/s.\n");Debug.Log("CIBIO_10_REVIEW_OK checks="+checks.Count);Application.Quit();
  }
  void Reset(){g.StartGame(false);g.reviewPose=true;g.progress.perks=7;g.messageUntil=0;foreach(var e in g.world.enemies){e.transform.position=new Vector2(20,40);e.velocity=Vector2.zero;}h.transform.position=new Vector2(-16,-.5f);h.buff=16;h.formStage=2;h.energy=100;h.facing=Vector2.right;h.direction=3;g.ToggleFlight();g.SnapCamera();}
  void Tests()
  {
   Reset();h.SetFlightControls(true,false);for(int n=0;n<200&&!h.SonicActive;n++)h.Walk(Vector2.right,h.FlightSpeed,1/60f);Check(h.SonicActive&&h.sonicBooms==1,"Barreira rompe depois de distancia real");Check(Mathf.Abs(h.velocity.magnitude-42)<.01f,"Arrancada imediata42u/s no quadro da ruptura");Check(h.FlightSpeed==60&&h.FlightSpeed/15==4,"Velocidade supersônica quatro vezes o impulso");for(int n=0;n<15;n++)h.Walk(Vector2.right,h.FlightSpeed,1/60f);Check(h.velocity.magnitude>57,"Aceleracao apos arrancada chega perto de60 em250ms: "+h.velocity.magnitude+" at "+h.Position);h.SetFlightControls(true,true);for(int n=0;n<20;n++)h.Walk(Vector2.right,h.FlightSpeed,1/60f);Check(!h.SonicActive&&h.velocity.magnitude<.1f,"Ctrl detem voo veloz em333ms");
   Reset();h.SetFlightControls(true,false);for(int n=0;n<200&&!h.SonicActive;n++)h.Walk(Vector2.right,h.FlightSpeed,1/60f);h.SetFlightControls(false,false);h.Walk(Vector2.right,h.FlightSpeed,1/60f);Check(!h.SonicActive&&h.SonicDistance==0&&h.FlightSpeed==12,"SoltarShift cancela supersônico e volta ao cruzeiro");
   var wall=new Rect(4,-4,.12f,4);g.world.obstacles.Add(wall);Vector2 stopped=g.world.Move(new Vector2(0,-2),Vector2.right*60*.2f);Check(stopped.x<4,"Movimento60u/s nao atravessa parede fina em quadro de200ms");g.world.obstacles.Remove(wall);
   Check(RpgAudio.Clip("sonic_boom").length>1.9f&&RpgAudio.Clip("sonic_wind").length>=3.9f,"Boom grave e vento supersônico próprios carregados");h.Land();Check(!h.SonicActive&&h.SonicDistance==0,"Pouso encerra vento e estado supersônico");Check(RpgGame.SaveKey=="Cibio.RPG.v3.Progress","Save v3 preservado");
  }
  IEnumerator Frames(int count,bool brake){for(int n=0;n<count;n++){h.SetFlightControls(true,brake);h.Walk(Vector2.right,h.FlightSpeed,1/24f);h.Tick(1/24f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,$"Frame_{frame++:D4}.png"));}}
  void OnDisable(){Time.captureFramerate=0;}
 }
}
