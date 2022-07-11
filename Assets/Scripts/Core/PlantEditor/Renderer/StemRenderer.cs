using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using mattatz.Triangulation2DSystem;
using UnityEngine;
using static BionicWombat.ListExtensions;
using static BionicWombat.VectorExtensions;

namespace BionicWombat {
  [Serializable]
  public static class StemRenderer {
    public static Mesh Render(IStem stem, int lineSteps) {
      Curve3D[] curves = stem.curves.ToArray();
      Vector3[] shape = stem.shape;
      (Vector3[] stemPoints, Vector3[] normals) = GetStemPoints(curves, lineSteps);
      List<Vector3> verts = new List<Vector3>();
      List<int> tris = new List<int>();
      Quaternion faceForward = Quaternion.Euler(90, 0, 0);

      for (int i = 0; i < stemPoints.Length; i++) {
        //create shape verts
        Vector3 stemPoint = stemPoints[i];
        Vector3 normal = normals[i].normalized.Truncate();
        Quaternion q = !normal.SoftEquals(Vector3.zero) ? Quaternion.LookRotation(normal, Vector3.up) : Quaternion.identity;
        bool singlePoint = false;
        foreach (Vector3 shapePoint in shape) {
          Vector3 scaledPoint = Vector3.Lerp(Vector3.zero, shapePoint, stem.ShapeScaleAtPercent((float)i / (stemPoints.Length - 1f)));
          if (scaledPoint == Vector3.zero) {
            singlePoint = true;
            verts.Add(scaledPoint + stemPoint);
            continue;
          }
          Vector3 rotatedPoint = q * faceForward * scaledPoint;
          verts.Add(rotatedPoint + stemPoint);
        }

        //create tris
        if (singlePoint || i < stemPoints.Length - 1) {
          int sides = shape.Length;
          int floor = i * sides;
          int ceil = floor + sides;
          if (singlePoint) ceil--;
          for (int vn = floor; vn < ceil; vn++) {
            int lessOne = vn - 1;
            if (lessOne < floor) lessOne += sides;
            if (singlePoint) {
              tris.AddRange(new int[] {
              vn, ceil, lessOne
            });
            } else {
              tris.AddRange(new int[] {
              vn, vn + sides, lessOne,
              vn + sides, lessOne + sides, lessOne
            });
            }
          }
        }
      }

      Mesh mesh = new Mesh();
      mesh.vertices = verts.ToArray();
      mesh.SetTriangles(tris.ToArray(), 0);
      mesh.RecalculateNormals();
      mesh.RecalculateBounds();
      return mesh;
    }

    public static (Vector3 attachmentPoint, Quaternion attachmentRotation) GetAttachmentInfo(LeafStem stem) {
      (Vector3[] stemPoints, Vector3[] normals) = GetStemPoints(stem.curves.ToArray(), 2);
      if (stemPoints.Length == 0 || normals.Length == 0) return (Vector3.zero, Quaternion.identity);
      Quaternion q = Quaternion.Euler(0, 180, 0);
      if (!normals.Last().SoftEquals(Vector3.zero)) q = Quaternion.LookRotation(normals.Last(), Vector3.up) * q;
      Vector3 buffer = normals.Last().normalized * 0.1f;
      return (stemPoints.Last() + buffer, q);
    }

    public static (Vector3[] points, Vector3[] normals) GetStemPoints(Curve3D[] curves, int lineSteps) {
      List<Vector3> points = new List<Vector3>();
      List<Vector3> normals = new List<Vector3>();
      foreach (Curve3D c in curves) {
        Vector3[] polyPoints = c.GetPolyPoints(lineSteps);
        int idx = c == curves.First() ? 0 : 1;
        points.AddRange(polyPoints.Skip(idx));
        for (float i = 0; i <= lineSteps; i++) {
          Vector3 normal = Bezier.GetFirstDerivative(c, i / lineSteps);
          normals.Add(normal);
        }
      }
      return (points.ToArray(), normals.ToArray());
    }
  }

}