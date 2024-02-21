using UnityEngine;

namespace BionicWombat {
#if UNITY_EDITOR
  [ExecuteInEditMode]
#endif
  public class FaceAt : MonoBehaviour {
    public Transform target;
    public Vector3 upDirection = Vector3.up;
    public new bool enabled = true;
    public bool up = false;
    public bool down = false;
    public bool left = false;
    public bool right = false;
    public bool forward = false;
    public bool back = false;
    public Vector3 extraRotation = Vector3.zero;

    void Update() {
      if (target == null) return;
      Quaternion extra = Quaternion.Euler(extraRotation);
      if (!enabled) {
        this.transform.rotation = extra;
        return;
      }
      if (up) upDirection = Vector3.up;
      if (down) upDirection = Vector3.down;
      if (left) upDirection = Vector3.left;
      if (right) upDirection = Vector3.right;
      if (forward) upDirection = Vector3.forward;
      if (back) upDirection = Vector3.back;
      up = down = left = right = forward = back = false;
      Quaternion q = Quaternion.LookRotation(target.position, upDirection);
      this.transform.rotation = q * extra;
    }
  }
}
