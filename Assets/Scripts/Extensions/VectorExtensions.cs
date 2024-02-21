using System;
using UnityEngine;

namespace BionicWombat {
  public delegate float ModFloat(float f);

  public static class VectorExtensions {
    //
    //Vector3
    //
    public static bool IsNaN(this Vector3 vec) => float.IsNaN(vec.x) || float.IsNaN(vec.y) || float.IsNaN(vec.z);

    public static Vector3 WithX(this Vector3 vec, float x) {
      return new Vector3(x, vec.y, vec.z);
    }

    public static Vector3 WithY(this Vector3 vec, float y) {
      return new Vector3(vec.x, y, vec.z);
    }

    public static Vector3 WithZ(this Vector3 vec, float z) {
      return new Vector3(vec.x, vec.y, z);
    }

    public static Vector3 AddX(this Vector3 vec, float x) {
      return new Vector3(vec.x + x, vec.y, vec.z);
    }

    public static Vector3 AddY(this Vector3 vec, float y) {
      return new Vector3(vec.x, vec.y + y, vec.z);
    }

    public static Vector3 AddZ(this Vector3 vec, float z) {
      return new Vector3(vec.x, vec.y, vec.z + z);
    }

    public static Vector3 MultX(this Vector3 vec, float f) {
      return new Vector3(vec.x * f, vec.y, vec.z);
    }

    public static Vector3 MultY(this Vector3 vec, float f) {
      return new Vector3(vec.x, vec.y * f, vec.z);
    }

    public static Vector3 MultZ(this Vector3 vec, float f) {
      return new Vector3(vec.x, vec.y, vec.z * f);
    }

    public static Vector3 Mult(this Vector3 vec, Vector3 v) {
      return new Vector3(vec.x * v.x, vec.y * v.y, vec.z * v.z);
    }

    public static Vector3 OnX(this Vector3 vec, ModFloat f) {
      return new Vector3(f(vec.x), vec.y, vec.z);
    }

    public static Vector3 OnY(this Vector3 vec, ModFloat f) {
      return new Vector3(vec.x, f(vec.y), vec.z);
    }

    public static Vector3 Jumble(this Vector3 vec, float range, bool withZ = true) {
      return new Vector3(vec.x + BWRandom.RangeAdd(range),
        vec.y + BWRandom.RangeAdd(range),
        vec.z + (withZ ? BWRandom.RangeAdd(range) : 0f));
    }

    public static Vector3 Vector3With(float f) {
      return new Vector3(f, f, f);
    }

    public static Vector3 Rotate(this Vector3 vec, float x, float y, float z, Vector3 pivot) {
      Vector3 _v = vec - pivot;
      _v = Quaternion.Euler(x, y, z) * _v;
      return _v + pivot;
    }

    public static Vector3 Rotate(this Vector3 vec, Quaternion q, Vector3 pivot) {
      Vector3 _v = vec - pivot;
      _v = q * _v;
      return _v + pivot;
    }

    public static Vector3 Truncate(this Vector3 vec, int decPlaces = 2) {
      float mult = Mathf.Pow(10f, decPlaces);
      return new Vector3(
        Mathf.RoundToInt(vec.x * mult) / mult,
        Mathf.RoundToInt(vec.y * mult) / mult,
        Mathf.RoundToInt(vec.z * mult) / mult
      );
    }

    public static bool SoftEquals(this Vector3 v1, Vector3 v2, float precision = 0.01f) =>
       v1.x.SoftEquals(v2.x, precision) && v1.y.SoftEquals(v2.y, precision) && v1.z.SoftEquals(v2.z, precision);

    //
    //Vector2
    //
    public static Vector2 WithX(this Vector2 vec, float x) {
      return new Vector2(x, vec.y);
    }

    public static Vector2 WithY(this Vector2 vec, float y) {
      return new Vector2(vec.x, y);
    }

    public static Vector2 AddX(this Vector2 vec, float x) {
      return new Vector2(vec.x + x, vec.y);
    }

    public static Vector2 AddY(this Vector2 vec, float y) {
      return new Vector2(vec.x, vec.y + y);
    }

    public static Vector2 MultX(this Vector2 vec, float mult) {
      return new Vector2(vec.x * mult, vec.y);
    }

    public static Vector2 MultY(this Vector2 vec, float mult) {
      return new Vector2(vec.x, vec.y * mult);
    }

