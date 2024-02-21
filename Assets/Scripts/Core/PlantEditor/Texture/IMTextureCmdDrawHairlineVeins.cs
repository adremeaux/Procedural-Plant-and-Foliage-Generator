using ImageMagick;

namespace BionicWombat {
  public class IMTextureCmdDrawHairlineVeins : BaseTextureCommand, IMTextureCommand {
    private MagickImage line;

    public IMTextureCmdDrawHairlineVeins() : base() {
      enabled = false;
    }

    public void Prepare() {
      MarkStart();

      line = new MagickImage(MagickColors.None, vars.imgSizeScaled, vars.imgSizeScaled);
      foreach (IMRenderableVein v in vars.rendVeins) {
        if (v.vein.type == LeafVeinType.Midrib) continue;
        PointD[] poly = v.veinPoints;
        PointD[] shortPoly = new PointD[poly.Length - 2];
        for (int i = 1; i < poly.Length - 1; i++) shortPoly[i - 1] = poly[i];
        line.Draw(new DrawablePolyline(shortPoly),
                  new DrawableFillColor(MagickColors.None),
                  new DrawableFillOpacity(new Percentage(0)),
                  new DrawableStrokeColor(MC(vars.COLOR(LPK.TexMarginColor))),
                  new DrawableStrokeOpacity(new Percentage(75f * vars.VAL(LPK.TexVeinOpacity))),
                  new DrawableStrokeWidth(0.5f)); //don't vars.downsample
      }

      MarkEnd();
    }

    public void Composite(MagickImage image) {
      image.Composite(line, CompositeOperator.HardLight);
      line.Dispose();
    }
  }
}
