using UnityEngine;
namespace Cibio.RPG
{
 public sealed class RpgActor:MonoBehaviour
 {
  public RpgWorld world;public bool isEnemy;public SpriteRenderer body,shadow;public int enemyKind;
  public Vector2 facing=Vector2.down,velocity;public int direction=2;public float health=100,energy=100,charge,buff,attackTimer,jumpTimer,invincible,transforming,powerPose;
  public bool charging,dead;public int hitsLanded,comboIndex;public float clock,enemyCooldown,deadTime;bool hitApplied,comboQueued;[SerializeField]Vector2 home;
  public bool flying,running;public float flightHeight;float gaitPhase,poseClock;string lastPose="";
  public int animationFrame;public float AttackProgress=>attackDuration>0?1-attackTimer/attackDuration:0;
  public void ResetMotion(){flying=false;flightHeight=gaitPhase=poseClock=0;lastPose="";running=false;comboQueued=hitApplied=false;lastAttack=clock-2;comboIndex=0;}
  public bool ToggleFlight()
  {
   if(flying){Land();return true;}if(dead||charging||transforming>0||powerPose>0||attackTimer>0||jumpTimer>0||flightHeight>0||energy<12)return false;
   energy-=12;flying=true;velocity=Vector2.zero;RpgAudio.Play("flight_launch",Position,.65f);RpgEffects.Burst(Position,RpgArt.Hex("A1F2DC"),1.6f,.45f);return true;
  }
  public void Land(){if(!flying)return;flying=false;RpgAudio.Play("flight_land",Position,.4f);}
  float attackDuration=.53f,lastAttack=-20;public string attackAction="slash",currentAnimation="idle";
  public Vector2 Position=>(Vector2)transform.position;
  public int MaxHealth=>isEnemy?(enemyKind==1?120:enemyKind==2?55:65):100+((RpgGame.Instance?RpgGame.Instance.progress.level:1)-1)*12;
  public float Damage=>isEnemy?(enemyKind==1?22:12):(20+(RpgGame.Instance.progress.level-1)*3)*(buff>0?2.3f:1);
  public string EnemyName=>enemyKind==1?"Fiscal blindado":enemyKind==2?"Arauto da Vuvuzela":"Cobrador";
  public string Family=>isEnemy?(enemyKind==1?"EnemyHeavy":enemyKind==2?"EnemyCaster":"Enemy"):buff>0?"HeroAwakened":charging&&charge>=55?"HeroLift":"Hero";
  public void Initialize(){home=Position;health=MaxHealth;shadow=RpgArt.Put("Sombra",RpgArt.Pixel,Position+Vector2.down*.04f,-3900,transform,new Color(.04f,.08f,.07f,.3f));shadow.transform.localScale=new Vector3(.72f,.12f,1);body=RpgArt.Put("Sprite animado",RpgArt.Frame(Family,"idle",2,0),Position,RpgWorld.Order(Position.y),transform);}
  public void ConfigureEnemy(int kind){enemyKind=kind;gameObject.name=EnemyName;health=MaxHealth;transform.localScale=Vector3.one*(kind==1?1.12f:kind==2?.96f:1);Pose("idle",0);}
  public void Tick(float dt)
  {
   clock+=dt;invincible=Mathf.Max(0,invincible-dt);buff=Mathf.Max(0,buff-dt);transforming=buff>0?Mathf.Max(0,transforming-dt):0;jumpTimer=Mathf.Max(0,jumpTimer-dt);powerPose=Mathf.Max(0,powerPose-dt);
   if(dead){deadTime+=dt;Pose("hurt",Mathf.Min(5,(int)(deadTime*9)));body.color=new Color(1,1,1,Mathf.Clamp01(4-deadTime));if(isEnemy&&deadTime>25)ResetEnemy();return;}
   if(flying){energy=Mathf.Max(0,energy-dt*(buff>0?7:10));if(energy<=0)Land();}
   flightHeight=Mathf.MoveTowards(flightHeight,flying?(buff>0?1.9f:1.55f):0,dt*2.8f);
   if(isEnemy)EnemyBrain(dt);
   if(attackTimer>0){attackTimer-=dt;if(AttackProgress>=(isEnemy?.55f:buff>0?(comboIndex==0?.36f:comboIndex==1?.52f:.54f):(comboIndex==0?.68f:comboIndex==1?.64f:.77f))&&!hitApplied){hitApplied=true;DealStrike();}if(attackTimer<=0&&comboQueued){comboQueued=false;Attack();}}
   if(charging){charge=Mathf.Min(100,charge+dt*31);energy=Mathf.Min(100,energy+dt*24);if(Random.value<dt*18)RpgEffects.Spark(Position+Vector2.up*.7f,RpgArt.Hex("FFD46E"),.6f+charge*.014f);}
   else if(!flying&&flightHeight<=0)energy=Mathf.Min(100,energy+dt*5);
   if(buff>0&&Random.value<dt*10)RpgEffects.Spark(Position+Vector2.up*.6f,RpgArt.Hex("FFE18B"),1.7f);
   string anim=charging||transforming>0||powerPose>0?"spellcast":attackTimer>0?attackAction:flightHeight>.02f?"flight":jumpTimer>0?"jump":velocity.sqrMagnitude>.05f?(running?"run":"walk"):"idle";
   if(anim!=lastPose){poseClock=0;if(anim=="walk"||anim=="run")gaitPhase=0;lastPose=anim;}else poseClock+=dt;
   int count=RpgArt.FrameCount(Family,anim);
   int frame=anim=="walk"?1+(int)(gaitPhase*8)%8:anim=="run"?(int)(gaitPhase*8)%8:anim=="flight"?(velocity.sqrMagnitude>.25f?2:0)+(int)(poseClock*4)%2:anim=="jump"?Mathf.Clamp((int)((.65f-jumpTimer)/.65f*count),0,count-1):attackTimer>0&&anim==attackAction?Mathf.Clamp((int)(AttackProgress*count),0,count-1):anim=="spellcast"?(int)(poseClock*9)%7:(int)(poseClock*2)%2;
   if(transforming>0){float age=1.15f-transforming;frame=age<.24f?2:age<.66f?5:2;}
   Pose(anim,frame);body.color=transforming<=0&&invincible>0&&((int)(clock*18)%2==0)?new Color(1,.65f,.63f):Color.white;
  }
  public void Pose(string animation,int frame){currentAnimation=animation;animationFrame=frame;body.sprite=RpgArt.Frame(transforming>0?"HeroLift":Family,animation,direction,frame);body.sortingOrder=RpgWorld.Order(Position.y);body.transform.localPosition=new Vector3(0,flightHeight+(flightHeight>.02f?Mathf.Sin(clock*4)*.035f:0)+(jumpTimer>0?Mathf.Sin(jumpTimer/.65f*Mathf.PI)*.75f:0),0);shadow.transform.localPosition=new Vector3(0,-.04f,0);}
  public void Walk(Vector2 input,float speed,float dt,bool run=false)
  {
   running=run;input=Vector2.ClampMagnitude(input,1);bool locked=attackTimer>0||charging||transforming>0||powerPose>0||dead;if(locked)input=Vector2.zero;if(!locked&&input.sqrMagnitude>.01f){facing=input.normalized;direction=Mathf.Abs(input.x)>Mathf.Abs(input.y)?(input.x>0?3:1):(input.y>0?0:2);}
   velocity=Vector2.MoveTowards(velocity,input*speed,dt*(input.sqrMagnitude>.01f?24:30));if(locked)velocity=Vector2.zero;Vector2 before=Position;transform.position=world.Move(Position,velocity*dt);float travel=Vector2.Distance(before,Position);gaitPhase+=travel/(running?2.9f:2.25f);
   if(travel<.00001f&&input.sqrMagnitude>.01f)velocity=Vector2.zero;
  }
  public bool Attack()
  {
   if(dead||charging||transforming>0||powerPose>0)return false;if(attackTimer>0){if(!isEnemy&&attackTimer<.24f)comboQueued=true;return false;}
   direction=Mathf.Abs(facing.x)>Mathf.Abs(facing.y)?(facing.x>0?3:1):(facing.y>0?0:2);comboIndex=isEnemy?0:clock-lastAttack<1.3f?(comboIndex+1)%3:0;lastAttack=clock;attackAction=isEnemy&&enemyKind==2?"spellcast":comboIndex==1?"thrust":comboIndex==2?"backslash":"slash";
   attackDuration=isEnemy?(enemyKind==1?.85f:enemyKind==2?.9f:.6f):comboIndex==0?.48f:comboIndex==1?.54f:.64f;attackTimer=attackDuration;hitApplied=false;velocity=Vector2.zero;
   RpgAudio.Play(isEnemy&&enemyKind==2?"spell_02":"punch_swing"+comboIndex,Position,isEnemy?.24f:.42f,isEnemy&&enemyKind==1?.78f:1);return true;
  }
  public void Cast(float duration){powerPose=duration;velocity=Vector2.zero;}
  public void ResetEnemy(){transform.position=home;health=MaxHealth;dead=false;deadTime=0;velocity=Vector2.zero;attackTimer=jumpTimer=invincible=transforming=powerPose=0;charging=false;charge=buff=0;enemyCooldown=1.2f;direction=2;facing=Vector2.down;body.color=Color.white;Pose("idle",0);}
  public bool Jump(){if(flying){Land();return true;}if(flightHeight>0||jumpTimer>0||attackTimer>0||powerPose>0||charging||dead||transforming>0)return false;jumpTimer=.65f;RpgAudio.Play("cloth1",Position,.25f,1.1f);return true;}
  void DealStrike()
  {
   if(isEnemy&&enemyKind==2){RpgProjectile.Fire(this,facing);hitsLanded++;return;}
   Vector2 center=Position+facing*(comboIndex==1?.85f:.65f);RpgEffects.Strike(center+Vector2.up*flightHeight,facing,comboIndex,isEnemy?RpgArt.Hex("EBA095"):RpgArt.Hex("FFE4A3"));
   if(isEnemy){var h=world.hero;if(h.flightHeight<1&&Vector2.Distance(Position,h.Position)<(enemyKind==1?1.55f:1.3f)&&!h.dead){float before=h.health;h.TakeDamage(Damage);if(h.health<before)RpgAudio.Punch(false,enemyKind==1?2:0,h.Position);hitsLanded++;}}
   else {if(buff>0){RpgEffects.PowerStrike(center+Vector2.up*flightHeight,facing,comboIndex);if(comboIndex==2)RpgAudio.Play("power_surge",Position,.3f);}foreach(var e in world.enemies)if(!e.dead&&Vector2.Distance(center,e.Position)<(comboIndex==2?1.45f:1.1f)){float before=e.health;e.TakeDamage(Damage*(comboIndex==2?1.55f:comboIndex==1?1.12f:1));if(e.health<before){RpgAudio.Punch(buff>0,comboIndex,e.Position);RpgGame.Instance.shake=Mathf.Max(RpgGame.Instance.shake,buff>0?.65f:.25f);RpgGame.Instance.impactPause=buff>0?.065f:.035f;e.transform.position=world.Move(e.Position,facing*(buff>0?.85f:.24f));}hitsLanded++;}}
  }
  public void TakeDamage(float amount)
  {
   if(dead||invincible>0)return;health=Mathf.Max(0,health-amount);invincible=isEnemy?.16f:.8f;RpgGame.Instance.FloatText(Position+Vector2.up*1.7f,"−"+Mathf.RoundToInt(amount),isEnemy?RpgArt.Hex("FFE39C"):RpgArt.Hex("FF8A84"));RpgAudio.Play("creature_hurt_01",Position,.12f,Random.Range(.92f,1.06f));
   if(health<=0){flying=false;flightHeight=0;dead=true;deadTime=0;velocity=Vector2.zero;charging=false;charge=buff=transforming=0;if(isEnemy){RpgGame.Instance.EnemyDefeated();RpgAudio.Play("creature_die_01",Position,.35f);}else RpgGame.Instance.Notify("Você caiu. Pressione [R] para voltar à praça.");}
  }
  void EnemyBrain(float dt)
  {
   var target=world.hero;enemyCooldown-=dt;if(target.dead){Walk(Vector2.zero,0,dt);return;}Vector2 delta=target.Position-Position;float distance=delta.magnitude;
   if(enemyKind==2&&distance<6)
   {
    if(attackTimer>0){Walk(Vector2.zero,0,dt);return;}facing=delta.normalized;direction=Mathf.Abs(delta.x)>Mathf.Abs(delta.y)?(delta.x>0?3:1):(delta.y>0?0:2);
    if(distance<2.5f)Walk(-delta.normalized,1.55f,dt);else Walk(Vector2.zero,0,dt);
    if(enemyCooldown<=0&&distance<5.7f){facing=delta.normalized;Attack();enemyCooldown=2.6f;}return;
   }
   float reach=enemyKind==1?1.3f:1.05f;
   if(distance<reach){Walk(Vector2.zero,0,dt);if(enemyCooldown<=0){facing=delta.normalized;Attack();enemyCooldown=enemyKind==1?2.6f:1.9f;}}
   else if(distance<5.2f){Vector2 move=delta.normalized;if(!world.CanStand(Position+move*.65f)){Vector2 side=new(-move.y,move.x);if(!world.CanStand(Position+side*.65f))side=-side;move=side;}Walk(move,enemyKind==1?1.2f:1.75f,dt);}
   else Walk(Vector2.zero,0,dt);
  }
 }
}

