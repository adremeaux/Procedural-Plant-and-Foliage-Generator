using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using ImageMagick;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

namespace BionicWombat {
[InitializeOnLoadAttribute]
#endif
public static class DataManager {

  static DataManager() {
#if UNITY_EDITOR
    EditorApplication.playModeStateChanged += PlayModeStateChanged;
#endif
  }

  public static void WriteIMTexture(MagickImage image, string name, TextureType type, PlantCollection collection) =>
    StorageManager.WriteIMTexture(image, name, type, collection);


  public static void WriteTexture(Texture2D tex, string name, TextureType type, PlantCollection collection) =>
    StorageManager.WriteTexture(tex, name, type, collection);

  public static Texture2D GetTexture(string leafName, TextureType type, PlantCollection collection) =>
    StorageManager.GetTexture(leafName, type, collection);

  public static void MigrateTexturesToPermanent(string leafName, PlantCollection collection) =>
    MigrateTexturesBetweenCollections(leafName, PlantCollection.Temporary, collection);

  public static void MigrateTexturesBetweenCollections(string leafName, PlantCollection from, PlantCollection to) {
    StorageManager.MigrateTexturesBetweenCollections(leafName, from, to);
    PresetManager.MigrateTexturesBetweenCollections(leafName, from, to);
  }

  public static void SavePreset(LeafParamPreset preset, PlantCollection collection) {
    PresetManager.SavePreset(preset, collection);
  }

  public static void DeleteCollection(PlantCollection collection) {
    PresetCollection c = PresetManager.GetCollection(collection);
    if (c.plantNames == null) {
      Debug.LogWarning("Cannot sweep: collection is not indexed");
      return;
    }

    StorageManager.Sweep(c);
    PresetManager.DeleteCollection(collection);
  }

#if UNITY_EDITOR
  [MenuItem("Assets/Reindex Collections")]
#endif
  private static void ReindexCollections() {
    PresetManager.ReindexCollections();
    PresetCollection[] recovered = StorageManager.RecoverCollections();
    foreach (PresetCollection c in recovered)
      PresetManager.AddToCollection(c.collection, c.plantNames);
  }

#if UNITY_EDITOR
  [MenuItem("Assets/Sweep Temporary Textures")]
#endif
  public static void Sweep() {
    DeleteCollection(PlantCollection.Temporary);
  }

  public static List<TextureType> MissingTexturesForLeaf(string name, PlantCollection collection) =>
    StorageManager.MissingTexturesForLeaf(name, collection);

#if UNITY_EDITOR
  private static void PlayModeStateChanged(PlayModeStateChange state) {
    Debug.Log("Play Mode State Change: " + state);
    if (state == PlayModeStateChange.ExitingPlayMode) Sweep();
  }
#endif
}

}