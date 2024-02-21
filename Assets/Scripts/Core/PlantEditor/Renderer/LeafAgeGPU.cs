using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BionicWombat {
  // file:///C:/Users/adrem/Plants_Game/Assets/Scripts/Tools/MakeStruct.html?a=AgeSpotGPU Vector2 pos float size
  [StructLayout(LayoutKind.Sequential)]
  public struct AgeSpotGPU {
    public Vector2 pos;
    public float size;

    public AgeSpotGPU(Vector2 pos, float size) {
      this.pos = pos;
      this.size = size;
    }

    public override string ToString() {
      return "[AgeSpot] pos: " + pos + " | size: " + size;
    }
  }

  public class LeafAgeGPU : ComputeShaderBridge {
    public ComputeBuffer ageSpots;
    private Vector3[] baseVerts;

    public LeafAgeGPU(LeafFactoryData lfd, LeafDeps deps) {
      int count = 1;
      ageSpots = new ComputeBuffer(count, Marshal.SizeOf<AgeSpotGPU>());
      AgeSpotGPU[] arr = new AgeSpotGPU[count];

      BWRandom.SetSeed(BWRandomPlantShop.GenTypedSeed(deps.leafData.randomSeed, LPType.Material));
      for (int i = 0; i < count; i++) {
        LeafShape.LeftyCheck left = BWRandom.Bool() ? LeafShape.LeftyCheck.Left : LeafShape.LeftyCheck.Right;
        LeafCurve curve = LeafShape.GetCurve(LeafCurveType.Tip, lfd.leafShape.curves, left);
        Vector3 point = curve.GetPoint(BWRandom.Range(0f, 1f));
        float size = BWRandom.Range(0f, 0.1f);
        point = new Vector3(0.5f, 0.5f, 0f); size = 0.1f;
        arr[i] = new AgeSpotGPU(lfd.NormalizePoint(point), size);
      }

      // Debug.Log("arr: " + arr.ToLog());
      ageSpots.SetData(arr);
    }

    public void Dispose() {
      ReleaseBuffer(ref ageSpots);
    }

    private void LogBuffers(DistortionCurve c = null) {
      // if (false) {
      //   Debug.Log("    LeafDISTORTGPU Logs");
      //   Debug.Log("baseVerts.Count(): " + baseVerts.Count());
      //   if (c != null) {
      //     Curve3DGPU[] c3DDebug = new Curve3DGPU[c.influenceCurves.Count()];
      //     debugBufferCurve3D.GetData(c3DDebug);
      //     Debug.Log("[GPU] debugBufferCurve3D: " + c3DDebug.ToLog());
      //   }

      //   float[] fDebug = new float[baseVerts.Count()];
      //   debugBufferFloat.GetData(fDebug);
      //   Debug.Log("[GPU] debugBufferFloat: " + fDebug.ToLog());

      // Vector3[] f3Debug = new Vector3[baseVerts.Count()];
      // debugBufferFloat3.GetData(f3Debug);
      // Debug.Log("[GPU] debugBufferFloat3: " + f3Debug.ToLog());
      // }
    }
  }
}
