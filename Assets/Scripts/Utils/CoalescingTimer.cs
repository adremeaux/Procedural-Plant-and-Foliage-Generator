using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
  public class CoalescingTimer {
    private class TimerData {
      public string timerName;
      // public DateTime start;
      // public DateTime last;
      // public int count;
      // public bool enable;
      // public bool force;
      public Dictionary<string, SplitData> splits;
    }

    private class SplitData {
      public string splitName;
      public float time { get; private set; }
      public float avgTime => time / (float)count;
      public int count;
      public void AddSplit(float time) {
        this.time += time;
        count++;
      }
      public override string ToString() {
        return $"[{splitName}] Avg: {time / (float)count} | Total: {time} | Count: {count}";
      }
    }

    public static bool globalEnable = false;
    private static Dictionary<string, TimerData> timers = new Dictionary<string, TimerData>();

    public static void Start(string name) {
      if (globalEnable) {
        if (timers.ContainsKey(name)) {
          Debug.LogWarning("Coalescing Timer [" + name + "] already started");
        } else Debug.Log("Starting Coalescing Timer [" + name + "]");
      }

      TimerData td = new TimerData();
      td.timerName = name;
      td.splits = new Dictionary<string, SplitData>();
      timers[name] = td;
    }

    public static void Coalesce(SplitTimer st, string splitName) {
      if (st == null) return;
      if (!timers.ContainsKey(st.name)) {
        // Debug.LogWarning("Coalescing Timer [" + st.name + "] must be started before adding data");
        // Debug.Log("timers: " + timers.ToLog());
        return;
      }

      // if (GlobalVars.instance != null &&
      //   !GlobalVars.instance.ShowCoalescingSplits) {
      //   Debug.LogWarning("Coalescing Timer [" + st.name + "] missing instance or disabled");
      //   return;
      // }

      string timerName = st.name;
      float split = st.Split(splitName, false);

      TimerData data = timers[timerName];
      if (!data.splits.ContainsKey(splitName)) {
        SplitData sd = new SplitData();
        sd.splitName = splitName;
        data.splits[splitName] = sd;
      }
      data.splits[splitName].AddSplit(split);
    }

    public static void Flush(string timerName) {
      // if (!GlobalVars.instance.ShowCoalescingSplits) return;
      if (!timers.ContainsKey(timerName)) {
        Debug.LogWarning("Cannot flush " + timerName + ", no splits found");
        return;
      }
      TimerData td = timers[timerName];
      // DebugBW.Log("td: " + td);
      if (globalEnable) {
        Debug.Log("[Coalescing Timer '" + timerName + "']");
        float total = 0;
        int count = 0;
        var sortedDict = td.splits.OrderBy(pair => -pair.Value.avgTime)
                           .Select(pair => (key: pair.Key, value: pair.Value))
                           .ToArray();
        // DebugBW.Log("sortedDict: " + td.splits.ToLog());
        foreach ((string splitName, SplitData sd) in sortedDict) {
          // SplitData spl = td.splits[splitName];
          total += sd.time;
          count++;
          Debug.Log("\t" + sd);
        }
        Debug.Log("[Coalescing Timer '" + timerName + "'] Total: " + $"<color={LColor.orange}>{total}</color>" + " | Avg: " + (total / (float)count) + " | Count: " + count);
      }
      timers.Remove(timerName);
    }
  }

}
