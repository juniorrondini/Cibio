using UnityEngine;

namespace Cibio.RPG
{
    public sealed class RpgSolid:MonoBehaviour
    {
        public Vector2 size=new(.72f,.5f),offset;
        public Rect Bounds=>new((Vector2)transform.position+offset-size*.5f,size);
        void OnDrawGizmosSelected(){Gizmos.color=new Color(.5f,.9f,.8f,.7f);var b=Bounds;Gizmos.DrawWireCube(b.center,b.size);}
    }
}
