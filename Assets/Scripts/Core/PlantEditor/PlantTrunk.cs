using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
#if UNITY_EDITOR
  [ExecuteInEditMode]
#endif
  [Serializable]
  public class PlantTrunk : IStem {
    public List<Curve3D> curves { get; set; }
    public Vector3[] shape { get; set; }
    private float taperStartPerc;
    public List<Vector3> linearPoints;

    public void CreateCurves(LeafParamDict fields, ArrangementData arr, FlowerPotController potController) {
      curves = new List<Curve3D>();
      shape = CreateShape(fields);

      float topStemPos = Arrangement.GetTopStemPos(fields, potController);
      float taperDist = fields[LPK.NodeDistance].value;
      taperStartPerc = topStemPos / (topStemPos + taperDist);
      Curve3D main = new Curve3D(Vector3.zero, new Vector3(0, topStemPos + taperDist, 0));
      main.SpreadHandlesEvenly();
      if (topStemPos + taperDist <= 0f) return;

      float wobble = fields[LPK.TrunkWobble].value;
      float maxVertWobble = 15f;
      float maxDisplace = (topStemPos / 3f);
      main.p1 = main.p1.AddPolar(new Polar3(fields[LPK.TrunkLean].value * maxDisplace, 0f, 180f, true));
      main.h0 = main.h0.AddPolar(new Polar3(BWRandom.Range(0f, maxDisplace / 3f) * wobble,
        BWRandom.RangeAdd(maxVertWobble), BWRandom.RangeAdd(180f), true));
      main.h1 = main.h1.AddPolar(new Polar3(BWRandom.Range(0f, maxDisplace / 2f) * wobble,
        BWRandom.RangeAdd(maxVertWobble), BWRandom.RangeAdd(180f), true));

      curves.Add(main);
      linearPoints = LeafVeins.CalcLinearPoints(curves);
    }

    private static Vector3[] CreateShape(LeafParamDict fields) {
      float s = Width(fields);
      int sides = 16;
      return Enumerable.Range(0, sides).ToArray().Select<int, Vector3>(i => {
        return new Polar3(s, 0, (float)i / (float)sides * Polar.Pi2 + Polar.Pi).Vector;
      }).ToArray();
    }

    public static float Width(LeafParamDict fields) => 0.25f * fields[LPK.TrunkWidth].value;

    public float ShapeScaleAtPercent(float perc) {
      if (perc <= taperStartPerc) return 1f;
      if (perc >= 0.99f) return 0f;
      float newPerc = (perc - taperStartPerc) / (1.0f - taperStartPerc);
      newPerc *= newPerc;
      return (1f - newPerc);
    }

    public Vector3 GetPointFromY(float yPos) => linearPoints == null ? Vector3.zero :
      LeafVeins.FindPointOnEdgeWithY(yPos, linearPoints, true);

    public bool IsEmpty() => curves.Count == 0 || shape.Length == 0;
    public bool IsTrunk() => true;
  }
}
