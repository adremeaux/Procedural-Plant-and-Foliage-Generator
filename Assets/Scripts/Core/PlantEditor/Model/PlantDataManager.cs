using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BionicWombat.BWSaving;
using ImageMagick;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using static Asserts;

namespace BionicWombat {
  public static class PlantDataManager {

    private static string SaveDirectory = Application.streamingAssetsPath + "/Plants/";
    private static string PlantsSaveDirectory = SaveDirectory + "SavedPlants/";
    private static string savePath;
    private static CollectionSaveData saveData;

    public delegate void DidUpdatePlantIndexData(PlantIndexEntry entry);
    public static event DidUpdatePlantIndexData DidUpdatePlantIndexDataEvent;

    public static string[] GetCollectionNames() => Enum.GetNames(typeof(PlantCollection)).ToArray();
    public static PlantCollection[] GetCollections() => Enum.GetValues(typeof(PlantCollection)).Cast<PlantCollection>().ToArray();
    public static PlantCollection[] GetVisibleCollections() => new PlantCollection[] {
      PlantCollection.Classic,
      PlantCollection.Cool
    };

    public static bool invalidate { get; private set; }
    public static void ClearInvalidate() => invalidate = false;

    //called from ShopSceneController
    public static void StaticAwake() {
      // saveDirectory = System.IO.Path.Combine(Application.persistentDataPath, "saves");
      invalidate = true;
      savePath = System.IO.Path.Combine(SaveDirectory, "plantdata.json");
      DebugBW.Log("PlantData savePath: " + savePath, LColor.brown);

      LoadSaveData();
    }

    private static Dictionary<string, Texture2D> textureCache;
    public static async void PreloadAllTextures(Action callback) {
      //THIS ONLY PRELOADS CLASSIC!
      textureCache = new Dictionary<string, Texture2D>();
      if (!GlobalVars.instance.UseTexturePreloadCache) {
        callback();
        return;
      }

      SplitTimer st = new SplitTimer("Preload");
      st.Start();
      PresetCollection col = GetCollection(PlantCollection.Classic);
      TextureType[] texTypes = Enum.GetValues(typeof(TextureType)).Cast<TextureType>().ToArray();
      int texCount = texTypes.Length;
      int count = col.PlantNames.Length * texCount;
      byte[][] preloadData = new byte[count][];
      st.Split("Prep");
      await Task.Run(() =>
        Parallel.For(0, count, i => {
          int mainIdx = Mathf.FloorToInt((float)i / (float)texCount);
          int texIdx = i % texCount;
          PlantIndexEntry entry = col.plants[mainIdx];
          TextureType type = (TextureType)texIdx;
          string cacheName = CacheName(entry, type);
          preloadData[i] = TextureStorageManager.PreloadTextureData(entry, type, PlantCollection.Classic);
        })
      );
      st.Split("byte data");
      DebugBW.Log("All preload complete", LColor.blue);

      for (int i = 0; i < count; i++) {
        int mainIdx = Mathf.FloorToInt((float)i / (float)texCount);
        int texIdx = i % texCount;
        PlantIndexEntry entry = col.plants[mainIdx];
        TextureType type = (TextureType)texIdx;
        string cacheName = CacheName(entry, type);
        Texture2D tex = TextureStorageManager.LoadTextureFromPreloadData(
          preloadData[i], cacheName);
        textureCache[cacheName] = tex;
      }
      st.Split("big load");
      st.Stop();

      callback();
    }

    public static void SaveBackup(string name) {
      string path = System.IO.Path.Combine(SaveDirectory, name + ".json");
      SavePlantData(saveData, path);
    }

    public static void LoadSaveData(string customName = "") {
      string path = !customName.HasLength() ? savePath :
        System.IO.Path.Combine(SaveDirectory, customName + ".json");

      saveData = LoadPlantSaveData(path);
      if (saveData.IsDefault()) {
        string backupPath = path + ".bak";
        Debug.LogError("Overwriting save data. Creating backup at: " + backupPath);
        if (File.Exists(path)) {
          File.Copy(path, backupPath, true);
        }
      } else {
        SavePlantData(saveData);
      }

      invalidate = true;
    }

    public static string GetSaveName(PlantIndexEntry entry) {
      return GetSaveName(entry.name, entry.uniqueID);
    }

