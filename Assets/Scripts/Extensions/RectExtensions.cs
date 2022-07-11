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
}

}