using System;
using UnityEngine;

namespace BionicWombat {
  public static class AssertsX {
    public static bool Assert(bool shouldBeTrue, string err) {
      if (!shouldBeTrue) Debug.LogError(err);
      return shouldBeTrue;
    }

    public static bool AssertWarning(bool shouldBeTrue, string err) {
      if (!shouldBeTrue) Debug.LogWarning(err);
      return shouldBeTrue;
    }
  }
}
