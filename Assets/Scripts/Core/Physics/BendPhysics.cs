using System.Collections.Generic;
using System.Linq;
// using SpringBonesTool;
using Unity.Collections;
using UnityEngine;
using static Asserts;

namespace BionicWombat {
  public class BendPhysics : MonoBehaviour {
    private Transform[] bones;
    private Transform[] leafBones;
    private Matrix4x4[] bindPoses;
    private Matrix4x4[] leafBindPoses;

    private IStem stem;
    private Vector3[] renderedPoints;
    private Vector3[] originalBonePoints;
    private float[] lengths;
    private float[] leafLengths;
    private float[] leafYPositions;

    // static float GravDefault = JiggleController.GravDefault;
    // static float RIDefault = JiggleController.RotInertiaDefault;
    // static float SSDefault = JiggleController.StrengthDefault;
    // static float DampDefault = JiggleController.DampDefault;
    // static float WindStrDefault = JiggleController.WindStrDefault;

    public void CreateBones(IStem stem, LeafBundle leafBundle, int baseLineSteps, LeafFactoryData lfd, float descale, string name) {
      try {
        if (bones != null) {
          foreach (Transform b in bones) {
            if (Application.isPlaying)
              Destroy(b.gameObject);
            else
              DestroyImmediate(b.gameObject);
          }
        }
      } catch { }

      // MiscCommands.ClearConsole();
      this.stem = stem;

      CreateStemBones(stem, leafBundle, baseLineSteps, name);
      //CreateLeafBones(leafBundle, lfd, descale);

      // CreateJigglies();
    }

    public void SetCachedBones(CachedTransform[] cachedBones,
        SkinnedMeshRenderer rend, Mesh stemMesh) {
      int steps = cachedBones.Length;
      this.bones = new Transform[steps];
      Transform parentTrans = rend.transform;
      for (int i = 0; i < steps; i++) {
        bones[i] = new GameObject("bone" + i + "_" + name).transform;
        bones[i].parent = parentTrans;
        parentTrans = bones[i];
        bones[i].localPosition = cachedBones[i].pos;
        bones[i].localRotation = cachedBones[i].quat;
      }

      lengths = new float[bones.Length - 1];
      for (int i = 0; i < lengths.Length; i++)
        lengths[i] = Vector3.Distance(bones[i].position, bones[i + 1].position);

      rend.bones = bones;
      rend.rootBone = bones.FirstOrNull();
      rend.sharedMesh = stemMesh;
      // CreateJigglies();
    }

    private void CreateStemBones(IStem stem, LeafBundle leafBundle, int baseLineSteps, string name) {
      Curve3D[] curves = stem.curves.ToArray();
      Vector3[] shape = stem.shape;
      int steps = 4;
      Vector3[] bonePoints = StemRenderer.GetStemPoints(curves, steps, 1f).stemPoints.ToArray();
      steps = bonePoints.Length;
      renderedPoints = StemRenderer.GetStemPoints(curves, baseLineSteps).stemPoints.ToArray();
      originalBonePoints = bonePoints;

      bones = new Transform[steps];
      Transform parentTrans = leafBundle.stemMeshRenderer.transform;
      Vector3 lastPos = Vector3.zero;
      for (int i = 0; i < bones.Length; i++) {
        bones[i] = new GameObject("bone" + i + "_" + name).transform;
        bones[i].parent = parentTrans;
        bones[i].localPosition = bonePoints[i] - lastPos;
        if (i > 0) {
          bones[i - 1].LookAt(bones[i], Vector3.up);
          bones[i - 1].localRotation *= Quaternion.Euler(90f, 0f, 0f);
        }
      }

      //chain everything together and set binds
      bindPoses = new Matrix4x4[steps];
      for (int i = 0; i < bones.Length; i++) {
        bones[i].parent = parentTrans;
        parentTrans = bones[i];
        //TODO: transform.localToWorldMatrix should probably be different
        bindPoses[i] = bones[i].worldToLocalMatrix * transform.localToWorldMatrix;
      }

      lengths = new float[bones.Length - 1];
      for (int i = 0; i < lengths.Length; i++)
        lengths[i] = Vector3.Distance(bones[i].position, bones[i + 1].position);
    }


