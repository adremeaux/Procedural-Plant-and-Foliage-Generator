using System;
using UnityEngine;

namespace BionicWombat {
  [Serializable]
  public struct FlowerPotData {
    public FlowerPotType type;
    public string displayName;
    public GameObject modelPrefab;
    public Sprite sprite;
    public float yAttachmentPos;
    public float shelfScale;
    public int cost;

    public override string ToString() {
      return "[FlowerPotData] type: " + type + " | displayName: " + displayName;
    }
  }
  //1.57, 1.5, 1.6, 1.6
}
