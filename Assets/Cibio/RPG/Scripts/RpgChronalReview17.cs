using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Cibio.RPG
{
    public sealed class RpgChronalReview17:MonoBehaviour
    {
        RpgGame g;RpgActor h;string folder;readonly List<string> checks=new();public int Failures{get;private set;}
        void Check(bool ok,string label){if(!ok)Failures++;checks.Add((ok?"PASS ":"FAIL ")+label);File.WriteAllLines(Path.Combine(folder,"Ruptura17.txt"),checks);}
        void StartFlight()
        {
            g.StartGame(false);g.reviewPose=true;h=g.world.hero;g.progress.perks=7;g.progress.questStage=3;
            h.transform.position=new Vector2(0,-10);h.facing=Vector2.right;h.buff=16;h.formStage=2;h.flying=true;h.flightHeight=2.2f;Step();
        }
        void Step(float dt=1/120f){g.DriveHero(Vector2.right,true,false,dt);g.TickWorld(dt);}
        void UntilTier(float tier){for(int n=0;n<4000&&g.Secret.Active&&g.Secret.FlightTier<tier;n++)Step();}
        void Tests()
        {
            StartFlight();UntilTier(4.3f);var c=g.Chronal;
            Check(g.Secret.Laps==4&&!c.Active&&c.Strength==0&&c.Ruptures==0,"Tunel e ruptura permanecem ausentes antes da quinta volta");
            UntilTier(4.7f);Check(c.Preparations==1&&c.Ruptures==0,"Succao reversa antecipa a quinta volta uma unica vez");
            UntilTier(5);Check(g.Secret.Laps==5&&c.Active&&c.Ruptures==1&&c.Impulse>.9f,"Quinta volta dispara impacto exatamente no cruzamento real");
            Check(c.Strength>=.66f&&c.BackgroundGain<.55f&&c.BedVolume>0&&c.AirVolume>0,"Entrada tem tunel forte, dois leitos de audio e ducking de fundo");
            float age=c.Age;g.mode=2;c.Tick(2);Check(c.Age==age&&c.BedVolume==0&&c.AirVolume==0,"Pausa congela a ruptura e silencia loops temporais");g.mode=1;
            for(int k=0;k<8;k++)Step();Check(c.BedVolume>0&&c.Age>age,"Retomar recupera leitos sem repetir impacto");Check(c.Ruptures==1,"Uma unica entrada durante a quinta volta");
            UntilTier(6);Check(c.Ruptures==2&&c.Impulse>.9f,"Sexta volta renova o impacto sem reiniciar toda a sequencia");
            UntilTier(9.1f);Check(c.Ruptures==5&&c.Strength==1,"Passagens5a9 tem cinco rupturas e intensidade maxima");
            UntilTier(10);Check(g.Secret.Cinematic&&c.Ruptures==6,"Decima passagem usa ruptura final e mantem revelacao do mundo");g.TickWorld(.3f);
            Check(!c.Active&&c.Strength==0&&c.BedVolume==0&&c.AirVolume==0,"Revelacao desliga tunel e leitos sem encobrir o mundo-vuvuzela");
            g.TickWorld(7);Check(g.Secret.InArena&&g.Secret.Boss.Family=="BoneroCompanheiro16","Arena e chefe dividido continuam acessiveis apos efeitos17");
            StartFlight();UntilTier(5.3f);g.DriveHero(Vector2.zero,true,true,.016f);g.TickWorld(.4f);
            Check(!c.Active&&c.Age==0&&c.Impulse==0&&c.BedVolume==0&&c.AirVolume==0&&g.world.CanStand(h.Position),"Ctrl cancela efeitos, faz cauda sonora e recupera chao seguro");
            g.StartGame(false);g.reviewPose=true;Check(c.Ruptures==0&&c.Preparations==0&&c.BedVolume==0,"Nova sessao limpa contadores e loops sem mudar save");
            foreach(string name in new[]{"chronal_entry17","chronal_lap17","chronal_rise17","chronal_loop17","chronal_air17","chronal_exit17","chronal_reveal17"})
            {
                var clip=RpgAudio.Clip(name);Check(clip&&clip.length>.5f&&clip.channels==2,"Efeito estereo de biblioteca/DSP importado: "+name);
                if(clip){float[] data=new float[clip.samples*clip.channels];bool read=clip.GetData(data,0);float peak=0;double squares=0;foreach(float a in data){peak=Mathf.Max(peak,Mathf.Abs(a));squares+=a*a;}Check(read&&peak>.03f&&peak<.98f&&Math.Sqrt(squares/data.Length)>.015,"PCM importado tem sinal e headroom: "+name+" peak="+peak.ToString("F3"));}
            }
            bool previous=RpgAudio.Instance.suppressOneShots;RpgAudio.Instance.suppressOneShots=false;RpgAudio.Play("chronal_entry17",Vector2.zero,.95f);RpgAudio.Instance.suppressOneShots=previous;
            var sources=UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);bool routed=false;foreach(var s in sources)if(s.gameObject.name=="Som • chronal_entry17"){routed=s.spatialBlend==0&&s.priority==20&&s.clip==RpgAudio.Clip("chronal_entry17");DestroyImmediate(s.gameObject);}
            Check(routed,"Impacto temporal usa rota2D dedicada e prioridade alta no Unity");
            var art=RpgArt.Texture("Vfx_ChronalRift17");Check(art&&art.width==1024&&art.height==1024,"Atlas organico temporal16quadros integrado");
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
            StartFlight();UntilTier(4.55f);var fx=g.lens.GetComponent<RpgTemporalFlight16>();int before=fx.StormFrames;var timeline=new List<string>{"frame;lap;progress;impulse"};
            for(int frame=0;frame<72;frame++)
            {
                Step();Step();g.Secret.CameraStep(1/60f);h.Pose("flight",2+frame%2);Native("Ruptura17_"+frame.ToString("D2"));timeline.Add(frame+";"+g.Secret.Laps+";"+g.Secret.LapProgress+";"+g.Chronal.Impulse);yield return null;
            }
            File.WriteAllLines(Path.Combine(folder,"Timeline17.csv"),timeline);Check(fx.StormFrames-before>20&&g.Chronal.Ruptures>=1,"72renders nativos atravessam a quinta volta e registram tunel real");
            fx.enabled=false;Native("SemDistorcao17");fx.enabled=true;Native("ComDistorcao17");
            // A short real-time mixer probe; unavailable devices produce no publishable preview.
            var capture=g.lens.gameObject.AddComponent<RpgAudioCapture>();bool suppressed=RpgAudio.Instance.suppressOneShots;RpgAudio.Instance.suppressOneShots=false;
            RpgAudio.Play("chronal_entry17",h.Position,.95f);float until=Time.realtimeSinceStartup+2.5f;
            while(Time.realtimeSinceStartup<until){g.Chronal.Tick(Time.unscaledDeltaTime);yield return null;}
            RpgAudio.Instance.suppressOneShots=suppressed;string probe="Unity mixer peak="+capture.peak+"; physical audition not performed.\n";
            if(capture.peak>.001){capture.Save(Path.Combine(folder,"Ruptura17-MixagemUnity.wav"));Check(capture.peak<.985,"Mixagem Unity capturada abaixo do teto do limiter");}
            else probe+="No audio samples/device available; use separately labelled offline mix preview.\n";
            File.WriteAllText(Path.Combine(folder,"Audio-Probe17.txt"),probe);Destroy(capture);
            Check(had==PlayerPrefs.HasKey(RpgGame.SaveKey)&&old==PlayerPrefs.GetString(RpgGame.SaveKey,""),"Save real preservado em testes, audio e capturas17");
            Debug.Log("CIBIO_17_REVIEW_"+(Failures==0?"OK":"FAIL")+" checks="+checks.Count+" failures="+Failures);
        }
    }
}
