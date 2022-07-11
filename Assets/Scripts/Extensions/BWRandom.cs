using System;
using UnityEngine;

public enum BWRandomCurve {
  Default,
  Quad,
}

public enum BWRandomHalf {
  All,
  PosOnly,
  NegOnly
}

namespace BionicWombat {
  public static class BWRandom {
    private static bool _enabled = true;
    public static bool enabled {
      get => _enabled;
    }

    public static void SetSeed(int seed) {
      UnityEngine.Random.InitState(seed);
      _enabled = seed != 0;
    }

    public static float Range(float min, float max) {
      if (!_enabled) return (min + max) / 2f;
      return UnityEngine.Random.Range(min, max);
    }

    public static float RangeAdd(float lower, float upper, BWRandomHalf half = BWRandomHalf.All) {
      if (!_enabled) return 0f;
      return UnityEngine.Random.Range(half == BWRandomHalf.PosOnly ? 0f : lower,
                          half == BWRandomHalf.NegOnly ? 0f : upper);
    }

    public static float RangeAdd(float range, BWRandomHalf half = BWRandomHalf.All) => RangeAdd(-range, range, half);

    public static float RangeMult(float range, BWRandomHalf half = BWRandomHalf.All) {
      if (!_enabled) return 1f;
      return UnityEngine.Random.Range(half == BWRandomHalf.PosOnly ? 1f : -range + 1f,
                          half == BWRandomHalf.NegOnly ? 1f : range + 1f);
    }
  }
}
