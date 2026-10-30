Shader "Cibio/Temporal Flight 16"
{
 Properties{_MainTex("Scene",2D)="white"{} _History("Previous moment",2D)="black"{} _Memory("Earlier moment",2D)="black"{} _RiftTex("Organic temporal rupture",2D)="black"{} _Strength("Dilation",Float)=0 _Storm("Time tunnel",Float)=0 _Rupture("Shock",Float)=0 _ChronalAge("Chronal age",Float)=0 _Clock("Orbit age",Float)=0 _Pulse("Lap rupture",Float)=0 _Focus("Protected center",Vector)=(.5,.5,0,0) _Heading("Flight",Vector)=(1,0,0,0)}
 SubShader
 {
  Cull Off ZWrite Off ZTest Always
  Pass
  {
   CGPROGRAM
   #pragma vertex vert_img
   #pragma fragment frag
   #include "UnityCG.cginc"
   sampler2D _MainTex,_History,_Memory,_RiftTex;float _Strength,_Clock,_Pulse,_Storm,_Rupture,_ChronalAge;float2 _Focus,_Heading;
   fixed4 frag(v2f_img i):SV_Target
   {
    float2 d=i.uv-_Focus;float radius=length(d);
    // Preserve hero and the central action; stretch time only toward the periphery.
    float border=min(min(i.uv.x,1-i.uv.x),min(i.uv.y,1-i.uv.y));
    float edge=smoothstep(.14,.57,radius)*smoothstep(0,.055,border);float amount=_Strength*edge;
    float tunnel=_Storm*edge;
    float ripple=sin(radius*39-_Clock*(7+_Strength*12))*(.0022*amount+.009*tunnel);
    float twist=(.045+.028*sin(_ChronalAge*3+radius*13))*tunnel;
    float sn=sin(twist),cs=cos(twist);float2 fold=float2(d.x*cs-d.y*sn,d.x*sn+d.y*cs);
    float2 uv=clamp(_Focus+fold+d*(dot(d,d)*(.075*amount+.17*tunnel)+ripple),.003,.997);
    float2 trail=(d*(.026+.07*_Storm)+_Heading*(.009+.014*_Storm))*amount;
    float3 c=tex2D(_MainTex,uv).rgb*.46;
    c+=tex2D(_MainTex,clamp(uv-trail*.4,.003,.997)).rgb*.24;
    c+=tex2D(_MainTex,clamp(uv-trail*.9,.003,.997)).rgb*.17;
    c+=tex2D(_MainTex,clamp(uv-trail*1.6,.003,.997)).rgb*.13;
    float2 chroma=d*(.010+.025*_Storm)*amount;
    c.r=lerp(c.r,tex2D(_MainTex,clamp(uv+chroma,.003,.997)).r,amount*.55);
    c.b=lerp(c.b,tex2D(_MainTex,clamp(uv-chroma,.003,.997)).b,amount*.55);
    float3 past=tex2D(_History,clamp(uv-trail*2,.003,.997)).rgb;
    c=lerp(c,past,amount*.23);
    float2 remembered=clamp(_Focus+(uv-_Focus)*(1+.09*_Storm)-_Heading*.025*_Storm,.003,.997);
    float3 memory=tex2D(_Memory,remembered).rgb;
    c=lerp(c,memory*float3(.83,.96,1.14),tunnel*(.22+_Rupture*.17));
    c*=1-edge*(_Strength*.13+_Storm*.18);
    c+=float3(.035,.065,.09)*amount*(.3+_Pulse*.55);
    float f=_Rupture>.01?min(7,floor((1-_Rupture)*8)):8+floor(fmod(_ChronalAge*8,4));
    float2 tile=float2(fmod(f,4),3-floor(f/4));
    float2 local=clamp((i.uv-_Focus)*.95+.5,0,1);
    float4 rift=tex2D(_RiftTex,(tile+local)*.25);
    float veil=rift.a*tunnel*(.45+.42*_Rupture);
    c=lerp(c,c*.62+rift.rgb*1.12,saturate(veil));
    c+=float3(.16,.21,.25)*_Rupture*tunnel*.4;
    return float4(c,1);
   }
   ENDCG
  }
 }
}
