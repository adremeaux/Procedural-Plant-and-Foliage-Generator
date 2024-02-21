using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.UnityConverters;
using UnityEngine;

namespace BionicWombat.BWSaving {
  public static class PresetManager {
    private static string PlantsSaveDirectory = Application.streamingAssetsPath + "/Plants/SavedPlants/";
    public static string StarterPlantsSaveDirectory = PlantsSaveDirectory + "StarterBases/";
    private static string CacheSuffix = "_cache.json";
    #region Plant Management

    public static void SavePlant(SavedPlant savedPlant, bool isStarterPlant = false) {
      DebugBW.Log("SavePlant: " + savedPlant.name + "_" + savedPlant.uniqueID, LColor.lightblue);
      string dir = PlantsSaveDirectory;
      if (isStarterPlant) dir = StarterPlantsSaveDirectory;
      Serialize<SavedPlant>(savedPlant, dir, PlantDataManager.GetSaveName(savedPlant.name, savedPlant.uniqueID));
    }

    public static (LeafParamDict fields, SavedPlant savedPlant) LoadPlant(PlantIndexEntry entry) {
      SavedPlant p = Deserialize<SavedPlant>(PlantsSaveDirectory,
        PlantDataManager.GetSaveName(entry));
      if (p == null) return (null, null);
      return CleanPreset(p, entry.uniqueID);
    }

    public static (LeafParamDict fields, SavedPlant savedPlant) LoadStarterPlant(PlantIndexEntry entry) {
      if (!Asserts.AssertWarning(entry.IsNotDefault(), "LoadStarterPlant default PlantIndexEntry")) return (null, default(SavedPlant));
      SavedPlant p = Deserialize<SavedPlant>(StarterPlantsSaveDirectory,
        PlantDataManager.GetSaveName(entry));
      return CleanPreset(p, entry.uniqueID, true);
    }

    public static void CachePlantToDisk(CachedPlant cachedPlantData, string hash) {
      string dir = PlantsSaveDirectory;
      string json = JsonConvert.SerializeObject(cachedPlantData, Formatting.None, new JsonSerializerSettings {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        Converters = GetConverters(),
      });
      json = "//" + hash + "\n" + json;
      string path = System.IO.Path.Combine(dir,
        PlantDataManager.GetSaveName(cachedPlantData.name, cachedPlantData.uniqueID) + CacheSuffix);
      File.WriteAllText(path, json);
    }

    public static string GetCachedPlantHash(PlantIndexEntry entry) {
      string dir = PlantsSaveDirectory;
      string path = System.IO.Path.Combine(dir, PlantDataManager.GetSaveName(entry) + CacheSuffix);
      if (File.Exists(path)) {
        string line1 = File.ReadLines(path).First();
        if (line1.Length < 100 && line1[0] == '/') return line1.Substring(2);
      }
      return "unhashed";
    }

    public static CachedPlant LoadCachedPlant(PlantIndexEntry entry) {
      string dir = PlantsSaveDirectory;
      string path = System.IO.Path.Combine(dir, PlantDataManager.GetSaveName(entry) + CacheSuffix);
      string data = File.ReadAllText(path);
      try {
        CachedPlant c = JsonConvert.DeserializeObject<CachedPlant>(data, GetConverters());
        return c;
      } catch (Exception e) {
        Debug.LogWarning("LoadCachedPlant error: " + e);
      }
      return default(CachedPlant);
    }

    public static bool CacheExistsForPlant(PlantIndexEntry entry) {
      string dir = PlantsSaveDirectory;
      string path = System.IO.Path.Combine(dir, PlantDataManager.GetSaveName(entry) + CacheSuffix);
      return File.Exists(path);
    }

    public static void RemovePlants(PlantIndexEntry[] entries) {
      foreach (PlantIndexEntry entry in entries) {
        string path = FullPath(PlantsSaveDirectory, PlantDataManager.GetSaveName(entry));
        if (File.Exists(path)) File.Delete(path);
      }
    }

    public static PresetCollection GetCollection_DEPRECATED(PlantCollection collection) {
      string StreamingAssetsPath = Application.streamingAssetsPath + "/Presets/LeafParamPresets/";
      PresetCollection pc = Deserialize<PresetCollection>(StreamingAssetsPath, collection.ToString());
      Debug.Log("pc: " + pc);
      return pc;
    }

    #endregion

    #region Private

