using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
  public enum HSLComponent {
    Hue,
    Saturation,
    Lightness,
    All
  }

  public class PlantRandomizer {
    public enum RandomizerStrength {
      None,
      Smidgen,
      Low,
      MediumLow,
      Medium,
      MediumHigh,
      High,
      Max
    }
    public static HSL RandomizeHSL(HSLRange hslRange, LPRandomValCenterBias bias, HSLComponent component, RandomizerStrength str) =>
      RandomizeHSL(hslRange, new LPRandomValCenterBias[] { bias, bias, bias }, component, FloatForRandomizerStrength(str));

    public static HSL RandomizeHSL(HSLRange hslRange, LPRandomValCenterBias[] biases, HSLComponent component,
        float strAdjust) {
      if (biases.Length != 3) {
        Debug.LogError("RandomizeHSL biases needs 3 params: " + biases.ToLog());
        return hslRange.defaultValues;
      }

      LPRandomValCurve hueCurve = LPRandomValCurve.CenterBell;
      LPRandomValCurve slCurve = LPRandomValCurve.CenterBellLRSplit;
      float hue = hslRange.hueRange.Default;
      if (component == HSLComponent.Hue || component == HSLComponent.All) {
        hue = RandWithCurve(new FloatRange(0f, 1f, 0.5f), hueCurve, ValForCenterBias(biases[0]) * strAdjust);
        hue -= (0.5f - hslRange.hueRange.Default);
        hue = (hue + 1f) % 1f; //get it back into 0-1
      }
      bool doSat, doLit = doSat = component == HSLComponent.All;
      doSat |= component == HSLComponent.Saturation;
      doLit |= component == HSLComponent.Lightness;

      HSL newHSL = new HSL(hue,
        doSat ? RandWithCurve(hslRange.satRange, slCurve, ValForCenterBias(biases[1]) * strAdjust) : hslRange.satRange.Default,
        doLit ? RandWithCurve(hslRange.valRange, slCurve, ValForCenterBias(biases[2]) * strAdjust) : hslRange.valRange.Default);

      // Debug.Log("RandomizeHSL from " + hslRange.defaultValues + " to " + newHSL + "[" + component + "]");
      return newHSL;
    }

    public static LeafParamDict RandomizeAllCats(RandomizerStrength str) =>
      Randomize(str, (LPCategory[])Enum.GetValues(typeof(LPCategory)));

    public static LeafParamDict Randomize(RandomizerStrength str, LPCategory[] includeCategories) {
      LeafParamDict defaults = LeafParamDefaults.Defaults;
      LeafParamDict result = new LeafParamDict();

      Dictionary<LPCategory, bool> includedCatsDict = new Dictionary<LPCategory, bool>();
      foreach (LPCategory cat in Enum.GetValues(typeof(LPCategory)))
        includedCatsDict[cat] = includeCategories.Contains(cat);

      float strMult = FloatForRandomizerStrength(str);
      foreach (LPK key in defaults.Keys) {
        LeafParam param = defaults[key].Copy();
        float catMult = includedCatsDict[param.category] ? 1f : 2f;
        float finalMult = strMult * catMult;
        if (param.softTechs != null) {
          RandomizerStrength softRS = RandomizerStrength.High; //1f == neutral
          finalMult *= FloatForRandomizerStrength(softRS);
          Debug.Log("Applying tech softening param [" + param.key + "] softening [" + softRS + "] finalMult: " + finalMult);
        }

        if (param.mode == LPMode.Float) {
          param.value = RandWithCurve(param.range, param.randomValCurve, BIAS(param) * finalMult);
        } else if (param.mode == LPMode.ToggleDEPRECATED) {
          //no op
        } else if (param.mode == LPMode.ColorHSL) {
          param.hslValue = RandomizeHSL(param.hslRange, param.randomValCenterBiases, HSLComponent.All, finalMult);
        } else {
          Debug.LogError("Randomize param mode invalid: " + param.mode + " for key " + key);
        }

        result[key] = param;
      }

      SetSpecifics(result);

      // foreach (LPRandomValCurve c in Enum.GetValues(typeof(LPRandomValCurve))) {
      // for (int j = 0; j < 6; j++) {
      //   Dictionary<int, int> d = new Dictionary<int, int>();
      //   FloatRange fr = new FloatRange(0f, 200f, 100f);
      //   float bias = 1f + ((j - 3f) / 5f);
      //   for (int i = 0; i < 100000; i++) {
      //     int r = Mathf.RoundToInt(RandWithCurve(fr, LPRandomValCurve.CenterBellLRSplit, bias));
      //     if (!d.ContainsKey(r)) d[r] = 0;
      //     d[r]++;
      //   }
      //   Debug.Log(bias + ": ");
      //   Debug.Log(d.ToLog());
      // }

      return result;
    }

    //for centerBias, use ValForCenterBias
    public static float RandWithCurve(FloatRange range, LPRandomValCurve curve, float centerBias = 1f, int depth = 0) {
      if (depth > 3) {
        Debug.LogError("What a remarkable string of luck! " + range + " | " + curve);
        return range.Default;
      }

      float u1 = BWRandom.Range(0f, 1f);
      float u2 = BWRandom.Range(0f, 1f);
      // Debug.Log("u1: " + u1 + " | u2: " + u2);
      if (centerBias == 0f) centerBias = 0.01f;
      float stdDevDivisor = (3.2f * centerBias);

      if (curve == LPRandomValCurve.Flat) {
        return BWRandom.Range(range.Start, range.End);

      } else if (curve == LPRandomValCurve.CenterBell) {
        float randStdNormal = Mathf.Sqrt(-2f * Mathf.Log(u1)) *
                              Mathf.Sin(2f * Polar.Pi * u2); //random normal(0,1)
        float devRange = (range.Default - range.Start < range.End - range.Default) ? range.Default - range.Start : range.End - range.Default;
        float stdDev = devRange / stdDevDivisor;
        float randNormal = range.Default + (stdDev * randStdNormal);
        if (randNormal < range.Start || randNormal > range.End) randNormal = RandWithCurve(range, curve, centerBias, depth + 1);
        return randNormal;

      } else if (curve == LPRandomValCurve.CenterBellLRSplit) {
        float randStdNormal = Mathf.Sqrt(-2f * Mathf.Log(u1)) *
                              Mathf.Sin(2f * Polar.Pi * u2); //random normal(0,1)
        float devRange = randStdNormal < 0f ? range.Default - range.Start : range.End - range.Default;
        float stdDev = devRange / stdDevDivisor;
        float randNormal = range.Default + (stdDev * randStdNormal);
        if (randNormal < range.Start || randNormal > range.End) randNormal = RandWithCurve(range, curve, centerBias, depth + 1);
        return randNormal;

      } else if (curve == LPRandomValCurve.ReverseBell) {
        Debug.LogWarning("LPRandomValCurve.ReverseBell not implemented");
        return BWRandom.Range(range.Start, range.End);
      } else if (curve == LPRandomValCurve.DefaultValueOnly) {
        return range.Default;
      }

      return range.Default;
    }

    private static void SetSpecifics(LeafParamDict dict) {
      dict[LPK.Pudge].enabled = dict[LPK.Heart].value <= 0f;
      dict[LPK.PotScale].value = dict[LPK.LeafScale].value;
    }

    public static float ValForCenterBias(LPRandomValCenterBias b) {
      switch (b) {
        case LPRandomValCenterBias.Default: return 1f;
        case LPRandomValCenterBias.Spread1: return 0.8f;
        case LPRandomValCenterBias.Spread2: return 0.6f;
        case LPRandomValCenterBias.Spread3: return 0.4f;
        case LPRandomValCenterBias.Squeeze1: return 1.2f;
        case LPRandomValCenterBias.Squeeze2: return 1.4f;
        case LPRandomValCenterBias.Squeeze3: return 1.7f;
        case LPRandomValCenterBias.Squeeze4: return 2.0f;
        case LPRandomValCenterBias.Squeeze5: return 2.4f;
        case LPRandomValCenterBias.Squeeze6: return 2.8f;
        case LPRandomValCenterBias.None: return 100000f;
        default: Debug.LogError("Unrecognized LPRandomValCenterBias " + b); return 1f;
      }
    }

    public static float FloatForRandomizerStrength(RandomizerStrength str) {
      switch (str) {
        case RandomizerStrength.None: return 1000f;
        case RandomizerStrength.Smidgen: return 5f;
        case RandomizerStrength.Low: return 2f;
        case RandomizerStrength.MediumLow: return 1.75f;
        case RandomizerStrength.Medium: return 1.5f;
        case RandomizerStrength.MediumHigh: return 1.25f;
        case RandomizerStrength.High: return 1f;
        case RandomizerStrength.Max: return 0.5f;
        default: Debug.LogError("Unrecognized RandomizerStrength: " + str); break;
      }
      return 1f;
    }

    private static float BIAS(LeafParam p) => ValForCenterBias(p.randomValCenterBias);
    private static float BIAS(LeafParam p, int idx) => ValForCenterBias(p.randomValCenterBiases[idx]);
  }
}
