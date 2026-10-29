using UnityEngine;
namespace Cibio.RPG
{
 public sealed class RpgParticleClock:MonoBehaviour
 {
  ParticleSystem particles;bool playing=true;
  void Awake(){particles=GetComponent<ParticleSystem>();}
  void Update(){bool next=RpgGame.Instance&&RpgGame.Instance.Playing;if(next==playing)return;playing=next;if(next)particles.Play();else particles.Pause();}
 }
}
