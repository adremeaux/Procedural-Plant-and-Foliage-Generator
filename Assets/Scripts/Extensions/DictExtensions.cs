using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace BionicWombat {
  public static class DictExtensions {
    public static Dictionary<T, U> Copy<T, U>(this Dictionary<T, U> d) => new Dictionary<T, U>(d);

    public static Dictionary<T, U> DeepCopy<T, U>(this Dictionary<T, U> dict) {
      if (typeof(U).IsValueType) {
        return new Dictionary<T, U>(dict);
      } else {
        var copy = new Dictionary<T, U>();
        foreach (var pair in dict) {
          copy[pair.Key] = pair.Value;
        }
        return copy;
      }
    }

    public static bool HasValueForKey<K, V>(this Dictionary<K, V> dict, K key) {
      if (dict == null) return false;
      if (!dict.ContainsKey(key)) return false;
      V v = dict[key];
      if (v == null) return false;
      return !v.Equals(default(V));
    }

    public static U NullableValue<T, U>(this Dictionary<T, U> dict, T key) =>
      dict != null && dict.ContainsKey(key) ? dict[key] : default(U);

    public static Dictionary<T, U> Setting<T, U>(this Dictionary<T, U> dict, T key, U val) {
      dict[key] = val;
      return dict;
    }
  }
}
