/**
 * @author mattatz / http://mattatz.github.io
 *
 * Ruppert's Delaunay Refinement Algorithm
 * Jim Ruppert. A Delaunay Refinement Algorithm for Quality 2-Dimensional Mesh Generation / http://www.cis.upenn.edu/~cis610/ruppert.pdf
 * The Quake Group at Carnegie Mellon University / https://www.cs.cmu.edu/~quake/tripaper/triangle3.html
 * Wikipedia / https://en.wikipedia.org/wiki/Ruppert%27s_algorithm
 * ETH zurich CG13 Chapter 7 / http://www.ti.inf.ethz.ch/ew/Lehre/CG13/lecture/Chapter%207.pdf
 */

using System;
using System.Collections.Generic;
using System.Linq;
using BionicWombat;
using UnityEngine;

namespace mattatz.Triangulation2DSystem {
  public enum TriangulationMode {
    Classic,
    Uniform,
    Simple
  }

  public class Triangulation2D {

    const float kAngleMax = 30f;

    public Polygon2D Polygon { get { return PSLG; } }
    public Triangle2D[] Triangles { get { return tris.ToArray(); } }
    public List<Segment2D> Edges { get { return dtSegs; } }
    public List<Vertex2D> Points { get { return dtVerts; } }

    List<Vertex2D> baseVerts = new List<Vertex2D>(); // vertices in PSLG
    List<Segment2D> baseSegs = new List<Segment2D>(); // segments in PSLG

    List<Vertex2D> dtVerts = new List<Vertex2D>(); // vertices in DT
    List<Segment2D> dtSegs = new List<Segment2D>(); // segments in DT

    Polygon2D PSLG;
    List<Triangle2D> tris = new List<Triangle2D>();
    private Mesh sharedMesh;
    private Vector3[] sharedVerts;
    private List<int> sharedTris;
    TriangulationMode mode;
    int density;
    bool shouldRetryIfNecessary;

    public Triangulation2D() {
      sharedMesh = new Mesh();
      sharedMesh.name = "Tri2D sharedMesh";
    }

    public bool Triangulate(Polygon2D polygon, int density, string randomBS, TriangulationMode mode = TriangulationMode.Uniform, bool shouldRetryIfNecessary = false, float angle = 20f, float threshold = 0.1f) {
      angle = Mathf.Min(angle, kAngleMax) * Mathf.Deg2Rad;
      this.mode = mode;
      this.density = density;
      this.shouldRetryIfNecessary = shouldRetryIfNecessary;

      PSLG = polygon;
      baseVerts = PSLG.Vertices.ToList();
      baseSegs = PSLG.Segments.ToList();
      return _Triangulate(polygon.Vertices.Select(v => v.Coordinate).ToArray(), density, angle, threshold, randomBS);
    }

    public void Build() {
      Build((Vertex2D v) => {
        var xy = v.Coordinate;
        return new Vector3(xy.x, xy.y, 0f);
      });
    }

    public void Build(Func<Vertex2D, Vector3> coord) {
      sharedVerts = dtVerts.Select(p => {
        return coord(p);
      }).ToArray();

      sharedTris = new List<int>();
      for (int i = 0, n = tris.Count; i < n; i++) {
        var t = tris[i];
        int a = vertsDict[t.a.Coordinate], b = vertsDict[t.b.Coordinate], c = vertsDict[t.c.Coordinate];
        if (a < 0 || b < 0 || c < 0) {
          // Debug.Log(a + " : " + b + " : " + c);
          continue;
        }
        if (Utils2D.LeftSide(t.a.Coordinate, t.b.Coordinate, t.c.Coordinate)) {
          sharedTris.Add(a); sharedTris.Add(c); sharedTris.Add(b);
        } else {
          sharedTris.Add(a); sharedTris.Add(b); sharedTris.Add(c);
        }
      }
    }

    public static int[] FilterTris(int[] tris) {
      List<int> l = new List<int>();
      for (int i = 0; i < tris.Length; i += 3) {
        int a = tris[i];
        int b = tris[i + 1];
        int c = tris[i + 2];
        if (a == b || b == c || a == c) {
          continue;
        }
        l.Add(a, b, c);
      }
      return l.ToArray();
    }

