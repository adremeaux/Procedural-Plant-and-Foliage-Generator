using ImageMagick;
using UnityEngine;

namespace BionicWombat {
  public class IMTextureCmdDrawColor : BaseTextureCommand, IMTextureCommand {
    private MagickImage layer;
    private Color color;

    public IMTextureCmdDrawColor(Color c) : base() {
      color = c;
    }

    public void Prepare() {
      MarkStart();
      MagickReadSettings readSettings = new MagickReadSettings() {
        Width = vars.imgSizeScaled,
        Height = vars.imgSizeScaled,
      };
      layer = new MagickImage(MC(color), vars.imgSizeScaled, vars.imgSizeScaled);
      MarkEnd();
    }

    public void Composite(MagickImage image) {
      image.Composite(layer, CompositeOperator.SrcOver);
      layer.Dispose();
    }
  }
}
