using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace BionicWombat {
public static class ColorExtensions {
  private static int gradSteps = 8;
  public static Gradient GradientWith(Color a, Color b) {
    Gradient g = new Gradient();
    g.colorKeys = new GradientColorKey[] {
        new GradientColorKey(a, 0f),
        new GradientColorKey(b, 1f)
      };
    return g;
  }
  public static Gradient GradientWith(HSL a, HSL b) {
    List<GradientColorKey> list = new List<GradientColorKey>();
    for (int i = 0; i < gradSteps; i++) {
      float perc = (float)i / (float)gradSteps;
      HSL c = new HSL(Mathf.Lerp(a.hue, b.hue, perc),
        Mathf.Lerp(a.saturation, b.saturation, perc),
        Mathf.Lerp(a.lightness, b.lightness, perc));
      if (c.hue < 0) c.hue += 1f;
      if (c.hue > 1) c.hue -= 1f;
      list.Add(new GradientColorKey(c.ToColor(), perc));
    }
    Gradient g = new Gradient();
    g.colorKeys = list.ToArray();
    return g;
  }

  public static float Difference(this Color c, Color c2, bool useAlpha = false) {
    float r = Mathf.Abs(c.r - c2.r);
    float g = Mathf.Abs(c.g - c2.g);
    float b = Mathf.Abs(c.b - c2.b);
    float a = Mathf.Abs(c.a - c2.a);
    float avg = (r + g + b) / 3f;
    if (useAlpha) avg = (avg * 3f + a) / 4f;
    return avg;
  }

  public static Color ColorHex(string hex) {
    string colorcode = hex.TrimStart('#');

    float component(int pos) =>
      (float)int.Parse(colorcode.Substring(pos, 2), NumberStyles.HexNumber) / 255f;

    Color col; // from System.Drawing or System.Windows.Media
    if (colorcode.Length == 6)
      col = new Color(component(0),
                      component(2),
                      component(4));
    else // assuming length of 8
      col = new Color(component(0),
                      component(2),
                      component(4),
                      component(6));
    return col;
  }

  public static Color ColorRGB(int r, int g, int b) {
    return new Color((float)r / 255f, (float)g / 255f, (float)b / 255f);
  }

  public static string ToHex(this Color c, bool withAlpha = false) {
    return "#" + ((int)(c.r * 255f)).ToString("X2") +
                 ((int)(c.g * 255f)).ToString("X2") +
                 ((int)(c.b * 255f)).ToString("X2") +
                 (withAlpha ? ((int)(c.a * 255f)).ToString("X2") : "");
  }

  public static Color RandomColor() {
    return new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
  }

  public static Color RandomSaturatedColor() {
    float r = UnityEngine.Random.Range(0f, 1f);
    float g = UnityEngine.Random.Range(0f, 1f);
    float b = UnityEngine.Random.Range(0f, 1f);

    if (r >= g && r >= b) r = 1f;
    else if (g >= b) g = 1f;
    else b = 1f;

    if (r <= g && r <= b) r = 0f;
    else if (g <= b) g = 0f;
    else b = 0f;

    return new Color(r, g, b);
  }

  public static Color ColorLerp(Color a, Color b, float t) {
    return new Color(Mathf.Lerp(a.r, b.r, t),
                     Mathf.Lerp(a.g, b.g, t),
                     Mathf.Lerp(a.b, b.b, t),
                     Mathf.Lerp(a.a, b.a, t));
  }

  public static Color WithAlpha(this Color c, float alpha) => new Color(c.r, c.g, c.b, alpha);

  public static Color Lighter(this Color color, float increase = 0.1f) {
    HSL hsl = color.ToHSL();
    float span = .99f - hsl.lightness;
    hsl.lightness = Mathf.Clamp01(hsl.lightness + span * increase);
    return hsl.ToColor().WithAlpha(color.a);
  }

  public static Color Darker(this Color color, float decrease = 0.1f) {
    HSL hsl = color.ToHSL();
    hsl.lightness = Mathf.Clamp01(hsl.lightness * (1.0f - decrease));
    return hsl.ToColor().WithAlpha(color.a);
  }

  public static HSL ToHSL(this Color c) {
    float h, s, v;
    Color.RGBToHSV(c, out h, out s, out v);
    float l = ((2f - s) * v) / 2f;

    if (l == 0f || l == 1f)
      s = 0f;
    else if (l > 0f && l < 0.5f)
      s = (s * v) / (l * 2f);
    else
      s = (s * v) / (2f - l * 2f);

    return new HSL(h, s, l);
  }
}

}