    public Mesh GetMesh() { //check main thread
      if (sharedMesh == null) {
        Debug.LogError("sharedMesh is null");
        return null;
      }
      if (sharedVerts == null || sharedTris == null) {
        Debug.LogError("Build must be called before GetMesh()");
        return null;
      }

      sharedMesh.vertices = sharedVerts;
      sharedTris = FilterTris(sharedTris.ToArray()).ToList();
      sharedMesh.SetTriangles(sharedTris.ToArray(), 0);
      sharedMesh.RecalculateNormals();

      // string s = "";
      // int idx = 0;
      // foreach (Vector2 v in sharedVerts) {
      //   s += idx++ + ": " + v + "  ---  ";
      // }
      // Debug.Log(s);

      // s = "";
      // int count = 0;
      // List<string> t = new List<string>();
      // foreach (int i in sharedTris) {
      //   if (count == 0) s += "(";
      //   s += i;
      //   if (count < 2) s += ", ";
      //   else s += ")";
      //   if (++count >= 3) {
      //     count = 0;
      //     t.Add(s);
      //     s = "";
      //   }
      // }
      // Debug.Log(t.ToLogShort());

      // foreach (Triangle2D tri in T) {
      //   Debug.Log(HasOuterSegments(tri));
      // }

      return sharedMesh;
    }

    public Vertex2D[] GetMarginVerts() {
      return baseVerts.ToArray();
    }

    Dictionary<Vector2, int> vertsDict = new Dictionary<Vector2, int>(new EqEqEqualityComparer());
    int FindVertex(Vector2 p, List<Vertex2D> Vertices) {
      if (mode == TriangulationMode.Uniform) {
        return vertsDict.ContainsKey(p) ? vertsDict[p] : -1;
      } else {
        return Vertices.FindIndex(v => {
          return v.Coordinate == p;
        });
      }
    }

    Dictionary<Segment2D, int> segsDict = new Dictionary<Segment2D, int>(new SegComparer());
    int FindSegment(Vertex2D a, Vertex2D b, List<Segment2D> Segments) {
      // if (uniform) {
      Segment2D seg = new Segment2D(a, b);
      return segsDict.ContainsKey(seg) ? segsDict[seg] : -1;
      // } else {
      //   Segment2D seg = new Segment2D(a, b);
      //   int r = segsDict.ContainsKey(seg) ? segsDict[seg] : -1;
      //   int fi = Segments.FindIndex(s => (s.a == a && s.b == b) || (s.a == b && s.b == a));
      //   if (r != fi && fi < 10) {
      //     Debug.Log("mismatch: " + r + " | " + fi + " | " + seg);
      //     SegComparer c = new SegComparer();
      //     Debug.Log("seg(" + seg.a + ", " + seg.b + "):[" + seg.a.GetHashCode() + ", " + seg.b.GetHashCode() + "] ");
      //     Debug.Log("segsDict addresses: " + segsDict.Keys.Select(s => "(" + s.a + ", " + s.b + "):[" + s.a.GetHashCode() + ", " + s.b.GetHashCode() + "] ").ToList().ToLog());
      //     Debug.Log("segsDict: " + segsDict.ToLog());
      //     Debug.Log("Segments: " + Segments.ToLog());
      //   }
      //   return fi;
      // }
    }

    public Vertex2D CheckAndAddVertex(Vector2 coord) => CheckAndAddVertex(coord, Vector2.zero);

    public Vertex2D CheckAndAddVertex(Vector2 coord, Vector2 offset) {
      Vector2 offsetCoord = coord + offset;
      var idx = FindVertex(offsetCoord, dtVerts);
      if (idx < 0) {
        var v = new Vertex2D(offsetCoord);
        dtVerts.Add(v);
        vertsDict[offsetCoord] = dtVerts.Count - 1;
        return v;
      }
      return dtVerts[idx];
    }

    public Segment2D CheckAndAddSegment(Vertex2D a, Vertex2D b) {
      var idx = FindSegment(a, b, dtSegs);
      Segment2D s;
      if (idx < 0) {
        s = new Segment2D(a, b);
        dtSegs.Add(s);
        segsDict[s] = dtSegs.Count - 1;
      } else {
        s = dtSegs[idx];
      }
      s.Increment();
      return s;
    }

