#ifndef __POLY_FILL_VERTEX_SHADER_INCLUDED__
#define __POLY_FILL_VERTEX_SHADER_INCLUDED__

#define N 256
#define PI 3.1415
#define HALFPI 1.5708
#define PI2 6.283

float cross2d(float2 v0, float2 v1) {
    return v0.x * v1.y - v0.y * v1.x;
}

float dot2(float2 v0, float2 v1) {
    return v0.x * v1.x + v0.y * v1.y;
}

float2 recenter(float2 xy) {
    return float2((xy.x - .5) / .5, (xy.y - .5) / .5);
}

uint SBLen(StructuredBuffer<float2> sb) {
    uint numStructs, stride;
    sb.GetDimensions(numStructs, stride);
    return numStructs;
}

float DistFromPoly(StructuredBuffer<float2> corners, float2 p, int cornerCount) {
    float2 edgeDist[N];
    float2 pointDist[N];
    float2 pq[N];
    
    int lastMark = 0;

    // data
    for (int i = 0; i < cornerCount; i++) {
        int i2 = i + 1;
        if (i2 == cornerCount) i2 = lastMark;

        edgeDist[i] = corners[i2] - corners[i];
        pointDist[i] = p - corners[i];
        float dotei = dot(edgeDist[i], edgeDist[i]);
        if (dotei == 0) dotei = .01;
        pq[i] = pointDist[i] - (edgeDist[i] * clamp(dot(pointDist[i], edgeDist[i]) / dotei, 0, 1 ));
    }

    
    //distance (loop: 100ms) OPTIMIZE: make optional if no blur or border
    float d = dot(pq[0], pq[0]); 
    for (i = 1; i < cornerCount; i++) {
        d = min(d, dot(pq[i], pq[i]));
    }

    // //winding number
    // // from http://geomalgorithms.com/a03-_inclusion.html
    int wn = 0; 
    lastMark = 0;
    bool runningVal = true;
    for (i = 0; i < cornerCount; i++) {
        int i2 = i + 1;
        if (i2 == cornerCount) i2 = lastMark;
        
        bool cond1 = 0. <= pointDist[i].y;
        bool cond2 = 0. > pointDist[i2].y;
        float val3 = cross2d(edgeDist[i], pointDist[i]); //isLeft
        wn += cond1 && cond2 && val3 > 0. ? 1 : 0; // have  a valid up intersect
        wn -= !cond1 && !cond2 && val3 < 0. ? 1 : 0; // have  a valid down intersect
    }
    runningVal = runningVal && (wn == 0);
    float s = runningVal ? 1. : -1.;
    
    return -sqrt(d) * s;
}

float4 PolyFill_float(float precalcDist,
                    float2 fragCoord,
                    float resolution,
                    float blur, 
                    float borderSize, 
                    float borderBlur, 
                    float4 innerColor, 
                    float4 outerColor, 
                    float4 borderColor)
{
    float rawDist = abs(precalcDist);
    float pos = sign(precalcDist);
    if (blur == 0) blur = 0.001;
    precalcDist /= blur;
    precalcDist = clamp(precalcDist, -1, 1);
    borderBlur /= 100.;

    float ic = precalcDist * .5 + .5;
    // float oc = -precalcDist * .5 + .5; 
    float oc = 1. - ic;

    float4 col = lerp(outerColor, innerColor, ic);
    if (pos < 1) col = lerp(innerColor, outerColor, oc);

    float bs = borderSize / resolution;
    float borderLimit = bs + borderBlur;
    if (rawDist < borderLimit) {
        float lowBound = bs - borderBlur;
        float highBound = borderLimit;
        float blurDist = borderBlur <= 0 ? 0. : (rawDist - lowBound) / (highBound - lowBound);
        blurDist = clamp(blurDist, 0., 1.);
        col = lerp(borderColor, col, blurDist);
    }
    return col;
}

float4 PolyFillGrad_float(float precalcDist,
                        float2 fragCoord,
                        float resolution,
                        float4 innerColor1,
                        float4 innerColor2, 
                        float4 outerColor,
                        float angleRads)
{
    float pos = sign(precalcDist);
    if (pos <= 0) return outerColor;

    float2 from = float2(cos(angleRads) * .5, sin(angleRads) * .5);
    float2 to = -from;
    from = from + float2(.5, .5);
    to = to + float2(.5, .5);
    float2 gradVec = to - from;
    float value = dot(fragCoord.xy - from, gradVec) / dot(gradVec, gradVec);
    value = clamp(value, 0.0, 1.0);
    
    float4 col = lerp(innerColor1, innerColor2, value);
    if (pos < 0) col = lerp(innerColor1, innerColor2, 1 - value);
    return col;
}

float4 PolyFillGradCone_float(float precalcDist,
                        float2 fragCoord,
                        float resolution,
                        float4 innerColor1,
                        float4 innerColor2, 
                        float4 outerColor)
{
    float pos = sign(precalcDist);
    if (pos <= 0) return outerColor;

    float theta = HALFPI;
    float2 xy = recenter(fragCoord);
    if (xy.x != 0) theta = atan(xy.y / xy.x);
    if (xy.x > 0) {
        if (xy.y > 0) { //quad 4
            theta = PI2 - theta;
        }
    } else if (xy.x < 0) {
        if (xy.y > 0) { //quad 2
            theta = PI - theta;
        } else { //quad 3
            theta = theta - PI;
        }
    }
    float angle01 = theta / PI2;

    float4 col = lerp(innerColor1, innerColor2, 1 - abs(angle01));
    return col;
}

void AlphaOverwrite_float(float4 Base, float4 Blend, out float4 Out)
{
    float4 result1 = 1.0 - 2.0 * (1.0 - Base) * (1.0 - Blend);
    float4 result2 = 2.0 * Base * Blend;
    float4 zeroOrOne = step(Base, 0.5);
    Out = result2 * zeroOrOne + (1 - zeroOrOne) * result1;
    Out = lerp(Base, Out, Blend.w);
}

#endif