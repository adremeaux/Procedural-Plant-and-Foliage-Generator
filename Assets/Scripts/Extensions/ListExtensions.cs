using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
public static class ListExtensions {
  public static T RandomObj<T>(this List<T> l) {
    int idx = UnityEngine.Random.Range(0, l.Count);
    return l[idx];
  }

  public static void Each<T>(this IEnumerable<T> source, Action<T> action) {
    if (source == null) throw new ArgumentNullException("source");
    if (action == null) throw new ArgumentNullException("action");

    foreach (T item in source)
      action(item);
  }

  public static IEnumerable<(T, T)> Pairwise<T>(this IEnumerable<T> source, bool includeEndStartPair = false) {
    if (source.Count() < 2) yield break;
    var previous = default(T);
    using (var it = source.GetEnumerator()) {
      if (it.MoveNext())
        previous = it.Current;
      var first = previous;

      while (it.MoveNext())
        yield return (previous, previous = it.Current);

      if (includeEndStartPair)
        yield return (previous, first);
    }
  }

  public static List<T> Reversed<T>(this List<T> l) {
    var r = l.ToList();
    r.Reverse();
    return r;
  }

  public static T[] Reversed<T>(this T[] l) {
    return l.ToList().Reversed().ToArray();
  }

  public static void Add<T>(this List<T> l, params T[] a) {
    l.AddRange(a);
  }

  public static T[] Copy<T>(this T[] arr) {
    T[] a2 = new T[arr.Length];
    Array.Copy(arr, a2, arr.Length);
    return a2;
  }

  public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source) {
    return source.Select((item, index) => (item, index));
  }

  public static Vector2[] ToVector2(this Vector3[] arr) => Array.ConvertAll<Vector3, Vector2>(arr, v => v);
  public static Vector3[] ToVector3(this Vector2[] arr) => Array.ConvertAll<Vector2, Vector3>(arr, v => v);
  public static List<Vector2> ToVector2(this List<Vector3> list) => list.ConvertAll<Vector2>(v => v);
  public static List<Vector3> ToVector3(this List<Vector2> list) => list.ConvertAll<Vector3>(v => v);
}

}