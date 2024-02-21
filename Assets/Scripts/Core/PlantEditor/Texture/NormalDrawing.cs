using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static BionicWombat.NormalColors;
using static BionicWombat.VectorExtensions;

namespace BionicWombat {
  public static class NormalDrawing {
    public static Color[] DrawSphere(int w, float squish = 1.0f, float flipPoint = 0.75f, bool linearCurve = false) {
      int rad = w / 2;
      float rad2 = Mathf.Pow(rad, 2f);
      Color[] colors = new Color[w * w];

      float InCircle(int x, int y, float w, float soften = 1.0f) {
        float span = w * soften;
        float dist = Mathf.Pow(x - rad, 2) + MathF.Pow(y - rad, 2);
        float alpha = 1f;
        if (soften > 0f) {
          float rDist = rad2 - dist;
          if (rDist > 0f && rDist < span)
            alpha = (rDist / span);
          else if (rDist > 0)
            alpha = 1f;
          else
            alpha = 0f;
        }
        return alpha;
      };

      const float accuracy = 0.001f;
      Vector2 center = new Vector2(rad, rad);
      for (int y = 0; y < w; y++) {
        for (int x = 0; x < w; x++) {
          float alpha = InCircle(x, y, w);
          if (alpha > 0f) {
            Vector2 origin = new Vector2((float)x, (float)y);
            Vector2 resultant = origin - center;
            float theta = CurveHelpers.Angle(origin, center);
            float cos = Mathf.Cos(theta).Truncate(3);
            float sin = Mathf.Sin(theta).Truncate(3);
            float dist = Vector2.Distance(origin, center) / rad;

            int sector = (int)(theta / Polar.HalfPi);
            float longi = resultant.x / (w / 2f); //-1 : 1
            float lat = resultant.y / (w / 2f); //-1 : 1h
            if (dist > flipPoint) {
              float add = cos;
              if (sector == 1 || sector == 2) add = -add;
              if (longi > accuracy) longi = -longi + add;
              else if (longi < -accuracy) longi = -longi - add;
              else longi = 0f;
              longi *= (1f / (1f - flipPoint));

              add = sin;
              if (sector >= 2) add = -add;
              if (lat > accuracy) lat = -lat + add;
              else if (lat < -accuracy) lat = -lat - add;
              else lat = 0f;
              lat *= (1f / (1f - flipPoint));
            } else {
              float multi = 1f / flipPoint;
              longi *= multi;
              lat *= multi;
            }

            if (linearCurve) {
              longi = 1f - (Mathf.Acos(longi) / Polar.Pi); //0 : 1 linear
              longi = longi * 2 - 1f; //0:1 -> -1:1
              lat = (Mathf.Asin(lat) / Polar.Pi) + 0.5f; //1 : 0 linear
              lat = lat * 2 - 1f; //1 : 0 -> 2 : 0
            }
            longi *= Polar.HalfPi;
            lat *= Polar.HalfPi;

            float len = Mathf.Lerp(squish, 1f, dist);
            colors[y * w + x] = ColorWithPolar3(new Polar3(len, lat, longi), alpha);
          }
        }
      }

      return colors;
    }

