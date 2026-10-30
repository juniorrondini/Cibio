Shader "Cibio/Organic Ground"
{
 Properties{_Grass("Grass",2D)="white"{} _Stone("Stone",2D)="white"{} _Earth("Earth",2D)="white"{} _Damp("Damp",2D)="white"{} _Mask("Road layout",2D)="black"{} _Sea("Sea",2D)="white"{} _Sand("Sand",2D)="white"{} _Wood("Wood",2D)="white"{} _CoastRock("Coast rock",2D)="white"{}}
 SubShader
 {
  Tags{"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off
  Pass
  {
   CGPROGRAM
   #pragma target 3.0
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct app{float4 vertex:POSITION;float2 uv:TEXCOORD0;};struct v2f{float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;};
   sampler2D _Grass,_Stone,_Earth,_Damp,_Mask,_Sea,_Sand,_Wood,_CoastRock;float4 _WorldBounds;float _Coastal;
   v2f vert(app i){v2f o;o.vertex=UnityObjectToClipPos(i.vertex);o.uv=i.uv;return o;}
   float hash(float2 p){float3 q=frac(float3(p.xyx)*.1031);q+=dot(q,q.yzx+33.33);return frac((q.x+q.y)*q.z);}
   float path(float2 p,float2 a,float2 b,float width){float2 d=b-a;float u=saturate(dot(p-a,d)/dot(d,d));float edge=length(p-(a+u*d));float irregular=sin(p.x*1.1+sin(p.y*.7))*.09+sin(p.y*1.7)*.06;return 1-smoothstep(width-.22,width+.45,edge+irregular);}
   float3 surface(sampler2D tex,float2 p,float tile){float3 a=tex2D(tex,p/tile).rgb;float3 b=tex2D(tex,(p.yx+float2(13.7,8.1))/tile).rgb;float blend=.5+.5*sin(p.x*.39+sin(p.y*.23));return lerp(a,b,blend*.52);}
   float3 grassTile(float2 p){float2 uv=frac(p/6.5);float2 w=sin(uv*3.14159265);w*=w;float3 a=tex2D(_Grass,uv).rgb,b=tex2D(_Grass,frac(uv+float2(.5,0))).rgb,c=tex2D(_Grass,frac(uv+float2(0,.5))).rgb,d=tex2D(_Grass,frac(uv+.5)).rgb;return lerp(lerp(d,c,w.x),lerp(b,a,w.x),w.y);}
   float coast(float2 p){float2 q=abs(p-float2(0,27))-float2(34,42);return length(max(q,0))+min(max(q.x,q.y),0)-9+.8*sin(p.y*.27)+.55*sin(p.x*.23+p.y*.15);}
   fixed4 frag(v2f i):SV_Target
   {
    float2 world=_WorldBounds.xy+i.uv*_WorldBounds.zw;float2 p=world+float2(24,18);float2 baseUV=p/float2(48,36);float4 m=tex2D(_Mask,baseUV);
    float road=1-smoothstep(1.05,1.85,abs(world.x-sin(world.y*.19)*.65));
    float cross=1-smoothstep(1.1,1.8,min(abs(world.y-24),min(abs(world.y-29),abs(world.y-37))));
    cross*=1-smoothstep(11,15,abs(world.x));float plaza=1-smoothstep(4.4,5.3,length(world-float2(0,36.5)));
    float north=smoothstep(15,18,world.y);m=lerp(m,float4(max(road,cross),plaza,0,0),north);
    float added=path(world,float2(-30,-.5),float2(30,-.5),1.15);
    added=max(added,path(world,float2(18,16),float2(29,16),1.1));added=max(added,path(world,float2(29,-6),float2(29,37),1.0));
    added=max(added,path(world,float2(-18,29),float2(-29,29),1.1));added=max(added,path(world,float2(-29,-6),float2(-29,34),1.0));
    added=max(added,path(world,float2(-29,34),float2(-20,39),1.1));added=max(added,path(world,float2(-20,39),float2(-10,46),1.1));
    added=max(added,path(world,float2(29,36),float2(17,45),1.1));added=max(added,path(world,float2(17,45),float2(8,46),1.1));
    float palmeiras=1-smoothstep(.9,1.08,length((world-float2(0,46))/float2(8,4.1)));m.r=max(m.r,added);m.g=max(m.g,palmeiras);
    if(_Coastal>.5){
     added=max(added,path(world,float2(8,46),float2(9,56),1.35));added=max(added,path(world,float2(9,56),float2(0,63),1.35));added=max(added,path(world,float2(-8,46),float2(-9,56),1.35));added=max(added,path(world,float2(-9,56),float2(0,63),1.35));added=max(added,path(world,float2(0,63),float2(25,68),1.25));
     added=max(added,path(world,float2(0,63),float2(-30,68),1.2));added=max(added,path(world,float2(-30,68),float2(-35,52),1.15));
     added=max(added,path(world,float2(29,29),float2(36,29),1.15));added=max(added,path(world,float2(36,29),float2(39,50),1.1));added=max(added,path(world,float2(39,50),float2(28,68),1.15));
     added=max(added,path(world,float2(29,16),float2(38,16),1.2));added=max(added,path(world,float2(38,16),float2(38,10),1.25));
     added=max(added,path(world,float2(-29,16),float2(-37,16),1.2));added=max(added,path(world,float2(-37,16),float2(-39,42),1.1));added=max(added,path(world,float2(0,-16),float2(0,-21),1.15));m.r=max(m.r,added);
    }
    float3 grass=grassTile(p)*.94;float3 stone=surface(_Stone,p,2.4)*.91;float3 earth=surface(_Earth,p,5)*.89;
    float damp=1-smoothstep(3.2,6.0,length((world-float2(-26,44))/float2(1.25,1)));grass=lerp(grass,surface(_Damp,p,6)*.96,damp*.62);
    float3 col=lerp(grass,earth,saturate(m.r)*.97);col=lerp(col,stone,saturate(m.g));
    if(_Coastal>.5){
     float d=coast(world);if(d>-5.5){float3 sand=surface(_Sand,world+31,5.2)*.88;float3 rock=surface(_CoastRock,world,4.8)*.85;
     float wave=sin(world.x*.44+world.y*.27-_Time.y*.65)*.025;float3 sea=surface(_Sea,world+float2(_Time.y*.035,_Time.y*.022),9)*lerp(.56,.93,saturate(1-d/7));
     col=lerp(col,sand,smoothstep(-5.2,-3.2,d));float rocky=smoothstep(59,71,world.y)*smoothstep(-4.5,-2,d)*(1-smoothstep(-1.6,-.4,d));col=lerp(col,rock,rocky*.78);
     col=lerp(col,sand*.68,smoothstep(-.8,-.1,d)*(1-smoothstep(-.05,.25,d))*.65);col=lerp(col,sea,smoothstep(-.05,.25,d));
     float foam=(1-smoothstep(.12,.72,abs(d-(.3+.22*sin(_Time.y*.75+world.y*.11)))))*(.4+.2*sin(world.x*.9+world.y*.4-_Time.y));col=lerp(col,float3(.78,.9,.84),foam*smoothstep(-.1,.22,d));}
    }
    return fixed4(col,1);
   }
   ENDCG
  }
 }
}
