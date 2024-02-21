using System;
using UnityEngine;

namespace BionicWombat {
  public static class Destroy {
    public static void PDestroy(UnityEngine.Object o) {
      UnityEngine.Object.Destroy(o);
      // if (Application.isPlaying) GameObject.Destroy(o);
      // else GameObject.DestroyImmediate(o);
    }
  }
}
