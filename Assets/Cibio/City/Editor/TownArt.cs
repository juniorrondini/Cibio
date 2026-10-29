using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Cibio.City.Editor.MeshWorkshop;

namespace Cibio.City.Editor
{
    public static class TownArt
    {
        private static readonly Dictionary<string,Mesh> cache=new Dictionary<string,Mesh>();
        private static readonly Dictionary<string,GameObject> prefabs=new Dictionary<string,GameObject>();
        public static void Reset(){cache.Clear();prefabs.Clear();}
        public static Transform Group(string name,Transform parent=null,Vector3 local=default)
        {var obj=new GameObject(name);obj.transform.SetParent(parent,false);obj.transform.localPosition=local;return obj.transform;}
        public static GameObject MeshObject(string name,Mesh mesh,string mat,Transform parent,Vector3 local=default,Quaternion rotation=default)
        {
            var obj=new GameObject(name);obj.transform.SetParent(parent,false);obj.transform.localPosition=local;
            obj.transform.localRotation=rotation==default?Quaternion.identity:rotation;
            obj.AddComponent<MeshFilter>().sharedMesh=mesh;obj.AddComponent<MeshRenderer>().sharedMaterial=CityPalette.Get(mat);return obj;
        }
        public static Mesh Cached(string key,System.Func<Mesh> factory)
        {if(!cache.TryGetValue(key,out var mesh)){mesh=Save(factory(),"Town_"+key);cache[key]=mesh;}return mesh;}
        public static GameObject Box(string name,Transform parent,Vector3 local,Vector3 size,string mat,float bevel=.035f,bool solid=false)
        {
            string key="Box_"+size.x.ToString("0.###",System.Globalization.CultureInfo.InvariantCulture)+"_"+size.y.ToString("0.###",System.Globalization.CultureInfo.InvariantCulture)+"_"+size.z.ToString("0.###",System.Globalization.CultureInfo.InvariantCulture)+"_"+bevel;
            var obj=MeshObject(name,Cached(key,()=>RoundedBox(key,size,bevel,4)),mat,parent,local);
            if(solid){var collider=obj.AddComponent<BoxCollider>();collider.size=size;}
            return obj;
        }
        public static GameObject Oval(string name,Transform parent,Vector3 local,Vector3 radii,string mat)
        {
            var mesh=Cached("Sphere",()=>Ellipsoid("Smooth reusable sphere",Vector3.zero,Vector3.one,32,20));
            var obj=MeshObject(name,mesh,mat,parent,local);obj.transform.localScale=radii;return obj;
        }
        public static GameObject Pipe(string name,Transform parent,Vector3[] points,float radius,string mat,int sides=12)
        {var mesh=Cached("Pipe_"+pipeCounter++,()=>Tube(name,points,radius,sides));return MeshObject(name,mesh,mat,parent);}
        private static int pipeCounter;
        public static GameObject Lathe(string name,Transform parent,Vector3 local,Ring[] rings,string mat,int sides=64)
        {var mesh=Cached("Lathe_"+pipeCounter++,()=>Loft(name,rings,sides));return MeshObject(name,mesh,mat,parent,local);}

