#ifndef HUBRIS_EASE_SINEIN
#define HUBRIS_EASE_SINEIN
// REQUIRES
#include "../Variables.hlsl"

namespace ColorLib
{
	inline float SineIn(float t)
    {
	    return sin((t - 1.0) * HUBRIS_HALF_PI) + 1.0;
    }
}

#endif // HUBRIS_EASE_SINEIN