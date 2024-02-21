using System.Collections.Generic;
using mattatz.Triangulation2DSystem;
using UnityEngine;

namespace BionicWombat {
  public class EqEqEqualityComparer : IEqualityComparer<Vector2> {
    public bool Equals(Vector2 a, Vector2 b) => a == b;
    public int GetHashCode(Vector2 a) => a.GetHashCode();
  }

  public class SegComparer : IEqualityComparer<Segment2D> {
    public bool Equals(Segment2D s1, Segment2D s2) {
      return (s1.a == s2.a && s1.b == s2.b) || (s1.a == s2.b && s1.b == s2.a);
    }
    public int GetHashCode(Segment2D a) => a.a.GetHashCode() + a.b.GetHashCode();
  }
}
