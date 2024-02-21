using UnityEngine;

namespace BionicWombat {
#if UNITY_EDITOR
  [ExecuteInEditMode]
#endif
  public class LookAt : MonoBehaviour {
    public Transform target;
    public Renderer targetRenderer;
    public Vector3 offset = Vector3.zero;
    void Update() {
      if (target == null) return;
      Bounds b = targetRenderer != null ? targetRenderer.bounds : new Bounds();
      Vector3 boundsOffset = new Vector3(0, b.size.y / -2f + b.max.y, 0);
      boundsOffset = Vector3.zero;
      transform.LookAt(target.position + offset + boundsOffset);
    }
  }
}
