#ifndef __POLY_FILL_VERTEX_SHADER_INCLUDED__
#define __POLY_FILL_VERTEX_SHADER_INCLUDED__

float sdEllipse(float2 p, float2 elPos, float2 wh) {
    // symmetry
    p -= elPos;
	p = abs(p);
    
    // initial value
    float2 q = wh * (p - wh);
    float2 cs = normalize((q.x < q.y) ? float2(0.01, 1) : float2(1, 0.01) );
    
    // find root with Newton solver (see https://www.shadertoy.com/view/4lsXDN)
    for (int i = 0; i < 5; i++) {
        float2 u = wh * float2( cs.x, cs.y);
        float2 v = wh * float2(-cs.y, cs.x);
        
        float a = dot(p - u, v);
        float c = dot(p - u, u) + dot(v, v);
        float b = sqrt(c * c - a * a);
        
        cs = float2(cs.x * b-cs.y * a, cs.y * b + cs.x * a) / c;
    }
    
    // compute final point and distance
    float d = length(p - wh * cs);
    
    // return signed distance
    return (dot(p / wh, p / wh) > 1.0) ? d : -d;
}

float4 EllipseFill(
    float2 ellipseCoord,
    float ellipseRad,
    float2 UV,
    float resolution,
    float blur, 
    float borderSize, 
    float borderBlur, 
    float4 innerColor, 
    float4 outerColor, 
    float4 borderColor)
{
    float2 p = (2. * UV.xy - 1.);

    float dist = -sdEllipse(p, 2. * ellipseCoord - 1., float2(ellipseRad, ellipseRad));
    float rawDist = abs(dist);
    float pos = sign(dist);
    if (blur == 0) blur = 0.001;
    dist /= blur;
    dist = clamp(dist, -1, 1);

    float ic = dist * .5 + .5;
    float oc = -dist * .5 + .5;

    float4 col = lerp(outerColor, innerColor, ic);
    if (pos < 1) col = lerp(innerColor, outerColor, oc);

    float bs = borderSize / resolution;
    float bBlur = bs * (borderBlur + 1.);
    if (rawDist < bBlur) {
        float lowBound = 0;
        float highBound = bBlur;
        float blurDist = borderBlur <= 0 ? 0. : (rawDist - lowBound) / (highBound - lowBound);
        blurDist = clamp(blurDist, 0., 1.);
        col = lerp(borderColor, col, blurDist);
    }
    return col;
}

#endif