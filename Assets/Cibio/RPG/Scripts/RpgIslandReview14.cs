using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Cibio.RPG
{
 public sealed class RpgIslandReview14:MonoBehaviour
 {
  RpgGame g;RpgActor h;string folder;readonly List<string> checks=new();int frame;
  void Check(bool ok,string text){checks.Add((ok?"PASS ":"FAIL ")+text);File.WriteAllLines(Path.Combine(folder,"Ilha-Combate14.txt"),checks);if(!ok)throw new InvalidOperationException(text);}
  void Reset(){g.StartGame(false);g.reviewPose=true;g.messageUntil=0;g.floating.Clear();h=g.world.hero;h.transform.position=new Vector2(0,-10);foreach(var e in g.world.enemies)e.transform.position=new Vector2(20,40);}
  void Step(float seconds){for(float t=0;t<seconds;t+=1/120f)h.Tick(1/120f);}
  RpgActor Target(Vector2 p,int index=0){var e=g.world.enemies[index];e.ResetEnemy();e.transform.position=p;e.invincible=0;return e;}
  void Tests()
  {
   Reset();Check(g.world.bounds==RpgIsland14.Bounds&&g.world.coastal,"Ilha96x112 com costa ativa");Check(g.world.enemies.Count==22&&g.world.enemies[13].enemyKind==4,"Oito novos inimigos preservam os14 originais e o chefe");Check(UnityEngine.Object.FindObjectsByType<RpgCache14>().Length==3,"Tres esconderijos exploraveis");
   foreach(var p in new[]{new Vector2(0,62),new Vector2(25,68),new Vector2(38,10),new Vector2(44,10),new Vector2(-39,30),new Vector2(0,-21),new Vector2(-33,63),new Vector2(30,69)})Check(Reachable(new Vector2(0,-3),p),"Rota terrestre acessivel: "+p);
   foreach(var p in new[]{new Vector2(47,20),new Vector2(0,82),new Vector2(-47,30),new Vector2(0,-27)})Check(!g.world.CanStand(p),"Mar bloqueia movimento fora da costa: "+p);
   Check(g.world.CanStand(new Vector2(45,10))&&!g.world.CanStand(new Vector2(45,14)),"Trapiche libera apenas sua superficie sobre o mar");
   Vector2 moved=g.world.Move(new Vector2(40,20),new Vector2(15,0));Check(RpgIsland14.OnLand(moved)&&moved.x<45,"Passo longo nao atravessa limite da agua");
   foreach(var name in new[]{"Lighthouse14","Cabin14","Boat14","Palm14","Pier14","CoastRock14","Cache14_closed","Cache14_open","Terrain_Sea14","Terrain_Sand14","Terrain_Wood14","Terrain_CoastRock14"})Check(RpgArt.Texture(name),"Asset costeiro integrado: "+name);
   foreach(int d in new[]{0,1,2,3})
   {
    Reset();h.facing=d==0?Vector2.up:d==1?Vector2.left:d==2?Vector2.down:Vector2.right;h.direction=d;var front=Target(h.Position+h.facing,0);var back=Target(h.Position-h.facing*.7f,3);h.Attack();Step(.42f);Check(front.health<front.MaxHealth&&back.health==back.MaxHealth,"Golpe respeita frente/costas na direcao"+d);Check(front.staggerTimer>0,"Impacto imobiliza inimigo comum na direcao"+d);
   }
   Reset();h.facing=Vector2.right;var target=Target(h.Position+Vector2.right*1.4f);var wall=new Rect(.65f,-11,.12f,2);g.world.obstacles.Add(wall);try{h.Attack();Step(.42f);Check(target.health==target.MaxHealth,"Soco nao atravessa parede fina");}finally{g.world.obstacles.Remove(wall);}
   Reset();Check(h.RequestAttack(),"Comando inicia golpe");h.Tick(.02f);Check(h.RequestAttack()&&h.QueuedCombo,"Segundo clique cedo fica no buffer");Step(.49f);Check(h.comboIndex==1&&h.attackAction=="thrust","Buffer encadeia golpe2 sem clique no fim");Step(.7f);Check(h.comboIndex==1&&h.attackTimer<=0,"Um comando nao gera varios golpes automaticos");
   Reset();h.RequestAttack();h.RequestAttack();h.CancelQueuedAttack();Step(.7f);Check(h.comboIndex==0&&h.attackTimer<=0&&!h.QueuedCombo,"Cancelamento limpa o comando antecipado");
   Reset();h.facing=Vector2.right;h.Attack();h.Tick(.1f);h.Walk(Vector2.up,2.8f,.016f);Check(h.direction==3&&h.velocity==Vector2.zero,"Antecipacao conserva direcao e trava deslocamento");h.Tick(.34f);h.Walk(Vector2.up,2.8f,.016f);Check(h.velocity.y>0,"Recuperacao final aceita movimento sem travar controle");
   Reset();var ordinary=Target(new Vector2(2,-10));ordinary.Attack();ordinary.Stagger(.3f,true);Check(ordinary.attackTimer==0&&ordinary.staggerTimer==.3f,"Finalizador interrompe ataque comum");var heavy=Target(new Vector2(3,-10),1);heavy.Stagger(.3f,true);Check(heavy.staggerTimer<ordinary.staggerTimer,"Blindado resiste mais ao stagger");var boss=Target(new Vector2(4,-10),13);boss.Stagger(.3f,false);Check(boss.staggerTimer==0,"Chefe ignora interrupcao por soco leve");boss.Stagger(.3f,true);Check(boss.staggerTimer>0&&boss.staggerTimer<heavy.staggerTimer,"Chefe recebe resposta curta ao finalizador");
   for(int stage=0;stage<3;stage++)for(int d=0;d<4;d++)
   {
    Reset();h.buff=stage>0?20:0;h.formStage=stage;Vector2 dir=d==0?Vector2.up:d==1?Vector2.left:d==2?Vector2.down:Vector2.right;Vector2 start=h.Position;Check(h.TryDodge(dir)&&h.energy==90,"Esquiva custa10, forma"+stage+" direcao"+d);h.invincible=0;h.invincible=.13f;float hp=h.health;h.TakeDamage(99);Check(h.health==hp,"Janela inicial evita dano, forma"+stage+" direcao"+d);
    for(int n=0;n<24;n++){h.Walk(dir,2.8f,1/120f);h.Tick(1/120f);}float distance=Vector2.Distance(start,h.Position),expected=(stage==2?13.5f:stage==1?11.5f:9)*.2f;Check(Mathf.Abs(distance-expected)<.025f&&!h.Dodging&&h.dodgeCooldown>0,"Esquiva percorre distancia continua esperada, forma"+stage+" direcao"+d);Check(!h.TryDodge(dir),"Recarga impede esquiva repetida, forma"+stage+" direcao"+d);
   }
   Reset();h.energy=9;Check(!h.TryDodge(Vector2.right),"Energia insuficiente bloqueia esquiva");h.energy=100;h.flying=true;Check(!h.TryDodge(Vector2.right),"Esquiva nao substitui controle de voo");h.flying=false;h.charging=true;Check(!h.TryDodge(Vector2.right),"Carga de energia nao pode ser cancelada por esquiva");
   Reset();wall=new Rect(.8f,-11,.1f,2);g.world.obstacles.Add(wall);try{h.TryDodge(Vector2.right);h.Walk(Vector2.right,2.8f,.2f);h.Tick(.2f);Check(h.Position.x<.8f&&g.world.CanStand(h.Position),"Esquiva nao atravessa parede mesmo com200ms");}finally{g.world.obstacles.Remove(wall);}
   Reset();h.TryDodge(Vector2.right);h.Tick(.15f);h.invincible=0;h.TakeDamage(12);Check(h.health==h.MaxHealth-12,"Esquiva nao concede invulnerabilidade permanente");h.TakeDamage(1000);h.invincible=0;h.TakeDamage(1000);Check(h.dead&&!h.Dodging&&!h.QueuedCombo,"Morte limpa esquiva e combo");
   Reset();var cache=UnityEngine.Object.FindObjectsByType<RpgCache14>().First(c=>c.id==0);h.transform.position=(Vector2)cache.transform.position+Vector2.down;foreach(var e in g.world.enemies)e.transform.position=new Vector2(20,40);h.jumpTimer=.4f;Check(g.Interact()&&!cache.Open,"Salto nao permite abrir bau");h.jumpTimer=0;h.flying=true;Check(g.Interact()&&!cache.Open,"Voo bloqueia bau antes de subir");h.flying=false;var guard=Target((Vector2)cache.transform.position+Vector2.right);Check(g.Interact()&&!cache.Open,"Inimigo proximo protege recompensa");guard.transform.position=new Vector2(20,40);Check(g.Interact()&&(g.progress.islandCaches&1)!=0&&cache.Open,"Interacao abre esconderijo e salva bit");int xp=g.progress.xp,level=g.progress.level;g.Interact();Check(g.progress.xp==xp&&g.progress.level==level,"Esconderijo nao duplica XP");string saved=JsonUtility.ToJson(g.progress);g.progress=JsonUtility.FromJson<RpgProgress>(saved);Check(cache.Open,"Esconderijo permanece aberto apos serializacao");g.progress=JsonUtility.FromJson<RpgProgress>("{\"version\":3,\"level\":4,\"perks\":7,\"captainDefeated\":true}");Check(g.progress.islandCaches==0&&g.progress.level==4&&g.progress.captainDefeated,"Savev3 anterior recebe campo novo sem perder progresso");
  }
  float KillTime(int stage,int index)
  {
   Reset();h.buff=stage>0?100:0;h.formStage=stage;h.facing=Vector2.right;var target=Target(h.Position+Vector2.right,index);float elapsed=0;
   for(int n=0;n<2400&&!target.dead;n++){h.RequestAttack();h.Tick(1/120f);target.invincible=Mathf.Max(0,target.invincible-1/120f);target.transform.position=h.Position+Vector2.right;elapsed+=1/120f;}
   Check(target.dead,"Combate real conclui alvo estacionario,forma"+stage+" tipo"+target.enemyKind);return elapsed;
  }
  void Balance()
  {
   float[] boss=new float[3];var rows=new List<string>{"form,armored_seconds,boss_seconds"};for(int stage=0;stage<3;stage++){float armor=KillTime(stage,1);boss[stage]=KillTime(stage,13);rows.Add(stage+","+armor.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+","+boss[stage].ToString("F3",System.Globalization.CultureInfo.InvariantCulture));}Check(boss[0]>boss[1]&&boss[1]>boss[2],"Evolucao reduz tempo real para vencer chefe no ensaio controlado");File.WriteAllLines(Path.Combine(folder,"Balance14.csv"),rows);
  }
  public void ValidateMechanics(string path){folder=path;Directory.CreateDirectory(folder);g=GetComponent<RpgGame>();Tests();Balance();}
  public IEnumerator Run(string path)
  {
   folder=path;Directory.CreateDirectory(folder);g=GetComponent<RpgGame>();if(Array.IndexOf(Environment.GetCommandLineArgs(),"-cibio-review-visible")>=0){Debug.Log("CIBIO_14_WAITING_FOR_VISIBLE_WINDOW");float deadline=Time.realtimeSinceStartup+60;while(!Application.isFocused&&Time.realtimeSinceStartup<deadline)yield return null;if(!Application.isFocused){Debug.LogError("Revisao visual precisa da janela do player em foco.");Application.Quit(3);yield break;}Debug.Log("CIBIO_14_VISIBLE_REVIEW_STARTED");}yield return new WaitForSecondsRealtime(.8f);bool had=PlayerPrefs.HasKey(RpgGame.SaveKey);string old=PlayerPrefs.GetString(RpgGame.SaveKey,"");
   try{Reset();GetComponent<RpgVerification>().RunChecks(folder);RpgAudio.Instance.suppressOneShots=true;gameObject.AddComponent<RpgReview09>().ValidateMechanics(folder);gameObject.AddComponent<RpgVillageReview11>().ValidateMechanics(folder);gameObject.AddComponent<RpgBossReview12>().ValidateMechanics(folder);gameObject.AddComponent<RpgArtReview13>().ValidateMechanics(folder);Tests();Balance();Check(had==PlayerPrefs.HasKey(RpgGame.SaveKey)&&old==PlayerPrefs.GetString(RpgGame.SaveKey,""),"Save real preservado nos testes");}
   catch(Exception ex){Debug.LogException(ex);Application.Quit(2);yield break;}finally{RpgAudio.Instance.suppressOneShots=false;}
   Reset();yield return Still("Porto14",new Vector2(37,15),8);yield return Still("Trapiche14",new Vector2(43,10),6.8f);g.Interact();yield return new WaitForSecondsRealtime(.15f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"BauAberto14.png"));yield return new WaitForSecondsRealtime(.15f);yield return Still("Farol14",new Vector2(25,67),8);yield return Still("Praia14",new Vector2(-39,31),8);yield return Still("Bosque14",new Vector2(-26,65),8);yield return Still("Vila14",new Vector2(0,-3),7.03125f);
   Reset();g.enabled=false;h.transform.position=new Vector2(0,-3);g.lens.orthographicSize=5.2f;g.lens.transform.position=new Vector3(0,-2.2f,-10);var e=Target(new Vector2(1,-3));e.health=1000;e.invincible=0;e.enemyCooldown=100;h.facing=Vector2.right;h.direction=3;Time.captureFramerate=24;
   for(int combo=0;combo<3;combo++){h.RequestAttack();for(int n=0;n<18;n++){h.Tick(1/24f);e.Tick(1/24f);g.impactPause=0;yield return Capture();}e.transform.position=h.Position+Vector2.right;}
   h.attackTimer=0;for(int d=0;d<4;d++){h.dodgeCooldown=0;h.energy=100;Vector2 dir=d==0?Vector2.up:d==1?Vector2.left:d==2?Vector2.down:Vector2.right;h.TryDodge(dir);for(int n=0;n<7;n++){h.Walk(dir,2.8f,1/24f);h.Tick(1/24f);yield return Capture();}}Time.captureFramerate=0;
   Reset();g.enabled=true;g.reviewPose=false;h.transform.position=new Vector2(37,31);h.invincible=100;g.SnapCamera();yield return new WaitForSecondsRealtime(.3f);float start=Time.realtimeSinceStartup;for(int n=0;n<120;n++)yield return new WaitForEndOfFrame();float elapsed=Time.realtimeSinceStartup-start;File.WriteAllText(Path.Combine(folder,"Desempenho14.txt"),$"120 quadros reais sem captura: {elapsed:F3}s; {120/elapsed:F1}FPS. Amostra curta nesta maquina, nao benchmark.\n");g.reviewPose=true;g.enabled=false;
   GetComponent<RpgInterface>().enabled=false;g.lens.orthographicSize=57;g.lens.transform.position=new Vector3(0,28,-10);var rt=new RenderTexture(1400,1663,24);g.lens.targetTexture=rt;g.lens.Render();var prior=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(1400,1663,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1400,1663),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,"Ilha14.png"),image.EncodeToPNG());g.lens.targetTexture=null;RenderTexture.active=prior;Destroy(image);Destroy(rt);Check(had==PlayerPrefs.HasKey(RpgGame.SaveKey)&&old==PlayerPrefs.GetString(RpgGame.SaveKey,""),"Capturas e desempenho preservam save real");Debug.Log("CIBIO_14_REVIEW_OK checks="+checks.Count);Application.Quit();
  }
  IEnumerator Still(string name,Vector2 point,float size){Reset();g.enabled=false;h.transform.position=point;h.Pose("idle",0);g.lens.orthographicSize=size;float width=size*g.lens.aspect;var b=g.world.bounds;g.lens.transform.position=new Vector3(Mathf.Clamp(point.x,b.xMin+width,b.xMax-width),Mathf.Clamp(point.y+.8f,b.yMin+size,b.yMax-size),-10);yield return new WaitForSecondsRealtime(.2f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,name+".png"));yield return new WaitForSecondsRealtime(.15f);}
  IEnumerator Capture(){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,$"Frame_{frame++:D4}.png"));yield return null;}
  bool Reachable(Vector2 a,Vector2 target){var q=new Queue<Vector2Int>();var seen=new HashSet<Vector2Int>();q.Enqueue(Vector2Int.RoundToInt(a));seen.Add(Vector2Int.RoundToInt(a));while(q.Count>0){var p=q.Dequeue();if(Vector2.Distance(p,target)<1.5f)return true;foreach(var d in new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right}){var n=p+d;if(seen.Add(n)&&g.world.CanStand(n))q.Enqueue(n);}}return false;}
  void OnDisable(){Time.captureFramerate=0;}
 }
}
