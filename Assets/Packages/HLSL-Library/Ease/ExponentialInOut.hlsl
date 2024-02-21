﻿#ifndef HUBRIS_EASE_EXPONENTIALINOUT
#define HUBRIS_EASE_EXPONENTIALINOUT

namespace ColorLib
{
	inline float ExponentialInOut(float t)
    {
	    return t == 0.0 || t == 1.0
		    ? t
		    : t < 0.5
		    ? 0.5 * pow(2.0, (20.0 * t) - 10.0)
		    : -0.5 * pow(2.0, 10.0 - (t * 20.0)) + 1.0;
    }
}

#endif // HUBRIS_EASE_EXPONENTIALINOUT