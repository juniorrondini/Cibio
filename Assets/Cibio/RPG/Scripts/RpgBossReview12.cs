using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Cibio.RPG
{
 public sealed class RpgBossReview12:MonoBehaviour
 {
  RpgGame g;RpgActor h,b;RpgQuest08 q;string folder;readonly List<string> checks=new();
  void Check(bool ok,string text){checks.Add((ok?"PASS ":"FAIL ")+text);File.WriteAllLines(Path.Combine(folder,"Chefe12.txt"),checks);if(!ok)throw new InvalidOperationException(text);}
  int Shots()=>UnityEngine.Object.FindObjectsByType<RpgProjectile>(FindObjectsSortMode.None).Count(p=>p.owner==b);
  void Reset(){g.StartGame(false);g.reviewPose=true;g.messageUntil=0;h=g.world.hero;b=g.world.enemies.Single(e=>e.enemyKind==4);q=GetComponent<RpgQuest08>();h.invincible=100;}
  void Tests()
  {
   Reset();Check(q.giver.name=="Seu Bonero • missão","NPC original renomeado em cena");
   Check(b.EnemyName=="Companheiro"&&b.Family=="Companheiro12","Chefe tem identidade e familia exclusiva");
   Check(b.MaxHealth==280&&b.Damage==26&&b.health==280,"Vida e dano anteriores preservados");
   Check(b.body.color==Color.white,"Barba, terno e emblema usam cores originais da arte");
   Check(g.world.enemies.Any(e=>e.enemyKind==1&&e.Family=="EnemyHeavy"),"Guardas blindados conservam a propria arte");
   foreach(string action in new[]{"idle","walk","slash","spellcast","hurt"})
   {
    int expected=action=="idle"?4:action=="walk"?9:action=="spellcast"?7:6;
    Check(RpgArt.FrameCount("Companheiro12",action)==expected,"Contagem do ciclo "+action);
    for(int d=0;d<4;d++)for(int f=0;f<expected;f++)
    {
     b.direction=d;b.Pose(action,f);var s=b.body.sprite;
     Check(s&&s.texture.name=="Companheiro12_"+action&&s.rect.size==new Vector2(128,128)&&s.pixelsPerUnit==64&&s.pivot==new Vector2(64,20.48f),$"Quadro exclusivo128/PPU64: {action} direcao{d} frame{f}");
    }
   }
   Reset();h.transform.position=q.giver.position;g.Interact();Check(g.progress.questStage==1&&g.message.StartsWith("Seu Bonero: ")&&g.message.Contains("Companheiro"),"Dialogo apresenta Bonero e o chefe correto");
   h.transform.position=b.Position+Vector2.down*.9f;h.invincible=0;b.enemyCooldown=0;int shots=Shots();b.Tick(.01f);
   Check(b.currentAnimation=="slash"&&!b.WindingUp,"De perto escolhe soco em vez de iniciar poder");float hp=h.health;b.Tick(.29f);Check(h.health==hp,"Soco nao aplica dano antes do contato");b.Tick(.04f);
   Check(h.health==hp-26&&b.animationFrame==3&&Shots()==shots,"Contato do soco no quadro3,26dano, sem projetil");
   Reset();h.transform.position=b.Position+Vector2.down*4;b.enemyCooldown=0;shots=Shots();b.Tick(.01f);
   Check(RpgInterface.PointerOverHudAt(new Vector2(720,115)),"Barra ativa do chefe bloqueia soco por clique na interface");
   Check(b.WindingUp&&b.animationFrame==0,"Poder comeca na antecipacao");
   for(int n=0;n<32;n++)b.Tick(.02f);
   Check(b.WindingUp&&b.animationFrame<=4&&Shots()==shots,"Nenhum projetil antes dos850ms; quadros0a4 antecipam");
   for(int n=0;n<11;n++)b.Tick(.02f);
   Check(!b.WindingUp&&Shots()==shots+3&&b.currentAnimation=="spellcast"&&b.animationFrame==5,"Tres projetis saem junto com a pose de liberacao5");
   Check(Mathf.Abs(b.enemyCooldown-2.8f)<.03f,"Recarga2.8s preservada");b.Tick(.13f);
   Check(b.currentAnimation=="spellcast"&&b.animationFrame==6,"Liberacao transita para recuperacao6 antes do idle");
   var center=UnityEngine.Object.FindObjectsByType<RpgProjectile>(FindObjectsSortMode.None).Where(p=>p.owner==b).OrderBy(p=>Mathf.Abs(p.velocity.x)).First();h.invincible=0;hp=h.health;center.Advance(.7f);
   Check(h.health==hp-16,"Projetil central causa16dano em trajeto real");h.invincible=0;center.Advance(.1f);Check(h.health==hp-16,"Projetil encerrado nao aplica dano repetido");
   Reset();g.progress.questStage=1;g.progress.silenced=7;b.invincible=0;b.TakeDamage(10000);
   Check(b.dead&&g.progress.captainDefeated&&g.progress.questStage==2,"Vitoria sobre Companheiro conclui o objetivo antigo");b.Tick(42);Check(b.dead,"Chefe nao renasce");
   h.transform.position=q.giver.position;g.Interact();Check(g.progress.questStage==3&&g.progress.Has(0)&&g.progress.Has(1),"Bonero entrega Palma,voo e Coracao");
   int xp=g.progress.xp,level=g.progress.level;g.Interact();Check(g.progress.xp==xp&&g.progress.level==level,"Recompensa nao se repete");
   string json=JsonUtility.ToJson(g.progress);Reset();g.progress=JsonUtility.FromJson<RpgProgress>(json);q.RefreshState();Check(b.dead&&b.health==0,"Save existente captainDefeated conserva chefe vencido");
   g.progress=JsonUtility.FromJson<RpgProgress>("{\"version\":3,\"level\":4,\"xp\":12,\"perks\":7,\"x\":1,\"y\":-3}");q.RefreshState();Check(g.progress.level==4&&g.progress.perks==7&&g.progress.questStage==0,"Savev3 anterior a missao continua compativel");
   Check(RpgGame.SaveKey=="Cibio.RPG.v3.Progress","Mesma chave de progresso");
  }
  public IEnumerator Run(string path)
  {
   folder=path;Directory.CreateDirectory(folder);g=GetComponent<RpgGame>();yield return new WaitForSecondsRealtime(.8f);
   bool saved=PlayerPrefs.HasKey(RpgGame.SaveKey);string old=PlayerPrefs.GetString(RpgGame.SaveKey,"");
   try
   {
    Reset();GetComponent<RpgVerification>().RunChecks(folder);gameObject.AddComponent<RpgReview09>().ValidateMechanics(folder);gameObject.AddComponent<RpgVillageReview11>().ValidateMechanics(folder);
    RpgAudio.Instance.suppressOneShots=true;Tests();Check(saved==PlayerPrefs.HasKey(RpgGame.SaveKey)&&old==PlayerPrefs.GetString(RpgGame.SaveKey,""),"Save real intacto apos verificacoes");
   }
   catch(Exception ex){Debug.LogException(ex);Application.Quit(2);yield break;}
   finally{RpgAudio.Instance.suppressOneShots=false;}
   h.GetComponent<RpgPowerForm>().StopAllCoroutines();foreach(var p in UnityEngine.Object.FindObjectsByType<RpgProjectile>())Destroy(p.gameObject);foreach(var p in UnityEngine.Object.FindObjectsByType<RpgEffects>())Destroy(p.gameObject);yield return null;
   Reset();h.transform.position=(Vector2)q.giver.position+Vector2.down;g.SnapCamera();g.Interact();yield return new WaitForSecondsRealtime(.3f);yield return Shot("SeuBonero12");
   Reset();h.transform.position=b.Position+new Vector2(-1.5f,-2);h.direction=2;h.Pose("idle",0);g.progress.questStage=1;g.enabled=false;g.lens.orthographicSize=4.8f;g.lens.transform.position=new Vector3(b.Position.x,b.Position.y+.8f,-10);b.direction=2;b.Pose("idle",0);
   yield return Shot("Companheiro12");
   foreach(string action in new[]{"idle","walk","slash","spellcast","hurt"})for(int d=0;d<4;d++)for(int f=0;f<RpgArt.FrameCount("Companheiro12",action);f++)
   {
    b.direction=d;b.Pose(action,f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,$"Atlas12_{action}_{d}_{f:D2}.png"));yield return null;
   }
   b.ResetEnemy();h.transform.position=b.Position+Vector2.down*4;h.invincible=100;b.enemyCooldown=0;b.direction=2;Time.captureFramerate=24;
   for(int n=0;n<96;n++)
   {
    b.Tick(1/24f);if(n==23)yield return Shot("Poder12");yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,$"Frame_{n:D4}.png"));yield return null;
   }
   Time.captureFramerate=0;b.ResetEnemy();h.transform.position=b.Position+Vector2.down*.9f;b.enemyCooldown=0;h.invincible=100;b.Tick(.01f);b.Tick(.33f);yield return Shot("Soco12");
   g.progress.questStage=1;g.progress.silenced=7;b.invincible=0;b.TakeDamage(10000);b.Tick(.58f);yield return Shot("Vitoria12");
   g.enabled=true;Reset();h.transform.position=q.giver.position;g.progress.questStage=2;g.progress.silenced=7;g.progress.captainDefeated=true;q.RefreshState();g.Interact();g.SnapCamera();yield return Shot("Missao12");
   Check(saved==PlayerPrefs.HasKey(RpgGame.SaveKey)&&old==PlayerPrefs.GetString(RpgGame.SaveKey,""),"Capturas tambem preservam save real");
   Debug.Log("CIBIO_12_REVIEW_OK checks="+checks.Count+" nativeFrames=128");Application.Quit();
  }
  IEnumerator Shot(string name){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,name+".png"));yield return new WaitForSecondsRealtime(.16f);}
  void OnDisable(){Time.captureFramerate=0;}
 }
}
