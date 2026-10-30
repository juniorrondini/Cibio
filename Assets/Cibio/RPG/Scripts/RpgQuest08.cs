using System.Linq;
using UnityEngine;
namespace Cibio.RPG
{
    public sealed class RpgQuest08:MonoBehaviour
    {
        RpgGame game;public Transform giver;public RpgResonator08[] nodes;
        void Awake(){game=GetComponent<RpgGame>();FindObjects();}
        void FindObjects()
        {
            if(!giver){var o=GameObject.Find("Seu Bonero • missão")??GameObject.Find("Seu Davi • missão");if(o)giver=o.transform;}
            nodes=Object.FindObjectsByType<RpgResonator08>(FindObjectsSortMode.None);
        }
        // Saved identifiers stay compatible: silenced bits now represent recovered evidence.
        public int SilencedCount=>RpgStory18.Count(game.progress);
        public string Objective=>RpgStory18.Objective(game.progress);
        public void RefreshState(){FindObjects();foreach(var e in game.world.enemies)if(e.enemyKind==4&&game.progress.captainDefeated){e.dead=true;e.deadTime=40;e.health=0;e.body.color=Color.clear;}}
        public bool TryInteract()
        {
            var h=game.world.hero;
            if(giver&&Vector2.Distance(h.Position,giver.position)<2.1f)
            {
                if(h.flightHeight>.15f){game.Notify("Pouse para conversar com Seu Bonero.");return true;}
                giver.GetComponent<RpgNpcDavi11>()?.Speak();
                if(game.progress.questStage==0){game.progress.questStage=1;game.Notify(RpgStory18.Accept);game.Save();}
                else if(game.progress.questStage==2)
                {
                    game.progress.questStage=3;game.progress.perks|=3;game.GainExperience(100);h.health=h.MaxHealth;h.energy=100;
                    game.Notify("DOSSIÊ ENTREGUE • esquema exposto • +100 XP • voo, aura de 24s e Palma [3]. [N] Rever as provas.");
                    RpgEffects.Burst(h.Position,RpgArt.Hex("FF617A"),3.5f,.8f);RpgAudio.Play("item_gem_01",h.Position,.65f);game.Save();
                }
                else game.Notify(game.progress.questStage==3?RpgStory18.Delivered:RpgStory18.Reminder);
                return true;
            }
            foreach(var n in nodes)if(Vector2.Distance(h.Position,n.transform.position)<2.1f)
            {
                if(game.progress.questStage==0){game.Notify("Converse com Seu Bonero, ao sul da praça, para iniciar a investigação.");return true;}
                if(RpgStory18.Has(game.progress,n.id)){game.Notify("Esta prova já está no dossiê. Pressione N para consultá-la.");return true;}
                if(h.flightHeight>.15f){game.Notify("Pouse para recolher a prova [E].");return true;}
                if(game.world.enemies.Any(e=>!e.dead&&Vector2.Distance(e.Position,n.transform.position)<4)){game.Notify("Afaste ou derrote os agentes próximos para investigar este posto.");return true;}
                game.progress.silenced|=1<<n.id;game.Notify(RpgStory18.Pickup[n.id]);RpgEffects.Burst(n.transform.position,RpgArt.Hex("7DE7C7"),1.7f,.55f);
                RpgAudio.Play("charge_release",n.transform.position,.5f);UpdateCompletion();game.Save();return true;
            }
            return false;
        }
        public void CaptainDefeated(){game.progress.captainDefeated=true;UpdateCompletion();}
        public void UpdateCompletion()
        {
            if(game.progress.questStage==1&&game.progress.silenced==7&&game.progress.captainDefeated)
            {
                game.progress.questStage=2;game.Notify("O ESQUEMA FOI EXPOSTO • entregue o dossiê a Seu Bonero, ao sul da praça.");
            }
        }
    }
}
