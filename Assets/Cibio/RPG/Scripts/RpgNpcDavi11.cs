using UnityEngine;
namespace Cibio.RPG
{
 public sealed class RpgNpcDavi11:MonoBehaviour
 {
  public SpriteRenderer body;public int direction=2;public bool reviewPose;float clock,talkClock,talking;
  public bool Talking=>talking>0;public int CurrentFrame{get;private set;}static readonly Sprite[] frames=new Sprite[32];
  public static Sprite Frame(int direction,int frame)
  {
   direction=Mathf.Clamp(direction,0,3);frame=Mathf.Clamp(frame,0,7);int key=direction*8+frame;if(!frames[key])frames[key]=Sprite.Create(RpgArt.Texture("Davi11"),new Rect(frame*128,(3-direction)*128,128,128),new Vector2(.5f,.16f),64,0,SpriteMeshType.FullRect);return frames[key];
  }
  void Awake(){if(!body)body=GetComponentInChildren<SpriteRenderer>();Pose(direction,0);}
  public void ResetNpc(){clock=talkClock=talking=0;reviewPose=false;Pose(2,0);}
  public void Speak(){talking=4.5f;talkClock=0;}
  public void Pose(int view,int frame){direction=view;CurrentFrame=frame;if(body){body.sprite=Frame(view,frame);body.color=Color.white;body.sortingOrder=RpgWorld.Order(transform.position.y);}}
  void Update()
  {
   var g=RpgGame.Instance;if(reviewPose||!g||!g.Playing)return;clock+=Time.deltaTime;talkClock+=Time.deltaTime;talking=Mathf.Max(0,talking-Time.deltaTime);Vector2 to=g.world.hero.Position-(Vector2)transform.position;
   if(to.sqrMagnitude<16&&to.sqrMagnitude>.16f)direction=Mathf.Abs(to.x)>Mathf.Abs(to.y)?(to.x>0?3:1):(to.y>0?0:2);else if(!Talking)direction=2;
   Pose(direction,Talking?4+(int)(talkClock*4)%4:(int)(clock*2)%4);
  }
 }
}
