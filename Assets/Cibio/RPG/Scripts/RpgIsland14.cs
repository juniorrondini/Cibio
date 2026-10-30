using UnityEngine;
namespace Cibio.RPG
{
 public static class RpgIsland14
 {
  public static readonly Rect Bounds=new(-48,-28,96,112),Pier=new(35,8.5f,11,3);
  public static float CoastDistance(Vector2 p)
  {
   Vector2 q=new(Mathf.Abs(p.x)-34,Mathf.Abs(p.y-27)-42);
   return new Vector2(Mathf.Max(q.x,0),Mathf.Max(q.y,0)).magnitude+Mathf.Min(Mathf.Max(q.x,q.y),0)-9+.8f*Mathf.Sin(p.y*.27f)+.55f*Mathf.Sin(p.x*.23f+p.y*.15f);
  }
  public static bool OnPier(Vector2 p,float radius=0)=>p.x>=Pier.xMin+radius&&p.x<=Pier.xMax-radius&&p.y>=Pier.yMin+radius&&p.y<=Pier.yMax-radius;
  public static bool OnLand(Vector2 p,float radius=.22f)=>CoastDistance(p)<-radius||OnPier(p,radius);
  public static string Region(Vector2 p)
  {
   if(OnPier(p))return "Trapiche do Trabalho";if(Vector2.Distance(p,new Vector2(25,69))<12)return "Ponta do Farol";
   if(p.x>32&&p.y<32&&p.y>5)return "Porto do Trabalho";if(CoastDistance(p)>-5)return "Praia do Trabalhador";
   if(p.y>55)return "Bosque do Refúgio";if(p.x<-33)return "Caminho da Maré";
   return p.y>42?"Esplanada":Mathf.Abs(p.x)>23?"Caminho do Trabalhador":p.y>18?"Distrito do Poder":"Bairro do Trabalhador";
  }
  public static Color MapColor(Vector2 p){float d=CoastDistance(p);return OnPier(p)?RpgArt.Hex("96734F"):d>0?RpgArt.Hex("255F70"):d>-4?RpgArt.Hex("B4A47B"):RpgArt.Hex("354C3D");}
 }
}
