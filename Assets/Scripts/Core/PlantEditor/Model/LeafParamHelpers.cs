using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace BionicWombat {
  public static class LeafParamHelpers {
    private static Dictionary<LPImportance, LPK[]> groupedByImportance;
    public static LPK[] ParamsWithImportance(LPImportance importance) {
      if (groupedByImportance == null) {
        LeafParamDict defaults = LeafParamDefaults.Defaults;
        Dictionary<LPImportance, List<LPK>> g = new Dictionary<LPImportance, List<LPK>>();
        foreach (LPImportance key in Enum.GetValues(typeof(LPImportance))) {
          g[key] = new List<LPK>();
        }
        foreach (LPK lpk in Enum.GetValues(typeof(LPK))) {
          LPImportance imp = defaults[lpk].importance;
          g[imp].Add(lpk);
        }
        groupedByImportance = new Dictionary<LPImportance, LPK[]>();
        foreach (LPImportance key in Enum.GetValues(typeof(LPImportance))) {
          groupedByImportance[key] = g[key].ToArray();
        }
      }
      return groupedByImportance[importance];

    }
  }
}
