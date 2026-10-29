Shader "Hidden/Cibio/ColorGrade"
{
    Properties { _MainTex ("Source", 2D) = "white" {} _Exposure ("Exposure", Float) = 1 }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex; float _Exposure;
            float4 frag(v2f_img i) : SV_Target
            {
                float3 color = tex2D(_MainTex, i.uv).rgb * _Exposure;
                // Gentle filmic highlight rolloff in the project's linear color space.
                color = (color * (2.51 * color + .03)) / (color * (2.43 * color + .59) + .14);
                float luma = dot(color, float3(.2126,.7152,.0722));
                color = lerp(luma.xxx, color, 1.04);
                float2 center = i.uv * 2 - 1;
                color *= 1 - .10 * dot(center, center);
                return float4(saturate(color), 1);
            }
            ENDCG
        }
    }
}
