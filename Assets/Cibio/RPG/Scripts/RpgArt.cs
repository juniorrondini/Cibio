using System.Collections.Generic;
using UnityEngine;

namespace Cibio.RPG
{
    public static class RpgArt
    {
        static readonly Dictionary<string,Texture2D> textures=new();
        static readonly Dictionary<string,Sprite> sprites=new();
        public static Texture2D Texture(string name)
        {
            if(!textures.TryGetValue(name,out var t)) {t=Resources.Load<Texture2D>("RpgArt/"+name);if(t)t.filterMode=name.StartsWith("Vfx_")||name.StartsWith("UI_")?FilterMode.Bilinear:FilterMode.Point;textures[name]=t;}return t;
        }
        public static Sprite Image(string name, Vector2? pivot=null)
        {
            string key=name+"/"+(pivot??Vector2.zero);
            if(!sprites.TryGetValue(key,out var s)) {var t=Texture(name);s=Sprite.Create(t,new Rect(0,0,t.width,t.height),pivot??new Vector2(.5f,.05f),32,0,SpriteMeshType.FullRect);sprites[key]=s;}return s;
        }
        public static int CellSize(string person)=>person=="HeroAscendant"?192:person=="HeroAwakened"?128:64;
        public static int FrameCount(string person,string action)=>Texture(person+"_"+action).width/CellSize(person);
        public static Sprite Frame(string person,string action,int direction,int frame)
        {
            var t=Texture(person+"_"+action);int cell=CellSize(person),columns=t.width/cell,rows=t.height/cell;
            frame=Mathf.Clamp(frame,0,columns-1);direction=Mathf.Clamp(direction,0,rows-1);
            string key=person+action+direction+"/"+frame;
            if(!sprites.TryGetValue(key,out var s)){s=Sprite.Create(t,new Rect(frame*cell,t.height-(direction+1)*cell,cell,cell),new Vector2(.5f,person=="HeroAscendant"?.10666667f:.16f),person=="HeroAscendant"?64:cell/2f,0,SpriteMeshType.FullRect);sprites[key]=s;}return s;
        }
        public static Sprite Tile(string atlas,int x,int y,int w=32,int h=32)
        {
            string key=atlas+x+"/"+y+"/"+w+"/"+h;
            if(!sprites.TryGetValue(key,out var s)){var t=Texture(atlas);s=Sprite.Create(t,new Rect(x,t.height-y-h,w,h),Vector2.one*.5f,32,0,SpriteMeshType.FullRect);sprites[key]=s;}return s;
        }
        public static Color Hex(string hex){ColorUtility.TryParseHtmlString("#"+hex,out var c);return c;}
        public static SpriteRenderer Put(string name,Sprite sprite,Vector2 position,int order,Transform parent,Color? color=null)
        {
            var o=new GameObject(name);o.transform.SetParent(parent,false);o.transform.position=position;var r=o.AddComponent<SpriteRenderer>();r.sprite=sprite;r.sortingOrder=order;r.color=color??Color.white;return r;
        }
        public static Sprite Pixel
        {
            get
            {
                if(!sprites.TryGetValue("pixel",out var s)){var t=Texture("Pixel");s=Sprite.Create(t,new Rect(0,0,1,1),Vector2.one*.5f,1);sprites["pixel"]=s;}return s;
            }
        }
    }
}
