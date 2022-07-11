using System;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
public static class FloatExtensions {
  public static bool SoftEquals(this float f, float f2, float precision = 0.01f) =>
    Mathf.Abs(f - f2) <= precision;

  public static bool SoftEqualsMany(this float f, params float[] others) =>
    others.All((float f2) => Mathf.Abs(f - f2) <= 0.01f);

  public static float Truncate(this float f, int digits) {
    float n = UnityEngine.Mathf.Pow(10f, (float)digits);
    return UnityEngine.Mathf.Round(f * n) / n;
  }
}

}