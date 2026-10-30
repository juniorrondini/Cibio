using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Cibio.RPG
{
 public sealed class RpgArtReview13:MonoBehaviour
 {
  RpgGame g;RpgActor h;string folder;readonly List<string> checks=new();
  void Check(bool ok,string text){checks.Add((ok?"PASS ":"FAIL ")+text);File.WriteAllLines(Path.Combine(folder,"Arte13.txt"),checks);if(!ok)throw new InvalidOperationException(text);}
  void Clean(){foreach(var p in UnityEngine.Object.FindObjectsByType<RpgProjectile>()){p.gameObject.SetActive(false);Destroy(p.gameObject);}foreach(var p in UnityEngine.Object.FindObjectsByType<RpgPalmaImpact13>()){p.gameObject.SetActive(false);Destroy(p.gameObject);}}
  void Reset(){Clean();g.StartGame(false);g.reviewPose=true;g.messageUntil=0;g.floating.Clear();h=g.world.hero;h.GetComponent<RpgPowerForm>().StopAllCoroutines();h.transform.position=new Vector2(0,-10);g.SnapCamera();}
  void CapturePosition(float y=46){h.transform.position=new Vector2(0,y);g.lens.orthographicSize=4.8f;g.lens.transform.position=new Vector3(0,y+.8f,-10);foreach(var e in g.world.enemies)e.transform.position=new Vector2(20,-10);}
  SpriteRenderer Renderer(Component p)=>p.GetComponentInChildren<SpriteRenderer>();
  MaterialPropertyBlock Properties(Component p){var m=new MaterialPropertyBlock();Renderer(p).GetPropertyBlock(m);return m;}
  RpgProjectile Latest()=>UnityEngine.Object.FindObjectsByType<RpgProjectile>(FindObjectsSortMode.None).Single(p=>p.owner==h);
  void Form(int stage){h.buff=stage>0?20:0;h.formStage=stage;h.transforming=h.powerPose=h.attackTimer=0;h.energy=100;h.Pose("idle",0);h.GetComponent<RpgPowerForm>().Refresh(1);}
  void Tests()
  {
   Reset();var title=RpgArt.Texture("Title_Vila13");Check(title&&Mathf.Abs(title.width/(float)title.height-16/9f)<.01f,"Tela inicial nova em16:9");var icon=RpgArt.Texture("UI_Palma13");Check(icon&&icon.width==256&&icon.height==256,"Palma tem icone proprio256px");
   foreach(var name in new[]{"Vfx_PalmaFlight13","Vfx_PalmaImpact13"})
   {
    var t=RpgArt.Texture(name);Check(t&&t.width==1024&&t.height==1024&&t.filterMode==FilterMode.Bilinear,"Atlas16quadros sem redimensionamento: "+name);
    var r=RpgEffects.Create("Revisao atlas",null);for(int f=0;f<16;f++){RpgEffects.Paint(r,name,f,Color.white,1);var m=new MaterialPropertyBlock();r.GetPropertyBlock(m);Check(m.GetTexture("_Atlas")==t&&m.GetVector("_Grid")==new Vector4(4,4,16,0)&&m.GetFloat("_Frame")==f,$"Shader seleciona {name} quadro{f}");}r.gameObject.SetActive(false);Destroy(r.gameObject);
   }
   h.energy=100;Check(!g.Bolt()&&h.energy==100,"Skill3 continua bloqueada antes da missao");
   for(int stage=0;stage<3;stage++)
   {
    Reset();g.progress.questStage=3;Form(stage);h.facing=Vector2.right;h.direction=3;var target=g.world.enemies[1];target.ResetEnemy();target.transform.position=h.Position+Vector2.right*3;float hp=target.health;
    Check(g.Bolt()&&h.energy==78&&Mathf.Abs(g.boltCooldown-(stage==2?1.1f:1.8f))<.001f,"Custo22 e recarga preservados,forma"+stage);var p=Latest();p.enabled=false;var r=Renderer(p);var m=Properties(p);
    Check(r.enabled&&m.GetTexture("_Atlas").name=="Vfx_PalmaFlight13"&&r.gameObject.layer==RpgEnergyParticles.Layer,"Projetil nasce texturizado na camada de energia,forma"+stage);
    Check(p.velocity==Vector2.right*(stage==2?11:8)&&r.transform.localScale.y==(stage==2?2.5f:stage==1?2.1f:1.8f),"Velocidade preservada e porte visual por forma"+stage);
    Check(!g.Bolt()&&h.energy==78,"Recarga impede disparo repetido,forma"+stage);
    float damage=h.Damage*(stage==2?1.8f:1.35f);p.Advance(.3f);Check(Mathf.Abs(target.health-(hp-damage))<.001f,"Dano original do projetil,forma"+stage);target.invincible=0;p.Advance(.1f);Check(Mathf.Abs(target.health-(hp-damage))<.001f,"Impacto unico preservado,forma"+stage);
    var impact=UnityEngine.Object.FindAnyObjectByType<RpgPalmaImpact13>();Check(impact&&Properties(impact).GetTexture("_Atlas").name=="Vfx_PalmaImpact13"&&!r.enabled,"Impacto exclusivo substitui explosao generica,forma"+stage);impact.enabled=false;impact.Advance(.51f);Check(!Renderer(impact).enabled,"Impacto encerra sem quadro opaco,forma"+stage);
   }
   Reset();g.progress.questStage=3;h.facing=Vector2.right;var wall=new Rect(1.5f,-11,.1f,2);g.world.obstacles.Add(wall);
   try{g.Bolt();var p=Latest();p.enabled=false;p.Advance(.4f);var impact=UnityEngine.Object.FindAnyObjectByType<RpgPalmaImpact13>();Check(impact&&impact.transform.position.x<1.5f&&!Renderer(p).enabled,"Parede fina interrompe Palma e recebe impacto no lado correto");}finally{g.world.obstacles.Remove(wall);}
   Reset();var enemy=g.world.enemies[1];enemy.transform.position=new Vector2(5,-10);var old=RpgProjectile.Fire(enemy,Vector2.right);old.enabled=false;old.Advance(0);Check(Properties(old).GetTexture("_Atlas").name=="Vfx_Wisp06","Projeteis inimigos mantem visual anterior");
   Check(RpgGame.SaveKey=="Cibio.RPG.v3.Progress","Chave de save preservada");
  }
  public void ValidateMechanics(string path){folder=path;Directory.CreateDirectory(folder);g=GetComponent<RpgGame>();Tests();}
  public IEnumerator Run(string path)
  {
   folder=path;Directory.CreateDirectory(folder);g=GetComponent<RpgGame>();yield return new WaitForSecondsRealtime(.8f);yield return Shot("Menu13-A");yield return new WaitForSecondsRealtime(1.2f);yield return Shot("Menu13-B");bool saved=PlayerPrefs.HasKey(RpgGame.SaveKey);string save=PlayerPrefs.GetString(RpgGame.SaveKey,"");
   try
   {
    Reset();GetComponent<RpgVerification>().RunChecks(folder);RpgAudio.Instance.suppressOneShots=true;gameObject.AddComponent<RpgReview09>().ValidateMechanics(folder);gameObject.AddComponent<RpgVillageReview11>().ValidateMechanics(folder);gameObject.AddComponent<RpgBossReview12>().ValidateMechanics(folder);Tests();Check(saved==PlayerPrefs.HasKey(RpgGame.SaveKey)&&save==PlayerPrefs.GetString(RpgGame.SaveKey,""),"Verificacao preserva save real");
   }
   catch(Exception ex){Debug.LogException(ex);Application.Quit(2);yield break;}
   finally{RpgAudio.Instance.suppressOneShots=false;}
   Reset();g.enabled=false;CapturePosition();g.progress.questStage=3;g.progress.perks=7;h.facing=Vector2.right;h.direction=3;GetComponent<RpgInterface>().ReviewSkill(4);yield return Shot("Skill3-13");GetComponent<RpgInterface>().ReviewSkill(-1);
   // Freeze a complete native atlas review; no generated preview substitutes for these captures.
   g.enabled=false;CapturePosition(45);
   foreach(string atlas in new[]{"Vfx_PalmaFlight13","Vfx_PalmaImpact13"})
   {
    var r=RpgEffects.Create("Revisao16quadros",null);r.transform.position=new Vector2(3,46.3f);r.transform.localScale=atlas.Contains("Flight")?new Vector3(1.3f,2.5f,1):new Vector3(2.8f,2.3f,1);r.sortingOrder=RpgWorld.Order(45)+4;
    for(int f=0;f<16;f++){RpgEffects.Paint(r,atlas,f,Color.white,1.6f);yield return Shot($"Atlas13_{atlas}_{f:D2}");}r.gameObject.SetActive(false);Destroy(r.gameObject);
   }
   Time.captureFramerate=24;int frame=0;
   for(int stage=0;stage<3;stage++)for(int d=0;d<4;d++)
   {
    Reset();g.enabled=false;CapturePosition(d==0?43:46);g.progress.questStage=3;Form(stage);h.direction=d;h.facing=d==0?Vector2.up:d==1?Vector2.left:d==2?Vector2.down:Vector2.right;g.Bolt();var p=Latest();p.enabled=false;
    for(int n=0;n<10;n++){h.Tick(1/24f);if(p)p.Advance(1/24f);g.boltCooldown=Mathf.Max(0,g.boltCooldown-1/24f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,$"Frame_{frame++:D4}.png"));yield return null;}
   }
   Time.captureFramerate=0;yield return new WaitForSecondsRealtime(.2f);for(int stage=0;stage<3;stage++)for(int d=0;d<4;d++)File.Copy(Path.Combine(folder,$"Frame_{(stage*4+d)*10+4:D4}.png"),Path.Combine(folder,$"Palma13_{stage}_{d}.png"),true);
   Reset();g.enabled=false;CapturePosition();g.progress.questStage=3;Form(2);h.facing=Vector2.right;h.direction=3;var target=g.world.enemies[1];target.transform.position=new Vector2(3,46);target.invincible=100;g.Bolt();var shot=Latest();shot.enabled=false;shot.Advance(.3f);UnityEngine.Object.FindAnyObjectByType<RpgPalmaImpact13>().Advance(.11f);yield return Shot("Impacto13");
   var fx=UnityEngine.Object.FindAnyObjectByType<RpgPalmaImpact13>();g.mode=2;var before=Properties(fx).GetFloat("_Frame");yield return new WaitForSecondsRealtime(.15f);Check(Properties(fx).GetFloat("_Frame")==before,"Pausa congela animacao do impacto");g.mode=1;
   Check(saved==PlayerPrefs.HasKey(RpgGame.SaveKey)&&save==PlayerPrefs.GetString(RpgGame.SaveKey,""),"Capturas preservam save real");Debug.Log("CIBIO_13_REVIEW_OK checks="+checks.Count+" vfxFrames=32 castViews=12");Application.Quit();
  }
  IEnumerator Shot(string name){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,name+".png"));yield return new WaitForSecondsRealtime(.12f);}
  void OnDisable(){Time.captureFramerate=0;}
 }
}
