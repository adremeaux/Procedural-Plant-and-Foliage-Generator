using UnityEngine;

namespace BionicWombat {
#if UNITY_EDITOR
  [ExecuteInEditMode]
#endif
  [ExecuteAlways]
  public class Turntable : MonoBehaviour {
    public bool active = false;
    private bool wasActive = false;
    private Transform t => GetComponent<Transform>();

    void Update() {
      if (!active) {
        if (wasActive) {
          this.transform.rotation = Quaternion.identity;
          wasActive = false;
        }
        return;
      }
      this.transform.Rotate(0, 2, 0);
      wasActive = true;
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
