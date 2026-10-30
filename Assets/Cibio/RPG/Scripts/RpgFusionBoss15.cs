using UnityEngine;

namespace Cibio.RPG
{
    public sealed class RpgFusionBoss15:MonoBehaviour
    {
        public enum Move{Approach,Gauntlet,Proclamation,March,FinalClamor}
        public Move CurrentMove{get;private set;}public float MoveAge{get;private set;}
        public bool Enraged{get;private set;}public int ReleasedAttacks{get;private set;}
        public string MoveName=>CurrentMove==Move.Gauntlet?"Punhos da Convergência":CurrentMove==Move.Proclamation?"Discurso Uníssono":CurrentMove==Move.March?"Marcha da Fusão":CurrentMove==Move.FinalClamor?"Clamor dos Dois":"A convergência observa";
        RpgActor actor;RpgGame game;Vector2 aim;float cooldown=1.2f,telegraph;int sequence,releases;bool chargeHit;
        void Awake(){actor=GetComponent<RpgActor>();game=RpgGame.Instance;}
        public void ResetFight(){CurrentMove=Move.Approach;MoveAge=0;Enraged=false;ReleasedAttacks=sequence=releases=0;cooldown=1.2f;chargeHit=false;}
        void Face(Vector2 d){if(d.sqrMagnitude<.001f)return;actor.facing=d.normalized;actor.direction=Mathf.Abs(d.x)>Mathf.Abs(d.y)?(d.x>0?3:1):(d.y>0?0:2);}
        public void BeginMove(Move move)
        {
            CurrentMove=move;MoveAge=0;releases=0;chargeHit=false;telegraph=0;aim=(game.world.hero.Position-actor.Position).normalized;if(aim.sqrMagnitude<.01f)aim=Vector2.down;Face(aim);actor.velocity=Vector2.zero;
            RpgAudio.Play(move==Move.Gauntlet?"fusion_combo15":"horn_windup15",actor.Position,.4f,Enraged?1.08f:1);
        }
        void Telegraph(float dt)
        {
            telegraph-=dt;if(telegraph>0)return;telegraph=.18f;
            Vector2 origin=actor.Position+Vector2.up*1.5f+aim*.5f;RpgAtlasFx15.Emit("Vfx_Horn15",origin,new Vector2(1,1.7f),.28f,new Color(1,.73f,.3f,.24f),Mathf.Atan2(aim.y,aim.x)*Mathf.Rad2Deg-90);
        }
        public void Tick(float dt)
        {
            if(actor.dead){actor.deadTime+=dt;actor.velocity=Vector2.zero;actor.Pose("hurt",Mathf.Clamp((int)(actor.deadTime*7),0,5));actor.body.color=Color.white;return;}
            var secret=game.GetComponent<RpgWorldSecret15>();var hero=game.world.hero;if(!secret.InArena||secret.IntroRemaining>0||hero.dead){actor.velocity=Vector2.zero;actor.Pose("idle",(int)(actor.clock*2)%4);return;}
            if(!Enraged&&actor.health<=actor.MaxHealth*.6f){Enraged=true;game.Notify("BONERO COMPANHEIRO • duas vozes, um só clamor");RpgAudio.Play("fusion_phase15",actor.Position,.65f);RpgAtlasFx15.Emit("Vfx_Horn15",actor.Position+Vector2.up,new Vector2(6,7),.8f,RpgArt.Hex("FFE4AE"));}
            actor.body.color=actor.invincible>0?new Color(1,.65f,.53f):Enraged?new Color(1,.91f,.82f):Color.white;MoveAge+=dt;
            Vector2 delta=hero.Position-actor.Position;float distance=delta.magnitude;
            if(CurrentMove==Move.Approach)
            {
                cooldown-=dt;if(actor.staggerTimer>0){actor.velocity=Vector2.zero;actor.Pose("idle",0);return;}
                if(cooldown<=0){sequence++;BeginMove(Enraged&&sequence%3==0?Move.FinalClamor:distance<3.1f?Move.Gauntlet:sequence%2==0?Move.March:Move.Proclamation);return;}
                Face(delta);if(distance>2.6f)actor.Walk(delta.normalized,Enraged?2.4f:1.8f,dt);else actor.Walk(Vector2.zero,0,dt);actor.Pose(actor.velocity.sqrMagnitude>.1f?"walk":"idle",actor.velocity.sqrMagnitude>.1f?(int)(actor.GaitTravel*4)%8:(int)(actor.clock*2)%4);return;
            }
            if(CurrentMove==Move.Gauntlet)
            {
                if(MoveAge<.27f){aim=delta.normalized;Face(aim);}float interval=Enraged?.36f:.42f;
                for(int k=0;k<3;k++)if(releases==k&&MoveAge>=.50f+k*interval){releases++;ReleasedAttacks++;actor.transform.position=game.world.Move(actor.Position,aim*.20f);if(hero.flightHeight<1&&Vector2.Distance(hero.Position,actor.Position)<2.5f&&Vector2.Dot(aim,(hero.Position-actor.Position).normalized)>.2f&&game.world.HasClearPath(actor.Position,hero.Position))hero.TakeDamage(Enraged?27:22);RpgAtlasFx15.Emit("Vfx_Horn15",actor.Position+aim+Vector2.up*.9f,new Vector2(2.2f,3),.32f,RpgArt.Hex("FFE0A1"),Mathf.Atan2(aim.y,aim.x)*Mathf.Rad2Deg-90);RpgAudio.Play("power_hit"+k,actor.Position,.35f);}
                int f=MoveAge<.5f?Mathf.Clamp((int)(MoveAge/.5f*3),0,2):MoveAge<.5f+interval*3?3+Mathf.Clamp((int)(Mathf.Repeat(MoveAge-.5f,interval)/interval*3),0,2):5;actor.Pose("slash",f);
                if(MoveAge>.5f+interval*3+.32f)Finish();return;
            }
            if(CurrentMove==Move.Proclamation)
            {
                if(MoveAge<.55f){aim=delta.normalized;Face(aim);}if(MoveAge<1){Telegraph(dt);actor.Pose("spellcast",Mathf.Clamp((int)(MoveAge*5),0,4));}
                else {if(releases==0){releases=1;Fan(aim,Enraged?5:3,Enraged?14:20,Enraged?9.2f:8.2f,22);ReleasedAttacks++;RpgAudio.Play("horn_blast15",actor.Position,.72f);}actor.Pose("spellcast",MoveAge<1.17f?5:MoveAge<1.35f?6:7);}if(MoveAge>1.9f)Finish();return;
            }
            if(CurrentMove==Move.March)
            {
                if(MoveAge<.40f){aim=delta.normalized;Face(aim);}if(MoveAge<.8f){Telegraph(dt);actor.Pose("slash",2);}
                else if(MoveAge<1.45f)
                {
                    if(releases==0){releases=1;ReleasedAttacks++;RpgAudio.Play("fusion_dash15",actor.Position,.6f);RpgAtlasFx15.Emit("Vfx_WorldFlight15",actor.Position+Vector2.up,new Vector2(3.5f,5.5f),.45f,RpgArt.Hex("FFD9A1"),Mathf.Atan2(aim.y,aim.x)*Mathf.Rad2Deg-90);}
                    Vector2 before=actor.Position,next=game.world.Move(before,aim*(Enraged?17:14)*dt);actor.transform.position=next;actor.velocity=(next-before)/Mathf.Max(.0001f,dt);actor.Pose("walk",(int)(MoveAge*20)%8);
                    if(!chargeHit&&hero.flightHeight<1&&RpgHornShot15.SegmentDistance(hero.Position,before,next)<1.35f){chargeHit=true;hero.TakeDamage(30);}
                }
                else{actor.velocity=Vector2.zero;actor.Pose("slash",5);}if(MoveAge>2.15f)Finish();return;
            }
            if(CurrentMove==Move.FinalClamor)
            {
                if(MoveAge<.65f){aim=delta.normalized;Face(aim);}if(MoveAge<1.15f){Telegraph(dt);actor.Pose("spellcast",Mathf.Clamp((int)(MoveAge/1.15f*5),0,4));}
                else{if(releases==0){releases=1;Fan(aim,7,24,6.7f,18);ReleasedAttacks++;RpgAudio.Play("horn_blast15",actor.Position,.7f,.8f);}if(releases==1&&MoveAge>=1.85f){releases=2;Fan(Quaternion.Euler(0,0,90)*aim,7,24,7.5f,18);ReleasedAttacks++;RpgAudio.Play("horn_blast15",actor.Position,.7f,1.12f);}actor.Pose("spellcast",MoveAge<2.05f?5:MoveAge<2.25f?6:7);}if(MoveAge>2.8f)Finish();
            }
        }
        void Fan(Vector2 direction,int count,float spread,float speed,float damage){for(int k=0;k<count;k++){Vector2 d=Quaternion.Euler(0,0,(k-(count-1)*.5f)*spread)*direction;RpgHornShot15.Fire(actor,d,speed,damage);}}
        void Finish(){CurrentMove=Move.Approach;MoveAge=0;actor.velocity=Vector2.zero;cooldown=Enraged?.65f:1;}
    }
    public sealed class RpgHornShot15:MonoBehaviour
    {
        public RpgActor owner;public Vector2 velocity;public float damage;public bool Finished{get;private set;}float age;SpriteRenderer image;
        public static RpgHornShot15 Fire(RpgActor owner,Vector2 direction,float speed,float damage)
        {
            var r=RpgEffects.Create("Voz da convergência • onda de bronze",null);var p=r.gameObject.AddComponent<RpgHornShot15>();p.owner=owner;p.velocity=direction.normalized*speed;p.damage=damage;p.image=r;r.transform.position=owner.Position+direction.normalized*.9f;r.transform.localScale=new Vector3(1.4f,3.6f,1);p.Paint();return p;
        }
        void Update(){if(RpgGame.Instance.Playing)Advance(Time.deltaTime);}
        public void Advance(float dt)
        {
            if(Finished)return;if(!owner||owner.dead){End();return;}age+=dt;if(age>3.8f){End();return;}Vector2 a=transform.position,b=a+velocity*dt;int steps=Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(a,b)/.15f));
            var h=owner.world.hero;Vector2 previous=a;
            for(int k=1;k<=steps;k++)
            {
                Vector2 point=Vector2.Lerp(a,b,k/(float)steps);if(!owner.world.CanStand(point,.08f)){End();return;}
                if(!h.dead&&SegmentDistance(h.Position,previous,point)<.58f){h.TakeDamage(damage);RpgAtlasFx15.Emit("Vfx_Horn15",h.Position+Vector2.up,new Vector2(2.5f,3),.4f,RpgArt.Hex("FFE5AE"));End();return;}previous=point;
            }
            transform.position=b;Paint();
        }
        void Paint(){image.sortingOrder=RpgWorld.Order(transform.position.y)+4;image.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(velocity.y,velocity.x)*Mathf.Rad2Deg-90);RpgEffects.Paint(image,"Vfx_Horn15",5+Mathf.Repeat(age*15,7),RpgArt.Hex("FFE7BA"),1.1f);}
        void End(){Finished=true;image.enabled=false;Destroy(gameObject);}
        public static float SegmentDistance(Vector2 p,Vector2 a,Vector2 b){Vector2 d=b-a;float t=d.sqrMagnitude>.00001f?Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude):0;return Vector2.Distance(p,a+d*t);}
    }
}
