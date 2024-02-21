Shader "Unlit/TextureFactoryShader" {
    Properties {
        _Resolution("Resolution", Float) = 512
        _BenchmarkLoops("BenchmarkLoops", Float) = 0.
        _Downsample("Downsample", Float) = 1
        _BaseColor("BaseColor", Color) = (1, 1, 1, 1)
        _DarkerColor("DarkerColor", Color) = (1, 1, 1, 1)
        _ShadowColor("ShadowColor", Color) = (1, 1, 1, 1)
        _VeinsTex ("VeinsTex", 2D) = "white" {} 
    }
    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" "RenderTexture"="True"}
        Pass {
            // Blend One One
            ZWrite Off
            ColorMask RGB

            CGPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Assets/Materials/PolyFill/PolyFillFragShader.hlsl"
            #include "Assets/Materials/PolyFill/BlendModesComposite.hlsl"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            StructuredBuffer<float2> _Corners;
            StructuredBuffer<float2> _BaseShadowCorners;
            StructuredBuffer<float2> _VeinCorners;

            float _Resolution;
            float _BenchmarkLoops;
            float _Downsample;
            float4 _BaseColor;
            float4 _DarkerColor;
            float4 _ShadowColor;
            sampler2D _VeinsTex;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                uint Overwrite = 0;
                uint Darken = 1;
                uint Multiply = 2;
                uint ColorBurn = 3;
                uint LinearBurn = 4;
                uint Lighten = 5;
                uint Screen = 6;
                uint ColorDodge = 7;
                uint LinearDodge = 8;
                uint Overlay = 9;
                uint SoftLight = 10;
                uint HardLight = 11;
                uint VividLight = 12;
                uint LinearLight = 13;
                uint PinLight = 14;
                uint HardMix = 15;
                uint Difference = 16;
                uint Exclusion = 17;
                uint Subtrac = 18;
                uint Divide = 19;

                float4 black = float4(0., 0., 0., 1.0);
                float4 clear = float4(0., 0., 0., 0.0);
                float4 white = float4(1., 1., 1., 1.0);
                float4 red = float4(1., 0., 0, 1.0);
                float4 blue = float4(0., 0., 1., 1.);
                float4 color;
                float4 color2;

                /*float precalcDist,
                    float2 fragCoord,
                    float resolution,
                    float blur, 
                    float borderSize, 
                    float borderBlur, 
                    float4 innerColor, 
                    float4 outerColor, 
                    float4 borderColor)*/

                float2 pt = (2. * i.uv - 1.);
                float mainEdgeDist = DistFromPoly(_Corners, pt, SBLen(_Corners));
                float baseShadowDist = DistFromPoly(_BaseShadowCorners, pt, SBLen(_BaseShadowCorners));
             
                //background grad
                color = PolyFillGrad_float(mainEdgeDist,
                    i.uv, 
                    _Resolution, 
                    _BaseColor,
                    _DarkerColor,
                    black,
                    HALFPI);

                if (1) {
                    //cells
                    color2 = tex2D(_VeinsTex, (i.uv * 2) % 1);
                    color2.a = 0.3;
                    color = Blend(color, color2, SoftLight);

                    // Margin Shadow
                    color2 = PolyFill_float(mainEdgeDist, 
                        i.uv, _Resolution, 0, 40. / _Downsample, 5. / _Downsample, clear, clear, _ShadowColor);
                    color = Blend(color, color2, Multiply, 0.6);

                    // Base Shadow
                    color2 = PolyFill_float(baseShadowDist,
                        i.uv, _Resolution, 0.12, 0, 0, _ShadowColor, clear, clear);
                    color = Blend(color, color2, Multiply, 0.4);

                    // // Margin Shadow 2
                    color2 = PolyFill_float(mainEdgeDist,
                        i.uv, _Resolution, 0, 20. / _Downsample, 1.75 / _Downsample, clear, clear, _ShadowColor);
                    color = Blend(color, color2, Overlay, 0.35);
                }

                if (1) {
                    //primary veins main
                    float mainShadowDist = DistFromPoly(_VeinCorners, pt, SBLen(_VeinCorners));
                    color2 = PolyFill_float(mainShadowDist,
                        i.uv, _Resolution, 0, 0, 0, white, clear, clear);
                    color = Blend(color, color2, Overwrite);
                }

                return color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}