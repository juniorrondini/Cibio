using System.Collections.Generic;
using UnityEngine;

namespace Cibio.RPG
{
    public sealed class RpgWorldSecret15:MonoBehaviour
    {
        public enum SecretState{Village,Orbit,Revelation,Arena,Victory}
        public SecretState State{get;private set;}
        public bool Active=>enabled&&State==SecretState.Orbit;
        public bool Cinematic=>State==SecretState.Revelation;
        public bool InArena=>State==SecretState.Arena||State==SecretState.Victory;
        public bool Victory=>State==SecretState.Victory;
        public int Laps{get;private set;}public float OrbitAge{get;private set;}public float CinemaAge{get;private set;}
        public string AxisLabel=>Mathf.Abs(heading.x)>=Mathf.Abs(heading.y)?"leste/oeste":"norte/sul";
        public float LapProgress=>Mathf.Clamp01(Mathf.Max(Mathf.Abs(orbitDisplacement.x),Mathf.Abs(orbitDisplacement.y)));
        public float FlightTier{get;private set;}
        public float Speed=>60*Mathf.Pow(1.20f,Mathf.Min(10,FlightTier));
        public float TemporalIntensity=>Active?Mathf.Clamp01((Speed-60)/(60*Mathf.Pow(1.2f,10)-60)):0;
        public int WrapSerial{get;private set;}
        public float LapPulse{get;private set;}
        public Vector2 EntryPosition{get;private set;}
        public Vector2 SavePosition=>State==SecretState.Village?game.world.hero.Position:EntryPosition;
        public Texture2D IslandSnapshot{get;private set;}
        public RpgActor Boss{get;private set;}
        public float IntroRemaining{get;private set;}
        RpgGame game;Vector2 heading,orbitDisplacement;List<RpgActor> villageEnemies;Rect villageBounds;GameObject arena;SpriteRenderer flightMantle;bool sceneryWasActive;readonly Dictionary<GameObject,bool> villageExtras=new();
        void Awake(){game=GetComponent<RpgGame>();}
        public Vector2 ResolveInput(RpgActor hero,Vector2 input,float dt)
        {
            if(!isActiveAndEnabled)return input;
            bool eligible=!InArena&&!Cinematic&&hero.FormStage==2&&hero.flying&&hero.flightBoosting&&!hero.flightBraking&&!hero.dead&&hero.transforming<=0&&hero.attackTimer<=0&&hero.powerPose<=0;
            if(!eligible){if(Active)CancelOrbit();return input;}
            if(!Active)
            {
                EntryPosition=NearestLand(hero.Position,hero.Position);heading=input.sqrMagnitude>.1f?input.normalized:hero.facing.normalized;if(heading.sqrMagnitude<.1f)heading=Vector2.right;
                orbitDisplacement=Vector2.zero;WrapSerial=0;LapPulse=0;FlightTier=0;
                Laps=0;OrbitAge=0;State=SecretState.Orbit;game.Notify("ALÉM DA BARREIRA • mantenha Shift • a ilha já não limita você");RpgAudio.Play("world_break15",hero.Position,.8f);
                flightMantle=RpgEffects.Create("Manto orbital • ruptura do mundo",hero.transform);flightMantle.transform.localPosition=Vector2.up*2.2f;flightMantle.transform.localScale=new Vector3(4.8f,7.4f,1);flightMantle.sortingOrder=hero.body.sortingOrder+6;
            }
            OrbitAge+=dt;LapPulse=Mathf.Max(0,LapPulse-dt*2);if(input.sqrMagnitude>.1f)heading=input.normalized;return heading;
        }
        public Vector2 MoveOrbit(Vector2 before,Vector2 delta)
        {
            var b=game.world.bounds;Vector2 next=before+delta;bool crossX=next.x>=b.xMax||next.x<b.xMin,crossY=next.y>=b.yMax||next.y<b.yMin;
            // Integrate UNWRAPPED real travel, normalized to each world dimension.
            // Starting while idle or changing heading can never lock counting to an old axis.
            // Signed displacement cancels boundary jitter and short backtracking.
            orbitDisplacement+=new Vector2(delta.x/b.width,delta.y/b.height);
            float progress=Mathf.Max(Mathf.Abs(orbitDisplacement.x),Mathf.Abs(orbitDisplacement.y));
            while(progress>=1&&Laps<10)
            {
                orbitDisplacement*=1-1/progress;Laps++;LapPulse=1;game.Chronal?.OnLap(Laps,before);
                if(Laps<5)RpgAudio.Play("orbit_lap15",before,.5f,Mathf.Min(1.7f,1+Laps*.065f));
                game.FloatText(before+Vector2.up*3,Laps+" / 10",RpgArt.Hex("C5EDFF"));
                progress=Mathf.Max(Mathf.Abs(orbitDisplacement.x),Mathf.Abs(orbitDisplacement.y));
            }
            FlightTier=Mathf.Max(FlightTier,Laps+LapProgress);
            if(crossX)next.x=b.xMin+Mathf.Repeat(next.x-b.xMin,b.width);if(crossY)next.y=b.yMin+Mathf.Repeat(next.y-b.yMin,b.height);
            if(crossX||crossY){WrapSerial++;game.lens.transform.position=new Vector3(next.x,next.y+1.5f,-10);game.ClearCameraInertia();}
            return next;
        }
        public void CancelOrbit()
        {
            if(State!=SecretState.Orbit)return;State=SecretState.Village;Laps=0;OrbitAge=0;FlightTier=0;orbitDisplacement=Vector2.zero;LapPulse=0;ClearMantle();
            var h=game.world.hero;Vector2 safe=NearestLand(h.Position,EntryPosition);if(Vector2.Distance(safe,h.Position)>.1f){h.transform.position=safe;game.SnapCamera();}h.ResetSonic();game.Chronal?.Tick(0);
        }
        Vector2 NearestLand(Vector2 point,Vector2 fallback)
        {
            if(game.world.CanStand(point))return point;
            for(float r=.5f;r<=9;r+=.5f)for(int k=0;k<24;k++){float a=k*Mathf.PI/12;Vector2 p=point+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r;if(game.world.CanStand(p))return p;}
            return game.world.CanStand(fallback)?fallback:new Vector2(0,-3);
        }
        public void Advance(float dt)
        {
            if(!game.Playing)return;game.Chronal?.Tick(dt);if(Active&&Laps>=10)BeginRevelation();
            if(Cinematic){CinemaAge+=dt;if(CinemaAge>=7.2f)EnterArena();}
            if(InArena)IntroRemaining=Mathf.Max(0,IntroRemaining-dt);
        }
        void LateUpdate()
        {
            if(!game.ready||!Active||!flightMantle)return;var h=game.world.hero;flightMantle.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(h.facing.y,h.facing.x)*Mathf.Rad2Deg-90);flightMantle.sortingOrder=h.body.sortingOrder+6;
            float f=OrbitAge<.8f?OrbitAge/.8f*9:8+Mathf.Repeat(OrbitAge*11,4);RpgEffects.Paint(flightMantle,"Vfx_WorldFlight15",f,Color.white,1.3f);
        }
        public void BeginRevelation()
        {
            if(Cinematic||InArena)return;if(State==SecretState.Village)EntryPosition=NearestLand(game.world.hero.Position,new Vector2(0,-3));
            State=SecretState.Revelation;CinemaAge=0;game.progress.worldHornDiscovered=true;game.Save();ClearTransientCombat();ClearMantle();
            var h=game.world.hero;h.ResetMotion();h.attackTimer=h.powerPose=h.transforming=0;h.GetComponent<RpgPowerForm>().StopAllCoroutines();h.gameObject.SetActive(false);
            CaptureIsland();game.GetComponent<RpgTalents15>().ResetSession();game.messageUntil=0;RpgAudio.Play("world_reveal15",EntryPosition,.85f);
        }
        void CaptureIsland()
        {
            if(IslandSnapshot)Destroy(IslandSnapshot);var cam=game.lens;var pos=cam.transform.position;float size=cam.orthographicSize;var previous=RenderTexture.active;var target=cam.targetTexture;
            var rt=new RenderTexture(960,1120,24);cam.orthographicSize=57;cam.transform.position=new Vector3(0,28,-10);cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;
            IslandSnapshot=new Texture2D(960,1120,TextureFormat.RGB24,false);IslandSnapshot.ReadPixels(new Rect(0,0,960,1120),0,0);IslandSnapshot.Apply();cam.targetTexture=target;RenderTexture.active=previous;Destroy(rt);cam.orthographicSize=size;cam.transform.position=pos;
        }
        void ClearMantle(){if(!flightMantle)return;flightMantle.enabled=false;Destroy(flightMantle.gameObject);flightMantle=null;}
        void ClearTransientCombat()
        {
            foreach(var p in Object.FindObjectsByType<RpgProjectile>(FindObjectsSortMode.None)){p.gameObject.SetActive(false);Destroy(p.gameObject);}
            foreach(var fx in Object.FindObjectsByType<RpgAtlasFx15>(FindObjectsSortMode.None)){fx.gameObject.SetActive(false);Destroy(fx.gameObject);}
            foreach(var fx in Object.FindObjectsByType<RpgEffects>(FindObjectsSortMode.None)){fx.gameObject.SetActive(false);Destroy(fx.gameObject);}
            foreach(var p in Object.FindObjectsByType<RpgHornShot15>(FindObjectsSortMode.None)){p.gameObject.SetActive(false);Destroy(p.gameObject);}
            foreach(var wake in Object.FindObjectsByType<RpgWakeFade09>(FindObjectsSortMode.None)){wake.gameObject.SetActive(false);Destroy(wake.gameObject);}
            foreach(var ps in Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None))ps.Clear();
            game.CancelPowerSequences();game.floating.Clear();game.impactPause=0;
        }
        public void EnterArena()
        {
            if(InArena)return;ClearTransientCombat();if(State==SecretState.Village)EntryPosition=NearestLand(game.world.hero.Position,new Vector2(0,-3));
            ClearMantle();villageBounds=game.world.bounds;villageEnemies=game.world.enemies;sceneryWasActive=game.world.scenery.gameObject.activeSelf;
            villageExtras.Clear();foreach(var shrine in game.world.shrines)if(shrine)villageExtras[shrine.gameObject]=shrine.gameObject.activeSelf;var quest=game.GetComponent<RpgQuest08>();if(quest.giver)villageExtras[quest.giver.gameObject]=quest.giver.gameObject.activeSelf;foreach(var node in quest.nodes)if(node)villageExtras[node.gameObject]=node.gameObject.activeSelf;
            foreach(var pair in villageExtras)pair.Key.SetActive(false);
            game.world.scenery.gameObject.SetActive(false);foreach(var e in villageEnemies)e.gameObject.SetActive(false);game.world.enemies=new List<RpgActor>();game.world.bounds=new Rect(-24,-18,48,36);
            State=SecretState.Arena;arena=new GameObject("A boca do mundo • arena secreta");arena.transform.SetParent(game.world.transform,false);
            var art=RpgArt.Texture("Arena_Vuvuzela15");var sprite=Sprite.Create(art,new Rect(0,0,art.width,art.height),Vector2.one*.5f,art.width/48f,0,SpriteMeshType.FullRect);var r=RpgArt.Put("Arena • bronze e cosmos",sprite,Vector2.zero,-4000,arena.transform);r.transform.localScale=new Vector3(1,36/(art.height/sprite.pixelsPerUnit),1);
            var h=game.world.hero;h.gameObject.SetActive(true);h.ResetMotion();h.attackTimer=h.powerPose=h.transforming=0;h.dead=false;h.deadTime=0;h.body.color=Color.white;h.transform.position=new Vector2(0,-6);h.health=h.MaxHealth;h.energy=100;h.buff=16+game.GetComponent<RpgTalents15>().ExtraDuration;h.formStage=2;h.invincible=2;h.jumpTimer=0;h.direction=0;h.facing=Vector2.up;h.Pose("idle",0);
            Boss=game.world.CreateActor("Bonero Companheiro",new Vector2(0,4.2f),true);Boss.transform.SetParent(arena.transform,true);Boss.ConfigureEnemy(5);Boss.shadow.sharedMaterial=Resources.Load<Material>("RpgArt/SoftShadow11");Boss.shadow.transform.localScale=new Vector3(1.6f,.3f,1);Boss.SetHome(Boss.Position);Boss.gameObject.AddComponent<RpgFusionBoss15>();game.world.enemies.Add(Boss);IntroRemaining=1.5f;
            game.messageUntil=0;game.GetComponent<RpgTalents15>().ResetSession();game.SnapCamera();game.lens.orthographicSize=11.4f;game.Save();RpgAudio.Play("fusion_entry15",Boss.Position,.8f);
        }
        public bool CanStandInArena(Vector2 p,float radius=.22f){Vector2 q=p-new Vector2(0,-1);return q.x*q.x/Mathf.Pow(17-radius,2)+q.y*q.y/Mathf.Pow(9-radius,2)<1;}
        public bool CameraStep(float dt)
        {
            if(Cinematic)return true;
            if(Active)
            {
                game.shake=Mathf.MoveTowards(game.shake,0,dt*2);var hero=game.world.hero;float targetSize=Mathf.Lerp(9.4f,13.8f,TemporalIntensity)+LapPulse*.55f+(game.Chronal?.Impulse??0)*.9f;
                game.lens.orthographicSize=Mathf.Lerp(game.lens.orthographicSize,targetSize,1-Mathf.Exp(-dt*7));
                Vector2 ahead=hero.Position+Vector2.up*(.8f+hero.flightHeight*.35f)+heading*1.2f;
                float shock=game.Chronal?.Impulse??0;Vector2 kick=new Vector2(Mathf.Sin(OrbitAge*37),Mathf.Cos(OrbitAge*29))*shock*.10f;game.lens.transform.position=Vector3.Lerp(game.lens.transform.position,new Vector3(ahead.x+kick.x,ahead.y+kick.y,-10),1-Mathf.Exp(-dt*18));
                return true;
            }
            if(!InArena)return false;var h=game.world.hero;
            game.lens.orthographicSize=Mathf.Lerp(game.lens.orthographicSize,11.4f,dt*4);Vector2 p=Vector2.Lerp(h.Position,Boss?Boss.Position:Vector2.zero,.35f);p=Vector2.ClampMagnitude(p,2.2f);
            game.shake=Mathf.MoveTowards(game.shake,0,dt*2);Vector2 shake=UnityEngine.Random.insideUnitCircle*game.shake*.07f;
            game.lens.transform.position=Vector3.Lerp(game.lens.transform.position,new Vector3(p.x+shake.x,-.3f+p.y*.25f+shake.y,-10),1-Mathf.Exp(-dt*6));return true;
        }
        public void BossDefeated()
        {
            if(!InArena||Victory)return;State=SecretState.Victory;game.progress.fusionBossDefeated=true;
            if(!game.progress.fusionRewardClaimed){game.progress.fusionRewardClaimed=true;game.progress.bonusTalentPoints=1;game.GainExperience(350);game.Notify("CONVERGÊNCIA VENCIDA • +350 XP • +1 ponto de talento • [E] retornar à vila");}
            else game.Notify("O SISTEMA FOI VENCIDO • [E] retornar à vila");
            ClearHostileShots();RpgEffects.Burst(Boss.Position,RpgArt.Hex("FFE2A4"),6,.9f);RpgAudio.Play("fusion_victory15",Boss.Position,.8f);game.world.hero.health=game.world.hero.MaxHealth;game.world.hero.energy=100;game.Save();
        }
        public bool Interaction()
        {
            if(InArena){if(Victory)ReturnToVillage();else game.Notify("Bonero Companheiro guarda a saída. Enfrente a convergência.");return true;}
            if(Cinematic||Active)return true;
            if(game.progress.worldHornDiscovered&&Vector2.Distance(game.world.hero.Position,new Vector2(25,67))<3){EntryPosition=game.world.hero.Position;EnterArena();return true;}return false;
        }
        public void Retry()
        {
            if(!InArena)return;ClearTransientCombat();State=SecretState.Arena;var h=game.world.hero;h.ResetMotion();h.dead=false;h.deadTime=0;h.attackTimer=h.powerPose=h.transforming=0;h.buff=16+game.GetComponent<RpgTalents15>().ExtraDuration;h.formStage=2;h.transform.position=new Vector2(0,-6);h.health=h.MaxHealth;h.energy=100;h.invincible=2;h.body.color=Color.white;h.jumpTimer=0;h.direction=0;h.facing=Vector2.up;h.Pose("idle",0);Boss.ResetEnemy();Boss.GetComponent<RpgFusionBoss15>().ResetFight();IntroRemaining=1.2f;game.GetComponent<RpgTalents15>().ResetSession();game.SnapCamera();
        }
        void ClearHostileShots(){foreach(var p in Object.FindObjectsByType<RpgHornShot15>(FindObjectsSortMode.None)){p.gameObject.SetActive(false);Destroy(p.gameObject);}}
        public void ReturnToVillage(){Vector2 at=EntryPosition;ResetWorld();var h=game.world.hero;h.ResetMotion();h.buff=h.attackTimer=h.powerPose=h.transforming=0;h.transform.position=NearestLand(at,new Vector2(0,-3));h.invincible=2;h.jumpTimer=0;h.direction=0;h.facing=Vector2.up;h.Pose("idle",0);game.SnapCamera();game.Save();game.Notify("O farol guarda a passagem de volta ao mundo-vuvuzela. [T] desenvolva sua resistência.");}
        public void Resume(bool continuing){if(continuing&&game.progress.worldHornDiscovered&&!game.progress.fusionBossDefeated){EntryPosition=game.world.hero.Position;EnterArena();}}
        public void ResetWorld()
        {
            ClearHostileShots();if(arena){ClearTransientCombat();game.world.bounds=villageBounds;game.world.enemies=villageEnemies;game.world.scenery.gameObject.SetActive(sceneryWasActive);foreach(var e in villageEnemies)if(e)e.gameObject.SetActive(true);foreach(var pair in villageExtras)if(pair.Key)pair.Key.SetActive(pair.Value);villageExtras.Clear();arena.SetActive(false);Destroy(arena);}
            arena=null;villageEnemies=null;Boss=null;ClearMantle();flightMantle=null;State=SecretState.Village;Laps=0;FlightTier=0;orbitDisplacement=Vector2.zero;LapPulse=0;OrbitAge=CinemaAge=IntroRemaining=0;game.world.hero.gameObject.SetActive(true);
        }
        void OnDisable(){if(game&&game.ready&&State==SecretState.Orbit)CancelOrbit();}
        void OnDestroy(){if(IslandSnapshot)Destroy(IslandSnapshot);}
    }
}
