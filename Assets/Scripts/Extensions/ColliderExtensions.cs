using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
  public static class ColliderExtensions {
    public static Mesh GetMesh(this BoxCollider col) {
      Vector3 boundPoint1 = col.center - (col.size / 2f);
      Vector3 boundPoint2 = col.center + (col.size / 2f);
      Vector3 boundPoint3 = new Vector3(boundPoint1.x, boundPoint1.y, boundPoint2.z);
      Vector3 boundPoint4 = new Vector3(boundPoint1.x, boundPoint2.y, boundPoint1.z);
      Vector3 boundPoint5 = new Vector3(boundPoint2.x, boundPoint1.y, boundPoint1.z);
      Vector3 boundPoint6 = new Vector3(boundPoint1.x, boundPoint2.y, boundPoint2.z);
      Vector3 boundPoint7 = new Vector3(boundPoint2.x, boundPoint1.y, boundPoint2.z);
      Vector3 boundPoint8 = new Vector3(boundPoint2.x, boundPoint2.y, boundPoint1.z);

      Mesh m = new Mesh();
      m.vertices = new Vector3[] { boundPoint1, boundPoint2, boundPoint3, boundPoint4, boundPoint5, boundPoint6, boundPoint7, boundPoint8 };
      m.triangles = new int[] {
      0,7,4,
      0,3,7,
      5,1,3,
      3,1,7,
      7,1,4,
      4,1,6,
      5,3,2,
      2,3,0,
      0,4,2,
      2,4,6,
      1,5,2,
      6,1,2
    };
      return m;
    }
  }
}