    private void CreateLeafBones(LeafBundle leafBundle, LeafFactoryData lfd, float descale) {
      try {
        if (leafBones != null) foreach (Transform b in leafBones) DestroyImmediate(b.gameObject);
      } catch { }

      int steps = 4;
      Transform leafTransform = leafBundle.leafMeshRenderer.transform;
      Vector3 basePoint = bones.Last().position;
      LeafCurve tipCurve = LeafShape.GetCurve(LeafCurveType.Tip, lfd.leafShape.curves, LeafShape.LeftyCheck.Right);
      Vector3 endPoint = (tipCurve.p1 * descale) * leafTransform.localScale;
      endPoint += basePoint;
      // DebugBW.Log("tipCurve: " + tipCurve);
      // DebugBW.Log("basePoint: " + basePoint + " | final endPoint: " + endPoint);
      Transform parentTrans = leafBundle.stemMeshRenderer.transform;
      Vector3 lastPos = Vector3.zero;

      leafBones = new Transform[steps];
      leafYPositions = new float[steps];
      for (int i = 0; i < steps; i++) {
        leafBones[i] = new GameObject("leaf_bone" + i).transform;
        leafBones[i].parent = parentTrans;
        parentTrans = leafBones[i];
        leafBones[i].position = Vector3.Lerp(basePoint, endPoint, (float)i / (steps - 1f));
        // DebugBW.Log($"bone{i} position: " + leafBones[i].position + " | lp: " + leafBones[i].localPosition);

        leafYPositions[i] = Vector3.Lerp(Vector3.zero, tipCurve.p1, (float)i / (steps - 1f)).y;
      }

      //rotate the base to fix rotation
      leafBones[0].localRotation = leafTransform.localRotation;

      //break the parentage to set look rotations
      parentTrans = leafBundle.stemMeshRenderer.transform;
      for (int i = 0; i < steps; i++) leafBones[i].parent = parentTrans;

      //reparent to the end of the stem bone, skipping the first
      parentTrans = bones.Last();
      leafBindPoses = new Matrix4x4[steps];
      for (int i = 0; i < steps; i++) {
        if (i < steps - 1) {
          leafBones[i].LookAt(leafBones[i + 1], Vector3.up);
          leafBones[i].localRotation *= Quaternion.Euler(90f, 0f, 0f);
        }
        leafBones[i].parent = parentTrans;
        parentTrans = leafBones[i];
      }

      leafBindPoses[0] = leafBones[0].worldToLocalMatrix * transform.localToWorldMatrix;
      for (int i = 1; i < steps; i++)
        leafBindPoses[i] = leafBones[i].worldToLocalMatrix * bones.Last().localToWorldMatrix;

      leafLengths = new float[leafBones.Length - 1];
      for (int i = 0; i < leafLengths.Length; i++)
        leafLengths[i] = Vector3.Distance(leafBones[i].position, leafBones[i + 1].position);
    }

