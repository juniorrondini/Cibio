using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Cibio.City.Editor.MeshWorkshop;

namespace Cibio.City.Editor
{
    public static class HeroArt
    {
        private sealed class Part
        { public Mesh mesh; public Material material; public Func<Vector3, BoneWeight> weight; }
        private static readonly List<Part> parts = new List<Part>();
        private static readonly List<Transform> bones = new List<Transform>();
        private static readonly Dictionary<string, int> indices = new Dictionary<string, int>();
        private static bool enemy;
        private static bool headMode;
        private static readonly Matrix4x4 HeadShape = Matrix4x4.Translate(new Vector3(0,1.75f,0))*Matrix4x4.Scale(new Vector3(.88f,.8f,.9f))*Matrix4x4.Translate(new Vector3(0,-1.75f,0));
        private static string Prefix => enemy ? "Cobrador" : "Brasileiro";

        public static GameObject Build(bool adversary, RuntimeAnimatorController controller)
        {
            enemy = adversary; parts.Clear(); bones.Clear(); indices.Clear(); staticCounter = 0;
            var root = new GameObject(Prefix + " • refinado");
            var body = new GameObject("Visual").transform; body.SetParent(root.transform, false);
            Bone("Hips", body, new Vector3(0, .96f, 0));
            Bone("Spine", B("Hips"), new Vector3(0, .15f, 0));
            Bone("Chest", B("Spine"), new Vector3(0, .23f, 0));
            Bone("Neck", B("Chest"), new Vector3(0, .18f, 0));
            Bone("Head", B("Neck"), new Vector3(0, .17f, 0));
            foreach (int s in new[] { -1, 1 })
            {
                string side = s < 0 ? "L" : "R";
                Bone(side + "UpperArm", B("Chest"), new Vector3(s * .28f, .07f, 0));
                Bone(side + "Forearm", B(side + "UpperArm"), new Vector3(s * .04f, -.24f, 0));
                Bone(side + "Hand", B(side + "Forearm"), new Vector3(s * .015f, -.22f, 0));
                Bone(side + "UpperLeg", B("Hips"), new Vector3(s * .13f, -.03f, 0));
                Bone(side + "LowerLeg", B(side + "UpperLeg"), new Vector3(0, -.41f, 0));
                Bone(side + "Foot", B(side + "LowerLeg"), new Vector3(0, -.40f, .015f));
            }
            BuildTorso(); headMode=true; BuildFace(); BuildHair(); headMode=false;
            foreach (int side in new[] { -1, 1 }) { BuildArm(side); BuildLeg(side); }
            Assemble(body);
            Transform leftEye = Eye(body, -1), rightEye = Eye(body, 1);
            if (!enemy)
            {
                Text("BRASIL", B("Chest"), new Vector3(0, -.08f, .157f), 180, .047f, "Trim");
                Text("10", B("Chest"), new Vector3(0, -.08f, -.155f), 0, .17f, "Ivory");
                Text("BRASIL", B("Chest"), new Vector3(0, .02f, -.156f), 0, .044f, "Ivory");
            }
            else
            {
                Text("PROTOCOLO", B("Chest"), new Vector3(0, .08f, -.16f), 0, .036f, "Paper");
                // Clipboard with a beveled frame, sheet and a metal clip.
                var hand = B("LHand");
                StaticPart("Prancheta", RoundedBox("Clipboard", new Vector3(.22f, .30f, .025f), .015f), "Wood", hand, new Vector3(-.025f, -.06f, .08f));
                StaticPart("Folha", RoundedBox("Sheet", new Vector3(.185f, .255f, .004f), .008f), "Paper", hand, new Vector3(-.025f, -.06f, .097f));
                StaticPart("Clipe", RoundedBox("Clip", new Vector3(.07f, .035f, .009f), .005f), "Metal", hand, new Vector3(-.025f, .065f, .104f));
                for (int i = 0; i < 7; i++)
                    StaticPart("Linha impressa", RoundedBox("InkLine", new Vector3(.13f - i % 3 * .02f, .003f, .002f), .0005f), "ShortSeam", hand, new Vector3(-.025f, .025f - i * .026f, .101f));
            }
            var animator = body.gameObject.AddComponent<Animator>(); animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false; animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            var driver = body.gameObject.AddComponent<HumanoidDriver>(); driver.leftEye = leftEye; driver.rightEye = rightEye;
            var capsule = root.AddComponent<CharacterController>(); capsule.height = 1.96f; capsule.center = Vector3.up * .98f;
            capsule.radius = .23f; capsule.skinWidth = .018f; capsule.stepOffset = .22f; capsule.slopeLimit = 50; capsule.minMoveDistance = 0;
            var health = root.AddComponent<Vitality>(); health.maximum = enemy ? 40 : 100; health.team = enemy ? 1 : 0;
            if (!enemy)
            {
                var motor = root.AddComponent<CityMotor>(); motor.visual = body;
                var combat = root.AddComponent<MeleeCombat>(); combat.facing = body; combat.animator = animator;
                driver.motor = motor;
            }
            else
            {
                var ai = root.AddComponent<StreetEnemy>(); ai.visual = body; ai.animator = animator; driver.enemy = ai;
                root.AddComponent<EnemyIndicator>().ringMaterial=CityPalette.Get("Red");
            }
            body.localRotation = Quaternion.Euler(0, 180, 0);
            return root;
        }

