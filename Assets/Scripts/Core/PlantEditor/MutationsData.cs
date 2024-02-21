using System;
using System.Collections.Generic;
using System.Linq;
using BionicWombat;
using UnityEngine;

namespace BionicWombat.Hybridize {
  public struct MutationSetup {
    public int maxScore;
    public LPImportance maxImportance;
    public Stability maxInstability;
    public int maxAffected;
    public Dictionary<LPImportance, int> forcedImportance;
    public MutationSetup(int maxScore, LPImportance maxImportance, Stability maxInstability, int maxAffected, Dictionary<LPImportance, int> forcedImportance) {
      this.maxScore = maxScore;
      this.maxImportance = maxImportance;
      this.maxInstability = maxInstability;
      this.maxAffected = maxAffected;
      this.forcedImportance = forcedImportance;
      if (this.forcedImportance == null) this.forcedImportance = new Dictionary<LPImportance, int>();
    }
    public override string ToString() {
      return "[MutationSetup] maxScore: " + maxScore + " | maxImportance: " + maxImportance + " | maxInstability: " + maxInstability + " | maxAffected: " + maxAffected + " | forcedImportance: " + forcedImportance;
    }
    public static MutationSetup Default => new MutationSetup(0, 0, 0, 0, null);
  }

  public struct MutationTarget {
    public LPK lpk;
    public Stability stability;
    public HSLComponent hslComponent;
    public MutationTarget(LPK lpk, Stability stability, HSLComponent hslComponent) {
      this.lpk = lpk;
      this.stability = stability;
      this.hslComponent = hslComponent;
    }
    public override string ToString() {
      return "[MutationTarget] lpk: " + lpk + " | stability: " + stability + " | hslComponent: " + hslComponent;
    }
  }

  public struct MutationsData { //need to include Strength
    public LPCategory[] categories;
    public List<MutationTarget> specifics;
    public MutationsData SetCategories(params LPCategory[] cats) {
      if (cats == null) categories = new LPCategory[0];
      else categories = cats;
      return this;
    }
    public MutationsData SetSpecifics(params MutationTarget[] specs) {
      if (specs == null) specs = new MutationTarget[0];
      specifics = specs.ToList();
      return this;
    }
    public MutationsData AddSpecifics(params MutationTarget[] specs) {
      if (specs == null) specifics = new List<MutationTarget>();
      specifics.Add(specs);
      return this;
    }
    public static MutationsData Empty => new MutationsData().SetCategories().SetSpecifics();
    public override string ToString() {
      return "[MutationsData] " + specifics.ToLog();
    }

    public static MutationsData GetMutationsData(MutationSetup setup) {

      MutationsData mutations = MutationsData.Empty;
      int lpkCount = Enum.GetValues(typeof(LPK)).Length;
      int totalScore = 0;
      Dictionary<LPImportance, int> forced = setup.forcedImportance.Copy();

      while (totalScore < setup.maxScore) {
        LPK lpk = 0;
        Stability stability = Stability.Stable;

        bool isForced = forced.Count > 0;
        if (isForced) {
          LPImportance maxImp = forced.Keys.Last();
          lpk = LeafParamHelpers.ParamsWithImportance(maxImp).RandomObj();
          forced[maxImp]--;
          if (forced[maxImp] <= 0) forced.Remove(maxImp);
        } else {
          lpk = BWRandom.RandomEnum<LPK>();
        }
        LeafParam param = LeafParamDefaults.Defaults[lpk];
        if (!isForced && param.importance > setup.maxImportance || param.importance == LPImportance.Disable) continue;

        //pick a random stability
        do {
          stability = BWRandom.RandomEnum<Stability>();
        } while (stability > setup.maxInstability || stability <= Stability.Stable);

        LPImportance importance = param.importance;
        HSLComponent hslComponent = HSLComponent.All;
        if (param.mode == LPMode.ColorHSL) {
          int comp = BWRandom.UnseededInt(0, 3);
          if (comp != (int)HSLComponent.Hue) importance--;
          hslComponent = (HSLComponent)comp;
        }

        int score = GetScore(importance, stability);
        totalScore += score;

        mutations.AddSpecifics(new MutationTarget(lpk, stability, hslComponent));
        string hslPrint = hslComponent != HSLComponent.All ? "-" + hslComponent : "";
        Debug.Log($"    Changing {lpk}{hslPrint} with stability {stability}, importance {param.importance}, score: {score}");
      }
      return mutations;
    }

    private static int GetScore(LPImportance importance, Stability stability) {
      float impScore = 0;
      float stabMult = 1f;
      switch (importance) {
        case LPImportance.Disable: return -1;
        case LPImportance.Low: impScore = 3; break;
        case LPImportance.Medium: impScore = 8; break;
        case LPImportance.High: impScore = 15; break;
      }
      switch (stability) {
        case Stability.Stable:
        case Stability.SlightlyInstable: stabMult = 1f; break;
        case Stability.Instable: stabMult = 1.5f; break;
        case Stability.VeryInstable: stabMult = 2.25f; break;
        case Stability.MostInstable: stabMult = 3f; break;
      }
      return Mathf.FloorToInt(impScore * stabMult);
    }
  }
}
