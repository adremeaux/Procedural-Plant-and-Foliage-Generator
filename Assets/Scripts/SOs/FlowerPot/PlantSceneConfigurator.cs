using System;
using System.Collections.Generic;
using UnityEngine;

namespace BionicWombat {
  public class PlantSceneConfigurator : MonoBehaviour {
    public List<PlantSpawnerSetup> configs;
    public bool enable = true;
    [Serializable]

    public struct PlantSpawnerSetup {
      public PlantSpawner spawner;
      public string name;
      public FlowerPotType potType;
    }

    // public void Start() {
    //   if (enable) {
    //     foreach (PlantSpawnerSetup s in configs) {
    //       s.spawner.DidTapMenuItem(s.name, s.potType.ToString());
    //     }
    //   }
    // }
  }

}
