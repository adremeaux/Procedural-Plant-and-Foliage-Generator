using UnityEngine;

namespace BionicWombat {
  public static class CommonErrorChecker {
    public static void CheckZeroZ(Vector3 v) {
      if (v.z <= -0.1f || v.z >= 0.1f) {
        Debug.LogError("Z position is not zero! Object may be hidden. " + v);
      }
    }

    public static void CheckZeroZ(GameObject go) {
      CheckZeroZ(go.transform.position);
    }

    public static void CheckZeroZ(Transform t) {
      CheckZeroZ(t.position);
    }
  }

}