    public void SetStemMesh(Mesh mesh, SkinnedMeshRenderer rend) {
      if (!Assert(bones != null, "Bones missing")) return;

      Vector3[] shape = stem.shape;
      int vertsPerSegment = shape.Length;
      float segsPerBone = (renderedPoints.Length - 1f) / ((float)bones.Length - 1f);
      Vector3[] verts = mesh.vertices;
      Vector3 segVertPos = Vector3.zero;
      if (!Assert(verts.Length / vertsPerSegment == renderedPoints.Length,
        "Unequal points count: " + verts.Length + " | vertsPerSegment: " + vertsPerSegment +
        " | rendererPoints.Length: " + renderedPoints.Length)) return;

      byte[] bonesPerVertex = new byte[verts.Length];
      // DebugBW.Log("renderedPoints.Length: " + renderedPoints.Length + " | verts.Length: " + verts.Length);
      // DebugBW.Log("verts.Length: " + verts.Length + " | vertsPerSegment: " + vertsPerSegment +
      //   " | segsPerBone: " + segsPerBone);
      for (int i = 0; i < bonesPerVertex.Length; i++)
        bonesPerVertex[i] = (i < vertsPerSegment) ? (byte)1 : (byte)2;
      BoneWeight1[] weights = new BoneWeight1[verts.Length * 2 - vertsPerSegment];

      (float d1, float d2) = (0f, 0f);
      (float w1, float w2) = (0f, 0f);
      int boneIdx = 0;
      Vector3 basePos = originalBonePoints[0];
      int weightIdx = 0;

      // Debug.Log("bones: " + bones.ToList().Select(t => t.localPosition).ToLog());
      // Debug.Log("RPs: " + renderedPoints.ToList().Select(t => t * scaleFactor).ToLog());

      for (int i = 0; i < verts.Length; i++) {
        int rpIdx = i / vertsPerSegment;
        boneIdx = Mathf.FloorToInt(Mathf.Max(rpIdx - 1, 0) / segsPerBone);
        if (i < vertsPerSegment) {
          weights[weightIdx++] = BoneWeight(1f, 0);
          continue;
        } else if (boneIdx >= bones.Length - 1) {
          weights[weightIdx++] = BoneWeight(1f, boneIdx);
          continue;
        }

        //next segment
        if (i % vertsPerSegment == 0) {
          segVertPos = renderedPoints[rpIdx];

          d1 = Vector3.Distance(segVertPos, originalBonePoints[boneIdx] - basePos);
          d2 = Vector3.Distance(segVertPos, originalBonePoints[boneIdx + 1] - basePos);

          // DebugBW.Log(i + "]  boneIdx: " + boneIdx + " | rpIdx: " + rpIdx + " | weightIdx: " + weightIdx);
          // DebugBW.Log(i + " | segVertPos: " + segVertPos + $" | bone{boneIdx}: " +
          //   (originalBonePoints[boneIdx]) + $" | bone{boneIdx + 1}: " + (originalBonePoints[boneIdx + 1]));

          if (d1 + d2 == 0) {
            Debug.LogWarning("BendPhysics div by zero prevented!");
            continue;
          }
          w2 = d1 / (d1 + d2);
          w1 = d2 / (d1 + d2);
          if (w1 > 0.05f) {
            w1 = 1f;
            w2 = 0f;
          }
          // DebugBW.Log("d1: " + d1 + " | d2: " + d2);
          // DebugBW.Log("w1: " + w1 + " | w2: " + w2);
        }

        // DebugBW.Log("weightIdx: " + weightIdx);
        // if (weightIdx < weights.Length) {
        weights[weightIdx++] = BoneWeight(w1, boneIdx);
        weights[weightIdx++] = BoneWeight(w2, boneIdx + 1);
        // }
      }

      // LogWeights(weights, vertsPerSegment);

      var bonesPerVertexArray = new NativeArray<byte>(bonesPerVertex, Allocator.Temp);
      var weightsArray = new NativeArray<BoneWeight1>(weights, Allocator.Temp);

      // Set the bone weights on the mesh
      mesh.SetBoneWeights(bonesPerVertexArray, weightsArray);
      bonesPerVertexArray.Dispose();
      weightsArray.Dispose();

      // Assign the bind poses to the mesh
      mesh.bindposes = bindPoses;

      // Assign the bones and the mesh to the renderer
      rend.bones = bones;
      rend.sharedMesh = mesh;
      rend.rootBone = bones.FirstOrNull();
    }

