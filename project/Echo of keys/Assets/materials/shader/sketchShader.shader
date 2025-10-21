Shader "Custom/sketchShader"
{
    Properties
    {
        _BrightColor("Bright Color", Color) = (1,1,1,1)
        _MidColor("Mid Color", Color) = (0.5,0.5,0.5,1)
        _DarkColor("Dark Color", Color) = (0,0,0,1)
        _SheenColor("Sheen Color", Color) = (1,1,0,1)
        _SheenPower("Sheen Power", Range(0,5)) = 2.0
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
            };

            float4 _BrightColor;
            float4 _MidColor;
            float4 _DarkColor;
            float4 _SheenColor;
            float _SheenPower;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.worldPos = worldPos;
                o.normal = mul(unity_ObjectToWorld, float4(v.normal, 0)).xyz;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 LIGHT_COORDS = TransformWorldToShadowCoord(i.worldPos);
                Light mainLight = GetMainLight(LIGHT_COORDS);
                half shadow = MainLightRealtimeShadow(LIGHT_COORDS);
                float NdotL = dot(normalize(i.normal), mainLight.direction);
                float4 color;
                if (NdotL < 0.0)
                    color = _DarkColor;
                else if (NdotL < 0.5)
                    color = lerp(_DarkColor, _MidColor, NdotL * 2.0);
                else
                    color = lerp(_MidColor, _BrightColor, (NdotL - 0.5) * 2.0);
                //sheen according to camera angle
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos.xyz);
                float viewDot = dot(normalize(i.normal), viewDir);
                color = lerp(color, _DarkColor, 1.0 - shadow);
                color += _SheenColor * pow(1.0 - abs(viewDot), _SheenPower);
                color.a = 1.0;
                return color;
            }
            ENDHLSL
        }
        Pass
       {
           Name "ShadowCast"
           Tags { "LightMode" = "ShadowCaster" }
           HLSLPROGRAM
           #pragma vertex vert
           #pragma fragment frag
           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
           struct appdata
           {
               float4 vertex : POSITION;
           };
           struct v2f
           {
               float4 pos : SV_POSITION;
           };
           v2f vert(appdata v)
           {
               v2f o;
               o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
               return o;
           }
           float4 frag(v2f i) : SV_Target
           {
               return float4(0.0, 0.0, 0.0, 1.0);
           }
           ENDHLSL
       }
    }
}