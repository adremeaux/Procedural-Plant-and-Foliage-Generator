using System;
using UnityEngine;

namespace BionicWombat {
  public static class BoundsExtensions {
    public static float Left(this Bounds b) {
      return b.min.x;
    }

    public static float Right(this Bounds b) {
      return b.max.x;
    }

    public static float Top(this Bounds b) {
      return b.max.y;
    }

    public static float Bottom(this Bounds b) {
      return b.min.y;
    }

    public static Vector2 TopLeft(this Bounds b) {
      return b.center + b.extents.MultX(-1f);
    }

    public static Vector2 TopRight(this Bounds b) {
      return b.center + b.extents;
    }

    public static Vector2 BottomRight(this Bounds b) {
      return b.center + b.extents.MultY(-1f);
    }

    public static Vector2 BottomLeft(this Bounds b) {
      return b.center - b.extents;
    }

    public static Bounds GetBoundsRecursive(this GameObject obj, params string[] ignoreTags) {
      Bounds bounds = new Bounds();
      Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
      if (renderers.Length > 0) {
        //Find first enabled renderer to start encapsulate from it
        foreach (Renderer renderer in renderers) {
          if (renderer.enabled && Array.IndexOf(ignoreTags, renderer.tag) == -1) {
            bounds = renderer.bounds;
            break;
          }
        }
        //Encapsulate for all renderers
        foreach (Renderer renderer in renderers) {
          if (renderer.enabled && Array.IndexOf(ignoreTags, renderer.tag) == -1) {
            bounds.Encapsulate(renderer.bounds);
          }
        }
      }
      return bounds;
    }

  }

}
