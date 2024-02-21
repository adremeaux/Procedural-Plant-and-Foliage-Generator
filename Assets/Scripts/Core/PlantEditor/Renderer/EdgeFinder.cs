using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
  public static class EdgeFinder {
    public static int[] FindEdgeVerts(int[] baseTris) {
      // Debug.Log("baseTris: " + baseTris.ToLog());
      Edge[] edges = new Edge[baseTris.Length];
      for (int i = 0; i < baseTris.Length; i += 3) {
        int a = baseTris[i];
        int b = baseTris[i + 1];
        int c = baseTris[i + 2];
        if (a == b || b == c || a == c) {
          edges[i] = edges[i + 1] = edges[i + 2] = new Edge(-1, -1);
          continue;
        }
        edges[i] = new Edge(a, b);
        edges[i + 1] = new Edge(b, c);
        edges[i + 2] = new Edge(a, c);
      }

      Edge[] nonDupes = FindNonDupes(edges);
      Edge[] ordered = Order(nonDupes);
      int[] ret = new int[ordered.Length];
      for (int i = 0; i < ordered.Length; i++)
        ret[i] = ordered[i].a;
      return ret;
    }

    public static Edge[] FindNonDupes(Edge[] edges) {
      return edges.GroupBy(i => i)
        .Where(group => !group.Skip(1).Any())
        .Select(group => group.Key)
        .ToArray();
    }

    public static Edge[] Order(Edge[] edges) {
      List<Edge> availEdges = edges.ToList();
      List<Edge> newEdges = new List<Edge>();
      newEdges.Add(edges[0]);
      int safety = edges.Length + 2;
      while (true) {
        if (newEdges.Count > 1 && newEdges.Last().Equals(newEdges.First())) {
          newEdges.RemoveAt(newEdges.Count - 1);
          break;
        }
        Edge a = newEdges.Last();
        bool got = false;
        for (int j = 0; j < availEdges.Count; j++) {
          Edge b = availEdges[j];
          if (!a.Equals(b) && (a.b == b.a || a.b == b.b)) {
            newEdges.Add(a.b == b.a ? b : b.Reversed());
            availEdges.RemoveAt(j);
            got = true;
            break;
          }
        }
        if (!got) break;
        if (--safety < 0) {
          Debug.LogError("EdgeFinder.Order caught infinite loop");
          break;
        }
      }
      return newEdges.ToArray();
    }
  }

  public struct Edge : IEquatable<Edge> {
    public int a, b;
    public Edge(int a, int b) => (this.a, this.b) = (a, b);
    public Edge Reversed() => new Edge(b, a);
    public override string ToString() => "[" + a + "-" + b + "]";
    public bool Equals(Edge e) => (a == e.a && b == e.b) || (a == e.b && b == e.a);
    public override int GetHashCode() => a.GetHashCode() ^ b.GetHashCode();
  }
}
