using System;

namespace BionicWombat {
  public class Tags {
    public static string Plant = "Plant";
    public static string Effect = "Effect";
  }

  [Flags]
  public enum LightLayers : uint {
    Default = 1,
    Plant = 2,
    PlantViewer = 4,
    Inventory = 8,
    Vendor = 16,
    LootBox = 32,
  }


}
