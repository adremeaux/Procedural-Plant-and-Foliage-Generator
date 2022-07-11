using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace BionicWombat {
public class LeafParamPreset {
  public string name;
  public List<LeafParam> leafParamsList;
  public LeafParam test;

  [XmlIgnore]
  public LeafParamDict leafParams {
    get {
      LeafParamDict d = new LeafParamDict();
      foreach (LeafParam lp in leafParamsList)
        d[lp.key] = lp;
      return d;
    }
  }

  public LeafParamPreset() { }

  public LeafParamPreset(string name, LeafParamDict leafParams) {
    this.name = name;
    leafParamsList = new List<LeafParam>();
    foreach (LPK key in leafParams.Keys) {
      LeafParam p = leafParams[key];
      leafParamsList.Add(p);
    }
  }

  //takes in deserialized data and builds it into a full dictionary
  //by repopulating non-serialized values from the defaults.
  //Looks for inconsistencies and repopulates and rewrites if any are found
  public static (LeafParamDict, bool) Rebuild(LeafParamDict dict) {
    LeafParamDict defaults = LeafParamDefaults.Defaults();
    bool dirty = false;
    bool log = true;
    foreach (LPK key in defaults.Keys) {
      if (!dict.ContainsKey(key)) {
        dict[key] = defaults[key];
        dirty = true;
        if (log) Debug.Log("Rebuild: missing key " + key);
      }
      LeafParam def = defaults[key];
      dict[key].group = def.group;
      dict[key].type = def.type;
      dict[key].mode = def.mode;
      dict[key].range = def.range;
      // if (log) Debug.Log(dict[key]);

      LPMode mode = dict[key].mode;
      if (mode == LPMode.Unknown) {
        Debug.LogWarning("Defaults load error Mode unknown");
        dict[key] = defaults[key];

      } else if (mode == LPMode.Float) {
        if (dict[key].range == null) {
          dict[key].range = def.range;
          dirty = true;
          if (log) Debug.Log("Rebuild: missing float range " + key);
        }
        if (dict[key].value < dict[key].range.Start ||
            dict[key].value > dict[key].range.End) {
          if (log) Debug.Log("Rebuild: float range " + key + " | value: " + dict[key].value + " | range: " + dict[key].range);
          dict[key].value = dict[key].range.Default;
          dirty = true;
        }

      } else if (mode == LPMode.Toggle) {
        //no op

      } else if (mode == LPMode.ColorHSL) {
        if (dict[key].hslRange == null ||
           !dict[key].hslRange.SoftEquals(def.hslRange)) {
          dict[key].hslRange = defaults[key].hslRange;
          dirty = true;
          if (log) Debug.Log("Rebuild: HSL range " + key);
        }
        HSL hsv = dict[key].hslValue;
        HSLRange dr = def.hslRange;
        if (hsv.hue < dr.hueRange.Start || hsv.hue > dr.hueRange.End ||
            hsv.saturation < dr.satRange.Start || hsv.saturation > dr.satRange.End ||
            hsv.lightness < dr.valRange.Start || hsv.lightness > dr.valRange.End) {
          dict[key].hslValue = dr.defValue;
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

}
}