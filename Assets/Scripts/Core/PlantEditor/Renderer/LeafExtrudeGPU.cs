using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BionicWombat {
  public class LeafExtrudeGPU : ComputeShaderBridge {
    private float[] vertDistances;

    private ComputeBuffer vertsBuffer;
    private ComputeBuffer orderedEdgeVertsBuffer;
    private ComputeBuffer trianglesBuffer;
    private ComputeBuffer uvBuffer;
    private ComputeBuffer colorsBuffer;

    private ComputeBuffer debugBufferFloat;
    private ComputeBuffer debugBufferFloat3;

    public void Dispose() {
      ReleaseBuffer(ref vertsBuffer);
      ReleaseBuffer(ref orderedEdgeVertsBuffer);
      ReleaseBuffer(ref trianglesBuffer);
      ReleaseBuffer(ref uvBuffer);
      ReleaseBuffer(ref colorsBuffer);

      ReleaseBuffer(ref debugBufferFloat);
      ReleaseBuffer(ref debugBufferFloat3);
    }

    public MeshData ExtrudeMesh(ComputeShader compute, MeshData extrudeData, float edgeDepth, float succThicc) {
      int newVertsCount = extrudeData.vertices.Count() * 2;
      int baseTrisCount = extrudeData.triangles.Count();
      int edgeVertsCount = extrudeData.orderedEdgeVerts.Count();
      int edgeTrisCount = edgeVertsCount * 6 * 2; //facing both directions
      int newTrisCount = extrudeData.triangles.Count() * 2 + edgeTrisCount;
      int normalsCount = baseTrisCount / 3;

      // Debug.Log("edgeVertsCount: " + edgeVertsCount + " | edgeVerts: " + extrudeData.orderedEdgeVerts.ToLog());
      // Debug.Log("vertices.Count: " + extrudeData.vertices.Count() + " | newVertsCount: " + newVertsCount
      //           + " | triangles.Count: " + extrudeData.triangles.Count() + " | newTrisCount: " + newTrisCount);
      // Debug.Log("extrudeData.vertices: " + extrudeData.vertices.ToLog());
      // Debug.Log("extrudeData.triangles: " + extrudeData.triangles.ToLogGrouped(3, true));

      void SetBuffers(int kernel) {
        compute.SetBuffer(kernel, "_VertsRW", vertsBuffer);
        compute.SetBuffer(kernel, "_OrderedEdges", orderedEdgeVertsBuffer);
        compute.SetBuffer(kernel, "_TrianglesRW", trianglesBuffer);
        compute.SetBuffer(kernel, "_UVsRW", uvBuffer);
        compute.SetBuffer(kernel, "_ColorsRW", colorsBuffer);
        compute.SetBuffer(kernel, "_DebugFloat", debugBufferFloat);
        compute.SetBuffer(kernel, "_DebugFloat3", debugBufferFloat3);
      };

      int[] triBufferData = new int[newTrisCount];
      for (int i = 0; i < newTrisCount; i++)
        triBufferData[i] = i < extrudeData.triangles.Length ? extrudeData.triangles[i] : 0;

      {
        int kernel = compute.FindKernel("ExtrudeMesh");
        uint tx, ty, tz;
        compute.GetKernelThreadGroupSizes(kernel, out tx, out ty, out tz);

        vertsBuffer = MakeBuffer<Vector3>(newVertsCount, extrudeData.vertices);
        orderedEdgeVertsBuffer = MakeBuffer<int>(extrudeData.orderedEdgeVerts.Count(), extrudeData.orderedEdgeVerts);
        trianglesBuffer = MakeBuffer<int>(newTrisCount, triBufferData);
        uvBuffer = MakeBuffer<Vector2>(newVertsCount, extrudeData.uv);
        colorsBuffer = MakeBuffer<Vector4>(newVertsCount, extrudeData.colors);

        debugBufferFloat = MakeBuffer<float>(newVertsCount, null);
        debugBufferFloat3 = MakeBuffer<Vector3>(newVertsCount, null);

        SetBuffers(kernel);

        compute.SetFloat("_EdgeDepth", edgeDepth);
        compute.SetFloat("_SuccThicc", succThicc);

        compute.Dispatch(kernel, Mathf.FloorToInt(extrudeData.vertices.Count() / tx) + 1, (int)ty, (int)tz);
        LogBuffers(newVertsCount, 0);
      }

      {
        int kernel = compute.FindKernel("CreateBackTriangles");
        uint tx, ty, tz;
        compute.GetKernelThreadGroupSizes(kernel, out tx, out ty, out tz);

        SetBuffers(kernel);

        compute.SetFloat("_BaseTrisCount", baseTrisCount);

        // Debug.Log("tx: " + tx + " | numX: " + Mathf.FloorToInt(baseTrisCount / tx));
        // Debug.Log("CreateBackTrianglesB: " + ReadData<int>(trianglesBuffer, newTrisCount).ToLogGrouped(3, true));
        compute.Dispatch(kernel, Mathf.FloorToInt(baseTrisCount / tx) + 1, (int)ty, (int)tz);
        LogBuffers(0, newTrisCount);
        // Debug.Log("CreateBackTrianglesA: " + ReadData<int>(trianglesBuffer, newTrisCount).ToLogGrouped(3, true));
      }

      {
        int kernel = compute.FindKernel("CreateEdgeTriangles");
        uint tx, ty, tz;
        compute.GetKernelThreadGroupSizes(kernel, out tx, out ty, out tz);

        SetBuffers(kernel);

        compute.SetFloat("_BaseTrisCount", baseTrisCount);

        compute.Dispatch(kernel, Mathf.Max(1, Mathf.FloorToInt(edgeVertsCount / tx) + 1), (int)ty, (int)tz);
        LogBuffers(0, newTrisCount);
        // Debug.Log("CreateEdgeTriangles: " + ReadData<int>(trianglesBuffer, newTrisCount).ToLogGrouped(3));
      }

      MeshData after = extrudeData;
      // after.orderedEdgeVerts = extrudeData.orderedEdgeVerts;
      after.vertices = ReadData<Vector3>(vertsBuffer, newVertsCount);
      after.triangles = ReadData<int>(trianglesBuffer, newTrisCount);
      after.uv = ReadData<Vector2>(uvBuffer, newVertsCount);
      after.colors = ReadData<Vector4>(colorsBuffer, newVertsCount);
      return after;
    }

    private void LogBuffers(int count, int triCount) {
      //   if (false) {
      //     Debug.Log("    LeafExtrudeGPU Logs");
      //     // float[] fDebug = new float[count];
      //     // debugBufferFloat.GetData(fDebug);
      //     // Debug.Log("[GPU] debugBufferFloat: " + fDebug.ToLog());

      //     // Vector3[] f3Debug = new Vector3[count];
      //     // debugBufferFloat3.GetData(f3Debug);
      //     // Debug.Log("[GPU] debugBufferFloat3: " + f3Debug.ToLog());

      //     Vector3[] vertsDebug = new Vector3[count];
      //     vertsBuffer.GetData(vertsDebug);
      //     Debug.Log("[GPU] vertsBuffer: " + vertsDebug.ToLog());

      //     if (triCount > 0) {
      //       int[] tDebug = new int[triCount];
      //       Debug.Log(triCount + " " + tDebug.Count());
      //       trianglesBuffer.GetData(tDebug);
      //       Debug.Log("[GPU] trianglesBuffer (last 1000): " + tDebug.ToList().GetRange(triCount - 1000, 1000).ToLog());
      //     }
      //   }
    }
  }
}
