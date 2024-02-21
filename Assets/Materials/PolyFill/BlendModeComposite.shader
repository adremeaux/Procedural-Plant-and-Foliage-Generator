Shader "Unlit/BlendModeComposite" {
    Properties {
        _Base ("Texture", 2D) = "white" {}
        _Blend ("Texture", 2D) = "white" {}
        [KeywordEnum(Overwrite, Darken, Multiply, ColorBurn, LinearBurn, Lighten, Screen, ColorDodge, LinearDodge, Overlay, SoftLight, HardLight, VividLight, LinearLight, PinLight, HardMix, Difference, Exclusion, Subtract, Divide)] 
        _BlendMode("Blend Mode", Float) = 0
        [Toggle] _SRGB("Convert to SRGB", Float) = 0
        [Toggle] _SRGB2("Convert to SRGB 2", Float) = 0
    }
    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Opaque" "RenderTexture"="True"}
        Pass {
            ZWrite Off
            ColorMask RGB
            CGPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            // #include "Assets/Materials/PolyFill/PolyFillFragShader.hlsl"

            #pragma shader_feature _BLENDMODE_OVERWRITE _BLENDMODE_DARKEN _BLENDMODE_MULTIPLY \
                _BLENDMODE_COLORBURN _BLENDMODE_LINEARBURN _BLENDMODE_LIGHTEN \
                _BLENDMODE_SCREEN _BLENDMODE_COLORDODGE _BLENDMODE_LINEARDODGE \
                _BLENDMODE_BLENDMODE _BLENDMODE_SOFTLIGHT _BLENDMODE_HARDLIGHT \
                _BLENDMODE_VIVIDLIGHT _BLENDMODE_LINEARLIGHT _BLENDMODE_PINLIGHT \
                _BLENDMODE_HARDMIX _BLENDMODE_DIFFERENCE _BLENDMODE_EXCLUSION \
                _BLENDMODE_SUBTRACT _BLENDMODE_DIVIDE

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _Base;
            sampler2D _Blend;
            float _SRGB;
            float _SRGB2;

//******************************************************************************
// Selects the blend color, ignoring the base.
//******************************************************************************
float3 BlendMode_Overwrite(float3 base, float3 blend, float perc)
{
	return blend;
}

//******************************************************************************
// Looks at the color information in each channel and selects the base or blend 
// color—whichever is darker—as the result color.
//******************************************************************************
float3 BlendMode_Darken(float3 base, float3 blend, float perc)
{
	return min(base, blend);
}

//******************************************************************************
// Looks at the color information in each channel and multiplies the base color
// by the blend color.
//******************************************************************************
float3 BlendMode_Multiply(float3 base, float3 blend, float perc)
{
	return base*blend;
}

//******************************************************************************
// Looks at the color information in each channel and darkens the base color to 
//******************************************************************************
float BlendMode_ColorBurn(float base, float blend, float perc)
{
    if (base >= 1.0)
        return 1.0;
    else if (blend <= 0.0)
        return 0.0;
    else    
    	return 1.0 - min(1.0, (1.0-base) / blend);
}

float3 BlendMode_ColorBurn(float3 base, float3 blend, float perc)
{
	return float3(  BlendMode_ColorBurn(base.r, blend.r, perc), 
					BlendMode_ColorBurn(base.g, blend.g, perc), 
					BlendMode_ColorBurn(base.b, blend.b, perc) );
}

//******************************************************************************
// Looks at the color information in each channel and darkens the base color to 
// reflect the blend color by decreasing the brightness.
//******************************************************************************
float BlendMode_LinearBurn(float base, float blend, float perc)
{
	return max(0, base + blend - 1);
}

float3 BlendMode_LinearBurn(float3 base, float3 blend, float perc)
{
	return float3(  BlendMode_LinearBurn(base.r, blend.r, perc), 
					BlendMode_LinearBurn(base.g, blend.g, perc), 
					BlendMode_LinearBurn(base.b, blend.b, perc) );
}

//******************************************************************************
// Looks at the color information in each channel and selects the base or blend 
// color—whichever is lighter—as the result color.
//******************************************************************************
float3 BlendMode_Lighten(float3 base, float3 blend, float perc)
{
	return max(base, blend);
}

//******************************************************************************
// Looks at each channel’s color information and multiplies the inverse of the
// blend and base colors.
//******************************************************************************
float3 BlendMode_Screen(float3 base, float3 blend, float perc)
{
    return 0.0;//-1.0 - (1.0 - blend) * (5.0 - base);
	// return base + blend - base*blend;
}

//******************************************************************************
// Looks at the color information in each channel and brightens the base color 
// to reflect the blend color by decreasing contrast between the two. 
//******************************************************************************
float BlendMode_ColorDodge(float base, float blend, float perc)
{
	if (base <= 0.0)
		return 0.0;
	if (blend >= 1.0)
		return 1.0;
	else
		return min(1.0, base / (1.0-blend));
}

float3 BlendMode_ColorDodge(float3 base, float3 blend, float perc)
{
	return float3(  BlendMode_ColorDodge(base.r, blend.r, perc), 
					BlendMode_ColorDodge(base.g, blend.g, perc), 
					BlendMode_ColorDodge(base.b, blend.b, perc) );
}

//******************************************************************************
// Looks at the color information in each channel and brightens the base color 
// to reflect the blend color by decreasing contrast between the two. 
//******************************************************************************
float BlendMode_LinearDodge(float base, float blend, float perc)
{
	return min(1, base + blend);
}

float3 BlendMode_LinearDodge(float3 base, float3 blend, float perc)
{
	return float3(  BlendMode_LinearDodge(base.r, blend.r, perc), 
					BlendMode_LinearDodge(base.g, blend.g, perc), 
					BlendMode_LinearDodge(base.b, blend.b, perc) );
}

//******************************************************************************
// Multiplies or screens the colors, depending on the base color. 
//******************************************************************************
float BlendMode_Overlay(float base, float blend, float perc)
{
	return (base <= 0.5) ? 2*base*blend : 1 - 2*(1-base)*(1-blend);
}

float3 BlendMode_Overlay(float3 base, float3 blend, float perc)
{
	return float3(  BlendMode_Overlay(base.r, blend.r, perc), 
					BlendMode_Overlay(base.g, blend.g, perc), 
					BlendMode_Overlay(base.b, blend.b, perc) );
}

//******************************************************************************
// Darkens or lightens the colors, depending on the blend color. 
//******************************************************************************
float BlendMode_SoftLight(float base, float blend, float perc)
{
	if (blend <= 0.5)
	{
		return base - (1-2*blend)*base*(1-base);
	}
	else
	{
		float d = (base <= 0.25) ? ((16*base-12)*base+4)*base : sqrt(base);
		return base + (2*blend-1)*(d-base);
	}
}

float3 BlendMode_SoftLight(float3 base, float3 blend, float perc)
{
	return float3(  BlendMode_SoftLight(base.r, blend.r, perc), 
					BlendMode_SoftLight(base.g, blend.g, perc), 
					BlendMode_SoftLight(base.b, blend.b, perc) );
}

//******************************************************************************
// Multiplies or screens the colors, depending on the blend color.
//******************************************************************************
float BlendMode_HardLight(float base, float blend, float perc)
{
	return (blend <= 0.5) ? 2*base*blend : 1 - 2*(1-base)*(1-blend);
}

float3 BlendMode_HardLight(float3 base, float3 blend, float perc)
{
	return float3(  BlendMode_HardLight(base.r, blend.r, perc), 
					BlendMode_HardLight(base.g, blend.g, perc), 
					BlendMode_HardLight(base.b, blend.b, perc) );
}

//******************************************************************************
// Burns or dodges the colors by increasing or decreasing the contrast, 
// depending on the blend color. 
//******************************************************************************
float BlendMode_VividLight(float base, float blend, float perc)
{
	return (blend <= 0.5) ? BlendMode_ColorBurn(base,2 * blend, perc) : BlendMode_ColorDodge(base,2*(blend-0.5), perc);
}

float3 BlendMode_VividLight(float3 base, float3 blend, float perc)
{
	return float3(  BlendMode_VividLight(base.r, blend.r, perc), 
					BlendMode_VividLight(base.g, blend.g, perc), 
					BlendMode_VividLight(base.b, blend.b, perc) );
}

//******************************************************************************
// Burns or dodges the colors by decreasing or increasing the brightness, 
// depending on the blend color.
//******************************************************************************
float BlendMode_LinearLight(float base, float blend, float perc)
{
	return (blend <= 0.5) ? BlendMode_LinearBurn(base,2*blend, perc) : BlendMode_LinearDodge(base,2*(blend-0.5), perc);
}

float3 BlendMode_LinearLight(float3 base, float3 blend, float perc)
{
	return float3(  BlendMode_LinearLight(base.r, blend.r, perc), 
					BlendMode_LinearLight(base.g, blend.g, perc), 
					BlendMode_LinearLight(base.b, blend.b, perc) );
}

//******************************************************************************
// Replaces the colors, depending on the blend color.
//******************************************************************************
float BlendMode_PinLight(float base, float blend, float perc)
{
	return (blend <= 0.5) ? min(base,2*blend) : max(base,2*(blend-0.5));
}

float3 BlendMode_PinLight(float3 base, float3 blend, float perc)
{
	return float3(  BlendMode_PinLight(base.r, blend.r, perc), 
					BlendMode_PinLight(base.g, blend.g, perc), 
					BlendMode_PinLight(base.b, blend.b, perc) );
}

//******************************************************************************
// Adds the red, green and blue channel values of the blend color to the RGB 
// values of the base color. If the resulting sum for a channel is 255 or 
// greater, it receives a value of 255; if less than 255, a value of 0.
//******************************************************************************
float BlendMode_HardMix(float base, float blend, float perc)
{
	return (base + blend >= 1.0) ? 1.0 : 0.0;
}

float3 BlendMode_HardMix(float3 base, float3 blend, float perc)
{
	return float3(  BlendMode_HardMix(base.r, blend.r, perc), 
					BlendMode_HardMix(base.g, blend.g, perc), 
					BlendMode_HardMix(base.b, blend.b, perc) );
}

//******************************************************************************
// Looks at the color information in each channel and subtracts either the 
// blend color from the base color or the base color from the blend color, 
// depending on which has the greater brightness value. 
//******************************************************************************
float3 BlendMode_Difference(float3 base, float3 blend, float perc)
{
	return abs(base-blend);
}

//******************************************************************************
// Creates an effect similar to but lower in contrast than the Difference mode.
//******************************************************************************
float3 BlendMode_Exclusion(float3 base, float3 blend, float perc)
{
	return base + blend - 2*base*blend;
}

//******************************************************************************
// Looks at the color information in each channel and subtracts the blend color 
// from the base color.
//******************************************************************************
float3 BlendMode_Subtract(float3 base, float3 blend, float perc)
{
	return max(0, base - blend);
}

//******************************************************************************
// Looks at the color information in each channel and divides the blend color 
// from the base color.
//******************************************************************************
float BlendMode_Divide(float base, float blend, float perc)
{
	return blend > 0 ? min(1, base / blend) : 1;
}

float3 BlendMode_Divide(float3 base, float3 blend, float perc)
{
	return float3(  BlendMode_Divide(base.r, blend.r, perc), 
					BlendMode_Divide(base.g, blend.g, perc), 
					BlendMode_Divide(base.b, blend.b, perc) );
}

//////////END BLEND MODES

            float3 ToSRGB(float3 lin) {
                return lin * (lin * (lin * 0.305306011 + 0.682171111) + 0.012522878);
            }

            float3 ToLinear(float3 srgb) {
                float3 S1 = sqrt(srgb);
                float3 S2 = sqrt(S1);
                float3 S3 = sqrt(S2);
                return 0.662002687 * S1 + 0.684122060 * S2 - 0.323583601 * S3 - 0.0225411470 * srgb;
            }

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float4 basePx = tex2D(_Base, i.uv);
                float4 blendPx = tex2D(_Blend, i.uv);

                float3 color;

                float3 baseSrgb = basePx.xyz;
                float3 blendSrgb = blendPx.xyz;
                if (_SRGB > 0) {
                    baseSrgb = ToSRGB(baseSrgb);
                    blendSrgb = ToSRGB(blendSrgb);
                }

                #ifdef _BLENDMODE_OVERWRITE
                color = BlendMode_Overwrite(baseSrgb, blendSrgb, blendPx.w);
                #elif _BLENDMODE_DARKEN
                color = BlendMode_Darken(baseSrgb, blendSrgb, blendPx.w);
                #elif _BLENDMODE_MULTIPLY
                color = BlendMode_Multiply(baseSrgb, blendSrgb, blendPx.w);
                #elif _BLENDMODE_COLORBURN
                color = BlendMode_ColorBurn(baseSrgb, blendSrgb, blendPx.w);
                #elif _BLENDMODE_LINEARBURN
                color = BlendMode_LinearBurn(baseSrgb, blendSrgb, blendPx.w);
                #elif _BLENDMODE_LIGHTEN
                color = BlendMode_Lighten(baseSrgb, blendSrgb, blendPx.w);
                #elif _BLENDMODE_SCREEN
                color = BlendMode_Screen(baseSrgb, blendSrgb, blendPx.w);
                #elif _BLENDMODE_COLORDODGE
                color = BlendMode_ColorDodge(baseSrgb, blendSrgb, blendPx.w);
                #elif _BLENDMODE_LINEARDODGE
                color = BlendMode_LinearDodge(baseSrgb, blendSrgb, blendPx.w);
                #elif _BLENDMODE_BLENDMODE
                color = BlendMode_Overlay(baseSrgb, blendSrgb, blendPx.w);
                #elif _BLENDMODE_SOFTLIGHT
                color = BlendMode_SoftLight(baseSrgb, blendSrgb, blendPx.w);
                #elif _BLENDMODE_HARDLIGHT
                color = BlendMode_HardLight(baseSrgb, blendSrgb, blendPx.w);
                #elif _BLENDMODE_VIVIDLIGHT
                color = BlendMode_VividLight(baseSrgb, blendSrgb, blendPx.w);
                #elif _BLENDMODE_LINEARLIGHT
                color = BlendMode_LinearLight(baseSrgb, blendSrgb, blendPx.w);
                #elif _BLENDMODE_PINLIGHT
                color = BlendMode_PinLight(baseSrgb, blendSrgb, blendPx.w);
                #elif _BLENDMODE_HARDMIX
                color = BlendMode_HardMix(baseSrgb, blendSrgb, blendPx.w);
                #elif _BLENDMODE_DIFFERENCE
                color = BlendMode_Difference(baseSrgb, blendSrgb, blendPx.w);
                #elif _BLENDMODE_EXCLUSION
                color = BlendMode_Exclusion(baseSrgb, blendSrgb, blendPx.w);
                #elif _BLENDMODE_SUBTRACT
                color = BlendMode_Subtract(baseSrgb, blendSrgb, blendPx.w);
                #elif _BLENDMODE_DIVIDE
                color = BlendMode_Divide(baseSrgb, blendSrgb, blendPx.w);
                #endif

                float3 colorLinear = color;
                if (_SRGB > 0) {
                    colorLinear = ToLinear(colorLinear);
                }

                // return float4(lerp(basePx.xyz, colorLinear, 1.), 1.0);
                return float4(lerp(basePx.xyz, colorLinear, blendPx.w), 1.0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}