        public static GameObject SavePrefab(Transform root,string name,bool combine=true)
        {
            if(combine)Combine(root,name);
            var asset=PrefabUtility.SaveAsPrefabAsset(root.gameObject,"Assets/Cibio/City/Prefabs/"+name+".prefab");
            prefabs[name]=asset;Object.DestroyImmediate(root.gameObject);return asset;
        }
        public static GameObject Place(string name,Transform parent,Vector3 position,float yaw=0,float scale=1)
        {var obj=(GameObject)PrefabUtility.InstantiatePrefab(prefabs[name]);obj.transform.SetParent(parent,false);obj.transform.localPosition=position;obj.transform.localRotation=Quaternion.Euler(0,yaw,0);obj.transform.localScale=Vector3.one*scale;return obj;}
        public static void Combine(Transform root,string name)
        {
            var meshes=root.GetComponentsInChildren<MeshFilter>();var groups=new Dictionary<Material,List<CombineInstance>>();
            foreach(var filter in meshes)
            {
                var renderer=filter.GetComponent<MeshRenderer>();
                if(!renderer||renderer.sharedMaterials.Length!=1||filter.GetComponent<TextMesh>()||!filter.sharedMesh)continue;
                var mat=renderer.sharedMaterial;if(!groups.ContainsKey(mat))groups[mat]=new List<CombineInstance>();
                groups[mat].Add(new CombineInstance{mesh=filter.sharedMesh,transform=root.worldToLocalMatrix*filter.transform.localToWorldMatrix});
                Object.DestroyImmediate(renderer);Object.DestroyImmediate(filter);
            }
            foreach(var group in groups)
            {
                var mesh=new Mesh{name=name+"_"+group.Key.name,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};
                mesh.CombineMeshes(group.Value.ToArray(),true,true);
                var node=new GameObject("Surface • "+group.Key.name);node.transform.SetParent(root,false);
                node.AddComponent<MeshFilter>().sharedMesh=Save(mesh,name+"_Combined_"+group.Key.name);node.AddComponent<MeshRenderer>().sharedMaterial=group.Key;
            }
            // Keep authored pivots and colliders; remove empty construction nodes.
            foreach(var child in root.GetComponentsInChildren<Transform>())
                if(child&&child!=root&&child.childCount==0&&child.GetComponents<Component>().Length==1)Object.DestroyImmediate(child.gameObject);
        }

        public static void CreateStreetFurniture()
        {
            var bench=Group("Banco ripado");
            for(int i=0;i<6;i++)Box("Ripa do assento",bench,new Vector3(0,.46f,-.24f+i*.088f),new Vector3(1.8f,.065f,.074f),i%2==0?"Wood":"WoodLight",.018f);
            for(int i=0;i<4;i++)Box("Ripa do encosto",bench,new Vector3(0,.70f+i*.105f,.25f),new Vector3(1.8f,.09f,.055f),i%2==0?"WoodLight":"Wood",.016f);
            foreach(float x in new[]{-.66f,.66f})
            {
                Pipe("Pes curvos",bench,new[]{new Vector3(x,.02f,-.22f),new Vector3(x,.12f,-.17f),new Vector3(x,.42f,-.13f),new Vector3(x,.49f,.05f),new Vector3(x,.42f,.22f),new Vector3(x,.02f,.3f)},.033f,"Metal");
                Pipe("Suporte do encosto",bench,new[]{new Vector3(x,.28f,.20f),new Vector3(x,.63f,.27f),new Vector3(x,1.04f,.27f)},.026f,"Metal");
                Pipe("Apoio de braco",bench,new[]{new Vector3(x,.46f,-.15f),new Vector3(x,.68f,-.15f),new Vector3(x,.72f,.08f),new Vector3(x,.73f,.25f)},.027f,"Metal");
                for(int i=0;i<4;i++)Oval("Parafuso",bench,new Vector3(x,.70f+i*.105f,.214f),Vector3.one*.009f,"Brass");
            }
            SavePrefab(bench,"Banco_Ripado");
            var lamp=Group("Poste de praca");
            Lathe("Base torneada",lamp,Vector3.zero,new[]{new Ring(0,.21f,.21f),new Ring(.08f,.21f,.21f),new Ring(.15f,.15f,.15f),new Ring(.35f,.12f,.12f),new Ring(.42f,.065f,.065f),new Ring(2.9f,.048f,.048f),new Ring(3.1f,.065f,.065f)},"Metal",40);
            Pipe("Braco do poste",lamp,new[]{new Vector3(0,2.85f,0),new Vector3(0,3.18f,0),new Vector3(0,3.37f,.14f),new Vector3(0,3.34f,.40f)},.043f,"Metal");
            Lathe("Lanterna",lamp,new Vector3(0,2.82f,.4f),new[]{new Ring(0,.11f,.11f),new Ring(.05f,.17f,.17f),new Ring(.37f,.14f,.14f),new Ring(.43f,.21f,.21f),new Ring(.48f,.13f,.13f),new Ring(.56f,.02f,.02f)},"Metal",32);
            Lathe("Vidro ambar",lamp,new Vector3(0,2.89f,.4f),new[]{new Ring(0,.145f,.145f),new Ring(.28f,.12f,.12f)},"Lamp",32);
            SavePrefab(lamp,"Poste_Colonial");
            var bin=Group("Lixeira");
            Lathe("Corpo",bin,Vector3.zero,new[]{new Ring(0,.25f,.25f),new Ring(.06f,.28f,.28f),new Ring(.65f,.28f,.28f),new Ring(.70f,.31f,.31f),new Ring(.75f,.31f,.31f)},"Metal",40);
            Lathe("Aro",bin,Vector3.zero,new[]{new Ring(.68f,.315f,.315f),new Ring(.73f,.315f,.315f),new Ring(.75f,.285f,.285f),new Ring(.68f,.285f,.285f)},"Brass",40);
            for(int i=0;i<18;i++){float a=i/18f*Mathf.PI*2;Pipe("Friso",bin,new[]{new Vector3(Mathf.Cos(a)*.284f,.1f,Mathf.Sin(a)*.284f),new Vector3(Mathf.Cos(a)*.284f,.62f,Mathf.Sin(a)*.284f)},.009f,"Teal",8);}
            SavePrefab(bin,"Lixeira_Urbana");
        }

