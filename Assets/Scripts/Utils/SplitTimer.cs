using System;
using System.Threading;
using UnityEngine;

namespace BionicWombat {
  [Serializable]
  public class SplitTimer {
    [SerializeField]
    public static bool globalEnable = false;

    public string name;
    DateTime start;
    DateTime last;
    int count = 0;
    public bool enable = true;
    bool force = false;
    public SplitTimer(string name, bool enable = true, bool force = false) {
      this.name = name;
      this.enable = enable;
      this.force = force;
    }

    public SplitTimer Start() {
      if ((enable && globalEnable) || force) Debug.Log("\t\t[Start: " + name + "]");
      start = DateTime.Now;
      last = start;
      return this;
    }

    public SplitTimer Reset() => Start();

    public float Split(string splitName) => Split(splitName, true);
    public float Split(bool log) => Split("", log);
    public float Split(string splitName, bool log) {
      // if ((!enable || !globalEnable) && !force) return -1f;
      if (start == null) {
        Debug.LogError($"TimerSplit {name} never called Start()");
        return -1f;
      }
      count++;
      double split = DateTime.Now.Subtract(last).TotalMilliseconds;
      double total = DateTime.Now.Subtract(start).TotalMilliseconds;
      last = DateTime.Now;
      if (splitName.Length > 0) splitName = ": " + splitName;
      if (log && enable) Debug.Log($"[Split({count})] [{name}{splitName}]\t{split}ms | total: {total}ms\t\t\t\t[Thread: {TI(Thread.CurrentThread)}]");
      return (float)split;
    }

    public float Stop() {
      if ((enable && globalEnable) || force) {
        float s = Split(false);
        Debug.Log("Timer [" + name + "] completed in " + s + "ms");
        return s;
      }
      return -1f;
    }

    private string TI(Thread t) => "name(" + t.Name + ") id(" + t.ManagedThreadId + ") isBG(" + t.IsBackground + ")";
  }

}
