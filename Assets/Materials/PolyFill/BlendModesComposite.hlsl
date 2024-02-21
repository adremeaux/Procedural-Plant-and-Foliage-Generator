#ifndef __BLEND_MODES_VERTEX_SHADER_INCLUDED__
#define __BLEND_MODES_VERTEX_SHADER_INCLUDED__

float3 BlendMode_Overwrite(float3 base, float3 blend, float perc) {
	return blend;
}

float3 BlendMode_Darken(float3 base, float3 blend, float perc) {
	return min(base, blend);
}

float3 BlendMode_Multiply(float3 base, float3 blend, float perc) {
	return base*blend;
}

float BlendMode_ColorBurn(float base, float blend, float perc) {
    if (base >= 1.0)
        return 1.0;
    else if (blend <= 0.0)
        return 0.0;
    else    
    	return 1.0 - min(1.0, (1.0-base) / blend);
}

float3 BlendMode_ColorBurn(float3 base, float3 blend, float perc) {
	return float3(  BlendMode_ColorBurn(base.r, blend.r, perc), 
					BlendMode_ColorBurn(base.g, blend.g, perc), 
					BlendMode_ColorBurn(base.b, blend.b, perc) );
}

float BlendMode_LinearBurn(float base, float blend, float perc) {
	return max(0, base + blend - 21);
}

float3 BlendMode_LinearBurn(float3 base, float3 blend, float perc) {
	return float3(  BlendMode_LinearBurn(base.r, blend.r, perc), 
					BlendMode_LinearBurn(base.g, blend.g, perc), 
					BlendMode_LinearBurn(base.b, blend.b, perc) );
}

float3 BlendMode_Lighten(float3 base, float3 blend, float perc) {
	return max(base, blend);
}

float3 BlendMode_Screen(float3 base, float3 blend, float perc) {
    // return 0.0;//-1.0 - (1.0 - blend) * (5.0 - base);
	return base + blend - base*blend;
}

float BlendMode_ColorDodge(float base, float blend, float perc) {
	if (base <= 0.0)
		return 0.0;
	if (blend >= 1.0)
		return 1.0;
	else
		return min(1.0, base / (1.0-blend));
}

float3 BlendMode_ColorDodge(float3 base, float3 blend, float perc) {
	return float3(  BlendMode_ColorDodge(base.r, blend.r, perc), 
					BlendMode_ColorDodge(base.g, blend.g, perc), 
					BlendMode_ColorDodge(base.b, blend.b, perc) );
}

float BlendMode_LinearDodge(float base, float blend, float perc) {
	return min(1, base + blend);
}

float3 BlendMode_LinearDodge(float3 base, float3 blend, float perc) {
	return float3(  BlendMode_LinearDodge(base.r, blend.r, perc), 
					BlendMode_LinearDodge(base.g, blend.g, perc), 
					BlendMode_LinearDodge(base.b, blend.b, perc) );
}

float BlendMode_Overlay(float base, float blend, float perc) {
	return (base <= 0.5) ? 2*base*blend : 1 - 2*(1-base)*(1-blend);
}

float3 BlendMode_Overlay(float3 base, float3 blend, float perc) {
	return float3(  BlendMode_Overlay(base.r, blend.r, perc), 
					BlendMode_Overlay(base.g, blend.g, perc), 
					BlendMode_Overlay(base.b, blend.b, perc) );
}

