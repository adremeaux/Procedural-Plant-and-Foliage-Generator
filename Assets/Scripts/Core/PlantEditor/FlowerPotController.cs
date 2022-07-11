using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using static BionicWombat.TransformExtentions;

namespace BionicWombat {
  public enum FlowerPotType {
    Terracotta,
    Lacquer,
    Clay1,
    Nursery1,
  }

#if UNITY_EDITOR
  [ExecuteInEditMode]
#endif
  [Serializable]
  public class FlowerPotController {
    private GameObject pot;
    private FlowerPotData data;

    public void LoadPot(FlowerPotType type, Transform parent) {
      if (parent == null) return;
      float oldScale = pot != null ? pot.transform.localScale.x : 1f;
      ClearPot();
      data = FlowerPotMap.GetFlowerPotData(type);
      pot = GameObject.Instantiate<GameObject>(data.modelPrefab, parent);
      pot.transform.Reset();
      SetScale(oldScale);
      pot.GetComponent<MeshRenderer>().renderingLayerMask = (uint)(LightLayers.Default | LightLayers.Plant);
    }

    public void SetEnabled(bool enabled) {
      if (pot == null) return;
      pot.GetComponent<MeshRenderer>().enabled = enabled;
    }

    public void SetScale(float scale) {
      if (pot == null) return;
      pot.transform.localScale = new Vector3(scale, scale, scale);
    }

    public void SetVisible(bool vis) {
      if (pot != null) pot.SetActive(vis);
    }

    public float GetCurrentYPos() => data != null ? data.yAttachmentPos : 0f;

    private void ClearPot() {
      if (pot == null) return;
      GameObject.DestroyImmediate(pot);
      pot = null;
    }

    public void Clear() {
      ClearPot();
      data = null;
    }

    public bool hasPot => pot != null;
  }
}