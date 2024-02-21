using System.Linq;
using UnityEngine;

namespace BionicWombat {
  public static class RectTransformExtensions {
    public static void SetWidth(this RectTransform rt, float width) => rt.sizeDelta = rt.sizeDelta.WithX(width);
    public static void SetHeight(this RectTransform rt, float height) => rt.sizeDelta = rt.sizeDelta.WithY(height);
    public static void SetSize(this RectTransform rt, Vector2 size) => rt.sizeDelta = size;
    public static float Bottom(this RectTransform rt) => rt.transform.localPosition.y - rt.sizeDelta.y;
  }
}
