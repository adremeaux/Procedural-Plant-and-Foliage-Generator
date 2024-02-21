using System;
using System.Collections.Generic;
using ImageMagick;
using UnityEngine;
using static BionicWombat.PointDExtensions;

namespace BionicWombat {
  public class IMTextureCmdDrawPuffyNormals : BaseTextureCommand, IMTextureCommand, IMPixelCommand {
    private List<PixelFillCommand> fillCommands;
    private MagickImage _line;
    private bool isHeightMap;

    public IMTextureCmdDrawPuffyNormals(bool heightMap = false) {
      this.isHeightMap = heightMap;
    }

    public void Prepare() {
      MarkStart();
      fillCommands = new List<PixelFillCommand>();
      List<PointD[]> puffs = vars.puffies;

      _line = new MagickImage(MagickColors.None, vars.imgSizeScaled, vars.imgSizeScaled);
      Color[] colors = new Color[] { Color.red, Color.green, Color.yellow, Color.blue, Color.magenta };
      int idx = -1;
      string[] toRender = vars.randomBS.Split(",", StringSplitOptions.RemoveEmptyEntries);
      foreach (PointD[] points in puffs) {
        idx++;
        // if (toRender.Length > 0 && !toRender.Contains("" + idx)) continue;
        // _line.Draw(new DrawablePolygon(points),
        //            new DrawableFillColor(MC(colors[idx % colors.Length])),
        //            new DrawableFillOpacity(new Percentage(50)),
        //            new DrawableStrokeColor(MC(Color.cyan)),
        //            new DrawableStrokeWidth(3f)
        //             );
        // continue;

        Rect extents = GetExtents(points);
        PixelFillCommand p = new PixelFillCommand();
        (p.colors, p.rect) = NormalDrawing.DrawPuffy(points.AsVectors(),
          1f - vars.VAL(LPK.NormalPuffySmooth),
          1f - vars.VAL(LPK.NormalPuffyPlateauClamp),
          vars.VAL(LPK.NormalPuffyStrength),
          2,
          isHeightMap);
        p.useAlpha = true;
        fillCommands.Add(p);
      }
      MarkEnd();
    }

    public void Composite(MagickImage image) {
      image.Composite(_line, CompositeOperator.SrcOver);
      _line.Dispose();
    }

    public PixelFillCommand[] GetPixelFillCommands() {
      return fillCommands.ToArray();
    }
  }

}
