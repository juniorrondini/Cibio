using UnityEngine;
namespace Cibio.RPG
{
 public sealed class RpgCache14:MonoBehaviour
 {
  public int id;public SpriteRenderer image;bool? open;
  public bool Open=>RpgGame.Instance&&(RpgGame.Instance.progress.islandCaches&(1<<id))!=0;
  void LateUpdate(){Refresh();}
  public void Refresh(){bool value=Open;if(open==value)return;open=value;image.sprite=RpgArt.Image(value?"Cache14_open":"Cache14_closed");image.sortingOrder=RpgWorld.Order(transform.position.y);}
  public bool Interact()
  {
   var g=RpgGame.Instance;var h=g.world.hero;if(Vector2.Distance(h.Position,transform.position)>1.9f)return false;
   if(Open){g.Notify("Este esconderijo já foi recolhido.");return true;}if(h.flying||h.flightHeight>.15f||h.jumpTimer>0){g.Notify("Pouse para abrir o esconderijo.");return true;}
   foreach(var e in g.world.enemies)if(!e.dead&&Vector2.Distance(e.Position,transform.position)<3.3f){g.Notify("Afaste os inimigos próximos para abrir o esconderijo.");return true;}
   g.progress.islandCaches|=1<<id;g.GainExperience(45);h.health=Mathf.Min(h.MaxHealth,h.health+20);h.energy=Mathf.Min(100,h.energy+20);Refresh();g.Save();g.Notify("ESCONDERIJO ENCONTRADO • +45 XP • vida e energia recuperadas");RpgAudio.Play("item_gem_01",h.Position,.55f);RpgEffects.Burst(transform.position,RpgArt.Hex("F9D995"),1.3f,.4f);return true;
  }
 }
}
