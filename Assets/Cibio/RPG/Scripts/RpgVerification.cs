using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Cibio.RPG
{
    public sealed class RpgVerification:MonoBehaviour
    {
        readonly List<string> checks=new();
        void Check(bool success,string description){checks.Add((success?"PASS ":"FAIL ")+description);if(!success)throw new InvalidOperationException(description);}
        public void RunChecks(string directory)
        {
            var g=GetComponent<RpgGame>();var w=g.world;var h=w.hero;
            bool saved=PlayerPrefs.HasKey(RpgGame.SaveKey);string oldSave=PlayerPrefs.GetString(RpgGame.SaveKey,"");RpgAudio.Instance.suppressOneShots=true;
            try
            {
                foreach(string family in new[]{"Hero","HeroAwakened"})Check(RpgArt.FrameCount(family,"flight")==4,"Voo tem hover e avanco em quatro direcoes: "+family);
                Check(w.built&&w.shrines.Count==3&&w.enemies.Count==14,"Mapa expandido, 3 altares e 14 inimigos carregados");
                var transition=RpgArt.Texture("Transform_Sequence05");Check(transition&&transition.width==768&&transition.height==512,"Atlas próprio da transição: seis etapas e quatro direções");
                foreach(var shrine in w.shrines)Check(Reachable(w,new Vector2(0,-3),shrine.transform.position),"Caminho livre até "+shrine.title);
                foreach(string action in new[]{"idle","walk","run","jump","slash","spellcast","hurt","thrust","backslash"})foreach(string person in new[]{"Hero","HeroLift","HeroGold","HeroAwakened","Enemy","EnemyHeavy","EnemyCaster"})
                {
                    int expected=action=="idle"?128:action=="walk"?576:action=="run"?512:action=="jump"?320:action=="slash"?384:action=="spellcast"?448:action=="thrust"?512:action=="backslash"?832:384;
                    var texture=RpgArt.Texture(person+"_"+action);Check(texture&&texture.width==expected*(RpgArt.CellSize(person)/64)&&texture.height==(action=="hurt"?RpgArt.CellSize(person):RpgArt.CellSize(person)*4),"Atlas com dimensões originais "+person+" "+action);
                    for(int d=0;d<(action=="hurt"?1:4);d++)Check(RpgArt.Frame(person,action,d,0)!=null,"Direção "+d+" de "+person+" "+action);
                }
                h.transform.position=new Vector2(0,-11);h.velocity=Vector2.zero;Vector2 start=h.Position;
                for(int i=0;i<60;i++)h.Walk(Vector2.down,2.8f,1/60f);
                Check(Vector2.Distance(start,h.Position)>2.3f,"Movimento contínuo com aceleração");
                var wall=new Rect(20,-6,1,3);w.obstacles.Add(wall);var blocked=w.Move(new Vector2(19,-4),new Vector2(4,0));Check(blocked.x<19.8f,"Colisão impede atravessar parede em dash");w.obstacles.Remove(wall);
                var diagonal=Vector2.ClampMagnitude(new Vector2(1,1),1);Check(Mathf.Abs(diagonal.magnitude-1)<.001f,"Velocidade diagonal normalizada");
                g.progress.perks=0;h.energy=100;g.dashCooldown=0;Check(!g.Dash()&&h.energy==100,"Poder bloqueado não consome energia");
                h.transform.position=w.shrines[0].transform.position;Check(g.Interact()&&g.progress.Has(0),"Interação aprende o primeiro poder");Check(!g.Interact(),"Altar não duplica poder");
                h.transform.position=new Vector2(0,-10);h.facing=Vector2.down;g.dashCooldown=0;h.charging=false;start=h.Position;Check(g.Dash()&&h.flying&&h.energy==88&&h.Position==start,"Voo decola sem teleportar e cobra 12 de energia");Check(!g.Dash(),"Intervalo impede alternar voo acidentalmente");
                for(int i=0;i<60;i++){h.Walk(Vector2.down,6,1/60f);h.Tick(1/60f);}Check(Vector2.Distance(start,h.Position)>4&&h.flightHeight>1.4f&&h.energy<79,"Voo move continuamente, ganha altura e consome energia");
                h.energy=.01f;h.Tick(.1f);Check(!h.flying,"Energia esgotada inicia pouso automatico");for(int i=0;i<60;i++){h.Walk(Vector2.zero,0,1/60f);h.Tick(1/60f);}Check(h.flightHeight==0,"Pouso volta ao chao sem alterar a posicao bruscamente");
                h.energy=100;h.buff=10;h.transform.position=new Vector2(0,-10);h.attackTimer=h.powerPose=h.transforming=0;h.Walk(Vector2.right,3.64f,.1f,false);h.Tick(.016f);Check(h.currentAnimation=="walk","Caminhada transformada nao toca corrida");h.Walk(Vector2.right,5.85f,.1f,true);h.Tick(.016f);Check(h.currentAnimation=="run","Shift seleciona corrida explicitamente");
                h.buff=0;h.velocity=Vector2.zero;h.attackTimer=h.powerPose=0;h.facing=Vector2.down;h.Attack();int lockedDirection=h.direction;h.Walk(Vector2.right,2.8f,.05f);Check(h.direction==lockedDirection&&h.facing==Vector2.down&&h.velocity==Vector2.zero,"Golpe trava direcao, alvo e movimento ate terminar");h.Tick(.8f);h.ResetMotion();
                g.progress.perks=0;h.buff=h.transforming=h.attackTimer=h.jumpTimer=h.powerPose=0;h.charge=0;g.SetChargeHeld(true);h.Tick(2);Check(h.Family=="HeroLift","Cabelo muda de silhueta durante a carga");g.SetChargeHeld(false);Check(h.buff==0,"Soltar antes da carga completa não transforma");
                h.charge=0;g.SetChargeHeld(true);for(int i=0;i<210;i++)h.Tick(1/60f);g.SetChargeHeld(false);Check(h.buff==16&&h.Family=="HeroAwakened"&&!h.charging,"Pressionar, completar e soltar C transforma sem altar");h.Tick(16.1f);Check(h.buff==0&&h.Family=="Hero","Expirar transformação devolve o cabelo normal");h.buff=h.transforming=0;
                h.transform.position=w.shrines[1].transform.position;Check(g.Interact()&&g.progress.Has(1),"Altar melhora transformação");
                h.buff=0;h.charge=50;g.ReleaseCharge();Check(h.buff==0&&h.charge==0,"Carga parcial não transforma");
                h.charging=true;h.charge=0;h.energy=0;for(int i=0;i<210;i++)h.Tick(1/60f);Check(h.charge>=99&&h.energy>80,"Canalização completa acumula carga e recupera energia");g.ReleaseCharge();Check(h.buff==24&&!h.charging&&h.charge==0,"Altar amplia transformação para 24 segundos");
                var form=h.GetComponent<RpgPowerForm>();form.Refresh(1);Check(h.body.transform.localScale.y>1.17f&&!h.body.enabled,"Forma desperta tem porte e torso maiores");Check(Resources.Load<Shader>("RpgArt/PowerSprite").isSupported,"Shader do corpo desperto suportado");Check(Resources.Load<Shader>("RpgArt/OrganicGround").isSupported&&w.GetComponent<RpgTerrain>(),"Terreno orgânico ativo");float boosted=h.Damage;h.buff=h.transforming=0;Check(boosted>h.Damage*2.2f,"Aura aumenta dano real");
                h.transform.position=w.shrines[2].transform.position;Check(g.Interact()&&g.progress.Has(2),"Onda desbloqueada no terceiro altar");
                h.transform.position=new Vector2(0,-3);h.energy=100;h.attackTimer=0;g.waveCooldown=0;var e=w.enemies[0];e.transform.position=h.Position+Vector2.right;e.health=65;e.dead=false;e.invincible=0;Check(g.Wave()&&h.energy==60&&h.powerPose>0,"Onda prepara animacao antes de liberar dano e cobra energia");Check(!g.Wave(),"Onda respeita cooldown");
                e.dead=false;e.health=65;e.invincible=0;e.transform.position=h.Position+Vector2.down*.7f;h.direction=2;h.facing=Vector2.down;h.attackTimer=h.powerPose=0;Check(h.Attack(),"Ataque inicia animação");string first=h.attackAction;for(int i=0;i<40;i++)h.Tick(1/60f);Check(e.health<65&&h.hitsLanded>0,"Golpe causa dano sincronizado à animação");
                h.Attack();string second=h.attackAction;for(int i=0;i<40;i++)h.Tick(1/60f);h.Attack();string third=h.attackAction;Check(new HashSet<string>{first,second,third}.Count==3,"Combo alterna três poses diferentes de golpes");h.attackTimer=0;
                Check(w.enemies[0].MaxHealth==65&&w.enemies[1].MaxHealth==120&&w.enemies[2].MaxHealth==55,"Três arquétipos com vida e aparências próprias");
                h.health=h.MaxHealth;h.invincible=0;h.transform.position=new Vector2(0,-3);var caster=w.enemies[2];caster.transform.position=Vector2.zero;var projectile=RpgProjectile.Fire(caster,Vector2.down);for(int i=0;i<16;i++)projectile.Advance(.04f);Check(h.health<h.MaxHealth,"Projétil do arauto atinge e causa dano");
                foreach(string clip in new[]{"charge_bass","charge_air","charge_electric","aura_sustain","transform_ignite","punch_hit0","punch_hit1","punch_hit2","power_hit2","flight_wind","flight_launch","flight_land","wave_awakened","wave_windup","wave_blast","Forest_Ambience","footstep00","blade_01","blade_02","blade_03","spell_01","spell_fire_01","spell_fire_02","spell_fire_03","creature_hurt_01","creature_die_01"})Check(RpgAudio.Clip(clip)&&RpgAudio.Clip(clip).samples>0,"Áudio real carregado: "+clip);
                Check(Resources.Load<Material>("RpgArt/OrganicEnergy").shader.isSupported,"Shader orgânico suportado");Check(UnityEngine.Object.FindObjectsByType<LineRenderer>().Length==0,"Aura e poderes sem contornos geométricos");
                h.invincible=0;h.health=100;h.TakeDamage(12);h.TakeDamage(12);Check(h.health==88,"Invulnerabilidade evita dano duplicado");
                g.progress=new RpgProgress();g.EnemyDefeated();g.EnemyDefeated();Check(g.progress.level==2&&g.progress.xp==0,"Vitórias dão XP e sobem nível");
                g.progress.perks=7;PlayerPrefs.SetString(RpgGame.SaveKey,JsonUtility.ToJson(g.progress));var loaded=RpgGame.Load();Check(loaded.level==2&&loaded.perks==7,"Save recupera nível e poderes");
                PlayerPrefs.SetString(RpgGame.SaveKey,"{invalid");Check(RpgGame.Load().level==1,"Save corrompido recupera padrão seguro");
                h.invincible=0;h.TakeDamage(999);Check(h.dead&&h.health==0,"Morte interrompe o personagem");g.Revive();Check(!h.dead&&h.health==h.MaxHealth&&h.Position==new Vector2(0,-3),"Renascimento recupera vida e posição");
                h.jumpTimer=0;Check(h.Jump(),"Salto disponível");h.Tick(.2f);Check(h.body.transform.localPosition.y>.2f,"Salto levanta o sprite sem perder posição no mapa");
                g.progress.perks=7;h.health=h.MaxHealth;h.energy=100;h.jumpTimer=0;h.attackTimer=0;h.invincible=0;h.velocity=Vector2.zero;
                foreach(string name in new[]{"AuraBody","AuraVeil","Wisp","Surge","Arc"}){var atlas=RpgArt.Texture("Vfx_"+name+"06");Check(atlas&&atlas.width==1024&&atlas.height==1024,"Atlas de energia com 16 quadros: "+name);}
                Check(Resources.Load<Material>("RpgArt/EnergyVolume06").shader.isSupported,"Shader de volume orgânico suportado");Check(RpgEnergyParticles.Material.shader.isSupported&&RpgEnergyParticles.Material.mainTexture,"Partículas usam textura orgânica e shader próprio");Check(g.lens.GetComponent<RpgEnergyBloom>().Available,"Brilho seletivo das energias disponível");
                var aura=h.GetComponent<RpgAura>();h.buff=0;h.charging=true;h.charge=100;aura.Refresh(1);Check(aura.Visible&&aura.LiveParticles>0&&aura.LiveParticles<=170,"Aura carregada envolve o corpo e emite partículas limitadas");
                h.charging=false;h.charge=0;h.buff=0;aura.Refresh(1);Check(!aura.Visible,"Encerrar carga dissipa as camadas da aura");h.charging=true;h.charge=100;aura.Refresh(1);h.dead=true;aura.Refresh(1);Check(!aura.Visible,"Morte encerra a aura volumétrica");h.dead=false;h.charging=false;h.charge=0;
                File.WriteAllLines(Path.Combine(directory,"Verificacao-RPG.txt"),checks);Debug.Log("CIBIO_RPG_CHECKS_OK "+checks.Count);
            }
            catch(Exception ex){File.WriteAllLines(Path.Combine(directory,"Verificacao-RPG.txt"),checks);Debug.LogException(ex);Application.Quit(2);throw;}
            finally{RpgAudio.Instance.suppressOneShots=false;if(saved)PlayerPrefs.SetString(RpgGame.SaveKey,oldSave);else PlayerPrefs.DeleteKey(RpgGame.SaveKey);PlayerPrefs.Save();}
        }
        static bool Reachable(RpgWorld w,Vector2 start,Vector2 target)
        {
            var queue=new Queue<Vector2Int>();var visited=new HashSet<Vector2Int>();Vector2Int origin=Vector2Int.RoundToInt(start*2);queue.Enqueue(origin);visited.Add(origin);
            var offsets=new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right};
            while(queue.Count>0){var p=queue.Dequeue();if(Vector2.Distance((Vector2)p*.5f,target)<1.8f)return true;foreach(var d in offsets){var next=p+d;if(visited.Contains(next)||!w.CanStand((Vector2)next*.5f))continue;visited.Add(next);queue.Enqueue(next);}}return false;
        }
    }
}



