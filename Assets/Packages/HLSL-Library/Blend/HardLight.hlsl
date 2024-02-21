#ifndef HUBRIS_BLEND_HARDLIGHT
#define HUBRIS_BLEND_HARDLIGHT

namespace ColorLib
{
    inline half3 HardLight(half4 backdrop, half4 source)
    {
      // return Overlay(source, backdrop);
	    // half maxRGB = max(source.r, source.g);
	    // maxRGB = max(maxRGB, source.b);

	    // half blend = smoothstep(0.2, 0.8, maxRGB);

	    // half3 multiply = backdrop.rgb * (2.0 * source.rgb);
	    // half3 screen = backdrop.rgb - (1.0 - 2.0 * source.rgb) - (backdrop.rgb * (1.0 - 2.0 * source.rgb));

	    // return lerp(multiply, screen, blend);

      half maxRGB = max(source.r, source.g);
	    maxRGB = max(maxRGB, source.b);
      half blend = smoothstep(0.2, 0.8, maxRGB);
      half3 dodge = LinearDodge(backdrop, source);
      half3 burn = LinearBurn(backdrop, source);
      return lerp(dodge, burn, 1. - blend);
    }
}

#endif // HUBRIS_BLEND_HARDLIGHT

/*Uses a combination of the Linear Dodge blend mode on the lighter pixels, 
and the Linear Burn blend mode on the darker pixels. 
It uses a half-strength application of these modes
*/