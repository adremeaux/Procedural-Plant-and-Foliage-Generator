using System;
using UnityEngine;

namespace BionicWombat {
  public class PlantDataManagerDebugger : MonoBehaviour {
    public void StaticReinit() {
      PlantDataManager.StaticAwake();
    }
  }
}
