using System;
using System.Collections.Generic;
using UnityEngine;

namespace BionicWombat {
#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
[RequireComponent(typeof(MeshRenderer))]
public class SegmentedRing : MonoBehaviour {
  public int numPieces = 7;
  public int segsPerPiece = 6;
  public float height = 0.04f;
  public float width = 0.05f;
  public float rad = .15f;
  public float gapInDegs = 12f;
  private string lastString = "";
  Mesh m;

  public void Update() {
    string newStr = SystemExtensions.PublicParamsString(typeof(SegmentedRing), this);
    if (newStr != lastString) {
      m = GenRing();
      GetComponent<MeshFilter>().sharedMesh = m;
    }
    lastString = newStr;
  }

  public Mesh GenRing() {
    List<Vector3> verts = new List<Vector3>();
    List<int> tris = new List<int>();

    float pieceBudget = (360f / numPieces);
    float step = (pieceBudget - gapInDegs) / (segsPerPiece - 1f);
    float curRotation = 0f;

    for (int pieceIdx = 0; pieceIdx < numPieces; pieceIdx++) {
      int start = verts.Count;
      for (int i = 0; i < segsPerPiece; i++) {
        verts.Add(Vector3.zero.AddPolar(new Polar3(rad, 0f, curRotation + i * step, true))); //don't change the order!
        verts.Add(Vector3.zero.AddPolar(new Polar3(rad + width, 0f, curRotation + i * step, true)));
        verts.Add(new Vector3(0, height, 0).AddPolar(new Polar3(rad, 0f, curRotation + i * step, true)));
        verts.Add(new Vector3(0, height, 0).AddPolar(new Polar3(rad + width, 0f, curRotation + i * step, true)));

        if (i != 0) {
          int pos = i * 4 + start;
          tris.AddRange(new int[] {
          pos - 4, pos, pos - 3,
          pos - 3, pos, pos + 1,
          pos - 4, pos - 2, pos,
          pos, pos - 2, pos + 2,
          pos - 3, pos + 1, pos - 1,
          pos + 1, pos + 3, pos - 1,
          pos - 2, pos - 1, pos + 2,
          pos + 2, pos - 1, pos + 3,
        });
        }
      }
      int end = verts.Count - 4;
      tris.AddRange(new int[] {
        start, start + 1, start + 2,
        start + 1, start + 3, start + 2,
        end, end + 2, end + 1,
        end + 1, end + 2, end + 3
      });
      curRotation += pieceBudget;
    }

    Mesh m = new Mesh();
    m.name = "Ring";
    (m.vertices, m.triangles) = RemakeMeshToDiscrete(verts.ToArray(), tris.ToArray());
    m.RecalculateBounds();
    m.RecalculateNormals();
    return m;
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