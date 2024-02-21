using System;
using UnityEngine;
using static BionicWombat.VectorExtensions;

namespace BionicWombat {
  [Serializable]
  // file:///C:/Users/adrem/Plants_Game/Assets/Scripts/Tools/MakeStruct.html?a=EffectData GameObject effect float scale Vector3 rotation
  public struct EffectData {
    public GameObject effect;
    public float scale;
    public Vector3 rotation;

    public EffectData(GameObject effect, float scale, Vector3 rotation) {
      this.effect = effect;
      this.scale = scale;
      this.rotation = rotation;
    }

    public override string ToString() {
      return "[EffectData] effect: " + effect + " | scale: " + scale + " | rotation: " + rotation;
    }
  }

  public class EffectsFactory : UnityEngine.Object {
    private static EffectsMap _map;
    private static EffectsMap map {
      get {
        _map = _map ?? Resources.Load<EffectsMap>("SOs/Data/EffectsMap_Map");
        return _map;
      }
    }

    public static GameObject CreateEffect(EffectName name, Vector3 pos, Transform parent, float scale = 1.0f) {
      return CreateEffect(map.GetEffect(name), pos, parent, scale);
    }

    public static GameObject CreateEffect(EffectData effect, Vector3 pos, Transform parent, float scale = 1.0f) {
      if (effect.IsDefault()) return null;
      GameObject g = Instantiate(effect.effect, Vector3.zero, Quaternion.identity, parent);
      // DebugBW.Log("effect.scale: " + effect.scale);
      g.transform.localScale = Vector3With(scale * effect.scale);
      g.transform.localPosition = pos;
      g.transform.localRotation = Quaternion.Euler(effect.rotation);
      g.tag = Tags.Effect;
      foreach (Transform t in g.transform)
        t.tag = Tags.Effect;

      return g;
    }
  }

}
