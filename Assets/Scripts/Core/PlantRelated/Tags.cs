using System;

namespace BionicWombat {
public class Tags {
  public static string Plant = "Plant";
}

[Flags]
public enum LightLayers : uint {
  Default = 1,
  Plant = 2,
}


}