    public static string GetSaveName(string name, string uniqueID) {
      if (name == null || uniqueID == null) return "(GetSaveName Invalid data name: " + name + " | uniqueID: " + uniqueID + ")";
      return name.Substring(0, Mathf.Min(name.Length, 10)) +
            "_" + uniqueID;
    }

    private static CollectionSaveData LoadPlantSaveData(string customPath = "") {
      string path = customPath.HasLength() ? customPath : savePath;
      Debug.Log("Loading plant data..." + path);
      invalidate = true;
      if (File.Exists(path)) {
        try {
          string jsonString = File.ReadAllText(path);
          DebugBW.Log("jsonString: " + jsonString);

          CollectionSaveData data = JsonConvert.DeserializeObject<CollectionSaveData>(jsonString,
            new JsonSerializerSettings {
              CheckAdditionalContent = false,
              TypeNameHandling = TypeNameHandling.Auto,
              NullValueHandling = NullValueHandling.Ignore,
              Converters = { new StringEnumConverter() },
            });

          data = data.DidDeserialize();
          //data.Migrate();

          return data;
        } catch (Exception e) {
          Debug.LogError("Load game error: " + e);
          return default(CollectionSaveData);
        }
      } else {
        Debug.LogWarning("Save data does not existing, creating new");
        CollectionSaveData data = default(CollectionSaveData);
        SavePlantData(data);
        return data;
      }
    }

    private static void SavePlantData(CollectionSaveData save, string customPath = "") {
      DebugBW.Log("Saving plant data..", LColor.brown);
      if (!Directory.Exists(SaveDirectory)) Directory.CreateDirectory(SaveDirectory);

      invalidate = true;
      string json = JsonConvert.SerializeObject(save, Formatting.Indented,
        new JsonSerializerSettings {
          CheckAdditionalContent = false,
          TypeNameHandling = TypeNameHandling.Auto,
          NullValueHandling = NullValueHandling.Ignore,
          Converters = { new StringEnumConverter() },
        });
      // Debug.Log("PlantData json: " + json);
      string path = customPath.HasLength() ? customPath : savePath;
      File.WriteAllText(path, json);
    }

    public static void ToggleFavorite(PlantIndexEntry entry, PlantCollection collection) {
      if (!AssertWarning(collection != PlantCollection.SeedBank, "Method does not permit seed bank fetch")) return;
      PresetCollection col = GetCollection(collection);
      entry.favorite = !entry.favorite;
      col.UpdateEntry(entry);
      saveData.SetCollection(collection, col);
      SavePlantData(saveData);

      DidUpdatePlantIndexDataEvent?.Invoke(entry);
    }

    public static void MarkPropegating(PlantIndexEntry entry, PlantCollection collection) {
      if (!AssertWarning(collection != PlantCollection.SeedBank, "Method does not permit seed bank fetch")) return;
      PresetCollection col = GetCollection(collection);
      entry.propegating = true;
      col.UpdateEntry(entry);
      saveData.SetCollection(collection, col);
      SavePlantData(saveData);

      DidUpdatePlantIndexDataEvent?.Invoke(entry);
    }

    public static void DeductHybridUses(PlantIndexEntry entry, PlantCollection collection) {
      SetHybridUses(entry, collection, entry.hybridsRemaining - 1);
    }

    public static void SetHybridUses(PlantIndexEntry entry, PlantCollection collection, int count) {
      if (!AssertWarning(collection != PlantCollection.SeedBank, "Method does not permit seed bank fetch")) return;
      PresetCollection col = GetCollection(collection);
      if (!Assert(entry.hybridsRemaining != 0, "Entry no hybrids remaining: " + entry)) return;
      entry.hybridsRemaining = count;
      DebugBW.Log("entry.hybridsRemaining: " + entry.hybridsRemaining + " | entry: " + entry);
      col.UpdateEntry(entry);
      saveData.SetCollection(collection, col);
      SavePlantData(saveData);

      DidUpdatePlantIndexDataEvent?.Invoke(entry);
    }

    public static PlantData[] GetPropegatingPlants() => saveData.GetPropegatingPlants();

    public static void FinishPropegating(PlantIndexEntry entry, PlantCollection collection) {
      if (!AssertWarning(collection != PlantCollection.SeedBank, "Method does not permit seed bank fetch")) return;
      PresetCollection col = GetCollection(collection);
      entry.propegating = false;
      entry.hybridsRemaining = PlantIndexEntry.DefaultHybridsRemaining;
      col.UpdateEntry(entry);
      saveData.SetCollection(collection, col);
      SavePlantData(saveData);
      invalidate = true;

      DidUpdatePlantIndexDataEvent?.Invoke(entry);
    }

