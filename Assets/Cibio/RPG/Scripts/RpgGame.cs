using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Cibio.RPG
{
    [Serializable] public sealed class RpgProgress
    {
        public int version=3,level=1,xp,perks,defeated,questStage,silenced,islandCaches;public bool captainDefeated;public float x,y=-3;
        public bool Has(int id)=>(perks&(1<<id))!=0;
        public int NextLevel=>level*50;
    }
    public sealed class RpgGame:MonoBehaviour
    {
        public static RpgGame Instance;public const string SaveKey="Cibio.RPG.v3.Progress";
        public RpgWorld world;public Camera lens;public Material effectsMaterial;public RpgProgress progress=new();
        public bool Playing=>mode==1;public bool HasSave=>PlayerPrefs.HasKey(SaveKey);public int mode; // 0 menu, 1 play, 2 pause, 3 credits
        public string message="";public float messageUntil,shake,saveTimer;public float dashCooldown,waveCooldown,boltCooldown,impactPause;Vector3 followVelocity;public bool ready;bool bufferedAttack,transformInputHeld;int session;
        public bool reviewPose,reviewRun;public readonly List<FloatingText> floating=new();public sealed class FloatingText{public Vector2 p;public string text;public Color color;public float born;}
        void Awake()
        {
            Instance=this;Application.targetFrameRate=60;world=GetComponent<RpgWorld>();if(!world)world=gameObject.AddComponent<RpgWorld>();world.Build();if(!GetComponent<RpgQuest08>())gameObject.AddComponent<RpgQuest08>();RpgTerrain.Ensure(world);if(!world.hero.GetComponent<RpgPowerForm>())world.hero.gameObject.AddComponent<RpgPowerForm>();lens=Camera.main;if(!lens.GetComponent<RpgAudioLimiter>())lens.gameObject.AddComponent<RpgAudioLimiter>();if(!lens.GetComponent<RpgEnergyBloom>())lens.gameObject.AddComponent<RpgEnergyBloom>();for(int n=0;n<world.enemies.Count;n++)world.enemies[n].ConfigureEnemy(n<7?n%3:world.enemies[n].enemyKind);world.shrines[0].title="Voo da Centelha";world.shrines[0].description="[1] decolar/pousar | 12 energia + 10/s no ar.";world.shrines[1].description="Melhora sua transformação: duração de 24s.";if(!GetComponent<RpgFlightWake09>())gameObject.AddComponent<RpgFlightWake09>();if(!GetComponent<RpgFeedback09>())gameObject.AddComponent<RpgFeedback09>();if(!GetComponent<RpgAudio>())gameObject.AddComponent<RpgAudio>();ready=true;SnapCamera();
            var args=Environment.GetCommandLineArgs();for(int i=0;i<args.Length-1;i++){if(args[i]=="-cibio-island-review14"){reviewRun=true;StartCoroutine(gameObject.AddComponent<RpgIslandReview14>().Run(args[i+1]));}else if(args[i]=="-cibio-art-review13"){reviewRun=true;StartCoroutine(gameObject.AddComponent<RpgArtReview13>().Run(args[i+1]));}else if(args[i]=="-cibio-boss-review12"){reviewRun=true;StartCoroutine(gameObject.AddComponent<RpgBossReview12>().Run(args[i+1]));}else if(args[i]=="-cibio-village-review11"){reviewRun=true;StartCoroutine(gameObject.AddComponent<RpgVillageReview11>().Run(args[i+1]));}else if(args[i]=="-cibio-sonic-review10"){reviewRun=true;StartCoroutine(gameObject.AddComponent<RpgSonicReview10>().Run(args[i+1]));}else if(args[i]=="-cibio-ui-review09"){reviewRun=true;StartCoroutine(gameObject.AddComponent<RpgReview09>().RunUi(args[i+1]));}else if(args[i]=="-cibio-review09"){reviewRun=true;StartCoroutine(gameObject.AddComponent<RpgReview09>().Run(args[i+1]));}else if(args[i]=="-cibio-review08"){reviewRun=true;StartCoroutine(gameObject.AddComponent<RpgReview08>().Run(args[i+1]));}else if(args[i]=="-cibio-review07"){reviewRun=true;StartCoroutine(gameObject.AddComponent<RpgReview07>().Run(args[i+1]));}else if(args[i]=="-cibio-review"){reviewRun=true;StartCoroutine(ReviewEffects06(args[i+1]));}else if(args[i]=="-cibio-vfx-review"){reviewRun=true;StartCoroutine(gameObject.AddComponent<RpgVfxReview>().Run(args[i+1]));}}
        }
        public void StartGame(bool continueGame)
        {
            session++;progress=continueGame?Load():new RpgProgress();mode=1;var hero=world.hero;
            Vector2 spawn=new(progress.x,progress.y);hero.transform.position=world.CanStand(spawn)?spawn:new Vector2(0,-3);
            bufferedAttack=transformInputHeld=false;hero.ResetMotion();floating.Clear();hero.GetComponent<RpgPowerForm>()?.StopAllCoroutines();hero.health=hero.MaxHealth;hero.energy=100;hero.buff=hero.charge=hero.attackTimer=hero.jumpTimer=hero.invincible=hero.transforming=hero.powerPose=0;hero.dead=hero.charging=false;hero.deadTime=0;hero.velocity=Vector2.zero;hero.direction=2;hero.facing=Vector2.down;dashCooldown=waveCooldown=boltCooldown=impactPause=0;hero.body.color=Color.white;hero.Pose("idle",0);var form=hero.GetComponent<RpgPowerForm>();if(form)form.Refresh(1);var aura=hero.GetComponent<RpgAura>();if(aura)aura.Refresh(1);foreach(var ps in hero.GetComponentsInChildren<ParticleSystem>())ps.Clear();
            foreach(var enemy in world.enemies)enemy.ResetEnemy();GetComponent<RpgQuest08>().RefreshState();GetComponent<RpgQuest08>().giver?.GetComponent<RpgNpcDavi11>()?.ResetNpc();
            Save();SnapCamera();Notify("Seu Bonero espera ao sul da praça • [E] conversar • [C] carregar energia");
        }
        void Update()
        {
            if(!ready)return;
            if(Input.GetKeyDown(KeyCode.Escape)){if(mode==1){bufferedAttack=false;world.hero.CancelQueuedAttack();Save();mode=2;}else if(mode==2)mode=1;else if(mode==3)mode=0;}
            if(!Playing||reviewPose)return;if(Input.GetKeyDown(KeyCode.J)||Input.GetMouseButtonDown(0)&&!RpgInterface.PointerOverHud())bufferedAttack=true;if(impactPause>0){impactPause=Mathf.Max(0,impactPause-Time.unscaledDeltaTime);return;}float dt=Time.deltaTime;
            var hero=world.hero;dashCooldown=Mathf.Max(0,dashCooldown-dt);waveCooldown=Mathf.Max(0,waveCooldown-dt);boltCooldown=Mathf.Max(0,boltCooldown-dt);
            if(hero.dead){bufferedAttack=false;if(Input.GetKeyDown(KeyCode.R))Revive();}
            else
            {
                HandleTransformInput(Input.GetKey(KeyCode.C)||RpgInterface.ChargeButtonHeld());
                Vector2 input=new(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));
                bool run=Input.GetKey(KeyCode.LeftShift);hero.SetFlightControls(run,Input.GetKey(KeyCode.LeftControl)||Input.GetKey(KeyCode.RightControl));float speed=hero.flying?hero.FlightSpeed:hero.GroundSpeed(run);if(Input.GetKeyDown(KeyCode.LeftAlt)||Input.GetKeyDown(KeyCode.RightAlt)||Input.GetMouseButtonDown(1)&&!RpgInterface.PointerOverHud())hero.TryDodge(input);hero.Walk(input,speed,dt,run);
                if(bufferedAttack){hero.RequestAttack();bufferedAttack=false;}
                if(Input.GetKeyDown(KeyCode.Space))hero.Jump();
                if(Input.GetKeyDown(KeyCode.E))Interact();if(Input.GetKeyDown(KeyCode.Alpha1))Dash();if(Input.GetKeyDown(KeyCode.Alpha2))Wave();if(Input.GetKeyDown(KeyCode.Alpha3))Bolt();
            }
            hero.Tick(dt);foreach(var enemy in world.enemies)enemy.Tick(dt);
            saveTimer+=dt;if(saveTimer>12){saveTimer=0;Save();}floating.RemoveAll(t=>Time.unscaledTime-t.born>1.15f);
        }
        void LateUpdate()
        {
            if(!lens||!ready)return;
            lens.orthographicSize=Mathf.Lerp(lens.orthographicSize,world.hero.transforming>0?(world.hero.FormStage==2?5.95f:6.28f):world.hero.SonicActive?9.4f:7.03125f,Time.deltaTime*(world.hero.SonicActive?10:5));
            Vector2 target=world.hero.Position+Vector2.up*(.8f+world.hero.flightHeight*.35f)+(world.hero.flying?Vector2.ClampMagnitude(world.hero.velocity*.15f,world.hero.SonicActive?3.5f:2):Vector2.zero);
            float h=lens.orthographicSize,w=h*lens.aspect;
            target.x=Mathf.Clamp(target.x,world.bounds.xMin+w,world.bounds.xMax-w);target.y=Mathf.Clamp(target.y,world.bounds.yMin+h,world.bounds.yMax-h);
            var desired=new Vector3(target.x,target.y,-10);var p=Vector3.SmoothDamp(lens.transform.position,desired,ref followVelocity,world.hero.SonicActive?.065f:.16f);
            if(shake>0){shake=Mathf.Max(0,shake-Time.deltaTime*2);p+=(Vector3)(UnityEngine.Random.insideUnitCircle*shake*.11f);}
            lens.transform.position=new Vector3(Mathf.Round(p.x*32)/32,Mathf.Round(p.y*32)/32,-10);
        }
        public void SnapCamera(){if(lens)lens.transform.position=new Vector3(world.hero.Position.x,world.hero.Position.y+.8f,-10);}
        public bool Interact()
        {
            if(world.hero.dead||world.hero.attackTimer>0||world.hero.transforming>0||world.hero.Dodging)return false;if(GetComponent<RpgQuest08>().TryInteract())return true;foreach(var cache in UnityEngine.Object.FindObjectsByType<RpgCache14>())if(cache.Interact())return true;var s=world.NearestShrine(world.hero.Position);if(!s){Notify("Aproxime-se de um altar e pressione [E].");return false;}
            if(progress.Has(s.id)){Notify(s.title+" já faz parte de você.");return false;}
            progress.perks|=1<<s.id;Save();Notify("PODER DESPERTO  •  "+s.title);RpgEffects.Burst(world.hero.Position,s.color,3,1);RpgAudio.Play("item_gem_01",world.hero.Position,.5f);shake=.4f;return true;
        }
        // Retained entry point for existing HUD/review bindings; the ability is continuous flight.
        public bool Dash()=>ToggleFlight();
        public bool ToggleFlight()
        {
            var h=world.hero;if(!progress.Has(0)){Notify("Encontre o altar do Voo da Centelha [E].");return false;}
            if(dashCooldown>0)return false;if(!h.ToggleFlight())return false;dashCooldown=.35f;Notify(h.flying?"VOO ATIVO | [1] ou Espaco para pousar":"Pousando...");return true;
        }
        public bool Wave()
        {
            var h=world.hero;if(!progress.Has(2)){Notify("O altar da Liberdade guarda esse poder [E].");return false;}
            if(h.Dodging||h.dead||h.charging||h.transforming>0||h.attackTimer>0||h.powerPose>0||h.energy<40||waveCooldown>0)return false;
            bool power=h.buff>0;h.energy-=40;waveCooldown=power?4:5;h.Cast(power?.72f:.55f,power?.18f:.12f);
            StartCoroutine(LibertyPulse(h.Position,power,h.flightHeight,h.Damage*(h.FormStage==2?2.8f:power?2.4f:2.2f),session,h.FormStage));return true;
        }
        IEnumerator LibertyPulse(Vector2 origin,bool power,float lift,float damage,int startedSession,int stage)
        {
            // Wind-up is visible; damage and impact sound begin at the release pose.
            RpgAudio.Play("wave_windup",origin,.38f);yield return ActiveDelay(power?.18f:.12f,startedSession);
            if(world.hero.dead||session!=startedSession||!Playing)yield break;float radius=stage==2?6.2f:power?5.4f:3.8f;var tint=RpgArt.Hex(stage==2?"A0D8FF":power?"FFE3A1":"BCB5FF");
            RpgEffects.Burst(origin+Vector2.up*lift,tint,radius,power?.85f:.65f);if(power)RpgEffects.PowerStrike(origin+Vector2.up*lift,world.hero.facing,2,tint);
            RpgAudio.Play(power?"wave_awakened":"wave_blast",origin,power?.8f:.65f);shake=Mathf.Max(shake,power?1.5f:.65f);
            foreach(var e in world.enemies)if(!e.dead&&Vector2.Distance(origin,e.Position)<radius){e.TakeDamage(damage);Vector2 away=(e.Position-origin).normalized;e.transform.position=world.Move(e.Position,away*(power?1.5f:.45f));e.enemyCooldown=Mathf.Max(e.enemyCooldown,power?1.6f:.7f);}
            if(power){yield return ActiveDelay(.16f,startedSession);if(world.hero.dead||session!=startedSession||!Playing)yield break;RpgEffects.Burst(origin+Vector2.up*lift,tint,3.5f,.55f);}
        }
        IEnumerator ActiveDelay(float seconds,int startedSession)
        {
            while(seconds>0&&session==startedSession&&!world.hero.dead){if(mode==0||mode==3)yield break;if(Playing)seconds-=Time.deltaTime;yield return null;}
        }
        public bool Bolt()
        {
            var h=world.hero;if(progress.questStage!=3){Notify("Conclua a missão de Seu Bonero para aprender a Palma da Centelha.");return false;}
            if(h.Dodging||h.dead||h.charging||h.transforming>0||h.attackTimer>0||h.powerPose>0||h.energy<22||boltCooldown>0)return false;
            h.energy-=22;boltCooldown=h.FormStage==2?1.1f:1.8f;h.Cast(.32f);RpgProjectile.FireHero(h,h.facing);return true;
        }
        public void HandleTransformInput(bool held)
        {
            var h=world.hero;bool pressed=held&&!transformInputHeld;transformInputHeld=held;
            if(h.FormStage>0){if(pressed&&h.FormStage==1)TryAscend();return;}
            SetChargeHeld(held);
        }
        public bool TryAscend()
        {
            var h=world.hero;if(h.dead||h.FormStage!=1||h.transforming>0||h.attackTimer>0||h.powerPose>0)return false;
            if(h.energy<25){Notify("A evolução precisa de 25 de energia.");return false;}
            h.energy-=25;h.charge=0;h.charging=false;h.formStage=2;h.buff=16;h.transforming=1.7f;h.invincible=1.8f;h.velocity=Vector2.zero;h.ResetSonic();h.Pose("spellcast",2);h.GetComponent<RpgPowerForm>().Awaken();RpgAudio.Play("ascend_ignite",h.Position,.82f);GetComponent<RpgFeedback09>().Transformation();return true;
        }
        public void SetChargeHeld(bool held)
        {
            var h=world.hero;bool wasHeld=h.charging;
            if(held&&h.flying)h.Land();
            if(held&&h.flightHeight<=0&&!h.Dodging&&!h.dead&&h.attackTimer<=0&&h.jumpTimer<=0&&h.transforming<=0&&h.powerPose<=0)h.charging=true;
            else if(!held){h.charging=false;if(wasHeld||h.charge>0)ReleaseCharge();}
        }
        public void ReleaseCharge()
        {
            var h=world.hero;if(h.dead){h.charge=0;h.charging=false;return;}
            if(h.charge>=99){h.formStage=1;h.buff=progress.Has(1)?24:16;h.transforming=1.15f;h.invincible=1.25f;h.attackTimer=0;h.velocity=Vector2.zero;h.Pose("spellcast",2);shake=1;Notify("Aura Canarinha ativa. Pressione C novamente para evoluir.");h.GetComponent<RpgPowerForm>().Awaken();RpgAudio.Play("transform_ignite",h.Position,.85f);}
            else if(h.charge>5)Notify("Energia recuperada. Para transformar, complete a carga e solte [C].");h.charge=0;h.charging=false;
        }
        public void EnemyDefeated(RpgActor victim=null)
        {
            if(victim&&victim.enemyKind==4)GetComponent<RpgQuest08>().CaptainDefeated();progress.defeated++;progress.xp+=victim&&victim.enemyKind==4?100:25;
            while(progress.xp>=progress.NextLevel){progress.xp-=progress.NextLevel;progress.level++;world.hero.health=world.hero.MaxHealth;Notify("NÍVEL "+progress.level+"  •  mais vida e força");RpgEffects.Burst(world.hero.Position,RpgArt.Hex("80E1BF"),3,1);RpgAudio.Play("handleCoins",world.hero.Position,.35f);}
            Save();
        }
        public void Revive(){var h=world.hero;h.ResetMotion();h.transform.position=new Vector2(0,-3);h.dead=false;h.deadTime=0;h.health=h.MaxHealth;h.energy=100;h.invincible=2;h.body.color=Color.white;SnapCamera();Save();}
        public void Save(){if(reviewRun||!ready||mode==0||mode==3)return;progress.x=world.hero.Position.x;progress.y=world.hero.Position.y;PlayerPrefs.SetString(SaveKey,JsonUtility.ToJson(progress));PlayerPrefs.Save();}
        public static RpgProgress Load()
        {
            try{var p=JsonUtility.FromJson<RpgProgress>(PlayerPrefs.GetString(SaveKey,""));if(p!=null&&p.version==3&&p.level>=1&&p.level<1000&&!float.IsNaN(p.x)&&!float.IsNaN(p.y)&&!float.IsInfinity(p.x)&&!float.IsInfinity(p.y)){p.perks&=7;p.xp=Mathf.Max(0,p.xp);p.questStage=Mathf.Clamp(p.questStage,0,3);p.silenced&=7;return p;}}catch(Exception){}return new RpgProgress();
        }
        void OnApplicationQuit(){if(mode==1||mode==2)Save();}
        public void Notify(string text){message=text;messageUntil=Time.unscaledTime+4.5f;}
        public void FloatText(Vector2 p,string text,Color color){floating.Add(new FloatingText{p=p,text=text,color=color,born=Time.unscaledTime});}
        IEnumerator ReviewEffects06(string folder)
        {
            Directory.CreateDirectory(folder);var sound=lens.gameObject.AddComponent<RpgAudioCapture>();yield return new WaitForSecondsRealtime(.8f);
            StartGame(false);reviewPose=true;GetComponent<RpgVerification>().RunChecks(folder);
            foreach(var fx in UnityEngine.Object.FindObjectsByType<RpgEffects>())Destroy(fx.gameObject);
            foreach(var projectile in UnityEngine.Object.FindObjectsByType<RpgProjectile>())Destroy(projectile.gameObject);
            foreach(var particles in UnityEngine.Object.FindObjectsByType<ParticleSystem>())particles.Clear();
            yield return new WaitForSecondsRealtime(.4f);StartGame(false);reviewPose=true;var h=world.hero;h.transform.position=new Vector2(0,-3);SnapCamera();
            for(int n=0;n<3;n++){world.enemies[n].transform.position=new Vector2(-4+n*4,1);world.enemies[n].direction=2;world.enemies[n].Pose("idle",0);}
            messageUntil=0;yield return new WaitForSecondsRealtime(.3f);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"01_Inimigos.png"));yield return new WaitForSecondsRealtime(.1f);
            for(int i=0;i<14;i++){h.Walk(Vector2.right,2.8f,.04f);h.Tick(.04f);yield return new WaitForSecondsRealtime(.04f);}h.transform.position=new Vector2(0,-3);h.velocity=Vector2.zero;h.direction=2;h.facing=Vector2.down;SnapCamera();
            progress.perks=0;SetChargeHeld(true);
            for(int i=0;i<55;i++){h.Tick(.064f);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"Charge_"+i.ToString("D2")+".png"));yield return new WaitForSecondsRealtime(.064f);}
            SetChargeHeld(false);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"02_Transformacao.png"));yield return new WaitForSecondsRealtime(.1f);
            for(int i=0;i<32;i++){h.Tick(.05f);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"Gold_"+i.ToString("D2")+".png"));yield return new WaitForSecondsRealtime(.05f);}
            foreach(int d in new[]{0,1,2,3}){h.direction=d;h.Pose("idle",0);yield return new WaitForSecondsRealtime(.12f);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"Cabelo_"+d+".png"));yield return new WaitForSecondsRealtime(.1f);}
            h.transforming=h.attackTimer=h.powerPose=0;
            foreach(int d in new[]{0,1,2,3})foreach(string action in new[]{"walk","run","jump"})
            {
                int count=RpgArt.FrameCount(h.Family,action);h.direction=d;
                for(int frame=action=="walk"?1:0;frame<count;frame++){h.Pose(action,frame);yield return new WaitForSecondsRealtime(.04f);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"Awakened_"+action+"_"+d+"_"+frame.ToString("D2")+".png"));}
            }
            h.buff=0;h.transforming=0;h.direction=2;h.facing=Vector2.down;h.attackTimer=0;h.powerPose=0;
            world.enemies[0].transform.position=h.Position+Vector2.down*.9f;
            for(int strike=0;strike<3;strike++){var target=world.enemies[0];target.dead=false;target.health=target.MaxHealth;target.invincible=0;target.transform.position=h.Position+Vector2.down*.9f;h.Attack();for(int i=0;i<14;i++){h.Tick(.05f);if(world.enemies[0].dead)world.enemies[0].Tick(.05f);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"Combo_"+(strike*14+i).ToString("D2")+".png"));yield return new WaitForSecondsRealtime(.05f);}}
            h.buff=12;h.transforming=0;h.attackTimer=0;
            for(int strike=0;strike<3;strike++){var e=world.enemies[0];e.dead=false;e.health=e.MaxHealth;e.invincible=0;e.transform.position=h.Position+Vector2.down*.9f;h.Attack();for(int i=0;i<14;i++){h.Tick(.05f);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"Powered_"+(strike*14+i).ToString("D2")+".png"));yield return new WaitForSecondsRealtime(.05f);}}
            progress.perks=7;h.energy=100;h.facing=Vector2.right;dashCooldown=0;Dash();SnapCamera();
            for(int i=0;i<12;i++){ScreenCapture.CaptureScreenshot(Path.Combine(folder,"Dash_"+i.ToString("D2")+".png"));yield return new WaitForSecondsRealtime(.05f);}
            h.energy=100;waveCooldown=0;Wave();for(int i=0;i<14;i++){h.Tick(.05f);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"Power_"+i.ToString("D2")+".png"));yield return new WaitForSecondsRealtime(.05f);}
            h.buff=h.transforming=0;h.Pose("idle",0);mode=0;yield return new WaitForSecondsRealtime(.2f);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"Menu06.png"));yield return new WaitForSecondsRealtime(.15f);mode=1;float size=lens.orthographicSize;lens.orthographicSize=19;lens.transform.position=new Vector3(0,0,-10);enabled=false;yield return new WaitForSecondsRealtime(.15f);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"Mapa06.png"));yield return new WaitForSecondsRealtime(.15f);lens.orthographicSize=size;enabled=true;
            sound.Save(Path.Combine(folder,"Cibio_Som_06.wav"));File.WriteAllText(Path.Combine(folder,"Audio-Capture.txt"),"Mixed output peak: "+sound.peak+"; played events: "+RpgAudio.Instance.played);Debug.Log("CIBIO_06_REVIEW_OK audioPeak="+sound.peak);
            yield return new WaitForSecondsRealtime(.3f);Application.Quit();
        }
        IEnumerator Review(string folder)
        {
            Directory.CreateDirectory(folder);yield return new WaitForSecondsRealtime(1);
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,"01_Menu.png"));yield return new WaitForSecondsRealtime(.4f);
            for(int i=0;i<14;i++){ScreenCapture.CaptureScreenshot(Path.Combine(folder,"Menu_"+i.ToString("D2")+".png"));yield return new WaitForSecondsRealtime(.12f);}
            StartGame(false);yield return new WaitForSecondsRealtime(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"02_Vila.png"));yield return new WaitForSecondsRealtime(.4f);
            GetComponent<RpgVerification>().RunChecks(folder);
            world.hero.transform.position=new Vector2(3,-3);world.hero.direction=2;SnapCamera();reviewPose=true;
            foreach(string action in new[]{"walk","run","jump","slash"})for(int i=0;i<(action=="walk"?9:action=="run"?8:action=="jump"?5:6);i++){world.hero.Pose(action,i);ScreenCapture.CaptureScreenshot(Path.Combine(folder,action+"_"+i.ToString("D2")+".png"));yield return new WaitForSecondsRealtime(.1f);}
            world.hero.charging=true;world.hero.charge=15;mode=1;reviewPose=true;
            // Review poses are stepped manually while input and enemies are paused.
            for(int i=0;i<28;i++){world.hero.Tick(.08f);RpgEffects.Burst(world.hero.Position,RpgArt.Hex("FFD675"),.8f,.4f);world.hero.Pose("spellcast",i%7);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"Charge_"+i.ToString("D2")+".png"));yield return new WaitForSecondsRealtime(.07f);}
            world.hero.charging=false;world.hero.charge=100;ReleaseCharge();mode=1;world.hero.Pose("idle",0);
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,"03_Aura.png"));yield return new WaitForSecondsRealtime(.4f);
            mode=0;ScreenCapture.CaptureScreenshot(Path.Combine(folder,"04_Menu_Final.png"));yield return new WaitForSecondsRealtime(.3f);Application.Quit();
        }
    }
}





