using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BionicWombat {
  public static class PresetManager {
    public static string StreamingAssetsPath = Application.streamingAssetsPath + "/Presets/LeafParamPresets/";
    private static string IndexName = "Index";

    private static void CreateCollection(PlantCollection collection, params string[] names) {
      Debug.Log("Creating new collection " + collection);
      PresetCollection c = new PresetCollection(collection, names);
      Serialize<PresetCollection>(c, StreamingAssetsPath, collection.ToString());
      ReindexCollections();
    }

    public static void AddToCollection(PlantCollection collection, params string[] names) {
      Debug.Log("Collection names: " + GetCollectionNames().ToLog());
      if (!GetCollectionNames().Contains(collection.ToString())) {
        CreateCollection(collection, names);
      } else {
        PresetCollection c = GetCollection(collection);
        c.AddToCollection(names);
        Debug.Log("Adding " + names.ToLog() + " to " + collection + ", full list: " + c.plantNames.ToLog());
        Serialize<PresetCollection>(c, StreamingAssetsPath, collection.ToString());
      }
    }

    public static void DeleteCollection(PlantCollection collection) {
      string path = StreamingAssetsPath + collection.ToString() + ".xml";
      if (File.Exists(path)) File.Delete(path);
      else Debug.LogWarning("Tried to delete non-existent collection " + collection + " at path " + path);
      if (File.Exists(path + ".meta")) File.Delete(path + ".meta");

      string dirPath = StreamingAssetsPath + collection.ToString() + "/";
      if (Directory.Exists(dirPath)) {
        foreach (string s in Directory.GetFiles(dirPath)) File.Delete(s);
      }
      ReindexCollections();
    }

    public static void MigrateTexturesBetweenCollections(string leafName, PlantCollection from, PlantCollection to) {
      PresetCollection c = PresetManager.GetCollection(from);
      DeleteCollection(from);
      AddToCollection(to, c.plantNames);
      ReindexCollections();
    }

    public static PresetCollection GetCollection(PlantCollection collection) =>
      Deserialize<PresetCollection>(StreamingAssetsPath, collection.ToString());

    public static string[] GetCollectionNames() => Deserialize<string[]>(StreamingAssetsPath, IndexName);

    private static void CreateDirectoryIfMissing(PlantCollection collection) {
      string path = GetDirectory(collection);
      if (!Directory.Exists(path)) {
        Debug.Log("Creating new directory at path: " + path);
        Directory.CreateDirectory(path);
      }
    }

    public static void SavePreset(LeafParamPreset preset, PlantCollection collection) {
      CreateDirectoryIfMissing(collection);
      Serialize<LeafParamPreset>(preset, GetDirectory(collection), preset.name);
    }

    public static LeafParamDict LoadPreset(string name, PlantCollection collection) {
      LeafParamPreset p = Deserialize<LeafParamPreset>(GetDirectory(collection), name);
      return CleanPreset(p, collection);
    }

    private static string GetDirectory(PlantCollection collection) => StreamingAssetsPath + collection.ToString() + "/";

    private static LeafParamDict CleanPreset(LeafParamPreset preset, PlantCollection collection) {
      (LeafParamDict filledIn, bool dirty) = LeafParamPreset.Rebuild(preset.leafParams);
      if (dirty) {
        SavePreset(new LeafParamPreset(preset.name, filledIn), collection);
        Debug.Log("Rewriting preset " + preset.name + " due to dirty load");
      }
      return filledIn;
    }

    public static void Serialize<T>(object obj, string path, string name) {
      string fullPath = path + name + ".xml";
      XmlSerializer serializer = new XmlSerializer(typeof(T));
      StreamWriter writer = new StreamWriter(fullPath);
      serializer.Serialize(writer.BaseStream, obj);
      writer.Close();
    }

    public static T Deserialize<T>(string path, string name) {
      string fullPath = path + name + ".xml";
      if (!File.Exists(fullPath)) return default(T);
      // Debug.Log("Deserialize: " + path + name);
      XmlSerializer serializer = new XmlSerializer(typeof(T));
      FileStream stream = new FileStream(fullPath, FileMode.Open);

      var t = (T)serializer.Deserialize(stream);
      stream.Close();
      return t;
    }

    public static void ReindexCollections() {
      List<string> strings = Directory.GetFiles(StreamingAssetsPath).ToList();
      List<string> newStrings = new List<string>();
      for (int i = 0; i < strings.Count; i++) {
        if (Path.GetExtension(strings[i]) == ".meta" ||
            Path.GetFileName(strings[i]) == IndexName + ".xml") continue; //skip .meta and Index.xml
        newStrings.Add(Path.GetFileNameWithoutExtension(strings[i]));
      }
      Serialize<string[]>(newStrings.ToArray(), StreamingAssetsPath, IndexName);
    }
  }

}