    public Triangle2D AddTriangle(Vertex2D a, Vertex2D b, Vertex2D c) {
      var s0 = CheckAndAddSegment(a, b);
      var s1 = CheckAndAddSegment(b, c);
      var s2 = CheckAndAddSegment(c, a);
      var t = new Triangle2D(s0, s1, s2);
      tris.Add(t);
      return t;
    }

    public void RemoveTriangle(Triangle2D t, bool markOrphans = false) {
      var idx = tris.IndexOf(t);
      if (idx < 0) return;

      tris.RemoveAt(idx);
      if (t.s0.Decrement() <= 0) RemoveSegment(t.s0);
      if (t.s1.Decrement() <= 0) RemoveSegment(t.s1);
      if (t.s2.Decrement() <= 0) RemoveSegment(t.s2);
    }

    private void RebuildVertsDict() {
      vertsDict.Clear();
      foreach ((Vertex2D v, int idx) in dtVerts.WithIndex()) {
        vertsDict[v.Coordinate] = idx;
      }

      segsDict.Clear();
      foreach ((Segment2D s, int idx) in dtSegs.WithIndex()) {
        segsDict[s] = idx;
      }
    }

    public void RemoveTriangle(Segment2D s) {
      tris.FindAll(t => t.HasSegment(s)).ForEach(t => RemoveTriangle(t));
    }

    public void RemoveSegment(Segment2D s) {
      dtSegs.Remove(s);
      segsDict.Remove(s);
      if (s.a.ReferenceCount <= 0) { dtVerts.Remove(s.a); vertsDict.Remove(s.a.Coordinate); }
      if (s.b.ReferenceCount <= 0) { dtVerts.Remove(s.b); vertsDict.Remove(s.b.Coordinate); }
    }

    void Bound(Vector2[] points, out Vector2 min, out Vector2 max) {
      min = Vector2.one * float.MaxValue;
      max = Vector2.one * float.MinValue;
      for (int i = 0, n = points.Length; i < n; i++) {
        var p = points[i];
        min.x = Mathf.Min(min.x, p.x);
        min.y = Mathf.Min(min.y, p.y);
        max.x = Mathf.Max(max.x, p.x);
        max.y = Mathf.Max(max.y, p.y);
      }
    }

    private float sqrt3 = Mathf.Sqrt(3f);
    public Triangle2D AddExternalTriangle(Vector2 min, Vector2 max) {
      Vector2 center = (max + min) * 0.5f;
      float diagonal = (max - min).magnitude;
      float dh = diagonal * 0.5f;
      float rdh = sqrt3 * dh;
      return AddTriangle(
        CheckAndAddVertex(center + new Vector2(-rdh, -dh) * 3f),
        CheckAndAddVertex(center + new Vector2(rdh, -dh) * 3f),
        CheckAndAddVertex(center + new Vector2(0f, diagonal) * 3f)
      );
    }

    enum LinkMeshEdgesResult {
      Success,
      OrphanTris,
      InfiniteLoop,
    }

    bool _Triangulate(Vector2[] points, int density, float angle, float threshold, string randomBS) {
      Vector2 min, max;
      Bound(points, out min, out max);

      AddExternalTriangle(min, max);

      if (mode == TriangulationMode.Uniform) {
        BuildMeshWeb(points, min, max, density);
        RemoveExternalPSLG();
        RemoveSegsCrossingBorder();
        LinkMeshEdgesResult res = LinkMeshEdges(points, randomBS);

        //respond to errors
        if (res == LinkMeshEdgesResult.InfiniteLoop || res == LinkMeshEdgesResult.OrphanTris) {
          // DebugBW.Log("res: " + res);
          tris.Clear();
          dtVerts.Clear();
          dtSegs.Clear();

          if (shouldRetryIfNecessary) {
            // Debug.Log("Retriangulating with old setup: " + res);
            // if (density == LeafFactory.DensityForRenderQuality(Plant.RenderQuality.Maximum, 0)) {
            //   DebugBW.Log("here", LColor.lightblue);
            //   return Triangulate(PSLG, 300, randomBS, TriangulationMode.Uniform, false);
            // } else {
            return Triangulate(PSLG, 0, randomBS, TriangulationMode.Classic, false);
            // }
          } else {
            return false;
          }
        }

      } else if (mode == TriangulationMode.Classic) {
        for (int i = 0, n = points.Length; i < n; i++) {
          var v = points[i];
          UpdateTriangulation(v);
          // if (i == 30) break;
        }

        Refine(angle, threshold);
        RemoveExternalPSLG();
      } else if (mode == TriangulationMode.Simple) {
        BuildSimpleMesh(points, min, max);
        RemoveExternalPSLG();
      }

      return true;
    }

