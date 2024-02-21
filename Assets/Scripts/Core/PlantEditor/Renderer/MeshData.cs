using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BionicWombat {
  [StructLayout(LayoutKind.Sequential)]
  public struct MeshData {
    public Vector3[] vertices; //rw
    public int[] orderedEdgeVerts; //r
    public int[] triangles; //rw
    public Vector2[] uv; //rw
    public Vector4[] colors; //rw
    public float[] randomNumbers;
    public MeshData(Vector3[] vertices, int[] orderedEdgeVerts, int[] triangles, Vector2[] uv, Vector4[] colors, float[] randomNumbers) {
      this.vertices = vertices;
      this.orderedEdgeVerts = orderedEdgeVerts;
      this.triangles = triangles;
      this.uv = uv;
      this.colors = colors;
      this.randomNumbers = randomNumbers;
    }
    public override string ToString() {
      return "[MeshData] vertices: " + vertices + " | orderedEdgeVerts: " + orderedEdgeVerts + " | triangles: " + triangles + " | uv: " + uv + " | colors: " + colors + " | randomNumbers: " + randomNumbers;
    }

    public Color[] GetColors() => Array.ConvertAll<Vector4, Color>(colors,
      v => v.ToColor());

    public void SetColors(Color[] colors) =>
      this.colors = Array.ConvertAll<Color, Vector4>(colors, c => c.ToVector4());
  }
}
