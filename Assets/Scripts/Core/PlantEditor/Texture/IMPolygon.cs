using System.Collections.Generic;
using System.Linq;
using ImageMagick;
using UnityEngine;
using static BionicWombat.PointDExtensions;

namespace BionicWombat {
  public class IMPolygon {
    private Vector2[] points;
    private Vector2[] iEdges;
    private float nearestNormSqr = float.MaxValue;

    public IMPolygon(Vector2[] points) {
      this.points = points;
    }

    public IMPolygon(PointD[] pds) {
      points = VecsFromPDs(pds);
    }

    public void Precalculate() {
      if (points.Length == 0) iEdges = new Vector2[0];
      else if (points.Length == 1) iEdges = new Vector2[1] { new Vector2(0, 0) };
      else {
        List<Vector2> list = new List<Vector2>();
        Vector2 pB = points.Last();
        for (int i = 0; i < points.Length; i++) {
          Vector2 pA = pB;
          pB = points[i];
          Vector2 pAB = pB - pA;
          float DD = pAB.NormSqr();
          if (DD > 0f)
            list.Add(pAB / DD);
          else
            list.Add(Vector2.zero);
        }
        iEdges = list.ToArray();
      }
    }

    public Vector2 NearestPointFrom(Vector2 p) {
      int len = points.Length;
      Vector2 nearest = Vector2.positiveInfinity;
      if (len > 1) {
        nearestNormSqr = float.MaxValue;
        nearest = Vector2.positiveInfinity;

        if (iEdges == null) Precalculate();

        Vector2 pB = points.Last();
        for (int i = 0; i < len; i++) {
          Vector2 pA = pB;
          pB = points[i];

          Vector2 q = Vector2.positiveInfinity;
          float t = Vector2.Dot(p - pA, iEdges[i]);
          if (t <= 0f)
            q = pA;
          else if (t < 1f)
            q = (1f - t) * pA + t * pB;
          else
            q = pB;

          float qq = (q - p).NormSqr();
          if (qq < nearestNormSqr) {
            nearest = q;
            nearestNormSqr = qq;
          }
        }
      } else if (len == 1) {
        nearest = points[0];
        nearestNormSqr = (nearest - p).NormSqr();
      } else {
        // Debug.LogError("NearestPointFrom points is empty");
        nearestNormSqr = float.MaxValue;
      }
      return nearest;
    }
  }

  public static class Extensions {
    public static float NormSqr(this Vector2 v) {
      return (v.x * v.x) + (v.y * v.y);
    }
  }
}
