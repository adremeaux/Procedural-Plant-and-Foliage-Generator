using System;
using UnityEngine;

namespace BionicWombat {
#if UNITY_EDITOR
  [ExecuteInEditMode]
#endif
  [Serializable]
  public static class Arrangement {
    private static float PotScaleFudge = 6.66f;
    public static float PotScaleBase = 2f * PotScaleFudge;

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
        if (count == 1) fullPerc = 1f;
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
        float stemYAngle = mostPerc * angleMax + symAngleAdd + rand + angleStart;
        Vector2 xz = new Vector2();
        float chordLen = LeafStem.Width(fields) / 2f;
        float rad = PlantTrunk.Width(fields);
        float dist = Mathf.Sqrt(rad * rad - chordLen * chordLen);
        xz = xz.AddPolar(new Polar(dist, -stemYAngle, true));

        Vector3 trunkPoint = trunk.GetPointFromY(yPos);
        xz = new Vector2(trunkPoint.x, trunkPoint.z);

        float extraFlop = fields[LPK.StemFlopLower].value * (1f - fullPerc) * 45f;
        float maxSkew = fields[LPK.LeafSkewMax].value;
        float leafZAngle = BWRandom.Range(-maxSkew, maxSkew);

        arr[i] = new ArrangementData(
          new Vector3(xz.x, yPos, xz.y), //pos
          Quaternion.Euler(0, stemYAngle, 0), //stemRotation
          leafZAngle,
          scale,
          count <= 1 ? 0 : fields[LPK.StemLengthIncrease].value * (i / ((float)count - 1f)), //stemLengthAdd
          1f + BWRandom.RangeAdd(fields[LPK.StemLengthRand].value * 0.3f), //stemLengthMult
          1f + BWRandom.RangeAdd(fields[LPK.StemFlopRand].value), //stemFlopMult
          extraFlop, //stemFlopAdd
          potScale
        );
        yPos += fields[LPK.NodeDistance].value;
      }
      return arr;
    }

    public static float GetTopStemPos(LeafParamDict fields, FlowerPotController potController = null) =>
      (fields[LPK.NodeDistance].value * (GetLeafCount(fields) - 1f)) +
        fields[LPK.NodeInitialY].value + (potController != null ? GetPotYAdd(fields, potController) : 0f);

    public static float GetFinalStemLength(LeafParamDict fields) => fields[LPK.StemLengthIncrease].value + fields[LPK.StemLength].value;
    public static int GetLeafCount(LeafParamDict fields) => Mathf.RoundToInt(fields[LPK.LeafCount].value);

    public static float GetPotScale(LeafParamDict fields) => PotScaleBase * fields[LPK.PotScale].value;
    public static float GetPotYAdd(LeafParamDict fields, FlowerPotController potController) =>
      (potController.GetCurrentYPos() * (GetPotScale(fields) / PotScaleFudge));
  }

  [Serializable]
  public struct ArrangementData {
    public Vector3 pos;
    public Quaternion stemRotation;
    public float leafZAngle;
    public float scale;
    public float stemLengthAdd;
    public float stemLengthMult;
    public float stemFlopMult;
    public float stemFlopAdd;
    public float potScale;

    public ArrangementData(Vector3 pos, Quaternion stemRotation, float leafZAngle, float scale,
        float stemLengthAdd, float stemLengthMult, float stemFlopMult, float stemFlopAdd, float potScale) {
      this.pos = pos;
      this.stemRotation = stemRotation;
      this.leafZAngle = leafZAngle;
      this.scale = scale;
      this.stemLengthAdd = stemLengthAdd;
      this.stemLengthMult = stemLengthMult;
      this.stemFlopMult = stemFlopMult;
      this.stemFlopAdd = stemFlopAdd;
      this.potScale = potScale;
    }

    public static ArrangementData Zero => new ArrangementData(Vector3.zero, Quaternion.identity, 0f, 1f, 0f, 1f, 1f, 0f, Arrangement.PotScaleBase);
    public override string ToString() {
      return "pos: " + pos + " | stemRotation: " + stemRotation + " | scale: " + scale + " | stemLengthAdd: " + stemLengthAdd + " | stemLengthMult: " + stemLengthMult
        + " | stemFlopMult: " + stemFlopMult + " | stemFlopAdd: " + stemFlopAdd + " | potScale: " + potScale;
    }

    public ArrangementData Copy() => new ArrangementData(
      pos, stemRotation, leafZAngle, scale, stemLengthAdd, stemLengthMult,
      stemFlopMult, stemFlopAdd, potScale);
  }
}
