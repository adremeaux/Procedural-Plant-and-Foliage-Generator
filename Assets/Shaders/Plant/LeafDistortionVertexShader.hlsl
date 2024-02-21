#ifndef __LEAF_DISTORTION_VERTEX_SHADER_INCLUDED__
#define __LEAF_DISTORTION_VERTEX_SHADER_INCLUDED__

StructuredBuffer<float3> _VertDeltas;
StructuredBuffer<float3> _AdjustedNormals;

bool AnyNaN(float3 f)
{
  return IsNaN(f.x) || IsNaN(f.y) || IsNaN(f.z);
}

//old: float2 UV, UnityTexture2D HeightTex, float heightAmp, 
void AdjustPositions_float(float3 BasePos, float3 BaseNormal, float VertexID, float Instances, 
  float InstanceIdx, float LongCount, out float3 Position, out float3 Normal)
{
  uint longCount = (uint)LongCount;
  uint smallCount = longCount / (uint)Instances;
  //check small count for null

  uint newID = (uint)VertexID;
  if ((uint)VertexID >= smallCount) newID -= smallCount;

  int offset = InstanceIdx * smallCount;
  int finalIdx = (int)newID + offset;
  float3 newPos = BasePos + _VertDeltas[finalIdx];
  Position = newPos;

  Normal = _AdjustedNormals[finalIdx];
  if ((uint)VertexID >= smallCount) Normal *= -1;
  if (AnyNaN(Normal)) {
    for (uint i = 1; i <= 3; i++) {
      if (finalIdx >= i && !AnyNaN(_AdjustedNormals[finalIdx - i]))  Normal = _AdjustedNormals[finalIdx - i];
      else if (finalIdx + i < longCount && !AnyNaN(_AdjustedNormals[finalIdx + i]))  Normal = _AdjustedNormals[finalIdx + i];
    }
    if (AnyNaN(Normal)) Normal = BaseNormal;
  }

  // float xtraHeight = HeightTex.Sample(ss, UV).r;
  // float xtraHeight = SAMPLE_TEXTURE2D_LOD(HeightTex, HeightTex.samplerstate, UV, 0).r;
  // Position = Position + float3(0, 0, -xtraHeight * heightAmp * 20);
}

#endif