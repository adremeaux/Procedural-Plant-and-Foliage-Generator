using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
  [Serializable]
  public struct CachedPlant {
    public string name;
    public string uniqueID;
    public PlantCollection collection;
    public SerializedMesh leafMesh;
    public SerializedMesh trunkMesh;
    public SerializedMesh[] stemMeshes;
    public LeafStem[] leafStems;
    public ArrangementData[] arrangementData;
    public Vector3[] collisionAdjustment;
    public bool[] hiddenLeaves;
    public LeafFactoryData lfd;
    public CachedTransform[][] bones;
    public int leafInstances;
    public Vector3[] bufferVertDeltas;
    public Vector3[] bufferAdjNormals;

    public CachedPlant(string name, string uniqueID, PlantCollection collection,
        Mesh leafMesh, Mesh trunkMesh, Mesh[] stemMeshes,
        LeafStem[] leafStems, ArrangementData[] arrangementData, Vector3[] collisionAdjustment,
        bool[] hiddenLeaves, LeafFactoryData lfd, Transform[][] bones,
        int leafInstances, Vector3[] bufferVertDeltas, Vector3[] bufferAdjNormals) {
      this.name = name;
      this.uniqueID = uniqueID;
      this.collection = collection;
      this.leafMesh = new SerializedMesh(leafMesh);
      this.trunkMesh = new SerializedMesh(trunkMesh);
      this.stemMeshes = stemMeshes.Select(sm => new SerializedMesh(sm)).ToArray();
      this.leafStems = leafStems;
      this.hiddenLeaves = hiddenLeaves;
      this.arrangementData = arrangementData;
      this.collisionAdjustment = collisionAdjustment;
      this.lfd = lfd;
      this.bones = bones.Select(boneArr =>
        boneArr.Select(bt => new CachedTransform(bt)).ToArray()).ToArray();
      this.leafInstances = leafInstances;
      this.bufferAdjNormals = bufferAdjNormals;
      this.bufferVertDeltas = bufferVertDeltas;
    }

    public override string ToString() {
      return "[CachedPlant] name: " + name + " | leafMesh: " + leafMesh + " | stemMeshes: " + stemMeshes + " | arrangementData: " + arrangementData + " | lfd: " + lfd;
    }
  }

  [Serializable]
  public struct CachedTransform {
    public Vector3 pos;
    public Quaternion quat;

    public CachedTransform(Transform t) {
      pos = t.localPosition;
      quat = t.localRotation;
    }

    public override string ToString() => "[CachedTransform] pos: " + pos + " | quat: " + quat;
  }
}
