using UnityEngine;
namespace Cibio.RPG
{
    public sealed class RpgResonator08:MonoBehaviour
    {
        public int id;SpriteRenderer lamp;public SpriteRenderer EvidenceSprite{get;private set;}
        void Start()
        {
            // Preserve authored scene objects/GUIDs; replace only their runtime presentation.
            foreach(var old in GetComponentsInChildren<SpriteRenderer>())old.enabled=false;
            gameObject.name=RpgStory18.Stations[id];EvidenceSprite=RpgArt.Put(gameObject.name,RpgArt.Image(RpgStory18.Assets[id]),transform.position,RpgWorld.Order(transform.position.y),transform);
            lamp=RpgEffects.Create("Selo de investigação",transform);lamp.transform.localPosition=Vector3.up*2.2f;lamp.transform.localScale=new Vector3(.4f,.6f,1);lamp.sortingOrder=RpgWorld.Order(transform.position.y)+1;
        }
        void Update()
        {
            var g=RpgGame.Instance;if(!g||!lamp)return;bool pending=!RpgStory18.Has(g.progress,id);
            RpgEffects.Paint(lamp,"Vfx_Wisp06",Time.time*(pending?7:4),RpgArt.Hex(pending?"EDC587":"79E3BF"),pending?.65f:.35f);lamp.enabled=g.Playing||g.mode==2;
        }
    }
}
