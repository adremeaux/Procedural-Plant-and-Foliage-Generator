using System.Collections.Generic;
using static BionicWombat.LeafParam;
using static BionicWombat.PlantRandomizer;

namespace BionicWombat {
  public static class LeafParamDefaults {

    private static float baseWidth = 1f;
    private static float baseHeight = 3f;

    public static LeafParamDict Defaults =
      new LeafParamDict {
      //gen 0
        { LPK.Pudge, LeafParamFloat(LPK.Pudge,
            new FloatRange(0, baseHeight, 1),
            "Gen 0", LPType.Leaf, LPCategory.LeafShape,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Disable,
            TechTreeTech.ShapeAdv, false)
        },
        { LPK.Sheer, LeafParamFloat(LPK.Sheer,
            new FloatRange(0, 1, 0.4f),
            "Gen 0", LPType.Leaf, LPCategory.LeafShape,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Medium,
            TechTreeTech.Shape1)
        },
        { LPK.Length, LeafParamFloat(LPK.Length,
            new FloatRange(1, baseHeight * 6f, 6f),
            "Gen 0", LPType.Leaf, LPCategory.LeafShape,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.High,
            TechTreeTech.Shape1)
        },
        { LPK.Width, LeafParamFloat(LPK.Width,
            new FloatRange(0.2f, baseWidth * 6f, 2.5f),
            "Gen 0", LPType.Leaf, LPCategory.LeafShape,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.High,
            TechTreeTech.Shape1)
        },
        { LPK.TipAngle, LeafParamFloat(LPK.TipAngle,
            new FloatRange(0, 90, 45),
            "Gen 0", LPType.Leaf, LPCategory.LeafShape,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.Shape1)
        },
        { LPK.TipAmplitude, LeafParamFloat(LPK.TipAmplitude,
            new FloatRange(0, 3, 1.5f),
            "Gen 0", LPType.Leaf, LPCategory.LeafShape,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.Shape1)
        },

        //gen 1
        { LPK.Heart, LeafParamToggleRange(LPK.Heart, 1f, 0f, -0.1f,
            "Gen 1", LPType.Leaf, LPCategory.LeafShape, LPRandomValCurve.CenterBell, LPRandomValCenterBias.Spread3, LPImportance.Medium,
            TechTreeTech.ShapeAdv)
        },
        { LPK.SinusSheer, LeafParamFloat(LPK.SinusSheer,
            new FloatRange(-0.5f, 4, 0.7f),
            "Gen 1", LPType.Leaf, LPCategory.LeafShape,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Medium,
            TechTreeTech.ShapeAdv)
        },
        { LPK.SinusHeight, LeafParamFloat(LPK.SinusHeight,
            new FloatRange(0, 4, 1.75f),
            "Gen 1", LPType.Leaf, LPCategory.LeafShape,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Medium,
            TechTreeTech.ShapeAdv)
        },
        { LPK.WaistAmp, LeafParamFloat(LPK.WaistAmp,
            new FloatRange(0.1f, 2, 1),
            "Gen 1", LPType.Leaf, LPCategory.LeafShape,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.ShapeAdv)
        },
        { LPK.WaistAmpOffset, LeafParamFloat(LPK.WaistAmpOffset,
            new FloatRange(-0.5f, 0.5f, 0),
            "Gen 1", LPType.Leaf, LPCategory.LeafShape,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.ShapeAdv)
        },

        //gen 2
        { LPK.Lobes, LeafParamToggleRange(LPK.Lobes, 1f, 0f, 0.5f,
            "Gen 2", LPType.Leaf, LPCategory.LeafShape,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Spread3, LPImportance.Low,
            TechTreeTech.ShapeAdv)
        },
        { LPK.LobeTilt, LeafParamFloat(LPK.LobeTilt,
            new FloatRange(-45, 90, 0),
            "Gen 2", LPType.Leaf, LPCategory.LeafShape,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.ShapeAdv)
        },
        { LPK.LobeAmplitude, LeafParamFloat(LPK.LobeAmplitude,
            new FloatRange(0, 1.5f, .75f),
            "Gen 2", LPType.Leaf, LPCategory.LeafShape,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.ShapeAdv)
        },
        { LPK.LobeAmpOffset, LeafParamFloat(LPK.LobeAmpOffset,
            new FloatRange(-0.5f, 0.5f, 0),
            "Gen 2", LPType.Leaf, LPCategory.LeafShape,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.ShapeAdv)
        },

        //gen 3
        { LPK.ScoopDepth, LeafParamFloat(LPK.ScoopDepth,
            new FloatRange(0f, 0.9f, 0.1f),
            "Gen 3", LPType.Leaf, LPCategory.LeafShape,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.ShapeAdv)
        },
        { LPK.ScoopHeight, LeafParamFloat(LPK.ScoopHeight,
            new FloatRange(0f, 1f, 0.1f),
            "Gen 3", LPType.Leaf, LPCategory.LeafShape,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.ShapeAdv)
        },

        //VEINS
        { LPK.VeinDensity, LeafParamFloat(LPK.VeinDensity,
            new FloatRange(0.1f, 1.5f, 0.5f),
            "Gen 0", LPType.Vein, LPCategory.Veins,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.High,
            TechTreeTech.Veins1)
        },
        { LPK.VeinBunching, LeafParamFloat(LPK.VeinBunching,
            new FloatRange(1, 3, 2f),
            "Gen 0", LPType.Vein, LPCategory.Veins,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Spread1, LPImportance.Medium,
            TechTreeTech.Veins1)
        },
        { LPK.VeinLobeBunching, LeafParamFloat(LPK.VeinLobeBunching,
            new FloatRange(1, 5, 3f),
            "Gen 0", LPType.Vein, LPCategory.Veins,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.Veins1)
        },
        { LPK.VeinOriginRand, LeafParamFloat(LPK.VeinOriginRand,
            new FloatRange(0f, 1f, 0.5f),
            "Gen 0", LPType.Vein, LPCategory.Veins,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.Veins1)
        },
        { LPK.GravVeinUpperBias, LeafParamFloat(LPK.GravVeinUpperBias,
            new FloatRange(0, 0.75f, 0.5f),
            "Gen 0", LPType.Vein, LPCategory.Veins,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.Veins1)
        },
        { LPK.GravVeinLowerBias, LeafParamFloat(LPK.GravVeinLowerBias,
            new FloatRange(0, 1, 0.5f),
            "Gen 0", LPType.Vein, LPCategory.Veins,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.Veins1)
        },
        { LPK.VeinEndOffset, LeafParamFloat(LPK.VeinEndOffset,
            new FloatRange(0, 2, 1f),
            "Gen 0", LPType.Vein, LPCategory.Veins,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.Veins1)
        },
        { LPK.VeinEndLerp, LeafParamFloat(LPK.VeinEndLerp,
            new FloatRange(0, 1, 0.5f),
            "Gen 0", LPType.Vein, LPCategory.Veins,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.Veins1)
        },
        { LPK.VeinDistFromMargin, LeafParamFloat(LPK.VeinDistFromMargin,
            new FloatRange(0, 1, 0.1f),
            "Gen 0", LPType.Vein, LPCategory.Veins,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Spread1, LPImportance.Medium,
            TechTreeTech.VeinsAdv)
        },
        { LPK.MidribDistFromMargin, LeafParamFloat(LPK.MidribDistFromMargin,
            new FloatRange(0, 2, 0.5f),
            "Gen 0", LPType.Vein, LPCategory.Veins,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.Veins1)
        },
        { LPK.SpannerLerp, LeafParamFloat(LPK.SpannerLerp,
            new FloatRange(-0.5f, 1, 0.2f),
            "Gen 0", LPType.Vein, LPCategory.Veins,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.VeinsAdv)
        },
        { LPK.SpannerSqueeze, LeafParamFloat(LPK.SpannerSqueeze,
            new FloatRange(0, 0.5f, 0.16f),
            "Gen 0", LPType.Vein, LPCategory.Veins,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.VeinsAdv)
        },


        { LPK.MidribThickness, LeafParamFloat(LPK.MidribThickness,
            new FloatRange(0, 0.15f, 0.06f),
            "Thickness", LPType.Vein, LPCategory.Veins,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Medium,
            TechTreeTech.Veins1)
        },
        { LPK.SecondaryThickness, LeafParamFloat(LPK.SecondaryThickness,
            new FloatRange(0, 0.12f, 0.04f),
            "Thickness", LPType.Vein, LPCategory.Veins,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Medium,
            TechTreeTech.Veins1)
        },
        { LPK.SpannerThickness, LeafParamFloat(LPK.SpannerThickness,
            new FloatRange(0, 1f, 0.0f),
            "Thickness", LPType.Vein, LPCategory.Veins,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Spread2, LPImportance.Medium,
            TechTreeTech.VeinsAdv)
        },
        { LPK.MidribTaper, LeafParamFloat(LPK.MidribTaper,
            new FloatRange(0, 4, 1f),
            "Thickness", LPType.Vein, LPCategory.Veins,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Squeeze1, LPImportance.Medium,
            TechTreeTech.Veins1)
        },
        { LPK.SecondaryTaper, LeafParamFloat(LPK.SecondaryTaper,
            new FloatRange(0, 4, 0.5f),
            "Thickness", LPType.Vein, LPCategory.Veins,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Squeeze1, LPImportance.Medium,
            TechTreeTech.Veins1)
        },
        { LPK.SpannerTaper, LeafParamFloat(LPK.SpannerTaper,
            new FloatRange(0, 2, 1f),
            "Thickness", LPType.Vein, LPCategory.Veins,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Squeeze1, LPImportance.Low,
            TechTreeTech.VeinsAdv)
        },
        { LPK.TaperRNG, LeafParamFloat(LPK.TaperRNG,
            new FloatRange(0, 2, 0.5f),
            "Thickness", LPType.Vein, LPCategory.Veins,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.VeinsAdv)
        },

        { LPK.VeinSplit, LeafParamToggleRange(LPK.VeinSplit, 1f, 0f, -0.3f,
            "Split", LPType.Vein, LPCategory.Veins,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Spread3, LPImportance.Medium,
            TechTreeTech.VeinsAdv)
        },
        { LPK.VeinSplitDepth, LeafParamFloat(LPK.VeinSplitDepth,
            new FloatRange(0.1f, 0.9f, 0.4f),
            "Split", LPType.Vein, LPCategory.Veins,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.VeinsAdv)
        },
        { LPK.VeinSplitAmp, LeafParamFloat(LPK.VeinSplitAmp,
            new FloatRange(0.1f, 0.9f, 0.5f),
            "Split", LPType.Vein, LPCategory.Veins,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.VeinsAdv)
        },
        { LPK.VeinSplitAmpOffset, LeafParamFloat(LPK.VeinSplitAmpOffset,
            new FloatRange(0, 1, 0.5f),
            "Split", LPType.Vein, LPCategory.Veins,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.VeinsAdv, false)
        },    

        //Texture
        { LPK.TexBaseColor, LeafParamHSLColor(LPK.TexBaseColor,
            new HSLRange(new FloatRange(0, 359f / 360f, 0.33f),
                         new FloatRange(.05f, .95f, .80f),
                         new FloatRange(.05f, .95f, .15f)),
            "Base", LPType.Texture, LPCategory.Color,
            new LPRandomValCenterBias[] {
                LPRandomValCenterBias.Squeeze1,
                LPRandomValCenterBias.Spread1,
                LPRandomValCenterBias.Squeeze2,
            },
            LPImportance.High,
            TechTreeTech.Color1,
            new Dictionary<TechTreeTech, RandomizerStrength>() {
                {TechTreeTech.Color1, RandomizerStrength.Medium},
                {TechTreeTech.Color2, RandomizerStrength.MediumHigh},
                {TechTreeTech.ColorAdv, RandomizerStrength.High},
            })
        },
        { LPK.TexShadowStrength, LeafParamFloat(LPK.TexShadowStrength,
            new FloatRange(0, 1, 0.75f),
            "Base", LPType.Texture, LPCategory.Color,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.Color1)
        },
        { LPK.TexMaskingStrength, LeafParamFloat(LPK.TexMaskingStrength,
            new FloatRange(0f, 2f, 1.1f), "Base", LPType.Texture, LPCategory.Texture,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.Texture1)
        },
        { LPK.TexVeinColor, LeafParamHSLColor(LPK.TexVeinColor,
            new HSLRange(new FloatRange(0, 359f / 360f, 0.33f),
                         new FloatRange(.20f, .95f, .80f),
                         new FloatRange(.05f, .99f, .2f)),
            "Veins", LPType.Texture, LPCategory.Color,
            new LPRandomValCenterBias[] {
                LPRandomValCenterBias.Default,
                LPRandomValCenterBias.Spread1,
                LPRandomValCenterBias.Spread1,
            },
            LPImportance.High,
            TechTreeTech.Color1,
            new Dictionary<TechTreeTech, RandomizerStrength>() {
                {TechTreeTech.Color1, RandomizerStrength.MediumHigh},
                {TechTreeTech.Color2, RandomizerStrength.High},
            })
        },
        { LPK.TexVeinOpacity, LeafParamFloat(LPK.TexVeinOpacity,
            new FloatRange(0, 1, 0.8f),
            "Veins", LPType.Texture, LPCategory.Color,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Medium,
            TechTreeTech.Color1)
        },
        { LPK.TexVeinSecondaryOpacity, LeafParamFloat(LPK.TexVeinSecondaryOpacity,
            new FloatRange(0, 1, 0.8f),
            "Veins", LPType.Texture, LPCategory.Color,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Medium,
            TechTreeTech.Color1)
        },
        { LPK.TexVeinDepth, LeafParamFloat(LPK.TexVeinDepth,
            new FloatRange(0, 1, 0.5f),
            "Veins", LPType.Texture, LPCategory.Veins,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Spread2, LPImportance.Low,
            TechTreeTech.Veins1)
        },
        { LPK.TexVeinBlur, LeafParamFloat(LPK.TexVeinBlur,
            new FloatRange(0, 1, 0.5f),
            "Veins", LPType.Texture, LPCategory.Veins,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Disable,
            TechTreeTech.Veins1)
        },

        { LPK.TexRadianceHue, LeafParamFloat(LPK.TexRadianceHue,
            new FloatRange(-1, 1, 0),
            "Radiance", LPType.Texture, LPCategory.Color,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Spread2, LPImportance.High,
            TechTreeTech.ColorAdv)
        },
        { LPK.TexRadianceLitPower, LeafParamFloat(LPK.TexRadianceLitPower,
            new FloatRange(0, 1, 0.1f),
            "Radiance", LPType.Texture, LPCategory.Color,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Medium,
            TechTreeTech.ColorAdv)
        },
        { LPK.TexRadianceInversion, LeafParamToggleRange(LPK.TexRadianceInversion, 1f, 0f, -0.5f,
           "Radiance", LPType.Texture, LPCategory.Color,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Spread3, LPImportance.Medium,
            TechTreeTech.ColorAdv)
        },
        { LPK.TexRadiance, LeafParamFloat(LPK.TexRadiance,
            new FloatRange(0, 1, 0.5f),
            "Radiance", LPType.Texture, LPCategory.Color,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Medium,
            TechTreeTech.ColorAdv)
        },
        { LPK.TexRadianceMargin, LeafParamFloat(LPK.TexRadianceMargin,
            new FloatRange(0, 1, 0.5f),
            "Radiance", LPType.Texture, LPCategory.Color,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Medium,
            TechTreeTech.ColorAdv)
        },
        { LPK.TexRadianceDensity, LeafParamFloat(LPK.TexRadianceDensity,
            new FloatRange(0.25f, 2f, 1f),
            "Radiance", LPType.Texture, LPCategory.Color,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.ColorAdv)
        },
        { LPK.TexRadianceWidthMult, LeafParamFloat(LPK.TexRadianceWidthMult,
            new FloatRange(1f, 5f, 2f),
            "Radiance", LPType.Texture, LPCategory.Color,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.ColorAdv)
        },

        { LPK.TexMarginColor, LeafParamHSLColor(LPK.TexMarginColor,
            new HSLRange(new FloatRange(0, 359f / 360f, 0.33f),
                         new FloatRange(.20f, .95f, .85f),
                         new FloatRange(.05f, 0.99f, .2f)),
            "Margin", LPType.Texture, LPCategory.Color,
            new LPRandomValCenterBias[] {
                LPRandomValCenterBias.Default,
                LPRandomValCenterBias.Spread1,
                LPRandomValCenterBias.Spread1,
            },
            LPImportance.High,
            TechTreeTech.Color2,
            new Dictionary<TechTreeTech, RandomizerStrength>() {
                {TechTreeTech.Color1, RandomizerStrength.MediumLow},
                {TechTreeTech.Color2, RandomizerStrength.MediumHigh},
                {TechTreeTech.ColorAdv, RandomizerStrength.High},
            })
        },
        { LPK.TexMarginProminance, LeafParamFloat(LPK.TexMarginProminance,
            new FloatRange(0, 1, 0.5f),
            "Margin", LPType.Texture, LPCategory.Color,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Spread2, LPImportance.Medium,
            TechTreeTech.Color2)
        },
        { LPK.TexMarginAlpha, LeafParamFloat(LPK.TexMarginAlpha,
            new FloatRange(0, 1, 0.5f),
            "Margin", LPType.Texture, LPCategory.Color,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Spread2, LPImportance.Medium,
            TechTreeTech.Color2)
        },


        //Normals
        { LPK.NormalMidribWidth, LeafParamFloat(LPK.NormalMidribWidth,
            new FloatRange(0, 3, 1f),
            "Normals", LPType.Normal, LPCategory.Texture,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Squeeze2, LPImportance.Low,
            TechTreeTech.Texture1)
        },
        { LPK.NormalMidribDepth, LeafParamFloat(LPK.NormalMidribDepth,
            new FloatRange(-1f, 1f, 0.1f),
            "Normals", LPType.Normal, LPCategory.Texture,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.Texture1)
        },
        { LPK.NormalSecondaryWidth, LeafParamFloat(LPK.NormalSecondaryWidth,
            new FloatRange(0, 3, 1f),
            "Normals", LPType.Normal, LPCategory.Texture,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Squeeze2, LPImportance.Low,
            TechTreeTech.Texture1)
        },
        { LPK.NormalSecondaryDepth, LeafParamFloat(LPK.NormalSecondaryDepth,
            new FloatRange(-1f, 1f, 0.1f),
            "Normals", LPType.Normal, LPCategory.Texture,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.Texture1)
        },
        { LPK.NormalVeinSmooth, LeafParamFloat(LPK.NormalVeinSmooth,
            new FloatRange(0f, 1f, 0f),
            "Normals", LPType.Normal, LPCategory.Texture,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Squeeze3, LPImportance.Low,
            TechTreeTech.Texture1)
        },
        { LPK.NormalPuffySmooth, LeafParamFloat(LPK.NormalPuffySmooth,
            new FloatRange(0f, 1f, 0f),
            "Normals", LPType.Normal, LPCategory.Texture,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Squeeze3, LPImportance.Low,
            TechTreeTech.Texture1)
        },
        { LPK.NormalPuffyPlateauClamp, LeafParamFloat(LPK.NormalPuffyPlateauClamp,
            new FloatRange(0f, 1f, 0.2f),
            "Normals", LPType.Normal, LPCategory.Texture,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Medium,
            TechTreeTech.Texture1)
        },
        { LPK.NormalPuffyStrength, LeafParamFloat(LPK.NormalPuffyStrength,
            new FloatRange(0f, 1f, 0.1f),
            "Normals", LPType.Normal, LPCategory.Texture,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Spread1, LPImportance.High,
            TechTreeTech.Texture1)
        },


        //Material
        { LPK.MaterialShininess, LeafParamFloat(LPK.MaterialShininess,
            new FloatRange(0f, 1f, 0.75f),
            "Material", LPType.Material, LPCategory.Texture,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Spread1, LPImportance.High,
            TechTreeTech.Texture1)
        },
        { LPK.MaterialMetallicness, LeafParamFloat(LPK.MaterialMetallicness,
            new FloatRange(0f, 1f, 0.1f),
            "Material", LPType.Material, LPCategory.Texture,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Squeeze2, LPImportance.Medium,
            TechTreeTech.TextureAdv)
        },
        { LPK.MaterialAOStrength, LeafParamFloat(LPK.MaterialAOStrength,
            new FloatRange(0f, 1f, 0.5f),
            "Material", LPType.Material, LPCategory.Texture,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Spread3, LPImportance.Low,
            TechTreeTech.Texture1)
        },
        { LPK.MaterialRimPower, LeafParamFloat(LPK.MaterialRimPower,
            new FloatRange(0f, 1f, 0.0f),
            "Material", LPType.Material, LPCategory.Texture,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Squeeze2, LPImportance.High,
            TechTreeTech.TextureAdv)
        },
        { LPK.MaterialMicrotexAmp, LeafParamFloat(LPK.MaterialMicrotexAmp,
            new FloatRange(0f, 1f, 0.9f),
            "Material", LPType.Material, LPCategory.Texture,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Squeeze2, LPImportance.Medium,
            TechTreeTech.TextureAdv)
        },
        { LPK.MaterialRimColor, LeafParamHSLColor(LPK.MaterialRimColor,
            new HSLRange(new FloatRange(0, 359f / 360f, 0.33f),
                         new FloatRange(.20f, .95f, .60f),
                         new FloatRange(.05f, 0.7f, .50f)),
            "Material", LPType.Material, LPCategory.Texture,
            new LPRandomValCenterBias[] {
                LPRandomValCenterBias.Default,
                LPRandomValCenterBias.Squeeze2,
                LPRandomValCenterBias.Squeeze3,
            },
            LPImportance.High,
            TechTreeTech.TextureAdv)
        },

        { LPK.AbaxialDarkening, LeafParamFloat(LPK.AbaxialDarkening,
            new FloatRange(-0.95f, 0.95f, 0.0f),
            "Abaxial", LPType.Material, LPCategory.Color,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Medium,
            TechTreeTech.Color1)
        },
        { LPK.AbaxialPurpleTint, LeafParamFloat(LPK.AbaxialPurpleTint,
            new FloatRange(0f, 1f, 0.1f),
            "Abaxial", LPType.Material, LPCategory.Color,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Medium,
            TechTreeTech.Color1)
        },
        { LPK.AbaxialHue, LeafParamFloat(LPK.AbaxialHue,
            new FloatRange(-.5f, .5f, 0),
            "Abaxial", LPType.Material, LPCategory.Color,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Spread1, LPImportance.Medium,
            TechTreeTech.Color2)
        },

        { LPK.VertBumpsPower, LeafParamFloat(LPK.VertBumpsPower,
            new FloatRange(0f, 1f, 0.3f),
            "Height", LPType.Material, LPCategory.Texture,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Medium,
            TechTreeTech.TextureAdv)
        },
        { LPK.VertBumpsScale, LeafParamFloat(LPK.VertBumpsScale,
            new FloatRange(0f, 100f, 30f),
            "Height", LPType.Material, LPCategory.Texture,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Medium,
            TechTreeTech.TextureAdv)
        },
        { LPK.VertBumpsStretch, LeafParamFloat(LPK.VertBumpsStretch,
            new FloatRange(1f, 5f, 3f),
            "Height", LPType.Material, LPCategory.Texture,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Squeeze2, LPImportance.Medium,
            TechTreeTech.TextureAdv)
        },
        { LPK.VertBumpsPower2, LeafParamFloat(LPK.VertBumpsPower2,
            new FloatRange(0f, 1f, 0.3f),
            "Height", LPType.Material, LPCategory.Texture,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Medium,
            TechTreeTech.TextureAdv)
        },
        { LPK.VertBumpsScale2, LeafParamFloat(LPK.VertBumpsScale2,
            new FloatRange(0f, 100f, 30f),
            "Height", LPType.Material, LPCategory.Texture,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Medium,
            TechTreeTech.TextureAdv)
        },
        { LPK.VertBumpsStretch2, LeafParamFloat(LPK.VertBumpsStretch2,
            new FloatRange(1f, 5f, 3f),
            "Height", LPType.Material, LPCategory.Texture,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Squeeze2, LPImportance.Medium,
            TechTreeTech.TextureAdv)
        },
        { LPK.RadialBumpsPower, LeafParamFloat(LPK.RadialBumpsPower,
            new FloatRange(0f, 0.7f, 0.15f),
            "Height", LPType.Material, LPCategory.Texture,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Medium,
            TechTreeTech.TextureAdv)
        },
        { LPK.RadialBumpsScale, LeafParamFloat(LPK.RadialBumpsScale,
            new FloatRange(0f, 2f, 0.5f),
            "Height", LPType.Material, LPCategory.Texture,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Medium,
            TechTreeTech.TextureAdv)
        },
        { LPK.RadialBumpsLenScale, LeafParamFloat(LPK.RadialBumpsLenScale,
            new FloatRange(6f, 15f, 10f),
            "Height", LPType.Material, LPCategory.Texture,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Squeeze3, LPImportance.Medium,
            TechTreeTech.TextureAdv)
        },
        { LPK.RadialBumpsWidth, LeafParamFloat(LPK.RadialBumpsWidth,
            new FloatRange(1f, 15f, 3f),
            "Height", LPType.Material, LPCategory.Texture,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Medium,
            TechTreeTech.TextureAdv)
        },



        { LPK.MaterialHeightAmp, LeafParamFloat(LPK.MaterialHeightAmp,
            new FloatRange(-1f, 1f, 0.25f),
            "Height", LPType.Material, LPCategory.Texture,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.Texture1)
        },
        { LPK.TrunkBrowning, LeafParamFloat(LPK.TrunkBrowning,
            new FloatRange(0f, 1f, 0.2f),
            "Trunk", LPType.Material, LPCategory.Color,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.Color2)
        },
        { LPK.TrunkLightness, LeafParamFloat(LPK.TrunkLightness,
            new FloatRange(0f, 1f, 0.2f),
            "Trunk", LPType.Material, LPCategory.Color,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.Color2)
        },


        //Distort
        { LPK.DistortionEnabled, LeafParamToggle(LPK.DistortionEnabled, true,
            "Distort", LPType.Distort, LPCategory.Distortion)
        },
        { LPK.DistortCurl, LeafParamFloat(LPK.DistortCurl,
            new FloatRange(-179f, 179f, 0f),
            "Distort", LPType.Distort, LPCategory.Distortion,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.Distortion1, false)
        },
        { LPK.DistortCurlPoint, LeafParamFloat(LPK.DistortCurlPoint,
            new FloatRange(0f, 1f, 0.8f),
            "Distort", LPType.Distort, LPCategory.Distortion,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.Distortion1, false)
        },
        { LPK.DistortCup, LeafParamFloat(LPK.DistortCup,
            new FloatRange(-1f, 1f, 0.2f),
            "Distort", LPType.Distort, LPCategory.Distortion,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.Distortion1)
        },
        { LPK.DistortCupClamp, LeafParamFloat(LPK.DistortCupClamp,
            new FloatRange(0.05f, 1f, 0.8f),
            "Distort", LPType.Distort, LPCategory.Distortion,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.Distortion1)
        },
        { LPK.DistortFlop, LeafParamFloat(LPK.DistortFlop,
            new FloatRange(0f, 90f, 10f),
            "Distort", LPType.Distort, LPCategory.Distortion,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Squeeze3, LPImportance.Medium,
            TechTreeTech.Distortion1)
        },
        { LPK.DistortFlopStart, LeafParamFloat(LPK.DistortFlopStart,
            new FloatRange(0f, 0.9f, 0.5f),
            "Distort", LPType.Distort, LPCategory.Distortion,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Spread2, LPImportance.Low,
            TechTreeTech.Distortion1)
        },
        { LPK.DistortWaveAmp, LeafParamFloat(LPK.DistortWaveAmp,
            new FloatRange(0f, 1f, 0.15f),
            "Distort", LPType.Distort, LPCategory.Distortion,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.High,
            TechTreeTech.Distortion1)
        },
        { LPK.DistortWavePeriod, LeafParamFloat(LPK.DistortWavePeriod,
            new FloatRange(0f, 20f, 4f),
            "Distort", LPType.Distort, LPCategory.Distortion,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Squeeze1, LPImportance.High,
            TechTreeTech.Distortion1)
        },
        { LPK.DistortWaveDepth, LeafParamFloat(LPK.DistortWaveDepth,
            new FloatRange(0.1f, 1f, 0.55f),
            "Distort", LPType.Distort, LPCategory.Distortion,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.Distortion1)
        },
        { LPK.DistortWaveDivergance, LeafParamFloat(LPK.DistortWaveDivergance,
            new FloatRange(0f, 1f, 0.5f),
            "Distort", LPType.Distort, LPCategory.Distortion,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Medium,
            TechTreeTech.Distortion1)
        },
        { LPK.DistortWaveDivergancePeriod, LeafParamFloat(LPK.DistortWaveDivergancePeriod,
            new FloatRange(0.25f, 2f, 1f),
            "Distort", LPType.Distort, LPCategory.Distortion,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.Distortion1)
        },

        { LPK.ExtrudeEnabled, LeafParamToggle(LPK.ExtrudeEnabled, true,
            "Extrude", LPType.Distort, LPCategory.LeafShape)
        },
        { LPK.ExtrudeEdgeDepth, LeafParamFloat(LPK.ExtrudeEdgeDepth,
            new FloatRange(0f, 1f, 0.2f),
            "Extrude", LPType.Distort, LPCategory.LeafShape,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Squeeze1, LPImportance.Low,
            TechTreeTech.Distortion1)
        },
        { LPK.ExtrudeSuccThicc, LeafParamFloat(LPK.ExtrudeSuccThicc,
            new FloatRange(0f, 1f, 0.1f),
            "Extrude", LPType.Distort, LPCategory.LeafShape,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.Distortion1)
        },

        //Stem 
        { LPK.StemLength, LeafParamFloat(LPK.StemLength,
            new FloatRange(0.3f, 10f, 3.5f),
            "Stem", LPType.Stem, LPCategory.Arrangement,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Squeeze2, LPImportance.Medium,
            TechTreeTech.Arrangement1)
        },
        { LPK.StemWidth, LeafParamFloat(LPK.StemWidth,
            new FloatRange(0.1f, 1f, 0.4f),
            "Stem", LPType.Stem, LPCategory.Arrangement,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.Arrangement1)
        },
        { LPK.StemFlop, LeafParamFloat(LPK.StemFlop,
            new FloatRange(0f, 90f, 20f),
            "Stem", LPType.Stem, LPCategory.Arrangement,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Squeeze1, LPImportance.High,
            TechTreeTech.Arrangement1)
        },
        { LPK.StemNeck, LeafParamFloat(LPK.StemNeck,
            new FloatRange(0f, 30f, 10f),
            "Stem", LPType.Stem, LPCategory.Arrangement,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Disable,
            TechTreeTech.ArrangementAdv)
        },
        { LPK.StemAttachmentAngle, LeafParamFloat(LPK.StemAttachmentAngle,
            new FloatRange(-20f, 85f, 45f),
            "Stem", LPType.Stem, LPCategory.Arrangement,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Medium,
            TechTreeTech.ArrangementAdv)
        },
        { LPK.StemBaseColor, LeafParamHSLColor(LPK.StemBaseColor,
            new HSLRange(new FloatRange(0, 359f / 360f, 0.33f),
                         new FloatRange(.05f, .95f, .80f),
                         new FloatRange(.05f, .75f, .1f)),
            "StemColor", LPType.Stem, LPCategory.Color,
            new LPRandomValCenterBias[] {
                LPRandomValCenterBias.Default,
                LPRandomValCenterBias.Spread1,
                LPRandomValCenterBias.Squeeze2,
            },
            LPImportance.Medium,
            TechTreeTech.Color2,
            new Dictionary<TechTreeTech, RandomizerStrength>() {
                {TechTreeTech.Color2, RandomizerStrength.Medium},
                {TechTreeTech.ColorAdv, RandomizerStrength.High},
            })
        },
        { LPK.StemTopColorHue, LeafParamFloat(LPK.StemTopColorHue,
            new FloatRange(-1f, 1f, 0f),
            "StemColor", LPType.Stem, LPCategory.Color,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Medium,
            TechTreeTech.Color2)
        },
        { LPK.StemTopColorLit, LeafParamFloat(LPK.StemTopColorLit,
            new FloatRange(-1f, 0.75f, -0.2f),
            "StemColor", LPType.Stem, LPCategory.Color,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Squeeze3, LPImportance.Low,
            TechTreeTech.Color1)
        },
        { LPK.StemTopColorSat, LeafParamFloat(LPK.StemTopColorSat,
            new FloatRange(-1f, 1f, 0f),
            "StemColor", LPType.Stem, LPCategory.Color,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Spread2, LPImportance.Low,
            TechTreeTech.Color2)
        },
        { LPK.StemColorBias, LeafParamFloat(LPK.StemColorBias,
            new FloatRange(-1f, 1f, 0f),
            "StemColor", LPType.Stem, LPCategory.Color,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.Color2)
        },
        { LPK.StemShine, LeafParamFloat(LPK.StemShine,
            new FloatRange(0f, 1f, 0.2f),
            "StemColor", LPType.Stem, LPCategory.Color,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.Color1)
        },
        { LPK.StemBaseTexType, LeafParamFloat(LPK.StemBaseTexType,
            new FloatRange(-1.5f, 1.5f, -0.4f),
            "StemColor", LPType.Stem, LPCategory.Color,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Spread2, LPImportance.Low,
            TechTreeTech.Color1)
        },
        { LPK.StemTopTexType, LeafParamFloat(LPK.StemTopTexType,
            new FloatRange(-1.5f, 1.5f, 0.4f),
            "StemColor", LPType.Stem, LPCategory.Color,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Spread2, LPImportance.Low,
            TechTreeTech.Color1)
        },
        

        //Arrangement
        { LPK.LeafCount, LeafParamFloat(LPK.LeafCount,
            new FloatRange(1f, 30f, 5f),
            "Leaf", LPType.Arrangement, LPCategory.Arrangement,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.High,
            TechTreeTech.Arrangement1)
        },
        { LPK.LeafScale, LeafParamFloat(LPK.LeafScale,
            new FloatRange(0.25f, 3f, 1f),
            "Leaf", LPType.Arrangement, LPCategory.LeafShape,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Spread2, LPImportance.Medium,
            TechTreeTech.ShapeAdv)
        },
        { LPK.ScaleMin, LeafParamFloat(LPK.ScaleMin,
            new FloatRange(0.1f, 1f, 0.7f),
            "Leaf", LPType.Arrangement, LPCategory.LeafShape,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Squeeze1, LPImportance.Low,
            TechTreeTech.ShapeAdv)
        },
        { LPK.ScaleRand, LeafParamFloat(LPK.ScaleRand,
            new FloatRange(0.0f, 1f, 0.5f),
            "Leaf", LPType.Arrangement, LPCategory.Arrangement,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Spread3, LPImportance.Low,
            TechTreeTech.Arrangement1)
        },
        { LPK.LeafSkewMax, LeafParamFloat(LPK.LeafSkewMax,
            new FloatRange(0.0f, 90f, 30f),
            "Leaf", LPType.Arrangement, LPCategory.Arrangement,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Spread3, LPImportance.Low,
            TechTreeTech.ArrangementAdv)
        },
        { LPK.PhysicsAmplification, LeafParamFloat(LPK.PhysicsAmplification,
            new FloatRange(-1f, 2f, 1f), //with overflow; Clamp01 at runtime
            "Leaf", LPType.Arrangement, LPCategory.Arrangement,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Spread3, LPImportance.Low,
            TechTreeTech.Arrangement1)
        },
        { LPK.RotationalSymmetry, LeafParamFloat(LPK.RotationalSymmetry,
            new FloatRange(0f, 7f, 3.5f),
            "Stem", LPType.Arrangement, LPCategory.Arrangement,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Spread3, LPImportance.Medium,
            TechTreeTech.ArrangementAdv)
        },
        { LPK.RotationClustering, LeafParamFloat(LPK.RotationClustering,
            new FloatRange(0f, 1f, 0.8f),
            "Stem", LPType.Arrangement, LPCategory.Arrangement,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Squeeze1, LPImportance.Medium,
            TechTreeTech.Arrangement1)
        },
        { LPK.RotationRand, LeafParamFloat(LPK.RotationRand,
            new FloatRange(0f, 1f, 0.5f),
            "Stem", LPType.Arrangement, LPCategory.Arrangement,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.Arrangement1)
        },
        { LPK.NodeDistance, LeafParamFloat(LPK.NodeDistance,
            new FloatRange(0f, 6f, 1f),
            "Stem", LPType.Arrangement, LPCategory.Arrangement,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.High,
            TechTreeTech.Arrangement1)
        },
        { LPK.NodeInitialY, LeafParamFloat(LPK.NodeInitialY,
            new FloatRange(0f, 20f, 2f),
            "Stem", LPType.Arrangement, LPCategory.Arrangement,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Spread1, LPImportance.Medium,
            TechTreeTech.Arrangement1)
        },
        { LPK.StemLengthIncrease, LeafParamFloat(LPK.StemLengthIncrease,
            new FloatRange(0f, 10f, 2f),
            "Stem", LPType.Arrangement, LPCategory.Arrangement,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Disable,
            TechTreeTech.ArrangementAdv)
        },
        { LPK.StemLengthRand, LeafParamFloat(LPK.StemLengthRand,
            new FloatRange(0f, 1f, 0.4f),
            "Stem", LPType.Arrangement, LPCategory.Arrangement,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.Arrangement1)
        },
        { LPK.StemFlopLower, LeafParamFloat(LPK.StemFlopLower,
            new FloatRange(0f, 1f, 0.4f),
            "Stem", LPType.Arrangement, LPCategory.Arrangement,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Spread2, LPImportance.Low,
            TechTreeTech.Arrangement1)
        },
        { LPK.StemFlopRand, LeafParamFloat(LPK.StemFlopRand,
            new FloatRange(0f, 1f, 0.2f),
            "Stem", LPType.Arrangement, LPCategory.Arrangement,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Squeeze2, LPImportance.Low,
            TechTreeTech.Arrangement1)
        },
        { LPK.TrunkWidth, LeafParamFloat(LPK.TrunkWidth,
            new FloatRange(0.3f, 1.6f, 0.6f),
            "Trunk", LPType.Arrangement, LPCategory.Arrangement,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Squeeze2, LPImportance.Low,
            TechTreeTech.Arrangement1)
        },
        { LPK.TrunkLean, LeafParamFloat(LPK.TrunkLean,
            new FloatRange(0f, 1f, 0.2f),
            "Trunk", LPType.Arrangement, LPCategory.Arrangement,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Default, LPImportance.Low,
            TechTreeTech.Arrangement1)
        },
        { LPK.TrunkWobble, LeafParamFloat(LPK.TrunkWobble,
            new FloatRange(0f, 1f, 0.7f),
            "Trunk", LPType.Arrangement, LPCategory.Arrangement,
            LPRandomValCurve.CenterBellLRSplit, LPRandomValCenterBias.Spread2, LPImportance.Low,
            TechTreeTech.Arrangement1)
        },
        { LPK.PotScale, LeafParamFloat(LPK.PotScale,
            new FloatRange(0.25f, 3f, 1f),
            "Pot", LPType.Arrangement, LPCategory.Arrangement,
            LPRandomValCurve.CenterBell, LPRandomValCenterBias.Squeeze2, LPImportance.Disable,
            TechTreeTech.Arrangement1)
        },
      };
  }

}
