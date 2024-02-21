using UnityEngine;

namespace BionicWombat {
  public static class TransformExtentions {
    public static void Reset(this Transform t) {
      t.localPosition = Vector3.zero;
      t.localRotation = Quaternion.identity;
      t.localScale = new Vector3(1f, 1f, 1f);
    }

    public static void MatchTransform(this Transform t, Transform t2) {
      t.localPosition = t2.localPosition;
      t.localScale = t2.localScale;
      t.localRotation = t2.localRotation;
    }

    public static void SetX(this Transform t, float x) => t.localPosition = t.localPosition.WithX(x);
    public static void SetY(this Transform t, float y) => t.localPosition = t.localPosition.WithY(y);
    public static void SetZ(this Transform t, float z) => t.localPosition = t.localPosition.WithZ(z);
    public static void AddX(this Transform t, float x) => t.localPosition = t.localPosition.AddX(x);
    public static void AddY(this Transform t, float y) => t.localPosition = t.localPosition.AddY(y);
    public static void AddZ(this Transform t, float z) => t.localPosition = t.localPosition.AddZ(z);
  }
}
