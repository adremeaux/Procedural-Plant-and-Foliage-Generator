using System.Linq;
using UnityEngine;

namespace BionicWombat {
  public static class MeshExtensions {
    public static Mesh Copy(this Mesh mesh) {
      Mesh newMesh = new Mesh();
      newMesh.subMeshCount = mesh.subMeshCount;
      newMesh.SetVertices(mesh.vertices);
      for (int i = 0; i < mesh.subMeshCount; i++)
        newMesh.SetTriangles(mesh.GetTriangles(i), i);
      newMesh.uv = mesh.uv;
      newMesh.normals = mesh.normals;
      newMesh.colors = mesh.colors;
      newMesh.tangents = mesh.tangents;
      newMesh.name = mesh.name + " Copy";
      return newMesh;
    }
  }


}
