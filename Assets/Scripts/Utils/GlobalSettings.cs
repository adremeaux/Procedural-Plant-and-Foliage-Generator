using System;
using UnityEngine;

namespace BionicWombat {
  [Serializable]
  [ExecuteInEditMode]
  public class GlobalSettings : MonoBehaviour {
    [SerializeField]
    public bool SplitTimerEnabled;

    public void Awake() {
      SplitTimerEnabled = SplitTimer.globalEnable;
    }

    private void Update() {
      SplitTimer.globalEnable = SplitTimerEnabled;
    }
  }

}
