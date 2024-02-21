using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BionicWombat {
  public static class PlantTools {
    public static Plant[] GetPlants() =>
      GameObject.FindGameObjectsWithTag("Plant").Select<GameObject, Plant>(go => go.GetComponent<Plant>()).ToArray();

    // #if UNITY_EDITOR
    //   [MenuItem("Window/Fix Plant Materials %#m")] // CTRL + SHIFT + M
    //   public static void FixAllMaterials() =>
    //     GetPlants().Each<Plant>(delegate (Plant p) { p.ResetMats(); return ""; });
    // #endif
  }
}
