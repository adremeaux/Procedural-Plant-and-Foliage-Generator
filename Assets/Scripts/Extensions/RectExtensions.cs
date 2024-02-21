using UnityEngine;

namespace BionicWombat {
  public static class RectExtensions {
    public static RectInt MirrorX(this RectInt r, int width) {
      return new RectInt(width - r.position.x - r.size.x - (r.size.x < 0 ? 2 : 0),
                         r.position.y,
                         r.size.x,
                         r.size.y);
    }

    public static RectInt MirrorY(this RectInt r, int height) {
      return new RectInt(r.position.x,
                         height - r.position.y - r.size.y - (r.size.y < 0 ? 2 : 0),
                         r.size.x,
                         r.size.y);
    }

    public static Rect WithSize(this Rect r, float w, float h) =>
      new Rect(r.position, new Vector2(w, h));

    public static Rect WithWidth(this Rect r, float w) => WithSize(r, w, r.size.y);

    public static Rect WithPos(this Rect r, float x, float y) =>
      new Rect(new Vector2(x, y), r.size);

    public static Rect WithSize(this Rect r, float s) => r.WithSize(s, s);
  }

  public static class UVRect {
    public static Rect Full = new Rect(0f, 0f, 1f, 1f);
    public static Rect Offset(Vector2Int offsetIndex, int gridSize) => Offset(offsetIndex, gridSize, gridSize);
    public static Rect Offset(Vector2Int offsetIndex, int gridSizeX, int gridSizeY) {
      float divisorX = 1f / (float)gridSizeX;
      float divisorY = 1f / (float)gridSizeY;
      return new Rect(new Vector2((float)offsetIndex.x * divisorX, 1f - (divisorY * ((float)offsetIndex.y + 1f))),
                      new Vector2(1f / (float)gridSizeX, 1f / (float)gridSizeY));
    }
  }

}