    public static Vector2 OnX(this Vector2 vec, ModFloat f) {
      return new Vector2(f(vec.x), vec.y);
    }

    public static Vector2 Vector2With(float f) {
      return new Vector2(f, f);
    }

    public static Vector2 Scale(this Vector2 vec, float s) {
      return vec * s;
    }

    public static Vector3 OneOverSelf(this Vector3 vec) =>
      new Vector3(1f / vec.x, 1f / vec.y, 1f / vec.z);

    public static Vector2 ExtraLerp(Vector2 a, Vector2 b, float f) {
      if (f < -1f || f > 2f) {
        Debug.LogWarning("ExtraLerp doesn't work with < -1 || > 2f");
        return f < 0 ? a : b;
      }
      float reg = f < 0 ? -f : f > 1 ? 1.0f - (f - 1.0f) : f;
      Vector2 regResult = Vector2.Lerp(a, b, reg);
      if (f > 1f) return regResult + (b - regResult) * 2f;
      if (f < 0f) return regResult + (a - regResult) * 2f;
      return regResult;
    }

    public static float Distance(this Vector2 vec, Vector2 from) {
      return (float)Math.Sqrt(Math.Pow((from.x - vec.x), 2) +
                              Math.Pow((from.y - vec.y), 2));
    }

    public static Polar GetPolar(this Vector2 vec, Vector2 from) {
      if (from.x - vec.x == 0) return new Polar(0, 0);
      float a = (float)Math.Atan((from.y - vec.y) / (from.x - vec.x));
      int quadrant = GetQuadrant(from, vec);
      switch (quadrant) {
        case 0: break;
        case 1: //with quad 2
        case 2: a = Polar.Pi + a; break;
        case 3: a = Polar.Pi2 + a; break;
      }
      return new Polar(
        vec.Distance(from),
        a
      );
    }

    public static Vector2 AddPolar(this Vector2 from, Polar polar) {
      return from +
        new Vector2(polar.len * (float)Math.Cos(polar.theta),
                    polar.len * (float)Math.Sin(polar.theta));
    }

    public static Vector3 AddPolar(this Vector3 from, Polar3 polar) {
      return from + polar.Vector;
    }

    private static int GetQuadrant(Vector2 v0, Vector2 v1) {
      if (v1.x > v0.x) {
        if (v1.y > v0.y) return 0;
        return 3;
      } else {
        if (v1.y > v0.y) return 1;
        return 2;
      }
    }

    public static (Vector2 result, bool error) GetIntersection(Vector2 A1, Vector2 A2, Vector2 B1, Vector2 B2, bool checkInside = false) {
      float tmp = (B2.x - B1.x) * (A2.y - A1.y) - (B2.y - B1.y) * (A2.x - A1.x);
      if (tmp == 0) {
        Debug.LogWarning("GetIntersection error: " + A1 + " " + A2 + " | " + B1 + " " + B2);
        return (Vector2.zero, true);
      }

      float mu = ((A1.x - B1.x) * (A2.y - A1.y) - (A1.y - B1.y) * (A2.x - A1.x)) / tmp;
      Vector2 r = new Vector2(B1.x + (B2.x - B1.x) * mu, B1.y + (B2.y - B1.y) * mu);

      if (checkInside) {
        if (!IsInsideLine(A1, A2, r) || !IsInsideLine(B1, B2, r)) {
          return (Vector2.negativeInfinity, true);
        }
      }
      return (r, false);
    }

    public static bool IsInsideLine(Vector2 s1, Vector2 s2, Vector2 point, float precision = 0.01f, bool log = false) {
      float p = precision;
      if (log) {
        Debug.Log("point.x >= s1.x: " + (point.x >= s1.x));
        Debug.Log("point.x <= s2.x: " + (point.x <= s2.x));
        Debug.Log("point.y >= s1.y: " + (point.y >= s1.y));
        Debug.Log("point.y - p <= s2.y: " + (point.y - p <= s2.y) + " | " + point.y + ", " + s2.y);
      }
      return (point.x + p >= s1.x && point.x - p <= s2.x
           || point.x + p >= s2.x && point.x - p <= s1.x)
          && (point.y + p >= s1.y && point.y - p <= s2.y
           || point.y + p >= s2.y && point.y - p <= s1.y);
    }

