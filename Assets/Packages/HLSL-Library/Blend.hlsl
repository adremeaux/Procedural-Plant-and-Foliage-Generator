/*
---------------------------------------------------------------------------------------
REFERENCES
http://www.w3.org/TR/compositing-1
https://www.shadertoy.com/view/XdS3RW
http://stackoverflow.com/questions/5919663/how-does-photoshop-blend-two-images-together
---------------------------------------------------------------------------------------

----------------------
BLEND MODES
----------------------
None                 0
                DARKEN
Darken               1
Multiply		         2
Color Burn           3
Linear Burn          4
Darker Color         5
               LIGHTEN
Lighten              6
Screen               7
Color Dodge          8
Linear Dodge (Add)   9
Lighter Color       10
              CONTRAST
Overlay             11
Soft Light          12
Hard Light          13
Vivid Light         14
Linear Light        15
Pin Light           16
Hard Mix            17
             INVERSION
Difference          16
Exclusion           19
           CANCELATION
Subtract            20
Divide              21
             COMPONENT
Hue                 22
Saturation          23
Color               24
Luminosity          25
______________________

----------------------
PORTER DUFF COMPOSITES
----------------------
Clear                0
Copy                 1
Dest          2
Src Over          3
Dest Over     4
Src In            5
Dest In       6
Src Out           7
Dest In       8
Src Atop          9
Dest Atop    10
XOR                 11
Lighter             12
______________________

----------------------
COVERAGE MODES
----------------------
None                 0
Both                 1
Src only          2
Dest only     3
______________________
*/

#ifndef HUBRIS_BLEND
#define HUBRIS_BLEND
// REQUIRES
#include "Blend/Darken.hlsl"
#include "Blend/Multiply.hlsl"
#include "Blend/ColorBurn.hlsl"
#include "Blend/LinearBurn.hlsl"
#include "Blend/DarkerColor.hlsl"
#include "Blend/Lighten.hlsl"
#include "Blend/Screen.hlsl"
#include "Blend/ColorDodge.hlsl"
#include "Blend/LinearDodge.hlsl"
#include "Blend/LighterColor.hlsl"
#include "Blend/Overlay.hlsl"
#include "Blend/SoftLight.hlsl"
#include "Blend/HardLight.hlsl"
#include "Blend/VividLight.hlsl"
#include "Blend/LinearLight.hlsl"
#include "Blend/PinLight.hlsl"
#include "Blend/HardMix.hlsl"
#include "Blend/Difference.hlsl"
#include "Blend/Exclusion.hlsl"
#include "Blend/Subtract.hlsl"
#include "Blend/Divide.hlsl"
#include "Blend/Hue.hlsl"
#include "Blend/Saturation.hlsl"
#include "Blend/Color.hlsl"
#include "Blend/Luminosity.hlsl"

// namespace Blend {
  // int BlendOverwrite = 0;
  // int BlendDarken = 1;
  // int BlendMultiply = 2;
  // int BlendColorBurn = 3;
  // int BlendLinearBurn = 4;
  // int BlendDarkerColor = 5;
  // int BlendLighten = 6;
  // int BlendScreen = 7;
  // int BlendColorDodge = 8;
  // int BlendLinearDodge= 9;
  // int BlendLighterColor = 10;
  // int BlendOverlay = 11;
  // int BlendSoftLight = 12;
  // int BlendHardLight = 13;
  // int BlendVividLight = 14;
  // int BlendLinearLight = 15;
  // int BlendPinLight = 16;
  // int BlendHardMix = 17;
  // int BlendDifference = 16;
  // int BlendExclusion = 19;
  // int BlendSubtract = 20;
  // int BlendDivide = 21;
  // int BlendHue = 22;
  // int BlendSaturation = 23;
  // int BlendColor = 24;
  // int BlendLuminosity = 25;

  // int CompClear = 0;
  // int CompCopy = 1;
  // int CompDest = 2;
  // int CompSrcOver = 3;
  // int CompDestOver = 4;
  // int CompSrcIn = 5;
  // int CompDestIn = 6;
  // int CompSrcOut = 7;
  // int CompDestOut = 8;
  // int CompSrcAtop = 9;
  // int CompDestAtop = 10;
  // int CompXOR = 11;
  // int CompLighter = 12;
// }

