#ifndef HUBRIS_EASE_SINEOUT
#define HUBRIS_EASE_SINEOUT
// REQUIRES
#include "../Variables.hlsl"

namespace ColorLib
{
	inline float SineOut(float t)
    {
	    return sin(t * HUBRIS_HALF_PI);
    }
}

#endif // HUBRIS_EASE_SINEOUT