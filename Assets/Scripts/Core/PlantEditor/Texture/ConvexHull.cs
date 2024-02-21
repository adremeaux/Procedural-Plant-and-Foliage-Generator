using System.Collections.Generic;
using UnityEngine;

namespace BionicWombat {
  public static class ConvexHull {
    public static List<Vector2> ComputeConvexHull(List<Vector2> points, bool sortInPlace = false) {
      if (!sortInPlace)
        points = new List<Vector2>(points);

      points.Sort((a, b) =>
        a.x == b.x ? a.y.CompareTo(b.y) : (a.x > b.x ? 1 : -1));

      // Importantly, DList provides O(1) insertion at beginning and end
      CircularList<Vector2> hull = new CircularList<Vector2>();
      int L = 0, U = 0; // size of lower and upper hulls

      // Builds a hull such that the output polygon starts at the leftmost Vector2.
      for (int i = points.Count - 1; i >= 0; i--) {
        Vector2 p = points[i], p1;

        // build lower hull (at end of output list)
        while (L >= 2 && (p1 = hull.Last).Sub(hull[hull.Count - 2]).Cross(p.Sub(p1)) >= 0) {
          hull.PopLast();
          L--;
        }
        hull.PushLast(p);
        L++;

        // build upper hull (at beginning of output list)
        while (U >= 2 && (p1 = hull.First).Sub(hull[1]).Cross(p.Sub(p1)) <= 0) {
          hull.PopFirst();
          U--;
        }
        if (U != 0) // when U=0, share the Vector2 added above
          hull.PushFirst(p);
        U++;
        Debug.Assert(U + L == hull.Count + 1);
      }
      hull.PopLast();
      return (List<Vector2>)hull;
    }

    private static Vector2 Sub(this Vector2 a, Vector2 b) {
      return a - b;
    }

    private static float Cross(this Vector2 a, Vector2 b) {
      return a.x * b.y - a.y * b.x;
    }

    private class CircularList<T> : List<T> {
      public T Last {
        get {
          return this[this.Count - 1];
        }
        set {
          this[this.Count - 1] = value;
        }
      }

      public T First {
        get {
          return this[0];
        }
        set {
          this[0] = value;
        }
      }

      public void PushLast(T obj) {
        this.Add(obj);
      }

      public T PopLast() {
        T retVal = this[this.Count - 1];
        this.RemoveAt(this.Count - 1);
        return retVal;
      }

      public void PushFirst(T obj) {
        this.Insert(0, obj);
      }

      public T PopFirst() {
        T retVal = this[0];
        this.RemoveAt(0);
        return retVal;
      }
    }
  }
}
