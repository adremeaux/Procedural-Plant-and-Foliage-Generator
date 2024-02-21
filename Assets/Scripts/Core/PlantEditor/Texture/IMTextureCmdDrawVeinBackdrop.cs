using ImageMagick;

namespace BionicWombat {
  public class IMTextureCmdDrawVeinBackdrop : BaseTextureCommand, IMTextureCommand {
    private MagickImage line;

    public void Prepare() {
      MarkStart();

      line = new MagickImage(MagickColors.None, vars.imgSizeScaled, vars.imgSizeScaled);
      foreach (IMRenderableVein v in vars.rendVeins) {
        PointD[] poly = v.veinPolyExtraThick;
        if (poly.Length < 3) continue;
        line.Draw(new DrawablePolygon(poly),
                  new DrawableFillColor(MC(vars.shadowColor)),
                  new DrawableFillOpacity(new Percentage(100f * vars.VAL(LPK.TexVeinDepth))));
      }
      line.Scale(vars.shadowSizeScaled, vars.shadowSizeScaled);
      line.Blur(3f / (double)vars.downsample, 3f / (double)vars.downsample);
      line.Resize(vars.imgSizeScaled, vars.imgSizeScaled);

      MarkEnd();
    }

    public void Composite(MagickImage image) {
      image.Composite(line, CompositeOperator.Overlay);
      line.Dispose();
    }
  }
}
