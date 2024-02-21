using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BionicWombat {
  public class LeafDistortGPU : ComputeShaderBridge {
    private float[] vertDistances;

    private ComputeBuffer baseVertsBufferRead;
    public ComputeBuffer deltaVertsRW;

    private ComputeBuffer influenceCurvesBuffer;
    private ComputeBuffer midribBuffer;
    private ComputeBuffer dcDataBuffer;
    private ComputeBuffer distortionPointsBuffer;
    private ComputeBuffer distortionOffsetsBuffer;
    public ComputeBuffer extrudeDataBufferRW;

    private ComputeBuffer debugBufferFloat;
    private ComputeBuffer debugBufferFloat3;
    private ComputeBuffer debugBufferCurve3D;

    private Vector3[] baseVerts;
    public int deltaVertsLen;

    public LeafDistortGPU(Vector3[] verts) {
      this.baseVerts = verts;

      baseVertsBufferRead = new ComputeBuffer(verts.Count(), Marshal.SizeOf<Vector3>());
      baseVertsBufferRead.SetData(verts);

      midribBuffer = new ComputeBuffer(1, Marshal.SizeOf<Curve3DGPU>());
      dcDataBuffer = new ComputeBuffer(1, Marshal.SizeOf<DistortionCurveData>());
      extrudeDataBufferRW = new ComputeBuffer(1, Marshal.SizeOf<MeshData>());

      debugBufferFloat = new ComputeBuffer(verts.Count(), Marshal.SizeOf<float>());
      debugBufferFloat3 = new ComputeBuffer(verts.Count(), Marshal.SizeOf<Vector3>());
      debugBufferCurve3D = new ComputeBuffer(verts.Count(), Marshal.SizeOf<Curve3DGPU>());
    }

    public void Dispose() {
      ReleaseBuffer(ref baseVertsBufferRead);
      deltaVertsRW = null;

      ReleaseBuffer(ref influenceCurvesBuffer);
      ReleaseBuffer(ref midribBuffer);
      ReleaseBuffer(ref dcDataBuffer);
      ReleaseBuffer(ref distortionPointsBuffer);
      ReleaseBuffer(ref distortionOffsetsBuffer);
      ReleaseBuffer(ref extrudeDataBufferRW);

      ReleaseBuffer(ref debugBufferFloat);
      ReleaseBuffer(ref debugBufferFloat3);
      ReleaseBuffer(ref debugBufferCurve3D);
    }

    public void Distort(ComputeShader compute, DistortionCurve[][] dCurves, Curve midrib, LeafParamDict fields, bool shouldDistort, float leafWidth) {
      int instances = dCurves.Count();
      int distortionsCount = dCurves[0].Count();

      deltaVertsLen = baseVerts.Count() * instances;
      deltaVertsRW = new ComputeBuffer(deltaVertsLen, Marshal.SizeOf<Vector3>());
      Vector3[] empties = new Vector3[deltaVertsLen];
      Array.Fill(empties, Vector3.zero);
      deltaVertsRW.SetData(empties);

      int kernel = compute.FindKernel("DistortOnCurve");
      uint tx, ty, tz;
      compute.GetKernelThreadGroupSizes(kernel, out tx, out ty, out tz);
      compute.SetBuffer(kernel, "_BaseVertsRead", baseVertsBufferRead);
      compute.SetBuffer(kernel, "_DeltaVertsRW", deltaVertsRW);
      compute.SetBuffer(kernel, "_DCData", dcDataBuffer);
      compute.SetBuffer(kernel, "_DebugFloat", debugBufferFloat);
      compute.SetBuffer(kernel, "_DebugFloat3", debugBufferFloat3);
      compute.SetBuffer(kernel, "_DebugCurve3D", debugBufferCurve3D);

      midribBuffer.SetData(new Curve3DGPU[] { new Curve3DGPU(midrib.p0, midrib.h0, midrib.h1, midrib.p1) });
      compute.SetBuffer(kernel, "_Midrib", midribBuffer);

      for (int distIdx = 0; distIdx < distortionsCount; distIdx++) {
        DistortionCurve[] instancedCurves = dCurves.Select((DistortionCurve[] d) => d[distIdx]).ToArray(); //all of the waves, for example [0-instances]
        DistortionCurve firstInstance = instancedCurves[0];

        // Curve3DGPU[] curve3DGPUs = new Curve3DGPU[firstInstance.influenceCurves.Count() * instances];
        // for (int bufferCurveIdx = 0; bufferCurveIdx < curve3DGPUs.Count(); bufferCurveIdx++) {
        //   int floor = (bufferCurveIdx / instances);
        //   int remainder = bufferCurveIdx % instances;
        //   Curve3D[] inflCurves = instancedCurves[floor].influenceCurves;
        //   curve3DGPUs[bufferCurveIdx] = (Curve3DGPU)inflCurves[remainder];
        // }

        // influenceCurvesBuffer = new ComputeBuffer(curve3DGPUs.Count(), Marshal.SizeOf(typeof(Curve3DGPU)));
        // influenceCurvesBuffer.SetData(curve3DGPUs);
        // compute.SetBuffer(kernel, "_InfluenceCurves", influenceCurvesBuffer);
        // Debug.Log("firstInstance.influenceCurves.Count(): " + firstInstance.influenceCurves.Count());
        influenceCurvesBuffer = new ComputeBuffer(firstInstance.influenceCurves.Count(), Marshal.SizeOf(typeof(Curve3DGPU)));
        influenceCurvesBuffer.SetData(Array.ConvertAll<Curve3D, Curve3DGPU>(firstInstance.influenceCurves, c => (Curve3DGPU)c));
        compute.SetBuffer(kernel, "_InfluenceCurves", influenceCurvesBuffer);

        //since each dCurve can have a different distortionPoints count, figure out where the offsets are and buffer them
        int[] lengths = Array.ConvertAll<DistortionCurve, int>(instancedCurves, dc => dc.distortionPoints.Count());
        int total = lengths.Sum();
        int[] distortionOffsets = new int[instances];
        distortionOffsets[0] = 0;
        for (int i = 1; i < instances; i++) distortionOffsets[i] = lengths[i - 1] + distortionOffsets[i - 1];

        distortionOffsetsBuffer = new ComputeBuffer(instances, Marshal.SizeOf<int>());
        distortionOffsetsBuffer.SetData(distortionOffsets);
        compute.SetBuffer(kernel, "_DistortionOffsets", distortionOffsetsBuffer);

        Vector3[] dPoints = new Vector3[total];
        for (int bufferDPointIdx = 0; bufferDPointIdx < dPoints.Count(); bufferDPointIdx++) {
          int floor = Array.FindIndex(distortionOffsets, val => bufferDPointIdx < val);
          if (floor == -1) floor = instances - 1;
          else floor--;

          int distPointsOffset = bufferDPointIdx - distortionOffsets[floor];
          // Debug.Log("floor: " + floor + " | instancedCurves.Count(): " + instancedCurves.Count());
          Vector3[] distPoints = instancedCurves[floor].distortionPoints;
          if (distPointsOffset >= distPoints.Length)
            Debug.Log("distPoints.Length: " + distPoints.Length + " | distPointsOffset: " + distPointsOffset +
                      " | bufferDPointIdx: " + bufferDPointIdx);
          dPoints[bufferDPointIdx] = distPoints[distPointsOffset];
        }

        distortionPointsBuffer = new ComputeBuffer(dPoints.Count(), Marshal.SizeOf<Vector3>());
        distortionPointsBuffer.SetData(dPoints);
        compute.SetBuffer(kernel, "_DistortionPoints", distortionPointsBuffer);

        dcDataBuffer.SetData(new DistortionCurveData[] {
      new DistortionCurveData(
          firstInstance.shouldFade,
          firstInstance.config.useDistFade,
          firstInstance.config.type == LeafDistortionType.Cup ? 1u : 0u,
          firstInstance.config.maxFadeDist,
          firstInstance.config.reverseFade,
          firstInstance.config.skipOutsideLowerBound,
          (uint)firstInstance.config.affectAxes,
          leafWidth,
          fields[LPK.DistortCupClamp].value,
          instances
        )
      });

        //DISPATCH
        compute.Dispatch(kernel, Mathf.FloorToInt(baseVerts.Count() / tx) + 1, (int)ty, (int)tz);

        // if (firstInstance.config.type == LeafDistortionType.Flop) {
        //   Vector3[] f3Debug = new Vector3[baseVerts.Count()];
        //   debugBufferFloat3.GetData(f3Debug);

        //   float[] fDebug = new float[baseVerts.Count()];
        //   debugBufferFloat.GetData(fDebug);

        //   Vector3[] deltas = new Vector3[baseVerts.Count()];
        //   deltaVertsRW.GetData(deltas);
        //   for (int i = 0; i < 10; i++) {
        //     Debug.Log("base: " + baseVerts[i] + " | magnet: " + f3Debug[i] + " | final: " + (baseVerts[i] + deltas[i]) + " | lerpPoint: " + fDebug[i]);
        //   }
        // }

        // LogBuffers(dCurve);
        ReleaseBuffer(ref influenceCurvesBuffer);
        ReleaseBuffer(ref distortionPointsBuffer);
      }
    }

    // private void LogBuffers(DistortionCurve c = null) {
    //   if (false) {
    //     Debug.Log("    LeafDISTORTGPU Logs");
    //     Debug.Log("baseVerts.Count(): " + baseVerts.Count());
    //     if (c != null) {
    //       Curve3DGPU[] c3DDebug = new Curve3DGPU[c.influenceCurves.Count()];
    //       debugBufferCurve3D.GetData(c3DDebug);
    //       Debug.Log("[GPU] debugBufferCurve3D: " + c3DDebug.ToLog());
    //     }

    //     float[] fDebug = new float[baseVerts.Count()];
    //     debugBufferFloat.GetData(fDebug);
    //     Debug.Log("[GPU] debugBufferFloat: " + fDebug.ToLog());

    //   Vector3[] f3Debug = new Vector3[baseVerts.Count()];
    //   debugBufferFloat3.GetData(f3Debug);
    //   Debug.Log("[GPU] debugBufferFloat3: " + f3Debug.ToLog());
    //   }
    // }
  }
}
