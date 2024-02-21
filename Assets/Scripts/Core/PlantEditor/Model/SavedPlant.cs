using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace BionicWombat {
  public class SavedPlant {
    public static int VersionNumber = 6;
    public string name;
    public string uniqueID;
    public int seed;
    public int version;
    public List<LeafParam> leafParamsList;

    public SavedPlant() { }

    public SavedPlant(string name, LeafParamDict leafParams, int seed) :
      this(name, GuidHelpers.Generate(), leafParams, seed, VersionNumber) { }

    public SavedPlant(string name, string uniqueID, LeafParamDict leafParams,
        int seed, int version) {
      this.name = name;
      this.uniqueID = uniqueID;
      this.seed = seed;
      this.version = version;
      leafParamsList = new List<LeafParam>();
      foreach (LPK key in leafParams.Keys) {
        LeafParam p = leafParams[key];
        leafParamsList.Add(p);
      }
    }

    public override string ToString() {
      return "[SavedPlant]" + " | name: " + name + " | seed: " + seed + " | uniqueID: " + uniqueID;
    }

    public LeafParamDict GetLeafParamDict() {
      LeafParamDict dict = new LeafParamDict();
      foreach (LeafParam p in leafParamsList)
        dict[p.key] = p;
      return dict;
    }
  }
}