    public static PresetCollection GetCollection(PlantCollection collection) {
#if UNITY_EDITOR
      if (saveData.IsDefault()) { //inspector-mode fix
        StaticAwake();
      }
#endif
      if (collection == PlantCollection.SeedBank) {
        return saveData.GetSeedBank();
      }

      PresetCollection col = saveData.GetCollection(collection);
      if (col.IsNotDefault()) return col;

      Debug.Log("Collection not found in save data, adding: " + collection + " | saveData: " + saveData);
      PresetCollection c = CreateCollection(collection);
      saveData.AddCollection(c);
      SavePlantData(saveData);
      return c;
    }

    private static void AddToCollection(PlantCollection collection, params SavedPlant[] savedPlants) {
      PresetCollection col = GetCollection(collection);
      var nms = savedPlants.ToList().Select(sp => new PlantIndexEntry(sp));
      col.AddToCollection(nms.ToArray());
      saveData.SetCollection(collection, col);
      SavePlantData(saveData);
      invalidate = true;
    }

    public static void RemoveFromCollection(PlantCollection collection, params PlantIndexEntry[] entries) {
      if (!entries.HasLength()) return;
      DebugBW.Log("removing: " + entries.ToLog());
      PresetCollection col = GetCollection(collection);
      col.RemoveFromCollection(entries);
      saveData.SetCollection(collection, col);
      SavePlantData(saveData);
      invalidate = true;
    }

    public static void MigrateSavedPlantBetweenCollections(PlantIndexEntry entry, PlantCollection from, PlantCollection to) {
      Debug.Log("Migrating " + entry.name + "[" + from + " -> " + to + "]");
      saveData = saveData.MigrateSavedPlantBetweenCollections(entry, from, to);
      SavePlantData(saveData);
      invalidate = true;
    }

    public static void RenamePlant(PlantIndexEntry indexEntry, string newName) {
      string oldName = GetSaveName(indexEntry);
      newName = StripNameChars(newName);
      PlantData pd = GetPlant(indexEntry);

      string oldPath = FullPath(PlantsSaveDirectory, indexEntry);
      if (!Assert(File.Exists(oldPath), "Cannot rename plant: file not found: " + oldPath)) return;
      FileInfo fileInfo = new FileInfo(oldPath);
      fileInfo.MoveTo(fileInfo.Directory.FullName + "\\" + newName + ".xml");

      PlantIndexEntry newEntry = saveData.RenamePlant(indexEntry, newName, pd.collection);
      SavePlantData(saveData);
      Debug.LogWarning("PlantDataManage RenamePlant only works in .Hybrids!");
      RenameTextures(indexEntry, newEntry, PlantCollection.Hybrids);
    }

    public static void RenameTextures(PlantIndexEntry oldEntry, PlantIndexEntry newEntry, PlantCollection collection) {
      TextureStorageManager.RenameTextures(oldEntry, newEntry, collection);
    }

    public static string GetRandomPlantNameFromCollection(PlantCollection collection) =>
      GetCollection(collection).PlantNames.RandomObj();

    // public static PlantIndexEntry TryGetIndexEntry(string name) {
    //   foreach (PresetCollection col in saveData.GetCollections()) {
    //     foreach (PlantIndexEntry entry in col.plants) {
    //       if (entry.name == name) return entry;
    //     }
    //   }
    //   return default(PlantIndexEntry);
    // }

    public static PlantIndexEntry TryGetIndexEntry(string name, PlantCollection collection) {
      name = StripNameChars(name);
      foreach (PlantIndexEntry entry in saveData.GetCollection(collection).plants) {
        if (entry.name == name) return entry;
      }
      return default(PlantIndexEntry);
    }

    public static PlantIndexEntry TryGetIndexEntryUnqID(string uniqueID) {
      foreach (PresetCollection col in saveData.GetCollections()) {
        foreach (PlantIndexEntry entry in col.plants) {
          if (entry.uniqueID == uniqueID) return entry;
        }
      }
      return default(PlantIndexEntry);
    }

    public static bool PlantExistsForData(PlantData data) => data == null ? false : PlantExists(data.indexEntry);
    public static bool PlantExists(PlantIndexEntry indexEntry, bool isStarterPlant = false) {
      string fullPath = FullPath(PlantsSaveDirectory, indexEntry);
      if (isStarterPlant) fullPath = FullPath(PresetManager.StarterPlantsSaveDirectory, indexEntry);
      // Debug.Log(indexEntry.name + " path: " + fullPath + " | exists: " + File.Exists(fullPath));
      return File.Exists(fullPath);
    }

