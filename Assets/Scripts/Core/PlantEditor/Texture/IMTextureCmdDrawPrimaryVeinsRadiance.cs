using System;
using System.Collections.Generic;
using ImageMagick;
using UnityEngine;
using static BionicWombat.IMHelpers;

namespace BionicWombat {
  public class IMTextureCmdDrawPrimaryVeinsRadiance : BaseTextureCommand, IMTextureCommand {
    private MagickImage line;

    public void Prepare() {
      MarkStart();

      double downsample = vars.downsample;
      line = new MagickImage(MagickColors.None, vars.imgSizeScaled, vars.imgSizeScaled);
      if (vars.VAL(LPK.TexRadiance) > 0) {
        foreach (IMRenderableVein v in vars.rendVeins) {
          PointD[] poly = v.veinPolyRadiance;
          if (poly.Length < 3) continue;
          line.Draw(new DrawablePolygon(poly),
                    new DrawableFillColor(MC(vars.radianceColor)),
                    new DrawableFillOpacity(new Percentage(100f * vars.VAL(LPK.TexRadiance))));
        }
      }
      if (vars.VAL(LPK.TexRadianceMargin) > 0) {
        line.Draw(new DrawablePolygon(vars.leafPoints),
                  new DrawableFillColor(MagickColors.None),
                  new DrawableFillOpacity(new Percentage(0)),
                  new DrawableStrokeColor(MC(vars.radianceColor)),
                  new DrawableStrokeOpacity(new Percentage(100f * vars.VAL(LPK.TexRadianceMargin))),
                  new DrawableStrokeWidth((20f * (vars.VAL(LPK.TexRadianceMargin) * 0.5f + 0.5f)) / downsample));
      }
      line.Scale(vars.shadowSizeScaledLess, vars.shadowSizeScaledLess);
      line.Blur((5f * vars.VAL(LPK.TexRadianceDensity)) / downsample, (5f * vars.VAL(LPK.TexRadianceDensity)) / downsample);
      line.Resize(vars.imgSizeScaled, vars.imgSizeScaled);

      MarkEnd();
    }

    public void Composite(MagickImage image) {
      image.Composite(line, vars.ENABLED(LPK.TexRadianceInversion) ? CompositeOperator.Darken : CompositeOperator.HardLight);
      line.Dispose();
    }
  }
}