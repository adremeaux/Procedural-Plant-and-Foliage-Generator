using System;
using System.Collections.Generic;
using UnityEngine;

namespace BionicWombat {
  public class UIController : MonoBehaviour {
    public PlantSpawner parent1;
    public PlantSpawner parent2;
    public PlantSpawner[] resultSpawners;

    public void SaveButtonPressed() {
      resultSpawners[0].SavePlantAs(null, PlantCollection.User);
    }

    public void DeleteSavedButtonPressed() {
      DataManager.DeleteCollection(PlantCollection.User);
    }

    public void HybridizeButtonPressed() {
      LeafParamDict f1 = parent1.GetSpawnedParams();
      LeafParamDict f2 = parent2.GetSpawnedParams();

      if (f1 == null || f2 == null) return;

      int count = resultSpawners.Length;
      for (int i = 0; i < count; i++) {
        float perc = ((i + 1f) / (count + 1f));
        LeafParamDict result = Hybridizer.Hybridize(f1, f2, perc);
        resultSpawners[i].SpawnHybrid(result, parent1.GetPlantName() + " x " + parent2.GetPlantName() + " " + perc.Truncate(2) + "x" + (1f - perc).Truncate(2));
      }
    }
  }
}