using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using static BionicWombat.LeafParamMigrator;

namespace BionicWombat {
  public static class PresetHelpers {
    //takes in deserialized data and builds it into a full dictionary
    //by repopulating non-serialized values from the defaults.
    //Looks for inconsistencies and repopulates and rewrites if any are found
    public static (LeafParamDict dict, bool dirty) Rebuild(List<LeafParam> leafParamsList, SavedPlant savedPlant) {
      bool log = true;
      bool dirty = false;
      Dictionary<LPK, LeafParamMigration> versionMigrations = new Dictionary<LPK, LeafParamMigration>();
      if (savedPlant.version != SavedPlant.VersionNumber) {
        dirty |= true;
        versionMigrations = GetMigrations(savedPlant.version);
      }

      (LeafParamDict dict, bool dictConversionDirty) = BuildLeafParamsDict(leafParamsList, versionMigrations);
      LeafParamDict defaults = LeafParamDefaults.Defaults;
      dirty |= dictConversionDirty;

      if (log && dictConversionDirty) Debug.Log("Rebuild: Dict Conversion found invalid key.");

      foreach (LPK key in defaults.Keys) {
        if (!dict.ContainsKey(key)) {
          dict[key] = defaults[key];
          dirty = true;
          if (log) Debug.Log("Rebuild: missing key " + key);
        }

        LeafParam def = defaults[key];
        dict[key].inspectorSubgroup = def.inspectorSubgroup;
        dict[key].type = def.type;
        dict[key].mode = def.mode;
        dict[key].range = def.range;
        dict[key].hslRange = def.hslRange;
        dict[key].randomValCurve = def.randomValCurve;

        LPMode mode = dict[key].mode;
        if (mode == LPMode.Unknown) {
          Debug.LogWarning("Defaults load error Mode unknown");
          dict[key] = defaults[key];

        } else if (mode == LPMode.Float) {
          if (dict[key].value < dict[key].range.Start ||
              dict[key].value > dict[key].range.End) {
            Debug.Log("Rebuild: float range " + key + " | value: " + dict[key].value + " | range: " + dict[key].range);
            if (versionMigrations.ContainsKey(key)) Debug.LogWarning($"{key} unclamped after conversion");
            dict[key].value = Mathf.Clamp(dict[key].value, dict[key].range.Start, dict[key].range.End);
            dirty = true;
          }

        } else if (mode == LPMode.ToggleDEPRECATED) {
          //!!!

        } else if (mode == LPMode.ColorHSL) {
          HSL hsv = dict[key].hslValue;
          HSLRange dr = def.hslRange;
          if (hsv.hue < dr.hueRange.Start || hsv.hue > dr.hueRange.End ||
              hsv.saturation < dr.satRange.Start || hsv.saturation > dr.satRange.End ||
              hsv.lightness < dr.valRange.Start || hsv.lightness > dr.valRange.End) {
            dict[key].hslValue = dr.defaultValues;
            dirty = true;
            Debug.Log("Setting new default HSL " + dict[key].hslValue);
          }
        }
      }

      //look for stale keys
      List<LPK> toRemove = new List<LPK>();
      foreach (LPK key in dict.Keys)
        if (!defaults.ContainsKey(key))
          toRemove.Add(key);
      foreach (LPK key in toRemove) dict.Remove(key);
      dirty |= toRemove.Count > 0;
      if (log && toRemove.Count > 0) Debug.Log("Rebuild: stale keys");

      return (dict, dirty);
    }

    private static (LeafParamDict dict, bool dirty) BuildLeafParamsDict(List<LeafParam> leafParamsList,
        Dictionary<LPK, LeafParamMigration> versionMigrations) {

      bool dirty = false;
      LeafParamDict dict = new LeafParamDict();
      foreach (LeafParam lp in leafParamsList) {
        if (lp.isInvalid) {
          dirty = true;
          continue;
        }
        dict[lp.key] = lp;
      }

      //perform migrations
      foreach (LPK key in dict.Keys) {
        if (versionMigrations.ContainsKey(key)) {
          float oldVal = dict[key].value;
          dict[key].value = versionMigrations[key].func(dict);
          if (versionMigrations[key].forceEnable) dict[key].enabled = true;
          Debug.Log($"Performing migration on {key}: {oldVal} -> {dict[key].value}" +
            $" | Version {versionMigrations[key].oldVersionNumber} -> {SavedPlant.VersionNumber}");
        }
      }

      return (dict, dirty);
    }
  }
}
