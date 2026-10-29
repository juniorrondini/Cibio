Shader "Cibio/Energy Particles 06"
{
 Properties{_MainTex("Wisp atlas",2D)="black"{}}
 SubShader
 {
  Tags{"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True"}
  Cull Off ZWrite Off Lighting Off Blend One OneMinusSrcAlpha
  Pass
  {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct app{float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};struct v2f{float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};sampler2D _MainTex;
   v2f vert(app i){v2f o;o.vertex=UnityObjectToClipPos(i.vertex);o.uv=i.uv;o.color=i.color;return o;}
   fixed4 frag(v2f i):SV_Target{float4 c=tex2D(_MainTex,i.uv);float l=max(c.r,max(c.g,c.b));float3 rgb=lerp(l.xxx,c.rgb,.2)*lerp(i.color.rgb,float3(1,.96,.84),pow(l,4)*.35);float a=c.a*i.color.a*pow(l,.8);return float4(rgb*a*1.3,a*.65);}
   ENDCG
  }
 }
}