    private void BuildSimpleMesh(Vector2[] points, Vector2 min, Vector2 max) {
      Vertex2D center = CheckAndAddVertex((min + max) / 2f); //middle
      for (int i = 1; i < points.Length; i++) {
        AddTriangle(center, CheckAndAddVertex(points[i]), CheckAndAddVertex(points[i - 1]));
      }
    }

    void BuildMeshWeb(Vector2[] points, Vector2 min, Vector2 max, int density) {
      float totalLen = max.y - min.y;
      float len = totalLen / (density * sqrt3 / 2f);
      float halfLen = len / 2f;
      float h = sqrt3 / 2f * len;
      Vector2 offset = new Vector2(totalLen / -2f, min.y);
      offset = new Vector2(offset.x - (offset.x % len), offset.y - (offset.y % h));
      PerfTools pt = new PerfTools("BuildMeshWeb", false);

      Vector2 TriPointFrom(int x, int y) {
        float xRet = x * len + ((y % 2 == 1) ? halfLen : 0f);
        float yRet = y * h;
        return new Vector2(xRet, yRet);
      };

      for (int y = 0; y < density; y++) {
        for (int x = 0; x < density; x++) {
          if (y % 2 == 0) {
            AddTriangle(
              CheckAndAddVertex(TriPointFrom(x, y), offset),
              CheckAndAddVertex(TriPointFrom(x, y + 1), offset),
              CheckAndAddVertex(TriPointFrom(x + 1, y), offset)
            );
            AddTriangle(
              CheckAndAddVertex(TriPointFrom(x + 1, y), offset),
              CheckAndAddVertex(TriPointFrom(x, y + 1), offset),
              CheckAndAddVertex(TriPointFrom(x + 1, y + 1), offset)
            );

          } else {
            AddTriangle(
              CheckAndAddVertex(TriPointFrom(x, y), offset),
              CheckAndAddVertex(TriPointFrom(x + 1, y + 1), offset),
              CheckAndAddVertex(TriPointFrom(x + 1, y), offset)
            );
            AddTriangle(
              CheckAndAddVertex(TriPointFrom(x + 1, y), offset),
              CheckAndAddVertex(TriPointFrom(x + 1, y + 1), offset),
              CheckAndAddVertex(TriPointFrom(x + 2, y + 1), offset)
            );
          }
        }
      }
      pt.Split();
    }

    void RemoveSegsCrossingBorder() {
      List<Segment2D> segsCrossingBorder = new List<Segment2D>();
      foreach (Segment2D seg in dtSegs) {
        if (seg.ReferenceCount == 1 && IsSegMidpointOutsidePoly(seg)) {
          segsCrossingBorder.Add(seg);
        }
      }
      foreach (Segment2D seg in segsCrossingBorder) {
        RemoveTriangle(seg);
      }
      if (segsCrossingBorder.Count > 0) RebuildVertsDict();
    }

