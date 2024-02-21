using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace BionicWombat {
  public enum LeafCurveType {
    FullSide,
    Scoop,
    LobeInner,
    LobeOuter,
    LowerHalf,
    Tip,
    Vein,
  }

  [Serializable]
  public class LeafCurve : Curve {
    public LeafCurveType curveType;
    public bool lefty;

    [NonSerialized] public LeafCurve prevCurve = null;
    [NonSerialized] public LeafCurve nextCurve = null;

    public LeafCurve(Curve curve, LeafCurveType curveType, bool lefty) : this(curve.p0, curve.h0, curve.h1, curve.p1, curveType, lefty) { }
    public LeafCurve(Vector2 p0, Vector2 p1, LeafCurveType type, bool lefty) : this(p0, p0, p1, p1, type, lefty) { }

    [JsonConstructor]
    public LeafCurve(Vector2 p0, Vector2 h0, Vector2 h1, Vector2 p1, LeafCurveType curveType, bool lefty) :
        base(p0, h0, h1, p1) {
      this.curveType = curveType;
      this.lefty = lefty;
    }

    public new LeafCurve Copy() => new LeafCurve(p0, h0, h1, p1, curveType, lefty);
    public Curve GetCurve() => new Curve(p0, h0, h1, p1);
    public static List<Curve> ToCurves(params LeafCurve[] l) => ToCurves(l.ToList());
    public static List<Curve> ToCurves(List<LeafCurve> l) => l.Select<LeafCurve, Curve>(lc => lc.GetCurve()).ToList();
    public static List<Curve> ToCurves(List<LeafCurve> l, Transform t) => l.Select<LeafCurve, Curve>(lc => lc.GetCurve().Transform(t)).ToList();

    public new LeafCurve Transform(Transform t) => t == null ? Copy() : new LeafCurve(
      t.TransformPoint(p0),
      t.TransformPoint(h0),
      t.TransformPoint(h1),
      t.TransformPoint(p1),
      curveType,
      lefty
    );

    public override string ToString() {
      return "curveType: " + curveType + " / lefty: " + lefty + " ||| [p0 " + p0 + " | h0 " + h0 + " | h1 " + h1 + " p1 " + p1 + "]";
    }

    public float Pudge {
      set {
        h0.y = value * (p1.y - p0.y);
        h1.y = (1 - value) * (p1.y - p0.y);
      }
    }

    public float Splay {
      set {
        Polar p = h1.GetPolar(h0);
        float angle = p.deg;
        Vector2 center = ((h1 - h0) / 2f) + h0;
        float splayOriginX = h0.x - center.x;
        Polar pol0 = h0.GetPolar(p0);
        Polar pol1 = h1.GetPolar(p1);
        float theta = 90 - (angle % 180);

        float tx0 = (h0.x - p0.x) + -(h0.y - p0.y) * (float)Math.Tan(theta * Polar.DegToRad);
        float tx1 = (h1.x - p1.x) + -(h1.y - p1.y) * (float)Math.Tan(theta * Polar.DegToRad);
        Vector2 target0 = p0.WithX(tx0);
        Vector2 target1 = p1.WithX(tx1);
        if (target0.x < 0) {
          float extent = center.x - target0.x;
          float perc = 1 - (-target0.x / extent);
          target0 = center + ((target0 - center) * perc);
          target1 = center + ((target1 - center) * perc);
        } else if (target1.x < 0) {
          float extent = center.x - target1.x;
          float perc = 1 - (-target1.x / extent);
          target0 = center + ((target0 - center) * perc);
          target1 = center + ((target1 - center) * perc);
        }

        float yExtent = center.y - target0.y;
        float yPerc = 1 - (h0.y / yExtent);
        float adjust = yPerc / 0.5f;
        bool should = value > 0.5f;
        if (value <= 0.5f)
          value *= adjust;
        else
          value = yPerc + ((value - 0.5f) * 2f * (1 - yPerc));
        Debug.Log("ext: " + yExtent + " | perc: " + yPerc + " | adj: " + adjust + " | value: " + value);
        // value *= adjust;
        h0 = center + ((target0 - center) * value);
        h1 = center + ((target1 - center) * value);
      }
    }

    public void Sheer(float val, float baseWidth) {
      h0.x = baseWidth * val * 2f;
      h1.x = baseWidth * (1 - val) * 2f;
    }

    public LeafCurve Extension() {
      LeafCurve c = new LeafCurve(
        p1,
        p1 - new Vector2(-1f, 0f),
        p1 - new Vector2(-2f, 0f),
        p1 - new Vector2(-3f, 0f),
        curveType,
        lefty);
      return c;
    }

    //returns the new curve
    public new LeafCurve Subdivide(float perc = 0.5f) {
      return new LeafCurve(base.Subdivide(perc), curveType, lefty);
    }

    public new LeafCurve GetSlice(float from, float to) {
      return new LeafCurve(base.GetSlice(from, to), curveType, lefty);
    }
  }
}
