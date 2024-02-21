#ifndef __SHADER_UTILS_INCLUDED__
#define __SHADER_UTILS_INCLUDED__

float DecodeDebugFloat3(float3 debugVal, int primaryIdx, int secondaryIdx) {
  if (primaryIdx >= 3) return -1;
  float v = debugVal.x;
  if (primaryIdx == 1) v = debugVal.y;
  else if (primaryIdx == 2) v = debugVal.z;

  float e = pow(100, secondaryIdx);
  return int(v / e) % 100;

  //12550305 / 10000
  //1255.0305
  //1255 % 100
  //55
}

#endif