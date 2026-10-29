#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Cibio.City
{
    public sealed class CityVerificationRunner : MonoBehaviour
    {
        private CityMotor player;
        private StreetEnemy[] enemies;
        private readonly List<string> report=new List<string>();
        private int failures;
        private float start;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Launch()
        {
            if(!SessionState.GetBool("Cibio.CityVerify",false))return;
            SessionState.SetBool("Cibio.CityVerify",false);new GameObject("City verification").AddComponent<CityVerificationRunner>();
        }
        private void Update(){if(Time.realtimeSinceStartup-start>100){report.Add("FAIL Test timeout");Finish(1);}}
        private IEnumerator Start()
        {
            start=Time.realtimeSinceStartup;Application.targetFrameRate=60;QualitySettings.vSyncCount=0;
            player=FindAnyObjectByType<CityMotor>();player.AutomatedInput=true;
            enemies=FindObjectsByType<StreetEnemy>();foreach(var actor in enemies)actor.enabled=false;
            yield return new WaitForSeconds(.5f);
            Check(player.Grounded,"Personagem repousa na calcada");
            Warp(new Vector3(0,.18f,-6));yield return new WaitForSeconds(.2f);
            player.TestInput=new CityInput{move=Vector2.right};yield return new WaitForSeconds(.6f);
            Check(player.transform.position.x>1,"Movimento horizontal X");
            Check(Mathf.Abs(player.Speed-player.walkSpeed)<.15f,"Velocidade de caminhada");
            player.TestInput=default;yield return new WaitForSeconds(.3f);Check(player.Speed<.05f,"Desaceleracao suave ate parar");

            Warp(new Vector3(0,.18f,-6));yield return new WaitForSeconds(.2f);
            player.TestInput=new CityInput{move=Vector2.up};yield return new WaitForSeconds(.6f);
            Check(player.transform.position.z>-5&&player.transform.position.y<.3f,"W avanca no mapa sem acionar salto");

            Warp(new Vector3(0,.18f,-6));yield return new WaitForSeconds(.2f);
            player.TestInput=new CityInput{move=Vector2.one};yield return new WaitForSeconds(.6f);
            Check(Mathf.Abs(player.Speed-player.walkSpeed)<.15f,"Diagonal normalizada sem vantagem de velocidade");
            Check(Vector3.Dot(player.visual.forward,new Vector3(1,0,1).normalized)>.98f,"Personagem orienta corpo na diagonal");

            Warp(new Vector3(0,.18f,-6));yield return new WaitForSeconds(.2f);
            player.TestInput=new CityInput{move=Vector2.right,run=true};yield return new WaitForSeconds(.7f);
            Check(Mathf.Abs(player.Speed-player.runSpeed)<.15f,"Corrida na velocidade configurada");
            var animator=player.visual.GetComponent<Animator>();
            Check(animator.GetFloat("Speed")>3.9f,"Animator recebe velocidade para transicao de corrida");

            Warp(new Vector3(0,.18f,-6));yield return new WaitForSeconds(.25f);
            var camera=FindAnyObjectByType<CityCamera>();float focusHeight=camera.Focus.y;
            float ground=player.transform.position.y;
            player.TestInput=new CityInput{jumpPressed=true,jumpHeld=true};yield return null;player.TestInput=new CityInput{jumpHeld=true};
            float longPeak=ground,end=Time.time+1.1f;
            while(Time.time<end){longPeak=Mathf.Max(longPeak,player.transform.position.y);yield return null;}
            Check(longPeak-ground>.7f&&longPeak-ground<1.0f,"Pulo complementar respeita altura: "+(longPeak-ground).ToString("0.00"));
            Check(player.Grounded,"Aterrissagem apos salto");
            Check(Mathf.Abs(camera.Focus.y-focusHeight)<.01f,"Camera vista de cima nao sobe junto com o pulo");
            Warp(new Vector3(0,.18f,-6));yield return new WaitForSeconds(.25f);
            player.TestInput=new CityInput{jumpPressed=true,jumpHeld=true};yield return null;player.TestInput=new CityInput{jumpHeld=true};
            yield return new WaitForSeconds(.055f);player.TestInput=default;
            float shortPeak=ground;end=Time.time+.8f;
            while(Time.time<end){shortPeak=Mathf.Max(shortPeak,player.transform.position.y);yield return null;}
            Check(shortPeak-ground<(longPeak-ground)*.8f,"Soltar espaco reduz a altura do pulo");

            var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.name="Verification wall";wall.transform.position=new Vector3(1.2f,.9f,-6);wall.transform.localScale=new Vector3(.3f,1.8f,3);
            Warp(new Vector3(0,.18f,-6));yield return new WaitForSeconds(.2f);player.TestInput=new CityInput{move=Vector2.right,run=true};yield return new WaitForSeconds(.8f);
            Check(player.transform.position.x<.87f,"Colisao bloqueia passagem por obstaculos");Destroy(wall);

            Warp(new Vector3(0,.18f,-6));yield return new WaitForSeconds(.25f);
            var enemy=enemies[0];MoveEnemy(enemy,new Vector3(0,.18f,-4.8f));enemy.target=player;
            player.visual.rotation=Quaternion.LookRotation(Vector3.back);
            var combat=player.GetComponent<MeleeCombat>();var enemyHealth=enemy.GetComponent<Vitality>();
            combat.TryAttack();yield return new WaitForSeconds(.65f);Check(enemyHealth.Current==enemyHealth.maximum,"Ataque nao atinge inimigo pelas costas do jogador");
            player.visual.rotation=Quaternion.identity;combat.TryAttack();yield return new WaitForSeconds(.65f);
            Check(enemyHealth.Current==20&&combat.ConfirmedHits==1,"Ataque frontal aplica um unico dano");
            combat.TryAttack();yield return new WaitForSeconds(.65f);Check(enemyHealth.IsDead&&MeleeCombat.DefeatedEnemies==1,"Segundo ataque derrota o inimigo e registra pontuacao");

            var ai=enemies[1];Warp(new Vector3(0,.18f,-4));MoveEnemy(ai,new Vector3(0,.18f,-7.5f));ai.target=player;ai.enabled=true;
            float distance=Vector3.Distance(ai.transform.position,player.transform.position);
            yield return new WaitForSeconds(.7f);Check(Vector3.Distance(ai.transform.position,player.transform.position)<distance-.6f,"Inimigo persegue o jogador no mapa");
            yield return new WaitForSeconds(1.8f);Check(player.GetComponent<Vitality>().Current<100,"Ataque do inimigo causa dano apos preparacao");
            player.SetPaused(true);Vector3 pausedPlayer=player.transform.position,pausedEnemy=ai.transform.position;
            yield return new WaitForSeconds(.4f);
            Check(Vector3.Distance(player.transform.position,pausedPlayer)<.001f&&Vector3.Distance(ai.transform.position,pausedEnemy)<.001f,"Pausa congela jogador e inimigos");
            Check(animator.speed==0,"Pausa congela animacao");player.SetPaused(false);ai.enabled=false;

            Warp(new Vector3(0,.18f,-6));yield return new WaitForSeconds(1.1f);player.GetComponent<Vitality>().Damage(1000,player.transform.position-Vector3.forward);
            Vector3 deathPosition=player.transform.position;player.TestInput=new CityInput{move=Vector2.right};yield return new WaitForSeconds(.4f);
            Check(player.GetComponent<Vitality>().IsDead&&Vector3.Distance(deathPosition,player.transform.position)<.05f,"Personagem sem vida nao continua andando");
            player.Respawn();yield return new WaitForSeconds(.25f);Check(player.GetComponent<Vitality>().Current==100&&player.Grounded,"Retorno restaura vida e movimento");

            // Verify an actual running pose deforms the rigged mesh without invalid coordinates.
            player.TestInput=new CityInput{move=Vector2.right,run=true};yield return new WaitForSeconds(.6f);
            var skin=player.visual.GetComponent<SkinnedMeshRenderer>();var baked=new Mesh();skin.BakeMesh(baked);
            bool valid=true;foreach(var point in baked.vertices)if(float.IsNaN(point.x)||float.IsNaN(point.y)||point.magnitude>4)valid=false;
            Check(valid&&baked.vertexCount==skin.sharedMesh.vertexCount,"Malha deformada permanece valida durante corrida");Destroy(baked);
            Warp(new Vector3(0,.18f,-6));yield return new WaitForSeconds(.4f);camera.Snap();
            Capture(camera.GetComponent<Camera>());
            Finish(failures==0?0:1);
        }
        private void Warp(Vector3 point){player.TestInput=default;player.spawn=point;player.Respawn();Physics.SyncTransforms();}
        private static void MoveEnemy(StreetEnemy enemy,Vector3 point)
        {var controller=enemy.GetComponent<CharacterController>();controller.enabled=false;enemy.transform.position=point;controller.enabled=true;Physics.SyncTransforms();}
        private void Check(bool success,string text){report.Add((success?"PASS ":"FAIL ")+text);if(!success)failures++;}
        private void Finish(int code)
        {
            var folder=Path.Combine(Path.GetDirectoryName(Application.dataPath),"CityReview");Directory.CreateDirectory(folder);File.WriteAllLines(Path.Combine(folder,"PlayValidation.txt"),report);
            Debug.Log("CIBIO_CITY_VERIFY: "+report.Count+" checks; "+failures+" failures.");EditorApplication.Exit(code);
        }
        private static void Capture(Camera view)
        {
            var target=new RenderTexture(1600,1000,24,RenderTextureFormat.ARGBHalf);var previous=RenderTexture.active;
            view.targetTexture=target;view.Render();RenderTexture.active=target;var texture=new Texture2D(1600,1000,TextureFormat.RGB24,false);
            texture.ReadPixels(new Rect(0,0,1600,1000),0,0);texture.Apply();
            File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Application.dataPath),"CityReview/City-PlayMode.png"),texture.EncodeToPNG());
            view.targetTexture=null;RenderTexture.active=previous;Destroy(texture);Destroy(target);
        }
    }
}
#endif
