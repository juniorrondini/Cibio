using System;using UnityEngine;
namespace Cibio.RPG
{
 // Listener insert: transparent below the knee, bounded peaks above it.
 public sealed class RpgAudioLimiter:MonoBehaviour
 {
  public float inputPeak;
  void OnAudioFilterRead(float[] data,int channels)
  {
   for(int i=0;i<data.Length;i++){float x=data[i],a=Math.Abs(x);if(a>inputPeak)inputPeak=a;if(a>.8f)data[i]=Math.Sign(x)*(.8f+.18f*(float)Math.Tanh((a-.8f)/.18f));}
  }
 }
}