    public static List<Segment2D> debugSegs;
    public static List<Vertex2D> debugInners;
    LinkMeshEdgesResult LinkMeshEdges(Vector2[] points, string randomBS) {
      //find orphan edges
      List<Segment2D> segOrphans = new List<Segment2D>();
      foreach (Segment2D seg in dtSegs) {
        if (seg.ReferenceCount == 1) {
          segOrphans.Add(new Segment2D(seg.a, seg.b));
        }
      }
      // Debug.Log("segOrphans(" + segOrphans.Count + "): " + segOrphans.ToLog());

      //sort orphan edges
      int segsCount = segOrphans.Count;
      if (segsCount == 0) return LinkMeshEdgesResult.InfiniteLoop;
      Segment2D activeSeg = segOrphans.First();
      segOrphans.Remove(activeSeg);
      List<Segment2D> orderedSegs = new List<Segment2D>();
      int count = 0;
      while (true) {
        bool didAdd = false;
        //Debug.Log("segOrphans: " + segOrphans.ToLog());
        foreach (Segment2D seg in segOrphans) {
          if (activeSeg.b == seg.a || activeSeg.b == seg.b) {
            if (activeSeg.b == seg.b) seg.Swap();
            orderedSegs.Add(activeSeg);
            activeSeg = seg;
            didAdd = true;
            break;
          }
        }
        if (didAdd) {
          segOrphans.Remove(activeSeg);
          //Debug.Log("DidAdd: " + segOrphans.Count + " | activeSeg: " + activeSeg);
        } else {
          // Debug.LogWarning("NOADD activeSeg: " + activeSeg);
          return LinkMeshEdgesResult.OrphanTris;
        }

        if (segOrphans.Count == 0) {
          //Debug.Log("Finished sorting " + orderedSegs.Count + " orphans");
          break;
        }
        if (count++ > segsCount + 1) {
          Debug.Log("count: " + count + " | segOrphans.Count: " + segOrphans.Count);
          Debug.Log("activeSeg: " + activeSeg);
          Debug.Log("orderedSegs: " + orderedSegs.ToLog());
          Debug.LogError("LinkMeshEdges infinite loop");
          return LinkMeshEdgesResult.InfiniteLoop;
        }
      }
      // Debug.Log("orderedSegs (" + orderedSegs.Count + "): " + orderedSegs.ToLog());
      debugSegs = orderedSegs;

      //get orphan verts from ordered edges
      List<Vertex2D> innerOrphans = new List<Vertex2D>();
      foreach (Segment2D seg in orderedSegs) {
        if (seg == orderedSegs[0]) {
          innerOrphans.Add(seg.a);
        }
        innerOrphans.Add(seg.b);
      }
      innerOrphans = innerOrphans.Reversed();
      debugInners = innerOrphans;
      // Debug.Log("innerOrphans: " + innerOrphans.ToLog());

      //find closest inner point from outer point 0
      int innerIdx = 0;
      int innerIdx2 = 0;
      int outerIdx = 0;
      Vector2 outer = points[0];
      float min = float.MaxValue;
      float min2 = float.MaxValue;
      foreach ((Vertex2D vert, int idx) in innerOrphans.WithIndex()) {
        float dist = Vector2.Distance(vert.Coordinate, outer);
        // DebugBW.Log("vert.Coordinate: " + vert.Coordinate + " | dist: " + dist);
        if (dist < min && vert.Coordinate.x.SoftEquals(0f)) {
          min = dist;
          innerIdx = idx;
        }
        if (dist < min2) {
          min2 = dist;
          innerIdx2 = idx;
        }
      }
      if (min2 * 5f < min) innerIdx = innerIdx2; //magic number. We're trying to see if there is no origin inner
      // DebugBW.Log("min: " + min + " | min2: " + min2);
      // DebugBW.Log("points: " + points.ToLog());
      // DebugBW.Log("innerIdx: " + innerIdx + " | points[innerIdx]: " + innerOrphans[innerIdx]);

      //build tris around the outside
      Vector2 inner;
      int innerStart = innerIdx;
      count = 0;
      bool didSkipInner = false;
      // bool hasLooped = false;
      int skippy = 0;
      // Int32.TryParse(randomBS, out skippy);
      while (true) {
        if (++count > innerOrphans.Count + points.Length) {
          Debug.LogError("break via infinite loop");
          break;
        } else if (outerIdx >= points.Length - skippy) {
          // Debug.Log("break via outer idx, didSkipInner: " + didSkipInner + " | innerIdx: " + innerIdx + " | innerStart: " + innerStart);
          if (innerIdx < innerStart) { //is this needed?
            Vertex2D nextIV = innerOrphans.NextAfter(innerIdx);
            AddTriangle(nextIV, innerOrphans[innerIdx], CheckAndAddVertex(points[0]));
          }
          break;
          // } else if (innerIdx == innerStart && hasLooped) { //this can orphan inner verts
          //   Debug.Log("break via inner idx: " + outerIdx + " | " + points.Length);
          //   break;
        } else if (innerIdx >= innerOrphans.Count) {
          innerIdx = 0;
          // hasLooped = true;
          // DebugBW.Log("loop inner idx", LColor.magenta);
          // break;
        }

        Vertex2D innerVert = innerOrphans[innerIdx];
        inner = innerVert.Coordinate;
        outer = points[outerIdx];//(points[outerIdx] + points.NextAfter(outerIdx));

        bool DistCheckCondition1(float _min, int _innerIdx, int _outerIdx) {
          return Vector2.Distance(points[_outerIdx], innerOrphans.NextAfter(_innerIdx).Coordinate) < _min;
        };
        bool DistCheckCondition2(int _innerIdx, int _outerIdx) {
          return Vector2.Distance(points.NextAfter(_outerIdx), innerOrphans[_innerIdx].Coordinate) >
                 Vector2.Distance(points[_outerIdx], innerOrphans.NextAfter(_innerIdx).Coordinate);
        }

        //check for stray
        min = Vector2.Distance(outer, inner);
        if (didSkipInner && (DistCheckCondition1(min, innerIdx, outerIdx) || DistCheckCondition2(innerIdx, outerIdx))) {
          // DebugBW.Log("Cond1: " + DistCheckCondition1(min, innerIdx, outerIdx) + " | " + DistCheckCondition2(innerIdx, outerIdx));
          //outer wants to skip here too, so force an IIO
          Vertex2D nextIV = innerOrphans.NextAfter(innerIdx);
          AddTriangle(nextIV, innerVert, CheckAndAddVertex(outer));
          didSkipInner = false;
          innerIdx++;
          count--;
          // Debug.Log("force adding iio: " + nextIV.Coordinate + "  ---  " + innerVert.Coordinate + "  ---  " + outer);
          continue;
        } else { didSkipInner = false; } //maybe this is wrong?

        //build outer-outer-inner tri
        // DebugBW.Log("inner: " + inner + " | outer: " + outer);
        min = Vector2.Distance(outer, inner);
        // if (Vector2.Distance(outer, innerOrphans.PreviousFrom(innerIdx).Coordinate) < min) {
        //   Debug.LogError("prev dist less");
        //   return;
        // } else 
        if (DistCheckCondition1(min, innerIdx, outerIdx)) {
          //Debug.Log("skip outer: " + outer); //This seems to break things!
          // } else if (DistCheckCondition2(innerIdx, outerIdx)) {
          //   //Debug.Log("special skip outer: " + outer);
        } else {
          AddTriangle(innerVert, CheckAndAddVertex(outer), CheckAndAddVertex(points.NextAfter(outerIdx)));
          // Debug.Log("adding ioo: " + innerVert.Coordinate + "  ---  " + outer + "  ---  " + points.NextAfter(outerIdx));
          outerIdx++;
        }

        //build inner-inner-outer tri
        outer = outerIdx == points.Length ? points[0] : points[outerIdx];
        Vertex2D nextInnerVert = innerOrphans.NextAfter(innerIdx);
        Vector2 nextInner = nextInnerVert.Coordinate;
        min = Vector2.Distance(outer, nextInner);
        if (Vector2.Distance(nextInner, points.NextAfter(outerIdx)) < min ||
            Vector2.Distance(inner, outer) < min) {
          // Debug.Log("skip inner: " + inner);
          didSkipInner = true;
          continue;
        } else {
          AddTriangle(nextInnerVert, innerVert, CheckAndAddVertex(outer));
          // Debug.Log("adding iio: " + nextInnerVert.Coordinate + "  ---  " + innerVert.Coordinate + "  ---  " + outer);
          innerIdx++;
        }
      }

      return LinkMeshEdgesResult.Success;
    }