        public static void CreateTree()
        {
            var tree=Group("Ipe da praca");
            Pipe("Tronco",tree,new[]{new Vector3(0,0,0),new Vector3(.04f,.8f,.03f),new Vector3(-.05f,1.5f,.01f),new Vector3(.12f,2.3f,0),new Vector3(.05f,3.15f,.08f)},.13f,"Wood",18);
            for(int i=0;i<7;i++)
            {
                float a=i/7f*Mathf.PI*2;Vector3 end=new Vector3(Mathf.Cos(a)*1.12f,2.8f+Mathf.Sin(i*3)*.3f,Mathf.Sin(a)*1.12f);
                Pipe("Galho",tree,new[]{new Vector3(0,1.65f,0),Vector3.Lerp(new Vector3(0,2,0),end,.5f),end},.055f,"Wood",12);
            }
            for(int i=0;i<19;i++)
            {
                float a=i*2.4f;float distance=Mathf.Sqrt(i/19f)*1.2f;
                var leafMesh=Cached("Canopy_"+(i%5),()=>OrganicCanopy(i%5));
                MeshObject("Massa de folhas",leafMesh,i%3==0?"LeavesLight":"Leaves",tree,new Vector3(Mathf.Cos(a)*distance,3.0f+Mathf.Cos(i*4)*.4f,Mathf.Sin(a)*distance));
            }
            for(int i=0;i<45;i++)
            {
                float a=i*2.399f,d=Mathf.Sqrt(i/45f)*1.5f;
                var flower=Oval("Flor do ipe",tree,new Vector3(Mathf.Cos(a)*d,3.35f+Mathf.Sin(i*3.7f)*.38f,Mathf.Sin(a)*d),new Vector3(.07f,.035f,.065f),i%3==0?"Flower":"LeavesLight");
            }
            var trunkCollider=tree.gameObject.AddComponent<CapsuleCollider>();trunkCollider.center=Vector3.up*1.3f;trunkCollider.height=2.6f;trunkCollider.radius=.19f;
            SavePrefab(tree,"Ipe_Detalhado");
        }
        private static Mesh OrganicCanopy(int seed)
        {
            var mesh=Ellipsoid("Irregular canopy",Vector3.zero,Vector3.one,32,24);var v=mesh.vertices;
            for(int i=0;i<v.Length;i++)
            {
                Vector3 p=v[i];float noise=Mathf.PerlinNoise(p.x*3.4f+seed*4,p.z*3.5f+p.y*2.7f);
                float r=.55f+noise*.2f;v[i]=Vector3.Scale(p*r,new Vector3(1,.8f,1));
            }
            mesh.vertices=v;mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();return mesh;
        }