        private static void BuildTorso()
        {
            string cloth = enemy ? "EnemyCoat" : "Jersey";
            Add(Loft("Contoured jersey", new[] {
                new Ring(.98f,.217f,.132f), new Ring(1.00f,.225f,.139f), new Ring(1.04f,.219f,.137f),
                new Ring(1.10f,.207f,.135f), new Ring(1.18f,.219f,.145f), new Ring(1.29f,.251f,.154f),
                new Ring(1.36f,.269f,.147f), new Ring(1.40f,.258f,.133f), new Ring(1.44f,.184f,.11f),
                new Ring(1.47f,.103f,.084f), new Ring(1.48f,.094f,.078f) }), cloth, TorsoWeight);
            Add(Loft("Jersey hem", new[] { new Ring(.986f,.226f,.14f), new Ring(1.008f,.225f,.14f) }), enemy ? "Wood" : "Trim", TorsoWeight);
            Add(Loft("Ribbed neckband", new[] { new Ring(1.453f,.114f,.092f), new Ring(1.468f,.106f,.088f), new Ring(1.482f,.095f,.083f) }), enemy ? "EnemyPants" : "Trim", Rigid("Chest"));
            Add(Loft("Neck", new[] { new Ring(1.46f,.075f,.071f), new Ring(1.51f,.074f,.07f), new Ring(1.57f,.073f,.069f),new Ring(1.63f,.067f,.064f),new Ring(1.66f,.060f,.058f) }), "Skin", p => Blend("Neck", "Head", Mathf.InverseLerp(1.50f,1.59f,p.y)));
            Add(Loft("Shaped shorts pelvis",new[]{new Ring(.80f,.237f,.139f),new Ring(.84f,.253f,.148f),new Ring(.90f,.247f,.151f),new Ring(.98f,.223f,.136f),new Ring(.996f,.221f,.137f)}), enemy ? "EnemyPants" : "Shorts", Rigid("Hips"));
            // Cloth seam lines follow the volume rather than flat blocks.
            foreach (int s in new[] { -1,1 })
            {
                var seam = new List<Vector3>();
                for (int i = 0; i <= 20; i++)
                {
                    float t = i / 20f;
                    seam.Add(new Vector3(s * (.207f + .04f * t),1.02f + .34f * t,.048f));
                }
                Add(Tube("Side stitching",seam.ToArray(),.0022f,6),enemy ? "Lips" : "Trim",TorsoWeight);
            }
            if (!enemy)
            {
                Add(Transform(RoundedBox("Woven crest",new Vector3(.058f,.073f,.008f),.012f),Matrix4x4.Translate(new Vector3(-.115f,1.334f,.148f))),"Trim",Rigid("Chest"));
                Add(Ellipsoid("Crest center",new Vector3(-.115f,1.337f,.155f),new Vector3(.017f,.019f,.004f),24,16),"Shorts",Rigid("Chest"));
                for(int i=0;i<4;i++) Add(Ellipsoid("Crest star",new Vector3(-.139f + i*.016f,1.386f,.141f),Vector3.one*.0034f,12,8),"Brass",Rigid("Chest"));
                // Subtle folds in the lower cloth, integrated into the shirt silhouette.
                for(int i=0;i<5;i++)
                {
                    float x=-.16f+i*.08f;
                    Add(Tube("Soft cloth fold",new[] {new Vector3(x,1.025f,.137f),new Vector3(x+.008f,1.07f,.139f),new Vector3(x+.012f,1.10f,.135f)},.002f,6,.15f),"Jersey",TorsoWeight);
                }
            }
        }

