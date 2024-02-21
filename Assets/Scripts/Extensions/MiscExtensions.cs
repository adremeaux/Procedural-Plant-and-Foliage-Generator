using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
  public static class MiscExtensions {
    public static DayOfWeek AddDay(this DayOfWeek day, int add) {
      int d = (int)day;
      d += add;
      if (d >= 7) d -= 7;
      if (d < 0) d += 7;
      return (DayOfWeek)d;
    }

    public static int DaysFrom(this DayOfWeek futureDay, DayOfWeek pastDay) {
      return ((int)futureDay - (int)pastDay).WrapAround(7);
    }

    public static bool IsDestroyed(this object go) => go == null && !ReferenceEquals(go, null);

    public static bool Is(this object obj, Type type) => obj.GetType() == type;

    public static string PrettyPrint(this object obj) => (obj == null) ? "<null>" :
      $"[{obj.GetType()}] \n";

    public static void AlphaAndActive(this CanvasGroup c, bool visible) {
      c.gameObject.SetActive(visible);
      c.alpha = visible ? 1f : 0f;
    }

    public static void Fade(this CanvasGroup c, bool fadeIn, float delay = 0f, float duration = 0.3f) {
      c.gameObject.SetActive(true);
    }
  }

  public static class MiscCommands {
    public static void ClearConsole() {
      var assembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.SceneView));
      var type = assembly.GetType("UnityEditor.LogEntries");
      var method = type.GetMethod("Clear");
      method.Invoke(new object(), null);
    }
  }
}
