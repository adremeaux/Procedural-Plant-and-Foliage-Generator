using System.Collections.Generic;
using ImageMagick;
using UnityEngine;

namespace BionicWombat {
  public class IMRenderableVein {
    public LeafVein vein;
    public PointD[] veinPoly;
    public PointD[] veinPolyRadiance;
    public PointD[] veinPolyExtraThick;
    public PointD[] veinPolyNormalWidth;
    public PointD[] veinPoints;

    public IMRenderableVein(LeafVein vein,
        PointD[] veinPoly,
        PointD[] veinPolyRadiance,
        PointD[] veinPolyExtraThick,
        PointD[] veinPolyNormalWidth,
        PointD[] veinPoints) {
      this.vein = vein;
      this.veinPoly = veinPoly;
      this.veinPolyRadiance = veinPolyRadiance;
      this.veinPolyExtraThick = veinPolyExtraThick;
      this.veinPolyNormalWidth = veinPolyNormalWidth;
      this.veinPoints = veinPoints;
    }

    public List<IMRenderableVeinPoly> GenPolysList(PointD[] points) {
      if (points.Length % 2 == 1) Debug.LogWarning("GenPolysList odd length");
      if (points.Length <= 2) {
        Debug.LogWarning("GenPolysList called with few points");
        return new List<IMRenderableVeinPoly>();
      }

      List<IMRenderableVeinPoly> list = new List<IMRenderableVeinPoly>();
      int count = points.Length / 2 - 1;
      if (count != veinPoints.Length - 1) {
        // Debug.LogError("GenPolysList count mismatch: " + count + ", " + (veinPoints.Length - 1));
        // return null;
      }

      PointD lastF = points[0];
      PointD lastB = points[points.Length - 1];
      PointD lastP = veinPoints[0];
      for (int front = 1; front <= count; front++) {
        int back = points.Length - front - 1;
        PointD newF = points[front];
        PointD newB = points[back];
        PointD newP = veinPoints[front];
        list.Add(new IMRenderableVeinPoly {
          points = new PointD[] { lastF, newF, newB, lastB },
          perpendicular = CurveHelpers.Angle(new Vector2((float)newP.X, (float)newP.Y), new Vector2((float)lastP.X, (float)lastP.Y)),
        });
        lastF = newF;
        lastB = newB;
        lastP = newP;
      }

      return list;
    }
  }

  public struct IMRenderableVeinPoly {
    public PointD[] points;
    public float perpendicular;

    // public IMRenderableVeinPoly(PointD
  }
}
