using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
  public MeshRenderer stemMeshRenderer;

  public void Setup(LeafDeps deps, LeafParamDict fields, LightLayers lightLayers) {
    this.fields = fields;
    this.deps = deps.Copy();
    this.lightLayers = lightLayers;
    OnEnable();
  }

  public void OnEnable() {
    deps.leafData.leafGameObject = transform.GetChild(0).gameObject;
    deps.leafData.leafTransform = deps.leafData.leafGameObject.transform;
    deps.leafData.stemGameObject = transform.GetChild(1).gameObject;

    leafMeshRenderer = deps.leafData.leafGameObject.GetComponent<MeshRenderer>();
    stemMeshRenderer = deps.leafData.stemGameObject.GetComponent<MeshRenderer>();
    leafMeshRenderer.renderingLayerMask = stemMeshRenderer.renderingLayerMask = (uint)lightLayers;
  }

  public void SetMeshes(Mesh leafMesh, Mesh stemMesh) {
    deps.leafData.leafGameObject.GetComponent<MeshFilter>().sharedMesh = leafMesh;
    deps.leafData.stemGameObject.GetComponent<MeshFilter>().sharedMesh = stemMesh;
  }

  public void SetMaterials(Material leafMat, Material stemMat) {
    leafMeshRenderer.material = leafMat;
    stemMeshRenderer.material = stemMat;
  }

  public void PositionLeaf(LeafFactoryData lfd, bool attachStem) {
    if (attachStem) {
      (Vector3 leafPos, Quaternion leafRotation) = StemRenderer.GetAttachmentInfo(leafStem);
      deps.leafData.leafGameObject.transform.localPosition = leafPos;

      leafRotation = Quaternion.Euler(0, 0, fields[LPK.StemAttachmentAngle].value) * leafRotation;
      deps.leafData.leafGameObject.transform.localRotation = leafRotation;
    } else {
      PositionStem(ArrangementData.Zero);
      deps.leafData.leafGameObject.transform.position = Vector3.zero;
      deps.leafData.leafGameObject.transform.rotation = Quaternion.identity;
    }
    stemMeshRenderer.enabled = attachStem;
  }

  public void SetVisible(bool visible) {
    leafMeshRenderer.enabled = visible;
    stemMeshRenderer.enabled = visible;
  }

  public void PositionStem(ArrangementData d) {
    transform.localPosition = d.pos;
    transform.localRotation = d.rotation;
    deps.leafData.leafTransform.localScale = new Vector3(d.scale, d.scale, d.scale);
  }

  public bool NeedsStem() => leafStem == null || leafStem.IsEmpty();
}
}