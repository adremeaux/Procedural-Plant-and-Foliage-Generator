using System.Collections.Generic;
using UnityEngine;

namespace BionicWombat {
#if UNITY_EDITOR
  [ExecuteInEditMode]
#endif
  [RequireComponent(typeof(MeshRenderer))]
  [RequireComponent(typeof(MeshFilter))]
  public class SegmentedRing : MonoBehaviour {
    public int numPieces = 7;
    public int segsPerPiece = 6;
    public float height = 0.02f;
    public float width = 0.04f;
    public float rad = .15f;
    public float gapInDegs = 18f;
    private string lastString = "";
    public bool redraw = false;
    public bool smooth = true;
    private Mesh mesh;

    private void Awake() {
      mesh = new Mesh();
      mesh.name = "Segmented Ring";
      GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    public void Update() {
      string newStr = SystemExtensions.PublicParamsString(typeof(SegmentedRing), this);
      if (newStr != lastString || redraw == true) {
        redraw = false;
        GenRing();
      }
      lastString = newStr;
    }

    public void GenRing() {
      List<Vector3> verts = new List<Vector3>();
      List<int> tris = new List<int>();

      float pieceBudget = (360f / numPieces);
      float step = (pieceBudget - gapInDegs) / (segsPerPiece);
      float curRotation = 0f;

      for (int pieceIdx = 0; pieceIdx < numPieces; pieceIdx++) {
        int start = verts.Count;
        for (int i = 0; i <= segsPerPiece; i++) {
          verts.Add(Vector3.zero.AddPolar(new Polar3(rad, 0f, curRotation + i * step, true))); //inside bottom           | p -4
          verts.Add(Vector3.zero.AddPolar(new Polar3(rad + width, 0f, curRotation + i * step, true))); //outside bottom  | p -3
          verts.Add(new Vector3(0, height, 0).AddPolar(new Polar3(rad, 0f, curRotation + i * step, true))); //inside top | p -2
          verts.Add(new Vector3(0, height, 0).AddPolar(new Polar3(rad + width, 0f, curRotation + i * step, true))); //outside top | p -1

          if (i != 0) {
            int pos = i * 4 + start;
            tris.AddRange(new int[] {
            pos - 4, pos - 2, pos - 0, //inside
            pos - 0, pos - 2, pos + 2,
            pos - 3, pos + 1, pos - 1, //outside
            pos + 1, pos + 3, pos - 1,
          });
          }
        }

        start = verts.Count;
        for (int i = 0; i <= segsPerPiece; i++) {
          verts.Add(Vector3.zero.AddPolar(new Polar3(rad, 0f, curRotation + i * step, true))); //inside bottom           | p -4
          verts.Add(Vector3.zero.AddPolar(new Polar3(rad + width, 0f, curRotation + i * step, true))); //outside bottom  | p -3
          verts.Add(new Vector3(0, height, 0).AddPolar(new Polar3(rad, 0f, curRotation + i * step, true))); //inside top | p -2
          verts.Add(new Vector3(0, height, 0).AddPolar(new Polar3(rad + width, 0f, curRotation + i * step, true))); //outside top | p -1

          if (i != 0) {
            int pos = i * 4 + start;
            tris.AddRange(new int[] {
            pos - 4, pos, pos - 3, //bottom
            pos - 3, pos, pos + 1,
            pos - 2, pos - 1, pos + 2, //top
            pos + 2, pos - 1, pos + 3,
          });
          }
        }

        verts.AddRange(verts.GetRange(verts.Count - 4));
        int end = verts.Count - 4;
        tris.AddRange(new int[] {
        end, end + 2, end + 1,
        end + 1, end + 2, end + 3
      });
        verts.AddRange(verts.GetRange(start, 4));
        end = verts.Count - 4;
        tris.AddRange(new int[] {
        end, end + 1, end + 2,
        end + 1, end + 3, end + 2
      });

        curRotation += pieceBudget;
      }

      mesh.Clear();
      (mesh.vertices, mesh.triangles) = RemakeMeshToDiscrete(verts.ToArray(), tris.ToArray(), smooth);
      mesh.RecalculateBounds();
      mesh.RecalculateNormals();
    }

    private static (Vector3[] verts, int[] tris) RemakeMeshToDiscrete(Vector3[] vert, int[] trig, bool smooth) {
      if (smooth) return (vert, trig);
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
