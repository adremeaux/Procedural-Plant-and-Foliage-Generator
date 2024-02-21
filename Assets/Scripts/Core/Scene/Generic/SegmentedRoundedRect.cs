using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
  [RequireComponent(typeof(MeshRenderer))]
  [RequireComponent(typeof(MeshFilter))]
  public class SegmentedRoundedRect : MonoBehaviour {
    [Range(2, 100)] public int numPieces = 20;
    [Range(1, 20)] public int segsPerPiece = 6;
    [Range(1, 20)] public int gapInSegs = 2;
    [Range(0.1f, 10f)] public float rectWidth = 2f;
    [Range(0.1f, 10f)] public float rectLen = 1f;
    [Range(0.01f, 2f)] public float pieceHeight = 0.02f;
    [Range(0.01f, 2f)] public float pieceDepth = 0.04f;
    [Range(0.01f, 2f)] public float cornerRad = 0.1f;
    public float animSpeed = 3f;
    private string lastString = "";
    public bool starShape = false;
    public bool use3D = false;
    public float offset = 0f;
    private Curve[] curves;
    private List<Vector3> linearPointsBase;
    private SplitTimer splitTimer;
    private float benchTime = 0f;
    private int benchExecs = 0;
    public bool resetBench = false;
    public float execTimeAvg = 0f;
    private Mesh mesh;
    public bool animating = true;

    private void Awake() {
      mesh = new Mesh();
      mesh.name = "Segmented Rounded Rect";
      GetComponent<MeshFilter>().sharedMesh = mesh;
      SetPath(RoundedRectPath(rectWidth, rectLen, cornerRad));
    }

    public void SpawnMesh_DEBUG() {
#if UNITY_EDITOR
      if (mesh == null) Awake();
#endif
    }

    private Curve[] RoundedRectPath(float w, float l, float cr) {
      if (starShape) {
        Vector3 v1 = new Vector3(0, 1, 1);
        Vector3 v2 = new Vector2(0.951f, 0.309f);
        Vector3 v3 = new Vector2(0.588f, -0.809f);
        Vector3 v4 = new Vector2(-0.588f, -0.809f);
        Vector3 v5 = new Vector2(-0.951f, 0.309f);
        float amt = Mathf.Clamp01(cornerRad);
        Vector3 v13 = Vector3.Lerp(Vector3.Lerp(v1, v3, 0.5f), Vector3.zero, amt);
        Vector3 v35 = Vector3.Lerp(Vector3.Lerp(v3, v5, 0.5f), Vector3.zero, amt);
        Vector3 v52 = Vector3.Lerp(Vector3.Lerp(v5, v2, 0.5f), Vector3.zero, amt);
        Vector3 v24 = Vector3.Lerp(Vector3.Lerp(v2, v4, 0.5f), Vector3.zero, amt);
        Vector3 v41 = Vector3.Lerp(Vector3.Lerp(v4, v1, 0.5f), Vector3.zero, amt);
        return new Curve[] {
        new Curve(v1, v13, v13, v3),
        new Curve(v3, v35, v35, v5),
        new Curve(v5, v52, v52, v2),
        new Curve(v2, v24, v24, v4),
        new Curve(v4, v41, v41, v1),
      };
      }

      float hcr = cr / 2f;
      float hw = w / 2f;
      float hl = l / 2f;
      return new Curve[] {
      new Curve(new Vector2(cr - hw, -hl), new Vector2(hw - cr, -hl)),
      new Curve(new Vector2(hw - cr, -hl), new Vector2(hw - cr + hcr, -hl), new Vector2(hw, hcr - hl), new Vector2(hw, cr - hl)),

      new Curve(new Vector2(hw, cr - hl), new Vector2(hw, hl - cr)),
      new Curve(new Vector2(hw, hl - cr), new Vector2(hw, hl - hcr), new Vector2(hw - hcr, hl), new Vector2(hw - cr, hl)),

      new Curve(new Vector2(hw - cr, hl), new Vector2(cr - hw, hl)),
      new Curve(new Vector2(cr - hw, hl), new Vector2(hcr - hw, hl), new Vector2(-hw, hl - hcr), new Vector2(-hw, hl - cr)),

      new Curve(new Vector2(-hw, hl - cr), new Vector2(-hw, cr - hl)),
      new Curve(new Vector2(-hw, cr - hl), new Vector2(-hw, hcr - hl), new Vector2(hcr - hw, -hl), new Vector2(cr - hw, -hl)),
    };
    }

    public void Update() {
      string newStr = SystemExtensions.PublicParamsString(typeof(SegmentedRoundedRect), this);
      bool shouldForce = false;
      if (newStr != lastString) {
        shouldForce = true;
        newStr = SystemExtensions.PublicParamsString(typeof(SegmentedRoundedRect), this);
        SetPath(RoundedRectPath(rectWidth, rectLen, cornerRad));
      }

      if (resetBench) {
        benchTime = 0f;
        benchExecs = 0;
        resetBench = false;
      }

      if ((animSpeed > 0 && animating) || shouldForce) {
        DateTime pre = DateTime.Now;
        GenMesh();
        double execTime = DateTime.Now.Subtract(pre).TotalMilliseconds;
        benchTime += (float)execTime;
        benchExecs++;
        execTimeAvg = benchTime / (float)benchExecs;
      }

      lastString = newStr;
    }

    public void SetPath(Curve[] curves) {
      this.curves = curves;

      int segAndGapCount = segsPerPiece + gapInSegs;
      int segsCount = numPieces * segAndGapCount;
      float totalDistance = curves.Sum(c => c.FastLength());
      offset = (offset + (animSpeed / 100f)).WrapAround(segAndGapCount);
      float cropOffset = offset % 1f;

      linearPointsBase = LeafVeins.CalcLinearPoints(
        curves.ToList().ConvertAll<Curve3D>(c => c).ToList(),
        renderLineSteps: segsCount,
        targetIncrement: totalDistance / (float)segsCount,
        tryDupeLast: false
      );
    }

    public void GenMesh() {
      if (curves == null || linearPointsBase == null) return;

      List<Vector3> verts = new List<Vector3>();
      List<Vector3> vertsSides = new List<Vector3>();
      List<Vector3> vertsEnds = new List<Vector3>();
      List<int> tris = new List<int>();
      List<int> trisSides = new List<int>();
      List<int> trisEnds = new List<int>();

      int segAndGapCount = segsPerPiece + gapInSegs;
      float cropOffset = offset % 1f;
      List<Vector3> lps = linearPointsBase.ToList();

      for (int i = 0; i < lps.Count - 1; i++) {
        lps[i] = Vector3.Lerp(lps[i], lps[i + 1], cropOffset);
      }
      lps[lps.Count - 1] = Vector3.Lerp(lps[lps.Count - 1], linearPointsBase[0], cropOffset);

      for (int i = 1; i <= offset; i++) {
        lps.Add(lps.Unshift());
      }
      Vector3[] linearPoints = lps.ToArray();
      int last = linearPoints.Length - 1;
      float w2 = pieceDepth / 2f;
      float h2 = pieceHeight / 2f;

      for (int pieceIdx = 0; pieceIdx < linearPoints.Length; pieceIdx++) {
        float tan = 0f;
        if (pieceIdx > 0 && pieceIdx < linearPoints.Length - 1) {
          tan = CurveHelpers.MiddlePointTangent(linearPoints[pieceIdx - 1], linearPoints[pieceIdx], linearPoints[pieceIdx + 1]);
        }
        if (resetBench) tan = 0f;
        Vector3 p = linearPoints[pieceIdx];

        p = new Vector3(p.x, 0, p.y);

        int pieceMod = pieceIdx % segAndGapCount;
        bool isPieceStart = pieceMod == 0;
        bool isPieceEnd = pieceMod == segsPerPiece;
        bool shouldDrawTris = pieceMod <= segsPerPiece && pieceMod != 0;
        if (gapInSegs == 0) shouldDrawTris = pieceIdx != 0;

        if (pieceIdx > 0 && pieceIdx < linearPoints.Length - 1) {
          if (CurveHelpers.Angle(linearPoints[pieceIdx - 1], linearPoints[pieceIdx]).SoftEquals(
              CurveHelpers.Angle(linearPoints[pieceIdx], linearPoints[pieceIdx + 1]), 0.001f) &&
              !isPieceStart && !isPieceEnd) {
            continue;
          }
        }

        verts.Add(p + new Vector3(0, 0, -w2).Rotate(0, tan, 0, Vector3.zero).WithY(0)); //inside bottom
        verts.Add(p + new Vector3(0, 0, w2).Rotate(0, tan, 0, Vector3.zero).WithY(0)); //outside bottom
        verts.Add(p + new Vector3(0, 0, -w2).Rotate(0, tan, 0, Vector3.zero).WithY(pieceHeight)); //inside top
        verts.Add(p + new Vector3(0, 0, w2).Rotate(0, tan, 0, Vector3.zero).WithY(pieceHeight)); //outside top

        // Debug.Log("pieceIdx: " + pieceIdx + " | pieceMod: " + pieceMod + " | shouldDrawTris: " + shouldDrawTris + " | p: " + p);
        if (shouldDrawTris) {
          int pos = verts.Count - 4;
          if (use3D) {
            tris.AddRange(new int[] {
            pos - 4, pos, pos - 3, //bottom
            pos - 3, pos, pos + 1,
            pos - 2, pos - 1, pos + 2, //top
            pos + 2, pos - 1, pos + 3,
          });

          } else {
            tris.AddRange(new int[] {
            pos - 2, pos - 1, pos + 2, //top
            pos + 2, pos - 1, pos + 3,
          });
          }
        }

        //second pass for better smoothing
        if (use3D) {
          List<Vector3> range = verts.GetRange(verts.Count - 4);
          vertsSides.AddRange(range);
          if (shouldDrawTris) {
            int pos = vertsSides.Count - 4;
            trisSides.AddRange(new int[] {
            pos - 4, pos - 2, pos - 0, //inside
            pos - 0, pos - 2, pos + 2,
            pos - 3, pos + 1, pos - 1, //outside
            pos + 1, pos + 3, pos - 1,
          });
          }
        }

        if (use3D) {
          if (isPieceEnd) {
            vertsEnds.AddRange(verts.GetRange(verts.Count - 4));
            int end = vertsEnds.Count - 4;
            trisEnds.AddRange(new int[] {
            end, end + 2, end + 1,
            end + 1, end + 2, end + 3
          });
          } else if (isPieceStart) {
            vertsEnds.AddRange(verts.GetRange(verts.Count - 4));
            int end = vertsEnds.Count - 4;
            trisEnds.AddRange(new int[] {
            end, end + 1, end + 2,
            end + 1, end + 3, end + 2
          });
          }
        }
      } //end main loop

      if (vertsSides.Count > 0) {
        int mainCount = verts.Count;
        for (int i = 0; i < trisSides.Count; i++) trisSides[i] += mainCount;
        verts.AddRange(vertsSides);
        tris.AddRange(trisSides);
      }

      if (vertsEnds.Count > 0) {
        int mainCount = verts.Count;
        for (int i = 0; i < trisEnds.Count; i++) trisEnds[i] += mainCount;
        verts.AddRange(vertsEnds);
        tris.AddRange(trisEnds);
      }

      if (gapInSegs == 0) {
        int pos = linearPoints.Length * 4;
        tris.AddRange(new int[] {
        pos - 2, pos - 1, 2, //top
        2, pos - 1, 3,
      });
      }

      // if (!smooth) (Vector3[] vs, int[] ts) = RemakeMeshToDiscrete(verts.ToArray(), tris.ToArray());
      mesh.Clear();
      mesh.SetVertices(verts);
      mesh.SetTriangles(tris, 0);
      mesh.RecalculateBounds();
      mesh.RecalculateNormals();
    }

    private static (Vector3[] verts, int[] tris) RemakeMeshToDiscrete(Vector3[] vert, int[] trig) {
      Vector3[] vertDiscrete = new Vector3[trig.Length];
      int[] trigDiscrete = new int[trig.Length];
      for (int i = 0; i < trig.Length; i++) {
        vertDiscrete[i] = vert[trig[i]];
        trigDiscrete[i] = i;
      }
      return (vertDiscrete, trigDiscrete);
    }

    public void SetActive(bool active) => gameObject.SetActive(active);
  }
}
