using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Cibio.RPG
{
    public sealed class RpgOrbitReview16:MonoBehaviour
    {
        RpgGame g;RpgActor h;string folder;readonly List<string> checks=new();
        public int Failures{get;private set;}
        void Check(bool ok,string label){if(!ok)Failures++;checks.Add((ok?"PASS ":"FAIL ")+label);File.WriteAllLines(Path.Combine(folder,"Orbita16.txt"),checks);}
        void StartFlight(Vector2 facing)
        {
            g.Secret.enabled=true;g.StartGame(false);g.reviewPose=true;g.mode=1;h=g.world.hero;h.transform.position=new Vector2(0,-10);
            g.progress.perks=7;g.progress.questStage=3;h.buff=16;h.formStage=2;h.flying=true;h.flightHeight=2.2f;h.facing=facing;h.energy=100;
        }
        void Step(Vector2 input,float dt=1/60f){g.DriveHero(input,true,false,dt);g.TickWorld(dt);}
        void UntilLaps(Vector2 input,int laps,float dt=1/60f)
        {
            for(int k=0;k<Mathf.CeilToInt(35/dt)&&g.Secret.Active&&g.Secret.Laps<laps;k++)Step(input,dt);
        }
        void Tests()
        {
            // The reported failure: Shift initially held while stationary, then turn 90 degrees.
            foreach(var facing in new[]{Vector2.up,Vector2.down,Vector2.left,Vector2.right})
            {
                Vector2 direction=new(-facing.y,facing.x);StartFlight(facing);Step(Vector2.zero);UntilLaps(direction,3);
                Check(g.Secret.Laps==3,"Shift parado e virada perpendicular contam tres voltas: "+facing+" -> "+direction);
                UntilLaps(direction,10);
                Check(g.Secret.Cinematic&&g.progress.worldHornDiscovered,"Decima volta dispara revelacao pelo mesmo TickWorld do jogo: "+direction);
                Check(!h.gameObject.activeSelf&&h.velocity==Vector2.zero,"Revelacao remove heroi e zera impulso: "+direction);
                g.TickWorld(7.3f);Check(g.Secret.InArena&&g.Secret.Boss&&g.Secret.Boss.EnemyName=="Bonero Companheiro","Sequencia entra automaticamente na arena: "+direction);
            }
            foreach(float dt in new[]{1/24f,1/60f,1/144f})
            {
                foreach(var direction in new[]{new Vector2(1,1),new Vector2(-1,1),new Vector2(-1,-1),new Vector2(1,-1)})
                {
                    StartFlight(Vector2.up);Step(Vector2.zero,dt);UntilLaps(direction,10,dt);
                    Check(g.Secret.Cinematic,"Diagonal chega a dez voltas em passo "+dt+" direcao "+direction);
                }
            }
            StartFlight(Vector2.up);Step(Vector2.zero);UntilLaps(Vector2.right,1);int first=g.Secret.Laps;UntilLaps(Vector2.up,3);
            Check(first==1&&g.Secret.Laps==3,"Trocar eixo durante a orbita conserva progresso e continua contando");
            StartFlight(Vector2.right);Step(Vector2.right);var b=g.world.bounds;
            for(int k=0;k<100;k++){g.Secret.MoveOrbit(new Vector2(b.xMax-.01f,-10),new Vector2(.02f,0));g.Secret.MoveOrbit(new Vector2(b.xMin+.01f,-10),new Vector2(-.02f,0));}
            Check(g.Secret.Laps==0&&g.Secret.LapProgress<.01f,"Oscilar e fazer wrap nas bordas nao fabrica distancia de volta");
            float age=g.Secret.OrbitAge,progress=g.Secret.LapProgress;Vector2 position=h.Position;g.mode=2;g.TickWorld(5);
            Check(h.Position==position&&g.Secret.LapProgress==progress&&g.Secret.OrbitAge==age,"Pausa nao move nem conta voltas");g.mode=1;
            g.DriveHero(Vector2.zero,false,false,1/60f);
            Check(!g.Secret.Active&&g.world.CanStand(h.Position)&&g.Secret.Laps==0,"Soltar Shift cancela e recupera posicao segura");
            StartFlight(Vector2.right);Step(Vector2.right);g.Secret.MoveOrbit(h.Position,new Vector2(b.width*2.4f,0));
            Check(g.Secret.Laps==2&&Mathf.Abs(g.Secret.LapProgress-.4f)<.002f,"Passo que cruza mais de uma largura nao perde voltas nem sobra fracionaria");
            StartFlight(Vector2.right);Step(Vector2.right);float previous=0;var samples=new List<string>{"volta;alvo_u_s;velocidade_real_u_s"};
            for(int lap=1;lap<=9;lap++)
            {
                UntilLaps(Vector2.right,lap);float speed=h.velocity.magnitude;
                Check(g.Secret.Laps==lap&&speed>previous+1,"Cada volta acelera de verdade: volta "+lap+", "+speed.ToString("F2")+" u/s");
                samples.Add(lap+";"+g.Secret.Speed.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+";"+speed.ToString("F3",System.Globalization.CultureInfo.InvariantCulture));previous=speed;
            }
            File.WriteAllLines(Path.Combine(folder,"Velocidade16.csv"),samples);
            Check(h.velocity.magnitude>280&&g.Secret.Speed>300,"Nona volta supera280u/s reais e300u/s de alvo");
            float top=g.Secret.Speed;g.Secret.MoveOrbit(h.Position,Vector2.left*3);Check(g.Secret.Speed>=top,"Breve correcao de direcao nao derruba velocidade acumulada");
            Check(g.Secret.TemporalIntensity>.7f&&g.lens.GetComponent<RpgTemporalFlight16>().Available,"Dilatacao temporal cresce com velocidade e shader e suportado");
            g.Secret.CameraStep(1);Check(g.lens.orthographicSize>12,"Camera abre progressivamente para acompanhar velocidade extrema");
            g.DriveHero(Vector2.zero,true,true,.016f);Check(!g.Secret.Active&&g.world.CanStand(h.Position)&&g.Secret.TemporalIntensity==0,"Ctrl encerra intangibilidade e distorcao temporal");
            StartFlight(Vector2.right);Step(Vector2.right);h.Land();Check(!g.Secret.Active&&!h.flying&&g.world.CanStand(h.Position),"Pouso encerra orbita e recupera chao");
            foreach(string action in new[]{"idle","walk","slash","spellcast","hurt"})
            {
                int expected=action=="idle"?4:action=="walk"||action=="spellcast"?8:6;var t=RpgArt.Texture("BoneroCompanheiro16_"+action);
                Check(t&&t.width==expected*256&&t.height==1024,"Nova fusao dividida possui atlas completo: "+action);
            }
            g.Secret.BeginRevelation();g.TickWorld(7.3f);
            Check(g.Secret.Boss.Family=="BoneroCompanheiro16"&&RpgArt.CellSize(g.Secret.Boss.Family)==256,"Arena carrega familia nova e tamanho/pivo correto");
        }
        void Native(string name)
        {
            var cam=g.lens;var prior=cam.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1280,720,24);var image=new Texture2D(1280,720,TextureFormat.RGB24,false);
            try{cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());}
            finally{cam.targetTexture=prior;RenderTexture.active=active;Destroy(rt);Destroy(image);}
        }
        public IEnumerator Run(string path)
        {
            folder=path;g=GetComponent<RpgGame>();bool had=PlayerPrefs.HasKey(RpgGame.SaveKey);string old=PlayerPrefs.GetString(RpgGame.SaveKey,"");
            try{Tests();}catch(Exception ex){Check(false,"Excecao: "+ex);Debug.LogException(ex);}
            StartFlight(Vector2.up);Step(Vector2.zero);UntilLaps(Vector2.right,6);var fx=g.lens.GetComponent<RpgTemporalFlight16>();
            for(int frame=0;frame<36;frame++)
            {
                for(int k=0;k<2;k++)Step(Vector2.right,1/120f);g.Secret.CameraStep(1/30f);h.Pose("flight",2+frame%2);
                Native("Temporal16_"+frame.ToString("D2"));yield return null;
            }
            Check(fx.RenderedFrames>=36,"Shader temporal renderizou36quadros nativos com historico");
            UntilLaps(Vector2.right,10);Check(g.Secret.Cinematic,"Captura de voo tambem alcanca revelacao automaticamente");g.TickWorld(7.3f);g.Secret.Advance(2);g.lens.transform.position=new Vector3(0,0,-10);g.lens.orthographicSize=12.3f;Native("Arena16");
            g.Secret.Boss.direction=2;g.Secret.Boss.Pose("idle",0);g.lens.orthographicSize=3.5f;g.lens.transform.position=new Vector3(0,5,-10);Native("Fusao16");
            Check(had==PlayerPrefs.HasKey(RpgGame.SaveKey)&&old==PlayerPrefs.GetString(RpgGame.SaveKey,""),"Save real intacto apos regressao e renders16");
            Debug.Log("CIBIO_16_REVIEW_"+(Failures==0?"OK":"FAIL")+" checks="+checks.Count+" failures="+Failures);
        }
    }
}
