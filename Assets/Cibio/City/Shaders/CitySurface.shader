Shader "Cibio/Refined Surface"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo", 2D) = "white" {}
        _BumpMap ("Normal", 2D) = "bump" {}
        _Glossiness ("Smoothness", Range(0,1)) = .25
        _Metallic ("Metal", Range(0,1)) = 0
        _EmissionColor ("Emission", Color) = (0,0,0,0)
        _Fade ("Visibility", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 300
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow
        #pragma target 3.0
        sampler2D _MainTex, _BumpMap;
        fixed4 _Color, _EmissionColor;
        half _Glossiness, _Metallic, _Fade;
        struct Input { float2 uv_MainTex; float2 uv_BumpMap; float3 worldPos; };
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // World-space dither keeps the player visible behind foreground buildings.
            float3 grid = floor(IN.worldPos * 180);
            float dither = frac(sin(dot(grid, float3(12.9898,78.233,39.425))) * 43758.5453);
            clip(_Fade - dither * .999);
            fixed4 tex = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = tex.rgb;
            o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
            o.Metallic = _Metallic; o.Smoothness = _Glossiness;
            o.Emission = _EmissionColor.rgb; o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
