using System;
using UnityEngine;

namespace BionicWombat {
  public static class MiscUtils {
    public static Vector3 EncodeInFloat3(int[] f1,
      int[] f2 = null,
      int[] f3 = null) {

      float EncodeArr(int[] arr) {
        if (!arr.HasLength()) return 0f;
        float r = 0f;
        for (int i = 0; i < arr.Length; i++) {
          int v = Mathf.Clamp(arr[i], 0, 99);
          r += v * Mathf.Pow(100, i);
        }
        return r;
      };

      return new Vector3(EncodeArr(f1), EncodeArr(f2), EncodeArr(f3));
    }
  }
}
