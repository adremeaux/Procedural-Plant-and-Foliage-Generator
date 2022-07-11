using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace BionicWombat {
public enum PlantCollection {
  Classic,
  Temporary,
  User,
}

[Serializable]
public struct PresetCollection {
  [SerializeField] public PlantCollection collection;
  [SerializeField] public string[] plantNames;
  public string resourcesPath => collection + "/";

  public PresetCollection(PlantCollection collectionName, string[] plantNames) {
    this.collection = collectionName;
    this.plantNames = plantNames.Copy();
  }

  public void AddToCollection(params string[] morePlants) {
    List<string> l = plantNames.ToList();
    foreach (string s in morePlants)
      if (!l.Contains(s))
        l.Add(s);
    plantNames = l.ToArray();
  }

  public IEnumerable<(string, TextureType, string)> AllFiles() {
    foreach (string name in plantNames)
      foreach (TextureType type in Enum.GetValues(typeof(TextureType)))
        foreach (string extension in StorageManager.Extensions)
          yield return (name, type, extension);
  }

  public override string ToString() {
    return collection + ": " + plantNames.ToLog();
  }
}
}