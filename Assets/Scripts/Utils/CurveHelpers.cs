using System;
using UnityEngine;

namespace BionicWombat {
  public static class CurveHelpers {
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
      // DebugBW.Log("a: " + (a * Mathf.Rad2Deg), LColor.white);
      // if (x.SoftEquals(0f)) a = -Polar.HalfPi;
      // if (y.SoftEquals(0f)) a = 0f;
      // DebugBW.Log("a: " + (a * Mathf.Rad2Deg) + " | (y / x): " + (y / x) + " | x: " + x + " | y: " + y, LColor.white);
      if (x < 0) { //left side
        if (y > 0) return Polar.Pi + a;
        return Polar.Pi + a;
      } else { //right side
        if (y > 0) return a;
        return Polar.Pi2 + a;
      }
    }

    public static float MiddlePointTangent(Vector2 v1, Vector2 v2, Vector2 v3, bool shouldLog = false) {
      Vector2 nv1 = new Vector2(1f, 0f);
      Vector2 nv2 = new Vector2(1f, -.25f);
      Vector2 nv3 = v3;//new Vector2(1f, -.17f);
                       // DebugBW.Log($"{Angle(nv1, nv2) * Mathf.Rad2Deg} | {Angle(nv1, nv3) * Mathf.Rad2Deg}", LColor.green);

      Vector2 e1 = v2 - v1;
      Vector2 e2 = v3 - v2;
      float angle = Angle(v1, v2) * Mathf.Rad2Deg;
      float angle2 = Angle(v2, v3) * Mathf.Rad2Deg;
      angle = (float)(Mathf.RoundToInt(angle) % 360);
      angle2 = (float)(Mathf.RoundToInt(angle2) % 360);

      if (angle2 - angle > 180f) angle2 -= 360f;
      if (angle - angle2 > 180f) angle -= 360f;

      if (shouldLog) {
        Debug.Log("  v1: " + v1 + " | v2: " + v2 + " | v3: " + v3);
        Debug.Log("  e1: " + e1 + " | e2: " + e2);
        DebugBW.Log("  angle: " + angle + " | angle2: " + angle2, LColor.orange);
      }

      return (angle + angle2) / -2f;
    }
  }
}
