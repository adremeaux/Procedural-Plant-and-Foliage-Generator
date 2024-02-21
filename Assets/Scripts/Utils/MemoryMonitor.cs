using UnityEngine;
using UnityEngine.Profiling;

namespace BionicWombat {
#if UNITY_EDITOR
  [ExecuteInEditMode]
#endif
  public class MemoryMonitor : MonoBehaviour {
    public bool active = true;
    public void Update() {
      if (active) {
        float used = (Profiler.GetMonoUsedSizeLong() / 1000f / 1000f).Truncate(3);
        float total = (Profiler.GetMonoHeapSizeLong() / 1000f / 1000f).Truncate(3);
        BatchLogger.Log(used + "mb | Avail: " + total + "mb (" + (used / total * 100) + "%)", "Used Memory", 1, 240);
      }
    }

    void OnDrawGizmos() {
#if UNITY_EDITOR
      if (active && !Application.isPlaying) {
        UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
        UnityEditor.SceneView.RepaintAll();
      }
#endif
    }
  }
}
