using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
  public class PlantLiveCache : MonoBehaviour {
    private Dictionary<string, Plant> dict = new Dictionary<string, Plant>();
    private bool shouldOrganize = false;
    public bool GivePlant(Plant plant, PlantSpawner parentSpawner) {
      if (plant == null) return false;
      string uniqueID = plant.indexEntry.uniqueID;
      // DebugBW.Log("PlantCache receiving plant: " + indexEntry.uniqueID, LColor.silver);
      if (dict.HasValueForKey(uniqueID)) {
        // DebugBW.Log("PlantCache already has plant: " + indexEntry.uniqueID, LColor.orange);
        return false;
      }

      dict[uniqueID] = plant;
      plant.transform.parent = transform;
      plant.transform.Reset();
      // plant.SetJiggleSimEnabled(false);
      // parentSpawner.ClearJiggles();

      shouldOrganize = true;
      return true;
    }

    public Plant TakePlant(PlantIndexEntry indexEntry, PlantSpawner parentSpawner) {
      if (!dict.HasValueForKey(indexEntry.uniqueID)) return null;
      Plant plant = dict[indexEntry.uniqueID];
      // plant.SetJiggleSimEnabled(true);
      plant.transform.parent = parentSpawner.transform;
      plant.transform.Reset();
      dict.Remove(indexEntry.uniqueID);
      shouldOrganize = true;
      return plant;
    }

    public bool HasPlant(PlantIndexEntry indexEntry) => dict.HasValueForKey(indexEntry.uniqueID);

    public void Organize() {
      int count = dict.Count;
      float sqrt = Mathf.Sqrt(count);
      int rows = Mathf.CeilToInt(sqrt);

      var keys = dict.Keys.ToArray();
      for (int i = 0; i < rows; i++) {
        for (int j = 0; j < rows; j++) {
          int idx = (i * rows) + j;
          if (idx >= keys.Length) break;
          dict[keys[idx]].transform.localPosition = new Vector3(i, 0, j);
        }
      }
    }

    private void Update() {
      if (shouldOrganize) {
        shouldOrganize = false;
        Organize();
      }
    }
  }
}
