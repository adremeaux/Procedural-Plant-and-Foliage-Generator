using UnityEngine;

namespace BionicWombat {
[System.Serializable]
public struct HSL {
  public float hue;
  public float saturation;
  public float lightness;
  public float alpha;

  public HSL(float h, float s, float l, float a = 1f) {
    this.hue = h;
    this.saturation = s;
    this.lightness = l;
    this.alpha = a;
  }

  public HSL(HSL hsl) {
    this.hue = hsl.hue;
    this.saturation = hsl.saturation;
    this.lightness = hsl.lightness;
    this.alpha = hsl.alpha;
  }

  public HSL WithHue(float h) => new HSL(h, saturation, lightness, alpha);
  public HSL WithSat(float s) => new HSL(hue, s, lightness, alpha);
  public HSL WithLit(float l) => new HSL(hue, saturation, l, alpha);
  public HSL WithAlpha(float a) => new HSL(hue, saturation, lightness, a);

  public Color ToColor() {
    float t = saturation * ((lightness < 0.5f) ? lightness : (1f - lightness));
    float v = lightness + t;
    float s = (lightness > 0f) ? (2f * t / v) : 0f;
    return Color.HSVToRGB(hue, s, v);
  }

  public HSL Mix(Color other, float perc = 0.5f) => Mix(other.ToHSL(), perc);

  public HSL Mix(HSL other, float perc = 0.5f) {
    float hue = Mathf.LerpAngle(this.hue, other.hue, perc);
    if (Mathf.Abs(this.hue - other.hue) > 0.5f)
      hue = this.hue > other.hue ? Mathf.LerpAngle(this.hue - 1f, other.hue, perc) : Mathf.LerpAngle(this.hue, other.hue - 1f, perc);
    if (hue < 0) hue += 1f;

    float saturation = Mathf.Lerp(this.saturation, other.saturation, perc);
    float lightness = Mathf.Lerp(this.lightness, other.lightness, perc);
    return new HSL(hue, saturation, lightness);
  }
}
}