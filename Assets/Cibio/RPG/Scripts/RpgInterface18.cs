using UnityEngine;

namespace Cibio.RPG
{
    public sealed partial class RpgInterface
    {
        public int DossierDraws{get;private set;}public bool DossierTextFits{get;private set;}
        public bool ToggleDossier()
        {
            if(game.Secret.Cinematic)return false;
            if(game.mode==5){game.mode=1;return true;}
            if(game.mode!=1)return false;help=false;game.world.hero.CancelQueuedAttack();game.Save();game.mode=5;return true;
        }
        void DossierEntry()
        {
            Rect r=new(24,317,250,32);Card(r,.86f);Text(new Rect(35,324,228,20),"N  Dossiê • Brasil no Vermelho",11,paper,true);
            if(GUI.Button(r,"",GUIStyle.none))ToggleDossier();
        }
        void DossierPanel()
        {
            DossierDraws++;DossierTextFits=Style(14,muted).CalcHeight(new GUIContent(RpgStory18.Premise),996)<=76;
            for(int id=0;id<3;id++)DossierTextFits&=Style(12,muted).CalcHeight(new GUIContent(RpgStory18.Findings[id]),290)<=82;
            Fill(new Rect(0,0,1440,810),new Color(.018f,.024f,.032f,.96f));Card(new Rect(188,53,1064,704),.98f);
            var p=game.progress;Text(new Rect(220,76,740,22),RpgStory18.Genre.ToUpper()+" • "+RpgStory18.Setting,11,muted,true);
            Text(new Rect(219,109,800,43),RpgStory18.Mission,29,paper,true);Text(new Rect(1010,116,207,27),RpgStory18.Count(p)+" / 3 provas",19,gold,true,TextAnchor.MiddleRight);
            Text(new Rect(221,161,996,76),RpgStory18.Premise,14,muted);
            Fill(new Rect(221,244,996,1),line);Text(new Rect(221,260,996,31),RpgStory18.Objective(p),15,paper,true);
            for(int id=0;id<3;id++)
            {
                bool found=RpgStory18.Has(p,id);Rect r=new(220+id*338,307,320,329);Card(r,found?1:.72f);
                var t=RpgArt.Texture(RpgStory18.Assets[id]);if(t)GUI.DrawTexture(new Rect(r.x+112,r.y+8,96,96),t,ScaleMode.ScaleToFit);
                Text(new Rect(r.x+15,r.y+110,290,18),RpgStory18.Themes[id],10,found?teal:gold,true);
                Text(new Rect(r.x+15,r.y+138,290,29),RpgStory18.Headlines[id],18,paper,true);
                Text(new Rect(r.x+15,r.y+176,290,21),RpgStory18.Stations[id],12,muted,true);
                Text(new Rect(r.x+15,r.y+213,290,82),found?RpgStory18.Findings[id]:"Investigue este posto no Distrito do Poder, ao norte. Afaste os agentes e recolha o registro com E.",12,muted);
                Text(new Rect(r.x+15,r.y+303,290,16),found?"PROVA RECOLHIDA":"AGUARDANDO INVESTIGAÇÃO",10,found?teal:gold,true);
            }
            Text(new Rect(221,659,682,42),p.captainDefeated?"Companheiro vencido. O registro das provas permanece disponível.":"Companheiro controla o distrito. Reúna os registros e enfrente-o na sede ao norte.",12,muted);
            if(Button(new Rect(1011,691,207,38),"Voltar  N / Esc",true))game.mode=1;
        }
        void EvidenceLabels18()
        {
            if(game.Secret.InArena)return;var q=game.GetComponent<RpgQuest08>();
            foreach(var node in q.nodes)
            {
                if(Vector2.Distance(game.world.hero.Position,node.transform.position)>5)continue;
                var at=Project((Vector2)node.transform.position+Vector2.up*2.85f);Rect r=new(at.x-106,at.y-25,212,23);
                Round(r,new Color(.025f,.04f,.05f,.82f),4);Text(new Rect(r.x+6,r.y+3,200,18),RpgStory18.Stations[node.id],11,RpgStory18.Has(game.progress,node.id)?teal:paper,true,TextAnchor.MiddleCenter);
            }
        }
    }
}
