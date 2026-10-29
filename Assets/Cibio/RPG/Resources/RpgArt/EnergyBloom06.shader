Shader "Cibio/Energy Bloom 06"
{
 Properties{_MainTex("Image",2D)="black"{} _GlowTex("Glow",2D)="black"{} _Direction("Blur",Vector)=(1,0,0,0) _Strength("Strength",Float)=.32}
 SubShader
 {
  Cull Off ZWrite Off ZTest Always
  CGINCLUDE
  #include "UnityCG.cginc"
  sampler2D _MainTex,_GlowTex;float4 _MainTex_TexelSize;float2 _Direction;float _Strength;
  fixed4 blur(v2f_img i):SV_Target
  {
   float2 step=_MainTex_TexelSize.xy*_Direction;float4 c=tex2D(_MainTex,i.uv)*.227027;
   c+=(tex2D(_MainTex,i.uv+step*1.384615)+tex2D(_MainTex,i.uv-step*1.384615))*.316216;
   c+=(tex2D(_MainTex,i.uv+step*3.230769)+tex2D(_MainTex,i.uv-step*3.230769))*.070270;return c;
  }
  fixed4 combine(v2f_img i):SV_Target{float4 c=tex2D(_MainTex,i.uv);float3 glow=tex2D(_GlowTex,i.uv).rgb*_Strength;return float4(c.rgb+glow*(1-c.rgb*.55),c.a);}
  ENDCG
  Pass
  {
   CGPROGRAM
   #pragma vertex vert_img
   #pragma fragment blur
   ENDCG
  }
  Pass
  {
   CGPROGRAM
   #pragma vertex vert_img
   #pragma fragment combine
   ENDCG
  }
 }
}
