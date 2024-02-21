using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace BionicWombat {
#if UNITY_EDITOR
  [ExecuteInEditMode]
#endif
  [Serializable]
  public class LeafStem : IStem {
    public List<Curve3D> curves { get; set; }
    [JsonIgnore] private List<Curve3D> curvesWithoutExtension;
    [JsonIgnore] public Vector3[] shape { get; set; }

    public void CreateCurves(LeafParamDict fields, ArrangementData arrData, FlowerPotController potController) {
      shape = CreateShape(fields, arrData.scale);
      curves = new List<Curve3D>();

      float flopPerc = GetFlopPerc(fields, arrData);
      float lenAdj = 0.25f;
      float len = arrData.stemLengthMult * (fields[LPK.StemLength].value + arrData.stemLengthAdd) /
        (1f + (lenAdj * flopPerc * flopPerc)); //from 1f to 1f + lenAdj -- this formula is bs
      Polar flop = new Polar(len, -(flopPerc * 90f) + 90, true);

      Curve3D main = new Curve3D(Vector3.zero, flop.vec);
      Vector2 h0s = new Vector2(0f, len * 0.25f);
      Vector2 h0e = new Vector2(len * 0.25f, len * 0.5f);
      Vector2 h1s = new Vector2(0f, len * 0.75f);
      Vector2 h1e = new Vector2(len * 0.75f, len * 0.5f);
      main.h0 = (h0e - h0s) * flopPerc + h0s;
      main.h1 = (h1e - h1s) * flopPerc + h1s;

      Curve neck = main.Subdivide(0.9f);
      float neckLen = neck.FastLength();
      float angle = neck.angleFull;
      neck.p1 = neck.p0.AddPolar(new Polar(neckLen, -fields[LPK.StemNeck].value + angle, true));

      float h1Len = Vector2.Distance(neck.p0, neck.h1);
      float p0ToH1Angle = Curve.Angle(neck.p0, neck.h1);
      float neckPerc = fields[LPK.StemNeck].value / 90f;
      neck.h1 = neck.p0.AddPolar(new Polar(h1Len * (1f + neckPerc * 0.3f),
        -fields[LPK.StemNeck].value / 2f + p0ToH1Angle,
        true));

      curves.Add(main);
      curves.Add((Curve3D)neck);
    }

    public static float GetFlopPerc(LeafParamDict fields, ArrangementData arrData) {
      float flopVal = fields[LPK.StemFlop].value;
      float flopDiff = arrData.stemFlopMult > 1 ? flopVal : (90f - flopVal);
      flopVal += flopDiff * (1f - arrData.stemFlopMult);
      flopVal += arrData.stemFlopAdd;

      return flopVal / 90f;
    }

    public void AddBaseExtension(Vector3 vec, float yRotation) {
      if (vec.IsDefault()) return;
      curvesWithoutExtension = curves.ToList();
      Vector3 finalPoint = -vec;
      finalPoint = finalPoint.Rotate(0, -yRotation, 0, Vector3.zero);
      Curve3D mainCurve = curves[0].Copy();
      Vector3 mainHalfwayPoint = mainCurve.GetPoint(0.5f);
      Vector3 mainHalfwayPointMore = mainCurve.GetPoint(0.52f);
      Vector3 mainHalfwayPointLess = mainCurve.GetPoint(0.48f);
      Vector3 mainTangent = mainHalfwayPointMore - mainHalfwayPointLess;
      mainTangent *= 5;
      mainCurve.p0 = mainHalfwayPoint;
      mainCurve.h0 = mainCurve.p0 + mainTangent;
      curves[0] = mainCurve;

      Curve3D baseExtension = new Curve3D(finalPoint, mainHalfwayPoint);
      baseExtension.h1 = mainCurve.p0 - mainTangent;
      Vector3 baseRisePoint = baseExtension.GetPoint(0.1f);
      baseExtension.h0 = baseRisePoint.AddY(1.5f);
      curves.Insert(0, baseExtension);
    }

    public void ClearBaseExtension() {
      if (curvesWithoutExtension.HasLength())
        curves = curvesWithoutExtension.ToList();
    }

    private static Vector3[] CreateShape(LeafParamDict fields, float scale) {
      float s = 0.25f * fields[LPK.StemWidth].value * scale;
      int sides = 6;
      return Enumerable.Range(0, sides).ToArray().Select<int, Vector3>(i => {
        return new Polar3(s, 0, (float)i / (float)sides * Polar.Pi2 + Polar.Pi).Vector;
      }).ToArray();
    }

    public static float Width(LeafParamDict fields) => 0.25f * fields[LPK.StemWidth].value;

    public float Length() => curves.Sum(c3d => c3d.FastLength());

    public float ShapeScaleAtPercent(float perc) {
      if (perc <= 0.95f) return 1f;
      float ret = 1f - ((perc - 0.95f) * 20f); //1f - 0f from (0.95 -> 1)
      float floor = 0.25f;
      ret = ret * (1f - floor) + floor; //1f - floor
      ret = 1 - (1 - ret) * (1 - ret); //EaseOutQuad
      return ret;
    }

    public bool IsEmpty() => curves.Count == 0 || shape.Length == 0;
    public bool IsTrunk() => false;
  }
}
