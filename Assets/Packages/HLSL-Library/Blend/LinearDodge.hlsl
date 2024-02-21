#ifndef HUBRIS_BLEND_LINEARDODGE
#define HUBRIS_BLEND_LINEARDODGE

namespace ColorLib
{
    inline half3 LinearDodge(half4 backdrop, half4 source)
    {
	    return backdrop.rgb + source.rgb;
    }
}

#endif // HUBRIS_BLEND_LINEARDODGE