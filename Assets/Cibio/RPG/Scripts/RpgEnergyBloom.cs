using UnityEngine;
namespace Cibio.RPG
{
 [RequireComponent(typeof(Camera))]
 public sealed class RpgEnergyBloom:MonoBehaviour
 {
  Camera lens,maskCamera;Material bloom;GameObject helper;
  public bool Available=>bloom&&bloom.shader.isSupported;
  void Awake()
  {
   lens=GetComponent<Camera>();var shader=Resources.Load<Shader>("RpgArt/EnergyBloom06");if(shader&&shader.isSupported)bloom=new Material(shader);
   helper=new GameObject("Câmera • luz das energias"){hideFlags=HideFlags.HideAndDontSave};maskCamera=helper.AddComponent<Camera>();maskCamera.enabled=false;
  }
  void OnRenderImage(RenderTexture source,RenderTexture destination)
  {
   if(!Available||!RpgGame.Instance||!RpgGame.Instance.Playing){Graphics.Blit(source,destination);return;}
   int w=Mathf.Max(64,source.width/4),h=Mathf.Max(36,source.height/4);var a=RenderTexture.GetTemporary(w,h,16,RenderTextureFormat.ARGB32);var b=RenderTexture.GetTemporary(w,h,0,RenderTextureFormat.ARGB32);a.filterMode=b.filterMode=FilterMode.Bilinear;
   try
   {
    maskCamera.CopyFrom(lens);maskCamera.enabled=false;maskCamera.transform.SetPositionAndRotation(lens.transform.position,lens.transform.rotation);maskCamera.cullingMask=1<<RpgEnergyParticles.Layer;maskCamera.clearFlags=CameraClearFlags.SolidColor;maskCamera.backgroundColor=Color.clear;maskCamera.targetTexture=a;maskCamera.allowHDR=false;maskCamera.allowMSAA=false;maskCamera.Render();
    for(int i=0;i<2;i++){bloom.SetVector("_Direction",new Vector4(1.3f+i,0,0,0));Graphics.Blit(a,b,bloom,0);bloom.SetVector("_Direction",new Vector4(0,1.3f+i,0,0));Graphics.Blit(b,a,bloom,0);}
    bloom.SetTexture("_GlowTex",a);bloom.SetFloat("_Strength",.30f);Graphics.Blit(source,destination,bloom,1);
   }
   finally{maskCamera.targetTexture=null;RenderTexture.ReleaseTemporary(a);RenderTexture.ReleaseTemporary(b);}
  }
  void OnDestroy(){if(bloom)Destroy(bloom);if(helper)Destroy(helper);}
 }
}

