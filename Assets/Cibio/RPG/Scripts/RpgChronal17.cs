using UnityEngine;

namespace Cibio.RPG
{
    public sealed class RpgChronal17:MonoBehaviour
    {
        RpgGame game;AudioSource bed,air;bool wasActive,prepared;float duck=1;
        public float Age{get;private set;}public float Impulse{get;private set;}
        public int Ruptures{get;private set;}public int Preparations{get;private set;}
        public bool Active=>game&&game.Secret&&game.Secret.Active&&game.Secret.Laps>=5;
        public float Strength=>Active?Mathf.Lerp(.66f,1,Mathf.Clamp01((game.Secret.FlightTier-5)/4)):0;
        public float BackgroundGain=>Active?Mathf.Lerp(.52f,.72f,1-Impulse):1;
        public float BedVolume=>bed?bed.volume:0;public float AirVolume=>air?air.volume:0;
        void Awake()
        {
            game=GetComponent<RpgGame>();bed=Loop("chronal_loop17");air=Loop("chronal_air17");
        }
        AudioSource Loop(string name)
        {
            var s=gameObject.AddComponent<AudioSource>();s.clip=RpgAudio.Clip(name);s.loop=true;s.volume=0;s.spatialBlend=0;s.priority=48;s.Play();return s;
        }
        public void OnLap(int lap,Vector2 at)
        {
            if(lap<5)return;if(lap==5)Age=0;Impulse=1;Ruptures++;
            string clip=lap==5?"chronal_entry17":lap==10?"chronal_reveal17":"chronal_lap17";
            RpgAudio.Play(clip,at,lap==5?.95f:.78f,lap==5||lap==10?1:Mathf.Lerp(.94f,1.16f,(lap-6)/3f));
            game.shake=Mathf.Max(game.shake,lap==5?1.8f:1.15f);
            // Existing generated particles carry the physical crack into the hero's wake.
            if(!game.reviewPose)RpgEnergyParticles.Eruption(at+Vector2.up*game.world.hero.flightHeight,RpgArt.Hex("B9EFFF"),lap==5?4:2.7f,lap==5?40:22);
        }
        public void Tick(float dt)
        {
            if(!game.Playing){bed.volume=air.volume=0;return;}bool active=Active;
            if(game.Secret.Active&&!prepared&&game.Secret.Laps==4&&game.Secret.LapProgress>=.6f)
            {
                prepared=true;Preparations++;RpgAudio.Play("chronal_rise17",game.world.hero.Position,.55f,2.6f);
            }
            if(active){Age+=dt;Impulse=Mathf.MoveTowards(Impulse,0,dt/.85f);}
            else
            {
                if(wasActive&&!game.Secret.Cinematic&&!game.Secret.InArena)RpgAudio.Play("chronal_exit17",game.world.hero.Position,.55f);
                Age=Impulse=0;if(!game.Secret.Active)prepared=false;
            }
            wasActive=active;duck=Mathf.MoveTowards(duck,Impulse>.5f?.48f:1,dt*2.5f);
            bed.volume=Mathf.MoveTowards(bed.volume,active?.23f*Strength*duck:0,dt*1.6f);
            air.volume=Mathf.MoveTowards(air.volume,active?.21f*Strength*duck:0,dt*1.6f);
            bed.pitch=Mathf.Lerp(.86f,1.14f,Strength);air.pitch=Mathf.Lerp(.8f,1.35f,Strength);
        }
        void Update(){if(game&&!game.Playing){bed.volume=air.volume=0;}}
        public void ResetSession(){Age=Impulse=0;Ruptures=Preparations=0;wasActive=prepared=false;duck=1;if(bed)bed.volume=0;if(air)air.volume=0;}
    }
}
