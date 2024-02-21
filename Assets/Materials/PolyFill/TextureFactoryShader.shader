Shader "Unlit/TextureFactoryShader" {
  Properties {
    _MainTex ("MainTex", 2D) = "white" {}
    _CellsTex ("CellsTex", 2D) = "white" {} 
    _Resolution("Resolution", Float) = 512
    _BenchmarkLoops("BenchmarkLoops", Float) = 0.
    _Downsample("Downsample", Float) = 1
    _BaseColor("BaseColor", Color) = (1, 1, 1, 1)
    _DarkerColor("DarkerColor", Color) = (1, 1, 1, 1)
    _ShadowColor("ShadowColor", Color) = (1, 1, 1, 1)
    _VeinsColor("VeinsColor", Color) = (1, 1, 1, 1)
    _VeinsMidribColor("VeinsMidribColor", Color) = (1, 1, 1, 1)
    _VeinsBlur("VeinsBlur", Float) = 0
    _VeinsDepth("VeinsDepth", Float) = 0
    _MarginProminance("MarginProminance", Float) = 0
    _MarginColor("MarginColor", Color) = (1,1,1,1)
    _MarginAlpha("MarginAlpha", Float) = 1

    _RadianceColor("RadianceColor", Color) = (0,0,0,0)
    _Radiance("Radiance", Float) = 0.
    _RadianceMargin("RadianceMargin", Float) = 0.
    _RadianceInversion("RadianceInversion", Float) = 0.
    _RadianceDensity("RadianceDensity", Float) = 0.

    _DebugFloat3("DebugFloat3", Vector) = (0, 0, 0)
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
      #include "Assets/Materials/PolyFill/ShaderUtils.hlsl"
      #include "Assets/Packages/HLSL-Library/Blend.hlsl"
      #include "Assets/Packages/HLSL-Library/Blur/GaussianBlur.hlsl"
      // #include "Assets/Packages/HLSL-Library/Blur/GaussianBlur2.hlsl"

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
      StructuredBuffer<float2> _VeinMidribCorners;
      RWStructuredBuffer<float> _Output;

      sampler2D _MainTex;
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
      float _MarginAlpha;

      float4 _RadianceColor;
      float _Radiance;
      float _RadianceMargin;
      float _RadianceInversion;
      float _RadianceDensity;

      float4 _DebugFloat3;

      v2f vert (appdata v) {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
        return o;
      }

      fixed4 frag (v2f i) : SV_Target {
          int BlendOverwrite = 0;
          int BlendDarken = 1;
          int BlendMultiply = 2;
          int BlendColorBurn = 3;
          int BlendLinearBurn = 4;
          int BlendDarkerColor = 5;
          int BlendLighten = 6;
          int BlendScreen = 7;
          int BlendColorDodge = 8;
          int BlendLinearDodge= 9;
          int BlendLighterColor = 10;
          int BlendOverlay = 11;
          int BlendSoftLight = 12;
          int BlendHardLight = 13;
          int BlendVividLight = 14;
          int BlendLinearLight = 15;
          int BlendPinLight = 16;
          int BlendHardMix = 17;
          int BlendDifference = 18;
          int BlendExclusion = 19;
          int BlendSubtract = 20;
          int BlendDivide = 21;
          int BlendHue = 22;
          int BlendSaturation = 23;
          int BlendColor = 24;
          int BlendLuminosity = 25;

          int CompClear = 0;
          int CompCopy = 1;
          int CompDest = 2;
          int CompSrcOver = 3;
          int CompDestOver = 4;
          int CompSrcIn = 5;
          int CompDestIn = 6;
          int CompSrcOut = 7;
          int CompDestOut = 8;
          int CompSrcAtop = 9;
          int CompDestAtop = 10;
          int CompXOR = 11;
          int CompLighter = 12;
          
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
        float veinShadowDist = DistFromPoly(_VeinCorners, pt, SBLen(_VeinCorners));

        //background grad
        color = PolyFillGrad_float(mainEdgeDist,
          i.uv, 
          _Resolution,// white, white, white,
          _BaseColor,
          _DarkerColor,
          black,
          HALFPI);

        //shadows
        if (1) {
          //veins backdrop
          float thickShadowDist = DistFromPoly(_VeinCornersThick, pt, SBLen(_VeinCornersThick));
          color2 = PolyFill_float(thickShadowDist,
              i.uv, _Resolution, .175 / _Downsample, 0, 0, _ShadowColor, clear, clear);
          color = ColorLib::Blend(color, color2, BlendOverlay, 1.);

          // Margin Shadow
          color2 = PolyFill_float(mainEdgeDist, 
              i.uv, _Resolution, 0, 140. / _Downsample, 10. / _Downsample, clear, clear, _ShadowColor);
          color = ColorLib::Blend(color, color2, BlendMultiply, 0.55);

          // // // Base Shadow
          float baseShadowDist = DistFromPoly(_BaseShadowCorners, pt, SBLen(_BaseShadowCorners));
          color2 = PolyFill_float(baseShadowDist,
              i.uv, _Resolution, 0.12, 0, 0, _ShadowColor, clear, clear);
          color = ColorLib::Blend(color, color2, BlendMultiply, 0.3);

          // // // // Margin Shadow 2
          color2 = PolyFill_float(mainEdgeDist,
              i.uv, _Resolution, 0, 40. / _Downsample, 4 / _Downsample, clear, clear, _ShadowColor);
          color = ColorLib::Blend(color, color2, BlendOverlay, 0.4);
        }

        //radiance
        if (1) { // _RadianceDensity * 50.,
          float blurAmt = _RadianceDensity * 50.;
          //blurAmt = DecodeDebugFloat3(_DebugFloat3, 0, 1);
          // float samples =  _Radiance / 2.;
          // samples = DecodeDebugFloat3(_DebugFloat3, 0, 0);
          uint radianceBlend = _RadianceInversion > 0. ? BlendDarken : BlendOverwrite; 
          //radianceBlend = DecodeDebugFloat3(_DebugFloat3, 1, 0);
          color2 = ColorLib::GaussianBlur(_MainTex, i.uv, blurAmt, _Resolution);
          color = ColorLib::Blend(color, color2, radianceBlend, _Radiance);

          // float r = _Resolution;
          // float num = (1. - i.uv.y) * r * r + (i.uv.x) * r;
          // _Output[num] = 0.5;//blurAmt;
          _Output[0] = _RadianceInversion;

           //margin radiance
          float marginRad = 30. * (_RadianceMargin * .5 + .5);
          float4 radClear = float4(_RadianceColor.rgb, 0.);
          float radianceAlpha = 0.25 * _RadianceMargin;//DecodeDebugFloat3(_DebugFloat3, 1, 1);
          color2 = PolyFill_float(mainEdgeDist, 
              i.uv, _Resolution, 0, marginRad / _Downsample, .1 + _RadianceDensity * 3., radClear, radClear, _RadianceColor);
          color = ColorLib::Blend(color, color2, radianceBlend, radianceAlpha); //alpha: 0.75 * _RadianceMargin
        }

        //cells
        if (1) {
          color2 = tex2D(_CellsTex, (i.uv * 2) % 1);
          color2.a = .45;
          color = ColorLib::Blend(color, color2, BlendSoftLight, 3);
        }

        //margin
        if (1) {
          float4 marginClear = float4(_MarginColor.rgb, 0.);
          color2 = PolyFill_float(mainEdgeDist, 
              i.uv, _Resolution, 0., _MarginProminance * 13. / _Downsample, .1, marginClear, marginClear, _MarginColor);
          color = ColorLib::Blend(color, color2, BlendOverwrite, _MarginAlpha);
          // float radianceBlend = DecodeDebugFloat3(_DebugFloat3, 1, 0);
          // float radianceAlpha = DecodeDebugFloat3(_DebugFloat3, 1, 1);
          // color = ColorLib::Blend(color, color2, radianceBlend, radianceAlpha); //why doesn't HardLight work?
        }
        
        //veins
        if (1) {
          //primary veins main
          float4 vc = _VeinsColor;
          vc.a = 1.;
          color2 = PolyFill_float(veinShadowDist,
              i.uv, _Resolution, _VeinsBlur / _Downsample / 40., 0, 0, vc, clear, clear);
          color = ColorLib::Blend(color, color2, BlendOverwrite, _VeinsColor.a);

          //primary veins midrib
          float veinShadowDist = DistFromPoly(_VeinMidribCorners, pt, SBLen(_VeinMidribCorners));
          vc = _VeinsMidribColor;
          vc.a = 1.;
          color2 = PolyFill_float(veinShadowDist,
              i.uv, _Resolution, _VeinsBlur / _Downsample / 40., 0, 0, vc, clear, clear);
          color = ColorLib::Blend(color, color2, BlendOverwrite, _VeinsMidribColor.a);
        }

        //mask
        if (1) {
          color2 = PolyFill_float(mainEdgeDist, 
              i.uv, _Resolution, 0, 0, 0., clear, black, clear);
          color = ColorLib::Blend(color, color2, BlendOverwrite);
        }

        return color;

        /*float precalcDist,
          float2 fragCoord,
          float resolution,
          float blur, 
          float borderSize, 
          float borderBlur, 
          float4 innerColor, 
          float4 outerColor, 
          float4 borderColor)*/
      }
      ENDCG
    }
  }
  FallBack "Diffuse"
}
