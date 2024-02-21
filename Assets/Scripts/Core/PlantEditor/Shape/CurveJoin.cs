using System;
using System.Collections.Generic;
using UnityEngine;

namespace BionicWombat {
public struct CurveJoin {
  public static void FlattenAngle(LeafCurve c0, LeafCurve c1, LeafDeps deps) {
    float a = Angle(c0, c1);
    float adjust = (180f - a) / 2f;
    if (deps.logOptions.logAngles) {
      Debug.Log("c1.a0: " + c1.angle0 + " | " + "c0.a1: " + c0.angle1);
      Debug.Log("from  " + a + " adjust " + adjust);
    }
    float tryC1 = c1.angle0 - adjust;
    float tryC2 = c0.angle1 + adjust;
    float result = Math.Abs(tryC1 - tryC2);
    if (Math.Abs(180f - result) < 1f) {
      c1.angle0 -= adjust;
      c0.angle1 += adjust;
    } else {
      c1.angle0 += adjust;
      c0.angle1 -= adjust;
    }
    if (deps.logOptions.logAngles) Debug.Log("final angle: " + Angle(c0, c1));
  }

  public static float Angle(LeafCurve c0, LeafCurve c1) {
    float a = c1.angle0 - c0.angle1;
    if (a < 0f) {
      if (a > -180f) return a * -1f;
      return 360f + a;
    }
    if (a > 180f) {
      return 360f - a;
    }
    return a;
  }

  public static float Angle(Vector2 p0, Vector2 p1) {
    float x = p1.x - p0.x;
    float y = p1.y - p0.y;
    float a = (float)Math.Atan(y / x);
    if (x > 0) { //right side
      if (y > 0) return a;
      return Polar.Pi2 + a;
    } else { //left side
      if (y > 0) return Polar.Pi + a;
      return Polar.Pi + a;
    }
  }
}
}