        public static void CreateFountain()
        {
            var fountain=Group("Fonte do Respiro");
            Lathe("Pedra esculpida",fountain,Vector3.zero,new[]{new Ring(0,1.65f,1.65f),new Ring(.08f,1.72f,1.72f),new Ring(.14f,1.68f,1.68f),new Ring(.20f,1.55f,1.55f),new Ring(.45f,1.55f,1.55f),new Ring(.50f,1.63f,1.63f),new Ring(.58f,1.63f,1.63f),new Ring(.62f,1.50f,1.50f),new Ring(.57f,1.40f,1.40f),new Ring(.34f,1.40f,1.40f)},"Stone",96);
            Oval("Espelho d'agua",fountain,new Vector3(0,.435f,0),new Vector3(1.42f,.017f,1.42f),"Water");
            Lathe("Coluna central",fountain,Vector3.zero,new[]{new Ring(.35f,.32f,.32f),new Ring(.48f,.37f,.37f),new Ring(.58f,.27f,.27f),new Ring(.99f,.17f,.17f),new Ring(1.10f,.24f,.24f),new Ring(1.18f,.63f,.63f),new Ring(1.24f,.68f,.68f),new Ring(1.30f,.61f,.61f),new Ring(1.26f,.54f,.54f)},"Cream",72);
            Oval("Bacia superior",fountain,new Vector3(0,1.265f,0),new Vector3(.54f,.015f,.54f),"Water");
            for(int i=0;i<8;i++)
            {
                float a=i*Mathf.PI*.25f;var path=new Vector3[20];
                for(int j=0;j<20;j++){float t=j/19f;path[j]=new Vector3(Mathf.Cos(a)*(.45f+t*.55f),1.27f+.24f*Mathf.Sin(t*Mathf.PI)-t*.81f,Mathf.Sin(a)*(.45f+t*.55f));}
                Pipe("Fio de agua",fountain,path,.012f,"Water",8);
            }
            var collider=fountain.gameObject.AddComponent<CapsuleCollider>();collider.center=Vector3.up*.32f;collider.height=.64f;collider.radius=1.62f;
            SavePrefab(fountain,"Fonte_de_Pedra");
        }

