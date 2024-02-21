using System;
using System.Linq;
using mattatz.Triangulation2DSystem;
using Newtonsoft.Json;
using UnityEngine;

namespace BionicWombat {
  [Serializable]
  public class Curve {
    public Vector2 p0;
    public Vector2 h0;
    public Vector2 h1;
    public Vector2 p1;

    public Curve(Vector2 p0, Vector2 p1) : this(p0, p0, p1, p1) { }

    public Curve(Vector2 p0, Vector2 h0, Vector2 h1, Vector2 p1) {
      this.p0 = p0;
      this.h0 = h0;
      this.h1 = h1;
      this.p1 = p1;
    }

    public Curve Copy() => new Curve(p0, h0, h1, p1);

    public static implicit operator Curve3D(Curve c) => new Curve3D(c.p0, c.h0, c.h1, c.p1);

    public Curve Transform(Transform t) => t == null ? Copy() : new Curve(
      t.TransformPoint(p0),
      t.TransformPoint(h0),
      t.TransformPoint(h1),
      t.TransformPoint(p1));

    public float FastLength() {
      float len = 0;
      float steps = 4;
      for (int i = 0; i < steps; i++) {
        len += Vector2.Distance(GetPoint(i / steps), GetPoint((i + 1) / steps));
      }
      return len;
    }

    public (bool intersects, Vector2 pos) Intersects(Curve c2, int steps = 4, bool log = false) {
      Vector2 lastC1Point = p0;
      for (int j = 1; j < steps + 1; j++) {
        Vector2 nextC1Point = GetPoint(j / ((float)steps));
        Vector2 lastC2Point = c2.p0;
        for (int i = 1; i < steps + 1; i++) {
          Vector2 nextC2Point = c2.GetPoint(i / ((float)steps));
          // if (log) Debug.Log("lastC1Point: " + lastC1Point + " | nextC1Point: " + nextC1Point + " | lastC2Point: " + lastC2Point + " | nextC2Point: " + nextC2Point);
          (bool intersect, Vector2 pos) = Utils2D.Intersect(lastC1Point, nextC1Point, lastC2Point, nextC2Point);
          if (intersect && pos != p0 && pos != p1) {
            return (true, pos);
          }
          lastC2Point = nextC2Point;
        }
        lastC1Point = nextC1Point;
      }
      return (false, Vector2.zero);
    }

    public override string ToString() {
      return "p0 " + p0 + " | h0 " + h0 + " | h1 " + h1 + " p1 " + p1;
    }

    #region Modify

    public float WidthExtent {
      set {
        Vector2 m = new Vector2(value, 1f);
        p0 *= m;
        h0 *= m;
        h1 *= m;
        p1 *= m;
      }
    }

    public float LengthExtent {
      set {
        Vector2 m = new Vector2(1f, value);
        p0 *= m;
        h0 *= m;
        h1 *= m;
        p1 *= m;
      }
    }

    #endregion
    #region Operate 

    //returns the new curve
    public Curve Subdivide(float perc = 0.5f) {
      Vector3 p = GetPoint(perc);
      Vector3[] v1;
      Vector3[] v2;
      Subdivide2(this, perc, out v1, out v2);
      p0 = v1[0]; h0 = v1[1]; h1 = v1[2]; p1 = v1[3];
      return new Curve(v2[0], v2[1], v2[2], v2[3]);
    }

    public static void Subdivide2(Curve c, float t, out Vector3[] firstPart, out Vector3[] secondPart) {
      var b0 = Vector3.Lerp(c.p0, c.h0, t); // Same as evaluating a Bezier
      var b1 = Vector3.Lerp(c.h0, c.h1, t);
      var b2 = Vector3.Lerp(c.h1, c.p1, t);

      var c0 = Vector3.Lerp(b0, b1, t);
      var c1 = Vector3.Lerp(b1, b2, t);

      var d0 = Vector3.Lerp(c0, c1, t); // This would be the interpolated point

      firstPart = new Vector3[] { c.p0, b0, c0, d0 }; // first point of each step
      secondPart = new Vector3[] { d0, c1, b2, c.p1 }; // last point of each step
    }

    public Curve GetSlice(float from, float to) {
      if (from > to) {
        float temp = to;
        to = from;
        from = temp;
      }
      Vector3[] v1;
      Vector3[] v2;
      Subdivide2(this, to, out v1, out v2);

      Curve firstHalf = new Curve(v1[0], v1[1], v1[2], v1[3]);
      Subdivide2(firstHalf, from * to, out v1, out v2);

      return new Curve(v2[0], v2[1], v2[2], v2[3]);
    }

    #endregion
    #region Access

    public Vector3 GetPoint(float t) {
      if (t <= 0.0f) return p0;
      if (t >= 1.0f) return p1;

      return Bezier.GetPoint(p0, h0, h1, p1, t);// + deps.transform.position;
    }

