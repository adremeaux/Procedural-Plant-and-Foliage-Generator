using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BionicWombat {
#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
[Serializable]
public static class Arrangement {
  public static float PotScaleBase = 2f;
  //Reseed BWRandom before calling
  public static ArrangementData[] Arrange(LeafParamDict fields, int count, PlantTrunk trunk, FlowerPotController potController) {
    ArrangementData[] arr = new ArrangementData[count];
    float angleMax = 360f * (1f - fields[LPK.RotationClustering].value);
    float angleStart = 90f - (angleMax / 2f);
    float potScale = GetPotScale(fields);
    float potYAdd = GetPotYAdd(fields, potController);
    float yPos = fields[LPK.NodeInitialY].value + potYAdd;
    float randRotationBase = 90f;
    float sym = Mathf.RoundToInt(fields[LPK.RotationalSymmetry].value);
    if (sym == 0) sym = 1;
    for (int i = 0; i < count; i++) {
      float fullPerc = (float)i / (count - 1f);
      float mostPerc = (float)i / count;
      float randRange = randRotationBase * fields[LPK.RotationRand].value;
      float rand = BWRandom.RangeAdd(randRange);
      float scale = count == 1 ? 1f :
        (1f - fields[LPK.ScaleMin].value) * fullPerc + fields[LPK.ScaleMin].value;
      scale *= fields[LPK.LeafScale].value;
      scale += BWRandom.RangeAdd(0.25f * fields[LPK.ScaleRand].value);

      float symAngleAdd = (360f / sym) * (i % sym);
      if (sym == 2) symAngleAdd += 90f;
      if (sym == 3) symAngleAdd += 180f;
      float yAngle = mostPerc * angleMax + symAngleAdd + rand + angleStart;
      Vector2 xz = new Vector2();
      float chordLen = LeafStem.Width(fields) / 2f;
      float rad = PlantTrunk.Width(fields);
      float dist = Mathf.Sqrt(rad * rad - chordLen * chordLen);
      xz = xz.AddPolar(new Polar(dist, -yAngle, true));

      Vector3 trunkPoint = trunk.GetPointFromY(yPos);
      xz = new Vector2(trunkPoint.x, trunkPoint.z);

      float extraFlop = fields[LPK.StemFlopLower].value * (1f - fullPerc) * 45f;

      arr[i] = new ArrangementData(
        new Vector3(xz.x, yPos, xz.y),
        Quaternion.Euler(0, yAngle, 0),
        scale,
        fields[LPK.StemLengthIncrease].value * i,
        1f + BWRandom.RangeAdd(fields[LPK.StemLengthRand].value * 0.5f), //stemLengthMult
        1f + BWRandom.RangeAdd(fields[LPK.StemFlopRand].value), //stemFlopMult
        extraFlop, //stemFlopAdd
        potScale
      );
      yPos += fields[LPK.NodeDistance].value;
    }
    return arr;
  }

  public static float GetTopStemPos(LeafParamDict fields, FlowerPotController potController) =>
    (fields[LPK.NodeDistance].value * (GetLeafCount(fields) - 1f)) +
      fields[LPK.NodeInitialY].value + GetPotYAdd(fields, potController);

  public static int GetLeafCount(LeafParamDict fields) => Mathf.RoundToInt(fields[LPK.LeafCount].value);

  public static float GetPotScale(LeafParamDict fields) => PotScaleBase * fields[LPK.PotScale].value;
  public static float GetPotYAdd(LeafParamDict fields, FlowerPotController potController) =>
    (potController.GetCurrentYPos() * GetPotScale(fields));
}

public struct ArrangementData {
  public Vector3 pos;
  public Quaternion rotation;
  public float scale;
  public float stemLengthAdd;
  public float stemLengthMult;
  public float stemFlopMult;
  public float stemFlopAdd;
  public float potScale;

  public ArrangementData(Vector3 pos, Quaternion rotation, float scale,
      float stemLengthAdd, float stemLengthMult, float stemFlopMult, float stemFlopAdd, float potScale) {
    this.pos = pos;
    this.rotation = rotation;
    this.scale = scale;
    this.stemLengthAdd = stemLengthAdd;
    this.stemLengthMult = stemLengthMult;
    this.stemFlopMult = stemFlopMult;
    this.stemFlopAdd = stemFlopAdd;
    this.potScale = potScale;
  }

  public static ArrangementData Zero => new ArrangementData(Vector3.zero, Quaternion.identity, 1f, 0f, 1f, 1f, 0f, Arrangement.PotScaleBase);
}
}