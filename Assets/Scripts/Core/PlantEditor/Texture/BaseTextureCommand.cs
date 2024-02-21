using System;
using ImageMagick;
using UnityEngine;

namespace BionicWombat {
  [Serializable]
  public class BaseTextureCommand {
    internal string currentMethod;
    internal DateTime runningTime;
    private bool logSplits;
    internal IMTextureVars vars;
    private bool _enabled = true;

    public bool enabled {
      get => _enabled;
      set => _enabled = value;
    }

    public void SetVars(IMTextureVars vars, bool logSplits) {
      this.logSplits = logSplits;
      this.vars = vars;
    }

    internal void MarkStart() {
      // if (logSplits) Debug.Log("start: " + Thread.CurrentThread.ManagedThreadId + " | bg: " + Thread.CurrentThread.IsBackground);
      currentMethod = this.GetType().Name;
      runningTime = DateTime.Now;
    }

    internal void MarkEnd() {
      if (logSplits) Debug.Log("Render Split " + currentMethod +
        ": \t\t" + DateTime.Now.Subtract(runningTime).TotalMilliseconds +
        " \t\tStart: " + runningTime.Ticks);
    }

    internal static MagickColor MC(Color c) {
      return (MagickColor)new MagickColorFactory().Create(c.ToHex());
    }

    internal static MagickColor ColorFromHex(string c) {
      return (MagickColor)new MagickColorFactory().Create(c);
    }
  }
}
