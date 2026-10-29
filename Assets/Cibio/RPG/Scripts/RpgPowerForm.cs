using System.Collections;
using UnityEngine;
namespace Cibio.RPG
{
 public sealed class RpgPowerForm:MonoBehaviour
 {
  RpgActor actor;Mesh mesh;MeshRenderer form;Material material;Vector3[] vertices=new Vector3[45];Vector2[] uvs=new Vector2[45],normalized=new Vector2[45];float strength,artMix,ascendantBlend;
  public float Strength=>strength;
  void Awake()
  {
   actor=GetComponent<RpgActor>();var child=new GameObject("Forma desperta • torso e porte");child.transform.SetParent(actor.body.transform,false);
   mesh=new Mesh{name="Corpo articulado • UVs preservadas"};int[] triangles=new int[192];int q=0;
   for(int y=0;y<8;y++)for(int x=0;x<4;x++){int k=y*5+x;triangles[q++]=k;triangles[q++]=k+6;triangles[q++]=k+1;triangles[q++]=k;triangles[q++]=k+5;triangles[q++]=k+6;}
   mesh.vertices=vertices;mesh.uv=uvs;mesh.triangles=triangles;mesh.MarkDynamic();child.AddComponent<MeshFilter>().sharedMesh=mesh;form=child.AddComponent<MeshRenderer>();material=new Material(Resources.Load<Shader>("RpgArt/PowerSprite"));material.SetTexture("_TransformAtlas",Resources.Load<Texture2D>("RpgArt/Transform_Sequence05"));form.sharedMaterial=material;form.enabled=false;
  }
  void LateUpdate(){Refresh(Time.deltaTime);}
  public void Refresh(float dt)
  {
   ascendantBlend=Mathf.MoveTowards(ascendantBlend,actor.FormStage==2?1:0,dt*2.5f);bool active=actor.buff>0&&!actor.dead;strength=Mathf.MoveTowards(strength,active?1:0,dt*(active?2.4f:3.5f));
   actor.body.transform.localScale=new Vector3(1+strength*(.14f+.06f*ascendantBlend),1+strength*(.18f+.06f*ascendantBlend),1);actor.shadow.transform.localScale=new Vector3(.72f*(1+strength*.3f)*Mathf.Lerp(1,.55f,actor.flightHeight/2),.12f*(1+strength*.1f)*Mathf.Lerp(1,.6f,actor.flightHeight/2),1);
   actor.shadow.color=new Color(.04f,.08f,.07f,Mathf.Lerp(.3f,.13f,actor.flightHeight/2));
   bool show=strength>.005f;form.enabled=show;actor.body.enabled=!show;if(!show||!actor.body.sprite)return;
   var s=actor.body.sprite;Rect r=s.rect;var tex=s.texture;float age=actor.TransformationDuration-actor.transforming;
   bool evolved=actor.FormStage==2;material.SetTexture("_TransformAtlas",Resources.Load<Texture2D>("RpgArt/"+(evolved?"Transform_Ascendant09":"Transform_Sequence05")));material.SetFloat("_TransformColumns",evolved?8:6);
   float desiredArt=actor.transforming>0?(evolved?1:Mathf.SmoothStep(0,1,age/.10f)):0;artMix=actor.transforming>0&&evolved?1:Mathf.MoveTowards(artMix,desiredArt,dt*12);
   float frame=actor.transforming>0?(evolved?7*Mathf.SmoothStep(0,1,age/1.5f):1+4*Mathf.SmoothStep(0,1,age/1.02f)):evolved?7:5;
   for(int y=0;y<=8;y++)for(int x=0;x<=4;x++){float u=x/4f,v=y/8f,localY=v*(r.width/s.pixelsPerUnit)-.32f;int k=y*5+x;vertices[k]=new Vector3((u-.5f)*(r.width/s.pixelsPerUnit),localY,0);uvs[k]=new Vector2((r.x+u*r.width)/tex.width,(r.y+v*r.height)/tex.height);normalized[k]=new Vector2(u,v);}
   mesh.vertices=vertices;mesh.uv=uvs;mesh.uv2=normalized;mesh.RecalculateBounds();material.SetTexture("_MainTex",tex);material.SetColor("_Tint",actor.body.color);material.SetFloat("_Crimson",0);material.SetFloat("_ArtMix",artMix);material.SetFloat("_Frame",frame);material.SetFloat("_Direction",actor.direction);form.sortingOrder=actor.body.sortingOrder;form.sortingLayerID=actor.body.sortingLayerID;
  }
  public void Awaken(){StopAllCoroutines();StartCoroutine(Eruption());}
  IEnumerator Eruption()
  {
   yield return new WaitForSeconds(actor.FormStage==2?.55f:.24f);if(actor.dead||actor.buff<=0)yield break;
   var game=RpgGame.Instance;game.shake=actor.FormStage==2?1.3f:1.65f;if(actor.FormStage==2)game.GetComponent<RpgFeedback09>().Flash(.16f);RpgEffects.Burst(actor.Position,actor.EnergyColor,actor.FormStage==2?5.3f:4.6f,.7f);
   foreach(var e in actor.world.enemies)if(!e.dead&&Vector2.Distance(actor.Position,e.Position)<3.2f){Vector2 away=(e.Position-actor.Position).normalized;e.transform.position=actor.world.Move(e.Position,away*1.15f);e.enemyCooldown=Mathf.Max(e.enemyCooldown,1.3f);e.attackTimer=0;}
   for(int i=0;i<20;i++)RpgEffects.Spark(actor.Position+Vector2.up*Random.Range(.2f,1.5f),actor.EnergyColor,Random.Range(1.7f,2.8f));
   yield return new WaitForSeconds(.36f);if(actor.dead||actor.buff<=0)yield break;RpgEffects.Burst(actor.Position,actor.EnergyColor,3.4f,.55f);game.shake=Mathf.Max(game.shake,.9f);
  }
  void OnDestroy(){if(mesh)Destroy(mesh);if(material)Destroy(material);}
 }
}
