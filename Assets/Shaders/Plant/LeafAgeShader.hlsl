#include "LeafAgeData.hlsl"
#include "Assets/Materials/PolyFill/EllipseFillFragShader.hlsl"
#include "Assets/Materials/PolyFill/BlendModesComposite.hlsl"

StructuredBuffer<AgeSpotGPU> _AgeSpots;

uint _NumSpots() {
    uint numStructs, stride;
    _AgeSpots.GetDimensions(numStructs, stride);
    return numStructs;
}

bool _SoftEqualsAll(float v, float a, float b, float c) {
    return abs(v - a) < .01 && abs(v - b) < .01 && abs(v - c) < .01;
}

//WIP: _AgeSpots buffer doesn't seem to be populated
void GenAgeSpots_float(float2 uv, out float4 Out) {
    // uint spotsCount = 2;//_NumSpots();
    uint spotsCount = 1;//_NumSpots();

    float4 white = float4(1., 1., 1., 1.);
    float4 black = float4(0., 0., 0., 1.);

    float4 col1 = float4(0, 0.3, 1., 1.);
    float4 col2 = float4(.6, .6, .4, 1.);
    float4 baseCol = black;
    for (uint i = 0; i < spotsCount; i++) {
        float4 color = EllipseFill(_AgeSpots[i].pos, _AgeSpots[i].size, uv,
            512, 0, 0, 0, white, black, black);
        // float2 pos = i == 0 ? float2(0.4, 0.5) : float2(0.6, 0.5);
        // float4 color = EllipseFill(pos, .3, uv,
        //     512, 0, 0, 0, i == 0 ? col1 : col2, black, black);
        baseCol = Blend(baseCol, color, 11);
        // baseCol = color;
    }
    Out = baseCol;
}
