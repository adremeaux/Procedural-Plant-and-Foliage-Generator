using System;
using System.Xml.Serialization;
using UnityEngine;

namespace BionicWombat {
[Serializable]
public class HSLRange {
  public static HSLRange Zero = new HSLRange(FloatRange.Zero, FloatRange.Zero, FloatRange.Zero);

  public FloatRange hueRange;
  public FloatRange satRange;
  public FloatRange valRange;
  [XmlIgnore] public HSL defValue;

  public HSLRange(FloatRange hue, FloatRange saturation, FloatRange value) {
    hueRange = hue;
    satRange = saturation;
    valRange = value;
    defValue = new HSL(hueRange.Default, satRange.Default, valRange.Default);
  }

  public HSLRange() { }

  public override string ToString() => "(" + hueRange + "-" + satRange + "-" + valRange + "): " + defValue;

  public bool SoftEquals(HSLRange other) {
    bool eq = true;
    eq &= hueRange.SoftEquals(other.hueRange);
    eq &= satRange.SoftEquals(other.satRange);
    eq &= valRange.SoftEquals(other.valRange);
    return eq;
  }
}
}