using System.Collections.Generic;
using System.Linq;
// using SpringBonesTool;
using Unity.Collections;
using UnityEngine;
using static Asserts;

namespace BionicWombat {
  public class BonesDebugger : MonoBehaviour {
    public void Print() {
      MiscCommands.ClearConsole();
      var mr = GetComponent<SkinnedMeshRenderer>();
      Debug.Log(mr.sharedMesh.GetBonesPerVertex().ToArray().ToLogGrouped(4));
      LogWeights(mr.sharedMesh.GetAllBoneWeights().ToArray(), 4);
    }

    private void LogWeights(BoneWeight1[] arr, int group) {
      string s = "";

      foreach ((BoneWeight1 bw, int idx) in arr.WithIndex()) {
        s += $"[{idx}] {bw.boneIndex}: {bw.weight}";
        if ((idx + 1) % group != 0) s += "  ---  ";
        if ((idx + 1) % group == 0) {
          Debug.Log(s);
          s = "";
        }
      }
      Debug.Log(s);
    }
  }
}
