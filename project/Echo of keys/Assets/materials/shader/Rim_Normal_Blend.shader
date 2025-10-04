Shader "Custom/URP_Rim_Normal_Blend"
{
    Properties
    {
        _MainColor("Main Color", Color) = (1,1,1,1)
        _RimColor("Rim Color", Color) = (0,1,0,1)
        _RimPower("Rim Power", Range(1,10)) = 2
        _Emiss("Emiss Strength", Float) = 1
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalRenderPipeline" "Queue"="Transparent" "RenderType"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "RimLightPass"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 normalWS     : TEXCOORD0;
                float3 viewDirWS    : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _MainColor;
                float4 _RimColor;
                float _RimPower;
                float _Emiss;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS);

                float3 positionWS = TransformObjectToWorld(IN.positionOS).xyz;
                OUT.normalWS = normalize(TransformObjectToWorldNormal(IN.normalOS));
                OUT.viewDirWS = normalize(_WorldSpaceCameraPos.xyz - positionWS);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 N = normalize(IN.normalWS);
                float3 V = normalize(IN.viewDirWS);

                float NdotV = saturate(dot(N, V));

                // rim term
                float rim = pow(1.0 - NdotV, _RimPower);

                float3 baseColor = _MainColor.rgb;
                float3 rimColor = _RimColor.rgb * rim * _Emiss;

                float3 finalColor = baseColor + rimColor;
                float alpha = saturate(rim * _Emiss);

                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
}