    public static (LeafParamDict fields, SavedPlant savedPlant) LoadPlant(string uniqueID, bool forceStarterBase = false) {
      PlantIndexEntry entry = TryGetIndexEntryUnqID(uniqueID);
      if (!Asserts.AssertWarning(entry.IsNotDefault(), "LoadPlant could not find plant with uniqueID: " + uniqueID + " | forceStarterBase: " + forceStarterBase))
        return (null, default(SavedPlant));
      return LoadPlant(entry, forceStarterBase);
    }

    public static (LeafParamDict fields, SavedPlant savedPlant) LoadPlant(PlantIndexEntry indexEntry, bool forceStarterBase = false) {
      // if (forceStarterBase) return PresetManager.LoadStarterPlant(indexEntry);
      return PresetManager.LoadPlant(indexEntry);
    }

    public static PlantData GetPlant(PlantIndexEntry indexEntry) {
      (LeafParamDict fields, SavedPlant savedPlant) = LoadPlant(indexEntry);
      return new PlantData(fields, indexEntry, FindCollection(indexEntry), savedPlant.seed);
    }

    public static PlantCollection FindCollection(PlantIndexEntry indexEntry) {
      foreach (PresetCollection col in saveData.GetCollections()) {
        if (col.plants.ToList().Find(pie => pie.uniqueID == indexEntry.uniqueID).IsNotDefault())
          return col.collection;
      }
      Debug.LogWarning("Could not find collection for " + indexEntry.name + " | " + indexEntry.uniqueID);
      return PlantCollection.Hybrids;
    }

    public static PlantIndexEntry SavePlant(string name, LeafParamDict fields, int seed, PlantCollection collection, bool tryOverwrite) {
      SavedPlant savedPlant = new SavedPlant(name, fields, seed);
      PlantIndexEntry entry = new PlantIndexEntry(savedPlant);
      if (tryOverwrite) {
        PlantIndexEntry tempEntry = TryGetIndexEntry(name, collection);
        if (tempEntry.IsNotDefault()) {
          entry = tempEntry;
          savedPlant.uniqueID = entry.uniqueID;
        }
      }
      Debug.Log("Saving " + name + " with ID " + entry.uniqueID);
      PresetManager.SavePlant(savedPlant, collection == PlantCollection.StarterBases);
      AddToCollection(collection, savedPlant);
      invalidate = true;
      entry = GetCollection(collection).plants.Find(pie => pie.uniqueID == savedPlant.uniqueID);
      return entry;
    }

    public static void CachePlantToDisk(CachedPlant cachedPlantData, int seed, LeafParamDict fields) {
      Debug.Log("Caching " + cachedPlantData.name);
      _ = Task.Run(() => {
        string hash = PresetManager.SavedPlantHash(new SavedPlant(cachedPlantData.name, fields, seed));
        PresetManager.CachePlantToDisk(cachedPlantData, hash);
      });
    }

    public static string GetCachedPlantHash(PlantIndexEntry entry) {
      return PresetManager.GetCachedPlantHash(entry);
    }

    public static CachedPlant LoadCachedPlant(PlantIndexEntry entry) {
      // Debug.Log("Loading cached plant: " + plantData.name);
      return PresetManager.LoadCachedPlant(entry);
    }

    public static bool CacheExistsForData(PlantIndexEntry entry) {
      return PresetManager.CacheExistsForPlant(entry);
    }

    public static void ClearCollection(PlantCollection col) {
      PresetCollection presetCol = GetCollection(col);
      if (presetCol.IsDefault()) return;
      RemoveFromCollection(col, presetCol.plants.ToArray());
      SweepOrphans();
      invalidate = true;
    }

    private static string FullPath(string path, PlantIndexEntry entry, string extension = "xml") =>
      path + GetSaveName(entry) + "." + extension;

    public static void WriteIMTexture(MagickImage image, PlantIndexEntry entry, TextureType type, PlantCollection collection) =>
      TextureStorageManager.WriteIMTexture(image, entry, type, collection);

    public static void WriteTexture(Texture2D tex, PlantIndexEntry entry, TextureType type, PlantCollection collection) =>
      TextureStorageManager.WriteTexture(tex, entry, type, collection);

