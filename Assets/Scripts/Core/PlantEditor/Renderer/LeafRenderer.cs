using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using mattatz.Triangulation2DSystem;
using UnityEngine;
using static BionicWombat.CoalescingTimer;

namespace BionicWombat {
  [Serializable]
  public class LeafRenderer {
    // public Vector3[] undistortedVerts { get; private set; }
    // private int[] undistortedTris;
    private float[] vertDistances;
    public float leafWidth = 0f;

    public async Task<Mesh> Render(List<Curve> contourCurves, List<Curve> additionalCurves,
        int lineSteps, int meshDensity, bool shouldRetryIfNecessary, int subdivSteps, string randomBS) {
      SplitTimer st = new SplitTimer("Render").Start();
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
      bool triResult = true;
      await Task.Run(() => {
        Vector2[] points = GetPolyPathPoints(contourCurves, lineSteps);
        List<Vector2> l = points.ToList<Vector2>();
        points = Utils2D.Constrain(l, 0.01f).ToArray();

        Vector2[] additionalPoints = GetPolyPathPoints(additionalCurves, 5);
        Coalesce(st, "GetPolyPathPoints");
        if (additionalPoints.Length > 0) {
          List<Vector2> l2 = additionalPoints.ToList<Vector2>();
          additionalPoints = Utils2D.Constrain(l2, 0.01f).ToArray();
          l2 = l2.GetRange(1, 3);
          // additionalPoints = new Vector2[1] { new Vector2(0f, -3f) };
        }

        TriangulationMode mode = TriangulationMode.Uniform;
        if (randomBS == "cla") mode = TriangulationMode.Classic;
        else if (randomBS == "sim") mode = TriangulationMode.Simple;

        Polygon2D polygon = Polygon2D.Contour(points.ToArray(), additionalPoints.ToArray());
        DateTime runningTime = DateTime.Now;
        triResult = tri.Triangulate(polygon, meshDensity, randomBS, mode, shouldRetryIfNecessary, angle, threshold);
        Coalesce(st, "Triangulate");
        if (triResult) {
          tri.Build();
          Coalesce(st, "tri.Build");
        }
      });
      if (!triResult) return null;

      Mesh m = tri.GetMesh();
      if (m == null) return null;

      (Vector3 tip, int tipIdx) = GetTip(m);
      Mesh subdiv = subdivSteps > 0 ? Torec.CatmullClark.Subdivide(m, subdivSteps,
        new Torec.CatmullClark.Options(Torec.CatmullClark.Options.BoundaryInterpolation.fixBoundaries)) : m;
      m = subdiv;
      Coalesce(st, "Subdivide");

      if (m.vertices.Length == 0) {
        Debug.LogWarning("No mesh verts");
        return m;
      }

      //correct the tip because the subdivision is moving it
      Vector3[] verts = m.vertices;
      verts[tipIdx] = tip;
      m.vertices = verts;

      m.uv = CalcUVs(m.vertices.Select(v => (Vector2)v).ToArray());
      Coalesce(st, "CalcUVs");

      m.RecalculateNormals();
      m.RecalculateBounds();
      Coalesce(st, "Finish");
      return m;
    }

    public Vector3[] Distort(List<DistortionCurve> dCurves, Vector3[] baseVerts, Curve midrib, LeafParamDict fields, bool shouldDistort) {
      Vector3[] newVerts = baseVerts.Copy();
      if (shouldDistort) {
        foreach (DistortionCurve dc in dCurves) {
          newVerts = DistortOnCurve(dc, baseVerts, newVerts, midrib, leafWidth, fields[LPK.DistortCupClamp].value);
        }
      }
      return newVerts;
    }

