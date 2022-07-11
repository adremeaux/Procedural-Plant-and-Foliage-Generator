using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using mattatz;
using mattatz.Triangulation2DSystem;
using UnityEngine;

namespace BionicWombat {
  [Serializable]
  public class LeafRenderer {
    private Vector3[] undistortedVerts;
    private int[] undistortedTris;
    private float[] vertDistances;
    float leafWidth = 0f;

    public async Task<Mesh> Render(List<Curve> contourCurves, List<Curve> additionalCurves, int lineSteps, int subdivSteps, string randomBS) {
      float angle = 20f;
      float threshold = 0.25f * ((float)lineSteps / 10f);
      threshold = Mathf.Clamp(threshold, 0.25f, 0.5f);
      if (randomBS.Length > 0) {
        string[] pieces = randomBS.Split(",");
        if (pieces.Length >= 2) {
          angle = float.Parse(pieces[0]);
          threshold = float.Parse(pieces[1]);
        }
      }

      (Vector2 min, Vector2 max) = GetBoundingBox(contourCurves, lineSteps);
      leafWidth = max.x - min.x;

      Triangulation2D tri = new Triangulation2D();
      DateTime rt = DateTime.Now;
      await Task.Run(() => {
        Vector2[] points = GetPolyPathPoints(contourCurves, lineSteps);
        List<Vector2> l = points.ToList<Vector2>();
        points = Utils2D.Constrain(l, 0.01f).ToArray();

        Vector2[] additionalPoints = GetPolyPathPoints(additionalCurves, 5);
        if (additionalPoints.Length > 0) {
          List<Vector2> l2 = additionalPoints.ToList<Vector2>();
          additionalPoints = Utils2D.Constrain(l2, 0.01f).ToArray();
          l2 = l2.GetRange(1, 3);
          // additionalPoints = new Vector2[1] { new Vector2(0f, -3f) };
        }

        Polygon2D polygon = Polygon2D.Contour(points.ToArray(), additionalPoints.ToArray());
        tri.Triangulate(polygon, angle, threshold);
        tri.Build();
      });

      Mesh m = tri.GetMesh();
      if (m == null) return null;

      (Vector3 tip, int tipIdx) = GetTip(m);
      Mesh subdiv = subdivSteps > 0 ? Torec.CatmullClark.Subdivide(m, subdivSteps,
        new Torec.CatmullClark.Options(Torec.CatmullClark.Options.BoundaryInterpolation.fixBoundaries)) : m;
      m = subdiv;

      //correct the tip because the subdivision is moving it
      Vector3[] verts = m.vertices;
      verts[tipIdx] = tip;
      m.vertices = verts;

      m.uv = CalcUVs(m.vertices.Select(v => (Vector2)v).ToArray());

      undistortedVerts = m.vertices;
      undistortedTris = m.triangles;

      m.RecalculateNormals();
      m.RecalculateBounds();
      return m;
    }

    public void Distort(Mesh m, List<DistortionCurve> dCurves, Curve midrib, LeafParamDict fields, bool shouldDistort) {
      Vector3[] baseVerts = undistortedVerts.Copy();
      m.triangles = undistortedTris.Copy();
      m.vertices = baseVerts;

      Vector3[] newVerts = undistortedVerts.Copy();
      if (shouldDistort) {
        foreach (DistortionCurve dc in dCurves) {
          newVerts = DistortOnCurve(dc, baseVerts, newVerts, midrib, leafWidth, fields[LPK.DistortCupClamp].value);
        }
      }
      m.vertices = newVerts;
      m.RecalculateNormals();
      m.RecalculateBounds();
    }

