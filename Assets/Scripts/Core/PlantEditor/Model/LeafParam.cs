using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using static BionicWombat.ColorExtensions;

namespace BionicWombat {
  [Serializable]
  public enum LPK {
    Pudge,
    Sheer,
    Length,
    Width,
    TipAngle,
    TipAmplitude,
    Heart,
    SinusSheer,
    SinusHeight,
    WaistAmp,
    WaistAmpOffset,
    Lobes,
    LobeTilt,
    LobeAmplitude,
    LobeAmpOffset,
    ScoopDepth,
    ScoopHeight,

    //veins
    //gen 0
    VeinDensity,
    VeinBunching,
    VeinLobeBunching,
    VeinOriginRand,
    GravVeinUpperBias,
    GravVeinLowerBias,
    VeinEndOffset,
    VeinEndLerp,
    VeinDistFromMargin,
    MidribDistFromMargin,
    SpannerLerp,
    SpannerSqueeze,
    MidribThickness,
    SecondaryThickness,
    SpannerThickness,
    MidribTaper,
    SecondaryTaper,
    SpannerTaper,
    TaperRNG,

    //gen 1
    VeinSplit,
    VeinSplitDepth,
    VeinSplitAmp,
    VeinSplitAmpOffset,

    //texture
    //gen 0
    TexBaseColor,
    TexShadowStrength,
    TexVeinColor,
    TexVeinOpacity,
    TexVeinSecondaryOpacity,
    TexVeinDepth,
    TexVeinBlur,
    TexRadianceHue,
    TexRadianceLitPower,
    TexRadianceInversion,
    TexRadiance,
    TexRadianceMargin,
    TexRadianceDensity,
    TexRadianceWidthMult,
    TexMarginColor,
    TexMarginProminance,

    //normals
    NormalMidribWidth,
    NormalMidribDepth,
    NormalSecondaryWidth,
    NormalSecondaryDepth,
    NormalVeinSmooth,
    NormalPuffyPlateauClamp,
    NormalPuffySmooth,
    NormalPuffyStrength,

    //material
    MaterialShininess,
    MaterialMetallicness,
    MaterialAOStrength,
    MaterialRimPower,
    MaterialMicrotexAmp,
    MaterialRimColor,
    MaterialVertBumps,
    MaterialHeightAmp,
    TrunkBrowning,
    TrunkLightness,

    //distortion
    DistortionEnabled,
    DistortCurl,
    DistortCurlPoint,
    DistortCup,
    DistortCupClamp,
    DistortFlop,
    DistortFlopStart,
    DistortWave,
    DistortWavePeriod,
    DistortWaveDepth,
    ExtrudeEnabled,
    ExtrudeEdgeDepth,
    ExtrudeSuccThicc,

    //stem
    StemLength,
    StemWidth,
    StemFlop,
    StemNeck,
    StemAttachmentAngle,

    //arrangement
    LeafCount,
    LeafScale,
    ScaleMin,
    ScaleRand,
    RotationalSymmetry,
    RotationClustering,
    RotationRand,
    NodeDistance,
    NodeInitialY,
    StemLengthIncrease,
    StemLengthRand,
    StemFlopLower,
    StemFlopRand,
    TrunkWidth,
    TrunkLean,
    TrunkWobble,
    PotScale,

    None,
  }

  public enum LPType {
    Leaf,
    Vein,
    Texture,
    Normal,
    Material,
    Distort,
    Stem,
    Arrangement
  }

  public enum LPMode {
    Unknown,
    Float,
    Toggle,
    ColorValueDEPRECATED,
    ColorHSL
  }

  [Serializable]
  public class LeafParam {
    public static float precision = 100f;

    //true params

    [XmlIgnore] public LPK key;
    [XmlElement("key")]
    public string XmlKey {
      get => key.ToString();
      set {
        try { key = (LPK)Enum.Parse(typeof(LPK), value); } catch {
          Debug.Log("Invalid key found: " + value);
          key = LPK.None;
        }
      }
    }

    public bool enabled = true;
    public LPMode mode = LPMode.Unknown;
    public FloatRange range;
    public HSLRange hslRange;
    [SerializeField] private float _value;
    [SerializeField] public HSL hslValue;

    [XmlIgnore] public string group;
    [XmlIgnore] public LPType type = LPType.Leaf;

    public LeafParam Copy() {
      LeafParam p = new LeafParam(this.key, this.group, this.type, this.enabled);
      p.mode = mode;
      p.range = range;
      p.hslRange = hslRange;
      p.value = value;
      p.hslValue = hslValue;
      return p;
    }

    public override string ToString() {
      return "LeafParam: " + key +
             " | Enabled: " + enabled +
             " | mode: " + mode +
              (range != null ? (" | range: " + range) : "") +
              (hslRange != null ? (" | hslRange: " + hslRange) : "") +
             " | _value: " + _value +
             " | hslValue: " + hslValue;
    }

    //props
    [XmlIgnore]
    public string name { get => key.ToString("F"); }

    public float value {
      get { return _value; }
      set { _value = value; }
    }

    [XmlIgnore]
    public float Hue {
      get => hslValue.hue;
      set => hslValue.hue = _value;
    }
    [XmlIgnore]
    public float Saturation {
      get => hslValue.saturation;
      set => hslValue.saturation = _value;
    }
    [XmlIgnore]
    public float Lightness { //AKA "Value"
      get => hslValue.lightness;
      set => hslValue.lightness = _value;
    }

    public Color colorValue {
      get {
        if (mode == LPMode.ColorHSL) {
          return hslValue.ToColor();
        }

        Debug.LogError("Attempting to get param color value on non-color field: " + mode + " | " + key);
        return Color.black;
      }
    }

    public bool hasColorValue => mode == LPMode.ColorHSL;

    public bool triggersFullRedraw => TriggerRedraw(key);

    public LeafParam() { }

    public LeafParam(LPK key,
                     string group,
                     LPType type,
                     bool shouldEnable = true) {
      this.key = key;
      this.group = group;
      this.type = type;
      this.enabled = shouldEnable;
    }

    public static LeafParam LeafParamToggle(LPK key,
              bool enabled,
              string group,
              LPType type) {
      LeafParam p = new LeafParam(key, group, type, enabled);
      p.mode = LPMode.Toggle;
      p.enabled = enabled;
      return p;
    }

    public static LeafParam LeafParamFloat(LPK key,
              FloatRange range,
              string group,
              LPType type,
              bool shouldEnable = true) {
      LeafParam p = new LeafParam(key, group, type, shouldEnable);
      p.mode = LPMode.Float;
      p.range = range;
      p.value = range.Default;
      return p;
    }

    public static LeafParam LeafParamHSLColor(LPK key,
              HSLRange range,
              string group,
              LPType type,
              bool shouldEnable = true) {
      LeafParam p = new LeafParam(key, group, type, shouldEnable);
      p.mode = LPMode.ColorHSL;
      p.hslRange = range;
      p.hslValue = range.defValue;
      return p;
    }

    private static bool TriggerRedraw(LPK key) {
      switch (key) {
        case LPK.ExtrudeEnabled:
        case LPK.DistortionEnabled:
          return true;
      }
      return false;
    }
  }
}