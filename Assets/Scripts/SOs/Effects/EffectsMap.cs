using UnityEngine;

namespace BionicWombat {
  public enum EffectName {
    LeafyCircleHighlight = 0,
    SmokePuff = 1,
    LeafPoof = 2,
    SmokePuffRect = 3,
  }

#pragma warning disable 0649
  public class EffectsMap : ScriptableObject {
    [SerializeField] EffectData LeafyCircleHighlight;
    [SerializeField] EffectData SmokePuff;
    [SerializeField] EffectData LeafPoof;
    [SerializeField] EffectData SmokePuffRect;

    public EffectData GetEffect(EffectName n) {
      switch (n) {
        case EffectName.LeafyCircleHighlight: return LeafyCircleHighlight;
        case EffectName.SmokePuff: return SmokePuff;
        case EffectName.LeafPoof: return LeafPoof;
        case EffectName.SmokePuffRect: return SmokePuffRect;
      }
      Debug.LogError("EffectMap.GetEffect(): Unknown EffectName " + n);
      return default(EffectData);
    }
  }
#pragma warning restore 0649

}
