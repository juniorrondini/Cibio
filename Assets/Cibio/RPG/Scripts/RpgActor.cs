using UnityEngine;
namespace Cibio.RPG
{
 public sealed class RpgActor:MonoBehaviour
 {
  public RpgWorld world;public bool isEnemy;public SpriteRenderer body,shadow;public int enemyKind;
  public Vector2 facing=Vector2.down,velocity;public int direction=2;public float health=100,energy=100,charge,buff,attackTimer,jumpTimer,invincible,transforming,powerPose;
  public bool charging,dead;public int hitsLanded,comboIndex;public float clock,enemyCooldown,deadTime;bool hitApplied,comboQueued;[SerializeField]Vector2 home;
  public bool flying,running,flightBoosting,flightBraking;public float flightHeight;public int formStage;Vector2 steering;float gaitTravel,flightTrail;public float attackContact;
  public const float DodgeDuration=.2f,DodgeCost=10;public float dodgeTimer,dodgeCooldown,staggerTimer;float attackBuffer,dodgeTrail;Vector2 dodgeDirection;
  float strikeTalentMultiplier=1;
  public bool Dodging=>dodgeTimer>0;public bool QueuedCombo=>comboQueued||attackBuffer>0;
  public void CancelQueuedAttack(){comboQueued=false;attackBuffer=0;}
  public bool RequestAttack(){if(dead||charging||transforming>0||powerPose>0||Dodging)return false;if(attackTimer>0){attackBuffer=.18f;if(AttackProgress>=.25f){comboQueued=true;attackBuffer=0;}return true;}return Attack();}
  public bool TryDodge(Vector2 input){if(isEnemy||dead||charging||transforming>0||powerPose>0||flying||flightHeight>0||jumpTimer>0||Dodging||dodgeCooldown>0||energy<(RpgGame.Instance.Talents?.DodgeCost??DodgeCost)||attackTimer>0&&AttackProgress<.82f)return false;dodgeDirection=input.sqrMagnitude>.01f?input.normalized:facing.normalized;if(dodgeDirection.sqrMagnitude<.1f)dodgeDirection=Vector2.down;facing=dodgeDirection;direction=Mathf.Abs(facing.x)>Mathf.Abs(facing.y)?(facing.x>0?3:1):(facing.y>0?0:2);energy-=RpgGame.Instance.Talents?.DodgeCost??DodgeCost;dodgeTimer=DodgeDuration;dodgeCooldown=RpgGame.Instance.Talents?.DodgeCooldown??.85f;RpgGame.Instance.Talents?.OnDodge();invincible=Mathf.Max(invincible,.13f);attackTimer=0;CancelQueuedAttack();ResetSonic();RpgAudio.Play("cloth1",Position,.35f,1.2f);return true;}
  public void Stagger(float time,bool finisher){if(!isEnemy||dead)return;if(enemyKind>=4&&!finisher)return;float factor=enemyKind>=4?.25f:enemyKind==1?.55f:1;staggerTimer=Mathf.Max(staggerTimer,time*factor);if(enemyKind<4&&(enemyKind!=1||finisher)){attackTimer=powerPose=eliteTell=eliteRush=0;enemyCooldown=Mathf.Max(enemyCooldown,.55f);CancelQueuedAttack();}velocity=Vector2.zero;}
  public int FormStage=>buff>0?(formStage>=2?2:1):0;
  public Color EnergyColor=>RpgArt.Hex(FormStage==2?"A0D8FF":"FFE1A0");
  public float TransformationDuration=>FormStage==2?1.7f:1.15f;
  public const float SonicTopSpeed=60,SonicLaunchSpeed=42;
  public bool SonicActive{get;private set;}public float SonicDistance{get;private set;}public int sonicBooms;Vector2 sonicHeading;
  public void ResetSonic(){SonicActive=false;SonicDistance=0;sonicHeading=Vector2.zero;}
  void TravelSonic(Vector2 travel,float dt,bool locked)
  {
   if(FormStage!=2||!flying||!flightBoosting||flightBraking||locked||travel.magnitude<.015f){ResetSonic();return;}
   Vector2 heading=travel.normalized;if(sonicHeading.sqrMagnitude>.1f&&Vector2.Dot(sonicHeading,heading)<.45f){ResetSonic();}if(sonicHeading.sqrMagnitude<.1f)sonicHeading=heading;
   SonicDistance=Mathf.Min(18,SonicDistance+travel.magnitude);if(!SonicActive&&SonicDistance>=18){SonicActive=true;velocity=heading*(RpgGame.Instance.Secret&&RpgGame.Instance.Secret.Active?Mathf.Max(SonicLaunchSpeed,velocity.magnitude):SonicLaunchSpeed);steering=Vector2.zero;sonicBooms++;RpgFlightWake09.Boom(this);}
  }
  public float FlightSpeed=>(RpgGame.Instance.Secret&&RpgGame.Instance.Secret.Active?RpgGame.Instance.Secret.Speed:SonicActive&&FormStage==2&&flying&&flightBoosting&&!flightBraking?SonicTopSpeed:(FormStage==2?12:FormStage==1?8:6)*(flightBoosting&&!flightBraking?1.25f:1))*(RpgGame.Instance.Talents?.FlightBonus??1);
  public float GroundSpeed(bool run)=>(run?4.5f:2.8f)*(FormStage==2?1.45f:FormStage==1?1.3f:1)*(run?(RpgGame.Instance.Talents?.RunBonus??1):1);
  public float GaitTravel=>gaitTravel;
  public Color BaseTint=>isEnemy?(enemyKind==3?new Color(.72f,.9f,.85f):Color.white):Color.white;
  public Vector2 Home=>home;
  public void SetHome(Vector2 p){home=p;}
  public void SetFlightControls(bool boost,bool brake){flightBraking=flying&&brake;flightBoosting=flying&&boost&&!brake;}
float gaitPhase,poseClock;string lastPose="";
  public int animationFrame;public float AttackProgress=>attackDuration>0?1-attackTimer/attackDuration:0;
  public void ResetMotion(){velocity=Vector2.zero;strikeTalentMultiplier=1;ResetSonic();dodgeTimer=dodgeCooldown=staggerTimer=attackBuffer=dodgeTrail=0;sonicBooms=0;flying=flightBoosting=flightBraking=false;formStage=0;steering=Vector2.zero;flightHeight=gaitPhase=poseClock=gaitTravel=0;lastPose="";running=false;comboQueued=hitApplied=false;lastAttack=clock-2;comboIndex=0;}
  public bool ToggleFlight()
  {
   if(flying){Land();return true;}if(dead||charging||transforming>0||powerPose>0||attackTimer>0||jumpTimer>0||flightHeight>0||energy<(RpgGame.Instance.Talents?.PowerCost(12)??12)||Dodging)return false;
   energy-=RpgGame.Instance.Talents?.PowerCost(12)??12;flying=true;steering=Vector2.zero;flightBoosting=flightBraking=false;velocity=Vector2.zero;RpgAudio.Play("flight_launch",Position,.65f);RpgEffects.Burst(Position,RpgArt.Hex("A1F2DC"),1.6f,.45f);return true;
  }
  public void Land(){ResetSonic();if(!flying)return;flying=flightBoosting=flightBraking=false;RpgGame.Instance.Secret?.CancelOrbit();RpgAudio.Play("flight_land",Position,.4f);}
  float castWindup;float attackDuration=.53f,lastAttack=-20;public string attackAction="slash",currentAnimation="idle";
  public Vector2 Position=>(Vector2)transform.position;
  public int MaxHealth=>isEnemy?(enemyKind==5?1100+60*Mathf.Clamp(RpgGame.Instance.progress.level-1,0,20):enemyKind==4?280:enemyKind==3?78:enemyKind==1?120:enemyKind==2?55:65):100+((RpgGame.Instance?RpgGame.Instance.progress.level:1)-1)*12;
  public float Damage=>isEnemy?(enemyKind==5?28:enemyKind==4?26:enemyKind==3?15:enemyKind==1?22:12):(20+(RpgGame.Instance.progress.level-1)*3)*(FormStage==2?3.15f:FormStage==1?2.3f:1);
  public string EnemyName=>enemyKind==5?"Bonero Companheiro":enemyKind==4?"Companheiro":enemyKind==3?"Batedor do Morro":enemyKind==1?"Fiscal blindado":enemyKind==2?"Arauto da Vuvuzela":"Cobrador";
  public string Family=>isEnemy?(enemyKind==5?"BoneroCompanheiro16":enemyKind==4?"Companheiro12":enemyKind==1?"EnemyHeavy":enemyKind==2?"EnemyCaster":"Enemy"):FormStage==2?"HeroAscendant":buff>0?"HeroAwakened":charging&&charge>=55?"HeroLift":"Hero";
  public void Initialize(){home=Position;health=MaxHealth;shadow=RpgArt.Put("Sombra",RpgArt.Pixel,Position+Vector2.down*.04f,-3900,transform,new Color(.04f,.08f,.07f,.3f));shadow.transform.localScale=new Vector3(.72f,.12f,1);body=RpgArt.Put("Sprite animado",RpgArt.Frame(Family,"idle",2,0),Position,RpgWorld.Order(Position.y),transform);}
  public void ConfigureEnemy(int kind){enemyKind=kind;gameObject.name=EnemyName;health=MaxHealth;transform.localScale=Vector3.one*(kind==5?1.28f:kind==4?1.32f:kind==3?.98f:kind==1?1.12f:kind==2?.96f:1);body.color=BaseTint;Pose("idle",0);}
  public void Tick(float dt)
  {
   if(isEnemy&&enemyKind==5){clock+=dt;invincible=Mathf.Max(0,invincible-dt);staggerTimer=Mathf.Max(0,staggerTimer-dt);GetComponent<RpgFusionBoss15>()?.Tick(dt);return;}
   float wasDodge=dodgeTimer;dodgeTimer=Mathf.Max(0,dodgeTimer-dt);if(dodgeTimer<.00001f)dodgeTimer=0;dodgeCooldown=Mathf.Max(0,dodgeCooldown-dt);staggerTimer=Mathf.Max(0,staggerTimer-dt);attackBuffer=Mathf.Max(0,attackBuffer-dt);if(wasDodge>0&&!Dodging)velocity=Vector2.zero;
   clock+=dt;invincible=Mathf.Max(0,invincible-dt);buff=Mathf.Max(0,buff-(!isEnemy&&RpgGame.Instance.Secret&&RpgGame.Instance.Secret.Active?0:dt));transforming=buff>0?Mathf.Max(0,transforming-dt):0;jumpTimer=Mathf.Max(0,jumpTimer-dt);powerPose=Mathf.Max(0,powerPose-dt);castWindup=Mathf.Max(0,castWindup-dt);
   if(dead){ResetSonic();deadTime+=dt;Pose("hurt",Mathf.Min(5,(int)(deadTime*9)));body.color=new Color(1,1,1,Mathf.Clamp01(4-deadTime));if(isEnemy&&enemyKind!=4&&deadTime>40)ResetEnemy();return;}
   if(FormStage!=2)ResetSonic();if(flying&&!(RpgGame.Instance.Secret&&RpgGame.Instance.Secret.Active)){energy=Mathf.Max(0,energy-dt*((FormStage==2?9:FormStage==1?7:10)+(SonicActive?9:flightBoosting?6:0)));if(energy<=0)Land();}
   flightHeight=Mathf.MoveTowards(flightHeight,flying?(FormStage==2?2.2f:FormStage==1?1.9f:1.55f):0,dt*2.8f);
   if(flying){flightTrail+=dt;if(flightTrail>.06f&&velocity.sqrMagnitude>9){flightTrail=0;RpgEffects.Spark(Position+Vector2.up*flightHeight-velocity.normalized*.4f,EnergyColor,FormStage==2?1.4f:.7f);}}
   if(isEnemy)EnemyBrain(dt);
   if(attackTimer>0){attackTimer-=dt;if(attackBuffer>0&&AttackProgress>=.25f){comboQueued=true;attackBuffer=0;}if(AttackProgress>=attackContact&&!hitApplied){hitApplied=true;DealStrike();}if(attackTimer<=0&&comboQueued){comboQueued=false;Attack();}}
   if(charging){charge=Mathf.Min(100,charge+dt*31);energy=Mathf.Min(100,energy+dt*24);if(Random.value<dt*18)RpgEffects.Spark(Position+Vector2.up*.7f,RpgArt.Hex("FFD46E"),.6f+charge*.014f);}
   else if(!flying&&flightHeight<=0)energy=Mathf.Min(100,energy+dt*(isEnemy?5:RpgGame.Instance.Talents?.EnergyRecovery??5));
   if(buff>0&&Random.value<dt*10)RpgEffects.Spark(Position+Vector2.up*(.6f+flightHeight),EnergyColor,FormStage==2?2.2f:1.7f);
   string anim=Dodging?"run":charging||transforming>0||powerPose>0?"spellcast":isEnemy&&eliteTell>0?"spellcast":attackTimer>0?attackAction:flightHeight>.02f?"flight":jumpTimer>0?"jump":velocity.sqrMagnitude>.05f?(running?"run":"walk"):"idle";
   if(anim!=lastPose){poseClock=0;if(anim=="walk"||anim=="run"){if(lastPose!="walk"&&lastPose!="run")gaitPhase=0;}lastPose=anim;}else poseClock+=dt;
   int count=RpgArt.FrameCount(Family,anim);
   int frame=anim=="walk"?1+(int)(gaitPhase*8)%8:anim=="run"?(int)(gaitPhase*8)%8:anim=="flight"?(velocity.sqrMagnitude>.25f?2:0)+(int)(poseClock*4)%2:anim=="jump"?Mathf.Clamp((int)((.65f-jumpTimer)/.65f*count),0,count-1):attackTimer>0&&anim==attackAction?Mathf.Clamp((int)(AttackProgress*count),0,count-1):anim=="spellcast"?(int)(poseClock*9)%7:(int)(poseClock*2)%count;
   if(isEnemy&&enemyKind==4&&eliteTell>0)frame=Mathf.Clamp((int)((.85f-eliteTell)/.85f*5),0,4);
   if(Dodging)frame=Mathf.Clamp((int)((DodgeDuration-dodgeTimer)/DodgeDuration*8),0,7);
   if(charging)frame=charge<20?1:charge<50?2:charge<75?3:4;
   if(powerPose>0)frame=isEnemy&&enemyKind==4?(powerPose>.12f?5:6):castWindup>0?2:5;
   if(transforming>0){float age=TransformationDuration-transforming;frame=FormStage==2?new[]{2,3,4,5,5,4,3,2}[Mathf.Clamp((int)(age/TransformationDuration*8),0,7)]:age<.24f?2:age<.66f?5:2;}
   Pose(anim,frame);body.color=transforming<=0&&invincible>0&&((int)(clock*18)%2==0)?new Color(1,.65f,.63f):isEnemy?(eliteTell>0?new Color(1,.52f,.4f):enemyKind==3?new Color(.72f,.9f,.85f):Color.white):Color.white;
  }
  public void Pose(string animation,int frame){currentAnimation=animation;animationFrame=frame;body.sprite=RpgArt.Frame(transforming>0&&FormStage<2?"HeroLift":Family,animation,direction,frame);body.sortingOrder=RpgWorld.Order(Position.y);body.transform.localPosition=new Vector3(staggerTimer>0?Mathf.Sin(clock*35)*.025f:0,flightHeight+(flightHeight>.02f?Mathf.Sin(clock*4)*.035f:0)+(jumpTimer>0?Mathf.Sin(jumpTimer/.65f*Mathf.PI)*.75f:0),0);shadow.transform.localPosition=new Vector3(0,-.04f,0);}
  public void Walk(Vector2 input,float speed,float dt,bool run=false)
  {
   if(!isEnemy&&RpgGame.Instance.Secret){input=RpgGame.Instance.Secret.ResolveInput(this,input,dt);if(flying)speed=FlightSpeed;}
   if(Dodging){float speedDodge=FormStage==2?13.5f:FormStage==1?11.5f:9;float step=Mathf.Min(dt,dodgeTimer);var start=Position;transform.position=world.Move(Position,dodgeDirection*speedDodge*step);velocity=(Position-start)/Mathf.Max(.0001f,dt);gaitPhase+=Vector2.Distance(start,Position)/2.9f;gaitTravel+=Vector2.Distance(start,Position);running=true;dodgeTrail-=dt;if(dodgeTrail<=0){dodgeTrail=.04f;var o=new GameObject("Esquiva • rastro de movimento");o.transform.position=body.transform.position;o.transform.localScale=body.transform.lossyScale;var r=o.AddComponent<SpriteRenderer>();r.sprite=body.sprite;r.sortingOrder=body.sortingOrder-2;r.color=new Color(.68f,.85f,1,.20f);o.AddComponent<RpgWakeFade09>().SetGhost(r,.15f);}return;}
   if(!isEnemy&&attackTimer>0&&AttackProgress>.88f&&!QueuedCombo&&input.sqrMagnitude>.01f)attackTimer=0;
   running=run;input=Vector2.ClampMagnitude(input,1);bool locked=attackTimer>0||charging||transforming>0||powerPose>0||dead||staggerTimer>0;if(locked)input=Vector2.zero;if(!locked&&!flying&&input.sqrMagnitude>.01f){facing=input.normalized;direction=Mathf.Abs(input.x)>Mathf.Abs(input.y)?(input.x>0?3:1):(input.y>0?0:2);}
   if(flying){if(flightBraking)input=Vector2.zero;velocity=Vector2.SmoothDamp(velocity,input*speed,ref steering,flightBraking?.045f:SonicActive?.065f:.11f,Mathf.Infinity,dt);if(!locked&&velocity.sqrMagnitude>.25f){facing=velocity.normalized;direction=Mathf.Abs(facing.x)>Mathf.Abs(facing.y)?(facing.x>0?3:1):(facing.y>0?0:2);}}
   else velocity=Vector2.MoveTowards(velocity,input*speed,dt*(input.sqrMagnitude>.01f?24:30));if(locked){velocity=steering=Vector2.zero;}Vector2 before=Position;Vector2 rawTravel=velocity*dt;bool orbit=!isEnemy&&RpgGame.Instance.Secret&&RpgGame.Instance.Secret.Active;transform.position=orbit?RpgGame.Instance.Secret.MoveOrbit(Position,rawTravel):world.Move(Position,rawTravel);float travel=orbit?rawTravel.magnitude:Vector2.Distance(before,Position);gaitPhase+=travel/(running?2.9f:2.25f);gaitTravel+=travel;TravelSonic(orbit?rawTravel:Position-before,dt,locked);
   if(travel<.00001f&&input.sqrMagnitude>.01f)velocity=Vector2.zero;
  }
  public bool Attack()
  {
   if(dead||charging||transforming>0||powerPose>0||Dodging||staggerTimer>0)return false;if(attackTimer>0){if(!isEnemy&&attackTimer<.24f)comboQueued=true;return false;}
   strikeTalentMultiplier=isEnemy?1:RpgGame.Instance.Talents?.BeginStrike()??1;direction=Mathf.Abs(facing.x)>Mathf.Abs(facing.y)?(facing.x>0?3:1):(facing.y>0?0:2);comboIndex=isEnemy?0:clock-lastAttack<1.3f?(comboIndex+1)%3:0;lastAttack=clock;attackAction=isEnemy&&enemyKind==2?"spellcast":comboIndex==1?"thrust":comboIndex==2?"backslash":"slash";
   attackDuration=isEnemy?(enemyKind==1?.85f:enemyKind==2?.9f:.6f):comboIndex==0?.48f:comboIndex==1?.54f:.64f;attackTimer=attackDuration;attackContact=isEnemy?.55f:FormStage>0?(comboIndex==0?.36f:comboIndex==1?.52f:.54f):(comboIndex==0?.68f:comboIndex==1?.64f:.77f);hitApplied=false;velocity=Vector2.zero;
   if(isEnemy)RpgEffects.Spark(Position+Vector2.up*.75f+facing*.3f,RpgArt.Hex("F0A98D"),.5f);
   RpgAudio.Play(isEnemy&&enemyKind==2?"spell_02":"punch_swing"+comboIndex,Position,isEnemy?.24f:.42f,isEnemy&&enemyKind==1?.78f:1);return true;
  }
  public void Cast(float duration,float windup=0){powerPose=duration;castWindup=windup;velocity=Vector2.zero;}
  public void ResetEnemy(){transform.position=home;health=MaxHealth;dead=false;deadTime=0;velocity=Vector2.zero;attackTimer=jumpTimer=invincible=transforming=powerPose=0;charging=false;charge=buff=dodgeTimer=dodgeCooldown=staggerTimer=attackBuffer=0;comboQueued=false;enemyCooldown=1.2f;eliteTell=eliteRush=0;rushHit=false;direction=2;facing=Vector2.down;body.color=BaseTint;Pose("idle",0);}
  public bool Jump(){if(flying){Land();return true;}if(flightHeight>0||jumpTimer>0||attackTimer>0||powerPose>0||charging||dead||transforming>0||Dodging)return false;jumpTimer=.65f;RpgAudio.Play("cloth1",Position,.25f,1.1f);return true;}
  void DealStrike()
  {
   if(isEnemy&&enemyKind==2){RpgProjectile.Fire(this,facing);hitsLanded++;return;}
   Vector2 center=Position+facing*(comboIndex==1?.85f:.65f);RpgEffects.Strike(center+Vector2.up*flightHeight,facing,comboIndex,isEnemy?RpgArt.Hex("EBA095"):RpgArt.Hex("FFE4A3"));
   if(isEnemy){var h=world.hero;if(h.flightHeight<1&&Vector2.Distance(Position,h.Position)<(enemyKind==1?1.55f:1.3f)&&Vector2.Dot(facing,(h.Position-Position).normalized)>.05f&&world.HasClearPath(Position,h.Position)&&!h.dead){float before=h.health;h.TakeDamage(Damage);if(h.health<before)RpgAudio.Punch(false,enemyKind==1?2:0,h.Position);hitsLanded++;}}
   else {if(buff>0){RpgEffects.PowerStrike(center+Vector2.up*flightHeight,facing,comboIndex,EnergyColor);if(comboIndex==2)RpgAudio.Play("power_surge",Position,.3f);}foreach(var e in world.enemies)if(!e.dead&&CanStrike(e)){float before=e.health;float strikeDamage=Damage*(RpgGame.Instance.Talents?.MeleeBonus??1)*strikeTalentMultiplier*(comboIndex==2?1.55f*(RpgGame.Instance.Talents?.FinisherBonus??1):comboIndex==1?1.12f:1);e.TakeDamage(strikeDamage);if(e.health<before){RpgGame.Instance.Talents?.MeleeHit(e,strikeDamage,comboIndex);RpgAudio.Punch(buff>0,comboIndex,e.Position);RpgGame.Instance.shake=Mathf.Max(RpgGame.Instance.shake,buff>0?.65f:.25f);RpgGame.Instance.impactPause=buff>0?.065f:.035f;e.Stagger(comboIndex==2?.3f:.16f,comboIndex==2);e.transform.position=world.Move(e.Position,facing*(buff>0?(comboIndex==2?1.1f:.85f):comboIndex==2?.45f:.24f));hitsLanded++;}}}
  }
  bool CanStrike(RpgActor target){Vector2 delta=target.Position-Position;float reach=(comboIndex==1?2.05f:comboIndex==2?2:1.8f)+(FormStage>0?.25f:0);return delta.magnitude<reach&&Vector2.Dot(facing,delta.normalized)>=(comboIndex==1?.35f:comboIndex==2?-.05f:.15f)&&world.HasClearPath(Position,target.Position);}
  public void TakeDamage(float amount)
  {
   if(dead||amount<=0)return;if(!isEnemy&&RpgGame.Instance.Secret&&RpgGame.Instance.Secret.Active)return;if(invincible>0){if(!isEnemy)RpgGame.Instance.Talents?.AvoidedDamage();return;}health=Mathf.Max(0,health-amount);invincible=isEnemy?.16f:.8f;RpgGame.Instance.FloatText(Position+Vector2.up*1.7f,"−"+Mathf.RoundToInt(amount),isEnemy?RpgArt.Hex("FFE39C"):RpgArt.Hex("FF8A84"));RpgAudio.Play("creature_hurt_01",Position,.12f,Random.Range(.92f,1.06f));
   if(health<=0){CancelQueuedAttack();attackTimer=dodgeTimer=staggerTimer=0;ResetSonic();flying=false;flightHeight=0;dead=true;deadTime=0;velocity=Vector2.zero;charging=false;charge=buff=transforming=0;if(isEnemy){RpgGame.Instance.EnemyDefeated(this);RpgAudio.Play("creature_die_01",Position,.35f);}else{RpgGame.Instance.Talents?.ResetSession();RpgGame.Instance.Notify(RpgGame.Instance.Secret.InArena?"Você caiu. Pressione [R] para tentar a convergência novamente.":"Você caiu. Pressione [R] para voltar à praça.");}}
  }
  float eliteTell,eliteRush;bool rushHit;Vector2 eliteAim;public bool WindingUp=>eliteTell>0;
  void EliteBrain(RpgActor target,Vector2 delta,float distance,float dt)
  {
   if(attackTimer>0||powerPose>0){Walk(Vector2.zero,0,dt);return;}
   if(Vector2.Distance(Position,home)>9){Walk((home-Position).normalized,2.5f,dt);return;}
   if(eliteRush>0){float step=Mathf.Min(dt,eliteRush);eliteRush-=dt;Vector2 a=Position,b=world.Move(a,eliteAim*11*step);transform.position=b;gaitPhase+=Vector2.Distance(a,b)/2.9f;gaitTravel+=Vector2.Distance(a,b);velocity=eliteAim*11;running=true;Vector2 d=b-a;float u=d.sqrMagnitude>.0001f?Mathf.Clamp01(Vector2.Dot(target.Position-a,d)/d.sqrMagnitude):0;if(!rushHit&&target.flightHeight<1&&Vector2.Distance(target.Position,a+d*u)<1.05f){target.TakeDamage(Damage);rushHit=true;}return;}
   running=false;if(eliteTell>0){velocity=Vector2.zero;eliteTell-=dt;if(eliteTell<=0){if(enemyKind==4){for(int k=-1;k<=1;k++)RpgProjectile.Fire(this,Quaternion.Euler(0,0,k*19)*eliteAim);Cast(.24f);}else {eliteRush=.24f;rushHit=false;RpgEffects.Burst(Position,RpgArt.Hex("DA927F"),1.1f,.3f);}enemyCooldown=enemyKind==4?2.8f:2.1f;}return;}
   if(distance>7){Walk(Vector2.zero,0,dt);return;}
   if(enemyCooldown<=0&&distance<(enemyKind==4?6.5f:4.4f)&&(enemyKind!=4||distance>1.8f)){eliteAim=delta.normalized;eliteTell=enemyKind==4?.85f:.6f;facing=eliteAim;direction=Mathf.Abs(facing.x)>Mathf.Abs(facing.y)?(facing.x>0?3:1):(facing.y>0?0:2);RpgEffects.Spark(Position+Vector2.up, RpgArt.Hex(enemyKind==4?"FF697E":"FAC97C"),2);RpgAudio.Play("wave_windup",Position,.2f);return;}
   if(distance<1.2f){Walk(Vector2.zero,0,dt);if(enemyCooldown<=0){facing=delta.normalized;Attack();enemyCooldown=2;}}
   else{Vector2 toward=delta.normalized;Vector2 orbit=new(-toward.y,toward.x);Walk(enemyKind==3&&distance<3?toward*.45f+orbit*.65f:toward,enemyKind==3?2.6f:1.6f,dt);}
  }
  void EnemyBrain(float dt)
  {
   if(RpgGame.Instance.Secret&&RpgGame.Instance.Secret.Active){Walk(Vector2.zero,0,dt);return;}var target=world.hero;enemyCooldown-=dt;if(staggerTimer>0){Walk(Vector2.zero,0,dt);return;}if(target.dead){Walk(Vector2.zero,0,dt);return;}Vector2 delta=target.Position-Position;float distance=delta.magnitude;
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
