using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
  public static class ListExtensions {
    public static T FirstOrNull<T>(this IEnumerable<T> l) => l.HasLength() ? l.First() : default(T);

    public static T RandomObj<T>(this List<T> l) {
      if (!l.HasLength()) return default(T);
      int idx = BWRandom.Unseeded<int>(() => BWRandom.Range(0, l.Count));
      return l[idx];
    }

    public static T RandomObj<T>(this T[] l) {
      if (l == null || l.Length == 0) return default(T);
      int idx = BWRandom.Unseeded<int>(() => BWRandom.Range(0, l.Length));
      return l[idx];
    }

    public static void Each<T>(this IEnumerable<T> source, Action<T> action) {
      if (source == null) throw new ArgumentNullException("source");
      if (action == null) throw new ArgumentNullException("action");

      foreach (T item in source)
        action(item);
    }

    public static List<T> Map<T>(this IEnumerable<T> source, Func<T, T> func) {
      if (source == null) throw new ArgumentNullException("source");
      if (func == null) throw new ArgumentNullException("func");

      List<T> l = new List<T>();
      foreach (T item in source)
        l.Add(func(item));
      return l;
    }

    public static IEnumerable<T> Filter<T>(this IEnumerable<T> source, Func<T, bool> predicate) =>
      source.Where(predicate);

    public static T MinBy<T>(this List<T> source, Func<T, float> predicate) {
      float min = float.MaxValue;
      int mindex = 0;
      for (int i = 0; i < source.Count(); i++) {
        float val = predicate(source[i]);
        if (val < min) {
          min = val;
          mindex = i;
        }
      }
      return source[mindex];
    }

    public static T[] Fill<T>(this T[] source, T value) {
      for (int i = 0; i < source.Length; i++) source[i] = value;
      return source;
    }

    public static List<T> FilledList<T>(int count, T value) =>
      (new T[count]).Fill(value).ToList();

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

    public static List<T> Swap<T>(this List<T> l, int i1, int i2) {
      if (i1 >= l.Count || i2 >= l.Count) {
        Debug.LogWarning("Cannot swap, bad indexes: " + l.Count + " | " + i1 + "_" + i2);
        return l;
      }
      List<T> l2 = l.ToList();
      l2[i1] = l[i2];
      l2[i2] = l[i1];
      return l2;
    }

    public static List<T> GetRange<T>(this List<T> list, int start) {
      return list.GetRange(start, list.Count - start);
    }

    public static bool HasLength<T>(this IEnumerable<T> arr) => arr != null && arr.Count() > 0;

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

    public static List<T> Adding<T>(this List<T> l, params T[] a) {
      l.AddRange(a);
      return l;
    }

    public static List<T> Removing<T>(this List<T> l, T a) {
      l.Remove(a);
      return l;
    }

    public static List<T> RemovingAt<T>(this List<T> l, int idx) {
      l.RemoveAt(idx);
      return l;
    }

    public static List<T> RemovingNull<T>(this List<T> l) where T : class =>
      l.Where(c => c != null).ToList();

    public static T[] RemovingNull<T>(this T[] l) where T : class =>
      l.Where(c => c != null).ToArray();

    public static List<T> Dedupe<T>(this List<T> list) {
      List<T> l2 = new List<T>();
      T last = list.First();
      foreach (T v in list) {
        if (!v.Equals(last)) l2.Add(v);
        last = v;
      }
      return l2;
    }

    //remove from the end
    public static T Pop<T>(this List<T> l) {
      T last = l.Last();
      l.Remove(last);
      return last;
    }

    //remove from the start
    public static T Unshift<T>(this List<T> l) {
      T first = l.First();
      l.Remove(first);
      return first;
    }

    public static T[] Copy<T>(this T[] arr) {
      T[] a2 = new T[arr.Length];
      Array.Copy(arr, a2, arr.Length);
      return a2;
    }

    public static T FromTheBack<T>(this List<T> l, int fromTheBack) =>
      (l.Count - fromTheBack - 1 >= 0) ? l[l.Count - fromTheBack - 1] : l.Last();

    public static T NextAfter<T>(this IEnumerable<T> l, int idx) => idx >= l.Count() - 1 ? l.ElementAt(0) : l.ElementAt(idx + 1);
    public static T PreviousFrom<T>(this IEnumerable<T> l, int idx) => idx >= 1 ? l.ElementAt(idx - 1) : l.ElementAt(l.Count() - 1);

    public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source) {
      if (source == null) return Enumerable.Empty<(T, int)>();
      return source.Select((item, index) => (item, index));
    }

    public static IEnumerable<T> Sorted<T>(this List<T> source, Comparison<T> comparison) {
      List<T> newList = source.ToList();
      newList.Sort(comparison);
      return newList;
    }

    public static Vector2[] ToVector2(this Vector3[] arr) => Array.ConvertAll<Vector3, Vector2>(arr, v => v);
    public static Vector3[] ToVector3(this Vector2[] arr) => Array.ConvertAll<Vector2, Vector3>(arr, v => v);
    public static List<Vector2> ToVector2(this List<Vector3> list) => list.ConvertAll<Vector2>(v => v);
    public static List<Vector3> ToVector3(this List<Vector2> list) => list.ConvertAll<Vector3>(v => v);

    public static string AsCommaCommaAndString(this List<string> l) => AsCommaCommaAndString(l.ToArray());
    public static string AsCommaCommaAndString(this string[] arr) {
      if (arr.Length == 0) return "";
      if (arr.Length == 1) return arr[0];
      string s = "";
      for (int i = 0; i < arr.Length; i++) {
        if (arr[i] == null || arr[i].Length == 0) continue;
        if (i == arr.Length - 1) s += " and ";
        else if (i > 0) s += ", ";
        s += arr[i];
      }
      return s;
    }
  }

}
