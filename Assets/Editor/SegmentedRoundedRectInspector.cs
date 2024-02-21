using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Asserts;

namespace BionicWombat {
  [CustomEditor(typeof(SegmentedRoundedRect))]
  public class SegmentedRoundedRectInspector : BWCustomEditor {
    public override void OnInspectorGUI() {
      SegmentedRoundedRect ctrl = (SegmentedRoundedRect)target;
      AddButton("Draw Poly (Edit Mode)", () => {
        ctrl.SpawnMesh_DEBUG();
        ctrl.Update();
      });
      DrawDefaultInspector();
    }
  }

  [CustomEditor(typeof(SizedQuad))]
  public class SizedQuadInspector : BWCustomEditor {
    public override void OnInspectorGUI() {
      SizedQuad ctrl = (SizedQuad)target;
      AddButton("Draw Poly (Edit Mode)", () => {
        ctrl.GenMesh();
      });
      DrawDefaultInspector();
    }
  }
}
