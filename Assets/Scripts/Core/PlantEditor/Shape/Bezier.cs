using UnityEngine;

namespace BionicWombat {
  public static class Bezier {

    public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t) {
      //return Vector3.Lerp(Vector3.Lerp(p0, p1, t), Vector3.Lerp(p1, p2, t), t);

      t = Mathf.Clamp01(t);
      float oneMinusT = 1f - t;
      return
        oneMinusT * oneMinusT * p0 +
        2f * oneMinusT * t * p1 +
        t * t * p2;
    }

    public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, float t) {
      return
        2f * (1f - t) * (p1 - p0) +
        2f * t * (p2 - p1);
    }

    public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
      t = Mathf.Clamp01(t);
      float oneMinusT = 1f - t;
      return
        oneMinusT * oneMinusT * oneMinusT * p0 +
        3f * oneMinusT * oneMinusT * t * p1 +
        3f * oneMinusT * t * t * p2 +
        t * t * t * p3;
    }

    public static Vector3 GetFirstDerivative(Curve3D c, float t) => GetFirstDerivative(c.p0, c.h0, c.h1, c.p1, t);
    public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 h0, Vector3 h1, Vector3 p1, float t) {
      t = Mathf.Clamp01(t);
      // float oneMinusT = 1f - t;
      // return
      //   3f * oneMinusT * oneMinusT * (h0 - p0) +
      //   6f * oneMinusT * t * (h1 - h0) +
      //   3f * t * t * (p1 - h1);

      return 3f * t * t * (p1 + 3f * (h0 - h1) - p0) +
             6f * t * (p0 - 2f * h0 + h1) +
             3f * (h0 - p0);
    }
  }
}
