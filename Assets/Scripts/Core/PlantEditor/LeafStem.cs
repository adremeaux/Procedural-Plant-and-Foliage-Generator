using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
[Serializable]
public class LeafStem : IStem {
  public List<Curve3D> curves { get; set; }
  public Vector3[] shape { get; set; }

  public void CreateCurves(LeafParamDict fields, LeafDeps deps, ArrangementData arrData, FlowerPotController potController) {
    shape = CreateShape(fields, deps, arrData.scale);
    curves = new List<Curve3D>();

    float flopVal = fields[LPK.StemFlop].value;
    float flopDiff = arrData.stemFlopMult > 1 ? flopVal : (90f - flopVal);
    flopVal += flopDiff * (1f - arrData.stemFlopMult);
    flopVal += arrData.stemFlopAdd;

    float flopPerc = flopVal / 90f;
    float lenAdj = 0.25f;
    float len = arrData.stemLengthMult * (fields[LPK.StemLength].value + arrData.stemLengthAdd) /
      (1f + (lenAdj * flopPerc * flopPerc)); //from 1f to 1f + lenAdj -- this formula is bs
    Polar flop = new Polar(len, -flopVal + 90, true);

    Curve main = new Curve(Vector3.zero, flop.vec);
    Vector2 h0s = new Vector2(0f, len * 0.25f);
    Vector2 h0e = new Vector2(len * 0.25f, len * 0.5f);
    Vector2 h1s = new Vector2(0f, len * 0.75f);
    Vector2 h1e = new Vector2(len * 0.75f, len * 0.5f);
    main.h0 = (h0e - h0s) * flopPerc + h0s;
    main.h1 = (h1e - h1s) * flopPerc + h1s;

    Curve neck = main.Subdivide(0.9f);
    float neckLen = neck.Length();
    float angle = neck.angleFull;
    neck.p1 = neck.p0.AddPolar(new Polar(neckLen, -fields[LPK.StemNeck].value + angle, true));

    float h1Len = Vector2.Distance(neck.p0, neck.h1);
    float p0ToH1Angle = Curve.Angle(neck.p0, neck.h1);
    float neckPerc = fields[LPK.StemNeck].value / 90f;
    neck.h1 = neck.p0.AddPolar(new Polar(h1Len * (1f + neckPerc * 0.3f),
      -fields[LPK.StemNeck].value / 2f + p0ToH1Angle,
      true));

    curves.Add(main);
    curves.Add(neck);
  }

  private static Vector3[] CreateShape(LeafParamDict fields, LeafDeps deps, float scale) {
    float s = 0.25f * fields[LPK.StemWidth].value * scale;
    int sides = 8;
    return Enumerable.Range(0, sides).ToArray().Select<int, Vector3>(i => {
      return new Polar3(s, 0, (float)i / (float)sides * Polar.Pi2 + Polar.Pi).Vector;
    }).ToArray();
  }

  public static float Width(LeafParamDict fields) => 0.25f * fields[LPK.StemWidth].value;

  public float ShapeScaleAtPercent(float perc) => perc <= 0.01f ? 1f : 1f;

  public bool IsEmpty() => curves.Count == 0 || shape.Length == 0;
}
}