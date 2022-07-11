using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BionicWombat {
public enum LeafDistortionType {
  Curl,
  Cup,
  Wave,
  Flop
}

public class LeafDistortion {
  LeafParamDict fields;
  List<LeafCurve> leafCurves;
  LeafVeins veins;
  LeafDeps deps;

  public LeafDistortion(LeafParamDict fields, List<LeafCurve> leafCurves, LeafVeins veins, LeafDeps deps) {
    this.fields = fields;
    this.leafCurves = leafCurves;
    this.veins = veins;
    this.deps = deps;
  }

  public List<DistortionCurve> GetDistortionSplines() {
    LeafBoundsData boundsData = LeafRenderer.GetBoundsData(LeafCurve.ToCurves(leafCurves), deps.baseParams.RenderLineSteps);

    List<DistortionCurve> list = new List<DistortionCurve>();
    (bool shouldCurl, bool shouldCup, bool shouldFlop, bool shouldWave) =
      (fields[LPK.DistortCurl].enabled, fields[LPK.DistortCup].enabled,
       fields[LPK.DistortFlop].enabled, fields[LPK.DistortWave].enabled);

    //curl
    float curlPointVal = fields[LPK.DistortCurlPoint].value;
    float curlDegrees = fields[LPK.DistortCurl].value;
    float curlPoint = curlPointVal * (boundsData.width / 2f);
    if (shouldCurl && !curlDegrees.SoftEquals(0f)) {
      Curve3D horizSpanRight = new Curve3D(boundsData.leftApex.WithX(curlPoint), boundsData.rightApex);
      Curve3D horizSpanLeft = new Curve3D(boundsData.leftApex.WithX(-curlPoint), boundsData.leftApex);
      horizSpanRight.SpreadHandlesEvenly();
      horizSpanLeft.SpreadHandlesEvenly();
      Curve3D curlRight = Arc(horizSpanRight, curlDegrees, false);
      Curve3D curlLeft = Arc(horizSpanLeft, curlDegrees, true);
      curlRight.Rotate(-90, 0, 0, curlRight.p0);
      curlLeft.Rotate(-90, 0, 0, curlLeft.p0);
      list.Add(new DistortionCurve(deps, horizSpanRight, curlRight,
        new DistCurveConfig(Axis.x | Axis.z, float.MinValue, false, false, LeafDistortionType.Curl)));
      list.Add(new DistortionCurve(deps, horizSpanLeft, curlLeft,
        new DistCurveConfig(Axis.x | Axis.z, float.MinValue, false, false, LeafDistortionType.Curl)));
    }

    //cup
    if (shouldCup) {
      Curve3D[] cupCurves = Array.ConvertAll<LeafCurve, Curve3D>(leafCurves.ToArray(), lc => new Curve3D(lc));
      Vector3[] cupPoints = GetCupPoints(cupCurves, fields[LPK.DistortCup].value);
      list.Add(new DistortionCurve(deps, cupCurves, cupPoints,
        new DistCurveConfig(Axis.z, float.MaxValue, true, false, LeafDistortionType.Cup)));
    }

    //flop
    if (shouldFlop && fields[LPK.DistortFlop].value > 0) {
      Curve3D flopCurve = veins.GetFullLengthMidrib();
      flopCurve = new Curve3D(flopCurve.GetPoint(fields[LPK.DistortFlopStart].value), flopCurve.p1);
      flopCurve.SpreadHandlesEvenly();
      flopCurve.Rotate(0, 0, 90, flopCurve.p0);
      Curve3D flopPoints = Arc(flopCurve, fields[LPK.DistortFlop].value, false);
      flopCurve.Rotate(0, 0, -90, flopCurve.p0);
      flopPoints.Rotate(0, 0, -90, flopPoints.p0);
      flopPoints.Rotate(0, -90, 0, flopPoints.p0);
      list.Add(new DistortionCurve(deps, flopCurve, flopPoints,
        new DistCurveConfig(Axis.y | Axis.z, float.MinValue, false, true, LeafDistortionType.Flop)));
    }

    //margin wave
    if (shouldWave) {
      LeafCurve c = LeafShape.GetCurve(LeafCurveType.LobeOuter, leafCurves, LeafShape.LeftyCheck.Right, true);
      LeafCurve c2 = LeafShape.GetCurve(LeafCurveType.LowerHalf, leafCurves, LeafShape.LeftyCheck.Right, true);
      Curve3D[] waveCurvesR = new Curve3D[] {
        new Curve3D(c),
        new Curve3D(c2),
      };
      Vector3[] wavePointsR = GetWavePoints(waveCurvesR, fields[LPK.DistortWave].value, fields[LPK.DistortWavePeriod].value);
      list.Add(new DistortionCurve(deps, waveCurvesR, wavePointsR,
        new DistCurveConfig(Axis.z, fields[LPK.DistortWaveDepth].value, false, false, LeafDistortionType.Wave)));

      LeafCurve l = LeafShape.GetCurve(LeafCurveType.LobeOuter, leafCurves, LeafShape.LeftyCheck.Left, true);
      LeafCurve l2 = LeafShape.GetCurve(LeafCurveType.LowerHalf, leafCurves, LeafShape.LeftyCheck.Left, true);
      Curve3D[] waveCurvesL = new Curve3D[] {
        new Curve3D(l2),
        new Curve3D(l),
      };
      Vector3[] wavePointsL = GetWavePoints(waveCurvesL, fields[LPK.DistortWave].value, fields[LPK.DistortWavePeriod].value);
      list.Add(new DistortionCurve(deps, waveCurvesL, wavePointsL,
        new DistCurveConfig(Axis.z, fields[LPK.DistortWaveDepth].value, false, false, LeafDistortionType.Wave)));
    }

    return list;
  }

  public static Curve3D Arc(Curve3D c, float degrees, bool reverse) {
    if (degrees.SoftEquals(0f)) return c;
    Vector2 p0 = c.p0;
    Vector2 p1 = c.p1;
    float width = p1.x - p0.x;
    float radius = width / (degrees * Polar.DegToRad);
    float mult = 1f;
    if (reverse) {
      radius *= -1f;
      mult = -1f;
    }
    Vector2 center = new Vector2(p0.x, p0.y + radius);
    Vector2 v0 = center.AddPolar(new Polar(radius, 270f, true));
    Vector2 v1 = center.AddPolar(new Polar(radius, 270f + degrees * mult, true));

    Curve3D nc = c.Copy();
    if (reverse) {
      (nc.h0, nc.h1) = Arc(p0, v1, center);
      nc.p1 = v1;
    } else {
      (nc.h0, nc.h1) = Arc(p0, v1, center);
      nc.p1 = v1;
    }

    return nc;
  }

  public static (Vector2 h1, Vector2 h2) Arc(Vector2 v1, Vector2 v2, Vector2 center) {
    float ax = v1.x - center.x;
    float ay = v1.y - center.y;
    float bx = v2.x - center.x;
    float by = v2.y - center.y;
    float q1 = ax * ax + ay * ay;
    float q2 = q1 + ax * bx + ay * by;
    float k2 = (4f / 3f) * (Mathf.Sqrt(2 * q1 * q2) - q2) / (ax * by - ay * bx);

    float x2 = center.x + ax - k2 * ay;
    float y2 = center.y + ay + k2 * ax;
    float x3 = center.x + bx + k2 * by;
    float y3 = center.y + by - k2 * bx;

    return (new Vector2(x2, y2), new Vector2(x3, y3));
  }

  public static Vector3[] GetCupPoints(Curve3D[] curves, float amp) {
    List<Vector3> points = new List<Vector3>();
    float sliceCount = DistortionCurve.accuracy;
    float slice;
    float shrink = 1f;//(1f - Mathf.Abs(amp)) * 0.2f + 0.8f;
    Vector3 mult = new Vector3(shrink, shrink, 1f);
    Vector3 add = new Vector3(0f, 0f, -amp);
    foreach (Curve3D c in curves) {
      sliceCount = c.GetSpan() >= 0.5f ? DistortionCurve.accuracy : 10f;
      slice = 1f / (sliceCount - 1f);
      for (float i = 0; i <= 1f + slice * 0.9f; i += slice) {
        points.Add(c.GetPoint(i).Mult(mult) + add);
      }
    }
    return points.ToArray();
  }

  // public static Vector3[] GetCurlPoints(Curve3D midrib, float degrees, float startPoint) {

  // }

  public static Vector3[] GetWavePoints(Curve3D[] curves, float amp, float waveCount) {
    float sliceCount = DistortionCurve.accuracy;
    float slice = 1f / (sliceCount - 1f);
    float repeats = waveCount;
    float span = repeats * Polar.Pi2;
    Vector3[] points = new Vector3[(int)sliceCount];
    int idx = 0;
    for (float i = 0; i <= 1f + slice * 0.9f; i += slice) {
      float dist = Mathf.Abs(i - 0.5f) * 2f; //1-0-1
      dist = 1f - (dist * dist); //0-1-0 quad
      float spreadPoint = (float)curves.Length * i;
      if (i >= 1f) spreadPoint = (int)spreadPoint - 0.0001f;
      int curveIdx = (int)Mathf.Min(spreadPoint, curves.Length - 1f);
      Vector3 p = curves[curveIdx].GetPoint(spreadPoint % 1f);
      p.z = Mathf.Sin(span * i) * dist * amp;
      // Debug.Log(idx + " | curveIdx: " + curveIdx + " | i: " + i + " | p: " + p);
      points[idx++] = p;
    }
    return points;
  }
}
}