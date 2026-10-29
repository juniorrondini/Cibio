Shader "Cibio/Volumetric Energy 06"
{
 Properties{_MainTex("Sprite",2D)="white"{} _Atlas("Animation",2D)="black"{} _Frame("Frame",Float)=0 _Grid("Grid",Vector)=(4,4,16,0) _Tint("Tint",Color)=(1,.8,.4,1) _Intensity("Intensity",Float)=1 _Phase("Phase",Float)=0 _Veil("Front veil",Float)=0 _Filament("Filaments only",Float)=0}
 SubShader
 {
  Tags{"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="False"}
  Cull Off ZWrite Off Lighting Off Blend One OneMinusSrcAlpha
  Pass
  {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct app{float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};struct v2f{float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
   sampler2D _Atlas;float4 _Tint,_Grid;float _Frame,_Intensity,_Phase,_Veil,_Filament;
   v2f vert(app i){v2f o;o.vertex=UnityObjectToClipPos(i.vertex);o.uv=i.uv;o.color=i.color;return o;}
   float4 frame(float2 uv,float f){f=fmod(max(0,f),_Grid.z);float2 cell=float2(fmod(f,_Grid.x),_Grid.y-1-floor(f/_Grid.x));return tex2D(_Atlas,(cell+uv)/_Grid.xy);}
   fixed4 frag(v2f i):SV_Target
   {
    float2 uv=i.uv;float t=_Time.y+_Phase;uv.x+=sin(uv.y*11-t*2.6)*.007+sin(uv.y*23-t*1.7)*.003;uv.y+=sin(uv.x*12+t*2)*.004;
    float edge=smoothstep(0,.05,uv.x)*smoothstep(0,.05,1-uv.x)*smoothstep(0,.05,uv.y)*smoothstep(0,.05,1-uv.y);uv=clamp(uv,.004,.996);
    float4 a=frame(uv,floor(_Frame)),b=frame(uv,floor(_Frame)+1);float alpha=lerp(a.a,b.a,frac(_Frame));
    float3 rgb=lerp(a.rgb*a.a,b.rgb*b.a,frac(_Frame))/max(alpha,.0001);float lum=max(rgb.r,max(rgb.g,rgb.b));
    float3 color=lerp(lum.xxx,rgb,.30)*lerp(_Tint.rgb,float3(1,.96,.82),pow(lum,5)*.35);
    // Light may pass over the torso; the face remains readable through the front layer.
    float face=exp(-pow((uv.x-.5)/.22,2)-pow((uv.y-.44)/.13,2));alpha*=lerp(1,1-face*.78,_Veil);
    alpha*=pow(saturate((lum-.08)/.92),.8)*lerp(1,smoothstep(.55,.9,lum),_Filament);
    alpha=saturate(alpha*edge*_Tint.a*i.color.a*_Intensity*.76);return float4(color*alpha*1.35,alpha*.65);
   }
   ENDCG
  }
 }
}

