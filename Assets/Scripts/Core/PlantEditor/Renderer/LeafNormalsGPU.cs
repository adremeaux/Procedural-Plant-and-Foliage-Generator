using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BionicWombat {
  public class LeafNormalsGPU : ComputeShaderBridge {
    public ComputeBuffer vertsBuffer;
    public ComputeBuffer trianglesBuffer;
    private ComputeBuffer surfaceNormalsBufferRW;
    private ComputeBuffer surfaceCentersBufferRW;
    public ComputeBuffer vertexNormalsBufferRW;

    private ComputeBuffer debugBufferFloat;
    private ComputeBuffer debugBufferFloat3;
    private int normalsCount = 0;

    public void Dispose() {
      ReleaseBuffer(ref vertsBuffer);
      ReleaseBuffer(ref trianglesBuffer);
      ReleaseBuffer(ref surfaceNormalsBufferRW);
      ReleaseBuffer(ref surfaceCentersBufferRW);
      vertexNormalsBufferRW = null;

      ReleaseBuffer(ref debugBufferFloat);
      ReleaseBuffer(ref debugBufferFloat3);
    }

    public void CalculateNormals(ComputeShader compute, MeshData extrudeData, ComputeBuffer deltaVertsBuffer, int instances) {
      if (deltaVertsBuffer == null) return;
      int baseVertsCount = extrudeData.vertices.Count();
      int baseTrisCount = extrudeData.triangles.Count();
      normalsCount = baseTrisCount / 3;
      int instancesVertsCount = baseVertsCount * instances;
      int instancesNormalsCount = normalsCount * instances;

      Vector3[] vertBlanks = new Vector3[instancesVertsCount];
      Array.Fill(vertBlanks, Vector3.back);

      vertsBuffer = MakeBuffer<Vector3>(baseVertsCount, extrudeData.vertices);
      trianglesBuffer = MakeBuffer<int>(baseTrisCount, extrudeData.triangles);
      surfaceNormalsBufferRW = MakeBuffer<Vector3>(instancesNormalsCount, new Vector3[instancesNormalsCount]);
      surfaceCentersBufferRW = MakeBuffer<Vector3>(instancesNormalsCount, new Vector3[instancesNormalsCount]);
      vertexNormalsBufferRW = MakeBuffer<Vector3>(instancesVertsCount, vertBlanks);
      debugBufferFloat = MakeBuffer<float>(baseVertsCount, null);
      debugBufferFloat3 = MakeBuffer<Vector3>(baseVertsCount, null);

      compute.SetFloat("_Instances", instances);
      void SetBuffers(int kernel) {
        compute.SetBuffer(kernel, "_Verts", vertsBuffer);
        compute.SetBuffer(kernel, "_Triangles", trianglesBuffer);
        compute.SetBuffer(kernel, "_DeltaVerts", deltaVertsBuffer);
        compute.SetBuffer(kernel, "_SurfaceNormalsRW", surfaceNormalsBufferRW);
        compute.SetBuffer(kernel, "_SurfaceCentersRW", surfaceCentersBufferRW);
        compute.SetBuffer(kernel, "_VertexNormalsRW", vertexNormalsBufferRW);
        compute.SetBuffer(kernel, "_DebugFloat", debugBufferFloat);
        compute.SetBuffer(kernel, "_DebugFloat3", debugBufferFloat3);
      };

      int kernel = compute.FindKernel("CalculateSurfaceNormals");
      uint tx, ty, tz;
      compute.GetKernelThreadGroupSizes(kernel, out tx, out ty, out tz);
      SetBuffers(kernel);
      compute.Dispatch(kernel, Mathf.Max(1, Mathf.FloorToInt(normalsCount / tx) + 1), (int)ty, (int)tz);

      // Debug.Log("baseVertsCount: " + baseVertsCount + " | baseTrisCount: " + baseTrisCount + " | normalsCount: " + normalsCount);
      // for (int i = 0; i < baseVertsCount; i++) {
      //   BatchLogger.Log(i + ":" + extrudeData.vertices[i], "i", 500);
      // }
      // Debug.Log("extrudeData.vertices: " + extrudeData.vertices.ToLog());
      // extrudeData.vertices.fi

      // Vector3[] surfNorms = ReadData<Vector3>(surfaceNormalsBufferRW, instancesNormalsCount);
      // for (int i = 0; i < surfNorms.Length; i++) {
      //   if (i % normalsCount == 0) BatchLogger.Flush();
      //   BatchLogger.Log("" + i + ":" + surfNorms[i], "surfaceNormalsBufferRW", 100000);
      // }
      // BatchLogger.Flush();
      // Vector3[] surfCents = ReadData<Vector3>(surfaceCentersBufferRW, instancesNormalsCount);
      // for (int i = 0; i < surfCents.Length; i++) {
      //   if (i % normalsCount == 0) BatchLogger.Flush();
      //   BatchLogger.Log("" + i + ":" + surfCents[i], "surfaceCentersBufferRW", 100000);
      // }
      // BatchLogger.Flush();

      // Debug.Log("(calls)baseVertsCount: " + baseVertsCount + " | (vertexNormalsBufferRW) instanceVertsCount: " + instancesVertsCount);
      kernel = compute.FindKernel("CalculateVertexNormals");
      compute.GetKernelThreadGroupSizes(kernel, out tx, out ty, out tz);
      SetBuffers(kernel);
      compute.Dispatch(kernel, Mathf.Max(1, Mathf.FloorToInt(baseVertsCount / tx) + 1), (int)ty, (int)tz);

      // Vector3[] sf = ReadData<Vector3>(vertexNormalsBufferRW, instancesVertsCount);
      // for (int i = 0; i < sf.Length; i++) {
      //   if (i % baseVertsCount == 0) BatchLogger.Flush();
      //   BatchLogger.Log("" + i + ":" + sf[i], "vertexNormalsBufferRW", 100000);
      // }
      // BatchLogger.Flush();

      // Vector3[] dl3 = ReadData<Vector3>(debugBufferFloat3, baseVertsCount);
      // Debug.Log("dl3: " + dl3.ToLog());
    }

    public NormalVector[] GetSurfaceNormals(int instanceIdx) {
      Vector3[] origins = ReadData<Vector3>(surfaceCentersBufferRW, normalsCount * (instanceIdx + 1));
      Vector3[] normals = ReadData<Vector3>(surfaceNormalsBufferRW, normalsCount * (instanceIdx + 1));
      NormalVector[] nv = new NormalVector[normalsCount];
      int offset = instanceIdx * normalsCount;
      for (int i = 0; i < normalsCount; i++)
        nv[i] = new NormalVector(origins[i + offset], normals[i + offset]);
      return nv;
    }
  }

}
