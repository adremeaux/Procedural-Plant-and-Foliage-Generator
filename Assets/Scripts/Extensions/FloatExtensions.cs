using System.Linq;
using UnityEngine;

namespace BionicWombat {
  public static class FloatExtensions {
    public static bool SoftEquals(this float f, float f2, float precision = 0.01f) =>
      Mathf.Abs(f - f2) <= precision;

    public static bool SoftEqualsMany(this float f, params float[] others) =>
      others.All((float f2) => Mathf.Abs(f - f2) <= 0.01f);

    public static float Truncate(this float f, int digits = 2) {
      float n = UnityEngine.Mathf.Pow(10f, (float)digits);
      return UnityEngine.Mathf.Round(f * n) / n;
    }

    public static float WrapAround(this float f, float maxExclusive) {
      if (f >= maxExclusive) f -= maxExclusive;
      if (f < 0) f += maxExclusive;
      return f;
    }

    public static int WrapAround(this int i, int maxExclusive) {
      if (i >= maxExclusive) i -= maxExclusive;
      if (i < 0) i += maxExclusive;
      return i;
    }

    public static bool WithinRange(this float f, float middle, float addOrSubtract) {
      return (f <= middle + addOrSubtract && f >= middle - addOrSubtract);
    }

    public static bool WithinRange(this int i, int lowIncl, int highIncl) {
      return i >= lowIncl && i <= highIncl;
    }
  }

}
