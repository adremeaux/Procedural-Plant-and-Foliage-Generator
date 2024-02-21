//UNITY_SHADER_NO_UPGRADE
#ifndef PARALLAX_INCLUDED
#define PARALLAX_INCLUDED

void Parallax_float(float HeightScale, float3 ViewDir, float2 UVs, UnityTexture2D HeightTex, 
    UnitySamplerState SampleState, out float2 Out) {
  const float minLayers = 30;
  const float maxLayers = 60;
  float numLayers = lerp(maxLayers, minLayers, abs(dot(float3(0, 0, 1), ViewDir)));
  float numSteps = numLayers;//60.0f; // How many steps the UV ray tracing should take
  float height = 1.0;
  float step = 1.0 / numSteps;

  float2 offset = UVs.xy;
  // float4 HeightMap = HeightTex.Sample(SampleState, offset);
  float heightR = 1.0 - HeightTex.Sample(SampleState, offset).r;

  float2 delta = ViewDir.xy * -HeightScale / (ViewDir.z * numSteps);

  // find UV offset
  for (float i = 0.0f; i < numSteps; i++) {
    if (heightR < height) {
      height -= step;
      offset += delta;
      heightR = 1.0 - HeightTex.Sample(SampleState, offset).r;
      // HeightMap = HeightTex.Sample(SampleState, offset);
    } else {
      break;
    }
  }
  Out = offset;
}

#endif //PARALLAX_INCLUDED