using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
  public static class StructExtensions {
    public static bool IsDefault<T>(this T t) where T : struct => t.Equals(default(T));
    public static bool IsNotDefault<T>(this T t) where T : struct => !t.Equals(default(T));
    public static bool IsValid<T>(this T t) where T : struct => !t.Equals(default(T));
  }
}
