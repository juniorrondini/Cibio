using UnityEngine;

namespace Cibio.RPG
{
    public sealed partial class RpgInterface
    {
        void TalentEntry()
        {
            var talents=game.GetComponent<RpgTalents15>();int points=RpgTalents15.Available(game.progress);Rect r=new(24,248,250,36);
            Card(r,.9f);Text(new Rect(r.x+12,r.y+9,218,20),"T  Talentos"+(points>0?"    +"+points+" disponível"+(points>1?"s":""):""),12,points>0?gold:muted,points>0);if(GUI.Button(r,"",GUIStyle.none))talents.Toggle();
            if(talents.CounterReady)Text(new Rect(26,292,265,20),"CONTRATEMPO • próximo soco +75%",11,blue,true);
        }
        void TalentIcon(Rect r,int id,float alpha)
        {
            var t=RpgArt.Texture("UI_Talents15");if(!t)return;var c=GUI.color;GUI.color=new Color(1,1,1,alpha);GUI.DrawTextureWithTexCoords(r,t,new Rect(id%3/3f,(2-id/3)/3f,1/3f,1/3f));GUI.color=c;
        }
        void TalentPanel()
        {
            Fill(new Rect(0,0,1440,810),new Color(.018f,.024f,.032f,.94f));Rect panel=new(170,52,1100,706);Card(panel,.98f);
            var controller=game.GetComponent<RpgTalents15>();var p=game.progress;int points=RpgTalents15.Available(p);
            Text(new Rect(202,78,500,38),"Desenvolva sua resistência",28,paper,true);Text(new Rect(203,120,650,22),"Um ponto por nível. Combine estilos e desbloqueie o talento final de cada linha.",13,muted);
            Text(new Rect(925,82,307,33),points+" ponto"+(points==1?"":"s")+" disponível"+(points==1?"":"s"),21,gold,true,TextAnchor.MiddleRight);
            string[] branches={"IMPACTO","CENTELHA","VANGUARDA"};Color[] colors={gold,blue,teal};
            for(int branch=0;branch<3;branch++)
            {
                float x=202+branch*352;Text(new Rect(x,161,326,21),branches[branch],12,colors[branch],true);Fill(new Rect(x,189,326,1),line);
                for(int tier=0;tier<3;tier++)
                {
                    int id=branch*3+tier,rank=RpgTalents15.Rank(p,id),max=RpgTalents15.MaxRank(id);bool unlocked=RpgTalents15.Prerequisite(p,id),available=unlocked&&points>0&&rank<max;Rect r=new(x,203+tier*148,326,137);Card(r,rank>0?.98f:.7f);
                    TalentIcon(new Rect(r.x+12,r.y+12,52,52),id,unlocked?1:.3f);Text(new Rect(r.x+76,r.y+13,231,25),RpgTalents15.Names[id],15,unlocked?paper:muted,true);Text(new Rect(r.x+77,r.y+43,158,18),tier==2?(rank>0?"DOMÍNIO DESPERTO":"TALENTO FINAL"):"Grau "+rank+" / "+max,10,rank>0?colors[branch]:muted,true);
                    Text(new Rect(r.x+13,r.y+73,300,tier==2?50:39),RpgTalents15.Descriptions[id],tier==2?11:12,unlocked?muted:new Color(.45f,.49f,.52f));
                    if(available){if(Button(new Rect(r.x+242,r.y+42,69,24),"+ 1",true))controller.Invest(id);}
                    else if(!unlocked)Text(new Rect(r.x+12,r.y+119,305,15),tier==1?"Requer 2 pontos no talento anterior":"Requer grau 3 no primeiro e 2 no segundo",9,new Color(.58f,.56f,.51f));
                }
            }
            bool before=GUI.enabled;GUI.enabled=controller.CanRespec&&p.talentRanks!=0;if(Button(new Rect(202,687,232,38),"Reorganizar pontos"))controller.Respec();GUI.enabled=before;
            Text(new Rect(455,693,490,30),controller.CanRespec?"Reorganização gratuita em segurança.":"Afaste os inimigos e saia da arena para reorganizar.",11,muted);
            if(Button(new Rect(1034,687,202,38),"Voltar  T / Esc",true))game.mode=1;
        }
        void SecretHud()
        {
            var s=game.GetComponent<RpgWorldSecret15>();if(s.Active)
            {
                Card(new Rect(474,93,492,87));Text(new Rect(492,102,456,24),"ÓRBITA DO MUNDO   "+s.Laps+" / 10",17,blue,true,TextAnchor.MiddleCenter);
                Text(new Rect(492,130,456,18),(s.Laps>=5?"RUPTURA TEMPORAL  •  ":"")+game.world.hero.velocity.magnitude.ToString("F0")+" u/s",12,paper,true,TextAnchor.MiddleCenter);
                Bar(new Rect(495,156,450,3),s.LapProgress,blue);Text(new Rect(492,163,456,14),"Shift mantém o impulso • Ctrl interrompe",10,muted,false,TextAnchor.MiddleCenter);
            }
            if(!s.InArena)return;
            if(s.IntroRemaining>0){Fill(new Rect(0,0,1440,72),new Color(.01f,.02f,.03f,.94f));Fill(new Rect(0,716,1440,94),new Color(.01f,.02f,.03f,.94f));Text(new Rect(280,723,880,42),"BONERO COMPANHEIRO",28,gold,true,TextAnchor.MiddleCenter);Text(new Rect(280,767,880,22),"Dois rostos. A mesma máquina.",13,muted,false,TextAnchor.MiddleCenter);}
            else if(!s.Victory){var b=s.Boss.GetComponent<RpgFusionBoss15>();Text(new Rect(475,158,490,26),b.MoveName,12,b.Enraged?gold:muted,true,TextAnchor.MiddleCenter);}
            if(s.Victory){Card(new Rect(416,166,608,82));Text(new Rect(437,178,566,28),"AS CONTAS FORAM PRESTADAS",21,gold,true,TextAnchor.MiddleCenter);Text(new Rect(437,215,566,20),"E  Retornar à vila     •     T  Desenvolver talentos",12,paper,false,TextAnchor.MiddleCenter);}
        }
        void SecretQuest(Rect r)
        {
            var s=game.GetComponent<RpgWorldSecret15>();Card(r,.90f);Text(new Rect(r.x+14,r.y+12,r.width-28,25),"A décima volta",15,gold,true);Text(new Rect(r.x+14,r.y+43,r.width-28,64),s.Victory?"A convergência foi vencida. O farol mantém a passagem aberta.":"Derrote Bonero Companheiro. Desvie dos sopros e aproveite a recuperação entre ataques.",12,muted);
        }
        void SecretMap(Rect r)
        {
            Card(r,.9f);Text(new Rect(r.x+12,r.y+10,r.width-24,22),"Boca do Mundo",12,gold,true);Rect m=new(r.x+10,r.y+35,r.width-20,r.height-45);GUI.DrawTexture(m,RpgArt.Texture("Arena_Vuvuzela15"),ScaleMode.ScaleAndCrop);
            var h=game.world.hero;float x=m.center.x+h.Position.x/48*m.width,y=m.center.y-h.Position.y/36*m.height;Round(new Rect(x-3,y-3,6,6),paper,3);
        }
        void WorldCinematic()
        {
            var s=game.GetComponent<RpgWorldSecret15>();float age=s.CinemaAge;Fill(new Rect(0,0,1440,810),new Color(.01f,.03f,.05f));
            if(age<1.2f&&s.IslandSnapshot){float zoom=Mathf.Lerp(1.2f,.83f,age/1.2f);GUI.DrawTexture(new Rect(720-720*zoom,405-405*zoom,1440*zoom,810*zoom),s.IslandSnapshot,ScaleMode.ScaleToFit);}
            var atlas=RpgArt.Texture("WorldFold15");float f=Mathf.Clamp((age-.7f)/.63f,0,7);int first=Mathf.FloorToInt(f),second=Mathf.Min(first+1,7);float blend=Mathf.SmoothStep(0,1,Mathf.Repeat(f,1));
            if(age>=.7f&&age<5.8f&&atlas){DrawFold(atlas,first,Mathf.Clamp01((age-.7f)/.3f));DrawFold(atlas,second,blend);}
            if(age>=5.15f){float alpha=Mathf.Clamp01((age-5.15f)/.5f);float zoom=Mathf.Lerp(1,1.85f,Mathf.SmoothStep(0,1,(age-5.8f)/1.4f));var c=GUI.color;GUI.color=new Color(1,1,1,alpha);GUI.DrawTexture(new Rect(1123-1123*zoom,162-162*zoom,1440*zoom,810*zoom),RpgArt.Texture("World_Vuvuzela15"),ScaleMode.ScaleAndCrop);GUI.color=c;}
            float flash=Mathf.Clamp01(1-Mathf.Abs(age-1.08f)/.16f)*.65f;Fill(new Rect(0,0,1440,810),new Color(.87f,.95f,1,flash));if(age>6.8f)Fill(new Rect(0,0,1440,810),new Color(.015f,.025f,.04f,Mathf.Clamp01((age-6.8f)/.4f)));
            Fill(new Rect(0,0,1440,72),new Color(.01f,.02f,.03f,.94f));Fill(new Rect(0,716,1440,94),new Color(.01f,.02f,.03f,.94f));
            Text(new Rect(170,727,1100,32),age<2.6f?"DEZ VOLTAS. UMA VERDADE IMPOSSÍVEL.":age<5.8f?"O BRASIL VIROU UMA VUVUZELA.":"ALGO RESPONDEU DO OUTRO LADO.",24,paper,true,TextAnchor.MiddleCenter);Text(new Rect(420,769,600,20),"A décima volta",12,gold,false,TextAnchor.MiddleCenter);
            if(game.mode==2)Pause();
        }
        void DrawFold(Texture2D atlas,int frame,float alpha){var c=GUI.color;GUI.color=new Color(1,1,1,alpha);GUI.DrawTextureWithTexCoords(new Rect(0,0,1440,810),atlas,new Rect(frame%2*.5f,(3-frame/2)*.25f,.5f,.25f));GUI.color=c;}
    }
}
