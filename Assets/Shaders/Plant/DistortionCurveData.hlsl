#ifndef __DISTORTION_CURVE_DATA_INCLUDED__
#define __DISTORTION_CURVE_DATA_INCLUDED__

struct DistortionCurveData {
  uint shouldFade;
  uint useDistFade;
  uint isCup;
  float maxFadeDist;
  uint reverseFade;
  uint skipOutsideLowerBound;
  uint affectAxes;
  float leafWidth;
  float distClamp;
  int instances;
};

struct Curve3D {
  float3 p0;
  float3 h0;
  float3 h1;
  float3 p1;
};

struct Curve {
  float2 p0;
  float2 h0;
  float2 h1;
  float2 p1;
};

#endif