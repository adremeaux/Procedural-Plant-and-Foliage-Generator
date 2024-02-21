using System.Collections.Generic;
using ImageMagick;
using UnityEngine;

namespace BionicWombat {
  public interface IMTextureCommand {
    public bool enabled { get; set; }
    public void SetVars(IMTextureVars vars, bool logSplits);
    public void Prepare();
    public void Composite(MagickImage image);
  }

  public interface IMPixelCommand {
    public PixelFillCommand[] GetPixelFillCommands();
  }

  public struct PixelFillCommand {
    public Color[] colors;
    public RectInt rect;
    public bool useAlpha;

    //this only works for bottom and right crops!!
    public void CropToRect(RectInt crop) {
      if (crop.width == rect.width && crop.height == rect.height) {
        if (crop.xMin != rect.xMin || crop.yMin != rect.yMin) Debug.LogError("Unexpected crop rect: " + crop + " vs " + rect);
        return;
      }

      List<Color> newColors = new List<Color>();
      for (int y = 0; y < crop.height; y++) {
        for (int x = 0; x < rect.width; x++) {
          if (x < crop.width)
            newColors.Add(colors[y * rect.width + x]);
        }
      }
      colors = newColors.ToArray();
      rect = crop;
    }
  }

}
