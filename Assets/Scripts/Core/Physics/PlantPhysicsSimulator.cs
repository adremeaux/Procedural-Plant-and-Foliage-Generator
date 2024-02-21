using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
  // MakeStruct.html?a=PhysicsSimColliderInfo Collider collider Transform transform Mesh mesh bool enabled
  public struct PhysicsSimColliderInfo {
    public Collider collider;
    public Transform transform;
    public Mesh mesh;
    public bool enabled;

    public PhysicsSimColliderInfo(Collider collider, Transform transform, Mesh mesh, bool enabled) {
      this.collider = collider;
      this.transform = transform;
      this.mesh = mesh;
      this.enabled = enabled;
    }

    public override string ToString() {
      return "[PhysicsSimColliderInfo] collider: " + collider + " | transform: " + transform + " | mesh: " + mesh + " | enabled: " + enabled;
    }
  }

  [RequireComponent(typeof(MeshCollider))]
  public class PlantPhysicsSimulator : MonoBehaviour {
    public MeshCollider meshCollider;
    public MeshFilter debugMeshFilter;
    public Transform scaler;

    public static Vector3 CollideWithPotVector = Vector3.negativeInfinity;

    public Vector3[] SolveCollisions(PhysicsSimColliderInfo[] leaves, PhysicsSimColliderInfo potCollider = default(PhysicsSimColliderInfo)) {
      Mesh m = new Mesh();
      m.name = "Collision Mesh";
      meshCollider.sharedMesh = m;

      Vector3[] adjustments = new Vector3[leaves.Length];
      adjustments[0] = Vector3.zero;
      float scaleAdjust = 1f / scaler.localScale.x;

      Vector3 direction;
      float distance;
      for (int i = 0; i < leaves.Length - 1; i++) {
        PhysicsSimColliderInfo leaf2 = leaves[i + 1];
        Collider col2 = leaf2.collider;
        Transform trans2 = leaf2.transform;
        if (!leaf2.enabled) continue;
        if (i == 0) {
          PhysicsSimColliderInfo leaf1 = leaves[i];
          Collider col1 = leaf1.collider;
          Transform trans1 = leaf1.transform;

          //pass B first as that's the one we want to move
          Physics.ComputePenetration(col2, trans2.position, trans2.rotation, col1, trans1.position, trans1.rotation, out direction, out distance);
          adjustments[i + 1] = (direction * distance * scaleAdjust);

          m = CombineMeshes(m, leaf1.mesh, leaf1.transform, Vector3.zero);
          meshCollider.sharedMesh = m;
          if (debugMeshFilter != null) debugMeshFilter.sharedMesh = m;
        } else {
          Physics.ComputePenetration(col2, trans2.position, trans2.rotation, meshCollider, Vector3.zero, Quaternion.identity, out direction, out distance);
          adjustments[i + 1] += (direction * distance * scaleAdjust);
        }

        if (i != leaves.Length - 1) {
          m = CombineMeshes(m, leaf2.mesh, leaf2.transform, adjustments[i + 1] / scaleAdjust);
          meshCollider.sharedMesh = m;
          if (debugMeshFilter != null) debugMeshFilter.sharedMesh = m;
        }
      }

      if (!potCollider.IsDefault()) {
        for (int i = 0; i < leaves.Length; i++) {
          PhysicsSimColliderInfo leaf = leaves[i];
          if (!leaf.enabled) continue;
          Collider col = leaf.collider;
          Transform trans = leaf.transform;
          Vector3 adj = adjustments[i] / scaleAdjust;
          Physics.ComputePenetration(potCollider.collider, potCollider.transform.position, potCollider.transform.rotation,
            col, trans.position + adj, trans.rotation, out direction, out distance);
          if (distance > 0) adjustments[i] = CollideWithPotVector;
        }
      }

      return adjustments;
    }

    private static Mesh CombineMeshes(Mesh baseMesh, Mesh newMesh, Transform newTransform, Vector3 adjustment) {
      CombineInstance[] cis = new CombineInstance[2];

      CombineInstance ciBase = new CombineInstance();
      ciBase.mesh = baseMesh;
      ciBase.transform = Matrix4x4.identity;
      cis[0] = ciBase;

      CombineInstance ci = new CombineInstance();
      ci.mesh = newMesh;
      ci.transform = newTransform.localToWorldMatrix;
      // Debug.Log("[Before] ci.transform: " + ci.transform.GetColumn(3) + " | adjustment: " + adjustment);
      ci.transform = ci.transform.Translate(adjustment);
      // Debug.Log("[After]  ci.transform: " + ci.transform.GetColumn(3));
      cis[1] = ci;

      Mesh finalMesh = new Mesh();
      finalMesh.CombineMeshes(cis, true, true);
      return finalMesh;
    }

    public static PhysicsSimColliderInfo[] CreateLeafData(LeafBundle[] bundles, LeafFactoryData lfd, LeafParamDict fields) {
      PhysicsSimColliderInfo[] infos = new PhysicsSimColliderInfo[bundles.Length];
      DistortionCurve[] firstCurves = lfd.distortionCurves[0];
      float flopZAdjust = 0f;
      float flopMinY = 0f;
      List<Vector3> allDistPoints = new List<Vector3>();
      foreach (DistortionCurve dc in firstCurves) {
        allDistPoints.Add(dc.distortionPoints);
        if (dc.config.type == LeafDistortionType.Flop) {
          Vector3[] distPoints = dc.distortionPoints;
          (Vector3 flopMin, Vector3 flopMax) = distPoints.GetExtents();
          flopZAdjust = flopMax.z - flopMin.z;
          flopMinY = flopMin.y;
        }
      }
      (Vector3 min, Vector3 max) = allDistPoints.ToArray().GetExtents();
      Vector3 adjust = new Vector3(max.x - min.x, max.y - flopMinY, flopZAdjust - min.z);
      float dampen = Mathf.Clamp01(fields[LPK.PhysicsAmplification].value);
      adjust = adjust.MultZ(dampen);

      foreach ((LeafBundle bundle, int idx) in bundles.WithIndex()) {
        bundle.gameObject.SetActive(true);
        Transform trans = bundle.GetLeafTransform();
        BoxCollider col = bundle.leafMeshRenderer.gameObject.GetComponent<BoxCollider>();
        if (col != null) { DestroyImmediate(bundle.leafMeshRenderer.GetComponent<Collider>()); col = null; }
        if (col == null) {
          col = bundle.leafMeshRenderer.gameObject.AddComponent<BoxCollider>();
          col.size = adjust;
          col.center = col.size / 2f;
          col.center = col.center.AddY(flopMinY).WithX(0f);
        }
        // Mesh mesh = bundle.leafMeshRenderer.GetComponent<MeshFilter>().sharedMesh;
        Mesh mesh = col.GetMesh();
        PhysicsSimColliderInfo info = new PhysicsSimColliderInfo(col, trans, mesh, dampen > 0f);
        infos[idx] = info;
      }
      return infos;
    }

    public static PhysicsSimColliderInfo CreatePotData(FlowerPotController controller) {
      GameObject pot = controller.pot;
      if (pot == null) return default(PhysicsSimColliderInfo);
      BoxCollider col = pot.GetComponent<BoxCollider>();
      if (col != null) DestroyImmediate(pot.GetComponent<Collider>());
      col = pot.AddComponent<BoxCollider>();
      PhysicsSimColliderInfo info = new PhysicsSimColliderInfo(col, pot.transform, null, true);
      return info;
    }

    public void Cleanup(LeafBundle[] bundles) {
      meshCollider.sharedMesh = null;

      foreach (LeafBundle bundle in bundles)
        bundle.leafMeshRenderer.GetComponent<Collider>().enabled = false;
      //   DestroyImmediate(bundle.leafMeshRenderer.GetComponent<Collider>());
    }
  }

}