    public static (Color[] colors, RectInt rect) DrawCylinder(int w, int h,
        float theta = 0f, float flipPoint = 0.5f, bool useLinearCurve = false) {
      int h1 = Mathf.RoundToInt((float)w * Mathf.Abs(Mathf.Sin(theta)));
      int h2 = Mathf.RoundToInt((float)h * Mathf.Abs(Mathf.Sin(Polar.HalfPi - theta)));
      int w1 = Mathf.RoundToInt((float)w * Mathf.Abs(Mathf.Sin(Polar.HalfPi - theta)));
      int w2 = Mathf.RoundToInt((float)h * Mathf.Abs(Mathf.Sin(theta)));
      int newW = Mathf.Abs(w1) + Mathf.Abs(w2);
      int newH = Mathf.Abs(h1) + Mathf.Abs(h2);
      int rad = newW / 2;
      RectInt rect = new RectInt(w1, h1, newW, newH);

      Color[] colors = new Color[newW * newH];
      Vector2 centerOrigin = new Vector2((float)w1 / 2f, (float)h1 / 2f);
      Vector2 centerEnd = new Vector2((float)newW - centerOrigin.x, (float)newH - centerOrigin.y);
      if ((theta > Polar.HalfPi && theta < Polar.Pi) ||
          (theta > Polar.Pi270 && theta < Polar.Pi2)) {
        centerOrigin.y = newH - centerOrigin.y;
        centerEnd.y = newH - centerEnd.y;
      }

      Vector2 center = new Vector2(rad, newH / 2);
      Dictionary<Vector2, int> vk = new Dictionary<Vector2, int>();
      float accuracy = 0.01f;
      for (int y = 0; y < newH; y++) {
        for (int x = 0; x < newW; x++) {
          Vector2 origin = new Vector2((float)x, (float)y);
          float modTheta = Polar.Pi2 - theta;
          float cos = Mathf.Cos(modTheta).Truncate(3);
          float sin = Mathf.Sin(modTheta).Truncate(3);
          Vector2 startPoint = -(float)newH * new Vector2(cos, sin) + origin;
          Vector2 endPoint = (float)newH * new Vector2(cos, sin) + origin;
          (Vector2 intersection, bool intersectionError) = VectorExtensions.GetIntersection(startPoint, endPoint, centerOrigin, centerEnd, true);
          float dist = Vector2.Distance(origin, intersection);
          if (dist > w / 2f || intersectionError) {
            colors[y * newW + x] = Color.clear;
            continue;
          }

          Vector2 resultant = origin - intersection;
          float longi = resultant.x / (w / 2f); //-1 : 1
          float lat = resultant.y / (w / 2f); //-1 : 1

          float normDist = dist / (w / 2f);
          int sector = (int)(theta / Polar.HalfPi);
          if (normDist > flipPoint) {
            float add = cos;
            if (sector == 1 || sector == 2) add = -add;
            if (longi > accuracy) longi = -longi + add;
            else if (longi < -accuracy) longi = -longi - add;
            else longi = 0f;
            longi *= (1f / (1f - flipPoint));

            add = sin;
            if (sector <= 1) add = -add;
            if (lat > accuracy) lat = -lat + add;
            else if (lat < -accuracy) lat = -lat - add;
            else lat = 0f;
            lat *= (1f / (1f - flipPoint));
          } else {
            float multi = 1f / flipPoint;
            longi *= multi;
            lat *= multi;
          }

          if (useLinearCurve) {
            longi = 1f - (Mathf.Acos(longi) / Polar.Pi); //0 : 1 linear
            longi = longi * 2 - 1f; //0:1 -> -1:1
            lat = (Mathf.Asin(lat) / Polar.Pi) + 0.5f; //1 : 0 linear
            lat = lat * 2 - 1f; //1 : 0 -> 2 : 0

            // longi = -(Mathf.Cos(Polar.Pi * longi) - 1f) / 2f; //EaseInOutSin: 0 : 1
            // longi = Mathf.Sin((longi * Polar.Pi) / 2f); //EaseOutSin: 0 : 1
          }
          longi *= Polar.HalfPi;
          lat *= Polar.HalfPi;

          float len = 1f;
          // len = 1f / (Mathf.Cos(longi) - Mathf.Sin(longi));
          // len = Mathf.Lerp(squish, 1f, dist)

          colors[y * newW + x] = ColorWithPolar3(new Polar3(len, lat, longi));

          if (vk.ContainsKey(intersection)) vk[intersection]++;
          else vk[intersection] = 1;
        }
      }

      return (colors, rect);
    }

