Shader "Cibio/Energy Ground Light 06"
{
 Properties{_MainTex("Sprite",2D)="white"{} _Tint("Light",Color)=(1,.7,.2,.2)}
 SubShader
 {
  Tags{"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off Blend One One
  Pass
  {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct app{float4 vertex:POSITION;float2 uv:TEXCOORD0;};struct v2f{float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;};float4 _Tint;
   v2f vert(app i){v2f o;o.vertex=UnityObjectToClipPos(i.vertex);o.uv=i.uv;return o;}
   fixed4 frag(v2f i):SV_Target{float2 p=(i.uv-.5)*2;float r=dot(p,p);float a=exp(-r*5)*smoothstep(1,.45,r)*_Tint.a;return float4(_Tint.rgb*a,a);}
   ENDCG
  }
 }
}
