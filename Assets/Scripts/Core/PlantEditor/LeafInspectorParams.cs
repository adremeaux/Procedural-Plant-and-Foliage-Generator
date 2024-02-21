using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BionicWombat {
  public enum LeafDisplayMode {
    Bezier,
    Veins,
    PreRender,
    Mesh,
    Normals,
    DistortionMesh,
    StemWire,
    Propegating,
    Plant,
  }

  [Serializable]
  public class LeafInspectorParams {
    public bool showSmooth = true;
    public bool showApprox = false;
    public bool showApproxVerts = false;
    public bool showHandles = true;
    public bool showIntersections = false;
    public bool showLinearPoints = false;
    public bool showVeins = true;
    public bool showVeinHandles = true;
    public bool showGravityCurve = false;
    public bool showVeinPolys = false;
    public bool showDistortionCurves = false;
    public bool showStem = false;
    public bool showBoundingBox = false;
    public bool showSwatches = true;
    public bool showVertexNormals = false;
    public bool showSurfaceNormals = false;

    public override string ToString() {
      string s = "";
      FieldInfo[] fields = typeof(LeafInspectorParams).GetFields();
      foreach (FieldInfo field in fields) {
        s += field.Name + ":" + field.GetValue(this) + ",";
      }
      return s;
    }

    public LeafInspectorParams Copy() {
      LeafInspectorParams b = new LeafInspectorParams();
      FieldInfo[] fields = typeof(LeafInspectorParams).GetFields();
      foreach (FieldInfo field in fields)
        field.SetValue(b, field.GetValue(this));
      return b;
    }

    public void SetAll(bool val) {
      FieldInfo[] fields = typeof(LeafInspectorParams).GetFields();
      foreach (FieldInfo field in fields) {
        field.SetValue(this, val);
      }
    }

    public void SetDisplayMode(LeafDisplayMode mode) {
      SetAll(false);
      if (mode == LeafDisplayMode.Bezier) {
        showSmooth = true;
        showHandles = true;
        showIntersections = true;
      } else if (mode == LeafDisplayMode.Veins) {
        showApprox = true;
        showLinearPoints = true;
        showVeins = true;
        showGravityCurve = true;
      } else if (mode == LeafDisplayMode.PreRender) {
        showApprox = true;
        showVeinPolys = true;
        showSwatches = true;
      } else if (mode == LeafDisplayMode.Mesh) {
        showApprox = true;
        showApproxVerts = true;
        showSwatches = true;
      } else if (mode == LeafDisplayMode.Normals) {
        showSwatches = true;
        showVertexNormals = true;
        showSurfaceNormals = true;
      } else if (mode == LeafDisplayMode.DistortionMesh) {
        showSmooth = true;
        showDistortionCurves = true;
        showSwatches = true;
      } else if (mode == LeafDisplayMode.StemWire) {
        showStem = true;
        showSwatches = true;
      } else if (mode == LeafDisplayMode.Propegating) {
        showSwatches = true;
      } else if (mode == LeafDisplayMode.Plant) {
        showSwatches = true;
      }
    }

    public static bool HideDistortionForMode(LeafDisplayMode mode) {
      switch (mode) {
        case LeafDisplayMode.Bezier:
        case LeafDisplayMode.Veins:
        case LeafDisplayMode.PreRender:
        case LeafDisplayMode.Mesh:
          return true;
        case LeafDisplayMode.Normals:
        case LeafDisplayMode.DistortionMesh:
        case LeafDisplayMode.StemWire:
        case LeafDisplayMode.Propegating:
        case LeafDisplayMode.Plant:
          return false;
      }
      Debug.LogError("ShowDistortionForMode not implemented: " + mode);
      return false;
    }

    public static bool ShowMeshForMode(LeafDisplayMode mode) {
      switch (mode) {
        case LeafDisplayMode.Bezier:
        case LeafDisplayMode.Veins:
        case LeafDisplayMode.PreRender:
          return false;
        case LeafDisplayMode.DistortionMesh:
        case LeafDisplayMode.Mesh:
        case LeafDisplayMode.Normals:
        case LeafDisplayMode.StemWire:
        case LeafDisplayMode.Propegating:
        case LeafDisplayMode.Plant:
          return true;
      }
      Debug.LogError("ShowMeshForMode not implemented: " + mode);
      return false;
    }

    public static bool ShowOnlyLastForMode(LeafDisplayMode mode) {
      switch (mode) {
        case LeafDisplayMode.Bezier:
        case LeafDisplayMode.Veins:
        case LeafDisplayMode.PreRender:
        case LeafDisplayMode.DistortionMesh:
        case LeafDisplayMode.Mesh:
        case LeafDisplayMode.Normals:
        case LeafDisplayMode.StemWire:
        case LeafDisplayMode.Plant:
          return false;
        case LeafDisplayMode.Propegating:
          return true;
      }
      Debug.LogError("ShowOnlyLastForMode not implemented: " + mode);
      return false;
    }

    public static bool AttachStemForMode(LeafDisplayMode mode) {
      switch (mode) {
        case LeafDisplayMode.Bezier:
        case LeafDisplayMode.Veins:
        case LeafDisplayMode.PreRender:
        case LeafDisplayMode.DistortionMesh:
        case LeafDisplayMode.Mesh:
        case LeafDisplayMode.Normals:
          return false;
        case LeafDisplayMode.StemWire:
        case LeafDisplayMode.Propegating:
        case LeafDisplayMode.Plant:
          return true;
      }
      Debug.LogError("AttachStemForMode not implemented: " + mode);
      return false;
    }

    public static bool HideTrunkForMode(LeafDisplayMode mode) {
      switch (mode) {
        case LeafDisplayMode.Bezier:
        case LeafDisplayMode.Veins:
        case LeafDisplayMode.PreRender:
        case LeafDisplayMode.DistortionMesh:
        case LeafDisplayMode.Mesh:
        case LeafDisplayMode.Normals:
        case LeafDisplayMode.StemWire:
        case LeafDisplayMode.Propegating:
          return true;
        case LeafDisplayMode.Plant:
          return false;
      }
      Debug.LogError("AttachStemForMode not implemented: " + mode);
      return false;
    }

#if UNITY_EDITOR
    public static DrawCameraMode DrawModeForMode(LeafDisplayMode mode) {
      switch (mode) {
        case LeafDisplayMode.Bezier:
        case LeafDisplayMode.Veins:
        case LeafDisplayMode.PreRender:
        case LeafDisplayMode.Mesh:
        case LeafDisplayMode.Normals:
        case LeafDisplayMode.Propegating:
        case LeafDisplayMode.Plant:
          return DrawCameraMode.Textured;

        case LeafDisplayMode.StemWire:
          return DrawCameraMode.Wireframe;

        case LeafDisplayMode.DistortionMesh:
          return DrawCameraMode.TexturedWire;
      }
      Debug.LogError("DrawModeForMode not implemented: " + mode);
      return DrawCameraMode.Textured;
    }
#endif

    public static bool? Use2DForMode(LeafDisplayMode mode) {
      switch (mode) {
        case LeafDisplayMode.Bezier:
        case LeafDisplayMode.Veins:
        case LeafDisplayMode.PreRender:
        case LeafDisplayMode.Mesh:
          return true;
        case LeafDisplayMode.DistortionMesh:
        case LeafDisplayMode.Plant:
        case LeafDisplayMode.Propegating:
          return false;
        case LeafDisplayMode.StemWire:
        case LeafDisplayMode.Normals:
          return null;
      }
      Debug.LogError("Use2DForMode not implemented: " + mode);
      return null;
    }
  }

}