    /*   2    <-- use this ordering
     1
             3
      4
    */
    public static (Color[] colors, RectInt rect) DrawQuad(Vector2[] points, float depth = 1.0f,
        float flipPoint = 1f, bool useLinearCurve = false) {
      float distinct = points.Distinct().Count();
      if (distinct != 4) {
        if (points.Length != 4)
          Debug.LogWarning("DrawQuad must be called with exactly 4 unique points");
        return (new Color[0], new RectInt(0, 0, 0, 0));
      }

      float pixelCorrection = 2f;
      RectInt extents = GetExtents(points, (int)pixelCorrection);
      Color[] colors = new Color[extents.width * extents.height];

      Vector2[] newPoints = new Vector2[points.Length];
      Vector2 origin = new Vector2(extents.x, extents.y);
      for (int i = 0; i < newPoints.Length; i++) {
        newPoints[i] = points[i] - origin;
        if (newPoints[i].x == extents.width) newPoints[i].x--;
        if (newPoints[i].y == extents.height) newPoints[i].y--;
      }
      points = null; //don't use it again!

      Vector2 centerOrigin = Vector2.Lerp(newPoints[0], newPoints[1], 0.5f);
      Vector2 centerEnd = Vector2.Lerp(newPoints[3], newPoints[2], 0.5f);
      float topDist = Vector2.Distance(newPoints[0], centerOrigin);
      float botDist = Vector2.Distance(newPoints[3], centerEnd);

      int lines = 8;
      (Vector2 l1, Vector2 l2)[] spanningLines = new (Vector2 l1, Vector2 l2)[lines];
      for (int i = 0; i < spanningLines.Length; i++)
        spanningLines[i] = (Vector2.Lerp(newPoints[0], newPoints[1], (float)i / (float)(lines - 1)),
                            Vector2.Lerp(newPoints[3], newPoints[2], (float)i / (float)(lines - 1)));
      float PercAlong(Vector2 p) {
        float minDist = float.MaxValue;
        float minDist2 = float.MaxValue;
        int minIndex = -1;
        int minIndex2 = -1;
        int idx = 0;
        foreach ((Vector2 l1, Vector2 l2) in spanningLines) {
          float dist = DistFromLine(p, l1, l2);
          if (dist < minDist) {
            minDist2 = minDist;
            minIndex2 = minIndex;
            minDist = dist;
            minIndex = idx;
          } else if (dist < minDist2) {
            minDist2 = dist;
            minIndex2 = idx;
          }
          idx++;
        }
        if (minIndex == -1) { Debug.LogWarning("PercAlong didn't find a close line somehow"); return 0f; }
        Vector2 closePoint = ClosestPointOnLine(p, spanningLines[minIndex].l1, spanningLines[minIndex].l2);
        float d1 = Vector2.Distance(closePoint, spanningLines[minIndex].l1);
        float d2 = Vector2.Distance(closePoint, spanningLines[minIndex].l2);
        float perc = d1 / (d1 + d2);
        float dFromCP = Vector2.Distance(closePoint, p);

        Vector2 closePoint2 = ClosestPointOnLine(p, spanningLines[minIndex2].l1, spanningLines[minIndex2].l2);
        float d12 = Vector2.Distance(closePoint2, spanningLines[minIndex2].l1);
        float d22 = Vector2.Distance(closePoint2, spanningLines[minIndex2].l2);
        float perc2 = d12 / (d12 + d22);
        float dFromCP2 = Vector2.Distance(closePoint2, p);

        float total = dFromCP + dFromCP2;
        float retPerc = (perc * (1f - (dFromCP / total))) + (perc2 * (1f - (dFromCP2 / total)));

        return retPerc;
      };

      const float accuracy = 0.001f;
      for (int y = 0; y < extents.height; y++) {
        for (int x = 0; x < extents.width; x++) {
          Vector2 p = new Vector2(x, y);
          float perc = PercAlong(p);
          Vector2 intersection = Vector2.Lerp(centerOrigin, centerEnd, perc);
          float maxDist = Mathf.Lerp(topDist, botDist, perc);
          (bool isInPoly, float distFromPoly) = IsPointInPolygon(newPoints, p, true); //convert to use IMPolygon
          const float precision = 1f;
          if (isInPoly == false && distFromPoly >= precision) {
            colors[y * extents.width + x] = Color.clear;
            continue;
          }
          Vector2 resultant = p - intersection;
          float longi = resultant.x / maxDist;
          float lat = resultant.y / maxDist;
          float alpha = isInPoly ? 1f : 1f - (distFromPoly / precision);
          float distFromCenter = Vector2.Distance(p, intersection) / maxDist;
          distFromCenter = Mathf.Clamp01(distFromCenter);
          float len = 1f;
          float theta = CurveHelpers.Angle(p, intersection);
          float cos = Mathf.Cos(theta).Truncate(3);
          float sin = Mathf.Sin(theta).Truncate(3);

          int sector = (int)(theta / Polar.HalfPi);
          if (distFromCenter > flipPoint) {
            float add = cos;
            if (sector == 1 || sector == 2) add = -add;
            if (longi > accuracy) longi = -longi + add;
            else if (longi < -accuracy) longi = -longi - add;
            else longi = 0f;
            longi *= (1f / (1f - flipPoint));

            add = sin;
            if (sector >= 2) add = -add;
            if (lat > accuracy) lat = -lat + add;
            else if (lat < -accuracy) lat = -lat - add;
            else lat = 0f;
            lat *= (1f / (1f - flipPoint));
          } else {
            float multi = 1f / flipPoint;
            longi *= multi;
            lat *= multi;
          }

          if (useLinearCurve) {
            longi = 1f - (Mathf.Acos(longi) / Polar.Pi); //0 : 1 linear
            longi = longi * 2 - 1f; //0:1 -> -1:1
            lat = (Mathf.Asin(lat) / Polar.Pi) + 0.5f; //1 : 0 linear
            lat = lat * 2 - 1f; //1 : 0 -> 2 : 0

            // longi = -(Mathf.Cos(Polar.Pi * longi) - 1f) / 2f; //EaseInOutSin: 0 : 1
            // longi = Mathf.Sin((longi * Polar.Pi) / 2f); //EaseOutSin: 0 : 1
          }
          longi *= depth * Polar.HalfPi;
          lat *= depth * Polar.HalfPi;

          colors[y * extents.width + x] = ColorWithPolar3(new Polar3(len, lat, longi), alpha);
        }
      }

      return (colors, extents);
    }

