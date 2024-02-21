using System;
using System.Collections.Generic;
using System.Linq;
using BionicWombat;
using Newtonsoft.Json;
using UnityEngine;

public enum PlantCollection {
  Classic = 1,
  Starters = 20,
  StarterBases = 21,
  Hybrids = 100,
  Temporary = 200,
  Vendor = 201,
  VendorBuys = 202,
  SeedBank = 300,
  Cool = 999,
}

namespace BionicWombat.PlantCollections {
  public static class PlantCollectionExtensions {
    public static string DisplayName(this PlantCollection pc) {
      return pc.ToString();
    }
    private static Dictionary<string, PlantCollection> reverseDict;
    public static PlantCollection PlantColEnum(this string pcString) {
      if (reverseDict == null) {
        reverseDict = new Dictionary<string, PlantCollection>();
        foreach (PlantCollection pc in Enum.GetValues(typeof(PlantCollection))) {
          reverseDict[pc.DisplayName()] = pc;
        }
      }
      return reverseDict[pcString];
    }

    public static bool IsPermanentCollection(this PlantCollection c) {
      switch (c) {
        case PlantCollection.Classic:
          return true;
        default:
          return false;
      }
    }
  }
}

[Serializable]
public struct PresetCollection {
  [SerializeField] public PlantCollection collection;
  [SerializeField] public List<PlantIndexEntry> plants;
  [JsonIgnore] public Dictionary<PlantIndexEntry, PlantCollection> seedBankDict;
  [JsonIgnoreAttribute] public string resourcesPath => collection + "/";

  public PresetCollection(PlantCollection collectionName, PlantIndexEntry[] plants,
      List<PlantCollection> seedBankCollections) {
    this.collection = collectionName;
    this.plants = plants.ToList();

    this.seedBankDict = new Dictionary<PlantIndexEntry, PlantCollection>();
    foreach ((PlantIndexEntry entry, int idx) in plants.WithIndex())
      seedBankDict[entry] = seedBankCollections[idx];
  }

  public PresetCollection DidDeserialize() {
    // DebugBW.Log(collection + " | plants: " + plants.Distinct().ToLog());
    PlantCollection col = collection;
    plants = plants.ToList()
      .Distinct()
      .Where(entry => {
        if (PlantDataManager.PlantExists(entry, col == PlantCollection.StarterBases)) return true;
        Debug.LogWarning("Removing plant from collection data: " + entry.name);
        return false;
      })
      .ToList();
    for (int i = 0; i < plants.Count; i++)
      Asserts.Assert(plants[i].uniqueID.HasLength(), "Plant UniqueID missing for " + plants[i].name);
    // DebugBW.Log(collection + " | plants: " + plants.ToLog());
    return this;
  }


  public void AddToCollection(params PlantIndexEntry[] morePlants) {
    foreach (PlantIndexEntry entry in morePlants)
      if (EntryWithName(entry.name).IsDefault())
        plants.Add(entry);
    Debug.Log("plantNames: " + plants.ToLog());
  }

  public void RemoveFromCollection(params PlantIndexEntry[] entries) {
    if (entries.Length > 0)
      DebugBW.Log("Removing from " + collection + ": " + plants.ToLog(), LColor.orange);
    string[] uids = entries.Select(pie => pie.uniqueID).ToArray();
    int count = plants.RemoveAll(entry => uids.Contains(entry.uniqueID));
    Debug.Log("Removed " + count + " plants, names: " + entries.ToLog());
  }

  public void UpdateEntry(PlantIndexEntry newEntry) {
    int idx = plants.FindIndex(pie => pie.uniqueID == newEntry.uniqueID);
    if (!Asserts.Assert(idx != -1, "Can't update entry for " + newEntry + " | arr: " + plants.ToLog())) return;
    plants[idx] = newEntry;
  }

  public PlantIndexEntry EntryWithName(string s) =>
    plants.Find(entry => entry.name == s);

  [JsonIgnore]
  public string[] PlantNames => plants.Select(entry => entry.name).ToArray();

  public override string ToString() {
    return collection + " [" + plants.ToLog() + "]";
  }
}
