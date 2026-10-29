using UnityEngine;

namespace Cibio.RPG
{
    public sealed class RpgSolid:MonoBehaviour
    {
        public Vector2 size=new(.72f,.5f),offset;
        public bool elliptical;
        public bool Blocks(Vector2 p,float radius){var b=Bounds;if(elliptical){var half=b.size*.5f+Vector2.one*radius;var d=p-b.center;return d.x*d.x/(half.x*half.x)+d.y*d.y/(half.y*half.y)<1;}var nearest=new Vector2(Mathf.Clamp(p.x,b.xMin,b.xMax),Mathf.Clamp(p.y,b.yMin,b.yMax));return (nearest-p).sqrMagnitude<radius*radius;}
        public Rect Bounds=>new((Vector2)transform.position+offset-size*.5f,size);
        void OnDrawGizmosSelected(){Gizmos.color=new Color(.5f,.9f,.8f,.7f);var b=Bounds;Gizmos.DrawWireCube(b.center,b.size);}
    }
}
