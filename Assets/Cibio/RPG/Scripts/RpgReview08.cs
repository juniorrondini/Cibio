using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Cibio.RPG
{
 public sealed class RpgReview08:MonoBehaviour
 {
  RpgGame g;RpgActor h;RpgQuest08 quest;string folder;int frame;readonly List<string> results=new();
  void Check(bool ok,string what){results.Add((ok?"PASS ":"FAIL ")+what);if(!ok){File.WriteAllLines(Path.Combine(folder,"Expansao-08.txt"),results);throw new InvalidOperationException(what);}}
  public IEnumerator Run(string directory)
  {
   g=GetComponent<RpgGame>();folder=directory;Directory.CreateDirectory(folder);yield return new WaitForSecondsRealtime(.8f);g.StartGame(false);g.reviewPose=true;h=g.world.hero;quest=g.GetComponent<RpgQuest08>();
   GetComponent<RpgVerification>().RunChecks(folder);yield return new WaitForSecondsRealtime(.4f);g.StartGame(false);g.reviewPose=true;
   try{Checks();}catch(Exception ex){Debug.LogException(ex);Application.Quit(2);yield break;}
   foreach(var fx in UnityEngine.Object.FindObjectsByType<RpgEffects>())Destroy(fx.gameObject);foreach(var p in UnityEngine.Object.FindObjectsByType<RpgProjectile>())Destroy(p.gameObject);
   h.GetComponent<RpgPowerForm>().StopAllCoroutines();yield return null;g.StartGame(false);g.reviewPose=true;h.transform.position=(Vector2)quest.giver.position+Vector2.down*.7f;g.SnapCamera();g.Interact();yield return new WaitForSecondsRealtime(.3f);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"Missao-Davi.png"));yield return new WaitForSecondsRealtime(.15f);h.transform.position=new Vector2(0,34);g.SnapCamera();g.messageUntil=0;yield return new WaitForSecondsRealtime(.2f);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"Inimigos-Morro.png"));yield return new WaitForSecondsRealtime(.15f);g.progress.questStage=3;g.progress.silenced=7;g.progress.perks=7;g.progress.captainDefeated=true;quest.RefreshState();foreach(var e in g.world.enemies){e.transform.position=new Vector2(20,40);e.velocity=Vector2.zero;}
   h.transform.position=new Vector2(0,26);g.SnapCamera();g.messageUntil=0;Time.captureFramerate=24;
   // Capture each animation frame in all four perspectives for inspection.
   var animationReport=new List<string>();
   foreach(int stage in new[]{0,1,2})foreach(int direction in new[]{0,1,2,3})foreach(string action in new[]{"walk","run","flight","slash","thrust","backslash"})
   {
    h.buff=stage>0?100:0;h.formStage=stage;h.flightHeight=action=="flight"?1.9f:0;h.direction=direction;int count=RpgArt.FrameCount(h.Family,action);
    for(int f=action=="walk"?1:0;f<count;f++){h.Pose(action,f);yield return new WaitForEndOfFrame();if((direction==2&&(action=="walk"||action=="thrust"))||action=="flight")ScreenCapture.CaptureScreenshot(Path.Combine(folder,"Atlas_"+stage+"_"+direction+"_"+action+"_"+f.ToString("D2")+".png"));animationReport.Add("PASS stage="+stage+" direction="+direction+" action="+action+" frame="+f);}
   }
   File.WriteAllLines(Path.Combine(folder,"Quadros-08.txt"),animationReport);h.buff=h.transforming=0;h.ResetMotion();h.Pose("idle",0);h.facing=Vector2.down;g.messageUntil=0;
   g.SetChargeHeld(true);yield return Frames(80,Vector2.zero,false);g.SetChargeHeld(false);yield return Frames(32,Vector2.zero,false);
   g.TryAscend();yield return Frames(112,Vector2.zero,false);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"Centelha-Rubra.png"));
   h.energy=100;g.dashCooldown=0;g.ToggleFlight();yield return Frames(16,Vector2.zero,false);yield return Frames(28,Vector2.up,true);h.SetFlightControls(false,true);yield return Frames(10,Vector2.up,false,true);yield return Frames(20,Vector2.down,false);h.Land();yield return Frames(20,Vector2.zero,false);
   h.attackTimer=h.powerPose=0;h.energy=100;h.facing=Vector2.right;g.boltCooldown=0;g.Bolt();yield return Frames(16,Vector2.zero,false);h.energy=100;g.waveCooldown=0;g.Wave();yield return Frames(24,Vector2.zero,false);
   Time.captureFramerate=0;h.buff=16;h.formStage=2;g.SnapCamera();float start=Time.realtimeSinceStartup;for(int n=0;n<120;n++){h.Tick(Time.deltaTime);yield return new WaitForEndOfFrame();}float elapsed=Time.realtimeSinceStartup-start;
   File.WriteAllText(Path.Combine(folder,"Desempenho-08.txt"),"120 quadros sem capturas: "+elapsed.ToString("F3")+"s; "+(120/elapsed).ToString("F1")+" FPS. Sequencia visual fixa: 24fps.\n");
   GetComponent<RpgInterface>().enabled=false;g.lens.orthographicSize=34;g.lens.transform.position=new Vector3(0,12,-10);g.enabled=false;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"Mapa-Expandido.png"));yield return new WaitForSecondsRealtime(.2f);Debug.Log("CIBIO_08_REVIEW_OK checks="+results.Count+" animationFrames="+animationReport.Count);Application.Quit();
  }
  void Checks()
  {
   h.buff=12;h.formStage=2;h.Pose("thrust",4);h.GetComponent<RpgPowerForm>().Refresh(1);g.StartGame(false);g.reviewPose=true;Check(h.FormStage==0&&h.currentAnimation=="idle"&&h.body.enabled&&h.body.sprite.texture.name=="Hero_idle","Nova partida apos transformacao restaura sprite, porte e pose imediatamente");Check(g.world.enemies[7].body.color==g.world.enemies[7].BaseTint&&g.world.enemies[13].body.color==g.world.enemies[13].BaseTint,"Paletas dos novos inimigos sao restauradas antes do primeiro quadro");
   Check(g.world.bounds.height==60&&g.world.enemies.Count==14,"Area expandida de 48x36 para 48x60, com 14 inimigos");Check(quest.giver&&quest.nodes.Length==3,"NPC e tres amplificadores persistidos na cena");
   foreach(var n in quest.nodes)Check(Reachable(new Vector2(0,-3),n.transform.position),"Caminho terrestre livre para amplificador "+n.id);
   Check(Reachable(new Vector2(0,-3),quest.giver.position),"Caminho livre para Seu Davi");
   h.transform.position=quest.nodes[0].transform.position;quest.TryInteract();Check(g.progress.silenced==0,"Nao desliga amplificador antes de receber a missao");h.transform.position=quest.giver.position;g.Interact();Check(g.progress.questStage==1,"Conversa aceita missao sem alterar os altares");
   h.transform.position=quest.nodes[0].transform.position;quest.TryInteract();Check(g.progress.silenced==0,"Guardas proximos bloqueiam a interacao");
   for(int i=7;i<g.world.enemies.Count;i++){var e=g.world.enemies[i];e.invincible=0;e.TakeDamage(10000);}Check(g.progress.captainDefeated,"Derrotar capitao registra objetivo da missao");
   foreach(var n in quest.nodes){h.transform.position=n.transform.position;quest.TryInteract();}Check(g.progress.silenced==7&&g.progress.questStage==2,"Tres amplificadores e capitao liberam retorno ao NPC");
   h.transform.position=quest.giver.position;g.Interact();Check(g.progress.questStage==3&&g.progress.Has(0)&&g.progress.Has(1),"Entrega libera estagio 2, voo e Palma");int reward=TotalXp();g.Interact();Check(TotalXp()==reward,"Recompensa nao pode ser recebida duas vezes");
   var roundTrip=JsonUtility.FromJson<RpgProgress>(JsonUtility.ToJson(g.progress));Check(roundTrip.questStage==3&&roundTrip.silenced==7&&roundTrip.captainDefeated,"Missao e desbloqueios sobrevivem a serializacao do save v3");
   var old=JsonUtility.FromJson<RpgProgress>("{\"version\":3,\"level\":2,\"perks\":7,\"x\":0,\"y\":-3}");Check(old.level==2&&old.perks==7&&old.questStage==0,"Save antigo conserva nivel e altares e recebe nova missao");
   foreach(var e in g.world.enemies){e.transform.position=new Vector2(20,40);e.velocity=Vector2.zero;}h.transform.position=new Vector2(0,-10);h.ResetMotion();h.dead=false;h.attackTimer=h.powerPose=h.transforming=0;h.buff=10;h.formStage=1;g.progress.questStage=0;h.charge=100;g.ReleaseCharge();Check(h.FormStage==1,"Estagio 2 fica bloqueado antes da missao");
   g.progress.questStage=3;h.transforming=0;h.charge=50;g.ReleaseCharge();Check(h.FormStage==1,"Carga parcial nao ativa o segundo estagio");h.energy=100;h.charge=0;g.TryAscend();Check(h.FormStage==2&&h.buff==16&&h.energy==75,"Segunda carga completa e liberada ativa Rubra por 12s e custa 25");
   Check(h.GetComponent<RpgAura>().CurrentColor.b>h.GetComponent<RpgAura>().CurrentColor.r,"Aura do estagio 2 tem paleta avermelhada");h.transforming=0;h.attackTimer=h.powerPose=0;h.energy=100;g.dashCooldown=0;Vector2 before=h.Position;g.ToggleFlight();Check(h.Position==before&&h.flying,"Voo decola sem teleporte no segundo estagio");
   Check(h.FlightSpeed==12,"Velocidade de cruzeiro Rubra e 12, contra 8 da dourada");h.SetFlightControls(true,false);Check(h.FlightSpeed==15,"Shift leva voo Rubra a 15 unidades/s");for(int i=0;i<30;i++){h.Walk(Vector2.right,h.FlightSpeed,1/60f);h.Tick(1/60f);}Check(h.velocity.x>14&&h.energy<90,"Impulso acelera continuamente e custa energia adicional");
   h.SetFlightControls(true,true);for(int i=0;i<20;i++)h.Walk(Vector2.right,h.FlightSpeed,1/60f);Check(!h.flightBoosting&&h.velocity.magnitude<.1f,"Ctrl prevalece sobre Shift e freia ate hover");h.energy=.01f;h.Tick(.1f);Check(!h.flying,"Energia esgotada inicia pouso mesmo no estagio 2");for(int i=0;i<60;i++)h.Tick(1/60f);Check(h.flightHeight==0,"Pouso suave termina no chao");
   h.buff=.02f;h.Tick(.03f);Check(h.FormStage==0&&h.Family=="Hero","Expiracao Rubra retorna ao estado normal");
   h.ResetMotion();h.buff=20;h.formStage=2;h.energy=100;h.attackTimer=h.powerPose=h.transforming=0;h.facing=Vector2.right;h.transform.position=new Vector2(0,-10);g.boltCooldown=0;var target=g.world.enemies[0];target.dead=false;target.invincible=0;target.health=1000;target.transform.position=new Vector2(3.5f,-10);Check(g.Bolt()&&h.energy==78,"Palma custa 22 e libera projetil");Check(!g.Bolt(),"Palma respeita recarga");var shots=UnityEngine.Object.FindObjectsByType<RpgProjectile>(FindObjectsSortMode.None);foreach(var shot in shots)if(shot.owner==h)for(int i=0;i<25;i++)shot.Advance(.03f);Check(target.health<900,"Palma Rubra atinge e causa dano real sem atravessar o alvo");
   target.transform.position=new Vector2(6,-10);target.health=1000;target.invincible=0;var barrier=new Rect(2,-11,.15f,2);g.world.obstacles.Add(barrier);var blocked=RpgProjectile.FireHero(h,Vector2.right);blocked.Advance(.5f);Check(target.health==1000&&blocked.transform.position.x<2,"Projetil nao atravessa parede fina mesmo num quadro longo");g.world.obstacles.Remove(barrier);
   var scout=g.world.enemies[7];Vector2 scoutHome=scout.Home;h.transform.position=new Vector2(0,-10);h.health=100;h.invincible=0;h.flightHeight=0;scout.transform.position=new Vector2(3,-10);scout.SetHome(scout.Position);scout.ResetEnemy();scout.enemyCooldown=0;Vector2 scoutStart=scout.Position;scout.Tick(.016f);Check(scout.WindingUp&&scout.Position==scoutStart&&h.health==100,"Batedor anuncia o bote antes de mover ou causar dano");scout.Tick(.31f);Check(h.health==100,"Aviso do bote preserva janela de esquiva");scout.Tick(.31f);scout.Tick(.12f);Check(Vector2.Distance(scoutStart,scout.Position)>1&&Vector2.Distance(scoutStart,scout.Position)<2,"Bote avanca continuamente, sem teleporte");scout.Tick(.12f);Check(h.health==85,"Bote causa um unico contato de 15 ao cruzar o alvo");
   scout.SetHome(scoutHome);scout.transform.position=new Vector2(20,40);scout.dead=true;
   var boss=g.world.enemies[13];Vector2 bossHome=boss.Home;boss.SetHome(new Vector2(0,-6));boss.ResetEnemy();boss.enemyCooldown=0;h.health=100;h.invincible=0;h.flightHeight=1.9f;int shotCount=UnityEngine.Object.FindObjectsByType<RpgProjectile>(FindObjectsSortMode.None).Length;boss.Tick(.016f);Check(boss.WindingUp&&h.health==100,"Capitao anuncia a rajada sem dano instantaneo");boss.Tick(.86f);var fan=UnityEngine.Object.FindObjectsByType<RpgProjectile>(FindObjectsSortMode.None);Check(fan.Length==shotCount+3,"Capitao libera um leque de tres projeteis apos aviso");foreach(var shot in fan)if(shot.owner==boss&&Vector2.Dot(shot.velocity.normalized,Vector2.down)>.99f)for(int i=0;i<22;i++)shot.Advance(.05f);Check(h.health==84,"Projetil do capitao ameaca o voo e causa 16 de dano uma unica vez");boss.SetHome(bossHome);boss.transform.position=new Vector2(20,40);boss.dead=true;h.flightHeight=0;
   float normal=ComboDamage(0),gold=ComboDamage(1),red=ComboDamage(2);Check(normal>65&&normal<85,"Combo normal tem dano esperado para inimigos comuns");Check(gold>normal*2.2f&&gold<normal*2.4f,"Dano dourado preserva multiplicador 2,3x");Check(red>gold*1.3f&&red<gold*1.45f,"Rubra aumenta dano sem eliminar a progressao entre formas");
   File.WriteAllText(Path.Combine(folder,"Balanceamento-08.txt"),"Nivel 1; tres contatos de combo: normal="+normal.ToString("F1")+", dourado="+gold.ToString("F1")+", rubro="+red.ToString("F1")+".\nCapitao: 280 HP; projeteis de 16; antecipacao .85s; recarga 2.8s. Batedor: 78 HP; bote com antecipacao .6s; recarga 2.1s. Rubra dura 12s; voo turbo 15/s consome 15 energia/s.\n");File.WriteAllLines(Path.Combine(folder,"Expansao-08.txt"),results);
  }
  int TotalXp(){int xp=g.progress.xp;for(int i=1;i<g.progress.level;i++)xp+=i*50;return xp;}
  float ComboDamage(int stage)
  {
   g.progress.level=1;h.ResetMotion();h.buff=stage>0?100:0;h.formStage=stage;h.attackTimer=h.powerPose=h.transforming=0;h.transform.position=new Vector2(0,-10);h.facing=Vector2.down;var e=g.world.enemies[0];e.transform.position=h.Position+Vector2.down*.85f;e.health=10000;e.dead=false;
   for(int s=0;s<3;s++){e.invincible=0;e.transform.position=h.Position+Vector2.down*.85f;h.Attack();for(int n=0;n<42;n++)h.Tick(1/60f);}return 10000-e.health;
  }
  bool Reachable(Vector2 start,Vector2 target)
  {
   var q=new Queue<Vector2Int>();var seen=new HashSet<Vector2Int>();var origin=Vector2Int.RoundToInt(start*2);q.Enqueue(origin);seen.Add(origin);while(q.Count>0){var p=q.Dequeue();if(Vector2.Distance((Vector2)p*.5f,target)<1.5f)return true;foreach(var dir in new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right}){var n=p+dir;if(seen.Add(n)&&g.world.CanStand((Vector2)n*.5f))q.Enqueue(n);}}return false;
  }
  IEnumerator Frames(int count,Vector2 input,bool boost,bool brake=false){for(int n=0;n<count;n++){h.SetFlightControls(boost,brake);h.Walk(input,h.flying?h.FlightSpeed:h.GroundSpeed(boost),1/24f,boost);h.Tick(1/24f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"Frame_"+(frame++).ToString("D4")+".png"));}}
  void OnDisable(){Time.captureFramerate=0;}
 }
}
