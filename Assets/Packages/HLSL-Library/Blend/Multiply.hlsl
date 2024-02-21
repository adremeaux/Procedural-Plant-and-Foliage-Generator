#ifndef HUBRIS_BLEND_MULTIPLY
#define HUBRIS_BLEND_MULTIPLY

namespace ColorLib
{
    inline half3 Multiply(half4 backdrop, half4 source)
    {
	    return backdrop.rgb * source.rgb;
    }
}

#endif // HUBRIS_BLEND_MULTIPLY