Shader "Cibio/Organic Ground"
{
 Properties{_Grass("Grass",2D)="white"{} _Stone("Stone",2D)="white"{} _Mask("Road layout",2D)="black"{}}
 SubShader
 {
  Tags{"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off
  Pass
  {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct app{float4 vertex:POSITION;float2 uv:TEXCOORD0;};struct v2f{float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;};
   sampler2D _Grass,_Stone,_Mask;
   v2f vert(app i){v2f o;o.vertex=UnityObjectToClipPos(i.vertex);o.uv=i.uv;return o;}
   float hash(float2 p){float3 q=frac(float3(p.xyx)*.1031);q+=dot(q,q.yzx+33.33);return frac((q.x+q.y)*q.z);}
   fixed4 frag(v2f i):SV_Target
   {
    float2 p=i.uv*float2(48,36);float4 m=tex2D(_Mask,i.uv);float2 uv=p/7;
    float3 grass=tex2D(_Grass,uv).rgb*.83;float3 stone=tex2D(_Stone,p/6).rgb;
    float grain=hash(floor(p*48));float3 earth=lerp(float3(.30,.238,.15),float3(.48,.387,.25),grain);
    earth*=.92+.08*sin(p.x*3+sin(p.y*2));float3 col=lerp(grass,earth,m.r*.91);col=lerp(col,stone*.94,m.g);
    return fixed4(col,1);
   }
   ENDCG
  }
 }
}
