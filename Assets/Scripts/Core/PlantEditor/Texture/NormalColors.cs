using UnityEngine;

namespace BionicWombat {
  public static class NormalColors {
    public static Color Facing = new Color(0.502f, 0.502f, 1f, 1f);

    public static Color ColorWithPolar3(Polar3 polar, float alpha = 1.0f) {
      polar.longi = Mathf.Max(Mathf.Min(Polar.HalfPi, polar.longi), -Polar.HalfPi);
      polar.lat = Mathf.Max(Mathf.Min(Polar.HalfPi, polar.lat), -Polar.HalfPi);

      float a = Mathf.Cos(polar.lat);
      float x = a * Mathf.Sin(polar.longi) * polar.len;
      float y = Mathf.Sin(polar.lat) * polar.len;
      float z = a * Mathf.Cos(polar.longi);
      // x = polar.len * Mathf.Sin(polar.longi);
      // y = 0f;
      // z = polar.len * Mathf.Cos(polar.longi);
      // x = 0f;
      // y = polar.len * Mathf.Sin(polar.lat);
      // z = polar.len * Mathf.Cos(polar.lat);

      x = x / 2f + 0.5f;
      y = y / 2f + 0.5f;
      z = z / 2f + 0.5f;
      return new Color(x, y, z, alpha);
    }

    private static bool CheckRange(float v, float range) {
      return (v <= range && v >= -range);
    }
  }
}
