using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BionicWombat {
  [Serializable]
  public class BaseParams {
    private static string delim = "|";
    private static string kvDelim = ":";
    public float BaseHeight = -3f;
    public float BaseWidth = 1f;
    public int RenderLineSteps = 16;
    public int SubdivSteps = 0;
    public int VeinLineSteps = 10;
    public int CustomMeshDensity = 16;
    public bool TriangulateWithInnerVerts = false;
    public int TextureSize = 1024;
    public int TextureDownsample = 1;
    public int NormalSupersample = 1;
    public int DistortionInstances = 3;
    public float LinearPointsIncr = 0.5f;
    public bool HideTrunk = false;
    public bool HideDistortion = false;
    public bool SkipPhysics = false;
    public string RandomBS = "3";

    public override string ToString() {
      string s = "";
      FieldInfo[] fields = typeof(BaseParams).GetFields();
      foreach (FieldInfo field in fields) {
        s += field.Name + kvDelim + field.GetValue(this) + delim;
      }
      return s;
    }

    public BaseParams Copy() {
      BaseParams b = new BaseParams();
      FieldInfo[] fields = typeof(BaseParams).GetFields();
      foreach (FieldInfo field in fields)
        field.SetValue(b, field.GetValue(this));
      return b;
    }

    public Dictionary<LPType, bool> FigureOutWhatsDirty(string last) {
      if (last == null || last.Length == 0) return AllDirty;
      Dictionary<string, string> cur = ParamsStringToDict(ToString());
      Dictionary<string, string> prev = ParamsStringToDict(last);
      Dictionary<LPType, bool> dirty = AllClean;
      // Debug.Log(cur.ToLogShort()); Debug.Log(prev.ToLogShort());
      foreach (string param in cur.Keys) {
        if (!prev.ContainsKey(param)) return AllDirty;
        if (cur[param] != prev[param]) {
          switch (param) {
            case "BaseHeight":
            case "BaseWidth":
            case "LinearPointsIncr":
            case "TriangulateWithInnerVerts":
              dirty[LPType.Leaf] = true; //leaf == Render All
              break;
            case "VeinLineSteps":
              dirty[LPType.Vein] = true;
              break;
            case "TextureSize":
            case "TextureDownsample":
              dirty[LPType.Texture] = true;
              break;
            case "NormalSupersample":
              dirty[LPType.Normal] = true;
              break;
            case "HideTrunk":
            case "HideDistortion":
              dirty[LPType.Distort] = true;
              break;
            case "RenderLineSteps":
            case "SubdivSteps":
            case "RandomBS":
            case "RandomSeed":
            case "SkipPhysics":
              return AllDirty;
            default:
              Debug.LogError("FigureOutWhatsDirty param not checked: " + param);
              return AllDirty;
          }
        }
      }
      return dirty;
    }

    private static Dictionary<string, string> ParamsStringToDict(string paramsString) {
      string[] arr = paramsString.Split(delim);
      Dictionary<string, string> dict = new Dictionary<string, string>();
      foreach (string s in arr) {
        string[] spl = s.Split(kvDelim);
        if (spl.Length == 2)
          dict[spl[0]] = spl[1];
      }
      return dict;
    }

    private static Dictionary<LPType, bool> _InitDirtyDict(bool dirty) => new Dictionary<LPType, bool>() {
    { LPType.Leaf, dirty },
    { LPType.Vein, dirty },
    { LPType.Texture, dirty },
    { LPType.Normal, dirty },
    { LPType.Material, dirty },
    { LPType.Distort, dirty },
    { LPType.Stem, dirty },
    { LPType.Arrangement, dirty },
  };

    public static Dictionary<LPType, bool> AllClean = _InitDirtyDict(false);
    public static Dictionary<LPType, bool> AllDirty = _InitDirtyDict(true);

    public int InstanceIdxFromRandomBS() {
      if (RandomBS.Length != 2) return 0;
#if UNITY_EDITOR
      if (RandomBS[0] == "i"[0] && RandomBS.Length >= 2) return int.Parse(RandomBS[1].ToString());
      return 0;
#else
    return 0;
#endif
    }

  }

}
