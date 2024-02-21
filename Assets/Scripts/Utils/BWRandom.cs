using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
  public enum BWRandomCurve {
    Default,
    Quad,
  }

  public enum BWRandomHalf {
    All,
    PosOnly,
    NegOnly
  }

  public static class BWRandom {
    public static bool enabled = true;

    private static UnityEngine.Random.State unseededState;

    public static void SetSeed(int seed) {
      UnityEngine.Random.InitState(seed);
      enabled = seed != 0;
    }

    public static int Range(int min, int max) {
      if (!enabled) return (min + max) / 2;
      if (min > max) return Range(max, min);
      return UnityEngine.Random.Range(min, max);
    }

    public static float Range(float min, float max) {
      if (!enabled) return (min + max) / 2f;
      if (min > max) return Range(max, min);
      return UnityEngine.Random.Range(min, max);
    }

    public static float RangeSD(float mean, float sd, float restrictToNumSDs = 0) {
      if (!enabled) return mean;
      float u = UnityEngine.Random.Range(0f, 1f);
      float v = UnityEngine.Random.Range(0f, 1f);
      float f = Mathf.Sqrt(-2.0f * Mathf.Log(u)) * Mathf.Cos(2.0f * Mathf.PI * v);
      if (restrictToNumSDs > 0)
        if (f > restrictToNumSDs || f < -restrictToNumSDs)
          return RangeSD(mean, sd, restrictToNumSDs);
      return f * sd + mean;
    }

    public static float RangeAdd(float lower, float upper, BWRandomHalf half = BWRandomHalf.All) {
      if (!enabled) return 0f;
      return UnityEngine.Random.Range(half == BWRandomHalf.PosOnly ? 0f : lower,
                          half == BWRandomHalf.NegOnly ? 0f : upper);
    }

    public static float RangeAdd(float range, BWRandomHalf half = BWRandomHalf.All) => RangeAdd(-range, range, half);

    public static float RangeMult(float range, BWRandomHalf half = BWRandomHalf.All) {
      if (!enabled) return 1f;
      return UnityEngine.Random.Range(half == BWRandomHalf.PosOnly ? 1f : -range + 1f,
                          half == BWRandomHalf.NegOnly ? 1f : range + 1f);
    }

    public static bool Bool() => UnityEngine.Random.Range(0, 2) == 1;

    public static float[] ManyFloats(int count, float min = 0f, float max = 1f) {
      float[] f = new float[count];
      for (int i = 0; i < count; i++) f[i] = Range(min, max);
      return f;
    }

    public static T Unseeded<T>(Func<T> func) {
      UnityEngine.Random.State seededState = UnityEngine.Random.state;
      if (unseededState.IsDefault()) {
        UnityEngine.Random.InitState(Environment.TickCount);
        unseededState = UnityEngine.Random.state;
      }
      UnityEngine.Random.state = unseededState;
      T ret = func();
      unseededState = UnityEngine.Random.state;
      UnityEngine.Random.state = seededState;
      return ret;
    }

    public static bool UnseededBool() => Unseeded<bool>(() => Bool());
    public static int UnseededInt(int start, int endExclusive) => Unseeded<int>(() => Range(start, endExclusive));
    public static float UnseededRange(float min, float max) => Unseeded<float>(() => Range(min, max));

    /** unseeded */
    public static T RandomEnum<T>() where T : System.Enum {
      Array typesEnumList = Enum.GetValues(typeof(T));
      return (T)typesEnumList.GetValue(BWRandom.Unseeded(() => BWRandom.Range(0, typesEnumList.Length)));
    }

    public static string RandomString(int lenMin, int lenMaxExcl) {
      const string glyphs = "abcdefghijklmnopqrstuvwxyz0123456789";
      int len = BWRandom.UnseededInt(lenMin, lenMaxExcl);
      string s = "";
      bool lastSpace = false;
      for (int i = 0; i < len; i++) {
        if (!lastSpace && BWRandom.UnseededInt(0, 7) == 0) {
          s += " ";
          lastSpace = true;
        } else {
          lastSpace = false;
          s += glyphs[BWRandom.UnseededInt(0, glyphs.Length)];
        }
      }
      return s;
    }

    public static void ShuffleArray<T>(T[] arr) {
      if (unseededState.IsDefault()) unseededState = UnityEngine.Random.state;
      UnityEngine.Random.State seededState = UnityEngine.Random.state;
      UnityEngine.Random.state = unseededState;

      T tmp;
      for (int i = 0; i < arr.Length - 1; i++) {
        int rnd = UnityEngine.Random.Range(i, arr.Length);
        tmp = arr[rnd];
        arr[rnd] = arr[i];
        arr[i] = tmp;
      }

      unseededState = UnityEngine.Random.state;
      UnityEngine.Random.state = seededState;
    }

    public static void ShuffleArray<T>(List<T> arr) {
      if (unseededState.IsDefault()) unseededState = UnityEngine.Random.state;
      UnityEngine.Random.State seededState = UnityEngine.Random.state;
      UnityEngine.Random.state = unseededState;

      T tmp;
      for (int i = 0; i < arr.Count - 1; i++) {
        int rnd = UnityEngine.Random.Range(i, arr.Count);
        tmp = arr[rnd];
        arr[rnd] = arr[i];
        arr[i] = tmp;
      }

      unseededState = UnityEngine.Random.state;
      UnityEngine.Random.state = seededState;
    }

    public static List<T> ShuffledArray<T>(List<T> arr) {
      List<T> l = arr.ToList();
      ShuffleArray(l);
      return l;
    }

    public static void RunTest() {
      MiscCommands.ClearConsole();
      Debug.Log("Set Seed: 427");
      UnityEngine.Random.InitState(427);
      Debug.Log("Seeded 0-100: " + BWRandom.Range(0, 100));
      Debug.Log("Seeded 0-100: " + BWRandom.Range(0, 100));
      Debug.Log("Seeded 0-100: " + BWRandom.Range(0, 100));
      Debug.Log("Seeded 0-100: " + BWRandom.Range(0, 100));
      Debug.Log("Seeded 0-100: " + BWRandom.Range(0, 100));

      Debug.Log("Unseeded 0-100: " + BWRandom.UnseededInt(0, 100));
      Debug.Log("Unseeded 0-100: " + BWRandom.UnseededInt(0, 100));
      Debug.Log("Unseeded 0-100: " + BWRandom.UnseededInt(0, 100));
      Debug.Log("Unseeded 0-100: " + BWRandom.UnseededInt(0, 100));
      Debug.Log("Unseeded 0-100: " + BWRandom.UnseededInt(0, 100));

      Debug.Log("Seeded 0-100: " + BWRandom.Range(0, 100));
      Debug.Log("Seeded 0-100: " + BWRandom.Range(0, 100));
      Debug.Log("Seeded 0-100: " + BWRandom.Range(0, 100));
      Debug.Log("Seeded 0-100: " + BWRandom.Range(0, 100));
      Debug.Log("Seeded 0-100: " + BWRandom.Range(0, 100));

      UnityEngine.Random.InitState(Time.frameCount);
      List<float> l = new List<float>();
      Dictionary<float, int> d = new Dictionary<float, int>();
      float min = 1000f;
      float max = -10000f;
      for (int i = 0; i < 100000; i++) {
        float f = BWRandom.RangeSD(1f, 2f, 1f);
        if (f > max) max = f;
        if (f < min) min = f;
        float trunc = f.Truncate(1);
        if (!d.ContainsKey(trunc)) d[trunc] = 0;
        d[trunc]++;
        l.Add(f);
      }
      Debug.Log("RangeSD: " + l.ToLog());
      DebugBW.Log("max: " + max + " | min: " + min);
      foreach (float f in d.Keys.ToList().Sorted((f1, f2) => f1 == f2 ? 0 : f1 > f2 ? 1 : -1))
        Debug.Log(f + ": " + d[f]);
    }
  }
}
