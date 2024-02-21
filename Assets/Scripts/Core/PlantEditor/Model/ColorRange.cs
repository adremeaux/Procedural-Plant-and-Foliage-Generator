using System;
using UnityEngine;

namespace BionicWombat {

  [Serializable]
  public class ColorRange {
    public static ColorRange Zero = new ColorRange(Color.black, Color.black, 0);
    public ColorRange(Color start, Color end, float def) {
      Start = start;
      End = end;
      Default = def;
    }

    public ColorRange() { }

    public Color Start;
    public Color End;
    public float Default;

    public override string ToString() => "(" + Start + "-" + Default + "-" + End + ")";

    public bool SoftEquals(ColorRange other) {
      bool eq = true;
      eq &= Start.ToHex() == other.Start.ToHex();
      eq &= End.ToHex() == other.End.ToHex();
      return eq;
    }
  }
}