    public void SetLeafMesh(Mesh leafMesh, SkinnedMeshRenderer leafRend) {
      Vector3[] verts = leafMesh.vertices;
      Debug.Log("verts: " + verts.ToLog());
      DebugBW.Log("leafYPositions: " + leafYPositions.ToLog());
      byte[] bonesPerVertex = new byte[verts.Length];

      for (int i = 0; i < bonesPerVertex.Length; i++) bonesPerVertex[i] = (byte)1;
      BoneWeight1[] weights = new BoneWeight1[verts.Length * 1];

      int j;
      float[] ys = leafYPositions;
      for (int i = 0; i < verts.Length; i++) {
        for (j = ys.Length - 1; j >= 0; j--) {
          Debug.Log(verts[i].y + " <= " + ys[j]);
          if (verts[i].y <= ys[j]) {
            weights[i] = BoneWeight(1f, j);
            DebugBW.Log("weights[i]: " + BoneLog(weights[i]) + " | verts[i].y: " + verts[i].y);
            break;
          }
        }
        if (weights[i].IsDefault()) weights[i] = BoneWeight(1f, 0);
      }
      // LogWeights(weights, 4);

      var bonesPerVertexArray = new NativeArray<byte>(bonesPerVertex, Allocator.Temp);
      var weightsArray = new NativeArray<BoneWeight1>(weights, Allocator.Temp);

      // Set the bone weights on the mesh
      leafMesh.SetBoneWeights(bonesPerVertexArray, weightsArray);
      bonesPerVertexArray.Dispose();
      weightsArray.Dispose();

      // Assign the bind poses to the mesh
      leafMesh.bindposes = leafBindPoses;

      // Assign the bones and the mesh to the renderer
      leafRend.bones = leafBones;
      leafRend.sharedMesh = leafMesh;
      leafRend.rootBone = leafBones.FirstOrNull();
    }

    // private void CreateJigglies() {
    //   int lastIdx = bones.Length - 2;
    //   foreach ((Transform bone, int idx) in bones.WithIndex()) {
    //     if (bone == bones.Last()) break;
    //     bool last = idx == lastIdx;
    //     var jig = bone.gameObject.AddComponent<Jiggle>();
    //     jig.CenterOfMass = new Vector3(0f, lengths[idx] / 2f, 0f);
    //     jig.CenterOfMassInertia = last ? 0.3f : 0.05f;
    //     jig.Gravity = GravDefault;
    //     jig.RotationInertia = RIDefault;
    //     jig.SpringStrength = SSDefault;
    //     jig.Dampening = DampDefault;
    //     jig.GizmoScale = 0.01f;
    //     jig.UseAngleLimit = true;
    //     jig.AngleLimit = last ? 0f : 5f;
    //     jig.AddWind = true;
    //     jig.WindStrength = WindStrDefault;
    //   }
    // }

    // public List<JiggleData> GetJiggleData() {
    //   int lastIdx = bones.Length - 2;
    //   List<JiggleData> jiggleData = new List<JiggleData>();
    //   foreach ((Transform bone, int idx) in bones.WithIndex()) {
    //     if (bone == bones.Last()) break;
    //     bool last = idx == lastIdx;
    //     var jig = bone.GetComponent<Jiggle>();
    //     if (jig)
    //       jiggleData.Add(new JiggleData(jig, last ? BoneType.StemEnd : BoneType.Stem));
    //   }
    //   return jiggleData;
    // }

    // public Jiggle[] GetJiggles() => bones.Select(b => b.GetComponent<Jiggle>()).ToArray();

    public void SetLeafJoint(GameObject leaf) {
      if (!bones.HasLength()) return;
      Transform last = bones.Last();
      var rb = last.gameObject.AddComponent<Rigidbody>();
      rb.useGravity = false;
      rb.isKinematic = true;
      var joint = last.gameObject.AddComponent<FixedJoint>();
      joint.connectedBody = leaf.GetComponent<Rigidbody>();
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

    private string BoneLog(BoneWeight1 bw) => $"{bw.boneIndex}: {bw.weight}";

    private static BoneWeight1 BoneWeight(float weight, int idx) {
      BoneWeight1 b = new BoneWeight1();
      b.weight = weight;
      b.boneIndex = idx;
      return b;
    }

    public void OnDrawGizmos() {
      // if (!bones.HasLength()) return;
      // for (int i = 0; i < bones.Length; i++) {
      //   if (i < bones.Length - 1)
      //     Gizmos.DrawLine(bones[i].position, bones[i + 1].position);
      //   Gizmos.DrawWireSphere(bones[i].position, 0.02f);
      // }
    }
  }
}
