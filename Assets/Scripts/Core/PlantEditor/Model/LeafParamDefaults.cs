using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using static BionicWombat.ColorExtensions;
using static BionicWombat.LeafParam;

namespace BionicWombat {
  public static class LeafParamDefaults {

    private static float baseWidth = 1f;
    private static float baseHeight = 3f;

    public static LeafParamDict Defaults() {
      return new LeafParamDict {
      //gen 0
        { LPK.Pudge, LeafParamFloat(LPK.Pudge,
            new FloatRange(0, baseHeight, 1),
            "Gen 0", LPType.Leaf, false)
        },
        { LPK.Sheer, LeafParamFloat(LPK.Sheer,
            new FloatRange(0, 1, 0.5f),
            "Gen 0", LPType.Leaf)
        },
        { LPK.Length, LeafParamFloat(LPK.Length,
            new FloatRange(1, baseHeight * 6f, 5),
            "Gen 0", LPType.Leaf)
        },
        { LPK.Width, LeafParamFloat(LPK.Width,
            new FloatRange(0.2f, baseWidth * 6f, 2),
            "Gen 0", LPType.Leaf)
        },
        { LPK.TipAngle, LeafParamFloat(LPK.TipAngle,
            new FloatRange(0, 90, 45),
            "Gen 0", LPType.Leaf)
        },
        { LPK.TipAmplitude, LeafParamFloat(LPK.TipAmplitude,
            new FloatRange(0, 3, 1),
            "Gen 0", LPType.Leaf)
        },

        //gen 1
        { LPK.Heart, LeafParamToggle(LPK.Heart,
            true, "Gen 1", LPType.Leaf)
        },
        { LPK.SinusSheer, LeafParamFloat(LPK.SinusSheer,
            new FloatRange(-0.5f, 4, 0.5f),
            "Gen 1", LPType.Leaf)
        },
        { LPK.SinusHeight, LeafParamFloat(LPK.SinusHeight,
            new FloatRange(0, 4, 1),
            "Gen 1", LPType.Leaf)
        },
        { LPK.WaistAmp, LeafParamFloat(LPK.WaistAmp,
            new FloatRange(0.1f, 2, 1),
            "Gen 1", LPType.Leaf)
        },
        { LPK.WaistAmpOffset, LeafParamFloat(LPK.WaistAmpOffset,
            new FloatRange(-0.5f, 0.5f, 0),
            "Gen 1", LPType.Leaf)
        },

        //gen 2
        { LPK.Lobes, LeafParamToggle(LPK.Lobes,
            true, "Gen 2", LPType.Leaf)
        },
        { LPK.LobeTilt, LeafParamFloat(LPK.LobeTilt,
            new FloatRange(0, 90, 0),
            "Gen 2", LPType.Leaf)
        },
        { LPK.LobeAmplitude, LeafParamFloat(LPK.LobeAmplitude,
            new FloatRange(0, 2, 1),
            "Gen 2", LPType.Leaf)
        },
        { LPK.LobeAmpOffset, LeafParamFloat(LPK.LobeAmpOffset,
            new FloatRange(-0.5f, 0.5f, 0),
            "Gen 2", LPType.Leaf)
        },

        //gen 3
        { LPK.ScoopDepth, LeafParamFloat(LPK.ScoopDepth,
            new FloatRange(0f, 0.9f, 0.1f),
            "Gen 3", LPType.Leaf)
        },
        { LPK.ScoopHeight, LeafParamFloat(LPK.ScoopHeight,
            new FloatRange(0f, 1f, 0.1f),
            "Gen 3", LPType.Leaf)
        },

        //VEINS
        { LPK.VeinDensity, LeafParamFloat(LPK.VeinDensity,
            new FloatRange(0.1f, 1.5f, 0.25f),
            "Gen 0", LPType.Vein)
        },
        { LPK.VeinBunching, LeafParamFloat(LPK.VeinBunching,
            new FloatRange(1, 3, 1.5f),
            "Gen 0", LPType.Vein)
        },
        { LPK.VeinLobeBunching, LeafParamFloat(LPK.VeinLobeBunching,
            new FloatRange(1, 5, 1.5f),
            "Gen 0", LPType.Vein)
        },
        { LPK.VeinOriginRand, LeafParamFloat(LPK.VeinOriginRand,
            new FloatRange(0f, 1f, 0.5f),
            "Gen 0", LPType.Vein)
        },
        { LPK.GravVeinUpperBias, LeafParamFloat(LPK.GravVeinUpperBias,
            new FloatRange(0, 1, 0.5f),
            "Gen 0", LPType.Vein)
        },
        { LPK.GravVeinLowerBias, LeafParamFloat(LPK.GravVeinLowerBias,
            new FloatRange(0, 1, 0.5f),
            "Gen 0", LPType.Vein)
        },
        { LPK.VeinEndOffset, LeafParamFloat(LPK.VeinEndOffset,
            new FloatRange(0, 2, 0.5f),
            "Gen 0", LPType.Vein)
        },
        { LPK.VeinEndLerp, LeafParamFloat(LPK.VeinEndLerp,
            new FloatRange(0, 1, 0.0f),
            "Gen 0", LPType.Vein)
        },
        { LPK.VeinDistFromMargin, LeafParamFloat(LPK.VeinDistFromMargin,
            new FloatRange(0, 1, 0.1f),
            "Gen 0", LPType.Vein)
        },
        { LPK.MidribDistFromMargin, LeafParamFloat(LPK.MidribDistFromMargin,
            new FloatRange(0, 1, 0.1f),
            "Gen 0", LPType.Vein)
        },
        { LPK.SpannerLerp, LeafParamFloat(LPK.SpannerLerp,
            new FloatRange(-0.5f, 1, 0.2f),
            "Gen 0", LPType.Vein)
        },
        { LPK.SpannerSqueeze, LeafParamFloat(LPK.SpannerSqueeze,
            new FloatRange(0, 0.5f, 0.16f),
            "Gen 0", LPType.Vein)
        },


        { LPK.MidribThickness, LeafParamFloat(LPK.MidribThickness,
            new FloatRange(0, 0.15f, 0.08f),
            "Thickness", LPType.Vein)
        },
        { LPK.SecondaryThickness, LeafParamFloat(LPK.SecondaryThickness,
            new FloatRange(0, 0.12f, 0.04f),
            "Thickness", LPType.Vein)
        },
        { LPK.SpannerThickness, LeafParamFloat(LPK.SpannerThickness,
            new FloatRange(0, 1f, 0.5f),
            "Thickness", LPType.Vein)
        },
        { LPK.MidribTaper, LeafParamFloat(LPK.MidribTaper,
            new FloatRange(0, 4, 1f),
            "Thickness", LPType.Vein)
        },
        { LPK.SecondaryTaper, LeafParamFloat(LPK.SecondaryTaper,
            new FloatRange(0, 4, 1f),
            "Thickness", LPType.Vein)
        },
        { LPK.SpannerTaper, LeafParamFloat(LPK.SpannerTaper,
            new FloatRange(0, 2, 1f),
            "Thickness", LPType.Vein)
        },
        { LPK.TaperRNG, LeafParamFloat(LPK.TaperRNG,
            new FloatRange(0, 2, 0.5f),
            "Thickness", LPType.Vein)
        },

        //gen 1
        { LPK.VeinSplit, LeafParamToggle(LPK.VeinSplit, false,
            "Split", LPType.Vein)
        },
        { LPK.VeinSplitDepth, LeafParamFloat(LPK.VeinSplitDepth,
            new FloatRange(0.1f, 0.9f, 0.5f),
            "Split", LPType.Vein)
        },
        { LPK.VeinSplitAmp, LeafParamFloat(LPK.VeinSplitAmp,
            new FloatRange(0.1f, 0.9f, 0.5f),
            "Split", LPType.Vein)
        },
        { LPK.VeinSplitAmpOffset, LeafParamFloat(LPK.VeinSplitAmpOffset,
            new FloatRange(0, 1, 0.5f),
            "Split", LPType.Vein)
        },    

        //Texture
        { LPK.TexBaseColor, LeafParamHSLColor(LPK.TexBaseColor,
            new HSLRange(new FloatRange(75f / 360f, 165f / 360f, 115f / 360f),
                        new FloatRange(.05f, .95f, .90f),
                        new FloatRange(.05f, .95f, .40f)),
            "Base", LPType.Texture)
        },
        { LPK.TexShadowStrength, LeafParamFloat(LPK.TexShadowStrength,
            new FloatRange(0, 1, 0.5f),
            "Base", LPType.Texture)
        },
        { LPK.TexVeinColor, LeafParamHSLColor(LPK.TexVeinColor,
            new HSLRange(new FloatRange(0, 359f / 360f, 0.33f),
                        new FloatRange(.20f, .95f, .80f),
                        new FloatRange(.05f, .99f, .8f)),
            "Veins", LPType.Texture)
        },
        { LPK.TexVeinOpacity, LeafParamFloat(LPK.TexVeinOpacity,
            new FloatRange(0, 1, 0.5f),
            "Veins", LPType.Texture)
        },
        { LPK.TexVeinSecondaryOpacity, LeafParamFloat(LPK.TexVeinSecondaryOpacity,
            new FloatRange(0, 1, 0.5f),
            "Veins", LPType.Texture)
        },
        { LPK.TexVeinDepth, LeafParamFloat(LPK.TexVeinDepth,
            new FloatRange(0, 1, 0.5f),
            "Veins", LPType.Texture)
        },
        { LPK.TexVeinBlur, LeafParamFloat(LPK.TexVeinBlur,
            new FloatRange(0, 1, 0.5f),
            "Veins", LPType.Texture)
        },

        { LPK.TexRadianceHue, LeafParamFloat(LPK.TexRadianceHue,
            new FloatRange(-1, 1, 0),
            "Radiance", LPType.Texture)
        },
        { LPK.TexRadianceLitPower, LeafParamFloat(LPK.TexRadianceLitPower,
            new FloatRange(0, 1, 0.5f),
            "Radiance", LPType.Texture)
        },
        { LPK.TexRadianceInversion, LeafParamToggle(LPK.TexRadianceInversion,
            true, "Radiance", LPType.Texture)
        },
        { LPK.TexRadiance, LeafParamFloat(LPK.TexRadiance,
            new FloatRange(0, 1, 0.5f),
            "Radiance", LPType.Texture)
        },
        { LPK.TexRadianceMargin, LeafParamFloat(LPK.TexRadianceMargin,
            new FloatRange(0, 1, 0.5f),
            "Radiance", LPType.Texture)
        },
        { LPK.TexRadianceDensity, LeafParamFloat(LPK.TexRadianceDensity,
            new FloatRange(0.25f, 2f, 1f),
            "Radiance", LPType.Texture)
        },
        { LPK.TexRadianceWidthMult, LeafParamFloat(LPK.TexRadianceWidthMult,
            new FloatRange(1f, 5f, 2f),
            "Radiance", LPType.Texture)
        },

        { LPK.TexMarginColor, LeafParamHSLColor(LPK.TexMarginColor,
            new HSLRange(new FloatRange(0, 359f / 360f, 0.33f),
                        new FloatRange(.20f, .95f, .80f),
                        new FloatRange(.05f, 0.99f, .8f)),
            "Margin", LPType.Texture)
        },
        { LPK.TexMarginProminance, LeafParamFloat(LPK.TexMarginProminance,
            new FloatRange(0, 1, 0.5f),
            "Margin", LPType.Texture)
        },


        //Normals
        { LPK.NormalMidribWidth, LeafParamFloat(LPK.NormalMidribWidth,
            new FloatRange(0, 3, 1f),
            "Normals", LPType.Normal)
        },
        { LPK.NormalMidribDepth, LeafParamFloat(LPK.NormalMidribDepth,
            new FloatRange(-1f, 1f, 0.5f),
            "Normals", LPType.Normal)
        },
        { LPK.NormalSecondaryWidth, LeafParamFloat(LPK.NormalSecondaryWidth,
            new FloatRange(0, 3, 1f),
            "Normals", LPType.Normal)
        },
        { LPK.NormalSecondaryDepth, LeafParamFloat(LPK.NormalSecondaryDepth,
            new FloatRange(-1f, 1f, 0.5f),
            "Normals", LPType.Normal)
        },
        { LPK.NormalVeinSmooth, LeafParamFloat(LPK.NormalVeinSmooth,
            new FloatRange(0f, 1f, 0f),
            "Normals", LPType.Normal)
        },
        { LPK.NormalPuffySmooth, LeafParamFloat(LPK.NormalPuffySmooth,
            new FloatRange(0f, 1f, 0f),
            "Normals", LPType.Normal)
        },
        { LPK.NormalPuffyPlateauClamp, LeafParamFloat(LPK.NormalPuffyPlateauClamp,
            new FloatRange(0f, 1f, 0.2f),
            "Normals", LPType.Normal)
        },
        { LPK.NormalPuffyStrength, LeafParamFloat(LPK.NormalPuffyStrength,
            new FloatRange(0f, 1f, 0.1f),
            "Normals", LPType.Normal)
        },


        //Material
        { LPK.MaterialShininess, LeafParamFloat(LPK.MaterialShininess,
            new FloatRange(0f, 1f, 0.3f),
            "Material", LPType.Material)
        },
        { LPK.MaterialMetallicness, LeafParamFloat(LPK.MaterialMetallicness,
            new FloatRange(0f, 1f, 0.1f),
            "Material", LPType.Material)
        },
        { LPK.MaterialAOStrength, LeafParamFloat(LPK.MaterialAOStrength,
            new FloatRange(0f, 1f, 0.5f),
            "Material", LPType.Material)
        },
        { LPK.MaterialRimPower, LeafParamFloat(LPK.MaterialRimPower,
            new FloatRange(0f, 1f, 0.0f),
            "Material", LPType.Material)
        },
        { LPK.MaterialMicrotexAmp, LeafParamFloat(LPK.MaterialMicrotexAmp,
            new FloatRange(0f, 1f, 0.1f),
            "Material", LPType.Material)
        },
        { LPK.MaterialRimColor, LeafParamHSLColor(LPK.MaterialRimColor,
            new HSLRange(new FloatRange(0, 359f / 360f, 0.33f),
                        new FloatRange(.20f, .95f, .80f),
                        new FloatRange(.05f, 0.99f, .8f)),
            "Material", LPType.Material)
        },
        { LPK.MaterialVertBumps, LeafParamFloat(LPK.MaterialVertBumps,
            new FloatRange(0f, 1f, 0.1f),
            "Height", LPType.Material)
        },
        { LPK.MaterialHeightAmp, LeafParamFloat(LPK.MaterialHeightAmp,
            new FloatRange(-1f, 1f, 0.25f),
            "Height", LPType.Material)
        },
        { LPK.TrunkBrowning, LeafParamFloat(LPK.TrunkBrowning,
            new FloatRange(0f, 1f, 0.2f),
            "Trunk", LPType.Material)
        },
        { LPK.TrunkLightness, LeafParamFloat(LPK.TrunkLightness,
            new FloatRange(0f, 1f, 0.2f),
            "Trunk", LPType.Material)
        },


        //Distort
        { LPK.DistortionEnabled, LeafParamToggle(LPK.DistortionEnabled, true,
            "Distort", LPType.Distort)
        },
        { LPK.DistortCurl, LeafParamFloat(LPK.DistortCurl,
            new FloatRange(-179f, 179f, 0f),
            "Distort", LPType.Distort, false)
        },
        { LPK.DistortCurlPoint, LeafParamFloat(LPK.DistortCurlPoint,
            new FloatRange(0f, 1f, 0.8f),
            "Distort", LPType.Distort)
        },
        { LPK.DistortCup, LeafParamFloat(LPK.DistortCup,
            new FloatRange(-1f, 1f, 0f),
            "Distort", LPType.Distort)
        },
        { LPK.DistortCupClamp, LeafParamFloat(LPK.DistortCupClamp,
            new FloatRange(0f, 1f, 0.8f),
            "Distort", LPType.Distort)
        },
        { LPK.DistortFlop, LeafParamFloat(LPK.DistortFlop,
            new FloatRange(0f, 90f, 10f),
            "Distort", LPType.Distort)
        },
        { LPK.DistortFlopStart, LeafParamFloat(LPK.DistortFlopStart,
            new FloatRange(0f, 0.9f, 0.2f),
            "Distort", LPType.Distort)
        },
        { LPK.DistortWave, LeafParamFloat(LPK.DistortWave,
            new FloatRange(0f, 1f, 0.15f),
            "Distort", LPType.Distort)
        },
        { LPK.DistortWavePeriod, LeafParamFloat(LPK.DistortWavePeriod,
            new FloatRange(0f, 20f, 8f),
            "Distort", LPType.Distort)
        },
        { LPK.DistortWaveDepth, LeafParamFloat(LPK.DistortWaveDepth,
            new FloatRange(0f, 1f, 0.1f),
            "Distort", LPType.Distort)
        },

        { LPK.ExtrudeEnabled, LeafParamToggle(LPK.ExtrudeEnabled, true,
            "Extrude", LPType.Distort)
        },
        { LPK.ExtrudeEdgeDepth, LeafParamFloat(LPK.ExtrudeEdgeDepth,
            new FloatRange(0f, 1f, 0.1f),
            "Extrude", LPType.Distort)
        },
        { LPK.ExtrudeSuccThicc, LeafParamFloat(LPK.ExtrudeSuccThicc,
            new FloatRange(0f, 1f, 0.1f),
            "Extrude", LPType.Distort)
        },

        //Stem
        { LPK.StemLength, LeafParamFloat(LPK.StemLength,
            new FloatRange(0f, 40f, 15f),
            "Stem", LPType.Stem)
        },
        { LPK.StemWidth, LeafParamFloat(LPK.StemWidth,
            new FloatRange(0.1f, 1f, 0.3f),
            "Stem", LPType.Stem)
        },
        { LPK.StemFlop, LeafParamFloat(LPK.StemFlop,
            new FloatRange(0f, 90f, 20f),
            "Stem", LPType.Stem)
        },
        { LPK.StemNeck, LeafParamFloat(LPK.StemNeck,
            new FloatRange(0f, 90f, 10f),
            "Stem", LPType.Stem)
        },
        { LPK.StemAttachmentAngle, LeafParamFloat(LPK.StemAttachmentAngle,
            new FloatRange(-70f, 90f, 0f),
            "Stem", LPType.Stem)
        },

        //Arrangement
        { LPK.LeafCount, LeafParamFloat(LPK.LeafCount,
            new FloatRange(1f, 30f, 3f),
            "Leaf", LPType.Arrangement)
        },
        { LPK.LeafScale, LeafParamFloat(LPK.LeafScale,
            new FloatRange(0.1f, 3f, 1f),
            "Leaf", LPType.Arrangement)
        },
        { LPK.ScaleMin, LeafParamFloat(LPK.ScaleMin,
            new FloatRange(0.1f, 1f, 0.7f),
            "Leaf", LPType.Arrangement)
        },
        { LPK.ScaleRand, LeafParamFloat(LPK.ScaleRand,
            new FloatRange(0.0f, 1f, 0.5f),
            "Leaf", LPType.Arrangement)
        },
        { LPK.RotationalSymmetry, LeafParamFloat(LPK.RotationalSymmetry,
            new FloatRange(0f, 7f, 1f),
            "Stem", LPType.Arrangement)
        },
        { LPK.RotationClustering, LeafParamFloat(LPK.RotationClustering,
            new FloatRange(0f, 1f, 0.5f),
            "Stem", LPType.Arrangement)
        },
        { LPK.RotationRand, LeafParamFloat(LPK.RotationRand,
            new FloatRange(0f, 1f, 0.5f),
            "Stem", LPType.Arrangement)
        },
        { LPK.NodeDistance, LeafParamFloat(LPK.NodeDistance,
            new FloatRange(0f, 10f, 1f),
            "Stem", LPType.Arrangement)
        },
        { LPK.NodeInitialY, LeafParamFloat(LPK.NodeInitialY,
            new FloatRange(0f, 10f, 0f),
            "Stem", LPType.Arrangement)
        },
        { LPK.StemLengthIncrease, LeafParamFloat(LPK.StemLengthIncrease,
            new FloatRange(0f, 10f, 1f),
            "Stem", LPType.Arrangement)
        },
        { LPK.StemLengthRand, LeafParamFloat(LPK.StemLengthRand,
            new FloatRange(0f, 1f, 0.2f),
            "Stem", LPType.Arrangement)
        },
        { LPK.StemFlopRand, LeafParamFloat(LPK.StemFlopRand,
            new FloatRange(0f, 1f, 0.2f),
            "Stem", LPType.Arrangement)
        },
        { LPK.StemFlopLower, LeafParamFloat(LPK.StemFlopLower,
            new FloatRange(0f, 1f, 0.2f),
            "Stem", LPType.Arrangement)
        },
        { LPK.TrunkWidth, LeafParamFloat(LPK.TrunkWidth,
            new FloatRange(0.1f, 2f, 0.6f),
            "Trunk", LPType.Arrangement)
        },
        { LPK.TrunkLean, LeafParamFloat(LPK.TrunkLean,
            new FloatRange(0f, 1f, 0.2f),
            "Trunk", LPType.Arrangement)
        },
        { LPK.TrunkWobble, LeafParamFloat(LPK.TrunkWobble,
            new FloatRange(0f, 1f, 0.3f),
            "Trunk", LPType.Arrangement)
        },
        { LPK.PotScale, LeafParamFloat(LPK.PotScale,
            new FloatRange(0.25f, 3f, 1f),
            "Pot", LPType.Arrangement)
        },
    };
    }
  }
}