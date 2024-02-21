using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
  [Serializable]
  public static class StemRenderer {
    // file:///C:/Users/adrem/Plants_Game/Assets/Scripts/Tools/MakeStruct.html?a=CurveData Vector3[] stemPoints Vector3[] normals
    public struct CurveData {
      public List<Vector3> stemPoints;
      public List<Vector3> normals;
      public List<float> widths;
      public List<float> percs;

      public CurveData(List<Vector3> stemPoints, List<Vector3> normals) {
        this.stemPoints = stemPoints;
        this.normals = normals;
        this.widths = null;
        this.percs = null;
      }
    }

    public static Mesh Render(IStem stem, int lineSteps) {
      Curve3D[] curves = stem.curves.ToArray();
      Vector3[] shape = stem.shape;
      CurveData curveData = GetStemPoints(curves, lineSteps);
      if (stem.IsTrunk()) {
        for (int i = 0; i < 10; i++)
          curveData = InsertNode(curveData, 10 * i, 1.0f, true);
      }

      List<Vector3> verts = new List<Vector3>();
      List<int> tris = new List<int>();
      List<Vector2> uvs = new List<Vector2>();
      Quaternion faceForward = Quaternion.Euler(90, 0, 0);
      float uvWidth = 0.125f;
      float uvOffset = 0f;//0.125f;
                          // uvOffset = BWRandom.UnseededInt(0, 2) * uvWidth;

      for (int i = 0; i < curveData.stemPoints.Count; i++) {
        //create shape verts
        Vector3 stemPoint = curveData.stemPoints[i];
        Vector3 normal = curveData.normals[i].normalized.Truncate();
        Quaternion q = !normal.SoftEquals(Vector3.zero) ? Quaternion.LookRotation(normal, Vector3.up) : Quaternion.identity;
        bool singlePoint = false;
        float perc = curveData.percs == null ? (float)i / (curveData.stemPoints.Count - 1f) : curveData.percs[i];
        float widthMod = curveData.widths != null ? curveData.widths[i] : 1f;
        foreach ((Vector3 shapePoint, int idx) in shape.WithIndex()) {
          Vector3 scaledPoint = widthMod * Vector3.Lerp(Vector3.zero, shapePoint,
            stem.ShapeScaleAtPercent(perc));
          if (scaledPoint == Vector3.zero) {
            singlePoint = true;
            verts.Add(stemPoint);
            uvs.Add(new Vector2(uvWidth / 2f, perc));
            continue;
          }
          Vector3 rotatedPoint = q * faceForward * scaledPoint;
          verts.Add(rotatedPoint + stemPoint);
          uvs.Add(new Vector2(idx / (float)shape.Length * uvWidth + uvOffset, perc));
          // DebugBW.Log("stemPoint: " + stemPoint + " | rotatedPoint: " + rotatedPoint + " | shapePoint: " + shapePoint);
        }

        //create tris
        if (singlePoint || i < curveData.stemPoints.Count - 1) {
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
      mesh.name = "Stem";
      mesh.vertices = verts.ToArray();
      mesh.SetTriangles(tris.ToArray(), 0);
      mesh.SetUVs(0, uvs);
      mesh.RecalculateNormals();
      mesh.RecalculateBounds();
      return mesh;
    }

    public static (Vector3 attachmentPoint, Quaternion attachmentRotation) GetAttachmentInfo(ArrangementData arrData, Curve3D[] stemCurves) {
      CurveData curveData = GetStemPoints(stemCurves, 2);
      if (curveData.stemPoints.Count == 0 || curveData.normals.Count == 0) return (Vector3.zero, Quaternion.identity);
      Quaternion q = Quaternion.Euler(0, 180, arrData.leafZAngle);
      Vector3 last = curveData.normals.Last();
      if (!last.SoftEquals(Vector3.zero) && !last.IsNaN()) {
        q = Quaternion.LookRotation(last, Vector3.up) * q; //Err: Normals.Last = NaN
      }
      Vector3 buffer = curveData.normals.Last().normalized * 0.02f;
      return (curveData.stemPoints.Last() + buffer, q);
    }

    public static CurveData GetStemPoints(
        Curve3D[] curves, int baseLineSteps, float treshhold = 0.2f) {
      List<Vector3> points = new List<Vector3>();
      List<Vector3> normals = new List<Vector3>();
      foreach (Curve3D c in curves) {
        float len = c.FastLength();
        int lineSteps = Mathf.Min(baseLineSteps, Mathf.RoundToInt(len / treshhold));
        Vector3[] polyPoints = c.GetPolyPoints(lineSteps);
        int idx = c == curves.First() ? 0 : 1;
        points.AddRange(polyPoints.Skip(idx));
        for (float i = 0; i <= lineSteps; i++) {
          Vector3 normal = Bezier.GetFirstDerivative(c, i / lineSteps);
          normals.Add(normal);
        }
      }
      return new CurveData(points, normals);
    }

    private static CurveData InsertNode(CurveData curveData, int pos, float widthMod, bool useSimpleNode) {
      if (pos >= curveData.stemPoints.Count - 1 || pos <= 0) return curveData;

      List<float> widths = curveData.widths == null ? Enumerable.Repeat(1f, curveData.stemPoints.Count).ToList() : curveData.widths;
      List<float> percs = curveData.percs?.ToList();
      if (percs == null) {
        float count = curveData.stemPoints.Count;
        float[] ps = new float[(int)count];
        for (int i = 0; i < curveData.stemPoints.Count; i++)
          ps[i] = i / (count - 1f);
        percs = ps.ToList();
      }
      int beforePos = pos - 1;
      int afterPos = pos + 1;
      Vector3 before = curveData.stemPoints[beforePos];
      Vector3 point = curveData.stemPoints[pos];
      Vector3 after = curveData.stemPoints[afterPos];
      Vector3 beforeN = curveData.normals[beforePos];
      Vector3 pointN = curveData.normals[pos];
      Vector3 afterN = curveData.normals[afterPos];
      float beforeP = percs[beforePos];
      float pointP = percs[pos];
      float afterP = percs[afterPos];
      List<Vector3> points = curveData.stemPoints.ToList();
      List<Vector3> norms = curveData.normals.ToList();

      void Insert(int pos, float perc, float width) {
        if (perc < 1f) {
          points.Insert(pos, Vector3.Lerp(before, point, perc));
          norms.Insert(pos, Vector3.Lerp(beforeN, pointN, perc));
          percs.Insert(pos, Mathf.Lerp(beforeP, pointP, perc));
        } else {
          perc -= 1f;
          points.Insert(pos, Vector3.Lerp(point, after, perc));
          norms.Insert(pos, Vector3.Lerp(pointN, afterN, perc));
          percs.Insert(pos, Mathf.Lerp(pointP, afterP, perc));
        }
        widths.Insert(pos, width * widthMod);
      }

      if (useSimpleNode) {
        widths[pos] = 1.2f * widthMod;
        Insert(afterPos, 1.1f, 1.1f);
        Insert(beforePos + 1, 0.9f, 1.1f);
      } else {
        Insert(afterPos, 1.3f, 1.05f);
        Insert(afterPos, 1.1f, 1.1f);
        Insert(beforePos + 1, 0.9f, 1.1f);
        Insert(beforePos + 1, 0.7f, 1.05f);
      }

      var cd = new CurveData(points, norms);
      cd.widths = widths;
      cd.percs = percs;
      return cd;
    }

    /* Shader Bias Function:

      [NaughtyAttributes.Button]
    public void Test() {
      for (float bias = -1f; bias <= 1.01f; bias += 0.25f) {
        Debug.Log("bias: " + bias);
        for (float y = 0f; y <= 1.01f; y += 0.1f) {
          Debug.Log("  y: " + y + " = " + Bias(bias, y));
        }
      }
    }

    public float Bias(float bias, float y) {
      float ret = 0f; //0 - 1
      float oneMinus = 1f - bias;
      if (bias >= 0f) {
        ret = Mathf.Min(y / oneMinus, 1f);
      } else {
        oneMinus = 1f - (bias * -1f);
        y = 1f - y;
        ret = Mathf.Min(y / oneMinus, 1f);
        ret = 1f - ret;
        // ret = ret * -1f + 1f;
      }
      return ret;
    }

    */
  }

}
