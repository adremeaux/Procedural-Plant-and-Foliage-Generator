using System.Collections.Generic;
using UnityEngine;

namespace BionicWombat {
  [ExecuteInEditMode]
  public class LeafPhysicsTestLeaf : MonoBehaviour {
    public GameObject zAxisRotContainer;

    public Collider leafCollider;
    public Transform leafTransform => leafCollider.GetComponent<Transform>();
    public Mesh leafMesh => leafCollider.GetComponent<MeshFilter>().sharedMesh;

    public Collider stemCollider;
    public Transform stemTransform => stemCollider.GetComponent<Transform>();
    public Mesh stemMesh => stemCollider.GetComponent<MeshFilter>().sharedMesh;

    public void Update() {
    }
  }
}
