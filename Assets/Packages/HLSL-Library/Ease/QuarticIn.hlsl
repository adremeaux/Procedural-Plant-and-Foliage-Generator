﻿#ifndef HUBRIS_EASE_QUARTICIN
#define HUBRIS_EASE_QUARTICIN

namespace ColorLib
{
	inline float QuarticIn(float t)
    {
	    return pow(t, 4.0);
    }
}

#endif // HUBRIS_EASE_QUARTICIN