    private static (LeafParamDict fields, SavedPlant savedPlant) CleanPreset(
        SavedPlant savedPlant, string uniqueID, bool isStarterPlant = false) {
      if (savedPlant == null) {
        Debug.LogError("ClearPreset null preset");
        return (null, null);
      }
      (LeafParamDict filledIn, bool dirty) = PresetHelpers.Rebuild(savedPlant.leafParamsList, savedPlant);
      if (!savedPlant.uniqueID.HasLength()) {
        dirty = true;
        Debug.Log("Preset " + savedPlant.name + " is missing uniqueID");
        savedPlant.uniqueID = uniqueID;
      }
      if (savedPlant.uniqueID != uniqueID) {
        Debug.LogWarning("xml uniqueID does not match plantdata uniqueID: " + savedPlant.uniqueID + " | " + uniqueID);
      }
      if (savedPlant.seed == 0) {
        savedPlant.seed = Math.Abs(Guid.NewGuid().GetHashCode());
        dirty = true;
        Debug.Log("Setting new seed " + savedPlant.seed + " on " + savedPlant.name);
      }
      LeafParamMigrator.PerformFixedMigrations(savedPlant);
      SavedPlant savedPlantCopy = new SavedPlant(savedPlant.name, savedPlant.uniqueID, filledIn, savedPlant.seed, SavedPlant.VersionNumber);
      if (dirty) {
        Debug.Log("Rewriting preset " + savedPlant.name + " due to dirty load (id " + savedPlant.uniqueID + ")");
        SavePlant(savedPlantCopy, isStarterPlant);
      }
      return (filledIn, savedPlantCopy);
    }

    public static void Serialize<T>(object obj, string path, string name) {
      string fullPath = FullPath(path, name);
      Debug.Log("Serializing " + name + " at " + path + " | " + obj);
      XmlSerializer serializer = new XmlSerializer(typeof(T));
      StreamWriter writer = new StreamWriter(fullPath);
      serializer.Serialize(writer.BaseStream, obj);
      writer.Close();
    }

    public static T Deserialize<T>(string path, string name) {
      string fullPath = FullPath(path, name);
      if (!File.Exists(fullPath)) {
        Debug.LogWarning("Deserialize " + name + " does not exist at path: " + fullPath);
        return default(T);
      }
      Debug.Log("Deserialize: " + fullPath);
      XmlSerializer serializer = new XmlSerializer(typeof(T));
      FileStream stream = new FileStream(fullPath, FileMode.Open);

      var t = (T)serializer.Deserialize(stream);
      stream.Close();
      return t;
    }

    private static string FullPath(string path, string name, string extension = "xml") =>
      path + name + "." + extension;

    private static void CreateDirectoryIfMissing() {
      if (!Directory.Exists(PlantsSaveDirectory)) {
        Debug.Log("Creating new directory at path: " + PlantsSaveDirectory);
        Directory.CreateDirectory(PlantsSaveDirectory);
      }
    }

    public static void SweepOrphans(string[] existingNames) {
      string[] files = Directory.GetFiles(PlantsSaveDirectory);
      Debug.Log("SweepOrphans existing: " + existingNames.ToLog());
      foreach (string file in files) {
        string fileName = Path.GetFileNameWithoutExtension(file);
        fileName = Path.GetFileNameWithoutExtension(fileName); //.xml.meta
        int cachedNamePos = fileName.IndexOf("_cache");
        if (cachedNamePos != -1) fileName = fileName.Substring(0, cachedNamePos);
        bool contains = existingNames.Contains(fileName);
        if (!contains) {
          DebugBW.Log("Deleting xml orphan " + fileName, LColor.orange);
          File.Delete(file);
        }
      }
    }

    private static JsonConverter[] GetConverters() => new JsonConverter[] {
      new Vector2Converter(),
      new Vector3Converter(),
      new QuaternionConverter(),
      new Matrix4x4Converter(),
    };

    public static string SavedPlantHash(SavedPlant plant) {
      XmlSerializer serializer = new XmlSerializer(typeof(SavedPlant));
      string s = "";
      using (StringWriter writer = new StringWriter()) {
        serializer.Serialize(writer, plant);
        s = writer.ToString();
      }

      using (SHA256 sha256 = SHA256.Create()) {
        byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(s));
        return Convert.ToBase64String(hashBytes);
      }
    }

    #endregion
  }
}
