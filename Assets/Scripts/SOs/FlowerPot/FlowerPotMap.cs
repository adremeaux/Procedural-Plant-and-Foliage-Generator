using System.Collections.Generic;
using UnityEngine;

namespace BionicWombat {
  [CreateAssetMenu(fileName = "FlowerPotMap_", menuName = "SOs/FlowerPotMap", order = 72)]
  public class FlowerPotMap : ScriptableObject {
    [SerializeField] public List<FlowerPotData> list;
    protected Dictionary<FlowerPotType, FlowerPotData> dict;

    private static FlowerPotMap instance = null;

    private void Configure() {
      dict = new Dictionary<FlowerPotType, FlowerPotData>();
      foreach (FlowerPotData d in list) {
        dict[d.type] = d;
      }
    }

    private static void CheckInstance() {
      if (instance == null) {
        instance = Resources.Load<FlowerPotMap>("SOs/Data/FlowerPotList");
        instance.Configure();
      }
    }

    public static FlowerPotData GetFlowerPotData(FlowerPotType type) {
      CheckInstance();
      return instance.dict[type];
    }

    public static FlowerPotData[] GetFlowerPotsData() {
      CheckInstance();
      return instance.list.ToArray();
    }
  }
}
