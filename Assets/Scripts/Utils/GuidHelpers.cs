using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BionicWombat {
  public static class GuidHelpers {
    public static string Generate() => DateTime.Now.Ticks + "-" + Guid.NewGuid().ToString();

    public static string GetAssetGUID(string searchString) {
      string[] ass = AssetDatabase.FindAssets(searchString);
      if (ass.Length > 1) Debug.LogWarning("Multiple assets found for search string " + searchString + ": " + ass.ToLog());
      if (ass.Length > 0) {
        return AssetDatabase.GUIDToAssetPath(ass[0]);
      }
      return null;
    }

    public static string[] GetAssetGUIDs(string searchString) {
      string[] ass = AssetDatabase.FindAssets(searchString);
      if (ass.Length > 0) {
        return ass.ToList().ConvertAll(str => AssetDatabase.GUIDToAssetPath(str)).ToArray();
      }
      return null;
    }
  }

}
