using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ImageMagick;
using UnityEngine;

namespace BionicWombat {
public static class IMHelpers {
  public static T[] Params<T>(T o, params T[] p) {
    return ((IEnumerable<T>)new T[] { o }).Concat(p).ToArray();
  }
}
}