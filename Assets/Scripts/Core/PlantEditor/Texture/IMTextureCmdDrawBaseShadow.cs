using ImageMagick;

namespace BionicWombat {
  public class IMTextureCmdDrawBaseShadow : BaseTextureCommand, IMTextureCommand {
    private MagickImage tri;

    public void Prepare() {
      MarkStart();
      tri = new MagickImage(MagickColors.None, vars.imgSizeScaled, vars.imgSizeScaled);
      PointD origin = vars.leafPoints[0];
      PointD topRight = vars.leafPoints[(int)(vars.lineSteps * 0.4)]; //this needs work!
      double horizDiff = topRight.X - origin.X;
      double vertDiff = origin.Y - topRight.Y;
      tri.Draw(
        new DrawablePolygon(new PointD[] {
          topRight,
          new PointD(origin.X, origin.Y + vertDiff * 1.5),
          new PointD(topRight.X - horizDiff * 2.0, topRight.Y)
        }),
        new DrawableFillColor(MC(vars.shadowColor))
      );
      tri.Scale(vars.shadowSizeScaled, vars.shadowSizeScaled);
      tri.Blur(5f / (double)vars.downsample, 5f / (double)vars.downsample);
      tri.Resize(vars.imgSizeScaled, vars.imgSizeScaled);
      MarkEnd();
    }

    public void Composite(MagickImage image) {
      image.Composite(tri, CompositeOperator.Multiply);
      tri.Dispose();
    }
  }
}
