using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
#if UNITY_EDITOR
  [ExecuteInEditMode]
#endif
  [Serializable]
  public class LeafShape {
    public List<LeafCurve> curves { get; private set; }
    private LeafParamDict fields;
    private LeafDeps deps;

    public LeafShape() {
      InitCurves();
    }

    private void InitCurves() {
      if (deps == null) {
        curves = new List<LeafCurve>();
        return;
      }
      curves = new List<LeafCurve>() {
      new LeafCurve(
        new Vector2(0f, 0f),
        new Vector2(deps.baseParams.BaseWidth, deps.baseParams.BaseHeight / 3f),
        new Vector2(deps.baseParams.BaseWidth, deps.baseParams.BaseHeight / 3f * 2f),
        new Vector2(0f, deps.baseParams.BaseHeight),
        LeafCurveType.FullSide,
        false),
    };
    }

    public void Render(LeafParamDict fields, LeafVeins leafVeins, LeafDeps deps) {
      this.fields = fields;
      this.deps = deps;

      InitCurves();

      //gen 0
      Pudge(fields[LPK.Pudge]);
      Sheer(fields[LPK.Sheer]);
      Scale(fields[LPK.Width], fields[LPK.Length]);
      Tip(fields[LPK.TipAngle], fields[LPK.TipAmplitude]);

      //gen 1
      Heartify(fields);

      //gen 2
      Lobes(fields);

      //gen 3
      Scoop(fields);

      leafVeins.Render(fields, curves, deps);

      Mirror();
      JoinEnd();

      FindIntersections();
    }

    public void Pudge(LeafParam param) {
      if (!param.enabled) return;
      _GetCurve(LeafCurveType.FullSide).Pudge = param.value / -deps.baseParams.BaseHeight;
    }

    public void Sheer(LeafParam param) {
      if (!param.enabled) return;
      GetCurve(LeafCurveType.FullSide, curves).Sheer(param.value, deps.baseParams.BaseWidth);
    }

    public void Scale(LeafParam width, LeafParam length) {
      if (length.enabled) _GetCurve(LeafCurveType.FullSide).LengthExtent = length.value / -deps.baseParams.BaseHeight;
      if (width.enabled) _GetCurve(LeafCurveType.FullSide).WidthExtent = width.value / deps.baseParams.BaseWidth;
    }

    public void Tip(LeafParam angle, LeafParam amp) {
      LeafCurve c = _GetCurve(LeafCurveType.FullSide);
      float a = angle.enabled ? angle.value : angle.range.Default;
      float r = amp.enabled ? amp.value : amp.range.Default;
      c.h1 = VectorExtensions.AddPolar(c.p1, new Polar(r, a, true));
    }

    public void Heartify(LeafParamDict fields) {
      if (fields[LPK.Heart].value <= 0f) return;

      LeafCurve baseCurve = _GetCurve(LeafCurveType.FullSide, false);
      float handlesAngle = baseCurve.handlesInnerAngle;

      Subdivide(baseCurve, 0.5f);
      LeafCurve c0 = curves[0];
      LeafCurve c1 = curves[1];
      c0.curveType = LeafCurveType.LobeOuter;
      c1.curveType = LeafCurveType.LowerHalf;

      float width = VAL(LPK.Width);
      float length = VAL(LPK.Length);
      float thirdLen = length / 3f;
      UpdatePoint(3, new Vector3(width, -length / 3f, 0));
      CurveHelpers.FlattenAngle(c0, c1, deps); //account for sheer and pudge

      float sinusHeight = VAL(LPK.SinusHeight);
      float sinusSheer = VAL(LPK.SinusSheer);
      float WaistAmp = thirdLen * VAL(LPK.WaistAmp);
      float WaistAmpOffset = VAL(LPK.WaistAmpOffset);
      WaistAmpOffset += 1f;
      c0.h1 = c0.p1.AddPolar(new Polar(-WaistAmp * WaistAmpOffset, handlesAngle));
      c1.h0 = c1.p0.AddPolar(new Polar(-WaistAmp * (2f - WaistAmpOffset),
        handlesAngle + Polar.Pi));
      c0.h0 = new Vector2(sinusSheer * width, sinusHeight * thirdLen);
    }

    public void Lobes(LeafParamDict fields) {
      if (fields[LPK.Lobes].value <= 0f || fields[LPK.Heart].value <= 0f) return;
      LeafCurve baseCurve = _GetCurve(LeafCurveType.LobeOuter);
      float subpoint = baseCurve.FindClosestAngle(0f);
      Subdivide(baseCurve, subpoint);

      LeafCurve c0 = curves[0];
      LeafCurve c1 = curves[1];
      c0.curveType = LeafCurveType.LobeInner;
      c1.curveType = LeafCurveType.LobeOuter;

      float lobeTilt = (360f - VAL(LPK.LobeTilt)) * Polar.DegToRad;
      float lobeAmp = VAL(LPK.LobeAmplitude);
      float lobeAmpOffset = VAL(LPK.LobeAmpOffset);
      lobeAmpOffset += 1f;
      c0.h1 = c0.p1.AddPolar(new Polar(-lobeAmp * lobeAmpOffset, lobeTilt));
      c1.h0 = c1.p0.AddPolar(new Polar(-lobeAmp * (2f - lobeAmpOffset), lobeTilt + Polar.Pi));
      //c0.h0 = new Vector2(sinusSheer * width, sinusHeight * thirdLen);
    }

    public void Scoop(LeafParamDict fields) {
      if (!fields[LPK.ScoopDepth].enabled || !fields[LPK.ScoopHeight].enabled) return;
      if (VAL(LPK.ScoopDepth) <= 0.01f || VAL(LPK.ScoopHeight) <= 0.01f) return;

      LeafCurve baseCurve = _GetCurve(LeafCurveType.LobeInner);
      LeafCurveType bct = baseCurve.curveType;
      if (bct == LeafCurveType.FullSide) return; //no scoop for simple leaf
      Subdivide(baseCurve, VAL(LPK.ScoopDepth));

      LeafCurve c0 = curves[0];
      LeafCurve c1 = curves[1];
      c0.curveType = LeafCurveType.Scoop;
      c1.curveType = bct;

      c0.p0.y += 0.5f * (c1.p0.y * 0.8f); //VAL(LPK.ScoopHeight)
      c0.h0.y = c0.p0.y + 0.01f;
      c0.h1.y = (c1.p0.y - c0.p0.y) / 2f + c0.p0.y;
    }

    public void JoinEnd() {
      int i = curves.Count - 1;
      LinkCurves(curves[i], curves[0]);
    }

    public void AddCurve() {
      LeafCurve curveOld = curves[curves.Count - 1];
      AppendCurve(curveOld.Extension());
    }

    public void AppendCurve(LeafCurve newCurve, int atIndex = -1) {
      if (atIndex == -1) atIndex = curves.Count - 1;
      LeafCurve curveOld = curves[atIndex];
      curves.Insert(atIndex + 1, newCurve);
      LinkCurves(curveOld, newCurve);
    }

    private void LinkCurves(LeafCurve first, LeafCurve second) {
      first.nextCurve = second;
      second.prevCurve = first;
    }

    public void FlattenAngle(int index) {
      index -= 1;
      if (index < curves.Count) {
        LeafCurve c0 = curves[index];
        if (c0.nextCurve != null) {
          CurveHelpers.FlattenAngle(c0, c0.nextCurve, deps);
          return;
        }
      }

      Debug.Log("flatten angle error");
    }

    public void Subdivide(LeafCurve c, float point = 0.5f) {
      int i = 0;
      for (; i < curves.Count; i++)
        if (curves[i] == c)
          break;
      if (i < curves.Count) {
        AppendCurve(c.Subdivide(point), i);
        RebuildJoins();
        return;
      }
      Debug.Log("Leaf Subdivide error");
    }

    public void Mirror(bool replace = false) {
      int len = curves.Count;
      if (replace) {
        if (len % 2 == 1) {
          curves.RemoveRange((len + 1) / 2, (len - 1) / 2);
          len = (len + 1) / 2;
        } else {
          curves.RemoveRange(len / 2, len / 2);
          len /= 2;
        }
      }
      int nextIndex = len;
      for (int i = len - 1; i >= 0; i--) { //backwards
        LeafCurve from = curves[i];
        LeafCurve c = new LeafCurve(
          new Vector2(from.p1.x * -1, from.p1.y),
          new Vector2(from.h1.x * -1, from.h1.y),
          new Vector2(from.h0.x * -1, from.h0.y),
          new Vector2(from.p0.x * -1, from.p0.y),
          from.curveType,
          true
        );
        curves.Add(c);
      }

      RebuildJoins();
    }

    public void RebuildJoins() {
      LeafCurve cur = curves[0];
      int i;
      for (i = 0; i < curves.Count; i++) {
        cur.prevCurve = null;
        cur.nextCurve = null;
      }
      cur = curves[0];
      for (i = 1; i < curves.Count; i++) {
        LeafCurve next = curves[i];
        cur.nextCurve = next;
        next.prevCurve = cur;
        cur = next;
      }
    }

    public static List<Vector2> intersections = new List<Vector2>(); //use in inspector only!
    public void FindIntersections() {
      LeafCurve[] rightSide = curves.GetRange(0, curves.Count / 2).ToArray();
      LeafCurve[] leftSide = curves.GetRange(curves.Count / 2, curves.Count / 2).Reversed().ToArray();
      intersections.Clear();
      foreach (LeafCurve r in rightSide) {
        foreach (LeafCurve l in leftSide) {
          //Debug.Log("Comparing " + r.curveType + " to " + l.curveType);
          (bool intersects, Vector2 point) = r.Intersects(l, 4, r == rightSide.First() && l == leftSide.First());
          if (intersects) {
            if (point != r.p0 && point != r.p1) {
              //Debug.Log("intersection at " + point + " between " + r + " and " + l);
              intersections.Add(point);

              float p1 = r.GetPercentFromPoint(point);
              float p2 = l.GetPercentFromPoint(point);
              if (r.curveType == LeafCurveType.Scoop) {
                LeafCurve newR = r.GetSlice(p1, 1f);
                LeafCurve newL = l.GetSlice(0f, p2);
                curves[0] = newR;
                curves[curves.Count - 1] = newL;
              } else if (r.curveType == LeafCurveType.LobeInner) {
                LeafCurve newR = r.GetSlice(p1, 1f);
                LeafCurve newL = l.GetSlice(0f, p2);
                curves.RemoveAt(0);
                curves.RemoveAt(curves.Count - 1);
                curves[0] = newR;
                curves[curves.Count - 1] = newL;
              }
            }
          }
        }
      }
    }

    public static Vector3 GetPoint(LeafCurve curve, float t) {
      return curve.GetPoint(t);
    }

    public enum LeftyCheck {
      All,
      Right,
      Left
    }

    private LeafCurve _GetCurve(LeafCurveType type, bool allowFallback = true) => GetCurve(type, curves, LeftyCheck.All, allowFallback);
    public static LeafCurve GetCurve(LeafCurveType type,
        List<LeafCurve> curves,
        LeftyCheck leftyCheck = LeftyCheck.All,
        bool allowFallback = true) {
      if (type == LeafCurveType.Vein) {
        Debug.LogError("Leaf.GetCurve does not support Vein type");
        return null;
      }

      LeafCurve curve = null;
      foreach (LeafCurve c in curves) {
        if ((leftyCheck == LeftyCheck.Right && c.lefty) || (leftyCheck == LeftyCheck.Left && !c.lefty)) continue;
        if (c.curveType == type) {
          if (curve != null) {
            // Debug.LogWarning("GetCurve found multiple curves with type " + type + ": " +
            //   curves.Select<LeafCurve, string>(c => c.curveType + "/" + (c.lefty ? "left" : "right")).ToArray().ToLog());
          }
          curve = c;
        }
      }

      if (curve == null && allowFallback) {
        if (type == LeafCurveType.Scoop) return GetCurve(LeafCurveType.LobeInner, curves, leftyCheck, true);
        if (type == LeafCurveType.LobeInner) return GetCurve(LeafCurveType.LobeOuter, curves, leftyCheck, true);
        if (type == LeafCurveType.LobeOuter) return GetCurve(LeafCurveType.LowerHalf, curves, leftyCheck, true);
        if (type == LeafCurveType.Tip) return GetCurve(LeafCurveType.LowerHalf, curves, leftyCheck, true);
        if (type == LeafCurveType.LowerHalf) return GetCurve(LeafCurveType.FullSide, curves, leftyCheck, true);
        if (type == LeafCurveType.FullSide) {
          Debug.LogError("GetCurve type FullSide has no fallback. ");
          return curves != null && curves.Count > 0 ? curves.First() : null;
        }
      }

      return curve;
    }

    public void UpdatePoint(int index, Vector3 pos, bool shouldMirror = false) {
      int cIndex = (int)Math.Floor((double)index / 4.0);
      LeafCurve c = curves[cIndex];
      int rem = index % 4;
      if (deps.logOptions.logOnDrag) Debug.Log("idx: " + cIndex + " rem " + rem);
      switch (rem) {
        case 0:
          c.p0 = pos;
          if (c.prevCurve != null) {
            c.prevCurve.p1 = pos;
            if (deps.logOptions.logOnDrag)
              Debug.Log("a0: " + c.prevCurve.angle1 + " |  a1: "
                               + c.angle0 + " | angle: "
                               + CurveHelpers.Angle(c.prevCurve, c));
          }
          break;
        case 1: c.h0 = pos; break;
        case 2: c.h1 = pos; break;
        case 3:
          c.p1 = pos;
          if (c.nextCurve != null) {
            c.nextCurve.p0 = pos;
          }
          break;
      }

      if (shouldMirror) Mirror(true);
    }

    private float VAL(LPK key) => fields[key].value;
    private LeafParam LP(LPK key) => fields[key];
  }

}
