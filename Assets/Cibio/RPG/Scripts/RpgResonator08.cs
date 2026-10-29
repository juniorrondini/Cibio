using UnityEngine;
namespace Cibio.RPG
{
 public sealed class RpgResonator08:MonoBehaviour
 {
  public int id;SpriteRenderer lamp;
  void Start(){lamp=RpgEffects.Create("Amplificador • energia orgânica",transform);lamp.transform.localPosition=Vector3.up*1.45f;lamp.transform.localScale=new Vector3(.9f,1.2f,1);lamp.sortingOrder=RpgWorld.Order(transform.position.y)+1;}
  void Update(){var g=RpgGame.Instance;if(!g||!lamp)return;bool on=(g.progress.silenced&(1<<id))==0;RpgEffects.Paint(lamp,"Vfx_Wisp06",Time.time*(on?11:4),RpgArt.Hex(on?"FF6479":"79E3BF"),on?1:.4f);lamp.enabled=g.Playing||g.mode==2;}
 }
}
