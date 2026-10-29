using UnityEngine;

namespace Cibio.RPG
{
    public sealed class RpgShrine:MonoBehaviour
    {
        public int id;public string title,description;public Color color;[SerializeField] SpriteRenderer crystal;float phase;
        public void Initialize()
        {
            RpgArt.Put("Pedestal antigo",RpgArt.Tile("terrain_atlas",544,384,32,64),transform.position,RpgWorld.Order(transform.position.y),transform);
            crystal=RpgArt.Put("Centelha",RpgArt.Pixel,(Vector2)transform.position+Vector2.up*1.8f,RpgWorld.Order(transform.position.y)+1,transform,color);
            crystal.transform.localScale=new Vector3(.32f,.52f,1);crystal.transform.rotation=Quaternion.Euler(0,0,45);phase=id*2;
        }
        void Start()
        {
            if(!crystal)return;crystal.enabled=false;crystal.gameObject.layer=RpgEnergyParticles.Layer;crystal.name="Centelha • chama viva";crystal.sprite=RpgArt.Pixel;crystal.sharedMaterial=Resources.Load<Material>("RpgArt/EnergyVolume06");crystal.transform.localRotation=Quaternion.identity;crystal.transform.localScale=new Vector3(.65f,.95f,1);
        }
        void Update()
        {
            if(!crystal)return;float t=Time.unscaledTime+id*2;crystal.transform.localPosition=new Vector3(0,1.8f+Mathf.Sin(t*1.7f)*.08f,0);crystal.color=Color.white;RpgEffects.Paint(crystal,"Vfx_Wisp06",t*10,color,1);
        }
    }
}
