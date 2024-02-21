using System;
using System.Xml.Serialization;

namespace BionicWombat {

  [Serializable]
  public class HSLRange {
    public static HSLRange Zero = new HSLRange(FloatRange.Zero, FloatRange.Zero, FloatRange.Zero);

    public FloatRange hueRange;
    public FloatRange satRange;
    public FloatRange valRange;
    [XmlIgnore] public HSL defaultValues;

    public HSLRange(FloatRange hue, FloatRange saturation, FloatRange lightness) {
      hueRange = hue;
      satRange = saturation;
      valRange = lightness;
      defaultValues = new HSL(hueRange.Default, satRange.Default, valRange.Default);
    }

    public HSLRange() { }

    public override string ToString() => "(" + hueRange + "-" + satRange + "-" + valRange + "): " + defaultValues;

    public bool SoftEquals(HSLRange other) {
      bool eq = true;
      eq &= hueRange.SoftEquals(other.hueRange);
      eq &= satRange.SoftEquals(other.satRange);
      eq &= valRange.SoftEquals(other.valRange);
      return eq;
    }
  }
}
