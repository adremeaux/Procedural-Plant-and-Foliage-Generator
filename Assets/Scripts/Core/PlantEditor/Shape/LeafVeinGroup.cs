using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
  [Serializable]
  public class LeafVeinGroup {
    private LeafDeps deps;
    public List<LeafVein> rightVeins;
    public List<LeafVein> leftVeins;
    public List<LeafVein> veins {
      get => rightVeins.Concat(leftVeins).ToList();
    }

    public LeafVeinGroup(LeafDeps deps) : this(null, deps) { }

    public LeafVeinGroup(LeafVein baseVein, LeafDeps deps) {
      this.deps = deps;
      rightVeins = new List<LeafVein>();
      leftVeins = new List<LeafVein>();

      if (baseVein != null)
        rightVeins.Add(baseVein);
    }

    public void SplitVein(Vector2 pointAlongMargin, float splitPoint, float veinEndLerp) {
      LeafVein[] _split(LeafVein original, Vector2 pAlongMargin, float splitPoint) {
        (LeafVein rootCurve, LeafVein lowerCurve) = SubdivideVein(original, splitPoint);
        LeafVein upperCurve = lowerCurve.Copy();
        upperCurve.type = LeafVeinType.SplitEndSecondary;
        original.type = original.type == LeafVeinType.MidToMargin ? LeafVeinType.MidToSplit : LeafVeinType.LobeToSplit;

        Vector2 h0Base = Vector2.Lerp(rootCurve.p1, pAlongMargin, 0.5f).WithY(lowerCurve.h0.y);
        float diff = h0Base.y - rootCurve.p1.y;
        float targetYh0 = h0Base.y - (diff * 2f * veinEndLerp);

        upperCurve.p1 = pAlongMargin;
        upperCurve.h0 = h0Base.WithY(targetYh0);
        upperCurve.h1 = Vector2.Lerp(upperCurve.h0, upperCurve.p1, veinEndLerp);
        return new LeafVein[] { rootCurve, upperCurve, lowerCurve };
      };

      rightVeins = _split(rightVeins[0], pointAlongMargin, splitPoint).ToList();
      leftVeins = _split(leftVeins[0], pointAlongMargin.MultX(-1f), splitPoint).ToList();
    }

    public static (LeafVein rootCurve, LeafVein lowerCurve) SubdivideVein(LeafVein original, float point) {
      LeafVein c2 = new LeafVein(original.Subdivide(point), original.GetVeinsParent(), original.deps,
        original.thickness, original.taper, original.taperRNG,
        LeafVeinType.SplitEndPrimary, original.lefty);
      original.truncationPoint = point;
      c2.truncationPoint = -point;
      c2.posAlongMidrib = original.posAlongMidrib;
      return (original, c2);
    }

    public void AddVein(LeafVein vein) {
      if (vein.lefty) leftVeins.Add(vein);
      else rightVeins.Add(vein);
    }

    public void Mirror() {
      if (leftVeins.Count > 0) {
        Debug.LogError("Trying to mirror veins but leftVeins already exists");
        return;
      }

      Vector2 mirror = new Vector2(-1f, 1f);
      foreach (LeafVein v in rightVeins) {
        LeafVein r = v.Copy() * mirror;
        r.lefty = true;
        leftVeins.Add(r);
      }
    }

    public List<SpannerData> GetMarginPoints(bool rightSide) {
      bool ValidType(LeafVeinType t) {
        return (t == LeafVeinType.LobeRib || t == LeafVeinType.MidToMargin || t == LeafVeinType.LobeToMargin ||
                t == LeafVeinType.SplitEndPrimary || t == LeafVeinType.SplitEndSecondary);
      };

      List<SpannerData> l = new List<SpannerData>();
      if (rightSide) {
        LeafVein baseVein = GetBaseVein(true);
        foreach (LeafVein v in rightVeins)
          if (ValidType(v.type)) {
            l.Add(new SpannerData(v.p1, baseVein.p0, v.PolyWidthAtPercent(1f)));
          }
      } else {
        LeafVein baseVein = GetBaseVein(false);
        for (int i = leftVeins.Count - 1; i >= 0; i--) {
          LeafVein v = leftVeins[i];
          if (ValidType(v.type))
            l.Add(new SpannerData(v.p1, baseVein.p0, v.PolyWidthAtPercent(1f)));
        }
      }

      return l;
    }

    public LeafVein GetBaseVein(bool rightSide) {
      LeafVein bv = null;
      foreach (LeafVein v in rightSide ? rightVeins : leftVeins) {
        if (LeafVein.IsRibTouchingType(v.type) ||
            v.type == LeafVeinType.LobeRib) {
          if (bv != null) Debug.LogWarning("GetBaseVein multiple results: " + (rightSide ? rightVeins.First() : leftVeins.First()));
          bv = v;
        }
      }
      return bv;
    }
  }

  public struct SpannerData {
    public Vector2 marginPoint;
    public Vector2 rootPoint;
    public float thickness;

    public SpannerData(Vector2 marginPoint, Vector2 rootPoint, float thickness) {
      this.marginPoint = marginPoint;
      this.rootPoint = rootPoint;
      this.thickness = thickness;
    }
  }
}
