using System.Linq;
using UnityEngine;
namespace Cibio.RPG
{
 public sealed class RpgQuest08:MonoBehaviour
 {
  RpgGame game;public Transform giver;public RpgResonator08[] nodes;
  void Awake(){game=GetComponent<RpgGame>();FindObjects();}
  void FindObjects(){if(!giver){var o=GameObject.Find("Seu Davi • missão");if(o)giver=o.transform;}nodes=Object.FindObjectsByType<RpgResonator08>(FindObjectsSortMode.None);}
  public int SilencedCount=>Count(game.progress.silenced);
  static int Count(int bits){int n=0;for(int i=0;i<3;i++)if((bits&(1<<i))!=0)n++;return n;}
  public string Objective=>game.progress.questStage==0?"Fale com Seu Davi [E], ao sul da praça.":game.progress.questStage==3?"Morro liberto! Carregue C na forma dourada para ativar a Rubra. [3] Palma.":game.progress.questStage==2?"Volte a Seu Davi para receber a recompensa.":"Morro do Eco • norte da vila\nAmplificadores: "+SilencedCount+"/3  •  Capitão: "+(game.progress.captainDefeated?"vencido":"enfrente no largo norte");
  public void RefreshState(){FindObjects();foreach(var e in game.world.enemies)if(e.enemyKind==4&&game.progress.captainDefeated){e.dead=true;e.deadTime=40;e.health=0;e.body.color=Color.clear;}}
  public bool TryInteract()
  {
   var h=game.world.hero;
   if(giver&&Vector2.Distance(h.Position,giver.position)<2.1f)
   {
    if(h.flightHeight>.15f){game.Notify("Pouse para conversar com Seu Davi.");return true;}
    if(game.progress.questStage==0){game.progress.questStage=1;game.Notify("Seu Davi: silencie os 3 amplificadores no Morro do Eco e vença o capitão. Siga ao norte!");game.Save();}
    else if(game.progress.questStage==2){game.progress.questStage=3;game.progress.perks|=3;game.progress.xp+=100;while(game.progress.xp>=game.progress.NextLevel){game.progress.xp-=game.progress.NextLevel;game.progress.level++;}h.health=h.MaxHealth;h.energy=100;game.Notify("MISSÃO CONCLUÍDA • Centelha Rubra + Palma [3] • +100 XP! Carregue C na forma dourada e solte.");RpgEffects.Burst(h.Position,RpgArt.Hex("FF617A"),3.5f,.8f);RpgAudio.Play("item_gem_01",h.Position,.65f);game.Save();}
    else game.Notify(game.progress.questStage==3?"Seu Davi: a voz da vila voltou! Na forma dourada, complete C outra vez para despertar o estágio 2.":"Seu Davi: o caminho fica ao norte. Desligue cada amplificador com [E] depois de afastar os guardas.");
    return true;
   }
   foreach(var n in nodes)if(Vector2.Distance(h.Position,n.transform.position)<2.1f)
   {
    if(game.progress.questStage==0){game.Notify("Seu Davi, ao sul da praça, sabe como desligar o amplificador.");return true;}
    if((game.progress.silenced&(1<<n.id))!=0){game.Notify("Este amplificador já está silencioso.");return true;}
    if(h.flightHeight>.15f){game.Notify("Pouse para desligar o amplificador [E].");return true;}
    if(game.world.enemies.Any(e=>!e.dead&&Vector2.Distance(e.Position,n.transform.position)<4)){game.Notify("Afaste ou derrote os guardas próximos antes de desligar o amplificador.");return true;}
    game.progress.silenced|=1<<n.id;game.Notify("AMPLIFICADOR SILENCIADO • "+SilencedCount+"/3");RpgEffects.Burst(n.transform.position,RpgArt.Hex("7DE7C7"),1.7f,.55f);RpgAudio.Play("charge_release",n.transform.position,.5f);UpdateCompletion();game.Save();return true;
   }
   return false;
  }
  public void CaptainDefeated(){game.progress.captainDefeated=true;UpdateCompletion();}
  public void UpdateCompletion(){if(game.progress.questStage==1&&game.progress.silenced==7&&game.progress.captainDefeated){game.progress.questStage=2;game.Notify("O MORRO RESPIRA DE NOVO • volte a Seu Davi, ao sul da praça.");}}
 }
}
