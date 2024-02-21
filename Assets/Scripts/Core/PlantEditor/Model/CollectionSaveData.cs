using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using BionicWombat.BWSaving;
using ImageMagick;
using Newtonsoft.Json;
using UnityEngine;
using static Asserts;

namespace BionicWombat.BWSaving {
  [Serializable]
  internal struct CollectionSaveData {
    [SerializeField][JsonPropertyAttribute] private PresetCollection[] data;

    public override string ToString() => this.PrettyPrint();

    [JsonConstructor]
    public CollectionSaveData(PresetCollection[] collections) {
      DebugBW.Log("collections: " + collections.ToLog());
      if (collections == null) collections = new PresetCollection[0];
      data = collections;
    }

    public CollectionSaveData DidDeserialize() {
      DebugBW.Log("        " + System.Reflection.MethodBase.GetCurrentMethod().Name, LColor.lightblue);
      for (int i = 0; i < data.Length; i++)
        data[i] = data[i].DidDeserialize();
      List<PresetCollection> newData = new List<PresetCollection>();
      List<PlantCollection> cols = new List<PlantCollection>();
      foreach (PresetCollection pc in data) {
        if (!cols.Contains(pc.collection)) {
          newData.Add(pc);
          cols.Add(pc.collection);
        } else {
          DebugBW.Log("Removing duplicate collection " + pc.collection, LColor.brown);
        }
      }
      data = newData.ToArray();
      return this;
    }

    public void Migrate() {
      // for (int i = 0; i < data.Length; i++) {
      //   PresetCollection col = data[i];
      //   List<PlantIndexEntry> l = new List<PlantIndexEntry>();
      //   foreach (string plantName in col.plantNames) {
      //     l.Add(new PlantIndexEntry(plantName, DateTime.Now));
      //   }
      //   col.plants = l.ToArray();
      //   data[i] = col;
      // }
    }

    public void AddCollection(PresetCollection col) {
      if (col.IsDefault()) return;
      if (data == null) data = new PresetCollection[0];
      data = data.ToList().Adding(col).ToArray();
    }

    public void SetCollection(PlantCollection collectionName, PresetCollection newPresetCol) {
      if (data == null) data = new PresetCollection[0];
      int idx = Array.FindIndex(data, pc => pc.collection == collectionName);
      if (idx != -1) data[idx] = newPresetCol;
      else Debug.LogWarning("CollectionSaveData SetCollection failed for " + collectionName);
    }

    public PresetCollection[] GetCollections() => data;
    public PresetCollection GetCollection(PlantCollection colName) {
      if (colName == PlantCollection.SeedBank) return GetSeedBank();
      if (data != null)
        foreach (PresetCollection col in data)
          if (col.collection == colName) return col;
      return default(PresetCollection);
    }

    public PresetCollection GetSeedBank() {
      List<PlantIndexEntry> l = new List<PlantIndexEntry>();
      List<PlantCollection> lc = new List<PlantCollection>();
      PlantCollection[] visibleCols = PlantDataManager.GetVisibleCollections();
      foreach (PresetCollection col in data) {
        if (!visibleCols.Contains(col.collection)) continue;
        foreach (PlantIndexEntry entry in col.plants) {
          if (entry.hybridsRemaining == 0) {
            l.Add(entry);
            lc.Add(col.collection);
          }
        }
      }
      return new PresetCollection(PlantCollection.SeedBank, l.ToArray(), lc);
    }

    public PlantData[] GetPropegatingPlants() {
      List<PlantData> l = new List<PlantData>();
      foreach (PresetCollection col in data) {
        foreach (PlantIndexEntry entry in col.plants) {
          if (entry.propegating) {
            l.Add(new PlantData(entry, col.collection));
          }
        }
      }
      return l.ToArray();
    }

    public CollectionSaveData MigrateSavedPlantBetweenCollections(PlantIndexEntry entry, PlantCollection from, PlantCollection to) {
      PresetCollection fromCol = GetCollection(from);
      PresetCollection toCol = GetCollection(to);
      fromCol.RemoveFromCollection(entry);
      toCol.AddToCollection(entry);

      return this;
    }

    public PlantIndexEntry RenamePlant(PlantIndexEntry entry, string newName, PlantCollection collection) {
      PresetCollection col = GetCollection(collection);
      entry.name = newName;
      DebugBW.Log("entry: " + entry);
      col.UpdateEntry(entry);
      SetCollection(collection, col);
      return entry;
    }
  }
}
