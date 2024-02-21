using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

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
    TexMaskingStrength,
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
    TexMarginAlpha,

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
    AbaxialDarkening,
    AbaxialPurpleTint,
    AbaxialHue,
    VertBumpsPower,
    VertBumpsScale,
    VertBumpsStretch,
    VertBumpsPower2,
    VertBumpsScale2,
    VertBumpsStretch2,
    RadialBumpsPower,
    RadialBumpsScale,
    RadialBumpsLenScale,
    RadialBumpsWidth,
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
    DistortWaveAmp,
    DistortWavePeriod,
    DistortWaveDepth,
    DistortWaveDivergance,
    DistortWaveDivergancePeriod,
    ExtrudeEnabled,
    ExtrudeEdgeDepth,
    ExtrudeSuccThicc,

    //stem
    StemLength,
    StemWidth,
    StemFlop,
    StemNeck,
    StemAttachmentAngle,
    StemBaseColor,
    StemTopColorHue,
    StemTopColorLit,
    StemTopColorSat,
    StemColorBias,
    StemShine,
    StemBaseTexType, //-1.5 - 1.5: lines / blank / dots
    StemTopTexType,

    //arrangement
    LeafCount,
    LeafScale,
    ScaleMin,
    ScaleRand,
    LeafSkewMax,
    PhysicsAmplification,
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
  }

  //internal
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

  //external
  [Serializable]
  public enum LPCategory {
    LeafShape,
    Veins,
    Color,
    Texture,
    Distortion,
    Arrangement,
    //fenestrations
    //edge detail
    //branching growth
    //variagation
    //asymmetry
  }

  public enum LPMode {
    Unknown,
    Float,
    ToggleDEPRECATED,
    ColorValueDEPRECATED,
    ColorHSL
  }

  public enum LPRandomValCurve {
    Flat,
    CenterBell,
    ReverseBell,
    CenterBellLRSplit,
    DefaultValueOnly,
  }

  public enum LPRandomValCenterBias {
    None,
    Squeeze6, //tighten the most
    Squeeze5,
    Squeeze4,
    Squeeze3,
    Squeeze2,
    Squeeze1, //tighten a little
    Default,
    Spread1, //widen a little
    Spread2,
    Spread3,  //widen the most
  }

  public enum LPImportance {
    Disable,
    Low,
    Medium,
    High,
  }

  [Serializable]
  public class LeafParam {
    public static float precision = 100f;

    //true params

    [XmlIgnore] public LPK key;
    [XmlIgnore] public bool isInvalid = false;
    [XmlElement("key")]
    public string XmlKey {
      get => key.ToString();
      set {
        try { key = (LPK)Enum.Parse(typeof(LPK), value); } catch {
          Debug.Log("Invalid key found: " + value);
          key = 0;
          isInvalid = true;
        }
      }
    }

    [SerializeField] public bool enabled = true;
    [SerializeField] private float _value;
    [SerializeField] public HSL hslValue;

    [XmlIgnore] public LPMode mode = LPMode.Unknown;
    [XmlIgnore] public FloatRange range;
    [XmlIgnore] public HSLRange hslRange;
    [XmlIgnore] public string inspectorSubgroup;
    [XmlIgnore] public LPType type = LPType.Leaf;
    [XmlIgnore] public LPCategory category;
    [XmlIgnore] public LPImportance importance;
    [XmlIgnore] public TechTreeTech boundTech;
    [XmlIgnore] public Dictionary<TechTreeTech, PlantRandomizer.RandomizerStrength> softTechs;
    [XmlIgnore] public LPRandomValCurve randomValCurve;
    [XmlIgnore] public LPRandomValCenterBias[] randomValCenterBiases;
    public LPRandomValCenterBias randomValCenterBias => randomValCenterBiases[0];

    public LeafParam Copy() {
      LeafParam p = new LeafParam(this.key, this.inspectorSubgroup, this.type, this.category, this.randomValCurve, this.randomValCenterBiases, this.importance, this.boundTech, this.enabled);
      p.mode = mode;
      p.range = range;
      p.hslRange = hslRange;
      p.value = value;
      p.hslValue = hslValue;
      p.importance = importance;
      p.boundTech = boundTech;
      p.softTechs = softTechs;
      p.randomValCurve = randomValCurve;
      p.randomValCenterBiases = randomValCenterBiases;
      return p;
    }

    public override string ToString() {
      return "LeafParam: " + key +
        " | Enabled: " + enabled +
        " | mode: " + mode +
        (mode == LPMode.Float ? (" | range: " + range + " | value: " + value) : "") +
        (mode == LPMode.ColorHSL ? (" | hslRange: " + hslRange + " | hslValue: " + hslValue) : "");
    }

    public string TooltipString() {
      return "LPCategory: " + category.ToString() +
             "\nTechTreeTech: " + boundTech +
             "\nLPImportance: " + importance +
             "\nBias: " + (randomValCenterBiases.ToLog());
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

    public float valuePercent {
      get {
        switch (mode) {
          case LPMode.ToggleDEPRECATED: return enabled ? 1f : 0f;
          case LPMode.Float: return (value - range.Start) / (range.End - range.Start);
        }
        return float.MinValue;
      }
    }

    public static float DistanceFromDefault(LeafParam param, LeafParam def) {
      if (!param.enabled) return -1f;
      float dist = 0f;
      if (def.mode == LPMode.Float) {
        dist = DistanceFromDefaultFloat(param, def);
      } else if (def.mode == LPMode.ColorHSL) {
        dist = (
          DistanceFromDefaultHSL(param, def, ColorChannel.H) +
          DistanceFromDefaultHSL(param, def, ColorChannel.S) +
          DistanceFromDefaultHSL(param, def, ColorChannel.L)) / 3f;
      } else {
        return -1f;
      }

      return dist;
    }

    //how far we are from default, returns 0-1
    //for HSL, call DistanceFromDefaultHSL
    public static float DistanceFromDefaultFloat(LeafParam param, LeafParam def) {
      if (def.mode == LPMode.ToggleDEPRECATED) return param.enabled ? 0f : 1f;
      if (def.mode == LPMode.Float) {
        float dist = DistanceFromExtent(def.range, param.value);
        return dist;
        // return dist * (value < range.Default ? -1f : 1f);
      }
      return float.MinValue;
    }

    public static float DistanceFromDefaultHSL(LeafParam param, LeafParam def, ColorChannel hslChannel) {
      if (def.mode == LPMode.ColorHSL) {
        if (hslChannel == ColorChannel.S) return DistanceFromExtent(def.hslRange.satRange, param.hslValue.saturation);
        if (hslChannel == ColorChannel.L) return DistanceFromExtent(def.hslRange.valRange, param.hslValue.lightness);
        if (hslChannel == ColorChannel.H) {
          float dist = Mathf.Abs(param.value - def.hslRange.hueRange.Default);
          if (dist > 0.5f) dist = 1f - dist;
          return dist / 0.5f;
        }
      }
      return float.MinValue;
    }

    private static float DistanceFromExtent(FloatRange range, float value) =>
      Mathf.Abs(value - range.Default) /
      Mathf.Max(range.End - range.Default, range.Default - range.Start);

    public (float perc, float weight) RarityScore() {
      LeafParam def = LeafParamDefaults.Defaults[key];
      float score = DistanceFromDefault(this, def);
      float importanceMult = ImportanceMult(def.importance);
      float biasMult = def.randomValCenterBiases.ToList()
        .Average(cb => PlantRandomizer.ValForCenterBias(cb));
      return (score, importanceMult * biasMult);
    }

    private static float ImportanceMult(LPImportance importance) {
      switch (importance) {
        case LPImportance.Disable: return 0f;
        case LPImportance.Low: return 1f;
        case LPImportance.Medium: return 3f;
        case LPImportance.High: return 10f;
      }
      Debug.LogError("ImportanceMult unsupported case: " + importance);
      return 0f;
    }

    public bool triggersFullRedraw => TriggerRedraw(key);

    private LeafParam() { }

    private LeafParam(LPK key,
                     string group,
                     LPType type,
                     LPCategory category,
                     LPRandomValCurve randomValCurve,
                     LPRandomValCenterBias[] randomValCenterBiases,
                     LPImportance importance,
                     TechTreeTech boundTech,
                     bool shouldEnable = true) {
      this.key = key;
      this.inspectorSubgroup = group;
      this.type = type;
      this.category = category;
      this.randomValCurve = randomValCurve;
      this.randomValCenterBiases = randomValCenterBiases;
      this.importance = importance;
      this.boundTech = boundTech;
      this.enabled = shouldEnable;
    }

    public static LeafParam LeafParamToggle(LPK key,
              bool enabled,
              string group,
              LPType type,
              LPCategory category) {
      LeafParam p = new LeafParam(key, group, type, category, LPRandomValCurve.Flat,
        ARR(LPRandomValCenterBias.Default), LPImportance.Disable, TechTreeTech.Null, enabled);
      p.mode = LPMode.ToggleDEPRECATED;
      p.enabled = enabled;
      return p;
    }

    public static LeafParam LeafParamToggleRange(LPK key,
              float overflowRange,
              float softenCenterRange,
              float defaultVal,
              string group,
              LPType type,
              LPCategory category,
              LPRandomValCurve randomValCurve,
              LPRandomValCenterBias randomValCenterBias,
              LPImportance importance,
              TechTreeTech boundTech,
              bool shouldEnable = true) {
      LeafParam p = new LeafParam(key, group, type, category, randomValCurve,
        ARR(randomValCenterBias), importance, boundTech, shouldEnable);
      p.mode = LPMode.Float;
      p.range = new FloatRange(-overflowRange, softenCenterRange + overflowRange, defaultVal);
      p.value = defaultVal;
      return p;
    }

    public static LeafParam LeafParamFloat(LPK key,
              FloatRange range,
              string group,
              LPType type,
              LPCategory category,
              LPRandomValCurve randomValCurve,
              LPRandomValCenterBias randomValCenterBias,
              LPImportance importance,
              TechTreeTech boundTech,
              bool shouldEnable = true) {
      LeafParam p = new LeafParam(key, group, type, category, randomValCurve, ARR(randomValCenterBias), importance, boundTech, shouldEnable);
      p.mode = LPMode.Float;
      p.range = range;
      p.value = range.Default;
      return p;
    }

    public static LeafParam LeafParamHSLColor(LPK key,
              HSLRange range,
              string group,
              LPType type,
              LPCategory category,
              LPRandomValCenterBias[] randomValCenterBiases,
              LPImportance importance,
              TechTreeTech boundTech,
              Dictionary<TechTreeTech, PlantRandomizer.RandomizerStrength> softTechs = null,
              bool shouldEnable = true) {
      LeafParam p = new LeafParam(key, group, type, category, LPRandomValCurve.CenterBell, randomValCenterBiases, importance, boundTech, shouldEnable);
      p.mode = LPMode.ColorHSL;
      p.softTechs = softTechs;
      p.hslRange = range;
      p.hslValue = range.defaultValues;
      return p;
    }

    public static LPRandomValCenterBias[] ARR(LPRandomValCenterBias b) =>
      new LPRandomValCenterBias[] { b };

    private static bool TriggerRedraw(LPK key) {
      switch (key) {
        case LPK.ExtrudeEnabled:
        case LPK.DistortionEnabled:
        case LPK.LeafCount:
          return true;
      }
      return false;
    }
  }
}
