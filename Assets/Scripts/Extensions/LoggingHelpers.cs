using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
public delegate string Action<T>(T item);

public class BatchLogger {
  private struct LogEntry {
    public List<string> list;
    public int per;
    public int skip;
    public int lastSkipped;

    public LogEntry(int per, int skip) {
      list = new List<string>();
      this.per = per;
      this.skip = skip;
      lastSkipped = 0;
    }
  }
  private static Dictionary<string, LogEntry> dict =
      new Dictionary<string, LogEntry>();

  public static void Log(float f, string id, int per = 10, int skip = 0) =>
    Log("" + f, id, per, skip);

  public static void Log(string s, string id, int per = 10, int skip = 0) {
    LogEntry entry = dict.ContainsKey(id) ? dict[id] : new LogEntry(per, skip);
    if (entry.per == 0) entry = new LogEntry(per, skip);
    if (entry.lastSkipped++ >= skip) {
      entry.list.Add(s);
      entry.lastSkipped = 0;
    }
    if (entry.list.Count >= entry.per) {
      LogID(id);
    } else {
      dict[id] = entry;
    }
  }

  private static void LogID(string id) {
    LogEntry entry = dict[id];
    Debug.Log(FormString(entry, id));
    dict.Remove(id);
  }

  private static string FormString(LogEntry entry, string id) {
    return (id + ": ").PadRight(25) + entry.list.ToLog("\t  ");
  }

  public static void AppendPrev(string s, string id) {
    if (!dict.ContainsKey(id)) {
      // Debug.LogError("BatchLogger contains no previous key " + id);
      return;
    }
    LogEntry entry = dict[id];
    entry.list[entry.list.Count - 1] += " | " + s;
  }
  public static void AppendPrev(float f, string id) => AppendPrev("" + f, id);

  public static void Flush() {
    List<string> keyList = new List<string>(dict.Keys);
    foreach (string id in keyList) {
      LogID(id);
    }
    dict.Clear();
  }

  public static void Empty() {
    dict.Clear();
  }
}

public static class LoggingHelpers {
  public static string ToLogLong(this Hashtable h) {
    string r = "";
    foreach (string s in h.Keys) {
      r += s + ": " + h[s] + "," + Environment.NewLine;
    }
    return r;
  }

  public static string ToLog<T>(this List<T> l, string delim = ", ") {
    return String.Join(delim, l);
  }

  public static string ToLog<T>(this T[] a) {
    return a.ToList().ToLog();
  }

  public static string ToLog<T, U>(this Dictionary<T, U> d) {
    return d.ToArray().ToLog();
  }

  public static string ToLogLong<T>(this List<T> l, Action<T> action = null) {
    string s = "List: [\n";
    foreach (T t in l) {
      string S;
      if (action is Action<T>) S = action(t);
      else S = "" + t;
      s += "\t" + S + ",\n";
    }
    s += "]";
    return s;
  }

  public static Dictionary<K, V> ToDict<K, V>(this Hashtable table) {
    return table
      .Cast<DictionaryEntry>()
      .ToDictionary(kvp => (K)kvp.Key, kvp => (V)kvp.Value);
  }
}

public class LogOverride : ILogHandler {
  private ILogHandler defaultLogger = Debug.unityLogger.logHandler;

  public LogOverride() {
    Debug.unityLogger.logHandler = this;
  }

  public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args) {
    defaultLogger.LogFormat(logType, context, "[" + Time.frameCount + "]\t" + format, args);
  }

  public void LogException(Exception exception, UnityEngine.Object context) {
    defaultLogger.LogException(exception, context);
  }
}
}