using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
  public static class EnumExt {
    public static T[] Values<T>() where T : System.Enum => (T[])Enum.GetValues(typeof(T));

    public static bool HasAnyBitmask<T>(this T enumm, T mask) where T : System.Enum {
      ulong keysVal = Convert.ToUInt64(enumm);
      ulong flagVal = Convert.ToUInt64(mask);

      return (keysVal & flagVal) > 0;
    }

    public static bool HasAllBitmask<T>(this T enumm, T mask) where T : System.Enum {
      ulong keysVal = Convert.ToUInt64(enumm);
      ulong flagVal = Convert.ToUInt64(mask);

      return (keysVal & flagVal) == flagVal;
    }
  }
}
