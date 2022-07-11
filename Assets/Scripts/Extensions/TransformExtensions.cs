using System;
using UnityEngine;

namespace BionicWombat {
public static class TransformExtentions {
  public static void Reset(this Transform t) {
    t.localPosition = Vector3.zero;
    t.localRotation = Quaternion.identity;
    t.localScale = new Vector3(1f, 1f, 1f);
  }
}
}