    public static (Color[] colors, RectInt rect) DrawPuffy(Vector2[] points,
        float flipPoint, float plateauClamp, float soften, int downsample, bool asHeightMap, bool useLinearCurve = false) {
      RectInt extents = GetExtents(points, 0);

      Vector2[] newPoints = new Vector2[points.Length];
      Vector2 origin = new Vector2(extents.x, extents.y);
      for (int i = 0; i < newPoints.Length; i++) {
        newPoints[i] = points[i] - origin;
        if (newPoints[i].x == extents.width) newPoints[i].x--;
        if (newPoints[i].y == extents.height) newPoints[i].y--;
      }
      points = null; //don't use it again!

      IMPolygon poly = new IMPolygon(newPoints);
      poly.Precalculate();

      //Calculate distances from edge
      int dsWidth = extents.width / downsample;
      int dsHeight = extents.height / downsample;
      float[] dists = new float[dsWidth * dsHeight];
      Vector2[] borderPoints = new Vector2[dsWidth * dsHeight];
      float maxDist = float.MinValue;
      float fDownsample = (float)downsample;
      for (int y = 0; y < dsHeight; y++) {
        for (int x = 0; x < dsWidth; x++) {
          Vector2 p = new Vector2((float)x * fDownsample, (float)y * fDownsample);
          int idx = y * dsWidth + x;
          if (IsPointInPolygon(newPoints, p).val) {
            Vector2 close = poly.NearestPointFrom(p);
            float dist = Vector2.Distance(close, p);
            dists[idx] = dist;
            borderPoints[idx] = close;
            if (dist > maxDist) {
              maxDist = dist;
            }
          } else {
            dists[idx] = float.MinValue;
          }
        }
      }

      //Calculate Plateaus
      maxDist *= plateauClamp;
      List<Vector2> plateau = new List<Vector2>();
      for (int y = 0; y < dsHeight; y++) {
        for (int x = 0; x < dsWidth; x++) {
          int distI = y * dsWidth + x;
          if (dists[distI] != float.MinValue) {
            float dist = Mathf.Min(1f, dists[distI] / maxDist);
            int cX = x * downsample;
            int cY = y * downsample;

            if (dist >= 1f) {
              int cidx = cY * extents.width + cX;
              Vector2 idxVec = new Vector2(cidx % extents.width, Mathf.FloorToInt(cidx / extents.width));
              plateau.Add(idxVec);
            }
          }
        }
      }

      //Recalculate distances from hull
      List<Vector2> hull = ConvexHull.ComputeConvexHull(plateau, false);
      IMPolygon hullPoly = new IMPolygon(hull.ToArray());
      hullPoly.Precalculate();
      Color[] colors = new Color[extents.width * extents.height];
      const float accuracy = 0.001f;
      for (int y = 0; y < dsHeight; y++) {
        for (int x = 0; x < dsWidth; x++) {
          int distI = y * dsWidth + x;
          int[] cIdxs = new int[downsample * downsample];
          int cX = x * downsample;
          int cY = y * downsample;
          int idx = 0;
          for (int ny = cY; ny < cY + downsample; ny++) {
            for (int nx = cX; nx < cX + downsample; nx++) {
              cIdxs[idx++] = ny * extents.width + nx;
            }
          }

          int distIdx = y * dsWidth + x;
          if (dists[distIdx] != float.MinValue) {
            Vector2 p = new Vector2((float)x * fDownsample, (float)y * fDownsample);
            if (IsPointInPolygon(hull.ToArray(), p).val) {
              foreach (int h in cIdxs)
                colors[h] = asHeightMap ? Color.white : NormalColors.Facing;
              continue;
            }
            Vector2 pointOnPlateau = hullPoly.NearestPointFrom(p);
            Vector2 pointOnBorder = borderPoints[distI];
            float distP = Vector2.Distance(p, pointOnPlateau);
            float distB = Vector2.Distance(p, pointOnBorder);
            float dist = distP / (distP + distB);
            float localMaxDist = Vector2.Distance(pointOnBorder, pointOnPlateau);
            float pW = Mathf.Abs(p.x - pointOnPlateau.x);
            float pH = Mathf.Abs(p.y - pointOnPlateau.y);
            float longi = (p - pointOnPlateau).x / localMaxDist;
            float lat = (p - pointOnPlateau).y / localMaxDist;
            dist = Mathf.Clamp01(dist);
            longi = Mathf.Clamp(longi, -1f, 1f);
            lat = Mathf.Clamp(lat, -1f, 1f);

            float theta = CurveHelpers.Angle(p, pointOnPlateau);
            float cos = Mathf.Cos(theta).Truncate(3);
            float sin = Mathf.Sin(theta).Truncate(3);

            int sector = (int)(theta / Polar.HalfPi);
            if (dist > flipPoint) {
              float add = cos;
              if (sector == 1 || sector == 2) add = -add;
              if (longi > accuracy) longi = -longi + add;
              else if (longi < -accuracy) longi = -longi - add;
              else longi = 0f;
              longi *= (1f / (1f - flipPoint));

              add = sin;
              if (sector >= 2) add = -add;
              if (lat > accuracy) lat = -lat + add;
              else if (lat < -accuracy) lat = -lat - add;
              else lat = 0f;
              lat *= (1f / (1f - flipPoint));
            } else {
              float multi = 1f / flipPoint;
              longi *= multi;
              lat *= multi;
            }

            if (useLinearCurve) {
              longi = 1f - (Mathf.Acos(longi) / Polar.Pi); //0 : 1 linear
              longi = longi * 2 - 1f; //0:1 -> -1:1
              lat = (Mathf.Asin(lat) / Polar.Pi) + 0.5f; //1 : 0 linear
              lat = lat * 2 - 1f; //1 : 0 -> 2 : 0

              // longi = -(Mathf.Cos(Polar.Pi * longi) - 1f) / 2f; //EaseInOutSin: 0 : 1
              // longi = Mathf.Sin((longi * Polar.Pi) / 2f); //EaseOutSin: 0 : 1
            }
            longi *= 1f * Polar.HalfPi * soften;
            lat *= 1f * Polar.HalfPi * soften;

            if (asHeightMap) dist = (1f - dist);// * 0.5f + 0.5f;
            foreach (int j in cIdxs)
              colors[j] = asHeightMap ? new Color(dist, dist, dist) : ColorWithPolar3(new Polar3(1f, lat, longi));
          } else {
            foreach (int i in cIdxs)
              colors[i] = Color.clear;
          }
        }
      }

      return (colors, extents);
    }
  }

  /* if (__L.Count > 0) Debug.Log(__L.ToLogShort());
      void DrawDotF(float x, float y, Color c, int rad = 2) => DrawDot((int)x, (int)y, c, rad);
      void DrawDot(int x, int y, Color c, int rad = 2) {
        try {
          colors[y * newW + x] = c;
          if (rad >= 2) {
            colors[y * newW + x + 1] = c;
            colors[(y + 1) * newW + x] = c;
            colors[(y + 1) * newW + x + 1] = c;
          }
        } catch (IndexOutOfRangeException) { }
      };
      foreach (Vector2 v in vk.Keys) DrawDotF(v.x, v.y, Color.yellow, 1);
      DrawDotF(centerOrigin.x, centerOrigin.y, Color.red);
      DrawDotF(centerEnd.x, centerEnd.y, Color.green);
      DrawDotF(w1, h1, Color.white);
      DrawDotF(w2, h2, Color.cyan);
      */
}
