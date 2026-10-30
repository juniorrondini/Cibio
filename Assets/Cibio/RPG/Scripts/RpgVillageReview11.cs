using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Cibio.RPG
{
 public sealed class RpgVillageReview11:MonoBehaviour
 {
  RpgGame g;RpgActor h;RpgQuest08 quest;RpgNpcDavi11 npc;string folder;int frame;readonly List<string> checks=new();
  void Check(bool ok,string text){checks.Add((ok?"PASS ":"FAIL ")+text);File.WriteAllLines(Path.Combine(folder,"Vila11.txt"),checks);if(!ok)throw new InvalidOperationException(text);}
  public void ValidateMechanics(string path)
  {
   folder=path;Directory.CreateDirectory(folder);g=GetComponent<RpgGame>();h=g.world.hero;quest=GetComponent<RpgQuest08>();npc=quest.giver.GetComponent<RpgNpcDavi11>();Tests();
  }
  public IEnumerator Run(string path)
  {
   folder=path;Directory.CreateDirectory(folder);g=GetComponent<RpgGame>();yield return new WaitForSecondsRealtime(.8f);g.StartGame(false);g.reviewPose=true;h=g.world.hero;quest=g.GetComponent<RpgQuest08>();npc=quest.giver.GetComponent<RpgNpcDavi11>();
   try{GetComponent<RpgVerification>().RunChecks(folder);gameObject.AddComponent<RpgReview09>().ValidateMechanics(folder);RpgAudio.Instance.suppressOneShots=true;Tests();}catch(Exception ex){Debug.LogException(ex);Application.Quit(2);yield break;}finally{RpgAudio.Instance.suppressOneShots=false;}
   h.GetComponent<RpgPowerForm>().StopAllCoroutines();foreach(var p in UnityEngine.Object.FindObjectsByType<RpgEffects>())Destroy(p.gameObject);foreach(var p in UnityEngine.Object.FindObjectsByType<RpgProjectile>())Destroy(p.gameObject);yield return null;g.StartGame(false);g.reviewPose=true;h.transform.position=(Vector2)quest.giver.position+Vector2.down*1.8f;g.SnapCamera();g.Interact();yield return new WaitForSecondsRealtime(.3f);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"SeuDavi11.png"));yield return new WaitForSecondsRealtime(.15f);g.messageUntil=0;npc.reviewPose=true;
   for(int d=0;d<4;d++)for(int f=0;f<8;f++){npc.Pose(d,f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,$"Davi11_{d}_{f}.png"));yield return null;}npc.reviewPose=false;
   yield return Still("Largo11",new Vector2(0,45.5f),8);yield return Still("Lago11",new Vector2(-20,42),7.5f);yield return Still("BairroLeste11",new Vector2(23,22),8);yield return Still("Praca11",new Vector2(0,-2),7.03125f);
   g.StartGame(false);g.reviewPose=true;g.progress.perks=7;g.messageUntil=0;h.transform.position=new Vector2(-28,-.5f);h.buff=16;h.formStage=2;h.energy=100;h.facing=Vector2.right;h.direction=3;g.ToggleFlight();g.SnapCamera();foreach(var e in g.world.enemies){e.transform.position=new Vector2(20,40);e.velocity=Vector2.zero;}Time.captureFramerate=24;
   yield return Flight(32,false);yield return Flight(14,false);yield return Flight(10,true);h.Land();yield return Flight(18,false);Time.captureFramerate=0;
   g.StartGame(false);g.reviewPose=true;g.messageUntil=0;h.transform.position=new Vector2(0,45);h.Pose("idle",0);g.SnapCamera();yield return new WaitForSecondsRealtime(.2f);float start=Time.realtimeSinceStartup;for(int n=0;n<120;n++)yield return new WaitForEndOfFrame();float elapsed=Time.realtimeSinceStartup-start;File.WriteAllText(Path.Combine(folder,"Desempenho11.txt"),$"120 quadros sem capturas no Largo: {elapsed:F3}s, {120/elapsed:F1} FPS. Amostra breve nesta maquina, nao benchmark.\n");
   GetComponent<RpgInterface>().enabled=false;g.enabled=false;var camera=g.lens;camera.orthographicSize=36.5f;camera.transform.position=new Vector3(0,18,-10);var rt=new RenderTexture(1400,1600,24);camera.targetTexture=rt;camera.Render();var prev=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(1400,1600,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1400,1600),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,"Mapa11.png"),image.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=prev;Destroy(image);Destroy(rt);Debug.Log("CIBIO_11_REVIEW_OK checks="+checks.Count+" npcFrames=32");Application.Quit();
  }
  IEnumerator Still(string name,Vector2 point,float size)
  {
   h.buff=h.transforming=h.powerPose=h.attackTimer=0;h.ResetMotion();h.transform.position=point;h.direction=2;h.Pose("idle",0);g.messageUntil=0;g.enabled=false;g.lens.orthographicSize=size;float width=size*g.lens.aspect;var bounds=g.world.bounds;g.lens.transform.position=new Vector3(Mathf.Clamp(point.x,bounds.xMin+width,bounds.xMax-width),Mathf.Clamp(point.y+.8f,bounds.yMin+size,bounds.yMax-size),-10);yield return new WaitForSecondsRealtime(.2f);ScreenCapture.CaptureScreenshot(Path.Combine(folder,name+".png"));yield return new WaitForSecondsRealtime(.15f);g.enabled=true;
  }
  void Tests()
  {
   g.StartGame(false);g.reviewPose=true;Check(g.world.bounds==RpgIsland14.Bounds,"Ilha96x112 mantem a vila anterior e expande a costa");Check(g.world.enemies.Count==22&&g.world.shrines.Count==3,"Vinte e dois inimigos, tres altares e seus componentes preservados");Check(npc&&quest.giver.position==new Vector3(-2,-6.3f,0),"Seu Bonero mantem identidade do objeto e ancora da missao");
   for(int d=0;d<4;d++)for(int f=0;f<8;f++){var s=RpgNpcDavi11.Frame(d,f);Check(s&&s.texture.name=="Davi11"&&s.pixelsPerUnit==64&&s.rect.size==new Vector2(128,128),$"Davi exclusivo, direcao{d} quadro{f}");}
   npc.Pose(2,0);Check(npc.body.color==Color.white&&npc.body.sprite.texture.name!="Hero_idle","NPC sem reutilizar ou tingir o sprite do jogador");
   foreach(var point in new[]{new Vector2(0,46),new Vector2(29,16),new Vector2(-29,29),new Vector2(-20,40)})Check(Reachable(new Vector2(0,-3),point),"Caminho terrestre acessivel para "+point);
   foreach(var name in new[]{"House11_0","House11_1","House11_2","House11_3","Tree11_0","Tree11_1","Tree11_2","Shrub11","Stone11","Reed11","Pond11","Terrain_Grass11","Terrain_Earth11","Terrain_Stone11","Terrain_Damp11","Barrel11","Bench11","Lamp11","Stall11","Planter11","Well11"})Check(RpgArt.Texture(name),"Arte original integrada: "+name);
   Check(!g.world.CanStand(quest.giver.position)&&g.world.CanStand((Vector2)quest.giver.position+Vector2.down),"NPC tem colisao propria e espaco de aproximacao livre");
   Check(Resources.Load<Shader>("RpgArt/OrganicGround").isSupported&&Resources.Load<Shader>("RpgArt/SoftShadow11").isSupported,"Materiais de terreno e sombras suaves suportados");
   RpgSolid pond=null;foreach(var s in g.world.solids)if(s&&s.name=="Pond11"){pond=s;break;}Check(pond&&pond.elliptical,"Lago novo usa colisao eliptica");var b=pond.Bounds;Check(pond.Blocks(b.center,.22f)&&!pond.Blocks(b.min+Vector2.one*.08f,.22f),"Agua bloqueia centro e libera cantos externos da margem arredondada");
   h.transform.position=(Vector2)quest.giver.position+Vector2.down*.7f;Check(!RpgInterface.PointerOverHudAt(new Vector2(600,570)),"Area de dialogo vazia nao bloqueia clique no mundo");g.Interact();Check(RpgInterface.PointerOverHudAt(new Vector2(600,570)),"Painel de dialogo ativo bloqueia soco por clique sobre o texto");Check(g.progress.questStage==1&&npc.Talking,"NPC animado aceita missao e inicia gesto de conversa");for(int i=7;i<g.world.enemies.Count;i++){var e=g.world.enemies[i];e.invincible=0;e.TakeDamage(10000);}foreach(var n in quest.nodes){h.transform.position=n.transform.position;quest.TryInteract();}Check(g.progress.questStage==2&&g.progress.silenced==7&&g.progress.captainDefeated,"Tres amplificadores e capitao continuam concluindo a missao");h.transform.position=quest.giver.position;g.Interact();Check(g.progress.questStage==3&&g.progress.Has(0)&&g.progress.Has(1),"Entrega da missao e recompensa preservadas com nova arte");int xp=g.progress.xp,level=g.progress.level;g.Interact();Check(g.progress.xp==xp&&g.progress.level==level,"Conversa repetida nao duplica recompensa");
   g.StartGame(false);Check(!npc.Talking&&npc.body.sprite==RpgNpcDavi11.Frame(2,0),"Nova sessao restaura idle do NPC e encerra conversa anterior");g.reviewPose=true;g.progress.perks=7;h.transform.position=new Vector2(-28,-.5f);h.buff=16;h.formStage=2;h.energy=100;g.ToggleFlight();h.SetFlightControls(true,false);float max=0;for(int n=0;n<160;n++){h.Walk(Vector2.right,h.FlightSpeed,1/60f);max=Mathf.Max(max,h.velocity.magnitude);h.Tick(1/60f);}Check(max>59.5f&&h.sonicBooms==1,"Nova avenida admite sonic boom e velocidade60 sem obstaculo invisivel");Check(g.world.CanStand(h.Position),"Voo rapido respeita geometria e limites expandidos");Check(RpgGame.SaveKey=="Cibio.RPG.v3.Progress","Savev3 permanece compativel");
  }
  bool Reachable(Vector2 from,Vector2 target)
  {
   var q=new Queue<Vector2Int>();var seen=new HashSet<Vector2Int>();var start=Vector2Int.RoundToInt(from);q.Enqueue(start);seen.Add(start);while(q.Count>0){var p=q.Dequeue();if(Vector2.Distance(p,target)<1.2f)return true;foreach(var dir in new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right}){var n=p+dir;if(seen.Add(n)&&g.world.CanStand(n))q.Enqueue(n);}}return false;
  }
  IEnumerator Flight(int count,bool brake){for(int n=0;n<count;n++){h.SetFlightControls(true,brake);h.Walk(Vector2.right,h.FlightSpeed,1/24f);h.Tick(1/24f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,$"Frame_{frame++:D4}.png"));}}
  void OnDisable(){Time.captureFramerate=0;}
 }
}
