using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BionicWombat {
  [ExecuteInEditMode]
  public class DebugTriangleNormals : MonoBehaviour {

    NormalVector[] verts;
    NormalVector[] faces;

    [SerializeField] Color vertsColor = Color.cyan;
    [SerializeField] Color facesColor = Color.magenta;

    public void SetVertexNormals(NormalVector[] verts) {
      this.verts = verts;
    }

    public void SetSurfaceNormals(NormalVector[] faces) {
      this.faces = faces;
    }

    public void SetNormals(NormalVector[] verts, NormalVector[] faces) {
      this.verts = verts;
      this.faces = faces;
    }

    public void ClearNormals() => SetNormals(null, null);


#if UNITY_EDITOR
    [SerializeField] float m_normalLineLength = 1f;
    private void OnDrawGizmos() {
      void DrawWithColor(NormalVector[] vecs, Color c) {
        if (vecs == null) return;
        Gizmos.color = c;
        for (int i = 0; i < vecs.Length; i++) {
          //Gizmos.DrawSphere(origins[i], m_normalSpotRadius);
          Gizmos.DrawLine(vecs[i].origin, vecs[i].origin + vecs[i].normal * m_normalLineLength);
        }
        Gizmos.color = Color.white;
      };

      DrawWithColor(verts, vertsColor);
      DrawWithColor(faces, facesColor);
    }
#endif


  }
}
