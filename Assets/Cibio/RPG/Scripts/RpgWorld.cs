using System.Collections.Generic;
using UnityEngine;

namespace Cibio.RPG
{
    public sealed class RpgWorld:MonoBehaviour
    {
        public List<Rect> obstacles=new();
        public List<RpgSolid> solids=new();
        public List<RpgShrine> shrines=new();
        public List<RpgActor> enemies=new();
        public readonly Rect bounds=new(-24,-18,48,36);
        public RpgActor hero;public Transform scenery;public bool built;
        public void Build()
        {
            if(built)return;built=true;
            scenery=new GameObject("Vila • cenário LPC").transform;scenery.SetParent(transform,false);
            var random=new System.Random(51029);
            for(int y=-18;y<18;y++)for(int x=-24;x<24;x++)
            {
                var s=RpgArt.Tile("terrain_atlas",128,752);
                RpgArt.Put("Relva",s,new Vector2(x+.5f,y+.5f),-4000,scenery,new Color(.72f,.84f,.73f));
                bool plaza=x>=-5&&x<=5&&y>=-5&&y<=3;
                bool path=(Mathf.Abs(x)<=1&&Mathf.Abs(y)<16)||(y>=-3&&y<=-1&&Mathf.Abs(x)<21)||(y>=7&&y<=8&&Mathf.Abs(x)<18);
                if(plaza||path)
                {
                    s=plaza?RpgArt.Tile("terrain_atlas",32+(x&1)*32,512+(1-(y&1))*32):RpgArt.Tile("base_out_atlas",608,96);
                    RpgArt.Put(plaza?"Praça de pedra":"Caminho",s,new Vector2(x+.5f,y+.5f),-3999,scenery,plaza?new Color(.92f,.88f,.75f):new Color(.88f,.87f,.79f));
                }
            }
            House(new(-8,2),0);House(new(0,7),2);House(new(9,5),1);House(new(13,-5),0);House(new(-11,-10),1);House(new(-17,8),2);
            var pond=Object("Pond",new(15,9),false,new Vector2(2.2f,1.7f));AddSolid(pond.gameObject,new Vector2(6,5),new Vector2(0,1.5f));
            for(int i=0;i<195;i++)
            {
                Vector2 p=new((float)random.NextDouble()*46-23,(float)random.NextDouble()*34-17);
                bool edge=Mathf.Abs(p.x)>20||Mathf.Abs(p.y)>15;
                if(!edge&&(Mathf.Abs(p.x)<16&&Mathf.Abs(p.y)<11))continue;
                if(!CanStand(p,.6f))continue;
                Object("Tree_"+random.Next(2),p,true);
            }
            foreach(var p in new[]{new Vector2(-6,6),new Vector2(5,5),new Vector2(-15,-5),new Vector2(9,-9),new Vector2(17,2),new Vector2(-18,12),new Vector2(4,12)})Object("Tree_0",p,true);
            for(int i=0;i<95;i++)
            {
                Vector2 p=new((float)random.NextDouble()*42-21,(float)random.NextDouble()*30-15);
                if(Mathf.Abs(p.x)<4||Mathf.Abs(p.y+2)<2||!CanStand(p,.5f))continue;
                Object(i%5==0?"Rock":"Bush",p,i%5==0,Vector2.one*(i%5==0?.7f:.8f));
            }
            foreach(var p in new[]{new Vector2(-6,1),new Vector2(3,6),new Vector2(11,4),new Vector2(15,-5),new Vector2(-13,-10)})Object("Barrel",p,true);
            AddShrine("Voo da Centelha","[1] decolar e pousar. Energia enquanto voa.",new(3,-1),0,"64DCCA");
            AddShrine("Coração Canarinho","Melhora a transformação: dura 24s.",new(-14,2),1,"F4C76B");
            AddShrine("Grito da Liberdade","Uma onda de impacto. [2]",new(13,-10),2,"BBA2F1");
            hero=CreateActor("Brasileiro",Vector2.zero,false);hero.transform.position=new Vector2(0,-3);hero.gameObject.AddComponent<RpgAura>();
            foreach(var p in new[]{new Vector2(8,-2),new Vector2(-9,-3),new Vector2(10,-10),new Vector2(-13,5),new Vector2(15,-8),new Vector2(4,11),new Vector2(-8,11)})enemies.Add(CreateActor("Cobrador da Vuvuzela",p,true));
        }
        public RpgActor CreateActor(string name,Vector2 position,bool enemy)
        {
            var o=new GameObject(name);o.transform.SetParent(transform,false);o.transform.position=position;var a=o.AddComponent<RpgActor>();a.world=this;a.isEnemy=enemy;a.Initialize();return a;
        }
        void House(Vector2 p,int variant){var r=Object("House_"+variant,p,false);AddSolid(r.gameObject,new Vector2(5,3.5f),new Vector2(0,1.7f));}
        void AddSolid(GameObject o,Vector2 size,Vector2 offset){var s=o.AddComponent<RpgSolid>();s.size=size;s.offset=offset;solids.Add(s);}
        SpriteRenderer Object(string asset,Vector2 p,bool solid,Vector2? scale=null)
        {
            var r=RpgArt.Put(asset,RpgArt.Image(asset),p,Order(p.y),scenery);r.transform.localScale=scale??Vector2.one;
            if(solid)AddSolid(r.gameObject,new Vector2(.72f,.5f),new Vector2(0,.07f));return r;
        }
        void AddShrine(string name,string description,Vector2 p,int id,string color)
        {
            var o=new GameObject(name);o.transform.SetParent(transform,false);o.transform.position=p;var s=o.AddComponent<RpgShrine>();s.id=id;s.title=name;s.description=description;s.color=RpgArt.Hex(color);s.Initialize();shrines.Add(s);
        }
        public static int Order(float y)=>1000-Mathf.RoundToInt(y*32);
        public bool CanStand(Vector2 p,float radius=.22f)
        {
            if(p.x<bounds.xMin+radius||p.x>bounds.xMax-radius||p.y<bounds.yMin+radius||p.y>bounds.yMax-radius)return false;
            foreach(var r in obstacles)if(Touches(r,p,radius))return false;
            foreach(var solid in solids)if(solid&&solid.enabled&&solid.gameObject.activeInHierarchy&&Touches(solid.Bounds,p,radius))return false;return true;
        }
        static bool Touches(Rect r,Vector2 p,float radius){var nearest=new Vector2(Mathf.Clamp(p.x,r.xMin,r.xMax),Mathf.Clamp(p.y,r.yMin,r.yMax));return (nearest-p).sqrMagnitude<radius*radius;}
        public Vector2 Move(Vector2 p,Vector2 delta)
        {
            int steps=Mathf.Max(1,Mathf.CeilToInt(delta.magnitude/.12f));delta/=steps;
            for(int i=0;i<steps;i++){if(CanStand(p+new Vector2(delta.x,0)))p.x+=delta.x;if(CanStand(p+new Vector2(0,delta.y)))p.y+=delta.y;}return p;
        }
        public RpgShrine NearestShrine(Vector2 p)
        {
            RpgShrine result=null;float best=2.1f;foreach(var s in shrines){float d=Vector2.Distance(p,s.transform.position);if(d<best){best=d;result=s;}}return result;
        }
    }
}

