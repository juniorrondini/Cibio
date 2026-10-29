using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Cibio.RPG
{
 public sealed class RpgAudioCapture:MonoBehaviour
 {
  readonly List<float> samples=new();readonly object sync=new();int channels=2;public double peak;
  void OnAudioFilterRead(float[] data,int channelCount){lock(sync){channels=channelCount;if(samples.Count>AudioSettings.outputSampleRate*channels*45)return;samples.AddRange(data);foreach(float value in data)peak=Math.Max(peak,Math.Abs(value));}}
  public void Save(string path)
  {
   lock(sync){using var writer=new BinaryWriter(File.Create(path));int bytes=samples.Count*2,rate=AudioSettings.outputSampleRate;writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+bytes);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)channels);writer.Write(rate);writer.Write(rate*channels*2);writer.Write((short)(channels*2));writer.Write((short)16);writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(bytes);foreach(float value in samples)writer.Write((short)(Mathf.Clamp(value,-1,1)*32767));}
  }
 }
}
