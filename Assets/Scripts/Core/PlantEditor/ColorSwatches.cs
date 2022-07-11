using System;
using System.Collections.Generic;
using System.Linq;
//using Shapes;
using UnityEngine;
using UnityEngine.Rendering;

namespace BionicWombat {
/*
#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
[RequireComponent(typeof(Plant))]
public class ColorSwatches : ImmediateModeShapeDrawer {
  public override void DrawShapes(Camera cam) {
#if UNITY_EDITOR
    if (UnityEditor.SceneView.lastActiveSceneView == null) return;
    cam = UnityEditor.SceneView.lastActiveSceneView.camera;
    using (Draw.Command(cam)) {
      Draw.Color = Color.blue;
      Draw.Matrix = cam.transform.localToWorldMatrix;
      Draw.Translate(0, 0, cam.nearClipPlane);

      float height = cam.orthographicSize * 2f;
      float width = height * cam.aspect;
      float margin = width / 70f;
      float targetSize = width / 15f;
      float yPos = margin + targetSize;

      Dictionary<string, Color> swatches = GetComponent<Plant>().GetSwatches();
      if (swatches == null) return;

      foreach (string name in swatches.Keys.Reverse()) {
        Draw.Color = swatches[name];
        Draw.Rectangle(new Vector3(width / 2f - margin, height / -2f + yPos, 0), new Rect(0, 0, -targetSize, -targetSize));
        yPos += margin + targetSize;
      }
    }
#endif
  }

  public static LPK[] Keys => new LPK[] {
    LPK.TexBaseColor,
    LPK.TexShadowStrength,
    LPK.TexVeinColor,
    LPK.TexRadianceHue,
    LPK.TexMarginColor,
    LPK.MaterialRimColor,
    LPK.TrunkBrowning,
  };

}*/



}