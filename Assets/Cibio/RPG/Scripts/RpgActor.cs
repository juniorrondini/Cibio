using UnityEngine;
namespace Cibio.RPG
{
 public sealed class RpgActor:MonoBehaviour
 {
  public RpgWorld world;public bool isEnemy;public SpriteRenderer body,shadow;public int enemyKind;
  public Vector2 facing=Vector2.down,velocity;public int direction=2;public float health=100,energy=100,charge,buff,attackTimer,jumpTimer,invincible,transforming,powerPose;
  public bool charging,dead;public int hitsLanded,comboIndex;public float clock,enemyCooldown,deadTime;bool hitApplied,comboQueued;[SerializeField]Vector2 home;
  public bool flying,running,flightBoosting,flightBraking;public float flightHeight;public int formStage;Vector2 steering;float gaitTravel,flightTrail;public float attackContact;
  public int FormStage=>buff>0?(formStage>=2?2:1):0;
  public Color EnergyColor=>RpgArt.Hex(FormStage==2?"FF576B":"FFE1A0");
  public float FlightSpeed=>(FormStage==2?12:FormStage==1?8:6)*(flightBoosting&&!flightBraking?1.25f:1);
  public float GroundSpeed(bool run)=>(run?4.5f:2.8f)*(FormStage==2?1.45f:FormStage==1?1.3f:1);
  public float GaitTravel=>gaitTravel;
  public Color BaseTint=>isEnemy?(enemyKind==3?new Color(.72f,.9f,.85f):enemyKind==4?new Color(1,.66f,.7f):Color.white):Color.white;
  public Vector2 Home=>home;
  public void SetHome(Vector2 p){home=p;}
  public void SetFlightControls(bool boost,bool brake){flightBraking=flying&&brake;flightBoosting=flying&&boost&&!brake;}
float gaitPhase,poseClock;string lastPose="";
  public int animationFrame;public float AttackProgress=>attackDuration>0?1-attackTimer/attackDuration:0;
  public void ResetMotion(){flying=flightBoosting=flightBraking=false;formStage=0;steering=Vector2.zero;flightHeight=gaitPhase=poseClock=gaitTravel=0;lastPose="";running=false;comboQueued=hitApplied=false;lastAttack=clock-2;comboIndex=0;}
  public bool ToggleFlight()
  {
   if(flying){Land();return true;}if(dead||charging||transforming>0||powerPose>0||attackTimer>0||jumpTimer>0||flightHeight>0||energy<12)return false;
   energy-=12;flying=true;steering=Vector2.zero;flightBoosting=flightBraking=false;velocity=Vector2.zero;RpgAudio.Play("flight_launch",Position,.65f);RpgEffects.Burst(Position,RpgArt.Hex("A1F2DC"),1.6f,.45f);return true;
  }
  public void Land(){if(!flying)return;flying=flightBoosting=flightBraking=false;RpgAudio.Play("flight_land",Position,.4f);}
  float castWindup;float attackDuration=.53f,lastAttack=-20;public string attackAction="slash",currentAnimation="idle";
  public Vector2 Position=>(Vector2)transform.position;
  public int MaxHealth=>isEnemy?(enemyKind==4?280:enemyKind==3?78:enemyKind==1?120:enemyKind==2?55:65):100+((RpgGame.Instance?RpgGame.Instance.progress.level:1)-1)*12;
  public float Damage=>isEnemy?(enemyKind==4?26:enemyKind==3?15:enemyKind==1?22:12):(20+(RpgGame.Instance.progress.level-1)*3)*(FormStage==2?3.15f:FormStage==1?2.3f:1);
  public string EnemyName=>enemyKind==4?"Capitão do Eco":enemyKind==3?"Batedor do Morro":enemyKind==1?"Fiscal blindado":enemyKind==2?"Arauto da Vuvuzela":"Cobrador";
  public string Family=>isEnemy?(enemyKind==1||enemyKind==4?"EnemyHeavy":enemyKind==2?"EnemyCaster":"Enemy"):buff>0?"HeroAwakened":charging&&charge>=55?"HeroLift":"Hero";
  public void Initialize(){home=Position;health=MaxHealth;shadow=RpgArt.Put("Sombra",RpgArt.Pixel,Position+Vector2.down*.04f,-3900,transform,new Color(.04f,.08f,.07f,.3f));shadow.transform.localScale=new Vector3(.72f,.12f,1);body=RpgArt.Put("Sprite animado",RpgArt.Frame(Family,"idle",2,0),Position,RpgWorld.Order(Position.y),transform);}
  public void ConfigureEnemy(int kind){enemyKind=kind;gameObject.name=EnemyName;health=MaxHealth;transform.localScale=Vector3.one*(kind==4?1.32f:kind==3?.98f:kind==1?1.12f:kind==2?.96f:1);body.color=BaseTint;Pose("idle",0);}
  public void Tick(float dt)
  {
   clock+=dt;invincible=Mathf.Max(0,invincible-dt);buff=Mathf.Max(0,buff-dt);transforming=buff>0?Mathf.Max(0,transforming-dt):0;jumpTimer=Mathf.Max(0,jumpTimer-dt);powerPose=Mathf.Max(0,powerPose-dt);castWindup=Mathf.Max(0,castWindup-dt);
   if(dead){deadTime+=dt;Pose("hurt",Mathf.Min(5,(int)(deadTime*9)));body.color=new Color(1,1,1,Mathf.Clamp01(4-deadTime));if(isEnemy&&enemyKind!=4&&deadTime>40)ResetEnemy();return;}
   if(flying){energy=Mathf.Max(0,energy-dt*((FormStage==2?9:FormStage==1?7:10)+(flightBoosting?6:0)));if(energy<=0)Land();}
   flightHeight=Mathf.MoveTowards(flightHeight,flying?(FormStage==2?2.2f:FormStage==1?1.9f:1.55f):0,dt*2.8f);
   if(flying){flightTrail+=dt;if(flightTrail>.06f&&velocity.sqrMagnitude>9){flightTrail=0;RpgEffects.Spark(Position+Vector2.up*flightHeight-velocity.normalized*.4f,EnergyColor,FormStage==2?1.4f:.7f);}}
   if(isEnemy)EnemyBrain(dt);
   if(attackTimer>0){attackTimer-=dt;if(AttackProgress>=attackContact&&!hitApplied){hitApplied=true;DealStrike();}if(attackTimer<=0&&comboQueued){comboQueued=false;Attack();}}
   if(charging){charge=Mathf.Min(100,charge+dt*31);energy=Mathf.Min(100,energy+dt*24);if(Random.value<dt*18)RpgEffects.Spark(Position+Vector2.up*.7f,RpgArt.Hex("FFD46E"),.6f+charge*.014f);}
   else if(!flying&&flightHeight<=0)energy=Mathf.Min(100,energy+dt*5);
   if(buff>0&&Random.value<dt*10)RpgEffects.Spark(Position+Vector2.up*(.6f+flightHeight),EnergyColor,FormStage==2?2.2f:1.7f);
   string anim=charging||transforming>0||powerPose>0?"spellcast":isEnemy&&eliteTell>0?"spellcast":attackTimer>0?attackAction:flightHeight>.02f?"flight":jumpTimer>0?"jump":velocity.sqrMagnitude>.05f?(running?"run":"walk"):"idle";
   if(anim!=lastPose){poseClock=0;if(anim=="walk"||anim=="run"){if(lastPose!="walk"&&lastPose!="run")gaitPhase=0;}lastPose=anim;}else poseClock+=dt;
   int count=RpgArt.FrameCount(Family,anim);
   int frame=anim=="walk"?1+(int)(gaitPhase*8)%8:anim=="run"?(int)(gaitPhase*8)%8:anim=="flight"?(velocity.sqrMagnitude>.25f?2:0)+(int)(poseClock*4)%2:anim=="jump"?Mathf.Clamp((int)((.65f-jumpTimer)/.65f*count),0,count-1):attackTimer>0&&anim==attackAction?Mathf.Clamp((int)(AttackProgress*count),0,count-1):anim=="spellcast"?(int)(poseClock*9)%7:(int)(poseClock*2)%2;
   if(charging)frame=charge<20?1:charge<50?2:charge<75?3:4;
   if(powerPose>0)frame=castWindup>0?2:5;
   if(transforming>0){float age=1.15f-transforming;frame=FormStage==2?new[]{2,3,4,5,5,4,3,2}[Mathf.Clamp((int)(age/1.15f*8),0,7)]:age<.24f?2:age<.66f?5:2;}
   Pose(anim,frame);body.color=transforming<=0&&invincible>0&&((int)(clock*18)%2==0)?new Color(1,.65f,.63f):isEnemy?(eliteTell>0?new Color(1,.52f,.4f):enemyKind==3?new Color(.72f,.9f,.85f):enemyKind==4?new Color(1,.66f,.7f):Color.white):Color.white;
  }
  public void Pose(string animation,int frame){currentAnimation=animation;animationFrame=frame;body.sprite=RpgArt.Frame(transforming>0&&FormStage<2?"HeroLift":Family,animation,direction,frame);body.sortingOrder=RpgWorld.Order(Position.y);body.transform.localPosition=new Vector3(0,flightHeight+(flightHeight>.02f?Mathf.Sin(clock*4)*.035f:0)+(jumpTimer>0?Mathf.Sin(jumpTimer/.65f*Mathf.PI)*.75f:0),0);shadow.transform.localPosition=new Vector3(0,-.04f,0);}
  public void Walk(Vector2 input,float speed,float dt,bool run=false)
  {
   running=run;input=Vector2.ClampMagnitude(input,1);bool locked=attackTimer>0||charging||transforming>0||powerPose>0||dead;if(locked)input=Vector2.zero;if(!locked&&!flying&&input.sqrMagnitude>.01f){facing=input.normalized;direction=Mathf.Abs(input.x)>Mathf.Abs(input.y)?(input.x>0?3:1):(input.y>0?0:2);}
   if(flying){if(flightBraking)input=Vector2.zero;velocity=Vector2.SmoothDamp(velocity,input*speed,ref steering,flightBraking?.045f:.11f,Mathf.Infinity,dt);if(!locked&&velocity.sqrMagnitude>.25f){facing=velocity.normalized;direction=Mathf.Abs(facing.x)>Mathf.Abs(facing.y)?(facing.x>0?3:1):(facing.y>0?0:2);}}
   else velocity=Vector2.MoveTowards(velocity,input*speed,dt*(input.sqrMagnitude>.01f?24:30));if(locked){velocity=steering=Vector2.zero;}Vector2 before=Position;transform.position=world.Move(Position,velocity*dt);float travel=Vector2.Distance(before,Position);gaitPhase+=travel/(running?2.9f:2.25f);gaitTravel+=travel;
   if(travel<.00001f&&input.sqrMagnitude>.01f)velocity=Vector2.zero;
  }
  public bool Attack()
  {
   if(dead||charging||transforming>0||powerPose>0)return false;if(attackTimer>0){if(!isEnemy&&attackTimer<.24f)comboQueued=true;return false;}
   direction=Mathf.Abs(facing.x)>Mathf.Abs(facing.y)?(facing.x>0?3:1):(facing.y>0?0:2);comboIndex=isEnemy?0:clock-lastAttack<1.3f?(comboIndex+1)%3:0;lastAttack=clock;attackAction=isEnemy&&enemyKind==2?"spellcast":comboIndex==1?"thrust":comboIndex==2?"backslash":"slash";
   attackDuration=isEnemy?(enemyKind==1?.85f:enemyKind==2?.9f:.6f):comboIndex==0?.48f:comboIndex==1?.54f:.64f;attackTimer=attackDuration;attackContact=isEnemy?.55f:FormStage>0?(comboIndex==0?.36f:comboIndex==1?.52f:.54f):(comboIndex==0?.68f:comboIndex==1?.64f:.77f);hitApplied=false;velocity=Vector2.zero;
   RpgAudio.Play(isEnemy&&enemyKind==2?"spell_02":"punch_swing"+comboIndex,Position,isEnemy?.24f:.42f,isEnemy&&enemyKind==1?.78f:1);return true;
  }
  public void Cast(float duration,float windup=0){powerPose=duration;castWindup=windup;velocity=Vector2.zero;}
  public void ResetEnemy(){transform.position=home;health=MaxHealth;dead=false;deadTime=0;velocity=Vector2.zero;attackTimer=jumpTimer=invincible=transforming=powerPose=0;charging=false;charge=buff=0;enemyCooldown=1.2f;eliteTell=eliteRush=0;rushHit=false;direction=2;facing=Vector2.down;body.color=BaseTint;Pose("idle",0);}
  public bool Jump(){if(flying){Land();return true;}if(flightHeight>0||jumpTimer>0||attackTimer>0||powerPose>0||charging||dead||transforming>0)return false;jumpTimer=.65f;RpgAudio.Play("cloth1",Position,.25f,1.1f);return true;}
  void DealStrike()
  {
   if(isEnemy&&enemyKind==2){RpgProjectile.Fire(this,facing);hitsLanded++;return;}
   Vector2 center=Position+facing*(comboIndex==1?.85f:.65f);RpgEffects.Strike(center+Vector2.up*flightHeight,facing,comboIndex,isEnemy?RpgArt.Hex("EBA095"):RpgArt.Hex("FFE4A3"));
   if(isEnemy){var h=world.hero;if(h.flightHeight<1&&Vector2.Distance(Position,h.Position)<(enemyKind==1?1.55f:1.3f)&&!h.dead){float before=h.health;h.TakeDamage(Damage);if(h.health<before)RpgAudio.Punch(false,enemyKind==1?2:0,h.Position);hitsLanded++;}}
   else {if(buff>0){RpgEffects.PowerStrike(center+Vector2.up*flightHeight,facing,comboIndex,EnergyColor);if(comboIndex==2)RpgAudio.Play("power_surge",Position,.3f);}foreach(var e in world.enemies)if(!e.dead&&Vector2.Distance(center,e.Position)<(comboIndex==2?1.45f:1.1f)){float before=e.health;e.TakeDamage(Damage*(comboIndex==2?1.55f:comboIndex==1?1.12f:1));if(e.health<before){RpgAudio.Punch(buff>0,comboIndex,e.Position);RpgGame.Instance.shake=Mathf.Max(RpgGame.Instance.shake,buff>0?.65f:.25f);RpgGame.Instance.impactPause=buff>0?.065f:.035f;e.transform.position=world.Move(e.Position,facing*(buff>0?.85f:.24f));}hitsLanded++;}}
  }
  public void TakeDamage(float amount)
  {
   if(dead||invincible>0)return;health=Mathf.Max(0,health-amount);invincible=isEnemy?.16f:.8f;RpgGame.Instance.FloatText(Position+Vector2.up*1.7f,"−"+Mathf.RoundToInt(amount),isEnemy?RpgArt.Hex("FFE39C"):RpgArt.Hex("FF8A84"));RpgAudio.Play("creature_hurt_01",Position,.12f,Random.Range(.92f,1.06f));
   if(health<=0){flying=false;flightHeight=0;dead=true;deadTime=0;velocity=Vector2.zero;charging=false;charge=buff=transforming=0;if(isEnemy){RpgGame.Instance.EnemyDefeated(this);RpgAudio.Play("creature_die_01",Position,.35f);}else RpgGame.Instance.Notify("Você caiu. Pressione [R] para voltar à praça.");}
  }
  float eliteTell,eliteRush;bool rushHit;Vector2 eliteAim;public bool WindingUp=>eliteTell>0;
  void EliteBrain(RpgActor target,Vector2 delta,float distance,float dt)
  {
   if(Vector2.Distance(Position,home)>9){Walk((home-Position).normalized,2.5f,dt);return;}
   if(eliteRush>0){float step=Mathf.Min(dt,eliteRush);eliteRush-=dt;Vector2 a=Position,b=world.Move(a,eliteAim*11*step);transform.position=b;gaitPhase+=Vector2.Distance(a,b)/2.9f;gaitTravel+=Vector2.Distance(a,b);velocity=eliteAim*11;running=true;Vector2 d=b-a;float u=d.sqrMagnitude>.0001f?Mathf.Clamp01(Vector2.Dot(target.Position-a,d)/d.sqrMagnitude):0;if(!rushHit&&target.flightHeight<1&&Vector2.Distance(target.Position,a+d*u)<1.05f){target.TakeDamage(Damage);rushHit=true;}return;}
   running=false;if(eliteTell>0){velocity=Vector2.zero;eliteTell-=dt;if(eliteTell<=0){if(enemyKind==4){for(int k=-1;k<=1;k++)RpgProjectile.Fire(this,Quaternion.Euler(0,0,k*19)*eliteAim);}else {eliteRush=.24f;rushHit=false;RpgEffects.Burst(Position,RpgArt.Hex("DA927F"),1.1f,.3f);}enemyCooldown=enemyKind==4?2.8f:2.1f;}return;}
   if(distance>7){Walk(Vector2.zero,0,dt);return;}
   if(enemyCooldown<=0&&distance<(enemyKind==4?6.5f:4.4f)){eliteAim=delta.normalized;eliteTell=enemyKind==4?.85f:.6f;facing=eliteAim;direction=Mathf.Abs(facing.x)>Mathf.Abs(facing.y)?(facing.x>0?3:1):(facing.y>0?0:2);RpgEffects.Spark(Position+Vector2.up, RpgArt.Hex(enemyKind==4?"FF697E":"FAC97C"),2);RpgAudio.Play("wave_windup",Position,.2f);return;}
   if(distance<1.2f){Walk(Vector2.zero,0,dt);if(enemyCooldown<=0){facing=delta.normalized;Attack();enemyCooldown=2;}}
   else{Vector2 toward=delta.normalized;Vector2 orbit=new(-toward.y,toward.x);Walk(enemyKind==3&&distance<3?toward*.45f+orbit*.65f:toward,enemyKind==3?2.6f:1.6f,dt);}
  }
  void EnemyBrain(float dt)
  {
   var target=world.hero;enemyCooldown-=dt;if(target.dead){Walk(Vector2.zero,0,dt);return;}Vector2 delta=target.Position-Position;float distance=delta.magnitude;
   if(enemyKind>=3){EliteBrain(target,delta,distance,dt);return;}
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

