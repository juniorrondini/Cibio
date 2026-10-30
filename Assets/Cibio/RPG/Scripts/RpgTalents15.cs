using System.Collections;
using System.Linq;
using UnityEngine;

namespace Cibio.RPG
{
    public sealed class RpgTalents15 : MonoBehaviour
    {
        public static readonly string[] Names={"Punhos de Aço","Ritmo Perfeito","Último Acorde","Fôlego da Centelha","Reserva Viva","Pulso em Cadeia","Passo Leve","Caçador da Brisa","Contratempo"};
        public static readonly string[] Descriptions={
            "+8% de dano nos socos por ponto.",
            "+12% de dano no finalizador por ponto.",
            "O finalizador deixa um eco após 0,22 s: 40% do golpe em uma pequena área. Um eco por finalizador.",
            "+1,5 de energia por segundo, por ponto, quando está no chão sem carregar.",
            "−6% no custo dos poderes e +1,5 s de transformação por ponto.",
            "A Palma que acerta um inimigo salta para até dois alvos próximos, causando 35% do dano.",
            "−10% de energia e −9% na recarga da esquiva por ponto.",
            "+4% na corrida e +5% no voo por ponto.",
            "Evite um golpe nos primeiros 0,13 s da esquiva: +8 de energia e próximo soco com 75% de dano extra, por 4 s."};
        RpgGame game;int observedLevel,session;bool dodgeRewarded,echoQueued;float counterUntil;
        public bool CounterReady=>game&&game.world.hero.clock<counterUntil;
        public static int MaxRank(int id)=>id%3==2?1:3;
        public static int Rank(RpgProgress p,int id)=>p==null||id<0||id>=9?0:Mathf.Min(MaxRank(id),(p.talentRanks>>(id*2))&3);
        public static int Spent(RpgProgress p){int n=0;for(int i=0;i<9;i++)n+=Rank(p,i);return n;}
        public static int Available(RpgProgress p)=>Mathf.Max(0,p.level-1+Mathf.Clamp(p.bonusTalentPoints,0,1)-Spent(p));
        public static bool Prerequisite(RpgProgress p,int id)=>id%3==0||id%3==1&&Rank(p,id-1)>=2||id%3==2&&Rank(p,id-2)==3&&Rank(p,id-1)>=2;
        public static void Sanitize(RpgProgress p)
        {
            p.bonusTalentPoints=Mathf.Clamp(p.bonusTalentPoints,0,1);int packed=0;
            for(int i=0;i<9;i++)packed|=Rank(p,i)<<(i*2);p.talentRanks=packed;
            for(int i=8;i>=0;i--){while(Rank(p,i)>0&&(!Prerequisite(p,i)||Spent(p)>p.level-1+p.bonusTalentPoints))p.talentRanks-=1<<(i*2);}
            // Removing an overspent lower node can invalidate a descendant, so check again.
            for(int i=0;i<9;i++)if(!Prerequisite(p,i))p.talentRanks&=~(3<<(i*2));
        }
        public int R(int id)=>Rank(game.progress,id);
        public float MeleeBonus=>1+.08f*R(0);
        public float FinisherBonus=>1+.12f*R(1);
        public float EnergyRecovery=>5+1.5f*R(3);
        public float PowerCost(float value)=>value*(1-.06f*R(4));
        public float ExtraDuration=>1.5f*R(4);
        public float DodgeCost=>10*(1-.10f*R(6));
        public float DodgeCooldown=>.85f*(1-.09f*R(6));
        public float RunBonus=>1+.04f*R(7);
        public float FlightBonus=>1+.05f*R(7);
        void Awake(){game=GetComponent<RpgGame>();}
        public void ResetSession(){session++;StopAllCoroutines();counterUntil=0;dodgeRewarded=echoQueued=false;observedLevel=game.progress.level;}
        void Update()
        {
            if(!game.ready)return;
            if(!game.reviewRun&&Input.GetKeyDown(KeyCode.T))Toggle();
            if(game.Playing&&!game.reviewPose&&game.progress.level>observedLevel){int gained=game.progress.level-observedLevel;game.Notify("NÍVEL "+game.progress.level+" • +"+gained+" ponto(s) de talento • [T] desenvolver");RpgEffects.Burst(game.world.hero.Position,RpgArt.Hex("88E5BC"),2.3f,.55f);}
            observedLevel=game.progress.level;
        }
        public void Toggle()
        {
            if(game.mode==4){game.mode=1;return;}if(!game.Playing||game.world.hero.dead||game.GetComponent<RpgWorldSecret15>().Cinematic)return;
            game.world.hero.CancelQueuedAttack();game.Save();game.mode=4;
        }
        public bool Invest(int id)
        {
            if(id<0||id>=9||Available(game.progress)<=0||Rank(game.progress,id)>=MaxRank(id)||!Prerequisite(game.progress,id))return false;
            game.progress.talentRanks+=1<<(id*2);game.Save();RpgAudio.Play("item_gem_01",game.world.hero.Position,.4f);return true;
        }
        public bool CanRespec=>!game.GetComponent<RpgWorldSecret15>().InArena&&!game.world.hero.dead&&!game.world.enemies.Any(e=>e&&e.gameObject.activeInHierarchy&&!e.dead&&Vector2.Distance(e.Position,game.world.hero.Position)<6);
        public bool Respec(){if(!CanRespec)return false;game.progress.talentRanks=0;counterUntil=0;game.Save();return true;}
        public void OnDodge(){dodgeRewarded=false;}
        public void AvoidedDamage()
        {
            var h=game.world.hero;if(R(8)==0||dodgeRewarded||!h.Dodging||h.dodgeTimer<.07f)return;
            dodgeRewarded=true;counterUntil=h.clock+4;h.energy=Mathf.Min(100,h.energy+8);game.FloatText(h.Position+Vector2.up*2,"CONTRATEMPO",RpgArt.Hex("A5E8FF"));RpgAudio.Play("item_gem_01",h.Position,.4f,1.25f);
            RpgEffects.PowerStrike(h.Position,h.facing,0,RpgArt.Hex("B4EEFF"));
        }
        public float BeginStrike(){echoQueued=false;bool critical=CounterReady;counterUntil=0;return critical?1.75f:1;}
        public void MeleeHit(RpgActor target,float damage,int combo)
        {
            if(combo!=2||R(2)==0||echoQueued)return;echoQueued=true;StartCoroutine(Echo(target.Position,damage*.4f,session));
        }
        IEnumerator Echo(Vector2 at,float damage,int started)
        {
            float delay=.22f;while(delay>0){if(started!=session||game.mode==0||game.world.hero.dead)yield break;if(game.Playing)delay-=Time.deltaTime;yield return null;}
            RpgEffects.PowerStrike(at,Vector2.up,2,RpgArt.Hex("FFD99D"));RpgAudio.Play("power_hit2",at,.4f,.82f);
            foreach(var e in game.world.enemies.ToArray())if(e&&!e.dead&&Vector2.Distance(at,e.Position)<2.1f&&game.world.HasClearPath(at,e.Position))e.TakeDamage(damage);
        }
        public int PalmaHit(RpgActor target,float damage)
        {
            if(R(5)==0)return 0;int hits=0;
            foreach(var e in game.world.enemies.Where(e=>e&&e!=target&&!e.dead&&Vector2.Distance(e.Position,target.Position)<4&&game.world.HasClearPath(target.Position,e.Position)).OrderBy(e=>(e.Position-target.Position).sqrMagnitude).Take(2).ToArray())
            {
                float before=e.health;e.TakeDamage(damage*.35f);if(e.health>=before)continue;hits++;
                Vector2 d=e.Position-target.Position;RpgAtlasFx15.Emit("Vfx_Arc06",(e.Position+target.Position)*.5f+Vector2.up*.85f,new Vector2(.8f,d.magnitude+1),.3f,RpgArt.Hex("A0E8FF"),Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg-90);
                RpgPalmaImpact13.Emit(e.Position,.85f,RpgArt.Hex("B1E8FF"),1.4f);
            }
            return hits;
        }
    }
    public sealed class RpgAtlasFx15:MonoBehaviour
    {
        public string atlas;public float duration,age;SpriteRenderer image;Color tint;
        public static RpgAtlasFx15 Emit(string atlas,Vector2 position,Vector2 size,float duration,Color color,float angle=0)
        {
            var r=RpgEffects.Create("Centelha • "+atlas,null);r.transform.position=position;r.transform.localScale=new Vector3(size.x,size.y,1);r.transform.rotation=Quaternion.Euler(0,0,angle);r.sortingOrder=RpgWorld.Order(position.y)+4;
            var fx=r.gameObject.AddComponent<RpgAtlasFx15>();fx.atlas=atlas;fx.duration=duration;fx.image=r;fx.tint=color;fx.Paint();return fx;
        }
        void Paint(){var c=tint;c.a*=Mathf.Clamp01((1-age/duration)*2);RpgEffects.Paint(image,atlas,Mathf.Clamp(age/duration*15,0,15),c,1.15f);}
        void Update(){if(!RpgGame.Instance||!RpgGame.Instance.Playing)return;age+=Time.deltaTime;if(age>=duration){Destroy(gameObject);return;}Paint();}
    }
}
