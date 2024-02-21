#ifndef HUBRIS_EASE_CIRCULARIN
#define HUBRIS_EASE_CIRCULARIN

namespace ColorLib
{
	inline float CircularIn(float t)
    {
	    return 1.0 - sqrt(1.0 - t * t);
    }
}

#endif // HUBRIS_EASE_CIRCULARIN