        private static void BuildFace()
        {
            Add(Loft("Sculpted head",new[] {
                new Ring(1.50f,.035f,.039f,0,.014f),new Ring(1.525f,.082f,.076f,0,.012f),
                new Ring(1.56f,.116f,.106f,0,.007f),new Ring(1.62f,.143f,.127f),
                new Ring(1.70f,.155f,.139f),new Ring(1.78f,.156f,.14f),new Ring(1.86f,.144f,.130f),
                new Ring(1.92f,.106f,.099f),new Ring(1.95f,.043f,.045f),new Ring(1.954f,.001f,.001f) },64),"Skin",Rigid("Head"));
            Add(Ellipsoid("Nose bridge",new Vector3(0,1.718f,.139f),new Vector3(.018f,.046f,.019f)),"SkinWarm",Rigid("Head"));
            Add(Ellipsoid("Nose tip",new Vector3(0,1.683f,.159f),new Vector3(.027f,.021f,.023f)),"SkinWarm",Rigid("Head"));
            foreach(int s in new[]{-1,1})
            {
                Add(Ellipsoid("Nostril",new Vector3(s*.019f,1.669f,.173f),new Vector3(.007f,.004f,.006f),24,12),"Lips",Rigid("Head"));
                Add(Ellipsoid("Ear shell",new Vector3(s*.157f,1.725f,-.007f),new Vector3(.028f,.049f,.030f)),"Skin",Rigid("Head"));
                Add(Ellipsoid("Ear concha",new Vector3(s*.177f,1.727f,.009f),new Vector3(.009f,.027f,.014f),24,16),"Ear",Rigid("Head"));
                var rim=new List<Vector3>();
                for(int i=0;i<=26;i++)
                {float a=i/26f*Mathf.PI*2;rim.Add(new Vector3(s*(.18f+.002f*Mathf.Sin(a)),1.727f+Mathf.Cos(a)*.033f,.006f+Mathf.Sin(a)*.019f));}
                Add(Tube("Helix",rim.ToArray(),.0048f,8),"SkinWarm",Rigid("Head"));
                var brow=new List<Vector3>();
                for(int i=0;i<=16;i++)
                {float t=i/16f;brow.Add(new Vector3(s*(.029f+t*.079f),1.802f+Mathf.Sin(t*Mathf.PI)*.009f-t*.009f,.125f-Mathf.Pow(t,2)*.012f));}
                Add(Tube("Eyebrow",brow.ToArray(),.0065f,8,.45f),"Hair",Rigid("Head"));
                var lid=new List<Vector3>();
                for(int i=0;i<=40;i++)
                {float a=i/40f*Mathf.PI*2;lid.Add(new Vector3(s*.066f+Mathf.Cos(a)*.039f,1.761f+Mathf.Sin(a)*.021f,.132f+Mathf.Sin(a)*.002f));}
                Add(Tube("Eyelid",lid.ToArray(),.004f,8),"SkinWarm",Rigid("Head"));
            }
            var lip=new List<Vector3>(); var lower=new List<Vector3>();
            for(int i=0;i<=24;i++)
            {
                float t=i/24f*2-1;
                lip.Add(new Vector3(t*.047f,1.613f+.003f*Mathf.Cos(t*Mathf.PI),.127f-.013f*t*t));
                lower.Add(new Vector3(t*.041f,1.606f-.004f*Mathf.Cos(t*Mathf.PI),.128f-.009f*t*t));
            }
            Add(Tube("Upper lip",lip.ToArray(),.0036f,8,.65f),"Lips",Rigid("Head"));
            Add(Tube("Lower lip",lower.ToArray(),.0042f,8,.6f),"SkinWarm",Rigid("Head"));
            if(enemy)
            {
                foreach(int s in new[]{-1,1})
                    Add(Ellipsoid("Moustache",new Vector3(s*.022f,1.637f,.132f),new Vector3(.026f,.011f,.008f)),"Hair",Rigid("Head"));
            }
        }

