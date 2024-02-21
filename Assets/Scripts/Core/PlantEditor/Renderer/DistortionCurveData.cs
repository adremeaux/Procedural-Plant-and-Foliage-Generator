using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BionicWombat {
  [StructLayout(LayoutKind.Sequential)]
  public struct DistortionCurveData {
    public uint shouldFade;
    public uint useDistFade;
    public uint isCup;
    public float maxFadeDist;
    public uint reverseFade;
    public uint skipOutsideLowerBound;
    public uint affectAxes;
    public float leafWidth;
    public float distClamp;
    public int instances; //how many instances of distortion we're applying
    public DistortionCurveData(bool shouldFade, bool useDistFade,
        uint isCup, float maxFadeDist, bool reverseFade,
        bool skipOutsideLowerBound, uint affectAxes,
        float leafWidth, float distClamp, int instances) {
      this.shouldFade = shouldFade ? 1u : 0u;
      this.useDistFade = useDistFade ? 1u : 0u;
      this.isCup = isCup;
      this.maxFadeDist = maxFadeDist;
      this.reverseFade = reverseFade ? 1u : 0u;
      this.skipOutsideLowerBound = skipOutsideLowerBound ? 1u : 0u; ;
      this.affectAxes = affectAxes;
      this.leafWidth = leafWidth;
      this.distClamp = distClamp;
      this.instances = instances;
    }
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct Curve3DGPU {
    public Vector3 p0;
    public Vector3 h0;
    public Vector3 h1;
    public Vector3 p1;
    public Curve3DGPU(Vector3 p0, Vector3 h0, Vector3 h1, Vector3 p1) {
      this.p0 = p0;
      this.h0 = h0;
      this.h1 = h1;
      this.p1 = p1;
    }
    public override string ToString() {
      return "[Curve3DGPU] p0: " + p0 + " | h0: " + h0 + " | h1: " + h1 + " | p1: " + p1;
    }
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct CurveGPU {
    public Vector2 p0;
    public Vector2 h0;
    public Vector2 h1;
    public Vector2 p1;
    public CurveGPU(Vector2 p0, Vector2 h0, Vector2 h1, Vector2 p1) {
      this.p0 = p0;
      this.h0 = h0;
      this.h1 = h1;
      this.p1 = p1;
    }
  }
}
