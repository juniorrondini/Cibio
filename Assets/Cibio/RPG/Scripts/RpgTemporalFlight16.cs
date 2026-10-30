using UnityEngine;

namespace Cibio.RPG
{
    [RequireComponent(typeof(Camera))]
    public sealed class RpgTemporalFlight16:MonoBehaviour
    {
        Material temporal;RenderTexture history,memory;Camera lens;int lastWrap=-1;float memoryClock;
        public bool Available=>temporal&&temporal.shader.isSupported;
        public int RenderedFrames{get;private set;}
        public int StormFrames{get;private set;}
        public float Strength=>RpgGame.Instance&&RpgGame.Instance.Secret?RpgGame.Instance.Secret.TemporalIntensity:0;
        void Awake()
        {
            lens=GetComponent<Camera>();var shader=Resources.Load<Shader>("RpgArt/TemporalFlight16");
            if(shader&&shader.isSupported)temporal=new Material(shader){hideFlags=HideFlags.HideAndDontSave};
        }
        void OnRenderImage(RenderTexture source,RenderTexture destination)
        {
            var g=RpgGame.Instance;var s=g?g.Secret:null;
            if(!Available||!g||!g.Playing||!s||!s.Active)
            {
                Graphics.Blit(source,destination);ClearHistory();return;
            }
            int w=Mathf.Max(64,source.width/2),h=Mathf.Max(36,source.height/2);
            if(!history||history.width!=w||history.height!=h)
            {
                ClearHistory();history=new RenderTexture(w,h,0,RenderTextureFormat.ARGB32){filterMode=FilterMode.Bilinear,hideFlags=HideFlags.HideAndDontSave};history.Create();Graphics.Blit(source,history);memory=new RenderTexture(w,h,0,RenderTextureFormat.ARGB32){filterMode=FilterMode.Bilinear,hideFlags=HideFlags.HideAndDontSave};memory.Create();Graphics.Blit(source,memory);memoryClock=0;
            }
            if(lastWrap!=s.WrapSerial){Graphics.Blit(source,history);Graphics.Blit(source,memory);memoryClock=0;lastWrap=s.WrapSerial;}
            var hero=g.world.hero;Vector3 point=lens.WorldToViewportPoint(hero.Position+Vector2.up*hero.flightHeight);
            temporal.SetVector("_Focus",new Vector4(Mathf.Clamp(point.x,.15f,.85f),Mathf.Clamp(point.y,.2f,.8f),0,0));
            temporal.SetVector("_Heading",new Vector4(hero.facing.x,hero.facing.y,0,0));
            var chronal=g.Chronal;float storm=chronal?chronal.Strength:0;temporal.SetFloat("_Storm",storm);temporal.SetFloat("_Rupture",chronal?chronal.Impulse:0);temporal.SetFloat("_ChronalAge",chronal?chronal.Age:0);temporal.SetTexture("_RiftTex",RpgArt.Texture("Vfx_ChronalRift17"));temporal.SetTexture("_Memory",memory);if(storm>0)StormFrames++;
            temporal.SetFloat("_Strength",Strength);temporal.SetFloat("_Clock",s.OrbitAge);temporal.SetFloat("_Pulse",s.LapPulse);temporal.SetTexture("_History",history);
            Graphics.Blit(source,destination,temporal);Graphics.Blit(source,history);memoryClock+=Time.unscaledDeltaTime;if(memoryClock>=.16f){Graphics.Blit(source,memory);memoryClock=0;}RenderedFrames++;
        }
        void ClearHistory(){if(history){history.Release();Destroy(history);history=null;}if(memory){memory.Release();Destroy(memory);memory=null;}memoryClock=0;lastWrap=-1;}
        void OnDisable(){ClearHistory();}
        void OnDestroy(){ClearHistory();if(temporal)Destroy(temporal);}
    }
}
