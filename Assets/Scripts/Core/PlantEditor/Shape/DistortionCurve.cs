using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
  [Flags]
  public enum Axis : byte {
    x = 1,
    y = 2,
    z = 4,
    All = 7
  }

  public struct DistCurveConfig {
    public Axis affectAxes;
    public float maxFadeDist;
    public bool reverseFade;
    public bool skipOutsideLowerBound;
    public LeafDistortionType type;
    public bool useDistFade => maxFadeDist == float.MaxValue;

    public DistCurveConfig(Axis affectAxes, float maxFadeDist, bool reverseFade, bool skipOutsideLowerBound, LeafDistortionType type) {
      this.affectAxes = affectAxes;
      this.maxFadeDist = maxFadeDist;
      this.reverseFade = reverseFade;
      this.skipOutsideLowerBound = skipOutsideLowerBound;
      this.type = type;
    }
  }

  [Serializable]
  public class DistortionCurve {
    [NonSerialized]
    public static float accuracy = 100f;
    public Curve3D[] influenceCurves;
    public Vector3[] influencePoints { get; private set; }
    public Vector3[] distortionPoints { get; private set; }
    public bool shouldFade = true;
    public DistCurveConfig config;

    public override string ToString() {
      return $"DistortionCurve[{config.type}]: {influenceCurves.ToLog()}";
    }

    public DistortionCurve(Curve3D influenceCurve, Curve3D distortionCurve, DistCurveConfig config) :
      this(new Curve3D[] { influenceCurve }, CreatePointsFromCurve(distortionCurve), config) { }

    public DistortionCurve(Curve3D influenceCurve, Vector3[] distortionPoints, DistCurveConfig config) :
      this(new Curve3D[] { influenceCurve }, distortionPoints, config) { }

    public DistortionCurve(Curve3D[] influenceCurves, Vector3[] distortionPoints, DistCurveConfig config) {
      this.influenceCurves = influenceCurves;
      influencePoints = CreatePointsFromCurves(influenceCurves);
      this.distortionPoints = distortionPoints;
      this.config = config;
      shouldFade = config.maxFadeDist != float.MinValue;
    }

    public DistortionCurve Copy() => new DistortionCurve(influenceCurves, distortionPoints, config);

    public DistortionCurve Transform(Transform t) {
      if (t == null) return Copy();
      DistortionCurve d = new DistortionCurve(influenceCurves, distortionPoints, config);
      d.influenceCurves = d.influenceCurves.Select(c3D => c3D.Transform(t)).ToArray();
      d.influencePoints = d.influencePoints.Select(v => t.TransformPoint(v)).ToArray();
      d.distortionPoints = d.distortionPoints.Select(v => t.TransformPoint(v)).ToArray();
      return d;
    }

    public (float raw, float clamped) FindPointAlong(Vector3 v) {
      int idx = 0;
      float min = float.MaxValue;
      float minP = 0f;
      foreach (Curve3D ic in influenceCurves) {
        float p = ic.FindPointAlong(v);
        float dist = Vector3.Distance(v, ic.GetPoint(p));
        if (dist < min) {
          min = dist;
          minP = (float)idx + p;
        }
        idx++;
        // Debug.Log("FindPointAlong dist: " + dist + " | p: " + p + " | minP: " + minP);
      }
      return (minP, minP / influenceCurves.Length);
    }

    public Vector3 GetMagnetPoint(float pointAlong) {
      float perc = pointAlong / (float)influenceCurves.Length;
      return distortionPoints[(int)((float)(distortionPoints.Length - 1f) * perc)];
    }

    public float DistanceFromPointAlongInfluence(Vector3 v, float point) {
      return Vector3.Distance(NearestPointAlongInfluence(point), v);
    }

    public Vector3 NearestPointAlongInfluence(float point) {
      float normalizedPoint = point;//point * (float)influenceCurves.Length;
      int idx = (int)normalizedPoint;
      if (idx >= influenceCurves.Length) {
        // Debug.Log("NearestPointAlongInfluence idx too large: " + idx + " from " + point + " | normalizedPoint: " + normalizedPoint);
        idx = influenceCurves.Length - 1;
      }
      float finalPoint = normalizedPoint - (float)idx;
      return influenceCurves[idx].GetPoint(finalPoint);
    }

    public static Vector3[] CreatePointsFromCurves(Curve3D[] curves) {
      List<Vector3> l = new List<Vector3>();
      foreach (Curve3D c in curves)
        l.AddRange(CreatePointsFromCurve(c));
      return l.ToArray();
    }

    public static Vector3[] CreatePointsFromCurve(Curve3D c) {
      Vector3[] points = new Vector3[(int)accuracy];
      float slice = 1f / (accuracy - 1f);
      int idx = 0;
      for (float i = 0; i <= 1f + slice * 0.99f; i += slice) {
        points[idx++] = c.GetPoint(i);
      }
      return points;
    }
  }

}
