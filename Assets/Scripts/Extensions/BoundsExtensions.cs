using UnityEngine;
using static BionicWombat.VectorExtensions;

namespace BionicWombat {
  public static class BoundsExtensions {
    public static float Left(this Bounds b) {
      return b.min.x;
    }

    public static float Right(this Bounds b) {
      return b.max.x;
    }

    public static float Top(this Bounds b) {
      return b.max.y;
    }

    public static float Bottom(this Bounds b) {
      return b.min.y;
    }

    public static Vector2 TopLeft(this Bounds b) {
      return b.center + b.extents.MultX(-1f);
    }

    public static Vector2 TopRight(this Bounds b) {
      return b.center + b.extents;
    }

    public static Vector2 BottomRight(this Bounds b) {
      return b.center + b.extents.MultY(-1f);
    }

    public static Vector2 BottomLeft(this Bounds b) {
      return b.center - b.extents;
    }
  }

}