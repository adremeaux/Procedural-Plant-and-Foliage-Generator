using System.Collections.Generic;
using UnityEngine;

namespace BionicWombat {
#if UNITY_EDITOR
  [ExecuteInEditMode]
#endif
  public class LeafBundle : MonoBehaviour {

    private LeafDeps deps = new LeafDeps();
    private LeafParamDict fields;
    private LightLayers lightLayers;
    public LeafStem leafStem;

    public MeshRenderer leafMeshRenderer;
    public SkinnedMeshRenderer stemMeshRenderer;
    public Vector3 collisionAdjustment { get; private set; }

    public void Setup(LeafDeps deps, LeafParamDict fields, LightLayers lightLayers) {
      this.fields = fields;
      this.deps = deps.Copy();
      this.lightLayers = lightLayers;
      OnEnable();
    }

    public void OnEnable() {
      MeshFilter leafMF = transform.GetChild(0).gameObject.GetComponent<MeshFilter>();
      MeshFilter stemMF = transform.GetChild(1).gameObject.GetComponent<MeshFilter>();
      deps.leafData.leafMeshFilter = new Weak<MeshFilter>(leafMF);
      deps.leafData.stemMeshFilter = new Weak<MeshFilter>(stemMF);

      leafMeshRenderer = leafMF.GetComponent<MeshRenderer>();
      stemMeshRenderer = stemMF.GetComponent<SkinnedMeshRenderer>();
      SetLightLayers(lightLayers);
    }

    public void SetLightLayers(LightLayers layers) {
      this.lightLayers = layers;
      leafMeshRenderer.renderingLayerMask = stemMeshRenderer.renderingLayerMask = (uint)lightLayers;
    }

    public void SetMeshes(Mesh leafMesh, Mesh stemMesh, bool setBones) {
      var bend = GetComponent<BendPhysics>();
      if (deps.leafData.GetLeaf() is MeshFilter leafMF && leafMesh != null) {
        leafMF.sharedMesh = leafMesh;
        // bend.SetLeafMesh(leafMesh, leafMeshRenderer); //leaf skinning WIP
      }

      if (deps.leafData.GetStem() is MeshFilter stemMF && stemMesh != null) {
        stemMF.sharedMesh = stemMesh;
        // stemMeshRenderer.sharedMesh = stemMesh;
        if (setBones) {
          bend.SetStemMesh(stemMesh, stemMeshRenderer);
          SetLeafJoint();
        }
      }
    }

    public void SetLeafJoint() {
      if (deps.leafData.GetLeaf() is MeshFilter leaf) {
        GetComponent<BendPhysics>().SetLeafJoint(leaf.gameObject);
      }
    }

    public void SetMaterials(Material leafMat, Material stemMat) {
      leafMeshRenderer.material = leafMat;
      stemMeshRenderer.material = stemMat;
    }

    public void PositionLeaf(LeafFactoryData lfd, ArrangementData arrData, bool attachStem) {
      Transform leafTransform = GetLeafTransform();
      if (leafTransform != null) {
        if (attachStem) {
          (Vector3 leafPos, Quaternion leafRotation) = GetFinalLeafAttachmentInfo(lfd, arrData,
            leafStem.curves.ToArray(), fields[LPK.StemAttachmentAngle].value);
          leafTransform.localPosition = leafPos;
          leafTransform.localRotation = leafRotation;
        } else {
          PositionStem(ArrangementData.Zero);
          leafTransform.position = Vector3.zero;
          leafTransform.rotation = Quaternion.identity;
        }
        stemMeshRenderer.enabled = attachStem && leafMeshRenderer.enabled;
      }
    }

    public void SetCollisionAdjustment(Vector3 adjustment, bool canHide = true) {
      if (adjustment.Equals(PlantPhysicsSimulator.CollideWithPotVector)) {
        this.collisionAdjustment = Vector3.zero;
        if (canHide) gameObject.SetActive(false);
        return;
      }
      gameObject.SetActive(true);
      this.collisionAdjustment = adjustment;
    }

    public void ResetCollisionAdjustment() {
      transform.localPosition -= collisionAdjustment;
      collisionAdjustment = Vector3.zero;
    }

    public Transform GetLeafTransform() => deps.leafData.GetTransform();
    public Mesh GetStemMesh() {
      if (deps.leafData.GetStem() is MeshFilter stemMF) return stemMF.sharedMesh;
      return null;
    }

    // public List<JiggleData> GetJiggleData(PlantLocation loc) =>
    //   GetComponent<BendPhysics>().GetJiggleData(loc);

    // public Jiggle[] GetJiggles() => GetComponent<BendPhysics>().GetJiggles();

    public Transform[] GetBones() => deps.leafData.GetStem().GetComponent<SkinnedMeshRenderer>().bones;

    public static (Vector3 leafPos, Quaternion leafRotation) GetFinalLeafAttachmentInfo(
          LeafFactoryData lfd, ArrangementData arrData, Curve3D[] stemCurves, float stemAttachmentAngle) {
      (Vector3 leafPos, Quaternion leafRotation) = StemRenderer.GetAttachmentInfo(arrData, stemCurves);

      leafRotation = Quaternion.Euler(0, 0, stemAttachmentAngle) * leafRotation;
      Quaternion finalRot = leafRotation;
      if (finalRot.eulerAngles.x > 180f)
        finalRot.eulerAngles = finalRot.eulerAngles.WithX(0f);
      return (leafPos, finalRot);
    }

    public void SetVisible(bool visible) {
      leafMeshRenderer.enabled = visible;
      stemMeshRenderer.enabled = visible;
    }

    public void PositionStem(ArrangementData d) {
      transform.localPosition = d.pos + collisionAdjustment;
      transform.localRotation = d.stemRotation;
      if (deps.leafData.GetTransform() is Transform t) t.localScale = new Vector3(d.scale, d.scale, d.scale);
    }

    public bool NeedsStem() => leafStem == null || leafStem.IsEmpty();

    private void OnDestroy() {
      MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
      leafMeshRenderer.GetPropertyBlock(propBlock);
      LeafMaterialController.DestroyTextures(propBlock);
    }
  }
}
