using System;
using System.Collections.Generic;
using UnityEngine;
using static BionicWombat.ColorExtensions;

namespace BionicWombat {
  using ModFunc = System.Func<LeafParam, float, System.Collections.Generic.Dictionary<LPK, LeafParam>, ColorADT>;
  public static class LeafParamBehaviors {
    private static Dictionary<LPK, ModFunc> _ColorTransformFuncs;
    public static Dictionary<LPK, ModFunc> ColorTransformFuncs {
      get {
        if (_ColorTransformFuncs == null) {
          _ColorTransformFuncs = new Dictionary<LPK, ModFunc>();

          _ColorTransformFuncs.Add(LPK.TexShadowStrength, (LeafParam self, float value, Dictionary<LPK, LeafParam> others) => {
            Color baseColor = others[LPK.TexBaseColor].colorValue;
            baseColor = IMTextureCmdDrawGradient.GradientDarkColor(baseColor); //slop!!!
            return new ColorADT(baseColor.Darker(0.6f * value));
          });

          _ColorTransformFuncs.Add(LPK.TexRadianceHue, (LeafParam self, float value, Dictionary<LPK, LeafParam> others) => {
            Color baseColor = others[LPK.TexBaseColor].colorValue;
            HSL adjColor = baseColor.Lighter(1f * others[LPK.TexRadianceLitPower].value).ToHSL();
            if (others[LPK.TexRadianceInversion].value > 0f)
              adjColor = baseColor.Darker(0.9f * others[LPK.TexRadianceLitPower].value).ToHSL();
            float hue = adjColor.hue + (value * 0.5f);
            return new ColorADT(adjColor.WithHue(hue));
          });

          _ColorTransformFuncs.Add(LPK.AbaxialHue, (LeafParam self, float value, Dictionary<LPK, LeafParam> others) => {
            Color baseColor = others[LPK.TexBaseColor].colorValue;
            HSL adjColor = baseColor.ToHSL().AddHue(value);
            return new ColorADT(adjColor);
          });

          _ColorTransformFuncs.Add(LPK.TrunkBrowning, (LeafParam self, float value, Dictionary<LPK, LeafParam> others) => {
            Color baseColor = GetColorForParam(others[LPK.TexShadowStrength], others);
            HSL lightBrown = new Color(222f / 255f, 214f / 255f, 195f / 255f).ToHSL();
            HSL darkBrown = new Color(27f / 255f, 19f / 255f, 14f / 255f).ToHSL();
            Color brown = darkBrown.Mix(lightBrown, others[LPK.TrunkLightness].value).ToColor();
            Color c = UnityEngine.Color.Lerp(baseColor, brown, value);
            return new ColorADT(c);
          });

          _ColorTransformFuncs.Add(LPK.StemTopColorHue, (LeafParam self, float value, Dictionary<LPK, LeafParam> others) => {
            Color baseColor = others[LPK.TexBaseColor].colorValue;
            float litVal = others[LPK.StemTopColorLit].value;
            HSL adjColor = default(HSL);
            if (litVal >= 0) adjColor = baseColor.Lighter(litVal).ToHSL();
            else adjColor = baseColor.Darker(litVal * -1f).ToHSL();
            float hue = adjColor.hue + (value * 0.5f);
            float sat = adjColor.saturation;
            float satVal = others[LPK.StemTopColorSat].value;
            if (satVal >= 0) sat = sat + (1f - sat) * satVal;
            else sat = sat + sat * satVal;
            return new ColorADT(adjColor.WithHue(hue).WithSat(sat));
          });
        }
        return _ColorTransformFuncs;
      }
    }

    public static Gradient GradientWithParamTransform(LeafParam modifier, Dictionary<LPK, LeafParam> others) {
      if (modifier.mode != LPMode.Float) {
        Debug.LogError("GradientWithParamTransform should only be called with mode Float. Given: " + modifier.mode);
        return null;
      }
      if (!ColorTransformFuncs.ContainsKey(modifier.key) || others.Count == 0) {
        Debug.LogWarning("GradientWithParamTransform called with invalid parameters");
        return null;
      }

      ModFunc func = ColorTransformFuncs[modifier.key];
      ColorADT c1 = func(modifier, modifier.range.Start, others);
      ColorADT c2 = func(modifier, modifier.range.End, others);
      return c1.isColor ? GradientWith(c1.color, c2.color) : GradientWith(c1.hsl, c2.hsl);
    }

    public static Color GetColorForParam(LeafParam modifier, Dictionary<LPK, LeafParam> others) {
      if (!ColorTransformFuncs.ContainsKey(modifier.key)) return modifier.colorValue; //not an error

      return ColorTransformFuncs[modifier.key](modifier, modifier.value, others).GetColor();
    }

    private static bool Expect(LeafParam[] others, int count, params LPMode[] modes) {
      if (others.Length != count || modes.Length == 0) {
        Debug.LogError("ColorTransformFuncs expected other params count " + count + " but got " + others.Length);
        return false;
      }
      int idx = 0;
      foreach (LeafParam p in others) {
        LPMode mode = modes.Length > 1 ? modes[idx] : modes[0];
        idx++;
        if (mode != p.mode) {
          Debug.LogError("ColorTransformFuncs expected mode " + mode + " got " + p.mode);
          return false;
        }
      }
      return true;
    }
  }

  public class ColorADT {
    public Color color;
    public HSL hsl;
    private bool _isColor;
    public bool isColor => _isColor;
    public ColorADT(Color c) {
      color = c;
      _isColor = true;
    }
    public ColorADT(HSL c) {
      hsl = c;
      _isColor = false;
    }
    public Color GetColor() => _isColor ? color : hsl.ToColor();
    public HSL GetHSL() => _isColor ? color.ToHSL() : hsl;
  }
}