    public static Texture2D GetTexture(PlantIndexEntry entry, TextureType type, string texDisplayName, PlantCollection collection) {
      string cacheName = CacheName(entry, type);
      if (Application.isPlaying) {
        if (textureCache != null && textureCache.ContainsKey(cacheName)) {
          // DateTime now = DateTime.Now;
          Texture2D baseTex = textureCache[cacheName];
          Texture2D copyTex = new Texture2D(baseTex.width, baseTex.height,
            UnityEngine.Experimental.Rendering.DefaultFormat.LDR, 1, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
          if (baseTex.mipmapCount > 1) Debug.LogWarning("Texture2D baseTex mipmap count is > 1");
          if (copyTex.mipmapCount > 1) Debug.LogWarning("Texture2D copyTex mipmap count is > 1");
          Graphics.CopyTexture(baseTex, copyTex);
          // Debug.Log("Time: " + DateTime.Now.Subtract(now).TotalMilliseconds);
          return copyTex;
        }
      }
      return TextureStorageManager.GetTexture(entry, type, texDisplayName, collection);
    }

    public static async Task<Texture2D> GetTextureAsync(PlantIndexEntry entry, TextureType type, string texDisplayName, PlantCollection collection) {
      byte[] arr = new byte[0];
      await Task.Run(() => {
        arr = TextureStorageManager.PreloadTextureData(entry, type, collection);
      });
      if (arr.Length == 0) return null;
      return TextureStorageManager.LoadTextureFromPreloadData(arr, texDisplayName);
    }

    public static byte[] GetPreloadTextureDataAsync(PlantIndexEntry entry, TextureType type, PlantCollection collection) {
      byte[] arr = new byte[0];
      arr = TextureStorageManager.PreloadTextureData(entry, type, collection);
      return arr;
    }

    public static Texture2D GetPreloadedTexture(string texDisplayName, byte[] preloadData) {
      if (!preloadData.HasLength()) return null;
      return TextureStorageManager.LoadTextureFromPreloadData(preloadData, texDisplayName);
    }

    public static bool TextureExistsAtPath(PlantIndexEntry entry, TextureType type, PlantCollection collection) =>
      TextureStorageManager.TextureExistsAtPath(entry, type, collection);

    // public static PlantIndexEntry CopyStarterPlantToPlayerCollection(PlantIndexEntry entry) {
    //   Debug.Log("CopyStarterPlantToPlayerCollection " + entry.name);
    //   SavedPlant sp = PresetManager.LoadStarterPlant(entry).savedPlant;
    //   PlantIndexEntry newEntry = SavePlant(sp.name, sp.GetLeafParamDict(), sp.seed, PlantCollection.Starters, false);
    //   TextureStorageManager.CopyStarterTextures(entry, newEntry);
    //   return newEntry;
    // }

    public static PresetCollection CreateCollection(PlantCollection collection) {
      return new PresetCollection(collection, new PlantIndexEntry[0], null);
    }

    public static int TextureSizeForLeaf(PlantIndexEntry entry, PlantCollection collection) {
      Texture2D tex = TextureStorageManager.GetTexture(entry, TextureType.Albedo, "width " + entry.name, collection);
      if (tex == null) return 0;
      return tex.width;
    }

    public static List<TextureType> MissingTexturesForLeaf(PlantIndexEntry entry, PlantCollection collection) =>
      TextureStorageManager.MissingTexturesForLeaf(entry, collection);

    public static void ClearTemporary() {
      ClearCollection(PlantCollection.Temporary);
    }

    public static void SweepOrphans() {
      List<string> allNames = new List<string>();
      foreach (PresetCollection col in saveData.GetCollections()) {
        allNames.Add(col.PlantNames);
        allNames.Add(col.plants.Select(pie => GetSaveName(pie)).ToArray());
      }
      // DebugBW.Log("allNames: " + allNames.ToLog());
      TextureStorageManager.SweepOrphans(allNames.ToArray());
      PresetManager.SweepOrphans(allNames.ToArray());
    }

    //only keep alphanumeric and _-
    public static string StripNameChars(string str) {
      if (!str.HasLength()) return null;
      return Regex.Replace(str,
        @"[^a-zA-Z0-9_\-]", "");
    }

    private static string CacheName(PlantIndexEntry indexEntry, TextureType type) =>
      GetSaveName(indexEntry) + "_" + type;
  }
}
