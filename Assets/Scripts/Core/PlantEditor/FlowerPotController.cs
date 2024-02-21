using System;
using UnityEngine;

namespace BionicWombat {
  public enum FlowerPotType : uint {
    NurseryBlack = 0,
    NurseryGreen = 1,
    NurseryBrown = 2,
    Terracotta = 100,
    TerracottaPale = 101,
    LacquerBlue = 200,
    LacquerGreen = 201,
    LacquerRed = 202,
    LacquerWhite = 203,
    LacquerPurple = 204,
    LacquerBlack = 205,
    Clay1 = 300,
    Stripe_Blue = 400,
    Stripe_Terra_White = 401,
    TileGeometric = 500,
    TerrazzoGreen = 600,
    TerrazzoPink = 601,
    TerrazzoWhite = 602,
  }

#if UNITY_EDITOR
  [ExecuteInEditMode]
#endif
  [Serializable]
  public class FlowerPotController {
    public GameObject pot { get; private set; }
    public FlowerPotData data { get; private set; }
    private LightLayers queuedLightLayers = 0;

    public void LoadPot(FlowerPotType type, Transform parent, bool useDataScale) {
      if (parent == null) return;
      float oldScale = pot != null ? pot.transform.localScale.x : 1f;
      ClearPot();
      data = FlowerPotMap.GetFlowerPotData(type);
      pot = GameObject.Instantiate<GameObject>(data.modelPrefab, parent);
      pot.transform.Reset();
      if (queuedLightLayers != 0) SetLightLayers(queuedLightLayers);
      SetScale(useDataScale ? data.shelfScale * 1.2f : oldScale);
    }

    public void SetLightLayers(LightLayers lightLayers) {
      if (pot != null) pot.GetComponent<MeshRenderer>().renderingLayerMask = (uint)lightLayers;
      else queuedLightLayers = lightLayers;
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

    public void AddCollider() {
      if (!pot.GetComponent<Collider>())
        pot.AddComponent<BoxCollider>();
    }

    public void SetColliderEnabled(bool enabled) {
      var col = pot?.GetComponent<Collider>();
      if (col != null)
        col.enabled = enabled;
    }

    public float GetCurrentYPos() => data.IsNotDefault() ? data.yAttachmentPos : 0f;

    private void ClearPot() {
      if (pot == null) return;
      GameObject.DestroyImmediate(pot);
      pot = null;
    }

    public void Clear() {
      ClearPot();
      data = default(FlowerPotData);
    }

    public bool hasPot => pot != null;
  }
}
