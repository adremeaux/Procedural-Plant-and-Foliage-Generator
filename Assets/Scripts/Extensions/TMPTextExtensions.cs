using System.Linq;
using TMPro;
using UnityEngine;

namespace BionicWombat {
  public static class TMPTextExtensions {
    public static void AutosizeHeight(this TMP_Text t) => t.rectTransform.SetHeight(
      t.GetPreferredValues(t.text, t.rectTransform.sizeDelta.x, 100000f).y);
  }
}
