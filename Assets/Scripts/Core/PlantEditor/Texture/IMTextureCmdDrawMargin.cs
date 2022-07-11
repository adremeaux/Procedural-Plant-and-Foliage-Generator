using System;
using System.Collections.Generic;
using ImageMagick;
using UnityEngine;
using static BionicWombat.IMHelpers;

namespace BionicWombat {
  public class IMTextureCmdDrawMargin : BaseTextureCommand, IMTextureCommand {
    private MagickImage line;

    public void Prepare() {
      if (vars.VAL(LPK.TexMarginProminance) <= 0f) return;
      MarkStart();
      line = new MagickImage(MagickColors.None, vars.imgSize, vars.imgSize);
      line.Draw(new DrawablePolygon(vars.leafPoints),
                new DrawableFillColor(MagickColors.None),
                new DrawableFillOpacity(new Percentage(0)),
                new DrawableStrokeColor(MC(vars.COLOR(LPK.TexMarginColor))),
                new DrawableStrokeWidth(10f * vars.VAL(LPK.TexMarginProminance) / (double)vars.downsample));
      MarkEnd();
    }

    public void Composite(MagickImage image) {
      if (line == null) return;
      image.Composite(line, CompositeOperator.HardLight);
      line.Dispose();
      line = null;
    }
  }
}