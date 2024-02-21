using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
  [Serializable]
  public class LeafVeins {

    // [HideInInspector] public LeafDeps deps;
    [HideInInspector] public List<LeafVeinGroup> veinGroups;
    [HideInInspector] public List<Vector3> linearPoints = new List<Vector3>();
    [HideInInspector] public List<Vector3> gravityPoints = new List<Vector3>();
    [HideInInspector] private List<LeafCurve> curves;
    [HideInInspector] private LeafParamDict fields;
    [HideInInspector] private LeafVeinCalcs calcs;

    public LeafVeins() { }

    public void Render(LeafParamDict fields, List<LeafCurve> curves, LeafDeps deps) {
      this.fields = fields;
      this.curves = curves;

      veinGroups = new List<LeafVeinGroup>();

      linearPoints = CalcLinearPoints(curves.ConvertAll(c => new Curve3D(c)), deps.baseParams.RenderLineSteps, deps.baseParams.LinearPointsIncr).ToList();
      CreateVeins(fields, deps);
      SplitVeins(fields);
      BuildMarginSpanningVeins(fields, deps);
    }

    private void CreateVeins(LeafParamDict fields, LeafDeps deps) {
      calcs = GetVeinCalcs(fields);
      Vector3 modTip = calcs.tip.AddY(VAL(LPK.MidribDistFromMargin));

      LeafVein midrib = new LeafVein(deps, this,
        calcs.origin, Vector3.Lerp(calcs.origin, modTip, 0.33f), Vector3.Lerp(calcs.origin, modTip, 0.66f), modTip,
        VAL(LPK.MidribThickness), VAL(LPK.MidribTaper), 0f, LeafVeinType.Midrib, false);
      veinGroups.Add(new LeafVeinGroup(midrib, deps)); //main vein

      bool hasLobes = fields[LPK.Heart].value > 0f;
      (LeafVeinGroup lobeRibGroup, float lobeApexPoint) = hasLobes ? BuildLobeRibs(fields, calcs, deps) : (null, 0f);
      if (hasLobes) veinGroups.Add(lobeRibGroup); //secondary vein R

      int tertiaryCount = Math.Max(2, (int)(VAL(LPK.VeinDensity) * 16f));
      Vector3 target = FindPointOnEdgeWithY(calcs.origin.y, linearPoints);
      int buffer = FindNextLinearPointIndexFrom(target, linearPoints) - 1;
      int numPoints = linearPoints.Count - buffer - 1;
      float spacing = (float)numPoints / (float)tertiaryCount;

      gravityPoints = GravityCurvePoints(calcs.origin, modTip, buffer,
        VAL(LPK.GravVeinUpperBias), VAL(LPK.GravVeinLowerBias), VAL(LPK.VeinDistFromMargin)); //switch target back to apex!
      if (gravityPoints.Count != linearPoints.Count - buffer) {
        Debug.LogError("GravityPoints / LinearPoints length mismatch (" +
          gravityPoints.Count + ", " + linearPoints.Count + ") buffer: " + buffer);
        Debug.Log(linearPoints[buffer]);
        return;
      }

      //lobe secondary
      if (hasLobes) {
        float lsSpan = (float)buffer - lobeApexPoint;
        int lsCount = Mathf.RoundToInt(lsSpan / spacing);
        float lsSpacing = lsSpan / ((float)lsCount + 1f);
        for (int i = 0; i < lsCount; i++) {
          SpanningVeinParams svParams = new SpanningVeinParams(fields, calcs, deps, lobeRibGroup.rightVeins[0]);
          svParams.buffer = (int)(lobeApexPoint + lsSpacing - 1);
          svParams.index = i + 1;
          svParams.spacing = lsSpacing;
          svParams.totalCount = lsCount + 1;
          svParams.mirror = false;
          svParams.mirrorRoot = false;
          svParams.bunching = VAL(LPK.VeinLobeBunching);
          svParams.reverseDirection = true;

          LeafVein right = BuildSpanningVein(svParams, LeafVeinType.LobeToMargin);

          svParams.fromVein = lobeRibGroup.leftVeins[0];
          svParams.mirror = true;
          LeafVein left = BuildSpanningVein(svParams, LeafVeinType.LobeToMargin);

          LeafVeinGroup group = new LeafVeinGroup(right, deps);
          group.AddVein(left);
          veinGroups.Add(group);
        }
      }

      for (int i = 0; i < tertiaryCount; i++) {
        SpanningVeinParams svParams = new SpanningVeinParams(fields, calcs, deps, midrib);
        svParams.buffer = buffer;
        svParams.index = i;
        svParams.spacing = spacing;
        svParams.totalCount = tertiaryCount;
        svParams.mirror = false;
        svParams.mirrorRoot = false;
        svParams.bunching = VAL(LPK.VeinBunching);
        svParams.reverseDirection = false;

        LeafVein right = BuildSpanningVein(svParams, LeafVeinType.MidToMargin);
        svParams.mirror = true;
        svParams.mirrorRoot = true;
        LeafVein left = BuildSpanningVein(svParams, LeafVeinType.MidToMargin);

        LeafVeinGroup group = new LeafVeinGroup(right, deps);
        group.AddVein(left);
        veinGroups.Add(group);
      }
    }

    public LeafVeinCalcs GetVeinCalcs(LeafParamDict fields) {
      Vector3 tip = curves.Last().p1;
      // tip.y += VAL(LPK.MidribDistFromMargin);
      LeafCurve lobeCurve = LeafShape.GetCurve(LeafCurveType.LobeInner, curves, LeafShape.LeftyCheck.All, true);
      float apexPos = lobeCurve.FindApex(0f);
      return new LeafVeinCalcs(Vector3.zero, tip, lobeCurve.GetPoint(apexPos), apexPos);
    }

    public (LeafVeinGroup, float linearPoint) BuildLobeRibs(LeafParamDict fields, LeafVeinCalcs calcs, LeafDeps deps) {
      float lerpVal = VAL(LPK.VeinEndLerp) * 0.5f + 0.5f;
      LeafVein lobeRib = new LeafVein(deps, this,
          calcs.origin, Vector3.Lerp(calcs.origin, calcs.apex, 0.33f), Vector3.Lerp(calcs.origin, calcs.apex, lerpVal), calcs.apex,
          VAL(LPK.MidribThickness), VAL(LPK.MidribTaper), 0f, LeafVeinType.LobeRib, false);

      float lobeApexPoint = FindExactLinearPointFrom(lobeRib.p1, linearPoints);
      (Vector3 lobeTarget, float perp) = PointAlongMargin(lobeApexPoint + VAL(LPK.VeinEndOffset) / 4f, linearPoints);
      lobeTarget = Vector3.Lerp(lobeTarget, calcs.origin, VAL(LPK.VeinDistFromMargin));
      lobeRib.p1 = lobeTarget;
      lobeRib.h0 = Vector3.Lerp(lobeRib.h0, calcs.origin, VAL(LPK.VeinDistFromMargin));
      lobeRib.h1 = Vector3.Lerp(lobeRib.h1, calcs.origin, VAL(LPK.VeinDistFromMargin));

      LeafVeinGroup lobeRibGroup = new LeafVeinGroup(lobeRib, deps);
      lobeRibGroup.Mirror();
      return (lobeRibGroup, lobeApexPoint);
    }

    public LeafVein BuildSpanningVein(SpanningVeinParams svp, LeafVeinType type) {
      float pos = ((float)svp.buffer + (float)svp.index * svp.spacing);
      float distFromMargin = VAL(LPK.VeinDistFromMargin);
      int idx = (int)Math.Floor(pos);

      float yPerc = svp.index / (float)svp.totalCount;
      float randRange = 0.1f * svp.fields[LPK.VeinOriginRand].value;
      float centerRandRange = randRange * (yPerc + 0.05f);
      float randYAdd = BWRandom.RangeAdd(centerRandRange, svp.index == 0 ? BWRandomHalf.NegOnly : BWRandomHalf.All);

      pos += BWRandom.RangeAdd(randRange);

      float posAlongMidrib = Mathf.Max(0f, Mathf.Min(1f, yPerc + randYAdd));
      if (svp.reverseDirection) posAlongMidrib = 1f - posAlongMidrib;
      posAlongMidrib = Mathf.Pow(posAlongMidrib, svp.bunching);

      Vector3 rootPoint = svp.fromVein.GetPoint(posAlongMidrib);
      (Vector3 endPoint, float marginPerpAngle) = PointAlongMargin(pos, linearPoints);
      endPoint = Vector3.Lerp(endPoint, rootPoint, VAL(LPK.VeinDistFromMargin));

      Vector3 gravityIntersectionPoint = FindGravityIntersectionPoint(gravityPoints,
        rootPoint, endPoint, GravityBiasAtPerc(svp.index / (svp.totalCount - 1)));
      Vector3 h0 = gravityIntersectionPoint;
      Vector3 h1 = Vector3.Lerp(gravityIntersectionPoint, endPoint, VAL(LPK.VeinEndLerp));

      pos += VAL(LPK.VeinEndOffset);
      (endPoint, marginPerpAngle) = PointAlongMargin(pos, linearPoints);
      endPoint = Vector3.Lerp(endPoint, rootPoint, VAL(LPK.VeinDistFromMargin));

      Vector3 mr = svp.mirror ? new Vector3(-1f, 1f) : Vector3.one;
      Vector3 rootMr = svp.mirrorRoot ? new Vector3(-1f, 1f) : Vector3.one;
      LeafVein newVein = new LeafVein(svp.deps, this, rootPoint.Mult(rootMr), h0.Mult(mr), h1.Mult(mr), endPoint.Mult(mr),
        VAL(LPK.SecondaryThickness), VAL(LPK.SecondaryTaper), VAL(LPK.TaperRNG), type, svp.mirror);
      newVein.pointAlongMargin = pos;
      newVein.posAlongMidrib = 1f - posAlongMidrib;

      return newVein;
    }

    public void BuildMarginSpanningVeins(LeafParamDict fields, LeafDeps deps) {
      List<SpannerData> terminalPoints = new List<SpannerData>();
      List<LeafVeinGroup> rev = new List<LeafVeinGroup>();
      foreach (LeafVeinGroup group in veinGroups) {
        terminalPoints.AddRange(group.GetMarginPoints(true));
        rev.Insert(0, group);
      }
      LeafVein midribRef = GetMidrib();
      terminalPoints.Add(new SpannerData(midribRef.p1, midribRef.p0, midribRef.PolyWidthAtPercent(1f)));
      foreach (LeafVeinGroup group in rev)
        terminalPoints.AddRange(group.GetMarginPoints(false));

      LeafVeinGroup marginSpanningVeins = new LeafVeinGroup(deps);
      bool didPass = false;
      foreach ((SpannerData p0, SpannerData p1) in terminalPoints.Pairwise()) {
        if (p0.marginPoint == midribRef.p1) didPass = true;

        float mult = 1f;
        if ((p0.marginPoint == terminalPoints.First().marginPoint || p1.marginPoint == terminalPoints.Last().marginPoint)
            && VAL(LPK.SpannerLerp) > 0f)
          mult = 3f; //need more juice at the lobes

        float squeeze = VAL(LPK.SpannerSqueeze);
        LeafCurve curve = new LeafCurve(
          p0.marginPoint,
          Vector3.Lerp(p0.marginPoint, p1.marginPoint, squeeze),
          Vector3.Lerp(p0.marginPoint, p1.marginPoint, 1f - squeeze),
          p1.marginPoint,
          LeafCurveType.Vein,
          didPass);
        LeafVein v = new LeafVein(curve, this, deps,
          VAL(LPK.SpannerThickness), VAL(LPK.SpannerTaper), VAL(LPK.TaperRNG), LeafVeinType.MarginSpanning, didPass);
        v.startThickness = p0.thickness;
        v.endThickness = p1.thickness;
        if (p0.marginPoint == midribRef.p1) { //the lefty tip needs to be reveserved for some reason
          v.startThickness = p1.thickness;
          v.endThickness = p0.thickness;
        }

        v.h0 = VectorExtensions.ExtraLerp(v.h0, p0.rootPoint, -0.2f * VAL(LPK.SpannerLerp) * mult);
        v.h1 = VectorExtensions.ExtraLerp(v.h1, p1.rootPoint, -0.2f * VAL(LPK.SpannerLerp) * mult);
        marginSpanningVeins.AddVein(v);
      }
      veinGroups.Add(marginSpanningVeins);
    }

    private void SplitVeins(LeafParamDict fields) {
      if (fields[LPK.VeinSplit].value <= 0f) return;

      for (int i = 3; i < veinGroups.Count; i++) {
        LeafVeinGroup group = veinGroups[i];
        LeafVein vein = group.veins[0];

        float p0 = veinGroups[i - 1].veins[0].pointAlongMargin;
        float p1 = vein.pointAlongMargin;
        float avgBias = VAL(LPK.VeinSplitAmp);
        float pos = p0 * avgBias + p1 * (1f - avgBias);

        Vector3 point = PointAlongMargin(pos, linearPoints).point;
        point = Vector3.Lerp(point, vein.p0, VAL(LPK.VeinDistFromMargin));

        float randDepth = VAL(LPK.VeinSplitDepth) * BWRandom.RangeMult(0.25f);
        group.SplitVein(point, randDepth, VAL(LPK.VeinEndLerp));
      }
    }

    public static (Vector3 point, float perpendicular) PointAlongMargin(float linearPoint, List<Vector3> linearPoints) {
      if (linearPoint > linearPoints.Count) {
        Debug.LogError("PointAlongMargin pos " + linearPoint + " greater than LP count " + linearPoints.Count);
        return (Vector3.zero, 0f);
      }

      int idx = (int)linearPoint;
      Vector3 marginStartPoint = linearPoints[idx];
      Vector3 marginEndPoint = idx < linearPoints.Count - 1 ? linearPoints[idx + 1] : linearPoints[idx];
      float angle = CurveHelpers.Angle(marginStartPoint, marginEndPoint);
      Vector3 lerpPoint = Vector3.Lerp(marginStartPoint, marginEndPoint, linearPoint - (float)idx);
      // Vector3 modPoint = lerpPoint.AddPolar(new Polar(distFromMargin, angle - Polar.HalfPi));
      return (lerpPoint, angle - Polar.HalfPi);
    }

    public static Vector3 FindPointOnEdgeWithY(float y, List<Vector3> linearPoints, bool goingUp = false) {
      if (linearPoints.Count == 0) return Vector3.zero;
      if (linearPoints.Count == 1) return linearPoints[0];

      int start = goingUp ? 0 : 1;
      Vector3 lastPoint = linearPoints[start]; //start with the 1-2 segment
      for (int i = start + 1; i < linearPoints.Count; i++) {
        Vector3 nextPoint = linearPoints[i];
        if ((goingUp && y >= lastPoint.y && y <= nextPoint.y + 0.01f) ||
            (!goingUp && y < lastPoint.y && y >= nextPoint.y - 0.01f)) {
          float span = nextPoint.y - lastPoint.y;
          float perc = (y - lastPoint.y) / span;
          return Vector3.Lerp(lastPoint, nextPoint, perc);
        }
        lastPoint = nextPoint;
      }
      if (y.SoftEquals(linearPoints.First().y)) return linearPoints.First();
      //Debug.LogWarning("Found no points on edge with y " + y + " | " + linearPoints.ToLog());
      return linearPoints.First();
    }

    private static int FindNextLinearPointIndexFrom(Vector3 point, List<Vector3> linearPoints) {
      Vector3 lastPoint = linearPoints.First();
      int bestIndex = -1;
      float bestDist = float.MaxValue;
      for (int i = 1; i < linearPoints.Count; i++) {
        Vector3 nextPoint = linearPoints[i];
        float dist = Vector3.Distance(lastPoint, point) + Vector3.Distance(nextPoint, point);
        if (dist < bestDist) {
          bestDist = dist;
          bestIndex = i;
        }
        lastPoint = nextPoint;
      }
      if (bestIndex == -1) {
        Debug.LogError("No next linear point found from " + point);
        return 0;
      }
      return bestIndex;
    }

    public static float FindExactLinearPointFrom(Vector3 point, List<Vector3> linearPoints) {
      int nextIdx = FindNextLinearPointIndexFrom(point, linearPoints);
      if (nextIdx == 0) return nextIdx;
      Vector3 mp0 = PointAlongMargin(nextIdx - 1, linearPoints).point;
      Vector3 mp1 = PointAlongMargin(nextIdx, linearPoints).point;
      float d0 = Vector3.Distance(point, mp0);
      float d1 = Vector3.Distance(point, mp1);
      float remainder = d0 / (d0 + d1);
      return (float)nextIdx - 1f + remainder;
    }

    public Vector3 FindGravityIntersectionPoint(List<Vector3> gravPoints, Vector3 p0, Vector3 p1, float lerp) {
      Vector3 target = Vector3.Lerp(p0, p1, lerp); //TODO
      float close = float.MaxValue;
      int idx = 0;
      foreach (Vector3 gp in gravPoints) {
        float dist = Vector3.Distance(gp, target);
        if (dist < close) close = dist;
        else break;
        idx++;
      }

      if (p0.SoftEquals(p1)) return p0;

      bool checkForward = idx + 1 < gravPoints.Count;
      if (idx >= gravPoints.Count) idx--;

      (Vector2 i0, bool error) = checkForward ?
        (VectorExtensions.GetIntersection(p0, p1, gravPoints[idx], gravPoints[idx + 1])) : (Vector2.zero, true);
      if (error && idx > 0)
        (i0, error) = VectorExtensions.GetIntersection(p0, p1, gravPoints[idx], gravPoints[idx - 1]);
      return error ? gravPoints[idx] : i0;
    }

    private float GravityBiasAtPerc(float perc) {
      return Mathf.Lerp(VAL(LPK.GravVeinUpperBias), VAL(LPK.GravVeinLowerBias), perc);
    }

    private List<Vector3> GravityCurvePoints(Vector3 origin, Vector3 tip, int linearPointIdx,
        float upperBias = 0.5f, float lowerBias = 0.5f, float distFromMargin = 0f) {
      List<Vector3> points = new List<Vector3>();

      int startPoint = linearPointIdx;
      int steps = linearPoints.Count - startPoint; //-1?

      for (int i = 0; i < steps; i++) {
        float perc = (float)i / ((float)steps - 1f);
        Vector3 mainVeinLerp = Vector3.Lerp(origin, tip, perc);
        Vector3 sidePoint = linearPoints[startPoint + i];
        float bias = Mathf.Lerp(upperBias, lowerBias, i / ((int)steps - 1f));
        bias *= (1f - distFromMargin);
        Vector3 lerp = Vector3.Lerp(mainVeinLerp, sidePoint, bias);
        points.Add(lerp);
        //Debug.Log("perc: " + perc + " | sidePoint: " + sidePoint);
      }

      return points;
    }

    public static List<Vector3> CalcLinearPoints(List<Curve3D> curves,
        int renderLineSteps = 16,
        float targetIncrement = 0.5f,
        float offset = 0f, //0-1f
        bool tryDupeLast = true
    ) {
      Vector3 lastPoint;
      float sum = 0f;
      List<Vector3> cachedPoints = new List<Vector3>();
      foreach (Curve3D curve in curves) {
        float lastSum = sum;
        lastPoint = curve.p0;
        for (int i = 1; i <= renderLineSteps; i++) {
          cachedPoints.Add(lastPoint);
          Vector3 nextPoint = curve.GetPoint(i / (float)renderLineSteps);
          float dist = Vector3.Distance(lastPoint, nextPoint);
          lastPoint = nextPoint;
          sum += dist;
        }
        // if (deps.logOptions.logLengthCalcs) Debug.Log("curve" + count++ + " length: " + (sum - lastSum) + " | " + sum);
        lastSum = sum;
      }
      if (tryDupeLast) cachedPoints.Add(curves.Last().p1);

      List<Vector3> linearPoints = new List<Vector3>();

      float div = sum / targetIncrement;
      targetIncrement = sum / (float)Math.Floor(div); //make it so there is no remainder

      float runningDist = 0f;
      lastPoint = cachedPoints.First();
      linearPoints.Add(lastPoint);
      foreach (Vector3 nextPoint in cachedPoints) {
        if (nextPoint == lastPoint) continue; //first step

        float thisDist = Vector3.Distance(lastPoint, nextPoint);
        runningDist += thisDist;
        int failsafe = 0;
        while (runningDist >= targetIncrement) {
          float extra = runningDist - targetIncrement;
          float onThisLine = targetIncrement - (runningDist - thisDist);
          float onThisLinePerc = onThisLine / thisDist;
          Vector3 targetPoint = Vector3.Lerp(lastPoint, nextPoint, onThisLinePerc);
          linearPoints.Add(targetPoint);
          runningDist -= targetIncrement;
          if (failsafe++ >= 25) {
            Debug.LogError("SolveLength infinite loop");
            break;
          }
        }
        lastPoint = nextPoint;
      }

      if (tryDupeLast) {
        Vector3 last = curves.Last().p1;
        if (Vector3.Distance(last, linearPoints.Last()) > 0.1f) linearPoints.Add(last);
      }

      // if (deps.logOptions.logLengthCalcs) {
      //   Debug.Log("targetIncrement: " + targetIncrement);
      //   Debug.Log("cachedPoints: " + cachedPoints.ToLog());
      //   Debug.Log("linearPoints: " + linearPoints.ToLog());
      //   Debug.Log("total length: " + sum + " in " + deps.baseParams.RenderLineSteps + " steps");
      // }

      Vector3 orig0 = linearPoints[0];
      for (int i = 0; i < linearPoints.Count - 1; i++) {
        linearPoints[i] = Vector3.Lerp(linearPoints[i], linearPoints[i + 1], offset);
      }
      linearPoints[linearPoints.Count - 1] = Vector3.Lerp(linearPoints[linearPoints.Count - 1], orig0, offset);

      return linearPoints;
    }

    public IEnumerable<LeafVein> GetVeins(Transform t = null) {
      foreach (LeafVeinGroup group in veinGroups) {
        foreach (LeafVein vein in group.veins) {
          if (t == null) yield return vein;
          else yield return vein.Transform(t);
        }
      }
    }

    public Dictionary<LeafVeinType, List<LeafVein>> CollateVeins(bool lefties) {
      Dictionary<LeafVeinType, List<LeafVein>> d = new Dictionary<LeafVeinType, List<LeafVein>>();
      foreach (LeafVein vein in GetVeins()) {
        if (!d.ContainsKey(vein.type)) d[vein.type] = new List<LeafVein>();
        if (vein.lefty == lefties) d[vein.type].Add(vein);
      }
      return d;
    }

    public float GetMidribThicknessAtPercent(float perc) => GetMidrib().PolyWidthAtPercent(perc);

    public List<LeafVein> GetVeinsWithType(LeafVeinType type) {
      List<LeafVein> l = new List<LeafVein>();
      foreach (LeafVein v in GetVeins()) {
        if (v.type == type) l.Add(v);
      }
      return l;
    }

    public LeafVein GetMidrib() => GetVeinsWithType(LeafVeinType.Midrib).First();
    public Curve3D GetFullLengthMidrib() {
      Curve3D c = new Curve3D(calcs.origin, calcs.tip);
      c.SpreadHandlesEvenly();
      return c;
    }

    public List<Vector3[]> GetPuffyPolys() {
      Dictionary<LeafVeinType, List<LeafVein>> veinDictR = CollateVeins(false);
      Dictionary<LeafVeinType, List<LeafVein>> veinDictL = CollateVeins(true);
      List<Vector3[]> polys = new List<Vector3[]>();

      //others[0] and others[last] must contact rib at p0
      const int acc = 10;
      Vector3[] CreatePoly(LeafVein rib, FloatRange ribRange, bool revRib, int[] revIndices, params LeafVein[] others) {
        List<Vector3> points = new List<Vector3>();

        LeafVein cFirst = others.First();
        LeafVein cLast = others.Last();

        int idx = 0;
        foreach (LeafVein v in others) {
          Vector3[] locPoints = Array.ConvertAll<Vector2, Vector3>(LeafRenderer.GetPolyPathPoints(v, acc), v => v);
          bool rev = revIndices.Contains(idx);
          points.AddRange(rev ? locPoints.Reversed() : locPoints);
          idx++;
        }

        if (rib != null) {
          Curve ribSlice = rib.GetSlice(1f - ribRange.Start, 1f - ribRange.End);
          Vector3[] ribPath = Array.ConvertAll<Vector2, Vector3>(LeafRenderer.GetPolyPathPoints(ribSlice, acc), v => v);

          if (revRib) ribPath = ribPath.Reversed();
          points.AddRange(ribPath);
        }

        return points.ToArray();
      }

      List<Vector3[]> SplitPolysMain(LeafVeinType ribType, LeafVeinType secondaryType,
          bool lefty, bool lobes, bool splits, params LeafVein[] extras) {
        Dictionary<LeafVeinType, List<LeafVein>> dict = lefty ? veinDictL : veinDictR;
        if (!dict.ContainsKey(ribType))
          return new List<Vector3[]>();

        List<Vector3[]> vec = new List<Vector3[]>();
        LeafVein rib = veinDictR[ribType][0];
        if (lobes && lefty) rib = veinDictL[ribType][0];
        List<LeafVein> margins = dict[LeafVeinType.MarginSpanning];
        List<LeafVein> sePrimaries = splits ? dict[LeafVeinType.SplitEndPrimary] : new List<LeafVein>();
        List<LeafVein> seSecondaries = splits ? dict[LeafVeinType.SplitEndSecondary] : new List<LeafVein>();
        List<LeafVein> mains = new List<LeafVein>();
        if (dict.ContainsKey(secondaryType))
          mains = dict[secondaryType];
        mains.AddRange(extras.ToList());

        List<int> _revs = new List<int>();
        if (splits) {
          if (lefty) _revs.Add(2);
          _revs.Add(3);
          _revs.Add(4);
        } else {
          if (lefty) _revs.Add(1);
          _revs.Add(2);
        }
        int[] revs = _revs.ToArray();

        //main pairs
        List<LeafVein>[] others = splits ? new List<LeafVein>[] { sePrimaries, margins, seSecondaries, mains } :
          new List<LeafVein>[] { margins, mains };
        foreach ((LeafVein c1, LeafVein c2) in mains.Pairwise()) {
          List<LeafVein> connectedVeins = FindConnectedVeins(c1, revs, others);
          vec.Add(CreatePoly(rib,
            new FloatRange(c1.posAlongMidrib, c2.posAlongMidrib, 0f),
            !(lefty && lobes),
            revs,
            connectedVeins.ToArray()));
        }

        if (!lobes) { //main section
                      //end piece
          List<LeafVein>[] endOthers = splits ? new List<LeafVein>[] { sePrimaries, margins } :
            new List<LeafVein>[] { margins };
          List<LeafVein> p = FindConnectedVeins(mains.Last(), revs, endOthers);
          vec.Add(CreatePoly(rib,
            new FloatRange(mains.Last().posAlongMidrib, 0f, 0f),
            true,
            revs,
            p.ToArray()));
        } else if (dict.ContainsKey(LeafVeinType.LobeToMargin)) { //lobe section
                                                                  //unsplit second piece
          LeafVein topVein = topVein = dict[LeafVeinType.LobeToMargin].First();
          List<LeafVein> p;
          if (splits) {
            revs = lefty ? new int[] { 1, 2, 3 } : new int[] { 2, 3 };
            p = FindConnectedVeins(topVein, revs, margins, seSecondaries, mains);
            vec.Add(CreatePoly(rib,
              new FloatRange(mains.First().posAlongMidrib, topVein.posAlongMidrib, 0f), //something wrong here
              true,
              revs,
              p.ToArray()));
          }

          //top piece
          revs = lefty ? new int[] { } : new int[] { 1 };
          p = FindConnectedVeins(topVein, revs, margins);
          vec.Add(CreatePoly(rib,
            new FloatRange(topVein.posAlongMidrib, 0f, 0f),
            true,
            revs,
            p.ToArray()));
        }

        return vec;
      } //end SplitPolysMain

      List<Vector3[]> SplitPolysSplitEnds(bool lefty) {
        Dictionary<LeafVeinType, List<LeafVein>> dict = lefty ? veinDictL : veinDictR;
        List<LeafVein> sePrimaries = dict[LeafVeinType.SplitEndPrimary];
        List<LeafVein> seSecondaries = dict[LeafVeinType.SplitEndSecondary];
        List<LeafVein> margins = dict[LeafVeinType.MarginSpanning];
        List<Vector3[]> vec = new List<Vector3[]>();

        int[] revs = lefty ? new int[] { 1, 2 } : new int[] { 2 };
        foreach (LeafVein secondary in seSecondaries) {
          vec.Add(CreatePoly(null, null, false, revs,
            FindConnectedVeins(secondary, revs, margins, sePrimaries).ToArray()));
        }

        return vec;
      }

      if (!veinDictR.ContainsKey(LeafVeinType.MidToSplit)) {
        polys.AddRange(SplitPolysMain(LeafVeinType.Midrib, LeafVeinType.MidToMargin, false, false, false));
        polys.AddRange(SplitPolysMain(LeafVeinType.Midrib, LeafVeinType.MidToMargin, true, false, false));

        polys.AddRange(SplitPolysMain(LeafVeinType.LobeRib, LeafVeinType.LobeToMargin, false, true, false,
          veinDictR[LeafVeinType.MidToMargin].First()));
        polys.AddRange(SplitPolysMain(LeafVeinType.LobeRib, LeafVeinType.LobeToMargin, true, true, false,
          veinDictL[LeafVeinType.MidToMargin].First()));

      } else {
        polys.AddRange(SplitPolysMain(LeafVeinType.Midrib, LeafVeinType.MidToSplit, false, false, true));
        polys.AddRange(SplitPolysMain(LeafVeinType.Midrib, LeafVeinType.MidToSplit, true, false, true));

        polys.AddRange(SplitPolysMain(LeafVeinType.LobeRib, LeafVeinType.LobeToSplit, false, true, true,
          veinDictR[LeafVeinType.MidToSplit].First()));
        polys.AddRange(SplitPolysMain(LeafVeinType.LobeRib, LeafVeinType.LobeToSplit, true, true, true,
          veinDictL[LeafVeinType.MidToSplit].First()));

        polys.AddRange(SplitPolysSplitEnds(false));
        polys.AddRange(SplitPolysSplitEnds(true));
      }

      return polys;
    }

    private static List<LeafVein> FindConnectedVeins(LeafVein first, //Vector3 anchor,
        int[] reversed, params List<LeafVein>[] others) {
      Vector3 nextPoint = reversed.Contains(0) ? first.p0 : first.p1;
      List<LeafVein> retList = new List<LeafVein>();
      retList.Add(first);
      int idx = 0;
      foreach (List<LeafVein> veins in others) {
        idx++;
        foreach (LeafVein v in veins) {
          (Vector3 p0, Vector3 p1) = !reversed.Contains(idx) ? (v.p0, v.p1) : (v.p1, v.p0);
          if (p0 == nextPoint) {
            nextPoint = p1;
            retList.Add(v);
            break;
          }
        }
      }
      return retList;
    }

    private float VAL(LPK key) => fields[key].value;
    private LeafParam LP(LPK key) => fields[key];
  }

}