    void RemoveCommonTriangles(Triangle2D target) {
      for (int i = 0, n = tris.Count; i < n; i++) {
        var t = tris[i];
        if (t.HasCommonPoint(target)) {
          RemoveTriangle(t);
          i--;
          n--;
        }
      }
      RebuildVertsDict();
    }

    void RemoveExternalPSLG() {
      for (int i = 0, n = tris.Count; i < n; i++) {
        var t = tris[i];
        if (IsOutsidePoly(t) || HasOuterSegments(t)) {
          // Debug.Log(ExternalPSLG(t) ? "External: " + t : "Outer: " + t);
          RemoveTriangle(t, true);
          i--;
          n--;
        }
      }
      RebuildVertsDict();
    }

    bool ContainsSegments(Segment2D s, List<Segment2D> segments) {
      return segments.FindIndex(s2 =>
      (s2.a.Coordinate == s.a.Coordinate && s2.b.Coordinate == s.b.Coordinate) ||
      (s2.a.Coordinate == s.b.Coordinate && s2.b.Coordinate == s.a.Coordinate)
     ) >= 0;
    }

    bool HasOuterSegments(Triangle2D t) {
      //return IsSegMidpointOutsidePoly(t.s0) || IsSegMidpointOutsidePoly(t.s1) || IsSegMidpointOutsidePoly(t.s2);
      if (!ContainsSegments(t.s0, baseSegs)) {
        return IsSegMidpointOutsidePoly(t.s0);
      }
      if (!ContainsSegments(t.s1, baseSegs)) {
        return IsSegMidpointOutsidePoly(t.s1);
      }
      if (!ContainsSegments(t.s2, baseSegs)) {
        return IsSegMidpointOutsidePoly(t.s2);
      }
      return false;
    }

