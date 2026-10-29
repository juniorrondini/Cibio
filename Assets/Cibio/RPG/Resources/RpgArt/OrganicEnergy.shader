Shader "Cibio/Organic Energy"
{
 Properties { _MainTex("Sprite",2D)="white"{} _Atlas("Frames",2D)="white"{} _Frame("Frame",Float)=0 _Tint("Color",Color)=(1,.8,.4,1) _Intensity("Intensity",Float)=1 _Phase("Phase",Float)=0 }
 SubShader
 {
  Tags {"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="False"}
  Cull Off ZWrite Off Lighting Off Blend One One
  Pass
  {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct data {float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
   struct result {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
   sampler2D _Atlas;float4 _Tint;float _Frame,_Intensity,_Phase;
   result vert(data v){result o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
   float4 frame(float2 uv,float f){f=fmod(f,8);return tex2D(_Atlas,float2((fmod(f,4)+uv.x)*.25,(1-floor(f/4)+uv.y)*.5));}
   fixed4 frag(result i):SV_Target
   {
    float2 uv=i.uv;uv.x+=sin(uv.y*13-_Time.y*3+_Phase)*.012;uv.y+=sin(uv.x*11+_Time.y*2+_Phase)*.007;
    float edge=smoothstep(0,.055,uv.x)*smoothstep(0,.055,1-uv.x)*smoothstep(0,.045,uv.y)*smoothstep(0,.045,1-uv.y);uv=clamp(uv,.004,.996);
    float4 c=lerp(frame(uv,floor(_Frame)),frame(uv,floor(_Frame)+1),frac(_Frame));float l=max(c.r,max(c.g,c.b));float3 rgb=lerp(l.xxx,c.rgb,.22)*_Tint.rgb;
    float a=c.a*edge*_Tint.a*i.color.a*_Intensity;return float4(rgb*a*1.8,a);
   }
   ENDCG
  }
 }
}
