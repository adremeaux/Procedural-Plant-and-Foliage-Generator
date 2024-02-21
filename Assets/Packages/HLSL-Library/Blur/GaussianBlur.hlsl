#ifndef HUBRIS_BLUR_GAUSSIANBLUR
#define HUBRIS_BLUR_GAUSSIANBLUR
// REFERENCES
// http://callumhay.blogspot.com/2010/09/gaussian-blur-shader-glsl.html
// http://http.developer.nvidia.com/GPUGems3/gpugems3_ch40.html

// passingTurn: 0 or 1 to indicate vertical or horizontal pass

// The sigma value for the gaussian function: higher value means more blur
// A good value for 9x9 is around 3 to 5
// A good value for 7x7 is around 2.5 to 4
// A good value for 5x5 is around 2 to 3.5
// REQUIRES
#include "../Variables.hlsl"


namespace ColorLib
{
  float gaussian(float2 i, float sigma) {
    return exp(-.5 * dot(i /= sigma, i)) / (6.28 * sigma * sigma);
  }

  float gauss(float x, float y, float sigma) {
    return  1. / (2. * 6.28 * sigma * sigma) * exp(-(x * x + y * y) / (2. * sigma * sigma));
  }

	inline half4 GaussianBlur(sampler2D tex0, float2 texCoordinates, float blurAmt, int res)
  {
    float4 col = float4(0., 0., 0., 0.);
    float accum = 0.0;
    float weight;
    float2 offset;
    const float scale = 1. / (float)res;
    float span = blurAmt;
    float sigma = span * .25;
    int samples = blurAmt / 2.;
    float sampleSize = round(max(1., span / samples));
    
    for (int x = -span; x < span; x += sampleSize) {
      for (int y = -span; y < span; y += sampleSize) {
        offset = float2((int)x, (int)y);
        weight = gaussian(offset, sigma);
        col += tex2D(tex0, texCoordinates + scale * offset) * weight;
        accum += weight;
      }
    }
    
    return col / accum;

    // blurAmt = clamp(blurAmt, 5., 200.);
    /*const int LOD = 1; // gaussian done on MIPmap at scale LOD
    const int sLOD = 1 << LOD; // tile size = 2^LOD
    const float scale = 1. / (float)res;
    float sigma = blurAmt * .25;
    float4 color = float4(0., 0., 0., 0.);  
    float accumulatedAlpha = 0.0;
    int s = int(blurAmt) / sLOD;
    
    for (int i = 0; i < s * s; i++) {
      float2 coord = float2(i % s, i / s) * float(sLOD) - blurAmt / 2.;
      float4 smp = tex2D(tex0, texCoordinates + scale * coord);
      // color += gaussian(coord, sigma) * smp;
      float weight = gaussian(coord, sigma);
      // float weight = gauss(coord.x, coord.y, sigma);
      color.rgb += weight * smp.rgb;
      accumulatedAlpha += weight * smp.a;
    }

    accumulatedAlpha = clamp(accumulatedAlpha, 0., 1.);
    
    float4 c = color;//(color / color.a);
    c.rgb /= accumulatedAlpha;
    c.a = max(accumulatedAlpha, alphaBlur);
    // c.a *= intensity * 50.; //ALPHA VALUES OVER 1. CREATE ISSUES!
    // c.a = clamp(c.a, 0, 1); 
    return c;*/

/*
		half4 outputColor = 0;
		float2 blurMultiplyVec;
		if (passingTurn == 0) blurMultiplyVec = float2(1.0, 0.0);
		else blurMultiplyVec = float2(0.0, 1.0);

		// Incremental Gaussian Coefficent Calculation (See GPU Gems 3 pp. 877 - 889)
		float3 incrementalGaussian;
		incrementalGaussian.x = 1.0 / (sqrt(2.0 * HUBRIS_PI) * sigma);
		incrementalGaussian.y = exp(-0.5f / (sigma * sigma));
		incrementalGaussian.z = incrementalGaussian.y * incrementalGaussian.y;

		half4 avgValue = half4(0.0, 0.0, 0.0, 0.0);
		float coefficientSum = 0.0;

		// Take the central sample first...
		avgValue += tex2D(tex0, texCoordinates) * incrementalGaussian.x;
		coefficientSum += incrementalGaussian.x;
		incrementalGaussian.xy *= incrementalGaussian.yz;

		// Go through the remaining 8 vertical samples (4 on each side of the center)
		for (float i = 1.0; i <= numBlurPixelsPerSide; i++)
		{
			avgValue += tex2D(tex0, texCoordinates - i * blurAmnt * blurMultiplyVec) * incrementalGaussian.x;
			avgValue += tex2D(tex0, texCoordinates + i * blurAmnt * blurMultiplyVec) * incrementalGaussian.x;
			coefficientSum += 2.0 * incrementalGaussian.x;
			incrementalGaussian.xy *= incrementalGaussian.yz;
		}

		outputColor = avgValue / coefficientSum;

		return outputColor;*/
	}
}

#endif // HUBRIS_BLUR_GAUSSIANBLUR