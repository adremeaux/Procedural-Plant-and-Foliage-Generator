using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
  [RequireComponent(typeof(MeshRenderer))]
  [RequireComponent(typeof(MeshFilter))]
  public class PlusMesh : MonoBehaviour {
    public float size = 0.2f;
    public float pieceWidth = 0.04f;
    public float pieceHeight = 0.02f;
    private string lastString = "";
    public bool redraw = false;
    private Mesh mesh;

    private void Awake() {
      mesh = new Mesh();
      mesh.name = "Plus";
      GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    public void Update() {
      string newStr = SystemExtensions.PublicParamsString(typeof(PlusMesh), this);
      if (newStr != lastString) {
        redraw = false;
        GenMesh();
      }
      lastString = newStr;
    }

    public void GenMesh() {
      List<Vector3> verts = new List<Vector3>();
      List<int> tris = new List<int>();

      float rad = size / 2f;
      float w2 = pieceWidth / 2f;

      verts.Add(
        new Vector3(-w2, 0f, -rad), new Vector3(w2, 0f, -rad),
        new Vector3(w2, 0f, -w2),
        new Vector3(rad, 0f, -w2), new Vector3(rad, 0f, w2),
        new Vector3(w2, 0f, w2),
        new Vector3(w2, 0f, rad), new Vector3(-w2, 0f, rad),
        new Vector3(-w2, 0f, w2),
        new Vector3(-rad, 0f, w2), new Vector3(-rad, 0f, -w2),
        new Vector3(-w2, 0f, -w2)
      );

      tris.Add(
        1, 2, 0,
        2, 3, 4,
        4, 5, 2,
        6, 7, 5,
        8, 5, 7,
        9, 10, 8,
        10, 11, 8,
        11, 0, 2,
        11, 2, 8,
        2, 5, 8
      );

      int count = verts.Count;
      var topVerts = verts.Select(v => v.WithY(pieceHeight)).ToList();
      var topTris = tris.Select(t => t + count).ToList().Reversed();
      verts.AddRange(topVerts);
      tris.AddRange(topTris);

      void AddSquare(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
        int l = verts.Count;
        verts.Add(v1, v2, v3, v4);
        tris.Add(
          l, l + 2, l + 1,
          l + 1, l + 2, l + 3
        );
      };

      for (int i = 0; i < 11; i++)
        AddSquare(verts[i], verts[i + 1], verts[i + 12], verts[i + 13]);
      AddSquare(verts[11], verts[0], verts[23], verts[12]);

      mesh.Clear();
      mesh.SetVertices(verts);
      mesh.SetTriangles(tris, 0);
      mesh.RecalculateBounds();
      mesh.RecalculateNormals();
    }
  }
}
