using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Unity.Collections;
using UnityEngine;

namespace BionicWombat {
  [Serializable]
  public struct SerializedMesh {
    public string name;
    public Vector3[] verts;
    public int[] tris;
    public Vector2[] uv;
    public Matrix4x4[] bindposes;
    public BoneWeight1[] boneWeights;
    public int[] bonesPerVertex;

    public SerializedMesh(Mesh mesh) {
      name = mesh.name;
      verts = mesh.vertices;
      tris = mesh.triangles;
      uv = mesh.uv;
      bindposes = mesh.bindposes;
      boneWeights = mesh.GetAllBoneWeights().ToArray();
      bonesPerVertex = mesh.GetBonesPerVertex().ToList().Select(b => (int)b).ToArray();
    }

    public Mesh GetMesh() {
      Mesh m = new Mesh();
      m.name = name;
      m.SetVertices(verts);
      m.SetTriangles(tris, 0);
      m.SetUVs(0, uv);
      m.bindposes = bindposes;

      var bonesPerVertexArray = new NativeArray<byte>(bonesPerVertex.ToList().Select(i => (byte)i).ToArray(), Allocator.Temp);
      var weightsArray = new NativeArray<BoneWeight1>(boneWeights, Allocator.Temp);
      m.SetBoneWeights(bonesPerVertexArray, weightsArray);
      bonesPerVertexArray.Dispose();
      weightsArray.Dispose();

      return m;
    }
  }

}
