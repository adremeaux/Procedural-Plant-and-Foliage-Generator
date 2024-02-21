using System;
using System.Collections.Generic;
using System.Linq;
using BionicWombat;
using UnityEngine;

namespace BionicWombat.Hybridize {
  public enum BiasIndex {
    None,
    Left,
    Right,
  }

  public enum Stability {
    Stable = 0,
    SlightlyInstable,
    Instable,
    VeryInstable,
    MostInstable,
  }

  public class BiasDict : Dictionary<LPCategory, BiasIndex> {
    public static BiasDict empty => new BiasDict();
    public override string ToString() =>
      String.Join("", Enum.GetValues(typeof(LPCategory)).Cast<LPCategory>().Select(type => this.ContainsKey(type) ? (int)this[type] : 0));
  }

  public class Hybridizer {
    public static PlantData Hybridize(PlantData hybrid1, PlantData hybrid2, float perc,
        BiasDict biases, Stability stability, MutationSetup mutationSetup) {
      // Debug.Log("Hybridize perc " + perc + " stability " + stability + " biases " + biases.ToLog() + " | mutationSetup: " + mutationSetup);

      MutationsData mutations = MutationsData.GetMutationsData(mutationSetup);
      // Debug.Log("Mutations: " + mutations);

      LeafParamDict hybrid = new LeafParamDict();
      LeafParamDict defaults = LeafParamDefaults.Defaults;

      foreach (LPK key in hybrid1.fields.Keys) {
        if (!hybrid2.fields.ContainsKey(key)) {
          Debug.LogWarning("Hybridize params second fields are missing key " + key);
          hybrid[key] = hybrid1.fields[key];
          continue;
        }

        LeafParam p1 = hybrid1.fields[key];
        LeafParam p2 = hybrid2.fields[key];
        LeafParam mix = p1.Copy();

        //80% should be flexible!
        //Algorithm: 1. take the base perc 2. move it 80% left or right (via PercForBias) 3. randomize on a bell curve
        float biasedPerc = biases.ContainsKey(p1.category) ? PercForBias(biases[p1.category], perc) : perc;
        biasedPerc = PlantRandomizer.RandWithCurve(new FloatRange(0f, 1f, biasedPerc),
          LPRandomValCurve.CenterBell,
          PlantRandomizer.ValForCenterBias(LPRandomValCenterBias.Squeeze2));

        LPRandomValCenterBias mutationBias = CenterBiasWithStability(stability);
        MutationTarget specificTarget = mutations.specifics.Find(mt => mt.lpk == key);
        if (!specificTarget.IsDefault()/* || mutations.categories.Contains(p1.category)*/) {
          mutationBias = defaults[specificTarget.lpk].randomValCenterBias;
          mutationBias = ModifyBiasFromDefault(mutationBias, specificTarget.stability);
          if (stability > specificTarget.stability) {
            Debug.LogWarning($"Hybridize base stability {stability} set higher than specific stability {specificTarget}");
          }
        } else if (p1.mode == LPMode.ColorHSL) { //for default value colors
          specificTarget.hslComponent = (HSLComponent)BWRandom.UnseededInt(0, 3);
        }

        if (p1.mode == LPMode.Float) {
          mix.value = p1.value * (1f - biasedPerc) + p2.value * biasedPerc;
          if (GlobalVars.instance.useDefaultBasedMutations) mix.value = p1.range.Default;
          float newVal = PlantRandomizer.RandWithCurve(p1.range.WithDefault(mix.value),
            p1.randomValCurve,
            PlantRandomizer.ValForCenterBias(mutationBias));
          mix.value = newVal;

        } else if (p1.mode == LPMode.ToggleDEPRECATED) {
          // mix.enabled = p1.enabled || p2.enabled;

        } else if (p1.mode == LPMode.ColorHSL) {
          HSL h1 = p1.hslValue;
          HSL h2 = p2.hslValue;
          HSL hMix = h1.Mix(h2, biasedPerc);
          mix.hslValue = hMix;
          if (GlobalVars.instance.useDefaultBasedMutations) mix.hslValue = p1.hslRange.defaultValues;

          HSLRange randRange = new HSLRange(p1.hslRange.hueRange.WithDefault(mix.hslValue.hue),
                                            p1.hslRange.satRange.WithDefault(mix.hslValue.saturation),
                                            p1.hslRange.valRange.WithDefault(mix.hslValue.lightness));
          mix.hslValue = PlantRandomizer.RandomizeHSL(randRange, mutationBias, specificTarget.hslComponent,
            PlantRandomizer.RandomizerStrength.High);
        } else {
          Debug.LogError("LeafParam mode not supported: " + p1.mode);
          hybrid[key] = p1;
          continue;
        }

        if (key == LPK.DistortCurl) mix.enabled = false;

        hybrid[key] = mix;
      }

      PlantIndexEntry entry = PlantIndexEntry.GenerateEntry(CreateHybridName(hybrid1, hybrid2));
      return new PlantData(hybrid, entry, PlantCollection.Temporary,
        BWRandom.UnseededInt(1, 99999999));
    }

    private static float PercForBias(BiasIndex bi, float percBase) {
      float divisor = 0.2f;
      switch (bi) {
        case BiasIndex.None: return percBase;
        case BiasIndex.Left: return percBase * divisor;
        case BiasIndex.Right: return percBase + ((1f - percBase) * (1f - divisor));
      }
      return percBase;
    }

    public static LPRandomValCenterBias CenterBiasWithStability(Stability stability) {
      switch (stability) {
        case Stability.SlightlyInstable: return LPRandomValCenterBias.Squeeze5;
        case Stability.Instable: return LPRandomValCenterBias.Squeeze3;
        case Stability.VeryInstable: return LPRandomValCenterBias.Squeeze1;
        case Stability.MostInstable: return LPRandomValCenterBias.Default;
      }
      return LPRandomValCenterBias.None;
    }

    public static LPRandomValCenterBias ModifyBiasFromDefault(LPRandomValCenterBias baseBias, Stability stability) {
      //increase baseBias by stability, capping at max value
      LPRandomValCenterBias newBias = (LPRandomValCenterBias)(Mathf.Min((int)baseBias + (int)stability, (int)LPRandomValCenterBias.Spread3));
      if (newBias == LPRandomValCenterBias.None) newBias = LPRandomValCenterBias.Squeeze3;
      return newBias;
    }

    public static string CreateHybridName(PlantData h1, PlantData h2) =>
      h1.indexEntry.name + " x " + h2.indexEntry.name + " " + BWRandom.Unseeded(() => BWRandom.Range(0, 10000));

  }
}
