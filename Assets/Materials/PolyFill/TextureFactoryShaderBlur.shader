Shader "Unlit/TextureFactoryShaderBlur" {
  Properties {
    _CellsTex ("CellsTex", 2D) = "white" {} 
    _Resolution("Resolution", Float) = 512
    _BenchmarkLoops("BenchmarkLoops", Float) = 0.
    _Downsample("Downsample", Float) = 1
    _BaseColor("BaseColor", Color) = (1, 1, 1, 1)
    _DarkerColor("DarkerColor", Color) = (1, 1, 1, 1)
    _ShadowColor("ShadowColor", Color) = (1, 1, 1, 1)
    _ShadowColor("VeinsColor", Color) = (1, 1, 1, 1)
    _ShadowColor("VeinsMidribColor", Color) = (1, 1, 1, 1)
    _VeinsBlur("VeinsBlur", Float) = 0
    _VeinsDepth("VeinsDepth", Float) = 0
    _MarginProminance("MarginProminance", Float) = 0
    _MarginColor("MarginColor", Color) = (1,1,1,1)

    _RadianceColor("RadianceColor", Color) = (0,0,0,0)
    _Radiance("Radiance", Float) = 0.
    _RadianceMargin("RadianceMargin", Float) = 0.
    _RadianceInversion("RadianceInversion", Float) = 0.
    _RadianceDensity("RadianceDensity", Float) = 0.

    _DebugFloat("DebugFloat", Float) = 0

  }
  SubShader {
    Tags {"Queue"="Transparent" "RenderType"="Transparent" "RenderTexture"="True"}
    Pass {
      // Blend One One
      ZWrite Off
      ColorMask RGBA

      CGPROGRAM
      #pragma target 4.5
      #pragma vertex vert
      #pragma fragment frag
      #include "UnityCG.cginc"
      #include "Assets/Materials/PolyFill/PolyFillFragShader.hlsl"
      #include "Assets/Packages/HLSL-Library/Blend.hlsl"

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
      StructuredBuffer<float2> _VeinCornersThick;
      RWStructuredBuffer<float> _Output;

      sampler2D _CellsTex;
      float _Resolution;
      float _BenchmarkLoops;
      float _Downsample;
      
      float4 _BaseColor;
      float4 _DarkerColor;
      float4 _ShadowColor;

      float4 _VeinsColor;
      float4 _VeinsMidribColor; //not implemented
      float _VeinsBlur;
      float _VeinsDepth;

      float _MarginProminance;
      float4 _MarginColor;

      float4 _RadianceColor;
      float _Radiance;
      float _RadianceMargin;
      float _RadianceInversion;
      float _RadianceDensity;

      float _DebugFloat;

      v2f vert (appdata v) {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
        return o;
      }

      fixed4 frag (v2f i) : SV_Target {
        float4 black = float4(0., 0., 0., 1.0);
        float4 clear = float4(0., 0., 0., 0.0);
        float4 white = float4(1., 1., 1., 1.0);
        float4 red = float4(1., 0., 0, 1.0);
        float4 redClear = float4(1., 0., 0, 0.0);
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

        // return red;

        float2 pt = (2. * i.uv - 1.);
        float veinShadowDist = DistFromPoly(_VeinCorners, pt, SBLen(_VeinCorners));

        float marginRad = 30. * (_RadianceMargin * .5 + .5);
        float4 radClear = float4(_RadianceColor.rgb, 0.);
        color = PolyFill_float(veinShadowDist, 
            i.uv, _Resolution, 0, 0, 0, _RadianceColor, radClear, clear);
        return color;
      }
      ENDCG
    }
  }
  FallBack "Diffuse"
}
