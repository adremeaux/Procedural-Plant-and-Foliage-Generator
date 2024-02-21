using ImageMagick;
using UnityEngine;

namespace BionicWombat {
  public class IMTextureCmdDrawGradient : BaseTextureCommand, IMTextureCommand {
    private MagickImage layer;

    public void Prepare() {
      MarkStart();
      MagickReadSettings readSettings = new MagickReadSettings() {
        Width = vars.imgSizeScaled,
        Height = vars.imgSizeScaled,
      };
      readSettings.SetDefine("gradient:direction", "south");
      Color baseColor = vars.COLOR(LPK.TexBaseColor);
      Color darkerColor = GradientDarkColor(baseColor);
      layer = new MagickImage("gradient:" + baseColor.ToHex() + "-" + darkerColor.ToHex(), readSettings);
      MarkEnd();
    }

    public void Composite(MagickImage image) {
      image.Composite(layer, CompositeOperator.SrcOver);
      layer.Dispose();
    }

    public static Color GradientDarkColor(Color baseColor) => baseColor.Darker(0.35f);
  }
}
