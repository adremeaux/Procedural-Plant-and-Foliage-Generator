Shader "Unlit/PolyFill" {
    Properties {
        _Resolution("Resolution", Float) = 512
        _Blur("Blur", Range(0, 1)) = 0
        _BorderSize("BorderSize", Range(0, 200)) = 0
        _BorderBlur("BorderBlur", Range(0, 8)) = 0
        _InnerColor("InnerColor", Color) = (1, 1, 1, 1)
        _OuterColor("OuterColor", Color) = (0, 0, 0, 0)
        _BorderColor("BorderColor", Color) = (1, 1, 1, 1)
    }
    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" "RenderTexture"="True"}
        Pass {
            Blend One One
            ZWrite Off
            ColorMask RGB
            CGPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Assets/Materials/PolyFill/PolyFillFragShader.hlsl"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _Resolution;
            float _Blur;
            float _BorderSize;
            float _BorderBlur;
            float4 _InnerColor;
            float4 _OuterColor;
            float4 _BorderColor;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            //I think this shader is unused
            fixed4 frag (v2f i) : SV_Target {
                // float4 color = PolyFill_float(i.uv, _Resolution, _Blur, _BorderSize, _BorderBlur, _InnerColor, _OuterColor, _BorderColor);
                return float4(0.5, 0.1, 0.7, 0.456789);
                // return color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}