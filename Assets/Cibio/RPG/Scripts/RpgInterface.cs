using UnityEngine;

namespace Cibio.RPG
{
    public sealed class RpgInterface:MonoBehaviour
    {
        RpgGame game;Font font;Texture2D gradient;Vector2 creditsScroll;bool help;int previousMode=-1;
        readonly Color ink=new(.035f,.075f,.09f,.95f),muted=RpgArt.Hex("9AACAB"),gold=RpgArt.Hex("F7D18A"),mint=RpgArt.Hex("71DAC4");
        void Awake(){game=GetComponent<RpgGame>();font=Resources.Load<Font>("RpgArt/PixelifySans");}
        void Update(){if(Input.GetKeyDown(KeyCode.H))help=!help;}
        public static bool PointerOverHud(){float y=Screen.height-Input.mousePosition.y;return y<100*Screen.height/810f||y>Screen.height-110*Screen.height/810f||Input.mousePosition.x>Screen.width*.78f;}
        void OnGUI()
        {
            if(!game||!game.ready)return;GUI.matrix=Matrix4x4.Scale(new Vector3(Screen.width/1440f,Screen.height/810f,1));GUI.depth=-20;
            if(previousMode!=game.mode){GUI.FocusControl(null);previousMode=game.mode;}
            if(game.mode==0||game.mode==3){Menu();return;}
            Hud();
            if(game.mode==2&&!game.reviewPose)Pause();
            if(help&&game.mode==1)Help();
        }
        GUIStyle TextStyle(int size,Color color,TextAnchor align=TextAnchor.UpperLeft)
        {
            return new GUIStyle(GUI.skin.label){font=font,fontSize=size,normal={textColor=QualitySettings.activeColorSpace==ColorSpace.Linear?color.gamma:color},alignment=align,richText=false,wordWrap=true,padding=new RectOffset(0,0,0,0)};
        }
        void Label(Rect r,string text,int size,Color color,TextAnchor anchor=TextAnchor.UpperLeft){GUI.Label(r,text,TextStyle(size,color,anchor));}
        static void Fill(Rect r,Color color){var old=GUI.color;GUI.color=color;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
        void Panel(Rect r,Color? border=null)
        {
            Fill(new Rect(r.x+4,r.y+5,r.width,r.height),new Color(0,0,0,.25f));Fill(r,border??new Color(.35f,.5f,.48f,.6f));Fill(new Rect(r.x+1,r.y+1,r.width-2,r.height-2),ink);
        }
        bool Button(Rect r,string text,bool primary=false,bool enabled=true)
        {
            bool over=r.Contains(Event.current.mousePosition);Color c=primary?gold:muted;
            Fill(new Rect(r.x,r.y+4,r.width,r.height),new Color(0,0,0,.3f));Fill(r,enabled?c:new Color(.25f,.31f,.3f));
            Fill(new Rect(r.x+2,r.y+2,r.width-4,r.height-4),primary?(over?RpgArt.Hex("FBE5AD"):gold):(over?RpgArt.Hex("23403E"):ink));
            Label(r,text,primary?27:21,primary?RpgArt.Hex("22352D"):enabled?Color.white:muted,TextAnchor.MiddleCenter);
            bool old=GUI.enabled;GUI.enabled=enabled;bool click=GUI.Button(r,"",GUIStyle.none);GUI.enabled=old;return click;
        }
        void Menu()
        {
            float t=Time.unscaledTime;var bg=RpgArt.Texture("Title_VilaDaAurora");float zoom=1.025f+Mathf.Sin(t*.13f)*.008f;
            GUI.DrawTexture(new Rect(720-720*zoom+Mathf.Sin(t*.12f)*5,405-405*zoom,1440*zoom,810*zoom),bg,ScaleMode.ScaleAndCrop);
            if(!gradient){gradient=new Texture2D(256,1);for(int x=0;x<256;x++){float a=Mathf.Pow(1-x/255f,2)*.61f;gradient.SetPixel(x,0,new Color(.01f,.045f,.07f,a));}gradient.Apply();}
            GUI.DrawTexture(new Rect(0,0,1100,810),gradient);
            for(int i=0;i<38;i++)
            {
                float x=(i*137.7f+Mathf.Sin(t*.17f+i)*34)%1440,y=810-((t*(10+i%5*3)+i*73)%840);
                float a=.25f+.35f*Mathf.Sin(t*1.3f+i)*Mathf.Sin(t*1.3f+i);Fill(new Rect(x,y,i%4==0?3:2,i%4==0?5:2),new Color(1,.76f,.35f,a));
            }
            Fill(new Rect(0,0,1440,4),new Color(gold.r,gold.g,gold.b,.7f));
            Label(new Rect(78,67,520,26),"UM RPG BRASILEIRO",18,mint);
            Label(new Rect(74,141,650,135),"CIBIO",118,gold);
            Fill(new Rect(82,285,56,3),gold);
            Label(new Rect(82,310,560,70),"O BRASIL VIROU\nUMA VUVUZELA",26,Color.white);
            Label(new Rect(82,396,475,44),"Encontre sua centelha. Recupere sua voz.",18,RpgArt.Hex("BCCAC2"));
            float pulse=.7f+Mathf.Sin(t*2)*.2f;Fill(new Rect(78,458,418,68),new Color(1,.75f,.3f,pulse*.25f));
            if(Button(new Rect(82,462,410,60),"JOGAR   >",true))game.StartGame(!game.HasSave?false:true);
            if(Button(new Rect(82,538,196,47),"Novo jogo"))game.StartGame(false);
            if(Button(new Rect(294,538,198,47),"Créditos"))game.mode=3;
            Label(new Rect(82,613,430,66),"Explore a vila. Desperte poderes.\nEnfrente os cobradores da Vuvuzela.",18,muted);
            Label(new Rect(80,763,650,24),"PIXEL RPG  /  VILA DA CENTELHA  /  PROTÓTIPO 08",14,RpgArt.Hex("AAC1B6"));
            Label(new Rect(1120,763,242,24),game.HasSave?"PROGRESSO SALVO  •  JOGAR CONTINUA":"SUA JORNADA COMEÇA AQUI",12,gold,TextAnchor.MiddleRight);
            if(game.mode==3)Credits();
        }
        void Credits()
        {
            Fill(new Rect(0,0,1440,810),new Color(0,.025f,.03f,.78f));Panel(new Rect(230,80,980,650),gold);
            Label(new Rect(270,108,800,45),"QUEM DEU VIDA À VILA",29,gold);
            var text=Resources.Load<TextAsset>("RpgCredits").text;
            creditsScroll=GUI.BeginScrollView(new Rect(270,171,900,440),creditsScroll,new Rect(0,0,860,TextStyle(19,Color.white).CalcHeight(new GUIContent(text),850)+20));
            Label(new Rect(0,0,850,1400),text,19,RpgArt.Hex("C8D8CF"));GUI.EndScrollView();
            if(Button(new Rect(270,649,210,46),"Voltar"))game.mode=0;
            if(Button(new Rect(760,649,410,46),"Fontes e licenças"))Application.OpenURL(new System.Uri(System.IO.Path.Combine(Application.streamingAssetsPath,"Cibio-Licenses","FONTES-E-LICENCAS.txt")).AbsoluteUri);
        }
        void Hud()
        {
            var h=game.world.hero;var p=game.progress;
            Fill(new Rect(0,0,1440,96),new Color(.025f,.06f,.075f,.94f));Fill(new Rect(0,95,1440,1),new Color(.47f,.64f,.53f,.4f));
            Panel(new Rect(22,15,60,65),gold);var avatar=RpgArt.Texture(h.buff>0?"HeroAwakened_idle":"Hero_idle");GUI.DrawTextureWithTexCoords(new Rect(19,9,68,68),avatar,new Rect(0,.25f,.5f,.25f));
            Label(new Rect(100,13,300,27),"BRASILEIRO",19,gold);Label(new Rect(355,14,95,25),"NV "+p.level.ToString("D2"),18,mint);
            Bar(new Rect(100,43,326,13),h.health/h.MaxHealth,RpgArt.Hex("D66C61"));Label(new Rect(440,39,110,20),Mathf.CeilToInt(h.health)+" / "+h.MaxHealth,13,Color.white);
            Bar(new Rect(100,65,326,9),h.energy/100,mint);Label(new Rect(440,60,110,20),"ENERGIA "+(int)h.energy,12,mint);
            Label(new Rect(600,18,360,26),game.world.hero.Position.y>18?"MORRO DO ECO":"VILA DA CENTELHA",20,RpgArt.Hex("D5DCC5"),TextAnchor.MiddleCenter);
            Label(new Rect(600,48,360,22),"EXP "+p.xp+" / "+p.NextLevel,12,muted,TextAnchor.MiddleCenter);Bar(new Rect(657,77,246,3),(float)p.xp/p.NextLevel,gold);
            Label(new Rect(1120,20,290,20),"[M] SOM  •  [H] AJUDA  •  [ESC] MENU",13,muted,TextAnchor.MiddleRight);
            Label(new Rect(1120,51,290,20),"PODERES "+Count(p.perks)+" / 3   •   VITÓRIAS "+p.defeated,13,gold,TextAnchor.MiddleRight);
            Panel(new Rect(1148,116,270,198));Label(new Rect(1170,133,232,24),"ALTARES DA CENTELHA",17,gold);
            string[] names={"Voo da Centelha","Coração Canarinho","Grito da Liberdade"};Color[] colors={mint,gold,RpgArt.Hex("BBA2F1")};
            for(int i=0;i<3;i++){Label(new Rect(1170,173+i*39,220,21),(p.Has(i)?"+  ":"·  ")+names[i],17,p.Has(i)?colors[i]:muted);Label(new Rect(1190,194+i*39,200,15),p.Has(i)?"DESPERTO":"ENCONTRE O ALTAR",10,p.Has(i)?colors[i]:muted);}
            Map(new Rect(1180,331,238,156));
            Panel(new Rect(14,608,325,141));Label(new Rect(24,621,300,20),"A VOZ DO MORRO",16,gold);
            Label(new Rect(24,650,300,91),game.GetComponent<RpgQuest08>().Objective,15,RpgArt.Hex("D0DACD"));
            string[] keys={"J","1","C","2","3"};string[] titles={"COMBO","VOAR / POUSAR","TRANSFORMAR","LIBERDADE","PALMA"};
            for(int i=0;i<5;i++)
            {
                Rect r=new(442+i*137,689,124,75);bool unlocked=i==0||i==2||(i==4?p.questStage==3:p.Has(i==1?0:2));Color c=i==2?gold:i==3?RpgArt.Hex("BBA2F1"):mint;
                Panel(r,unlocked?c:new Color(.26f,.35f,.34f));Label(new Rect(r.x+12,r.y+6,30,28),keys[i],24,unlocked?c:muted);Label(new Rect(r.x+8,r.y+48,110,18),titles[i],11,unlocked?Color.white:muted,TextAnchor.MiddleCenter);
                float cd=i==1?game.dashCooldown:i==3?game.waveCooldown:i==4?game.boltCooldown:0;
                Label(new Rect(r.x+50,r.y+8,63,25),!unlocked?"SELADO":i==1&&h.flying?"NO AR":cd>0?cd.ToString("0.0"):i==2?h.buff>0?((int)h.buff)+"s":"SEGURE":"PRONTO",11,unlocked?c:muted,TextAnchor.MiddleRight);
                if(GUI.Button(r,"",GUIStyle.none)&&game.Playing){if(i==0)h.Attack();if(i==1)game.Dash();if(i==2)game.Notify("Segure [C] para carregar; solte com a carga completa para transformar.");if(i==3)game.Wave();if(i==4)game.Bolt();}
            }
            Fill(new Rect(400,775,642,29),ink);Label(new Rect(422,781,600,19),h.flying?"WASD mover  •  SHIFT impulso  •  CTRL freio  •  ESPAÇO pousar":"WASD mover  •  SHIFT correr  •  ESPAÇO saltar  •  E interagir",12,muted,TextAnchor.MiddleCenter);
            var shrine=game.world.NearestShrine(h.Position);
            if(shrine&&game.Playing){Panel(new Rect(451,600,538,66),shrine.color);Label(new Rect(467,611,506,22),"[E]  "+shrine.title.ToUpper(),18,shrine.color,TextAnchor.MiddleCenter);Label(new Rect(467,639,506,18),p.Has(shrine.id)?"PODER JÁ DESPERTO":shrine.description,13,muted,TextAnchor.MiddleCenter);}
            if(h.charging){Panel(new Rect(522,537,396,49),gold);Label(new Rect(538,544,365,18),h.charge>=99?"CARGA COMPLETA  •  SOLTE [C]":"CANALIZANDO ENERGIA   "+(int)h.charge+"%",14,gold,TextAnchor.MiddleCenter);Bar(new Rect(540,570,360,5),h.charge/100,gold);}
            else if(h.buff>0)Label(new Rect(510,530,420,30),(h.FormStage==2?"CENTELHA RUBRA • ESTÁGIO 2 • ":"AURA CANARINHA • ESTÁGIO 1 • ")+Mathf.CeilToInt(h.buff)+"s",19,h.FormStage==2?RpgArt.Hex("FF7186"):gold,TextAnchor.MiddleCenter);
            foreach(var e in game.world.enemies)if(!e.dead&&Vector2.Distance(h.Position,e.Position)<6){Vector2 v=Project(e.Position+Vector2.up*1.7f);Bar(new Rect(v.x-25,v.y,50,4),e.health/e.MaxHealth,RpgArt.Hex("D97178"));Label(new Rect(v.x-90,v.y-23,180,20),e.EnemyName,11,e.enemyKind==1?gold:e.enemyKind==2?mint:Color.white,TextAnchor.MiddleCenter);}
            var quest=game.GetComponent<RpgQuest08>();if(quest.giver&&Vector2.Distance(h.Position,quest.giver.position)<6){var q=Project((Vector2)quest.giver.position+Vector2.up*1.9f);Label(new Rect(q.x-110,q.y-20,220,40),"SEU DAVI  •  [E] MISSÃO",15,gold,TextAnchor.MiddleCenter);}
            foreach(var n in quest.nodes)if(Vector2.Distance(h.Position,n.transform.position)<6){var q=Project((Vector2)n.transform.position+Vector2.up*2.2f);Label(new Rect(q.x-120,q.y-20,240,42),(p.silenced&(1<<n.id))==0?"[E] AMPLIFICADOR "+(n.id+1):"SILENCIADO",14,mint,TextAnchor.MiddleCenter);}
            foreach(var f in game.floating){float age=Time.unscaledTime-f.born;Vector2 v=Project(f.p+Vector2.up*age*.5f);var c=f.color;c.a=1-age/1.15f;Label(new Rect(v.x-40,v.y-15,80,34),f.text,23,c,TextAnchor.MiddleCenter);}
            if(Time.unscaledTime<game.messageUntil){Panel(new Rect(350,112,740,77),new Color(gold.r,gold.g,gold.b,.7f));Label(new Rect(366,124,708,60),game.message,17,gold,TextAnchor.MiddleCenter);}
            if(h.dead){Fill(new Rect(0,0,1440,810),new Color(.1f,.01f,.02f,.65f));Label(new Rect(420,310,600,90),"SUA VOZ AINDA ECOA",34,gold,TextAnchor.MiddleCenter);if(Button(new Rect(510,422,420,60),"[R] VOLTAR À PRAÇA",true))game.Revive();}
        }
        Vector2 Project(Vector2 p){var v=game.lens.WorldToScreenPoint(p);return new Vector2(v.x/Screen.width*1440,(1-v.y/Screen.height)*810);}
        void Bar(Rect r,float value,Color color){Fill(r,new Color(0,0,0,.6f));Fill(new Rect(r.x+1,r.y+1,(r.width-2)*Mathf.Clamp01(value),r.height-2),color);}
        int Count(int bits){int c=0;for(int i=0;i<3;i++)if((bits&(1<<i))!=0)c++;return c;}
        void Map(Rect r)
        {
            Panel(r);Label(new Rect(r.x+11,r.y+8,220,20),"ARREDORES",12,muted);
            Rect m=new(r.x+11,r.y+34,r.width-22,r.height-45);Fill(m,RpgArt.Hex("1F3832"));
            var bounds=game.world.bounds;Vector2 Point(Vector2 p)=>new(m.x+(p.x-bounds.xMin)/bounds.width*m.width,m.y+(bounds.yMax-p.y)/bounds.height*m.height);
            foreach(var b in game.world.obstacles){Vector2 v=Point(new Vector2(b.x,b.y+b.height));Fill(new Rect(v.x,v.y,b.width/bounds.width*m.width,b.height/bounds.height*m.height),RpgArt.Hex("52614A"));}
            foreach(var solid in game.world.solids)if(solid&&solid.enabled){var b=solid.Bounds;Vector2 v=Point(new Vector2(b.x,b.y+b.height));Fill(new Rect(v.x,v.y,b.width/bounds.width*m.width,b.height/bounds.height*m.height),RpgArt.Hex("52614A"));}
            foreach(var s in game.world.shrines){var v=Point(s.transform.position);Fill(new Rect(v.x-2,v.y-2,5,5),s.color);}
            var quest=game.GetComponent<RpgQuest08>();if(quest.giver){var q=Point(quest.giver.position);Fill(new Rect(q.x-2,q.y-2,5,5),gold);}foreach(var n in quest.nodes){var q=Point(n.transform.position);Fill(new Rect(q.x-2,q.y-2,4,4),(game.progress.silenced&(1<<n.id))==0?RpgArt.Hex("FF7186"):mint);}
            var h=Point(game.world.hero.Position);Fill(new Rect(h.x-2,h.y-2,5,5),Color.white);
        }
        void Pause()
        {
            Fill(new Rect(0,0,1440,810),new Color(0,.02f,.03f,.78f));Panel(new Rect(485,220,470,390),gold);Label(new Rect(520,253,400,52),"RESPIRE UM POUCO",29,gold,TextAnchor.MiddleCenter);
            if(Button(new Rect(535,343,370,60),"CONTINUAR",true))game.mode=1;
            if(Button(new Rect(535,425,370,48),"Controles"))help=!help;
            if(Button(new Rect(535,496,370,48),"Salvar e voltar ao início")){game.Save();game.mode=0;}
            if(help)Help();
        }
        void Help()
        {
            Panel(new Rect(290,130,890,570),gold);Label(new Rect(330,160,820,44),"ENCONTRE SUA CENTELHA",28,gold);
            Label(new Rect(330,230,820,370),"WASD / setas     Explorar a vila\nShift                   Correr\nEspaço               Saltar\nJ / clique            Atacar\nE                         Despertar um altar próximo\n1                         Voo: decolar / pousar; Shift impulso, Ctrl freio\n2                         Grito da Liberdade (40 de energia)\n3                         Palma da Centelha (22, após missão)\nC                         Segurar para carregar; soltar para transformar\nEsc                     Pausar e salvar",21,Color.white);
            if(Button(new Rect(830,636,280,44),"Entendi [H]"))help=false;
        }
    }
}

