using ImageMagick;

namespace BionicWombat {
  public class IMTextureCmdDrawPrimaryVeinsDropShadow : BaseTextureCommand, IMTextureCommand {
    private MagickImage line;

    public IMTextureCmdDrawPrimaryVeinsDropShadow() : base() {
      enabled = false;
    }

    public void Prepare() {
      MarkStart();

      line = new MagickImage(MagickColors.None, vars.imgSizeScaled, vars.imgSizeScaled);
      foreach (IMRenderableVein v in vars.rendVeins) {
        PointD[] poly = v.veinPoly;
        if (poly.Length < 3) continue;
        line.Draw(new DrawablePolygon(poly),
                  new DrawableFillColor(MC(vars.shadowColor)),
                  new DrawableFillOpacity(new Percentage(vars.VAL(LPK.TexVeinDepth) * 100f)));
      }
      line.Scale(vars.shadowSizeScaled, vars.shadowSizeScaled);
      line.Blur(1f / (double)vars.downsample, 1f / (double)vars.downsample);
      line.Resize(vars.imgSizeScaled, vars.imgSizeScaled);

      MarkEnd();
    }

    public void Composite(MagickImage image) {
      image.Composite(line, CompositeOperator.HardLight);
      line.Dispose();
    }
  }
}
