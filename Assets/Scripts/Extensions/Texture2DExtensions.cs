using System;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
public static class Texture2DExtensions {
  public static void SetColor(this Texture2D tex2, Color32 color) {
    var fillColorArray = tex2.GetPixels32();
    for (var i = 0; i < fillColorArray.Length; ++i)
      fillColorArray[i] = color;
    tex2.SetPixels32(fillColorArray);
    tex2.Apply();
  }
}
}