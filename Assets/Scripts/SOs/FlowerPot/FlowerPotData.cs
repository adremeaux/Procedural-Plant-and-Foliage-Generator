using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BionicWombat {
[CreateAssetMenu(fileName = "FlowerPotData_", menuName = "SOs/FlowerPotData", order = 71)]
public class FlowerPotData : ScriptableObject {
  public FlowerPotType type;
  public GameObject modelPrefab;
  public float yAttachmentPos;
}

}