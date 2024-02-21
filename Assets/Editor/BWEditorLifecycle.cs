using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BionicWombat {
  [InitializeOnLoadAttribute]
  public static class BWEditorLifecycle {
    static BWEditorLifecycle() {
      EditorApplication.playModeStateChanged += PlayModeStateChanged;
    }

    private static void PlayModeStateChanged(PlayModeStateChange state) {
      DebugBW.Log("Play Mode State Change: " + state, LColor.purple);
      if (state == PlayModeStateChange.ExitingPlayMode) {
        PlantDataManager.ClearTemporary();
        PlantDataManager.SweepOrphans();
      }
    }
  }
}