        private static void BuildHair()
        {
            const int sides=64, rows=20;
            var vertices=new List<Vector3>(); var uv=new List<Vector2>(); var tris=new List<int>();
            for(int row=0;row<=rows;row++)for(int s=0;s<=sides;s++)
            {
                float phi=s/(float)sides*Mathf.PI*2;
                float front=(Mathf.Sin(phi)+1)*.5f;
                float maxAngle=Mathf.Lerp(1.98f,1.10f,front);
                float theta=(.001f+row/(float)rows)*maxAngle;
                vertices.Add(new Vector3(Mathf.Sin(theta)*Mathf.Cos(phi)*.178f,1.758f+Mathf.Cos(theta)*.228f,Mathf.Sin(theta)*Mathf.Sin(phi)*.171f+.002f));
                uv.Add(new Vector2(s/(float)sides,row/(float)rows));
                if(row==rows||s==sides)continue;
                int a=row*(sides+1)+s;tris.AddRange(new[]{a,a+1,a+sides+1,a+1,a+sides+2,a+sides+1});
            }
            var cap=new Mesh{name="Fitted hair cap"};cap.SetVertices(vertices);cap.SetUVs(0,uv);cap.SetTriangles(tris,0);cap.RecalculateNormals();cap.RecalculateTangents();
            Add(cap,"Hair",Rigid("Head"));
            for(int strand=0;strand<42;strand++)
            {
                float phi=strand/42f*Mathf.PI*2;var points=new Vector3[18];
                float front=(Mathf.Sin(phi)+1)*.5f;float end=Mathf.Lerp(1.80f,1.01f,front);
                for(int j=0;j<points.Length;j++)
                {
                    float t=j/(float)(points.Length-1),theta=.15f+t*end,angle=phi+.24f*Mathf.Sin(t*Mathf.PI);
                    points[j]=new Vector3(Mathf.Sin(theta)*Mathf.Cos(angle)*.181f,1.758f+Mathf.Cos(theta)*.232f,Mathf.Sin(theta)*Mathf.Sin(angle)*.174f+.002f);
                }
                Add(Tube("Combed hair lock",points,strand%4==0?.0036f:.0028f,8,.45f),strand%4==0?"HairLight":"Hair",Rigid("Head"));
            }
        }

