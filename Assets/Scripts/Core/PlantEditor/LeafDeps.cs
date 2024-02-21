using System;
using System.Reflection;
using UnityEngine;

namespace BionicWombat {
  [Serializable]
  public class LeafDeps {
    public LeafData leafData;
    public BaseParams baseParams = new BaseParams();
    public LeafInspectorParams inspector = new LeafInspectorParams();
    public LeafLogOptions logOptions = new LeafLogOptions();

    public override string ToString() {
      string s = "";
      FieldInfo[] fields = typeof(LeafDeps).GetFields();
      foreach (FieldInfo field in fields) {
        s += field.Name + ":" + field.GetValue(this) + ",";
      }
      return s;
    }

    public LeafDeps Copy() {
      LeafDeps d = new LeafDeps();
      d.leafData = leafData;
      d.baseParams = baseParams.Copy();
      d.inspector = inspector.Copy();
      d.logOptions = logOptions;
      return d;
    }
  }

  [Serializable]
  public struct LeafData {
    public int randomSeed;
    public Weak<MeshFilter> leafMeshFilter;
    public Weak<MeshFilter> stemMeshFilter;

    public MeshFilter GetLeaf() => leafMeshFilter.Check() ? (MeshFilter)leafMeshFilter.obj : null;
    public MeshFilter GetStem() => stemMeshFilter.Check() ? (MeshFilter)stemMeshFilter.obj : null;
    public Transform GetTransform() => (GetLeaf() is MeshFilter mf) ? mf.GetComponent<Transform>() : null;
  }

}
