Shader "Cibio/Power Sprite"
{
 Properties{_MainTex("Sprite",2D)="white"{} _Tint("Tint",Color)=(1,1,1,1) _TransformAtlas("Transformation",2D)="black"{} _ArtMix("New art",Float)=0 _Frame("Frame",Float)=0 _Direction("Direction",Float)=0 _Crimson("Crimson accent",Range(0,1))=0}
 SubShader
 {
  Tags{"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
  Pass
  {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct app{float4 vertex:POSITION;float2 uv:TEXCOORD0;float2 local:TEXCOORD1;};struct v2f{float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float2 local:TEXCOORD1;};sampler2D _MainTex,_TransformAtlas;float4 _Tint;float _ArtMix,_Frame,_Direction,_Crimson;
   v2f vert(app i){v2f o;o.vertex=UnityObjectToClipPos(i.vertex);o.uv=i.uv;o.local=i.local;return o;}
   float4 frame(float2 uv,float f){return tex2D(_TransformAtlas,float2((clamp(f,0,5)+uv.x)/6,(3-_Direction+uv.y)/4));}
   fixed4 frag(v2f i):SV_Target
   {
    float4 a=tex2D(_MainTex,i.uv),b=lerp(frame(i.local,floor(_Frame)),frame(i.local,ceil(_Frame)),frac(_Frame));
    // Interpolate premultiplied colors, so transparent borders never create dark fringes.
    float alpha=lerp(a.a,b.a,_ArtMix);float3 rgb=lerp(a.rgb*a.a,b.rgb*b.a,_ArtMix)/max(alpha,.0001);float warm=saturate((rgb.r-rgb.b)*2.1)*smoothstep(.35,.8,i.local.y);rgb=lerp(rgb,rgb*float3(1.08,.67,.78),_Crimson*warm*.5);return float4(rgb,alpha)*_Tint;
   }
   ENDCG
  }
 }
}
