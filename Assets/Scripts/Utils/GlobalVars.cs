using UnityEngine;
using UnityEngine.Rendering;

namespace BionicWombat {
  [ExecuteInEditMode]
  public class GlobalVars : MonoBehaviour {
    public bool UseOldDistortion = false;
    public bool ShowCoalescingSplits = true;
    public bool useDefaultBasedMutations = false;
    public bool UseTexturePreloadCache = true;
    public bool UsePlantCache = true;

    private static GlobalVars _instance;
    public static GlobalVars instance {
      get {
        // if (_instance == null) _instance = new GlobalVars();
        return _instance;
      }
    }

    public void SetInstance() {
      _instance = this;
    }

#if UNITY_EDITOR
    private void Update() {
      if (instance == null) SetInstance();
    }
#endif
  }
}
