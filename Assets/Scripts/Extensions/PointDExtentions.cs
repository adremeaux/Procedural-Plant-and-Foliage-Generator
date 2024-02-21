using System.Collections.Generic;
using ImageMagick;
using UnityEngine;

namespace BionicWombat {
  public static class PointDExtensions {
    public static Vector2[] VecsFromPDs(PointD[] pds) {
      Vector2[] points = new Vector2[pds.Length];
      for (int i = 0; i < pds.Length; i++)
        points[i] = new Vector2((float)pds[i].X, (float)pds[i].Y);
      return points;
    }

    public static PointD[] PDsFromVecs(Vector2[] vecs) {
      PointD[] points = new PointD[vecs.Length];
      for (int i = 0; i < vecs.Length; i++)
        points[i] = new PointD((double)vecs[i].x, (double)vecs[i].y);
      return points;
    }

    public static Vector2[] AsVectors(this PointD[] pds) => VecsFromPDs(pds);
    public static PointD[] AsPointDs(this Vector2[] vecs) => PDsFromVecs(vecs);

    public static Rect GetExtents(PointD[] points) {
      double minX = double.MaxValue;
      double minY = double.MaxValue;
      double maxX = double.MinValue;
      double maxY = double.MinValue;
      foreach (PointD p in points) {
        if (p.X > maxX) maxX = p.X;
        if (p.Y > maxY) maxY = p.Y;
        if (p.X < minX) minX = p.X;
        if (p.Y < minY) minY = p.Y;
      }
      Rect rect = new Rect((float)minX, (float)minY, (float)(maxX - minX), (float)(maxY - minY));
      return rect;
    }

    public static Vector2[] RearrangeToVec(PointD[] points) {
      List<Vector2> newP = new List<Vector2>();
      foreach (PointD p in points)
        newP.Add(new Vector2((float)p.X, (float)p.Y));
      Vector2 tmp = newP[1];
      newP[1] = newP[3];
      newP[3] = tmp;
      return newP.ToArray();
    }
  }
}
