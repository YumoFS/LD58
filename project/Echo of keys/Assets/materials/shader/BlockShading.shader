Shader "Custom/BlockShader"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _TopColor("Top Color", Color) = (0,1,0,1)
        _FrontColor("Front Color", Color) = (0,1,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD0;
            };

            fixed4 _BaseColor;
            fixed4 _TopColor;
            fixed4 _FrontColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 如果法线接近 Vector3.up（0,1,0）
                float upDot = dot(normalize(i.normal), float3(0,1,0));
                float rightDot = dot(normalize(i.normal), float3(0,0,-1));
                if(upDot > 0.9) // 阈值，越接近1说明越向上
                    return _TopColor;
                else if (rightDot > 0.9)
                    return _FrontColor;
                else
                    return  _BaseColor;
            }
            ENDCG
        }
         Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ColorMask 0
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return 0;
            }
            ENDCG
        }
    }
}