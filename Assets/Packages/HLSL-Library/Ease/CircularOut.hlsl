﻿#ifndef HUBRIS_EASE_CIRCULAROUT
#define HUBRIS_EASE_CIRCULAROUT

namespace ColorLib
{
	inline float CircularOut(float t)
    {
	    return sqrt((2.0 - t) * t);
    }
}

#endif // HUBRIS_EASE_CIRCULAROUT