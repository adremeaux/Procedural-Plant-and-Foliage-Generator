#ifndef HUBRIS_EASE_CUBICIN
#define HUBRIS_EASE_CUBICIN

namespace ColorLib
{
	inline float CubicIn(float t)
    {
	    return pow(t, 3.0);
    }
}

#endif // HUBRIS_EASE_CUBICIN