        private static void BuildArm(int sign)
        {
            string side=sign<0?"L":"R";
            Func<Vector3,BoneWeight> skin=p=>Blend(side+"Forearm",side+"UpperArm",Mathf.InverseLerp(1.12f,1.23f,p.y));
            Add(Loft("Continuous arm",new[]{
                new Ring(.94f,.036f,.039f,sign*.335f),new Ring(.98f,.039f,.043f,sign*.333f),
                new Ring(1.07f,.049f,.051f,sign*.329f),new Ring(1.15f,.047f,.049f,sign*.32f),
                new Ring(1.18f,.049f,.05f,sign*.319f),new Ring(1.27f,.062f,.064f,sign*.303f),
                new Ring(1.37f,.055f,.057f,sign*.285f),new Ring(1.41f,.02f,.025f,sign*.275f),new Ring(1.42f,.001f,.001f,sign*.275f)},40),"Skin",skin);
            Add(Loft("Tailored sleeve",new[]{new Ring(1.285f,.069f,.076f,sign*.305f),new Ring(1.32f,.077f,.081f,sign*.299f),new Ring(1.40f,.091f,.094f,sign*.265f),new Ring(1.448f,.085f,.088f,sign*.238f),new Ring(1.465f,.045f,.057f,sign*.221f),new Ring(1.473f,.001f,.001f,sign*.219f)}),enemy?"EnemyCoat":"Jersey",p=>Blend(side+"UpperArm","Chest",Mathf.InverseLerp(1.39f,1.46f,p.y)*.65f));
            Add(Loft("Sleeve ribbing",new[]{new Ring(1.283f,.071f,.078f,sign*.305f),new Ring(1.305f,.073f,.078f,sign*.303f)}),enemy?"EnemyPants":"Trim",Rigid(side+"UpperArm"));
            Add(Ellipsoid("Palm",new Vector3(sign*.335f,.91f,.004f),new Vector3(.044f,.062f,.026f)),"Skin",Rigid(side+"Hand"));
            for(int finger=0;finger<4;finger++)
            {
                float x=sign*(.307f+finger*.018f);float length=.050f-Mathf.Abs(finger-1.4f)*.008f;
                var path=new[]{new Vector3(x,.883f,.011f),new Vector3(x,.864f,.021f),new Vector3(x,.883f-length,.027f),new Vector3(x,.879f-length,.038f)};
                Add(Tube("Finger",path,.0095f,10,.75f),"Skin",Rigid(side+"Hand"));
                Add(Ellipsoid("Fingernail",path[3]+new Vector3(0,.005f,.002f),new Vector3(.006f,.008f,.002f),16,10),"SkinWarm",Rigid(side+"Hand"));
            }
            Add(Tube("Thumb",new[]{new Vector3(sign*.296f,.932f,.012f),new Vector3(sign*.283f,.913f,.034f),new Vector3(sign*.291f,.890f,.045f)},.013f,12,.8f),"SkinWarm",Rigid(side+"Hand"));
        }

