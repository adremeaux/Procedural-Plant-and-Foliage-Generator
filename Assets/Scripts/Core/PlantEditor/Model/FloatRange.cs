using System;

namespace BionicWombat {

  [Serializable]
  public class FloatRange {
    public static FloatRange Zero = new FloatRange(0, 0, 0);
    public FloatRange() { }
    public FloatRange(float start, float end) : this(start, end, -1f) { }
    public FloatRange(float start, float end, float def) {
      Start = start;
      End = end;
      Default = def;
    }


    public float Start;
    public float End;
    public float Default;
    public float Span => End - Start;

    public override string ToString() => "(" + Start + "-" + Default + "-" + End + ")";

    public bool SoftEquals(FloatRange other) {
      bool eq = true;
      eq &= Start == other.Start;
      eq &= End == other.End;
      return eq;
    }

    public FloatRange WithDefault(float def) => new FloatRange(Start, End, def);
  }

}
