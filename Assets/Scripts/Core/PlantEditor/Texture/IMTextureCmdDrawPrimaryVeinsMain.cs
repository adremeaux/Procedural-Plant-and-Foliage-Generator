using ImageMagick;
using UnityEngine;

namespace BionicWombat {
  public class IMTextureCmdDrawPrimaryVeinsMain : BaseTextureCommand, IMTextureCommand {
    private MagickImage line;
    private Color? overrideColor = null;
    private bool useBlur = true;

    public IMTextureCmdDrawPrimaryVeinsMain(Color? overrideColor = null, bool useBlur = true) {
      this.overrideColor = overrideColor;
      this.useBlur = useBlur;
    }

    public void Prepare() {
      MarkStart();

      HSL hslMidrib = vars.COLOR(LPK.TexVeinColor).ToHSL();
      hslMidrib.saturation = hslMidrib.saturation * vars.VAL(LPK.TexVeinOpacity);
      float lit = hslMidrib.lightness - 0.5f;
      lit *= vars.VAL(LPK.TexVeinOpacity);
      lit += 0.5f;
      hslMidrib.lightness = lit;

      HSL hslSecondary = vars.COLOR(LPK.TexVeinColor).ToHSL();
      hslSecondary.saturation = hslSecondary.saturation * vars.VAL(LPK.TexVeinSecondaryOpacity);
      float litS = hslSecondary.lightness - 0.5f;
      litS *= vars.VAL(LPK.TexVeinSecondaryOpacity);
      litS += 0.5f;
      hslSecondary.lightness = litS;

      if (overrideColor is Color oc) hslMidrib = hslSecondary = oc.ToHSL();

      line = new MagickImage(MC(hslSecondary.ToColor()), vars.imgSizeScaled, vars.imgSizeScaled);
      using (MagickImage noise = new MagickImage(MagickColors.None, vars.imgSizeScaled, vars.imgSizeScaled)) {
        noise.AddNoise(NoiseType.Uniform, 100f, Channels.RGB);
        noise.Fx("intensity");

        using (MagickImage blurShapeLayer = new MagickImage(MagickColors.None, vars.imgSizeScaled, vars.imgSizeScaled)) {
          foreach (IMRenderableVein v in vars.rendVeins) {
            PointD[] poly = v.veinPoly;
            if (poly.Length < 3) continue;
            if (v.vein.type == LeafVeinType.Midrib)
              line.Draw(new DrawablePolygon(poly), new DrawableFillColor(MC(hslMidrib.ToColor())));
            blurShapeLayer.Draw(new DrawablePolygon(poly), new DrawableFillColor(MagickColors.Magenta));
          }
          // blurShapeLayer.Scale(vars.shadowSizeScaled, vars.shadowSizeScaled);
          if (vars.VAL(LPK.TexVeinBlur) > 0.05f && useBlur)
            blurShapeLayer.Blur(9f * vars.VAL(LPK.TexVeinBlur) / (double)vars.downsample,
                                9f * vars.VAL(LPK.TexVeinBlur) / (double)vars.downsample);
          // blurShapeLayer.Resize(vars.imgSizeScaled, vars.imgSizeScaled);
          noise.Composite(blurShapeLayer, CompositeOperator.SrcOver);

          line.Composite(noise, CompositeOperator.CopyAlpha);
        }
      }

      MarkEnd();
    }

    public void Composite(MagickImage image) {
      image.Composite(line, CompositeOperator.HardLight);
      line.Dispose();
    }
  }
}
