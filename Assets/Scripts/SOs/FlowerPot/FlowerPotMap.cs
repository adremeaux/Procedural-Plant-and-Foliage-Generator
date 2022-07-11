using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BionicWombat {
[CreateAssetMenu(fileName = "FlowerPotMap_", menuName = "SOs/FlowerPotMap", order = 72)]
public class FlowerPotMap : ScriptableObject {
  [SerializeField]
  protected List<FlowerPotData> list;
  protected Dictionary<FlowerPotType, FlowerPotData> dict;

  private static FlowerPotMap instance = null;

  private void Configure() {
    dict = new Dictionary<FlowerPotType, FlowerPotData>();
    foreach (FlowerPotData d in list)
      dict[d.type] = d;
  }

  public static FlowerPotData GetFlowerPotData(FlowerPotType type) {
    if (instance == null) {
      instance = Resources.Load<FlowerPotMap>("SOs/Data/FlowerPotMap_asset");
      instance.Configure();
    }
    if (!instance.dict.ContainsKey(type)) instance.Configure();
    return instance.dict[type];
  }
}
}