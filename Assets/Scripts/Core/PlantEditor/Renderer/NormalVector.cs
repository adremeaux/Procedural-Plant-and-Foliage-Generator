using System;
using UnityEngine;

namespace BionicWombat {
  public struct NormalVector {
    public Vector3 origin;
    public Vector3 normal;
    public NormalVector(Vector3 origin, Vector3 normal) {
      this.origin = origin;
      this.normal = normal;
    }
    public override string ToString() {
      return "[NV] origin: " + origin + " | normal: " + normal;
    }
  }

}