        private static void BuildLeg(int sign)
        {
            string side=sign<0?"L":"R";float x=sign*.13f;
            Func<Vector3,BoneWeight> leg=p=>p.y<.22f?Blend(side+"Foot",side+"LowerLeg",Mathf.InverseLerp(.13f,.23f,p.y)):Blend(side+"LowerLeg",side+"UpperLeg",Mathf.InverseLerp(.45f,.60f,p.y));
            Add(Loft("Shaped leg",new[]{new Ring(.13f,.05f,.057f,x),new Ring(.19f,.051f,.058f,x),new Ring(.29f,.062f,.071f,x),new Ring(.39f,.067f,.078f,x),new Ring(.48f,.063f,.066f,x),new Ring(.54f,.067f,.070f,x),new Ring(.64f,.079f,.087f,x),new Ring(.69f,.080f,.088f,x),new Ring(.70f,.001f,.001f,x)}),enemy?"EnemyPants":"Skin",leg);
            Add(Loft("Shorts leg",new[]{new Ring(.66f,.108f,.115f,x),new Ring(.69f,.113f,.12f,x),new Ring(.80f,.121f,.135f,x),new Ring(.86f,.122f,.139f,x)}),enemy?"EnemyPants":"Shorts",p=>Blend(side+"UpperLeg","Hips",Mathf.InverseLerp(.78f,.86f,p.y)));
            Add(Loft("Short hem",new[]{new Ring(.659f,.109f,.117f,x),new Ring(.681f,.112f,.12f,x)}),enemy?"EnemyPants":"ShortSeam",Rigid(side+"UpperLeg"));
            if(!enemy)
            {
                Add(Loft("Woven sock",new[]{new Ring(.13f,.055f,.063f,x),new Ring(.19f,.055f,.064f,x),new Ring(.26f,.061f,.068f,x),new Ring(.275f,.061f,.068f,x)}),"Socks",Rigid(side+"LowerLeg"));
                Add(Loft("Sock stripe",new[]{new Ring(.246f,.061f,.069f,x),new Ring(.258f,.061f,.069f,x)}),"Trim",Rigid(side+"LowerLeg"));
            }
            Add(Transform(RoundedBox("Cushioned outsole",new Vector3(.164f,.065f,.315f),.021f,10),Matrix4x4.Translate(new Vector3(x,.045f,.064f))),"Sole",Rigid(side+"Foot"));
            Add(Transform(RoundedBox("Rubber tread",new Vector3(.164f,.018f,.31f),.008f,8),Matrix4x4.Translate(new Vector3(x,.015f,.064f))),enemy?"Metal":"Rubber",Rigid(side+"Foot"));
            var upper=Loft("Shaped sneaker upper",new[]{new Ring(-.095f,.001f,.001f),new Ring(-.088f,.044f,.050f),new Ring(-.06f,.069f,.066f),new Ring(.02f,.074f,.061f),new Ring(.12f,.074f,.047f),new Ring(.19f,.064f,.033f),new Ring(.215f,.036f,.018f),new Ring(.221f,.002f,.002f)},48);
            Transform(upper,Matrix4x4.TRS(new Vector3(x,.080f,0),Quaternion.Euler(90,0,0),Vector3.one));
            Add(upper,enemy?"Wood":"Leather",Rigid(side+"Foot"));
            for(int i=0;i<5;i++)
            {
                float z=-.025f+i*.025f;float y=.153f-i*.006f;
                Add(Tube("Lace",new[]{new Vector3(x-.045f,y-.01f,z),new Vector3(x,y+.005f,z+.008f),new Vector3(x+.045f,y-.01f,z)},.0032f,8),"Socks",Rigid(side+"Foot"));
            }
            foreach(int s in new[]{-1,1})
            {
                var path=new[]{new Vector3(x+s*.073f,.094f,.02f),new Vector3(x+s*.075f,.104f,.068f),new Vector3(x+s*.068f,.099f,.13f)};
                Add(Tube("Shoe side piping",path,.004f,8),enemy?"EnemyPants":"Trim",Rigid(side+"Foot"));
            }
        }