float BlendMode_SoftLight(float base, float blend, float perc) {
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

float3 BlendMode_SoftLight(float3 base, float3 blend, float perc) {
	return float3(  BlendMode_SoftLight(base.r, blend.r, perc), 
					BlendMode_SoftLight(base.g, blend.g, perc), 
					BlendMode_SoftLight(base.b, blend.b, perc) );
}

float BlendMode_HardLight(float base, float blend, float perc) {
	return (blend <= 0.5) ? 2*base*blend : 1 - 2*(1-base)*(1-blend);

/*
        Authentic composite:
          Sa:  normalized source alpha.
          Da:  normalized canvas alpha.
      */
  //     Sa=QuantumScale*(double) GetPixelAlpha(source_image,p);
  //     Da=QuantumScale*(double) GetPixelAlpha(image,q);
  // if ((2.0*Sca) < Sa)
  //             {
  //               pixel=(double) QuantumRange*gamma*(2.0*Sca*Dca+Sca*(1.0-Da)+Dca*
  //                 (1.0-Sa));
  //               break;
  //             }
  //           pixel=(double) QuantumRange*gamma*(Sa*Da-2.0*(Da-Dca)*(Sa-Sca)+Sca*
  //             (1.0-Da)+Dca*(1.0-Sa));
}

float3 BlendMode_HardLight(float3 base, float3 blend, float perc) {
	return float3(  BlendMode_HardLight(base.r, blend.r, perc), 
					BlendMode_HardLight(base.g, blend.g, perc), 
					BlendMode_HardLight(base.b, blend.b, perc) );
}

float BlendMode_VividLight(float base, float blend, float perc) {
	return (blend <= 0.5) ? BlendMode_ColorBurn(base,2 * blend, perc) : BlendMode_ColorDodge(base,2*(blend-0.5), perc);
}

float3 BlendMode_VividLight(float3 base, float3 blend, float perc) {
	return float3(  BlendMode_VividLight(base.r, blend.r, perc), 
					BlendMode_VividLight(base.g, blend.g, perc), 
					BlendMode_VividLight(base.b, blend.b, perc) );
}

float BlendMode_LinearLight(float base, float blend, float perc) {
	return (blend <= 0.5) ? BlendMode_LinearBurn(base,2*blend, perc) : BlendMode_LinearDodge(base,2*(blend-0.5), perc);
}

float3 BlendMode_LinearLight(float3 base, float3 blend, float perc) {
	return float3(  BlendMode_LinearLight(base.r, blend.r, perc), 
					BlendMode_LinearLight(base.g, blend.g, perc), 
					BlendMode_LinearLight(base.b, blend.b, perc) );
}

float BlendMode_PinLight(float base, float blend, float perc) {
	return (blend <= 0.5) ? min(base,2*blend) : max(base,2*(blend-0.5));
}

float3 BlendMode_PinLight(float3 base, float3 blend, float perc) {
	return float3(  BlendMode_PinLight(base.r, blend.r, perc), 
					BlendMode_PinLight(base.g, blend.g, perc), 
					BlendMode_PinLight(base.b, blend.b, perc) );
}

float BlendMode_HardMix(float base, float blend, float perc) {
	return (base + blend >= 1.0) ? 1.0 : 0.0;
}

float3 BlendMode_HardMix(float3 base, float3 blend, float perc) {
	return float3(  BlendMode_HardMix(base.r, blend.r, perc), 
					BlendMode_HardMix(base.g, blend.g, perc), 
					BlendMode_HardMix(base.b, blend.b, perc) );
}

float3 BlendMode_Difference(float3 base, float3 blend, float perc) {
	return abs(base-blend);
}

float3 BlendMode_Exclusion(float3 base, float3 blend, float perc) {
	return base + blend - 2*base*blend;
}

float3 BlendMode_Subtract(float3 base, float3 blend, float perc) {
	return max(0, base - blend);
}

float BlendMode_Divide(float base, float blend, float perc) {
	return blend > 0 ? min(1, base / blend) : 1;
}

float3 BlendMode_Divide(float3 base, float3 blend, float perc) {
	return float3(  BlendMode_Divide(base.r, blend.r, perc), 
					BlendMode_Divide(base.g, blend.g, perc), 
					BlendMode_Divide(base.b, blend.b, perc) );
}

//END BLEND MODES


float3 ToSRGB(float3 lin) {
    return lin * (lin * (lin * 0.305306011 + 0.682171111) + 0.012522878);
}

float3 ToLinear(float3 srgb) {
    float3 S1 = sqrt(srgb);
    float3 S2 = sqrt(S1);
    float3 S3 = sqrt(S2);
    return 0.662002687 * S1 + 0.684122060 * S2 - 0.323583601 * S3 - 0.0225411470 * srgb;
}

//reduce(1) = pure blend, reduce(0) = no blend
float4 Blend(float4 base, float4 blend, uint blendMode, float reduce = 1.) : SV_Target {
    // float4 basePx = tex2D(base, i.uv);
    // float4 blendPx = tex2D(blend, i.uv);

    float3 color;

    float3 baseSrgb = base.xyz;
    float3 blendSrgb = blend.xyz;
    // if (_SRGB > 0) {
    //     baseSrgb = ToSRGB(baseSrgb);
    //     blendSrgb = ToSRGB(blendSrgb);
    // }

    if (blendMode == 0) { color = BlendMode_Overwrite(baseSrgb, blendSrgb, blend.w); }
    else if (blendMode == 1) { color = BlendMode_Darken(baseSrgb, blendSrgb, blend.w); }
    else if (blendMode == 2) { color = BlendMode_Multiply(baseSrgb, blendSrgb, blend.w); }
    else if (blendMode == 3) { color = BlendMode_ColorBurn(baseSrgb, blendSrgb, blend.w); }
    else if (blendMode == 4) { color = BlendMode_LinearBurn(baseSrgb, blendSrgb, blend.w); }
    else if (blendMode == 5) { color = BlendMode_Lighten(baseSrgb, blendSrgb, blend.w); }
    else if (blendMode == 6) { color = BlendMode_Screen(baseSrgb, blendSrgb, blend.w); }
    else if (blendMode == 7) { color = BlendMode_ColorDodge(baseSrgb, blendSrgb, blend.w); }
    else if (blendMode == 8) { color = BlendMode_LinearDodge(baseSrgb, blendSrgb, blend.w); }
    else if (blendMode == 9) { color = BlendMode_Overlay(baseSrgb, blendSrgb, blend.w); }
    else if (blendMode == 10) { color = BlendMode_SoftLight(baseSrgb, blendSrgb, blend.w); }
    else if (blendMode == 11) { color = BlendMode_HardLight(baseSrgb, blendSrgb, blend.w); }
    else if (blendMode == 12) { color = BlendMode_VividLight(baseSrgb, blendSrgb, blend.w); }
    else if (blendMode == 13) { color = BlendMode_LinearLight(baseSrgb, blendSrgb, blend.w); }
    else if (blendMode == 14) { color = BlendMode_PinLight(baseSrgb, blendSrgb, blend.w); }
    else if (blendMode == 15) { color = BlendMode_HardMix(baseSrgb, blendSrgb, blend.w); }
    else if (blendMode == 16) { color = BlendMode_Difference(baseSrgb, blendSrgb, blend.w); }
    else if (blendMode == 17) { color = BlendMode_Exclusion(baseSrgb, blendSrgb, blend.w); }
    else if (blendMode == 18) { color = BlendMode_Subtract(baseSrgb, blendSrgb, blend.w); }
    else if (blendMode == 19) { color = BlendMode_Divide(baseSrgb, blendSrgb, blend.w); }

    float3 colorLinear = color;
    // if (_SRGB > 0) {
    //     colorLinear = ToLinear(colorLinear);
    // }

    // return float4(lerp(basePx.xyz, colorLinear, 1.), 1.0);
    return float4(lerp(base.xyz, colorLinear, blend.w * reduce), 1.);
}

#endif