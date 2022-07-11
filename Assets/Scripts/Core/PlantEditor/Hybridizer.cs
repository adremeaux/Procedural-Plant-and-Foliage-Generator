using System;
using System.Collections.Generic;
using UnityEngine;

namespace BionicWombat {
  public class Hybridizer {
    public static LeafParamDict Hybridize(LeafParamDict fields1, LeafParamDict fields2, float bias = 0.5f) {
      LeafParamDict hybrid = new LeafParamDict();
      foreach (LPK key in fields1.Keys) {
        if (!fields2.ContainsKey(key)) {
          Debug.LogWarning("Preset params are missing key " + key);
          hybrid[key] = fields1[key];
          continue;
        }

        LeafParam p1 = fields1[key];
        LeafParam p2 = fields2[key];
        LeafParam mix = p1.Copy();
        if (p1.mode == LPMode.Float) {
          mix.value = p1.value * (1f - bias) + p2.value * bias;
        } else if (p1.mode == LPMode.Toggle) {
          mix.enabled = p1.enabled || p2.enabled;
        } else if (p1.mode == LPMode.ColorHSL) {
          HSL h1 = p1.hslValue;
          HSL h2 = p2.hslValue;
          HSL hMix = h1.Mix(h2, bias);
          mix.hslValue = hMix;
        } else {
          Debug.LogError("LeafParam mode not supported: " + p1.mode);
          hybrid[key] = p1;
          continue;
        }

        if (key == LPK.DistortCurl) mix.enabled = false;

        hybrid[key] = mix;
      }

      return hybrid;
    }
  }
}