        private static Transform Eye(Transform body,int side)
        {
            var root=new GameObject(side<0?"LeftEye":"RightEye").transform;root.SetParent(B("Head"),false);
            root.localPosition=HeadShape.MultiplyPoint3x4(new Vector3(side*.066f,1.761f,.131f))-new Vector3(0,1.69f,0);
            root.localScale=new Vector3(.88f,.8f,.9f);
            StaticPart("Sclera",Ellipsoid("Almond eye",Vector3.zero,new Vector3(.035f,.017f,.010f)),enemy?"DarkGlass":"Eye",root,Vector3.zero);
            StaticPart("Iris",Ellipsoid("Iris",Vector3.zero,new Vector3(.012f,.014f,.0035f),32,16),"Iris",root,new Vector3(0,0,.010f));
            StaticPart("Pupil",Ellipsoid("Pupil",Vector3.zero,new Vector3(.0055f,.0065f,.002f),24,12),"Pupil",root,new Vector3(0,0,.014f));
            StaticPart("Eye glint",Ellipsoid("Eye highlight",Vector3.zero,new Vector3(.0025f,.0025f,.001f),16,10),"Eye",root,new Vector3(-.003f,.004f,.016f));
            return root;
        }
        private static void StaticPart(string name,Mesh mesh,string material,Transform parent,Vector3 localPosition)
        {
            var obj=new GameObject(name);obj.transform.SetParent(parent,false);obj.transform.localPosition=localPosition;
            obj.AddComponent<MeshFilter>().sharedMesh=Save(mesh,Prefix+"_"+name.Replace(" ","_")+"_"+bones.Count+"_"+staticCounter++);
            obj.AddComponent<MeshRenderer>().sharedMaterial=CityPalette.Get(material);
        }
        private static int staticCounter;
        private static void Text(string text,Transform parent,Vector3 position,float yaw,float size,string material)
        {
            var obj=new GameObject(text);obj.transform.SetParent(parent,false);obj.transform.localPosition=position;obj.transform.localRotation=Quaternion.Euler(0,yaw,0);
            var label=obj.AddComponent<TextMesh>();label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");label.fontSize=100;label.characterSize=size/10;
            label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;label.text=text;label.color=CityPalette.Get(material).color;
            obj.GetComponent<MeshRenderer>().sharedMaterial=label.font.material;
            obj.AddComponent<WorldLettering>().depthShader=Shader.Find("Cibio/World Lettering");
        }
        private static Transform Bone(string name,Transform parent,Vector3 position)
        {var bone=new GameObject(name).transform;bone.SetParent(parent,false);bone.localPosition=position;indices[name]=bones.Count;bones.Add(bone);return bone;}
        private static Transform B(string name)=>bones[indices[name]];
        private static BoneWeight Blend(string a,string b,float t)=>new BoneWeight{boneIndex0=indices[a],boneIndex1=indices[b],weight0=1-t,weight1=t};
        private static Func<Vector3,BoneWeight> Rigid(string bone)=>p=>new BoneWeight{boneIndex0=indices[bone],weight0=1};
        private static BoneWeight TorsoWeight(Vector3 p)=>p.y<1.16f?Blend("Hips","Spine",Mathf.InverseLerp(1,1.16f,p.y)):Blend("Spine","Chest",Mathf.InverseLerp(1.16f,1.36f,p.y));
        private static void Add(Mesh mesh,string material,Func<Vector3,BoneWeight> weights)
        {if(headMode)MeshWorkshop.Transform(mesh,HeadShape);parts.Add(new Part{mesh=mesh,material=CityPalette.Get(material),weight=weights});}
        private static void Assemble(Transform visual)
        {
            var v=new List<Vector3>();var n=new List<Vector3>();var uv=new List<Vector2>();var weights=new List<BoneWeight>();
            var mats=new List<Material>();var triangles=new List<List<int>>();
            foreach(var part in parts)
            {
                int material=mats.IndexOf(part.material);if(material<0){material=mats.Count;mats.Add(part.material);triangles.Add(new List<int>());}
                int start=v.Count;var vertices=part.mesh.vertices;v.AddRange(vertices);n.AddRange(part.mesh.normals);uv.AddRange(part.mesh.uv);
                foreach(var point in vertices)weights.Add(part.weight(point));
                foreach(int tri in part.mesh.triangles)triangles[material].Add(start+tri);
                UnityEngine.Object.DestroyImmediate(part.mesh);
            }
            var mesh=new Mesh{name=Prefix+" • smooth skinned mesh",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};
            mesh.SetVertices(v);mesh.SetNormals(n);mesh.SetUVs(0,uv);mesh.boneWeights=weights.ToArray();
            mesh.bindposes=bones.ConvertAll(b=>b.worldToLocalMatrix*visual.localToWorldMatrix).ToArray();
            mesh.subMeshCount=mats.Count;for(int i=0;i<mats.Count;i++)mesh.SetTriangles(triangles[i],i);
            mesh.RecalculateBounds();mesh.RecalculateTangents();
            var renderer=visual.gameObject.AddComponent<SkinnedMeshRenderer>();renderer.sharedMesh=Save(mesh,Prefix+"_Rigged");
            renderer.bones=bones.ToArray();renderer.rootBone=B("Hips");renderer.sharedMaterials=mats.ToArray();
            renderer.localBounds=new Bounds(Vector3.up,Vector3.one*4);renderer.updateWhenOffscreen=true;
            Debug.Log("CIBIO_ART: "+Prefix+" "+v.Count+" vertices; "+bones.Count+" bones.");
        }
    }
}