    // public float Length() => Vector2.Distance(p0, p1);

    public static float Angle(Vector2 p0, Vector2 p1) {
      if (p0.x == p1.x) return p0.y > p1.y ? 270f : 90f;
      float a = (float)(Math.Atan((p1.y - p0.y) / (p1.x - p0.x)) * (180f / Math.PI));
      int quadrant = GetQuadrant(p0, p1);
      switch (quadrant) {
        case 0: return a;
        case 1: //with quad 2
        case 2: return 180f + a;
        case 3: return 360f + a;
      }
      return a;
    }

    private static int GetQuadrant(Vector2 v0, Vector2 v1) {
      if (v1.x > v0.x) {
        if (v1.y > v0.y) return 0;
        return 3;
      } else {
        if (v1.y > v0.y) return 1;
        return 2;
      }
    }

    [JsonIgnore]
    public float handlesInnerAngle {
      get {
        return Angle(h0, h1) * Polar.DegToRad;
      }
    }

    // Angle between p0 and h0
    [JsonIgnore]
    public float angle0 {
      get {
        return Angle(p0, h0);
      }
      set {
        float oldAngle = angle0;
        float delta = value - oldAngle;
        float dR = delta * (float)(Math.PI / 180f);

        float newX = ((h0.x - p0.x) * (float)Math.Cos(dR)) -
                     ((h0.y - p0.y) * (float)Math.Sin(dR)) + p0.x;
        float newY = ((h0.x - p0.x) * (float)Math.Sin(dR)) +
                     ((h0.y - p0.y) * (float)Math.Cos(dR)) + p0.y;
        h0 = new Vector2(newX, newY);
      }
    }

    // Angle between p1 and h1
    [JsonIgnore]
    public float angle1 {
      get {
        return Angle(p1, h1);
      }
      set {
        float oldAngle = angle1;
        float delta = value - oldAngle;
        float dR = delta * (float)(Math.PI / 180f);

        float newX = ((h1.x - p1.x) * (float)Math.Cos(dR)) -
                     ((h1.y - p1.y) * (float)Math.Sin(dR)) + p1.x;
        float newY = ((h1.x - p1.x) * (float)Math.Sin(dR)) +
                     ((h1.y - p1.y) * (float)Math.Cos(dR)) + p1.y;
        h1 = new Vector2(newX, newY);
      }
    }

    // Angle between p0 and p1
    [JsonIgnore]
    public float angleFull => Angle(p0, p1);

    [JsonIgnore]
    public float MaxX {
      get {
        return Math.Max(Math.Max(Math.Max(p0.x, p1.x), h0.x), h1.x);
      }
    }
    [JsonIgnore]
    public float MinY {
      get {
        return Math.Min(Math.Min(Math.Min(p0.y, p1.y), h0.y), h1.y);
      }
    }

    public float FindClosestAngle(float angle) {
      return FindApex(angle);
    }

    public const float NoOverride = float.MinValue;
    public float FindApex(float angleOveride = NoOverride) {
      int i;
      int lineSteps = 30;
      float[] angles = new float[lineSteps];
      Vector3 lineStart = GetPoint(0f);
      for (i = 1; i <= lineSteps; i++) {
        Vector3 lineEnd = GetPoint(i / (float)lineSteps);
        angles[i - 1] = CurveHelpers.Angle(lineStart, lineEnd);
        lineStart = lineEnd;
      }
      float target = angleOveride == NoOverride ? (angles.Last() + angles.First()) / 2f
        : angleOveride;

      (int, float) bestWithTarget(float target) {
        target = target > Polar.Pi2 ? target - Polar.Pi2 : target;
        float close = float.MaxValue;
        int bestIndex = -1;
        for (i = 0; i < angles.Length; i++) {
          float angle = angles[i];
          float dist = Math.Abs(target - angle);
          if (dist > 180f) dist = Math.Abs(360f - dist);
          if (dist < close) {
            close = dist;
            bestIndex = i;
          }
        }
        return (bestIndex, close);
      };
      (int b1, float d1) = bestWithTarget(target);
      (int b2, float d2) = bestWithTarget(target + Polar.Pi);
      float bestIndex = d1 < d2 ? b1 : b2;

      return (bestIndex + 0.5f) / lineSteps;
    }

    public Vector2 FindApexPoint(float angleOveride = NoOverride) {
      return GetPoint(FindApex(angleOveride));
    }

    public Vector3[] FlattenedPoints() => new Vector3[] { p0, h0, h1, p1 };

    public Vector3 GetClosestPoint(Vector2 p, int slices = 10, int iterations = 2) {
      return GetPoint(Curve3D.GetClosestPointToCubicBezier(p, this, slices, iterations));
    }

    public float GetPercentFromPoint(Vector2 p) {
      return Curve3D.GetClosestPointToCubicBezier(p, this, 10, 2);
    }
    #endregion
  }
}
