using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
  public enum LeafVeinType {
    Midrib,
    LobeRib,
    LobeInner,
    LobeToMargin,
    LobeToSplit,
    MidToMargin,
    MidToSplit,
    SplitEndPrimary,
    SplitEndSecondary,
    MarginSpanning,
  }

  [Serializable]
  public class LeafVein : Curve {
    private WeakReference<LeafVeins> veins;
    public LeafDeps deps;
    public LeafVeinType type;
    public bool lefty;
    public float thickness;
    public float taper;
    public float taperRNG;

    //non-constructor args, ensure usage in Copy() 
    public float pointAlongMargin;
    public float posAlongMidrib = 1f; //0-1 top-bottom
    public float truncationPoint = 1f; //for taper, 0 == root

    public float startThickness = -1f; //for margin spanners only
    public float endThickness = -1f; //for margin spanners only

    Vector2[] _polyPath;
    List<Vector2> poly;

    public LeafVein(Curve c, LeafVeins parent, LeafDeps deps, float thickness, float taper, float taperRNG, LeafVeinType type, bool lefty)
      : this(deps, parent, c.p0, c.h0, c.h1, c.p1, thickness, taper, taperRNG, type, lefty) { }

    public LeafVein(LeafDeps deps, LeafVeins parent,
      Vector2 p0, Vector2 h0, Vector2 h1, Vector2 p1,
      float thickness, float taper, float taperRNG, LeafVeinType type, bool lefty)
        : base(p0, h0, h1, p1) {
      this.deps = deps;
      this.type = type;
      this.lefty = lefty;
      this.thickness = thickness;
      this.taper = taper;
      this.taperRNG = taperRNG;
      RandomizeTaper();
      if (parent == null) Debug.LogWarning("LeafVein parent is null: " + type + "/" + (lefty ? "lefty" : "right"));
      veins = new WeakReference<LeafVeins>(parent);
    }

    private void RandomizeTaper() {
      if (LeafVein.IsPrimaryType(type)) return;
      float add;
      if (taper > 1f) {
        float diff = taper - 1f;
        add = BWRandom.RangeAdd(diff, 0.5f);
      } else {
        float diff = 1f - taper;
        add = BWRandom.RangeAdd((1f - diff) / 2f, diff * 2f);
      }
      add *= taperRNG;
      taper += add;
    }

    public LeafVeins GetVeinsParent() {
      if (veins == null) return null;
      LeafVeins v;
      veins.TryGetTarget(out v);
      if (v == null) {
        Debug.LogError("LeafVein Copy error: veins weakreference is null");
      }
      return v;
    }

    public new LeafVein Copy() {
      LeafVein l = new LeafVein(deps, GetVeinsParent(), p0, h0, h1, p1, thickness, taper, taperRNG, type, lefty);
      l.pointAlongMargin = pointAlongMargin;
      l.posAlongMidrib = posAlongMidrib;
      l.truncationPoint = truncationPoint;
      l.startThickness = startThickness;
      l.endThickness = endThickness;
      return l;
    }

    public new LeafVein Transform(Transform t) {
      // return this;
      LeafVein l = Copy();
      if (t == null) return this;
      l.p0 = t.TransformPoint(p0);
      l.h0 = t.TransformPoint(h0);
      l.h1 = t.TransformPoint(h1);
      l.p1 = t.TransformPoint(p1);
      return l;
    }

    public static LeafVein operator *(LeafVein c, Vector2 v) {
      c.p0 *= v; c.h0 *= v; c.h1 *= v; c.p1 *= v;
      return c;
    }

    public Vector2[] PolyPath {
      get {
        if (_polyPath != null) return _polyPath;
        _polyPath = LeafRenderer.GetPolyPathPoints(this, deps.baseParams.VeinLineSteps);
        return _polyPath;
      }
    }

    public List<Vector2> AsPoly(float widthAdd = 0f, float widthMult = 1f) {
      if (deps == null) return new List<Vector2>();
      if (poly != null && widthAdd == 0f && widthMult == 1f) return poly;
      float width = thickness;

      Vector2[] polyPath = PolyPath;
      polyPath = polyPath != null ? polyPath : LeafRenderer.GetPolyPathPoints(this, deps.baseParams.VeinLineSteps);
      List<Vector2> rightSide = new List<Vector2>();
      List<Vector2> leftSide = new List<Vector2>();
      float idx = 0;
      float total = polyPath.Length - 2;
      foreach ((Vector2 p1, Vector2 p2) in ListExtensions.Pairwise(polyPath)) {
        float angle = CurveHelpers.Angle(p1, p2) * Polar.RadToDeg;
        if ((type == LeafVeinType.MidToMargin || type == LeafVeinType.MidToSplit || type == LeafVeinType.LobeToMargin) &&
             p1 == polyPath.First()) {
          angle = lefty ? 180f : 0f;
        }

        if (ShouldCheckMidribWidth(type) && GetVeinsParent() != null) width = Mathf.Min(thickness, GetVeinsParent().GetMidribThicknessAtPercent(1f - posAlongMidrib));
        float perp = (angle + 90f) % 360f;
        float perc = idx / total;

        if (truncationPoint > 0) perc *= truncationPoint;
        else if (truncationPoint < 0) {
          float someWidth = PolyWidthAtPercent(-truncationPoint);
          width = someWidth;
        }

        float thisWidth = PolyWidthAtPercent(perc, width);
        if (type != LeafVeinType.SplitEndSecondary && type != LeafVeinType.MarginSpanning) thisWidth += widthAdd;
        thisWidth *= widthMult;
        if (thisWidth < 0f) thisWidth = 0f;

        if (type != LeafVeinType.MarginSpanning ||
           (type == LeafVeinType.MarginSpanning && thisWidth > 0f)) {
          Vector2 pRight = p1.AddPolar(new Polar(thisWidth, perp, true));
          Vector2 pLeft = p1.AddPolar(new Polar(-thisWidth, perp, true));
          rightSide.Add(pRight);
          leftSide.Add(pLeft);
        }

        idx++;

        if (p2 == polyPath.Last()) {
          rightSide.Add(p2.AddPolar(new Polar(thisWidth, perp, true)));
          leftSide.Add(p2.AddPolar(new Polar(-thisWidth, perp, true)));
        }
        if (thisWidth <= 0f && type != LeafVeinType.MarginSpanning) break;
      }
      leftSide.Reverse();

      List<Vector2> l = rightSide.Concat(leftSide).ToList();
      if (widthAdd == 0f && widthMult == 1f) poly = l;
      return l;
    }

    public float PolyWidthAtPercent(float perc, float baseWidth = -1f) {
      if (type == LeafVeinType.MarginSpanning) {
        float end = endThickness;
        float start = startThickness;
        if (taper <= 1f) end *= (1f - taper);
        else end = -startThickness * (taper - 1f);
        end = Mathf.Min(endThickness, end);
        if (lefty) perc = 1f - perc;
        return thickness * Mathf.Lerp(start, end, perc);
      }

      float width = baseWidth != -1f ? baseWidth : thickness;
      perc = Mathf.Min(1f, perc * taper);
      float thisWidth = (1f - perc) * width;
      if (thisWidth < 0f) thisWidth = 0f;
      return thisWidth;
    }

    private static bool ShouldCheckMidribWidth(LeafVeinType t) {
      switch (t) {
        case LeafVeinType.MidToMargin:
        case LeafVeinType.MidToSplit:
        case LeafVeinType.LobeToMargin:
        case LeafVeinType.LobeToSplit:
        case LeafVeinType.SplitEndPrimary:
        case LeafVeinType.SplitEndSecondary:
          return true;
      }
      return false;
    }

    public static bool IsPrimaryType(LeafVeinType t) {
      switch (t) {
        case LeafVeinType.Midrib:
        case LeafVeinType.LobeRib:
          return true;
      }
      return false;
    }

    public static bool IsRibTouchingType(LeafVeinType t) {
      switch (t) {
        case LeafVeinType.MidToMargin:
        case LeafVeinType.MidToSplit:
        case LeafVeinType.LobeToMargin:
        case LeafVeinType.LobeToSplit:
          return true;
      }
      return false;
    }

    public override string ToString() {
      return type + "/" + (lefty ? "left" : "right") + ": " + base.ToString();
    }
  }

}
