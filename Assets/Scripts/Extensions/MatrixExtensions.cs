using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
  public static class MatrixExtensions {
    public static Matrix4x4 Translate(this Matrix4x4 m, Vector3 translate) {
      m.m03 += translate.x;
      m.m13 += translate.y;
      m.m23 += translate.z;
      return m;
    }
  }
}
