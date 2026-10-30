Shader "Cibio/Temporal Flight 16"
{
 Properties{_MainTex("Scene",2D)="white"{} _History("Previous moment",2D)="black"{} _Strength("Dilation",Float)=0 _Clock("Orbit age",Float)=0 _Pulse("Lap rupture",Float)=0 _Focus("Protected center",Vector)=(.5,.5,0,0) _Heading("Flight",Vector)=(1,0,0,0)}
 SubShader
 {
  Cull Off ZWrite Off ZTest Always
  Pass
  {
   CGPROGRAM
   #pragma vertex vert_img
   #pragma fragment frag
   #include "UnityCG.cginc"
   sampler2D _MainTex,_History;float _Strength,_Clock,_Pulse;float2 _Focus,_Heading;
   fixed4 frag(v2f_img i):SV_Target
   {
    float2 d=i.uv-_Focus;float radius=length(d);
    // Preserve hero and the central action; stretch time only toward the periphery.
    float border=min(min(i.uv.x,1-i.uv.x),min(i.uv.y,1-i.uv.y));
    float edge=smoothstep(.16,.58,radius)*smoothstep(0,.055,border);float amount=_Strength*edge;
    float ripple=sin(radius*39-_Clock*(7+_Strength*12))*.0022*amount;
    float2 uv=clamp(i.uv+d*(dot(d,d)*.075*amount+ripple),.003,.997);
    float2 trail=(d*.026+_Heading*.009)*amount;
    float3 c=tex2D(_MainTex,uv).rgb*.46;
    c+=tex2D(_MainTex,clamp(uv-trail*.4,.003,.997)).rgb*.24;
    c+=tex2D(_MainTex,clamp(uv-trail*.9,.003,.997)).rgb*.17;
    c+=tex2D(_MainTex,clamp(uv-trail*1.6,.003,.997)).rgb*.13;
    float2 chroma=d*.010*amount;
    c.r=lerp(c.r,tex2D(_MainTex,clamp(uv+chroma,.003,.997)).r,amount*.55);
    c.b=lerp(c.b,tex2D(_MainTex,clamp(uv-chroma,.003,.997)).b,amount*.55);
    float3 past=tex2D(_History,clamp(uv-trail*2,.003,.997)).rgb;
    c=lerp(c,past,amount*.23);
    c*=1-edge*_Strength*.13;
    c+=float3(.035,.065,.09)*amount*(.3+_Pulse*.55);
    return float4(c,1);
   }
   ENDCG
  }
 }
}
