using System;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace BionicWombat {
  [Serializable]
  public class Curve3D {
    public Vector3 p0;
    public Vector3 h0;
    public Vector3 h1;
    public Vector3 p1;

    public Curve3D(Vector3 p0, Vector3 p1) : this(p0, p0, p1, p1) { }
    public Curve3D(LeafCurve lc) : this(lc.p0, lc.h0, lc.h1, lc.p1) { }
    public Curve3D(Curve c) : this(c.p0, c.h0, c.h1, c.p1) { }

    [JsonConstructor]
    public Curve3D(Vector3 p0, Vector3 h0, Vector3 h1, Vector3 p1) {
      this.p0 = p0;
      this.h0 = h0;
      this.h1 = h1;
      this.p1 = p1;
    }

    public override string ToString() {
      return "(p0 " + p0 + " [h0 " + h0 + ", h1 " + h1 + "], p1 " + p1 + ")";
    }

    public float FastLength() {
      float len = 0;
      float steps = 4;
      for (int i = 0; i < steps; i++) {
        len += Vector3.Distance(GetPoint(i / steps), GetPoint((i + 1) / steps));
      }
      return len;
    }

    //returns the new curve
    public Curve3D Subdivide(float perc = 0.5f) {
      Vector3 p = GetPoint(perc);
      Vector3[] v1;
      Vector3[] v2;
      Subdivide2(this, perc, out v1, out v2);
      p0 = v1[0]; h0 = v1[1]; h1 = v1[2]; p1 = v1[3];
      return new Curve3D(v2[0], v2[1], v2[2], v2[3]);
    }

    public static void Subdivide2(Curve3D c, float t, out Vector3[] firstPart, out Vector3[] secondPart) {
      var b0 = Vector3.Lerp(c.p0, c.h0, t); // Same as evaluating a Bezier
      var b1 = Vector3.Lerp(c.h0, c.h1, t);
      var b2 = Vector3.Lerp(c.h1, c.p1, t);

      var c0 = Vector3.Lerp(b0, b1, t);
      var c1 = Vector3.Lerp(b1, b2, t);

      var d0 = Vector3.Lerp(c0, c1, t); // This would be the interpolated point

      firstPart = new Vector3[] { c.p0, b0, c0, d0 }; // first point of each step
      secondPart = new Vector3[] { d0, c1, b2, c.p1 }; // last point of each step
    }

    public Curve3D Copy() => new Curve3D(p0, h0, h1, p1);
    public Curve3D Transform(Transform t) => t == null ? Copy() : new Curve3D(
      t.TransformPoint(p0),
      t.TransformPoint(h0),
      t.TransformPoint(h1),
      t.TransformPoint(p1));

    public static implicit operator Vector3[](Curve3D c) => new Vector3[4] { c.p0, c.h0, c.h1, c.p1 };
    public static implicit operator Curve(Curve3D c) => new Curve(c.p0, c.h0, c.h1, c.p1);
    public static implicit operator Curve3DGPU(Curve3D c) => new Curve3DGPU(c.p0, c.h0, c.h1, c.p1);

    public void SpreadHandlesEvenly() {
      h0 = (p1 - p0) * 0.33f + p0;
      h1 = (p1 - p0) * 0.67f + p0;
    }

    public void Rotate(float x, float y, float z, Vector3 pivot) {
      p0 = p0.Rotate(x, y, z, pivot);
      h0 = h0.Rotate(x, y, z, pivot);
      h1 = h1.Rotate(x, y, z, pivot);
      p1 = p1.Rotate(x, y, z, pivot);
    }

    public void Rotate(Quaternion quat, Vector3 pivot) {
      p0 = p0.Rotate(quat, pivot);
      h0 = h0.Rotate(quat, pivot);
      h1 = h1.Rotate(quat, pivot);
      p1 = p1.Rotate(quat, pivot);
    }

    public float GetSpan() => Vector3.Distance(p0, p1);

    static bool sample = false;
    public float FindPointAlong(Vector3 v) {
      // sample = v.x.SoftEquals(0f) && v.y.SoftEquals(0.18f);
      if (sample)
        // Debug.Log("Begin sample: " + v + " | v.x: " + v.x + " | p0.x: " + p0.x + " | p1.x: " + p1.x);
        Debug.Log("Begin sample: " + v + " | v.y: " + v.y + " | p0.y: " + p0.y + " | p1.y: " + p1.y);

      if (p0.x.SoftEqualsMany(p1.x, h0.x, h1.x)) return Mathf.Clamp01((v.y - p0.y) / (p1.y - p0.y));
      if (p0.y.SoftEqualsMany(p1.y, h0.y, h1.y)) return Mathf.Clamp01((v.x - p0.x) / (p1.x - p0.x));

      // float a = Mathf.Clamp01((v.x - p0.x) / (p1.x - p0.x));
      float a = Mathf.Clamp01((v.y - p0.y) / (p1.y - p0.y));
      float b = GetClosestPointToCubicBezier(v, this, 10, 2);
      if (sample) Debug.Log(a + " | " + b + " [ v: " + v + ", p0: " + p0 + ", p1: " + p1 + "]");
      return b;
    }

    public Vector3 GetPoint(float t) {
      if (t <= 0.0f) return p0;
      if (t >= 1.0f) return p1;

      return Bezier.GetPoint(p0, h0, h1, p1, t);
    }

    public Vector3[] GetPolyPoints(int lineSteps) =>
      Enumerable.Range(0, lineSteps + 1).ToArray().Select<int, Vector3>(i => GetPoint((float)i / (float)lineSteps)).ToArray();

    public static float GetClosestPointToCubicBezier(Vector2 p, Curve3D curve, int slices = 10, int iterations = 2) {
      return (float)_GetClosestPointToCubicBezier(iterations, p, curve, 0f, 1f, slices);
    }

    private static double _GetClosestPointToCubicBezier(int iterations, Vector2 p, Curve3D curve, double start, double end, int slices) {
      if (iterations <= 0) return (start + end) / 2d;
      double tick = (end - start) / (double)slices;
      double x, y, dx, dy;
      double best = 0;
      double bestDistance = float.MaxValue;
      double currentDistance;
      double t = start;
      while (t <= end * 1.01f) {
        //B(t) = (1-t)**3 p0 + 3(1 - t)**2 t P1 + 3(1-t)t**2 P2 + t**3 P3
        x = (1f - t) * (1f - t) * (1f - t) * curve.p0.x + 3f * (1f - t) * (1f - t) * t * curve.h0.x + 3f * (1f - t) * t * t * curve.h1.x + t * t * t * curve.p1.x;
        y = (1f - t) * (1f - t) * (1f - t) * curve.p0.y + 3f * (1f - t) * (1f - t) * t * curve.h0.y + 3f * (1f - t) * t * t * curve.h1.y + t * t * t * curve.p1.y;

        dx = x - p.x;
        dy = y - p.y;
        dx *= dx;
        dy *= dy;
        currentDistance = dx + dy;
        if (currentDistance < bestDistance) {
          bestDistance = currentDistance;
          best = t;
        }
        if (sample) {
          // Debug.Log("x: " + x + " | p.x: " + p.x + " | dx: " + (x - p.x) + " | t: " + t + " | tick: " + tick + " | best: " + best);
          Debug.Log("y: " + y + " | p.y: " + p.y + " | dy: " + (y - p.y) + " | t: " + t + " | tick: " + tick + " | best: " + best);
        }
        t += tick;
      }
      return _GetClosestPointToCubicBezier(iterations - 1, p, curve, Math.Max(best - tick, 0f), Math.Min(best + tick, 1f), slices);
    }

    public Curve3D TransformCurve(Curve3D c, Vector3 pos, Quaternion rot, float scale) {
      Curve3D newCurve = new Curve3D(c.p0 * scale + pos,
        c.h0 * scale + pos,
        c.h1 * scale + pos,
        c.p1 * scale + pos);
      newCurve.Rotate(rot, pos);
      return newCurve;
    }
  }
}