    public static float DistFromLine(Vector2 p, Vector2 a1, Vector2 a2) {
      float l2 = Mathf.Pow(Vector2.Distance(a1, a2), 2);
      if (l2 <= 0.0) return Vector2.Distance(p, a1);
      float t = Mathf.Max(0, Mathf.Min(1, Vector2.Dot(p - a1, a2 - a1) / l2));
      Vector2 projection = a1 + t * (a2 - a1);
      return Vector2.Distance(p, projection);
    }

    public static Vector2 ClosestPointOnLine(Vector2 p, Vector2 a1, Vector2 a2) {
      Vector2 AP = p - a1;       //Vector from A to P   
      Vector2 AB = a2 - a1;       //Vector from A to B  

      float magnitudeAB = Mathf.Pow(Vector2.Distance(a1, a2), 2);     //Magnitude of AB vector (it's length squared)     
      float ABAPproduct = Vector2.Dot(AP, AB);    //The DOT product of a_to_p and a_to_b     
      float distance = ABAPproduct / magnitudeAB; //The normalized "distance" from a to your closest point  

      if (distance < 0) {    //Check if P projection is over vectorAB     
        return a1;
      } else if (distance > 1) {
        return a2;
      } else {
        return a1 + AB * distance;
      }
    }

    public static (bool val, float dist) IsPointInPolygon(Vector2[] polygon, Vector2 testPoint, bool returnDist = false) {
      bool result = false;
      int j = polygon.Length - 1;
      float minDist = float.MaxValue;
      for (int i = 0; i < polygon.Length; i++) {
        if (polygon[i].y < testPoint.y && polygon[j].y >= testPoint.y ||
            polygon[j].y < testPoint.y && polygon[i].y >= testPoint.y) {
          float perc = (testPoint.y - polygon[i].y) / (polygon[j].y - polygon[i].y);
          float span = (polygon[j].x - polygon[i].x);
          float origin = polygon[i].x;
          float xPointOnLine = perc * span + origin;
          if (testPoint.x > xPointOnLine) {
            result = !result;
          }
        }
        if (returnDist) {
          float dist = DistFromLine(testPoint, polygon[i], polygon[j]);
          if (dist < minDist) minDist = dist;
        }
        j = i;
      }
      return (result, minDist);
    }

    public static RectInt GetExtents(Vector2[] points, int pixelCorrection = 0) {
      float minX = float.MaxValue;
      float minY = float.MaxValue;
      float maxX = float.MinValue;
      float maxY = float.MinValue;
      foreach (Vector2 p in points) {
        if (p.x > maxX) maxX = p.x;
        if (p.y > maxY) maxY = p.y;
        if (p.x < minX) minX = p.x;
        if (p.y < minY) minY = p.y;
      }
      return new RectInt(
        Mathf.RoundToInt(minX) - pixelCorrection,
        Mathf.RoundToInt(minY) - pixelCorrection,
        Mathf.RoundToInt(maxX - minX) + pixelCorrection * 2,
        Mathf.RoundToInt(maxY - minY) + pixelCorrection * 2
      );
    }

    public static (Vector3 min, Vector3 max) GetExtents(this Vector3[] points) {
      float minX = float.MaxValue;
      float minY = float.MaxValue;
      float minZ = float.MaxValue;
      float maxX = float.MinValue;
      float maxY = float.MinValue;
      float maxZ = float.MinValue;
      for (int i = 0; i < points.Length; i++) {
        Vector3 p = points[i];
        if (p.x < minX) minX = p.x;
        if (p.y < minY) minY = p.y;
        if (p.z < minZ) minZ = p.z;
        if (p.x > maxX) maxX = p.x;
        if (p.y > maxY) maxY = p.y;
        if (p.z > maxY) maxZ = p.z;
      }
      return (new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
    }

    //
    //Vector3Int
    //
    public static Vector3Int AddX(this Vector3Int vec, int x) {
      return new Vector3Int(vec.x + x, vec.y, vec.z);
    }

    public static Vector3Int AddY(this Vector3Int vec, int y) {
      return new Vector3Int(vec.x, vec.y + y, vec.z);
    }

    //
    //Vector2Int
    //
    public static Vector2Int AddX(this Vector2Int vec, int x) {
      return new Vector2Int(vec.x + x, vec.y);
    }

    public static Vector2Int AddY(this Vector2Int vec, int y) {
      return new Vector2Int(vec.x, vec.y + y);
    }

    public static Vector4 ToColor(this Vector4 v) =>
      new Color(v.x, v.y, v.z, v.w);

    public static string String(this Vector3 v, int precisionCount) => v.ToString("F" + precisionCount);
  }

}
