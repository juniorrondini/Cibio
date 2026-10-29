Shader "Cibio/Soft Contact Shadow"
{
 Properties{_MainTex("Sprite",2D)="white"{}}
 SubShader
 {
  Tags{"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
  Pass
  {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct app{float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};struct v2f{float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
   v2f vert(app i){v2f o;o.vertex=UnityObjectToClipPos(i.vertex);o.uv=i.uv;o.color=i.color;return o;}
   fixed4 frag(v2f i):SV_Target{float2 p=(i.uv-.5)*2;return float4(i.color.rgb,i.color.a*pow(saturate(1-dot(p,p)),2.2));}
   ENDCG
  }
 }
}
