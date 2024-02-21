using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
  [ExecuteInEditMode]
  [RequireComponent(typeof(MeshRenderer))]
  [RequireComponent(typeof(MeshFilter))]
  public class SizedQuad : MonoBehaviour {
    public float length = 0.04f;
    public float width = 0.02f;
    private string lastString = "";
    private Mesh mesh;

    private void Awake() {
      mesh = new Mesh();
      mesh.name = "SizedQuad";
      GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    public void Update() {
      string newStr = SystemExtensions.PublicParamsString(typeof(SizedQuad), this);
      if (newStr != lastString) {
        mesh = GetComponent<MeshFilter>().sharedMesh;
        GenMesh();
      }
      lastString = newStr;
    }

    public void GenMesh() {
      Vector3[] verts = new Vector3[] {
      new Vector3(-width, 0f, -length),
      new Vector3(width, 0f, -length),
      new Vector3(width, 0f, length),
      new Vector3(-width, 0f, length)
    };
      int[] tris = new int[] {
      0, 2, 1,
      3, 2, 0
    };

      mesh.Clear();
      mesh.vertices = verts;
      mesh.triangles = tris;
      mesh.uv = new Vector2[] {
      new Vector2(1, 0),
      new Vector2(1, 1),
      new Vector2(0, 1),
      Vector2.zero
    };
      mesh.RecalculateBounds();
      mesh.RecalculateNormals();
    }
  }
}
