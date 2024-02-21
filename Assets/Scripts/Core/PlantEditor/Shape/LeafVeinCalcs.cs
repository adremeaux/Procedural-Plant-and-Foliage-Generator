using UnityEngine;

namespace BionicWombat {
  public struct LeafVeinCalcs {
    public Vector2 origin;
    public Vector2 tip;
    public Vector2 apex;
    public float apexPos;
    public float span;

    public LeafVeinCalcs(Vector2 origin, Vector2 tip, Vector2 apex, float apexPos) {
      this.origin = origin;
      this.tip = tip;
      this.apex = apex;
      this.apexPos = apexPos;
      span = origin.y - tip.y;
    }
  }

}