    bool DoEdgesCrossPolyBorder(Triangle2D t) {
      return IsSegMidpointOutsidePoly(t.s0) || IsSegMidpointOutsidePoly(t.s1) || IsSegMidpointOutsidePoly(t.s2);
    }

    void UpdateTriangulation(Vector2 p) {
      List<Triangle2D> tmpTris = new List<Triangle2D>();
      List<Segment2D> tmpSegs = new List<Segment2D>();

      Vertex2D v = CheckAndAddVertex(p);
      tmpTris = tris.FindAll(t => t.PointInsideCircumCircle(v));
      tmpTris.ForEach(t => {
        tmpSegs.Add(t.s0);
        tmpSegs.Add(t.s1);
        tmpSegs.Add(t.s2);

        AddTriangle(t.a, t.b, v);
        AddTriangle(t.b, t.c, v);
        AddTriangle(t.c, t.a, v);
        RemoveTriangle(t);
      });

      while (tmpSegs.Count != 0) {
        Segment2D seg = tmpSegs.Last();
        tmpSegs.RemoveAt(tmpSegs.Count - 1);

        List<Triangle2D> commonTri = tris.FindAll(t => t.HasSegment(seg));
        if (commonTri.Count <= 1) continue;

        Triangle2D abc = commonTri[0];
        Triangle2D abd = commonTri[1];

        if (abc.Equals(abd)) {
          RemoveTriangle(abc);
          RemoveTriangle(abd);
          continue;
        }

        Vertex2D a = seg.a;
        Vertex2D b = seg.b;
        Vertex2D c = abc.ExcludePoint(seg);
        Vertex2D d = abd.ExcludePoint(seg);

        Circle2D ec = Circle2D.GetCircumscribedCircle(abc);
        if (ec.Contains(d.Coordinate)) {
          RemoveTriangle(abc);
          RemoveTriangle(abd);

          AddTriangle(a, c, d); // add acd
          AddTriangle(b, c, d); // add bcd

          var segments0 = abc.ExcludeSegment(seg);
          tmpSegs.Add(segments0[0]);
          tmpSegs.Add(segments0[1]);

          var segments1 = abd.ExcludeSegment(seg);
          tmpSegs.Add(segments1[0]);
          tmpSegs.Add(segments1[1]);
        }
      }
      RebuildVertsDict();
    }

    bool FindAndSplit(float threshold) {
      for (int i = 0, n = baseSegs.Count; i < n; i++) {
        var s = baseSegs[i];
        if (s.Length() < threshold) continue;

        for (int j = 0, m = dtVerts.Count; j < m; j++) {
          if (s.EncroachedUpon(dtVerts[j].Coordinate)) {
            SplitSegment(s);
            return true;
          }
        }
      }
      return false;
    }