    private Vector3[] DistortOnCurve(DistortionCurve curve, Vector3[] baseVerts, Vector3[] newVerts, Curve midrib,
        float leafWidth, float distClamp) {
      if (curve.shouldFade && curve.config.maxFadeDist <= 0f) return newVerts;

      if (curve.config.type == LeafDistortionType.Cup) vertDistances = new float[baseVerts.Length];
      Vector3[] vArr = new Vector3[baseVerts.Length];
      for (int i = 0; i < baseVerts.Length; i++) {
        Vector3 baseVert = baseVerts[i];
        float pointAlong = curve.FindPointAlong(baseVert).raw;
        Vector3 inflVert = curve.NearestPointAlongInfluence(pointAlong);
        Vector3 midribPoint = midrib.GetClosestPoint(baseVert);
        float dist = Vector3.Distance(baseVert, inflVert);
        float fade = 1f;
        if (curve.shouldFade) {
          if (curve.config.useDistFade) {
            fade = dist / (leafWidth / 2f);
            fade = Mathf.Min(fade, distClamp);
            fade /= distClamp;
            if (curve.config.type == LeafDistortionType.Cup) vertDistances[i] = fade;
          } else {
            float span = Vector3.Distance(inflVert, midribPoint);
            float propDist = dist / span;
            fade = (Mathf.Min(propDist, curve.config.maxFadeDist) / curve.config.maxFadeDist);
          }

          if (!curve.config.reverseFade) fade = 1f - fade;
        }

        Vector3 magnet = curve.GetMagnetPoint(pointAlong);
        // Debug.Log("dist: " + dist + " | baseVert: " + baseVert + " | pointAlong: " + pointAlong + " | inflVert: " + inflVert + " | magnet: " + magnet);
        if (fade <= 0.01f ||
            (curve.config.skipOutsideLowerBound && pointAlong <= 0.01f)) {
          vArr[i] = newVerts[i];
          continue;
        }

        Vector3 deltaVert = new Vector3((magnet.x - baseVert.x) * fade,
                                        (magnet.y - baseVert.y) * fade,
                                        (magnet.z - baseVert.z) * fade);
        if ((curve.config.affectAxes & Axis.x) == 0) deltaVert = deltaVert.WithX(0);
        if ((curve.config.affectAxes & Axis.y) == 0) deltaVert = deltaVert.WithY(0);
        if ((curve.config.affectAxes & Axis.z) == 0) deltaVert = deltaVert.WithZ(0);

        vArr[i] = newVerts[i] + deltaVert;
      }
      return vArr;
    }

    public void ExtrudeMesh(Mesh m, float edgeDepth, float succThicc, bool shouldResetFirst) {
      if (shouldResetFirst) {
        Vector3[] baseVerts = undistortedVerts.Copy();
        m.triangles = undistortedTris.Copy();
        m.vertices = baseVerts;
      }

      List<Vector2> uvList = m.uv.ToList();
      uvList.AddRange(m.uv);

      //create back and translate
      int vertCount = m.vertices.Length;
      Vector3[] newVerts = m.vertices.ToArray();
      Vector3[] allVerts = new Vector3[vertCount * 2];
      for (int i = 0; i < vertCount; i++) {
        allVerts[i] = m.vertices[i];
        float z = 0.02f + (edgeDepth * 0.1f);
        if (vertDistances.Length == vertCount) {
          float dist = vertDistances[i];
          z += (0.5f * succThicc * dist);
        }
        allVerts[i + vertCount] = newVerts[i] = newVerts[i].AddZ(z);
      }
      m.vertices = allVerts;
      m.uv = uvList.ToArray();

      //turn around the back triangles
      List<int> tris = m.triangles.ToList();
      List<int> newTris = tris.ToList();
      for (int i = 0; i < tris.Count; i++)
        newTris[i] = tris[i] + vertCount;
      tris.AddRange(newTris.Reversed());
      m.triangles = tris.ToArray();

      //create edge tris
      int[] orderedEdgeVerts = EdgeFinder.FindEdgeVerts(m);
      List<int> edgeTris = new List<int>();
      for (int i = 0; i < orderedEdgeVerts.Length - 1; i++) {
        int e = orderedEdgeVerts[i];
        int f = orderedEdgeVerts[i + 1];
        edgeTris.Add(e, e + vertCount, f);
        edgeTris.Add(f, e + vertCount, f + vertCount);

        //face the other way too because I can't figure it out
        edgeTris.Add(e + vertCount, e, f);
        edgeTris.Add(e + vertCount, f, f + vertCount);
      }
      // Vector3 surfaceNormal = GetTriFacing(allVerts[edgeTris[0]], allVerts[edgeTris[1]], allVerts[edgeTris[2]]);
      // edgeTris = edgeTris.Reversed();
      // if ((allVerts[edgeTris[0]].x < 0f && surfaceNormal.x < 0f) ||
      //     (allVerts[edgeTris[0]].x > 0f && surfaceNormal.x > 0f)) edgeTris = edgeTris.Reversed();
      List<int> mTri = m.triangles.ToList();
      mTri.AddRange(edgeTris);
      m.triangles = mTri.ToArray();

      // Debug.Log(surfaceNormal.x + " | " + allVerts[edgeTris[0]] + " | " + allVerts[edgeTris[1]] + " | " + allVerts[edgeTris[2]]);

      // Vector2[] uv = m.uv;
      // Vector2 baseUV = uv[orderedEdgeVerts[0]];
      // for (int i = uv.Length - edgeTris.Count; i < uv.Length; i++)
      //   uv[i] = Vector2.zero;//baseUV;
      // m.uv = uv;

      m.RecalculateNormals();
      m.RecalculateBounds();
    }