    public static void AssignToMesh(Mesh m, MeshData meshData) {
      // Debug.Log("num verts old: " + m.vertices.Length + " | num verts new: " + meshData.vertices.Length);
      // Debug.Log("tris: " + meshData.triangles.ToLogGrouped(3));
      // Debug.Log("filtered: " + FilterTris(meshData.triangles).ToLog());
      // Debug.Log("meshData.vertices: " + meshData.vertices.ToLog());
      if (meshData.vertices != null) m.vertices = meshData.vertices;
      if (meshData.triangles != null) m.triangles = meshData.triangles;
      if (meshData.uv != null) m.uv = meshData.uv;
      if (meshData.colors != null) m.colors = meshData.GetColors();
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
        BatchLogger.Log("" + fade, "fade", 100000, 0);

        // Debug.Log("dist: " + dist + " | baseVert: " + baseVert + " | pointAlong: " + pointAlong + " | inflVert: " + inflVert + " | magnet: " + magnet);
        if (fade <= 0.01f ||
            (curve.config.skipOutsideLowerBound && pointAlong <= 0.01f)) {
          vArr[i] = newVerts[i];
          // BatchLogger.Log("" + Vector3.zero, "deltaVert", 100000, 0);
          continue;
        }

        Vector3 magnet = curve.GetMagnetPoint(pointAlong);
        Vector3 deltaVert = new Vector3((magnet.x - baseVert.x) * fade,
                                        (magnet.y - baseVert.y) * fade,
                                        (magnet.z - baseVert.z) * fade);
        if ((curve.config.affectAxes & Axis.x) == 0) deltaVert = deltaVert.WithX(0);
        if ((curve.config.affectAxes & Axis.y) == 0) deltaVert = deltaVert.WithY(0);
        if ((curve.config.affectAxes & Axis.z) == 0) deltaVert = deltaVert.WithZ(0);
        // BatchLogger.Log("" + deltaVert, "deltaVert", 100000, 0);
        vArr[i] = newVerts[i] + deltaVert;
      }
      // Debug.Log("BaseVertsCount: " + baseVerts.Count());
      // BatchLogger.Flush();
      return vArr;
    }

    public MeshData ExtrudeMesh(MeshData extrudeData, float edgeDepth, float succThicc) {
      MeshData returnData = new MeshData();
      List<Vector2> uvList = extrudeData.uv.ToList();
      uvList.AddRange(extrudeData.uv);

      List<Color> colors = Enumerable.Repeat(Color.white, extrudeData.vertices.Length).ToList();

      //create back and translate
      int vertCount = extrudeData.vertices.Length;
      Vector3[] allVerts = new Vector3[vertCount * 2];
      for (int i = 0; i < vertCount; i++) {
        allVerts[i] = extrudeData.vertices[i];
        float z = 0.02f + (edgeDepth * 0.1f);
        if (vertDistances.Length == vertCount) {
          float dist = vertDistances[i];
          z += (0.5f * succThicc * dist);
        }
        allVerts[i + vertCount] = allVerts[i].AddZ(z);
      }
      returnData.vertices = allVerts;
      returnData.uv = uvList.ToArray();

      colors.AddRange(Enumerable.Repeat(Color.black, allVerts.Length - colors.Count));

      //turn around the back triangles
      List<int> tris = extrudeData.triangles.ToList();
      List<int> newTris = tris.ToList();
      for (int i = 0; i < tris.Count; i++)
        newTris[i] = tris[i] + vertCount;
      tris.AddRange(newTris.Reversed());
      returnData.triangles = tris.ToArray();

      //create edge tris
      int[] orderedEdgeVerts = extrudeData.orderedEdgeVerts;
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
      List<int> mTri = returnData.triangles.ToList();
      mTri.AddRange(edgeTris);
      returnData.triangles = mTri.ToArray();

      // Debug.Log(surfaceNormal.x + " | " + allVerts[edgeTris[0]] + " | " + allVerts[edgeTris[1]] + " | " + allVerts[edgeTris[2]]);

      // Vector2[] uv = m.uv;
      // Vector2 baseUV = uv[orderedEdgeVerts[0]];
      // for (int i = uv.Length - edgeTris.Count; i < uv.Length; i++)
      //   uv[i] = Vector2.zero;//baseUV;
      // m.uv = uv;

      returnData.SetColors(colors.ToArray());
      return returnData;
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
        int useSteps = GetLineSteps(curve, curves, lineSteps);
        for (float i = 0; i <= useSteps + 0.1f; i++) {
          newPoints.Add(curve.GetPoint(i / (float)useSteps));
        }
      }
      return newPoints.ToArray();
    }

    public static int GetLineSteps(Curve curve, List<Curve> curves, int lineSteps) {
      int useSteps = lineSteps;
      if (curve.FastLength() <= 2f ||
         ((curve == curves.First() || curve == curves.Last()) && curves.Count > 2))
        return Mathf.Min(lineSteps, 8);
      return useSteps;
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

    public static Vector2 GetNormalizedCenter(Vector2 min, Vector2 max) {
      return new Vector2(-min.x / (max.x - min.x), 1f - (-min.y / (max.y - min.y)));
    }

    public static LeafBoundsData GetBoundsData(List<Curve> curves, int lineSteps) {
      Vector2[] uvs = GetPolyPathPoints(curves, lineSteps);
      return new LeafBoundsData(uvs);
    }

    public void Clear() {
      vertDistances = null;
    }
  }

  public class LeafBoundsData {
    public float scale = 1f;
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

    public float width => (maxX - minX) * scale;
    public float height => (maxY - minY) * scale;
    public float area => width * height;
  }

}