namespace ColorLib
{
    //Below: inline float4 Blend(float4 source, float4 destination, int blendMode, float reduce = 1.) {
    inline float4 Blend(float4 source, float4 destination, int blendMode, 
      int composite)
    {
        // ---> Mix
        float3 mixed = 0.0;

        // ---> Blend
        if (blendMode == 0) mixed = destination.rgb;
        else if (blendMode == 1) mixed = Darken(source, destination);
        else if (blendMode == 2) mixed = Multiply(source, destination);
        else if (blendMode == 3) mixed = ColorBurn(source, destination);
        else if (blendMode == 4) mixed = LinearBurn(source, destination);
        else if (blendMode == 5) mixed = DarkerColor(source, destination);
        else if (blendMode == 6) mixed = Lighten(source, destination);
        else if (blendMode == 7) mixed = Screen(source, destination);
        else if (blendMode == 8) mixed = ColorDodge(source, destination);
        else if (blendMode == 9) mixed = LinearDodge(source, destination);
        else if (blendMode == 10) mixed = LighterColor(source, destination);
        else if (blendMode == 11) mixed = Overlay(source, destination);
        else if (blendMode == 12) mixed = SoftLight(source, destination);
        else if (blendMode == 13) mixed = HardLight(source, destination);
        else if (blendMode == 14) mixed = VividLight(source, destination);
        else if (blendMode == 15) mixed = LinearLight(source, destination);
        else if (blendMode == 16) mixed = PinLight(source, destination);
        else if (blendMode == 17) mixed = HardMix(source, destination);
        else if (blendMode == 18) mixed = Difference(source, destination);
        else if (blendMode == 19) mixed = Exclusion(source, destination);
        else if (blendMode == 20) mixed = Subtract(source, destination);
        else if (blendMode == 21) mixed = Divide(source, destination);
        else if (blendMode == 22) mixed = Hue(source, destination);
        else if (blendMode == 23) mixed = Saturation(source, destination);
        else if (blendMode == 24) mixed = Color(source, destination);
        else if (blendMode == 25) mixed = Luminosity(source, destination);

        float3 blend = (1.0 - source.a) * destination.rgb + source.a * mixed.rgb;

        // ---> Composite
        float2 porterDuff = 0.0;
    
        // Clear
        if (composite == 0) porterDuff = 0.0;
        // Copy
        else if (composite == 1) porterDuff.x = 1.0, porterDuff.y = 0.0;
        // Dest
        else if (composite == 2) porterDuff.x = 0.0, porterDuff.y = 1.0;
        // Src Over
        else if (composite == 3) porterDuff.x = 1.0, porterDuff.y = 1.0 - destination.a;
        // Dest Over
        else if (composite == 4) porterDuff.x = 1.0 - source.a, porterDuff.y = 1.0;
        // Src In
        else if (composite == 5) porterDuff.x = source.a, porterDuff.y = 0.0;
        // Dest In
        else if (composite == 6) porterDuff.x = 0.0, porterDuff.y = destination.a;
        // Src Out
        else if (composite == 7) porterDuff.x = 1.0 - source.a, porterDuff.y = 0.0;
        // Dest Out
        else if (composite == 8) porterDuff.x = 0.0, porterDuff.y = 1.0 - destination.a;
        // Src Atop
        else if (composite == 9) porterDuff.x = source.a, porterDuff.y = 1.0 - destination.a;
        // Dest Atop
        else if (composite == 10) porterDuff.x = 1.0 - source.a, porterDuff.y = destination.a;
        // XOR
        else if (composite == 11) porterDuff.x = 1.0 - source.a, porterDuff.y = 1.0 - destination.a;
        // Lighter
        else if (composite == 12) porterDuff = 1.0;
    
        float3 porterDuffComp = destination.a * porterDuff.x * blend + source.a * porterDuff.y * source.rgb;
    //     float num = destination.a;
    // porterDuffComp = float3(num, num, num);
    
        // ---> Coverage
        float cover = 0.0;
    
        // None
        // if (coverage == 0) cover = (1.0 - destination.a) * (1.0 - source.a);
        // // Both
        // else if (coverage == 1) cover = destination.a * source.a;
        // // Src only
        // else if (coverage == 2) cover = destination.a * (1.0 - source.a);
        // // Dest only
        // else if (coverage == 3) cover = source.a * (1.0 - destination.a);
        cover = max(source.a, destination.a);

        // ---> Final
        return float4(clamp(porterDuffComp, 0.0, 1.0), clamp(cover, 0.0, 1.0));
    }

    inline float4 Blend(float4 source, float4 destination, int blendMode, float reduce) {
      return lerp(source, Blend(source, destination, blendMode, 3), clamp(reduce, 0, 1));
    }

    inline float4 Blend(float4 source, float4 destination, int blendMode) {
      return Blend(source, destination, blendMode, 3);
    }
}

#endif // HUBRIS_BLEND