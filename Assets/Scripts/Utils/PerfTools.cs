using System;
using UnityEngine;

namespace BionicWombat {
  [Serializable]
  public class PerfTools {
    DateTime runningTime;
    string name;
    bool enabled;

    public PerfTools(string name, bool enabled = true) {
      this.name = name;
      runningTime = DateTime.Now;
      this.enabled = enabled;
    }

    public void MarkStart() {
      runningTime = DateTime.Now;
    }

    public void Split(string s = "") {
      if (enabled) Debug.Log(name + (s.Length > 0 ? "(" + s + ")" : "") + ": " + DateTime.Now.Subtract(runningTime).TotalMilliseconds);
      runningTime = DateTime.Now;
    }
  }
}
