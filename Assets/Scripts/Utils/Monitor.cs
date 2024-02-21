using UnityEngine;

namespace BionicWombat {
#if UNITY_EDITOR
  [ExecuteInEditMode]
#endif
  [ExecuteAlways]
  public class Monitor : MonoBehaviour {
    public bool enableLogging = true;
    public int uniqueID = -1;
    public bool logDimensions = true;
    public Renderer rend;
    private static int _uniqueID = 1000;
    private Bounds lastBounds;
    public void Update() {
      if (uniqueID == -1) uniqueID = ++_uniqueID;
      if (rend == null) rend = GetComponent<Renderer>();
      if (rend == null || !enableLogging) return;

      if (logDimensions) {
        if (rend.bounds != lastBounds)
          Debug.Log("Bounds" + uniqueID + ": " + rend.bounds.ToString());
        lastBounds = rend.bounds;
      }
    }

    void OnDrawGizmos() {
#if UNITY_EDITOR
      if (!Application.isPlaying) {
        UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
        UnityEditor.SceneView.RepaintAll();
      }
#endif
    }
  }
}
