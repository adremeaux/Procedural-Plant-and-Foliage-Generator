using ImageMagick;

namespace BionicWombat {
  public class IMTextureCmdDrawInnerShadow : BaseTextureCommand, IMTextureCommand {
    private MagickImage line;
    private double width;
    private double blur;
    private CompositeOperator compOp;

    public IMTextureCmdDrawInnerShadow(double width, double blur, CompositeOperator op) : base() {
      this.width = width;
      this.blur = blur;
      compOp = op;
    }

    public void Prepare() {
      MarkStart();
      line = new MagickImage(MagickColors.None, vars.imgSizeScaled, vars.imgSizeScaled);
      line.Draw(new DrawablePolygon(vars.leafPoints),
                new DrawableFillColor(MagickColors.None),
                new DrawableFillOpacity(new Percentage(0)),
                new DrawableStrokeColor(MC(vars.shadowColor)),
                new DrawableStrokeWidth(width / (double)vars.downsample));
      line.Scale(vars.shadowSizeScaled, vars.shadowSizeScaled);
      line.Blur(blur / (double)vars.downsample, blur / (double)vars.downsample);
      line.Resize(vars.imgSizeScaled, vars.imgSizeScaled);
      MarkEnd();
    }

    public void Composite(MagickImage image) {
      image.Composite(line, compOp);
      line.Dispose();
    }
  }
}
