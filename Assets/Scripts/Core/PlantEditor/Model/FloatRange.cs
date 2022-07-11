using System;
using UnityEngine;

namespace BionicWombat {
[Serializable]
public class FloatRange {
  public static FloatRange Zero = new FloatRange(0, 0, 0);
  public FloatRange(float start, float end, float def) {
    Start = start;
    End = end;
    Default = def;
  }

  public FloatRange() { }

  public float Start;
  public float End;
  public float Default;

  public override string ToString() => "(" + Start + "-" + Default + "-" + End + ")";

  public bool SoftEquals(FloatRange other) {
    bool eq = true;
    eq &= Start == other.Start;
    eq &= End == other.End;
    return eq;
  }
}

}