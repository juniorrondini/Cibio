using UnityEngine;

namespace Cibio.City
{
    public sealed class EnemyIndicator : MonoBehaviour
    {
        public Material ringMaterial;
        private StreetEnemy enemy;
        private Vitality health;
        private LineRenderer ring;
        private Camera view;
        private void Start()
        {
            enemy=GetComponent<StreetEnemy>();health=GetComponent<Vitality>();view=Camera.main;
            var obj=new GameObject("Aviso de ataque");obj.transform.SetParent(transform,false);obj.transform.localPosition=Vector3.up*.023f;
            ring=obj.AddComponent<LineRenderer>();ring.useWorldSpace=false;ring.loop=true;ring.widthMultiplier=.035f;ring.positionCount=64;ring.sharedMaterial=ringMaterial;
            ring.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;ring.receiveShadows=false;
            for(int i=0;i<64;i++){float a=i/64f*Mathf.PI*2;ring.SetPosition(i,new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*1.4f);}
        }
        private void Update(){if(ring)ring.enabled=enemy.Attacking&&!health.IsDead;}
        private void OnGUI()
        {
            if(!view||health.IsDead||health.Current>=health.maximum||CityHUD.Hide)return;
            Vector3 point=view.WorldToScreenPoint(transform.position+Vector3.up*2.14f);if(point.z<0)return;
            var old=GUI.color;GUI.color=new Color(.08f,.12f,.11f,.85f);GUI.DrawTexture(new Rect(point.x-22,Screen.height-point.y,44,5),Texture2D.whiteTexture);
            GUI.color=new Color(.8f,.3f,.24f);GUI.DrawTexture(new Rect(point.x-21,Screen.height-point.y+1,42*health.Current/health.maximum,3),Texture2D.whiteTexture);GUI.color=old;
        }
    }
}