        public static void CreateBuilding(string name,string color,bool shop)
        {
            var house=Group(name);float w=5.4f,h=shop?3.65f:4.7f,d=4.3f;
            Box("Alvenaria",house,new Vector3(0,h/2,0),new Vector3(w,h,d),color,.06f,true);
            Box("Rodape",house,new Vector3(0,.18f,0),new Vector3(w+.04f,.36f,d+.05f),"Stone",.025f);
            Box("Cornija",house,new Vector3(0,h-.12f,0),new Vector3(w+.16f,.19f,d+.16f),"Ivory",.035f);
            MeshObject("Empena fechada",Cached("Gable_"+h,()=>GableMesh(w,d)),color,house,new Vector3(0,h,0));
            foreach(float x in new[]{-w/2+.16f,w/2-.16f})
                Box("Pilastro",house,new Vector3(x,h/2,-d/2-.035f),new Vector3(.19f,h,.12f),"Ivory",.015f);
            if(shop)
            {
                for(int i=0;i<3;i++)Window(house,new Vector3(-1.66f+i*1.66f,1.35f,-d/2-.03f),1.35f,2.05f,true);
                Box("Fascia",house,new Vector3(0,2.77f,-d/2-.10f),new Vector3(4.8f,.49f,.16f),"Trim",.045f);
                Label(name,house,new Vector3(0,2.78f,-d/2-.19f),.24f,"Paper");
                for(int strip=0;strip<16;strip++)
                {
                    var mesh=Cached("AwningStrip",AwningMesh);
                    MeshObject("Toldo em tecido",mesh,strip%2==0?"Ivory":color,house,new Vector3(-2.25f+strip*.3f,2.6f,-d/2-.1f));
                }
            }
            else
            {
                Window(house,new Vector3(-1.55f,1.35f,-d/2-.035f),.95f,1.65f,false);
                Window(house,new Vector3(1.55f,1.35f,-d/2-.035f),.95f,1.65f,false);
                Box("Porta antiga",house,new Vector3(0,1.13f,-d/2-.06f),new Vector3(.94f,2.26f,.14f),"Wood",.045f);
                Box("Alizar da porta",house,new Vector3(0,1.16f,-d/2-.04f),new Vector3(1.14f,2.43f,.08f),"Ivory",.065f);
                Box("Folha da porta",house,new Vector3(0,1.13f,-d/2-.11f),new Vector3(.90f,2.21f,.10f),"WoodLight",.035f);
                foreach(float z in new[]{.6f,1.5f})Box("Painel entalhado",house,new Vector3(0,z,-d/2-.175f),new Vector3(.69f,.65f,.03f),"Wood",.025f);
                Oval("Macaneta",house,new Vector3(.32f,1.11f,-d/2-.21f),Vector3.one*.027f,"Brass");
                for(int i=0;i<3;i++)Window(house,new Vector3(-1.62f+i*1.62f,3.55f,-d/2-.035f),.96f,1.25f,false);
            }
            // Side windows are visible from the elevated camera too.
            for(int side=-1;side<=1;side+=2)
            {
                var pivot=Group("Janela lateral",house,new Vector3(side*w/2,1.8f,0));pivot.localRotation=Quaternion.Euler(0,side<0?90:-90,0);
                Window(pivot,Vector3.zero,1.1f,1.5f,false);
            }
            var tile=Cached("TerracottaTile",RoofTile);
            float slope=Mathf.Atan2(.80f,d/2+.3f)*Mathf.Rad2Deg;
            for(int side=-1;side<=1;side+=2)
            for(int row=0;row<7;row++)
            for(int col=0;col<24;col++)
            {
                float z=side*(.16f+row*.35f),x=-2.87f+col*.25f;
                float y=h+.88f-Mathf.Abs(z)*.80f/(d/2+.3f);
                MeshObject("Telha ceramica",tile,(row+col)%5==0?"RoofLight":"Roof",house,new Vector3(x,y,z),Quaternion.Euler(side*slope,0,0));
            }
            Pipe("Cumeeira",house,new[]{new Vector3(-3,h+.98f,0),new Vector3(3,h+.98f,0)},.12f,"RoofLight",20);
            foreach(int side in new[]{-1,1})
            {
                Pipe("Calha",house,new[]{new Vector3(-3,h+.09f,side*2.48f),new Vector3(3,h+.09f,side*2.48f)},.055f,"Metal");
                Pipe("Descida pluvial",house,new[]{new Vector3(2.7f,h+.09f,side*2.48f),new Vector3(2.7f,h-.22f,side*2.36f),new Vector3(2.7f,.12f,side*2.36f)},.035f,"Metal");
            }
            SavePrefab(house,name.Replace(" ","_"));
        }