    public static Vector3 GetTriFacing(Vector3 a, Vector3 b, Vector3 c) {
      Vector3 v = Vector3.Cross(b - a, c - a);
      return v;
    }

    public static Vector2[] GetPolyPathPoints(Curve curve, int lineSteps) =>
      GetPolyPathPoints(new List<Curve>() { curve }, lineSteps);

    public static Vector2[] GetPolyPathPoints(List<Curve> curves, int lineSteps) {
      List<Vector2> newPoints = new List<Vector2>();
      foreach (Curve curve in curves) {
        for (float i = 0; i <= lineSteps + 0.1f; i++) {
          newPoints.Add(curve.GetPoint(i / (float)lineSteps));
        }
      }
      return newPoints.ToArray();
    }

    public static Vector2[] CalcUVs(Vector2[] uvs) {
      Vector2[] newUVs = uvs;
      (float scale, Vector2 offset) = GetUVScaleAndOffset(uvs);
      for (int i = 0; i < newUVs.Length; i++) {
        Vector2 v = newUVs[i];
        newUVs[i] = (v - offset) / scale;
      }
      return newUVs;
    }

    public static (float scale, Vector2 offset) GetUVScaleAndOffset(Vector2[] uvs) {
      (Vector2 min, Vector2 max) = GetBoundingBox(uvs);
      float spanX = max.x - min.x;
      float spanY = max.y - min.y;

      float scale = spanY > spanX ? spanY : spanX;
      Vector2 offset = spanY > spanX ?
        new Vector2(-spanY / 2f, max.y) :
        new Vector2(spanX / 2f, (spanX - spanY) / 2f + max.y);
      // Debug.Log(uvs.ToLogShort());
      // Debug.Log("min: (" + minX + "," + minY + ") max: (" + maxX + "," + maxY + ")");
      // Debug.Log("span: " + spanX + ", " + spanY);
      // Debug.Log("offset: " + offset);

      return (scale, offset);
    }

    public static (Vector3 pos, int idx) GetTip(Mesh m) {
      int idx = 0;
      foreach (Vector3 v in m.vertices) {
        // BatchLogger.Log(m.bounds.Bottom() + " | " + v.y, "id");
        if (v.y <= m.bounds.Bottom() + 0.01f)
          return (v, idx);
        idx++;
      }
      return (Vector3.zero, 0);
    }

    public static (Vector2 min, Vector2 max) GetBoundingBox(List<Curve> curves, int lineSteps) {
      Vector2[] uvs = GetPolyPathPoints(curves, lineSteps);
      return GetBoundingBox(uvs);
    }

    public static (Vector2 min, Vector2 max) GetBoundingBox(Vector2[] uvs) {
      float minX = float.MaxValue;
      float minY = float.MaxValue;
      float maxX = float.MinValue;
      float maxY = float.MinValue;
      foreach (Vector2 v in uvs) {
        minX = v.x < minX ? v.x : minX;
        minY = v.y < minY ? v.y : minY;
        maxX = v.x > maxX ? v.x : maxX;
        maxY = v.y > maxY ? v.y : maxY;
      }
      return (new Vector2(minX, minY), new Vector2(maxX, maxY));
    }

    public static LeafBoundsData GetBoundsData(List<Curve> curves, int lineSteps) {
      Vector2[] uvs = GetPolyPathPoints(curves, lineSteps);
      return new LeafBoundsData(uvs);
    }

    public void Clear() {
      undistortedVerts = null;
      undistortedTris = null;
      vertDistances = null;
    }
  }

  public class LeafBoundsData {
    public Vector2 leftApex;
    public Vector2 rightApex;
    public Vector2 tip;
    public Vector2 lobeLeft;
    public Vector2 lobeRight;
    public float minX = float.MaxValue;
    public float minY = float.MaxValue;
    public float maxX = float.MinValue;
    public float maxY = float.MinValue;

    public LeafBoundsData(Vector2[] uvs) {
      foreach (Vector2 v in uvs) {
        if (v.x < minX) {
          minX = v.x;
          leftApex = v;
        }
        if (v.x > maxX) {
          maxX = v.x;
          rightApex = v;
        }
        if (v.y < minY) {
          minY = v.y;
          tip = v;
        }
        if (v.y >= maxY) {
          maxY = v.y;
          if (v.x < 0) lobeLeft = v;
          else lobeRight = v;
        }
      }
    }

    public float width => maxX - minX;
    public float height => maxY - minY;
  }


}