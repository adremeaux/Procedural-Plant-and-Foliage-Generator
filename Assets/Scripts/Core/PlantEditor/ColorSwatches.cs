using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BionicWombat {
  [ExecuteInEditMode]
  [RequireComponent(typeof(Plant))]
  public class ColorSwatches : MonoBehaviour {
    public GameObject swatchesContainer;
    public RawImage prefab_Swatch;
    private Vector2 size = new Vector2(100, 100);
    private float margin = 30f;
    private RawImage[] swatchBoxes = null;

#if UNITY_EDITOR
    private void Update() {
      if (swatchesContainer == null || prefab_Swatch == null) return;
      Plant plant = GetComponent<Plant>();
      swatchesContainer.SetActive(plant.deps.inspector.showSwatches);
      if (!swatchesContainer.activeSelf) return;

      Dictionary<string, Color> swatchesDict = plant.GetSwatches();
      if (swatchesDict == null) return;

      int numChildren = swatchesContainer.transform.childCount;
      if (swatchBoxes == null || swatchBoxes.Length != Keys.Length || swatchBoxes[0] == null || numChildren != Keys.Length) {
        try {
          for (int i = 0; i < numChildren; i++) DestroyImmediate(swatchesContainer.transform.GetChild(i).gameObject);
        } catch { }
        float yPos = margin;
        Vector2 offset = swatchesContainer.GetComponent<RectTransform>().sizeDelta;
        swatchBoxes = new RawImage[Keys.Length];
        for (int i = 0; i < swatchBoxes.Length; i++) {
          RawImage im = Instantiate<RawImage>(prefab_Swatch, Vector3.zero, Quaternion.identity, swatchesContainer.transform);
          // im.rectTransform.SetSize(size);
          im.transform.localPosition = new Vector2(margin - offset.x, -yPos + offset.y);
          im.gameObject.SetActive(true);
          yPos += margin + size.y;
          swatchBoxes[i] = im;
          im.GetComponentInChildren<TMP_Text>().text = Keys[i].ToString();
        }
      }

      foreach ((string name, int idx) in swatchesDict.Keys.WithIndex()) {
        // Debug.Log("idx: " + idx);
        Color c = swatchesDict[name];
        swatchBoxes[idx].color = c;
      }
    }
#endif

    public static LPK[] Keys => new LPK[] {
    LPK.TexBaseColor,
    LPK.TexShadowStrength,
    LPK.TexVeinColor,
    LPK.TexRadianceHue,
    LPK.TexMarginColor,
    LPK.MaterialRimColor,
    LPK.TrunkBrowning,
  };
  }



}
