using System.Collections.Generic;
using ImageMagick;
using UnityEngine;
using static BionicWombat.PointDExtensions;

namespace BionicWombat {
  public class IMTextureCmdDrawPrimaryVeinsNormals : BaseTextureCommand, IMTextureCommand, IMPixelCommand {
    private List<PixelFillCommand> fillCommands;
    private bool isHeightMap;

    public IMTextureCmdDrawPrimaryVeinsNormals(bool heightMap = false) {
      this.isHeightMap = heightMap;
    }

    public void Prepare() {
      MarkStart();
      fillCommands = new List<PixelFillCommand>();
      if (isHeightMap) return;

      foreach (IMRenderableVein v in vars.rendVeins.Reversed()) {
        if (v.veinPoly.Length <= 2) continue;
        // if (v.vein.type != LeafVeinType.Midrib) continue;

        List<IMRenderableVeinPoly> polysList = v.GenPolysList(v.veinPolyNormalWidth);
        foreach (IMRenderableVeinPoly poly in polysList) {
          if (poly.points.Length < 3) continue;
          else if (poly.points.Length == 3) {
            Debug.LogWarning("WARNING 3 points WARNING");
            continue;
          }

          Rect extents = GetExtents(poly.points);
          Vector2[] newPoints = RearrangeToVec(poly.points);
          if (extents.width <= 0 || extents.height <= 0) continue;

          PixelFillCommand p1 = new PixelFillCommand();
          float depth = LeafVein.IsPrimaryType(v.vein.type) ? vars.VAL(LPK.NormalMidribDepth) : vars.VAL(LPK.NormalSecondaryDepth);
          depth *= depth; //soften it a bit
          (p1.colors, p1.rect) = NormalDrawing.DrawQuad(newPoints, depth, 1f - vars.VAL(LPK.NormalVeinSmooth));
          p1.useAlpha = true;
          fillCommands.Add(p1);
        }
      }
      MarkEnd();
    }

    public void Composite(MagickImage image) { }

    public PixelFillCommand[] GetPixelFillCommands() {
      return fillCommands.ToArray();
    }
  }
}
