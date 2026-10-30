using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Cibio.RPG
{
    public sealed class RpgIdentityReview19:MonoBehaviour
    {
        readonly List<string> checks=new();public int Failures=>checks.Count(s=>s.StartsWith("FAIL"));
        void Check(bool ok,string label)=>checks.Add((ok?"PASS ":"FAIL ")+label);
        public IEnumerator Run(string folder)
        {
            var g=GetComponent<RpgGame>();var ui=GetComponent<RpgInterface>();
            bool had=PlayerPrefs.HasKey(RpgGame.SaveKey);string old=PlayerPrefs.GetString(RpgGame.SaveKey,"");
            try
            {
                foreach(var sample in new[]{"João","Ana Maria","D'Ávila","Zé-13","José123","Trabalhador 2026"})Check(RpgIdentity19.TryName(sample,out var value)&&value==sample,"Aceita nome: "+sample);
                foreach(var sample in new[]{""," ","A","--","<b>João</b>","João😀","ABC\u200BDEF",new string('A',19),"Nome/Arquivo","Nome@email"})Check(!RpgIdentity19.TryName(sample,out _),"Recusa nome invalido #"+checks.Count);
                Check(RpgIdentity19.TryName("  João   da  Silva  ",out var spaced)&&spaced=="João da Silva","Normaliza espacos sem perder acentos");
                Check(RpgIdentity19.TryName("Jose\u0301",out var accent)&&accent=="José","Normaliza acento Unicode decomposto");
                Check(RpgIdentity19.TryName(new string('A',18),out _),"Limite exato de 18 caracteres aceito");
                var legacy=JsonUtility.FromJson<RpgProgress>("{\"version\":3,\"level\":8,\"perks\":7,\"questStage\":3,\"silenced\":7,\"talentRanks\":3}");
                Check(RpgIdentity19.Display(legacy)=="Brasileiro"&&legacy.level==8&&legacy.questStage==3&&legacy.talentRanks==3,"Save antigo sem nome preserva progresso e nome padrao");
                legacy.playerName="Maria do Brasil";var restored=JsonUtility.FromJson<RpgProgress>(JsonUtility.ToJson(legacy));
                Check(RpgIdentity19.Display(restored)==legacy.playerName&&restored.perks==7&&restored.silenced==7,"Nome e progresso sobrevivem serializacao juntos");
                g.StartGame(false);g.reviewPose=true;g.progress.level=9;string current=JsonUtility.ToJson(g.progress);g.mode=0;
                Check(ui.BeginJourney(false)&&g.mode==6&&!g.Playing,"Jogar abre tela de nome e mantem mundo pausado");
                Check(current==JsonUtility.ToJson(g.progress),"Abrir novo jogo nao substitui progresso antes da confirmacao");
                ui.NameDraft="A";Check(!ui.ConfirmJourney()&&g.mode==6&&current==JsonUtility.ToJson(g.progress),"Nome invalido bloqueia inicio sem mudar progresso");
                Check(RpgInterface.PointerOverHudAt(new Vector2(700,400)),"Tela de nome bloqueia socos por clique");
                ui.CancelJourney();Check(g.mode==0&&current==JsonUtility.ToJson(g.progress),"Cancelar volta ao menu sem reiniciar partida");
                ui.BeginJourney(false);ui.NameDraft="  João   Brasileiro ";Check(ui.ConfirmJourney()&&g.mode==1&&g.progress.playerName=="João Brasileiro"&&g.progress.level==1,"Confirmacao inicia personagem novo com nome normalizado");
                Check(!ui.BeginJourney(false),"Criacao nao interrompe partida em andamento");
                Check(!ui.ConfirmJourney(),"Confirmacao repetida nao reinicia personagem");
                g.mode=0;Check(ui.BeginJourney(true)&&ui.JourneyContinues==had,"Continuar respeita existencia do save real");ui.CancelJourney();
                Check(had==PlayerPrefs.HasKey(RpgGame.SaveKey)&&old==PlayerPrefs.GetString(RpgGame.SaveKey,""),"Ensaios de criacao nunca alteram save real");
            }
            catch(Exception e){Check(false,"Excecao: "+e);Debug.LogException(e);}
            Directory.CreateDirectory(folder);File.WriteAllLines(Path.Combine(folder,"Identidade19.txt"),checks);
            Debug.Log("CIBIO_19_REVIEW_"+(Failures==0?"OK":"FAIL")+" checks="+checks.Count+" failures="+Failures);yield return null;
        }
    }
}
