using ImageMagick;
using UnityEngine;

namespace BionicWombat {
  //load in at half rez!
  public class IMTextureCmdDrawCellsOverlay : BaseTextureCommand, IMTextureCommand {
    private MagickImage tile;
    private int offset;

    public void Prepare() {
      MarkStart();
      const float rotate = 10;
      float rotRad = rotate * Polar.DegToRad;
      float x1 = (float)vars.imgSize * Mathf.Cos(rotRad);
      float x2 = (float)vars.imgSize * Mathf.Sin(rotRad);
      int toCoverSize = (int)(x1 + x2);
      float xx1 = (float)toCoverSize * Mathf.Cos(rotRad);
      float xx2 = (float)toCoverSize * Mathf.Sin(rotRad);
      int rotatedSize = (int)(xx1 + xx2);
      offset = (rotatedSize - vars.imgSize) / 2;
      offset /= vars.downsample;
      tile = new MagickImage("tile:" + TextureStorageManager.StreamingAssetsPath + "cells.png", new MagickReadSettings() {
        Width = toCoverSize,
        Height = toCoverSize,
      });
      tile.Alpha(AlphaOption.Set);
      tile.BackgroundColor = MagickColors.None;
      tile.Evaluate(Channels.Alpha, EvaluateOperator.Set, 80);
      tile.FilterType = FilterType.Lanczos2Sharp;
      int scaleSize = vars.imgSizeScaled + offset * 2;
      tile.Scale(scaleSize, scaleSize);
      tile.Rotate(rotate);
      MarkEnd();
    }

    public void Composite(MagickImage image) {
      image.Composite(tile, -offset, -offset, CompositeOperator.SoftLight);
      tile.Dispose();
    }
  }
}
