using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
  public static class PolygonExtensions {
    [Serializable]
    // file:///C:/Users/adrem/Plants_Game/Assets/Scripts/Tools/MakeStruct.html?a=Coord int idx Vector2 point float dist float angle
    public struct Coord {
      public int idx;
      public Vector2 point;
      public float dist;
      public float angle;

      public Coord(int idx, Vector2 point, float dist, float angle) {
        this.idx = idx;
        this.point = point;
        this.dist = dist;
        this.angle = angle;
      }

      public override string ToString() {
        return "[Coord] idx: " + idx + " | point: " + point + " | dist: " + dist + " | angle: " + angle;
      }
    }

    public static Vector2[] SimplifyPolyDeprecated(this List<Vector2> points, int targetLen) {
      if (points.Count <= targetLen) return points.ToArray();

      float maxAngle = 10f;
      Coord[] arr = new Coord[points.Count];
      arr[0] = new Coord(0, points[0], 9999f, 90f);
      arr[arr.Length - 1] = new Coord(0, points[points.Count - 1], 9999f, 90f);
      for (int i = 0; i < points.Count - 2; i++) {
        Vector2 v0 = points[i];
        Vector2 v1 = points[i + 1];
        Vector2 v2 = points[i + 2];

        float a1 = Curve.Angle(v0, v1);
        float a2 = Curve.Angle(v1, v2);
        float res = 180f + a2 - a1;
        if (res < 0) res = 360f + res;
        res = res % 180f;
        res = 90f - Mathf.Abs(res - 90);
        arr[i + 1] = new Coord(i + 1, v1, Mathf.Abs(v0.Distance(v2)), res);
      }

      // DebugBW.Log("arr2: " + arr.ToLog());
      List<Coord> sortedByDist = arr.ToList().Sorted((c1, c2) => c1.dist.CompareTo(c2.dist)).ToList();
      // DebugBW.Log("l: " + sortedByDist.ToLog());

      int removeTarget = sortedByDist.Count - targetLen;
      Debug.Log("Trying to remove " + removeTarget + " elements ");
      for (int i = 0; i < sortedByDist.Count; i++) {
        if (removeTarget <= 0) break;
        if (sortedByDist[i].angle < maxAngle) {
          removeTarget--;
          sortedByDist.RemoveAt(i);
          i--;
        }
      }
      // Debug.Log("Final count: " + sortedByDist.Count + " | targetsRemaining: " + removeTarget);
      sortedByDist.Sort((c1, c2) => c1.idx.CompareTo(c2.idx));
      Vector2[] final = sortedByDist.Select(coord => coord.point).ToArray();
      // DebugBW.Log("Final: " + final.ToLog());

      return final;
    }
  }
}
