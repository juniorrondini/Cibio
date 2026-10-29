using UnityEngine;
namespace Cibio.RPG
{
 public sealed class RpgFeedback09:MonoBehaviour
 {
  float flash;public void Flash(float amount){flash=Mathf.Max(flash,amount);}public void Transformation(){flash=.08f;}
  void Update(){if(RpgGame.Instance&&RpgGame.Instance.Playing)flash=Mathf.MoveTowards(flash,0,Time.deltaTime*.9f);}
  void OnGUI()
  {
   var g=RpgGame.Instance;if(!g||!g.Playing)return;GUI.depth=5;var h=g.world.hero;float shade=h.FormStage==2&&h.transforming>0?Mathf.Sin(Mathf.Clamp01(1-h.transforming/1.7f)*Mathf.PI)*.30f:0;
   var old=GUI.color;if(shade>0){GUI.color=new Color(.01f,.025f,.05f,shade);GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);}if(flash>0){GUI.color=new Color(.82f,.94f,1,flash);GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);}GUI.color=old;
  }
 }
}