    bool IsOutsidePoly(Vector2 p) {
      return !Utils2D.Contains(p, baseVerts);
    }

    bool IsOutsidePoly(Triangle2D t) {
      return
        IsOutsidePoly(t.a.Coordinate) ||
        IsOutsidePoly(t.b.Coordinate) ||
        IsOutsidePoly(t.c.Coordinate)
      ;
    }

    bool IsSegMidpointOutsidePoly(Segment2D s) {
      return IsOutsidePoly(s.Midpoint());
    }

    void Refine(float angle, float threshold) {
      int c = 0;
      while (tris.Any(t => !IsOutsidePoly(t) && t.Skinny(angle, threshold))) {
        RefineSubRoutine(angle, threshold);
        if (c++ > 50) {
          // Debug.LogWarning("refine 200"); 
          return;
        }
      }
    }

    void RefineSubRoutine(float angle, float threshold) {
      int c = 0;
      while (true) {
        if (!FindAndSplit(threshold)) break;
        if (c++ > 100) {
          // Debug.LogWarning("sub 100");
          return;
        }
      }

      var skinny = tris.Find(t => !IsOutsidePoly(t) && t.Skinny(angle, threshold));
      var p = skinny.Circumcenter();

      var segments = baseSegs.FindAll(s => s.EncroachedUpon(p));
      if (segments.Count > 0) {
        segments.ForEach(s => SplitSegment(s));
      } else {
        SplitTriangle(skinny);
      }
    }

    void SplitTriangle(Triangle2D t) {
      var c = t.Circumcenter();
      UpdateTriangulation(c);
    }

    void SplitSegment(Segment2D s) {
      Vertex2D a = s.a, b = s.b;
      var mv = new Vertex2D(s.Midpoint());

      // add mv to V 
      // the index is between a and b.
      var idxA = baseVerts.IndexOf(a);
      var idxB = baseVerts.IndexOf(b);
      if (Mathf.Abs(idxA - idxB) == 1) {
        var idx = (idxA > idxB) ? idxA : idxB;
        baseVerts.Insert(idx, mv);
      } else {
        baseVerts.Add(mv);
      }

      UpdateTriangulation(mv.Coordinate);

      // Add two halves to S
      var sidx = baseSegs.IndexOf(s);
      baseSegs.RemoveAt(sidx);

      baseSegs.Add(new Segment2D(s.a, mv));
      baseSegs.Add(new Segment2D(mv, s.b));
    }

    bool CheckUnique() {
      var flag = false;

      for (int i = 0, n = dtVerts.Count; i < n; i++) {
        var v0 = dtVerts[i];
        for (int j = i + 1; j < n; j++) {
          var v1 = dtVerts[j];
          if (Utils2D.CheckEqual(v0, v1)) {
            Debug.LogWarning("vertex " + i + " equals " + j);
            flag = true;
          }
        }
      }

      for (int i = 0, n = dtSegs.Count; i < n; i++) {
        var s0 = dtSegs[i];
        for (int j = i + 1; j < n; j++) {
          var s1 = dtSegs[j];
          if (Utils2D.CheckEqual(s0, s1)) {
            Debug.LogWarning("segment " + i + " equals " + j);
            flag = true;
          }
        }
      }

      for (int i = 0, n = tris.Count; i < n; i++) {
        var t0 = tris[i];
        for (int j = i + 1; j < n; j++) {
          var t1 = tris[j];
          if (Utils2D.CheckEqual(t0, t1)) {
            Debug.LogWarning("triangle " + i + " equals " + j);
            flag = true;
          }
        }
      }

      for (int i = 0, n = tris.Count; i < n; i++) {
        var t = tris[i];
        if (Utils2D.CheckEqual(t.s0, t.s1) || Utils2D.CheckEqual(t.s0, t.s2) || Utils2D.CheckEqual(t.s1, t.s2)) {
          Debug.LogWarning("triangle " + i + " has duplicated segments");
          flag = true;
        }
      }

      return flag;
    }

    public void DrawGizmos() {
      tris.ForEach(t => t.DrawGizmos());
    }

  }

}
