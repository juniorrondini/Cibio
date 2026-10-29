using System.Collections;
using System.IO;
using UnityEngine;
namespace Cibio.RPG
{
 public sealed class RpgVfxReview:MonoBehaviour
 {
  RpgGame game;string folder;
  public IEnumerator Run(string directory)
  {
   game=GetComponent<RpgGame>();folder=directory;Directory.CreateDirectory(folder);yield return new WaitForSecondsRealtime(.7f);
   game.StartGame(false);game.reviewPose=true;GetComponent<RpgVerification>().RunChecks(folder);
   foreach(var fx in Object.FindObjectsByType<RpgEffects>())Destroy(fx.gameObject);foreach(var p in Object.FindObjectsByType<RpgProjectile>())Destroy(p.gameObject);foreach(var ps in Object.FindObjectsByType<ParticleSystem>())ps.Clear();
   var h=game.world.hero;h.GetComponent<RpgPowerForm>().StopAllCoroutines();yield return new WaitForSecondsRealtime(.3f);game.StartGame(false);game.reviewPose=true;game.messageUntil=0;
   h.transform.position=new Vector2(0,-3);h.Pose("idle",0);game.SnapCamera();foreach(var enemy in game.world.enemies){enemy.transform.position=new Vector2(16+enemy.enemyKind,10);enemy.Pose("idle",0);}
   Time.captureFramerate=24;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"Antes.png"));
   game.SetChargeHeld(true);yield return Frames("Charge",80,true);game.SetChargeHeld(false);yield return Frames("Gold",60,true);game.messageUntil=0;
   for(int n=0;n<48;n++){h.Walk(n<24?Vector2.right:Vector2.left,5.85f,1/24f);h.Tick(1/24f);yield return new WaitForEndOfFrame();Shot("Run",n);}h.velocity=Vector2.zero;h.direction=2;h.facing=Vector2.down;h.Pose("idle",0);
   h.Jump();yield return Frames("Jump",20,true);
   game.progress.perks=7;h.energy=100;h.facing=Vector2.right;game.dashCooldown=0;game.Dash();for(int n=0;n<32;n++){h.Walk(Vector2.right,8,1/24f);h.Tick(1/24f);yield return new WaitForEndOfFrame();Shot("Flight",n);}h.Land();h.velocity=Vector2.zero;yield return Frames("Landing",18,true);
   h.energy=100;game.waveCooldown=0;game.Wave();yield return Frames("Wave",24,true);
   h.facing=Vector2.down;h.direction=2;
   for(int strike=0;strike<3;strike++)
   {
    var e=game.world.enemies[0];e.transform.position=h.Position+Vector2.down*.85f;e.dead=false;e.health=e.MaxHealth;e.invincible=0;h.Attack();
    for(int n=0;n<16;n++){h.Tick(1/24f);yield return new WaitForEndOfFrame();Shot("Combo",strike*16+n);}
   }
   h.charging=false;h.charge=h.buff=h.transforming=0;yield return Frames("Fade",38,true);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"Depois.png"));
   Time.captureFramerate=0;yield return new WaitForSecondsRealtime(.5f);for(int i=0;i<12;i++)yield return new WaitForEndOfFrame();
   h.buff=16;h.Pose("idle",0);float start=Time.realtimeSinceStartup;for(int i=0;i<120;i++)yield return new WaitForEndOfFrame();float seconds=Time.realtimeSinceStartup-start;
   int particles=0,limit=0;foreach(var ps in Object.FindObjectsByType<ParticleSystem>()){particles+=ps.particleCount;limit+=ps.main.maxParticles;}
   File.WriteAllText(Path.Combine(folder,"VFX-Runtime.txt"),"120 quadros sem captura: "+seconds.ToString("F3")+"s; FPS observado: "+(120/seconds).ToString("F1")+"; partículas vivas: "+particles+"; limite total: "+limit+"; camadas: "+h.GetComponent<RpgAura>().LayerCount+".\nCapturas: passo fixo 24fps; gameplay normal mantém tempo real.\n");
   Debug.Log("CIBIO_VFX_06_REVIEW_OK FPS="+(120/seconds));Application.Quit();
  }
  IEnumerator Frames(string prefix,int count,bool tick){for(int n=0;n<count;n++){if(tick)game.world.hero.Tick(1/24f);yield return new WaitForEndOfFrame();Shot(prefix,n);}}
  void Shot(string prefix,int n){ScreenCapture.CaptureScreenshot(Path.Combine(folder,prefix+"_"+n.ToString("D3")+".png"));}
  void OnDisable(){Time.captureFramerate=0;}
 }
}
