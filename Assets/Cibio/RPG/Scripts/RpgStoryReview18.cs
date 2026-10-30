using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Cibio.RPG
{
    public sealed class RpgStoryReview18:MonoBehaviour
    {
        RpgGame g;RpgActor h;RpgQuest08 q;string folder;readonly List<string> checks=new();public int Failures{get;private set;}
        void Check(bool ok,string text){if(!ok)Failures++;checks.Add((ok?"PASS ":"FAIL ")+text);File.WriteAllLines(Path.Combine(folder,"Historia18.txt"),checks);}
        int TotalXp(){int xp=g.progress.xp;for(int level=1;level<g.progress.level;level++)xp+=level*50;return xp;}
        void Reset(){g.StartGame(false);g.reviewPose=true;h=g.world.hero;q=g.GetComponent<RpgQuest08>();foreach(var e in g.world.enemies)e.transform.position=new Vector2(30,70);}
        void Tests()
        {
            Reset();Check(q.nodes.Length==3&&g.world.enemies.Count==22,"Tres postos e22inimigos da cena anterior preservados");
            foreach(var node in q.nodes)
            {
                Check(node.EvidenceSprite&&node.EvidenceSprite.sprite.texture.name==RpgStory18.Assets[node.id],"Posto usa asset proprio de investigacao: "+node.id);
                Check(node.EvidenceSprite.sprite.pixelsPerUnit==192&&node.gameObject.name==RpgStory18.Stations[node.id],"Escala e nome publico do posto corretos: "+node.id);
            }
            h.transform.position=q.nodes[0].transform.position;q.TryInteract();Check(g.progress.silenced==0,"Investigacao exige receber a missao primeiro");
            h.transform.position=q.giver.position;g.Interact();Check(g.progress.questStage==1&&g.message==RpgStory18.Accept,"Seu Bonero apresenta investigacao e antagonista corretos");
            var node0=q.nodes.First(n=>n.id==0);h.transform.position=node0.transform.position;h.flightHeight=2;h.flying=true;q.TryInteract();Check(g.progress.silenced==0,"Voo impede recolher prova ate pousar");h.flightHeight=0;h.flying=false;
            var guard=g.world.enemies[0];guard.transform.position=node0.transform.position+Vector3.right;guard.dead=false;q.TryInteract();Check(g.progress.silenced==0,"Agente proximo protege o posto");guard.transform.position=new Vector2(30,70);
            foreach(var node in q.nodes.OrderBy(n=>n.id))
            {
                h.transform.position=node.transform.position;q.TryInteract();Check(RpgStory18.Has(g.progress,node.id)&&q.SilencedCount==node.id+1,"Recolha real registra prova e contador: "+node.id);
                int bits=g.progress.silenced,xp=TotalXp();q.TryInteract();Check(bits==g.progress.silenced&&xp==TotalXp(),"Investigar de novo nao duplica prova nem XP: "+node.id);
            }
            Check(g.progress.questStage==1&&g.progress.silenced==7,"Tres provas ainda exigem confrontar Companheiro");
            var boss=g.world.enemies.First(e=>e.enemyKind==4);boss.dead=false;boss.health=1;boss.invincible=0;boss.TakeDamage(10);
            Check(g.progress.captainDefeated&&g.progress.questStage==2,"Derrota real de Companheiro abre entrega do dossie");
            int before=TotalXp();h.transform.position=q.giver.position;g.Interact();Check(g.progress.questStage==3&&TotalXp()==before+100&&g.progress.Has(0)&&g.progress.Has(1),"Entrega conserva100XP e desbloqueios de voo/aura/Palma");
            before=TotalXp();g.Interact();Check(TotalXp()==before&&g.message==RpgStory18.Delivered,"Conversa pos-missao usa novo desfecho sem recompensa repetida");
            string roundTrip=JsonUtility.ToJson(g.progress);var restored=JsonUtility.FromJson<RpgProgress>(roundTrip);Check(restored.questStage==3&&RpgStory18.Count(restored)==3&&restored.captainDefeated,"Provas e conclusao sobrevivem serializacao v3");
            foreach(int mask in new[]{0,1,3,7})
            {
                var legacy=JsonUtility.FromJson<RpgProgress>("{\"version\":3,\"level\":8,\"questStage\":1,\"silenced\":"+mask+",\"perks\":7,\"talentRanks\":3}");
                Check(legacy.level==8&&legacy.perks==7&&legacy.talentRanks==3&&RpgStory18.Count(legacy)==(mask==0?0:mask==1?1:mask==3?2:3),"Save legado reaproveita mascara sem perder nivel/poder/talento: "+mask);
            }
            var ui=g.GetComponent<RpgInterface>();Check(ui.ToggleDossier()&&g.mode==5&&!g.Playing,"Dossie abre pelo controlador compartilhado e pausa mundo");Vector2 position=h.Position;float clock=h.clock;g.TickWorld(2);Check(h.Position==position&&h.clock==clock,"Dossie aberto impede atualizacao do combate");
            Check(RpgInterface.PointerOverHudAt(new Vector2(700,400)),"Clique no dossie nao vaza para soco no mundo");Check(ui.ToggleDossier()&&g.mode==1&&g.Playing,"Fechar dossie retoma jogo");
            g.mode=0;Check(!ui.ToggleDossier()&&g.mode==0,"Dossie nao abre sobre menu inicial");g.mode=1;g.Secret.BeginRevelation();Check(!ui.ToggleDossier(),"Dossie nao interrompe revelacao cinematografica");
            Reset();Check(RpgIsland14.Region(new Vector2(0,25))==RpgStory18.District&&RpgIsland14.Region(new Vector2(0,-3))==RpgStory18.Home,"Regioes publicas acompanham a nova narrativa");
            Check(RpgStory18.EnemyName(4)=="Companheiro"&&RpgStory18.EnemyName(5)=="Bonero Companheiro","Chefes mantem identidades pedidas pelo usuario");
        }
        void Native(string name)
        {
            var cam=g.lens;var prior=cam.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1280,720,24);var image=new Texture2D(1280,720,TextureFormat.RGB24,false);
            try{cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());}
            finally{cam.targetTexture=prior;RenderTexture.active=active;Destroy(rt);Destroy(image);}
        }
        public IEnumerator Run(string path)
        {
            folder=path;g=GetComponent<RpgGame>();bool had=PlayerPrefs.HasKey(RpgGame.SaveKey);string old=PlayerPrefs.GetString(RpgGame.SaveKey,"");
            try{Tests();}catch(Exception ex){Check(false,"Excecao: "+ex);Debug.LogException(ex);}
            Reset();g.progress.questStage=1;g.progress.silenced=7;var ui=g.GetComponent<RpgInterface>();ui.ToggleDossier();yield return new WaitForSecondsRealtime(.3f);
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,"Dossie18-Janela.png"));yield return new WaitForSecondsRealtime(.2f);
            if(ui.DossierDraws>0)Check(ui.DossierTextFits,"Metricas reais da fonte cabem nos blocos do dossie");
            File.WriteAllText(Path.Combine(folder,"UI18.txt"),"Dossier OnGUI draws="+ui.DossierDraws+"; font bounds fit="+ui.DossierTextFits+". Window screenshot requires separate visual verification.\n");ui.ToggleDossier();
            foreach(var node in q.nodes.OrderBy(n=>n.id))
            {
                h.transform.position=(Vector2)node.transform.position+Vector2.down*2;h.direction=0;h.facing=Vector2.up;h.Pose("idle",0);g.lens.transform.position=new Vector3(node.transform.position.x,node.transform.position.y+1.0f,-10);g.lens.orthographicSize=4.1f;
                yield return null;Native("Posto18_"+node.id);
            }
            File.WriteAllText(Path.Combine(folder,"Roteiro18.txt"),RpgStory18.Genre+"\n"+RpgStory18.Premise+"\n\n"+string.Join("\n\n",RpgStory18.Findings));
            Check(had==PlayerPrefs.HasKey(RpgGame.SaveKey)&&old==PlayerPrefs.GetString(RpgGame.SaveKey,""),"Save real preservado durante reformulacao e capturas18");
            Debug.Log("CIBIO_18_REVIEW_"+(Failures==0?"OK":"FAIL")+" checks="+checks.Count+" failures="+Failures);
        }
    }
}
