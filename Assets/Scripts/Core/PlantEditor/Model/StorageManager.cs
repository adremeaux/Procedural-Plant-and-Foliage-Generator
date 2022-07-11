using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImageMagick;
using UnityEngine;

namespace BionicWombat {
  public static class StorageManager {
    public static string StreamingAssetsPath = Application.streamingAssetsPath + "/Textures/";
    public static string GenPath = StreamingAssetsPath + "Gen/";
    public static string AlbedoBlankPath = StreamingAssetsPath + "Albedo_blank";
    private static string ImageTypeExtension = "png";
    public static string[] Extensions = new string[] { ImageTypeExtension, ImageTypeExtension + ".meta" };

    public static void WriteIMTexture(MagickImage image, string name, TextureType type, PlantCollection collection) {
      string path = GetAbsolutePath(name, type, collection, ImageTypeExtension);
      if (path == null) return;

      CreateDirectoryIfMissing(collection);
      image.Write(path, MagickFormat.Png32);
    }

    public static void WriteTexture(Texture2D tex, string name, TextureType type, PlantCollection collection) {
      string path = GetAbsolutePath(name, type, collection, ImageTypeExtension);
      if (path == null) return;
      if (!tex.isReadable) {
        tex = IMTextureFactory.GetReadableTexture(tex);
      }
      CreateDirectoryIfMissing(collection);

      tex.hideFlags = HideFlags.None;
      byte[] bytes = tex.EncodeToPNG();
      File.WriteAllBytes(path, bytes);
      // Debug.Log("Write " + name + " to path " + path);
    }

    public static Texture2D GetTexture(string leafName, TextureType type, PlantCollection collection) {
      string path = GetAbsolutePath(leafName, type, collection, ImageTypeExtension);
      return LoadTextureAtPath(path);
    }

    public static Texture2D GetBlankTexture() => LoadTextureAtPath(AlbedoBlankPath);

    private static Texture2D LoadTextureAtPath(string path) {
      Texture2D tex = null;
      byte[] fileData;

      if (File.Exists(path)) {
        fileData = File.ReadAllBytes(path);
        tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);
      } else {
        Debug.Log("No file exists at path " + path);
      }
      return tex;
    }

    private static void CreateDirectoryIfMissing(PlantCollection collection) {
      string path = GetAbsoluteDirectoryPath(collection);
      if (!Directory.Exists(path)) {
        Debug.Log("Creating new directory at path: " + path);
        Directory.CreateDirectory(path);
      }
    }

    public static void MigrateTexturesBetweenCollections(string leafName, PlantCollection from, PlantCollection to) {
      PresetCollection c = PresetManager.GetCollection(from);
      CreateDirectoryIfMissing(to);
      foreach ((string name, TextureType type, string extension) in c.AllFiles()) {
        string oldPath = GetAbsolutePath(name, type, from, extension);
        string newPath = GetAbsolutePath(name, type, to, extension);
        if (File.Exists(oldPath)) File.Move(oldPath, newPath);
      }
    }

    public static void Sweep(PresetCollection presetCollection) {
      int count = 0;
      foreach ((string name, TextureType type, string extension) in presetCollection.AllFiles()) {
        string path = GetAbsolutePath(name, type, presetCollection.collection, extension);
        bool exists = File.Exists(path);
        if (exists) {
          Debug.Log("Deleting " + name + "." + type + " at " + path);
          File.Delete(path);
          count++;
        } else {
          Debug.Log("File " + name + "_" + type + "." + extension + " does not exist at " + path);
        }
      }

      Debug.Log("Deleted " + count + "  files from collection " + presetCollection.collection);

      string dirPath = GetAbsoluteDirectoryPath(presetCollection.collection);
      string[] files = Directory.GetFiles(dirPath);
      if (files.Length > 0) Debug.Log("Files remaining after sweep: " + files.ToLog());
    }

    public static List<TextureType> MissingTexturesForLeaf(string name, PlantCollection collection) {
      List<TextureType> list = new List<TextureType>();
      foreach (TextureType type in Enum.GetValues(typeof(TextureType))) {
        if (!File.Exists(GetAbsolutePath(name, type, collection, ImageTypeExtension)))
          list.Add(type);
      }
      return list;
    }

    public static PresetCollection[] RecoverCollections() {
      string[] dirs = Directory.GetDirectories(GenPath);
      string[] existing = PresetManager.GetCollectionNames();
      List<PresetCollection> list = new List<PresetCollection>();
      foreach (string dirPath in dirs) {
        string[] allFiles = Directory.GetFiles(dirPath);
        string dir = new DirectoryInfo(dirPath).Name;
        HashSet<string> names = new HashSet<string>();
        foreach (string f in allFiles) {
          string fName = Path.GetFileNameWithoutExtension(f);
          int index = fName.IndexOf("_");
          if (index >= 0)
            names.Add(fName.Substring(0, index));
        }
        if (names.Count == 0 || existing.Contains(dir)) continue;
        try {
          PlantCollection plantCol = (PlantCollection)Enum.Parse(typeof(PlantCollection), dir);
          PresetCollection col = new PresetCollection(plantCol, names.ToArray());
          list.Add(col);
          Debug.Log("Recovered collection: " + col);
        } catch { }
      }
      return list.ToArray();
    }

    private static string GetAbsolutePath(string leafName, TextureType type, PlantCollection collection, string extension) {
      if (leafName.Length == 0) {
        Debug.LogWarning("GetWriteablePath needs a valid leafName: " + leafName);
        return null;
      }
      string suffix = "_" + type.ToString().ToLower();
      return GetAbsoluteDirectoryPath(collection) + leafName + suffix + "." + extension;
    }

    private static string GetAbsoluteDirectoryPath(PlantCollection collection) {
      return GenPath + collection.ToString() + "/";
    }
  }

}