        private static void Window(Transform parent,Vector3 center,float w,float h,bool storefront)
        {
            Box("Moldura em pedra",parent,center,new Vector3(w+.15f,h+.17f,.11f),"Ivory",.045f);
            Box("Rebaixo",parent,center+Vector3.back*.07f,new Vector3(w,h,.07f),"DarkGlass",.028f);
            Box("Vidro",parent,center+Vector3.back*.12f,new Vector3(w-.12f,h-.12f,.035f),"Glass",.018f);
            Box("Travessa vertical",parent,center+Vector3.back*.15f,new Vector3(.045f,h,.035f),"WoodLight",.005f);
            Box("Travessa horizontal",parent,center+Vector3.back*.15f,new Vector3(w,.045f,.035f),"WoodLight",.005f);
            Box("Peitoril",parent,center+new Vector3(0,-h/2-.10f,-.12f),new Vector3(w+.25f,.10f,.30f),"Cream",.028f);
            if(!storefront)
            foreach(int side in new[]{-1,1})
            {
                var shutter=Group("Veneziana",parent,center+new Vector3(side*(w/2+.18f),0,-.09f));shutter.localRotation=Quaternion.Euler(0,side*-15,0);
                Box("Caixilho",shutter,Vector3.zero,new Vector3(.29f,h,.065f),"Trim",.016f);
                for(int i=0;i<11;i++)
                {
                    var slat=Box("Lamina",shutter,new Vector3(0,-h*.43f+i*h*.085f,-.043f),new Vector3(.23f,.055f,.033f),"Teal",.008f);slat.transform.localRotation=Quaternion.Euler(-20,0,0);
                }
            }
        }
        private static Mesh RoofTile()
        {
            var v=new List<Vector3>();var uv=new List<Vector2>();var tris=new List<int>();
            for(int row=0;row<3;row++)for(int i=0;i<=12;i++)
            {
                float angle=i/12f*Mathf.PI;v.Add(new Vector3(Mathf.Cos(angle)*.142f,Mathf.Sin(angle)*.087f,row*.24f));uv.Add(new Vector2(i/12f,row/2f));
                if(row==2||i==12)continue;int a=row*13+i;tris.AddRange(new[]{a,a+1,a+13,a+1,a+14,a+13});
            }
            var mesh=new Mesh{name="Telha meia cana"};mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateTangents();return mesh;
        }
        private static Mesh GableMesh(float width,float depth)
        {
            var vertices=new[]{new Vector3(-width/2,0,-depth/2-.2f),new Vector3(-width/2,0,depth/2+.2f),new Vector3(-width/2,.90f,0),new Vector3(width/2,0,-depth/2-.2f),new Vector3(width/2,0,depth/2+.2f),new Vector3(width/2,.90f,0)};
            var triangles=new[]{0,2,1,3,4,5,0,3,5,0,5,2,2,5,4,2,4,1,0,1,4,0,4,3};
            var mesh=new Mesh{name="Closed gable"};mesh.vertices=vertices;mesh.triangles=triangles;
            var uv=new Vector2[vertices.Length];for(int i=0;i<uv.Length;i++)uv[i]=new Vector2(vertices[i].x,vertices[i].z);
            mesh.uv=uv;mesh.RecalculateNormals();mesh.RecalculateTangents();return mesh;
        }
        private static Mesh AwningMesh()
        {
            var v=new List<Vector3>();var uv=new List<Vector2>();var tris=new List<int>();
            for(int row=0;row<=12;row++)for(int side=0;side<2;side++)
            {
                float t=row/12f;v.Add(new Vector3(side*.30f,-.15f*t-.12f*t*t,-t*.85f));uv.Add(new Vector2(side,t));
                if(row==12||side==1)continue;int a=row*2;tris.AddRange(new[]{a,a+1,a+2,a+1,a+3,a+2,a+2,a+1,a,a+2,a+3,a+1});
            }
            var mesh=new Mesh{name="Toldo curvo"};mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetTriangles(tris,0);
            var normals=new Vector3[v.Count];for(int i=0;i<normals.Length;i++)normals[i]=new Vector3(0,.98f,-.2f).normalized;
            mesh.normals=normals;mesh.RecalculateTangents();return mesh;
        }
        public static TextMesh Label(string text,Transform parent,Vector3 local,float height,string material,float yaw=0)
        {
            var obj=new GameObject(text);obj.transform.SetParent(parent,false);obj.transform.localPosition=local;obj.transform.localRotation=Quaternion.Euler(0,yaw,0);
            var label=obj.AddComponent<TextMesh>();label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");label.fontSize=100;label.characterSize=height/10;
            label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;label.text=text;label.color=CityPalette.Get(material).color;
            obj.GetComponent<MeshRenderer>().sharedMaterial=label.font.material;
            obj.AddComponent<WorldLettering>().depthShader=Shader.Find("Cibio/World Lettering");return label;
        }
    }
}
