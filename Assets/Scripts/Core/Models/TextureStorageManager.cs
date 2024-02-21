using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BionicWombat.PlantCollections;
using ImageMagick;
using UnityEngine;

namespace BionicWombat {
  public static class TextureStorageManager {
    public static string StreamingAssetsPath = Application.streamingAssetsPath + "/Textures/";
    public static string GenPath = StreamingAssetsPath + "Gen/";
    public static string PermaPath = GenPath + "Permanent/";
    public static string AlbedoBlankPath = StreamingAssetsPath + "Albedo_blank";
    private static string ImageTypeExtension = "png";
    public static string[] MoveExtensions = new string[] { ImageTypeExtension, ImageTypeExtension + ".meta" };
    public static string[] CopyExtensions = new string[] { ImageTypeExtension };

    public static void WriteIMTexture(MagickImage image, PlantIndexEntry entry, TextureType type, PlantCollection collection) {
      string path = GetAbsolutePath(entry, type, ImageTypeExtension, collection);
      if (path == null) return;

      CreateDirectoryIfMissing();
      image.Write(path, MagickFormat.Png32);
    }

    public static void WriteTexture(Texture2D tex, PlantIndexEntry entry, TextureType type, PlantCollection collection) {
      string path = GetAbsolutePath(entry, type, ImageTypeExtension, collection);
      if (path == null) return;
      if (!tex.isReadable) {
        tex = IMTextureFactory.GetReadableTexture(tex);
      }
      CreateDirectoryIfMissing();

      tex.hideFlags = HideFlags.None;
      byte[] bytes = tex.EncodeToPNG();
      File.WriteAllBytes(path, bytes);
      // Debug.Log("Write " + name + " to path " + path);
    }

    public static void WriteTextureBytes(byte[] bytes, PlantIndexEntry entry, TextureType type, PlantCollection collection) {
      string path = GetAbsolutePath(entry, type, ImageTypeExtension, collection);
      if (path == null) return;
      CreateDirectoryIfMissing();
      File.WriteAllBytes(path, bytes);
    }

    public static void RenameTextures(PlantIndexEntry oldEntry, PlantIndexEntry newEntry, PlantCollection collection) {
      foreach (TextureType type in EnumExt.Values<TextureType>()) {
        string oldPath = GetAbsolutePath(oldEntry, type, ImageTypeExtension, collection);
        if (!File.Exists(oldPath)) DebugBW.Log("  oldPath: " + oldPath + " exists? " + File.Exists(oldPath), LColor.green);
        if (File.Exists(oldPath)) {
          string newFileName = GetFileName(newEntry, type, ImageTypeExtension);
          //DebugBW.Log("  Renaming from " + oldPath + " (to) " + newFileName, LColor.aqua);
          FileInfo fileInfo = new FileInfo(oldPath);
          fileInfo.MoveTo(fileInfo.Directory.FullName + "\\" + newFileName);
        }
      }
    }

    // public static void CopyStarterTextures(PlantIndexEntry starterBasesEntry, PlantIndexEntry newEntry) {
    //   foreach (TextureType type in EnumExt.Values<TextureType>()) {
    //     string oldPath = GetAbsolutePath(starterBasesEntry, type, ImageTypeExtension, PlantCollection.StarterBases);
    //     if (!File.Exists(oldPath)) Debug.LogWarning("  CopyStarterTextures no file at old path oldPath: " + oldPath);
    //     else if (File.Exists(oldPath)) {
    //       string newPath = GetAbsolutePath(newEntry, type, ImageTypeExtension, PlantCollection.Starters);
    //       File.Copy(oldPath, newPath);
    //       // DebugBW.Log("Copying " + oldPath + " to " + newPath);
    //     }
    //   }
    // }

    public static bool TextureExistsAtPath(PlantIndexEntry entry, TextureType type, PlantCollection collection) =>
      File.Exists(GetAbsolutePath(entry, type, ImageTypeExtension, collection));

    public static Texture2D GetTexture(PlantIndexEntry entry, TextureType type, string texDisplayName, PlantCollection collection) {
      string path = GetAbsolutePath(entry, type, ImageTypeExtension, collection);
      // DebugBW.Log("path: " + path);
      return LoadTextureAtPath(path, texDisplayName);
    }

    public static Texture2D GetBlankTexture() => LoadTextureAtPath(AlbedoBlankPath, "BLANK");

    private static Texture2D LoadTextureAtPath(string path, string texDisplayName) {
      Texture2D tex = null;
      byte[] fileData;
      if (File.Exists(path)) {
        fileData = File.ReadAllBytes(path);
        tex = new Texture2D(2, 2,
          UnityEngine.Experimental.Rendering.DefaultFormat.LDR, 1, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
        tex.name = texDisplayName;
        tex.LoadImage(fileData);
      } else {
        // Debug.Log("No file exists at path " + path);
      }
      return tex;
    }

    public static byte[] PreloadTextureData(PlantIndexEntry indexEntry, TextureType type, PlantCollection collection) {
      string path = GetAbsolutePath(indexEntry, type, ImageTypeExtension, collection);
      byte[] fileData = new byte[0];
      if (File.Exists(path)) {
        fileData = File.ReadAllBytes(path);
      }
      return fileData;
    }

    public static Texture2D LoadTextureFromPreloadData(byte[] textureData, string texDisplayName) {
      Texture2D tex = new Texture2D(2, 2,
        UnityEngine.Experimental.Rendering.DefaultFormat.LDR, 1, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
      tex.name = texDisplayName;
      tex.LoadImage(textureData);
      return tex;
    }

    private static void CreateDirectoryIfMissing() {
      if (!Directory.Exists(GenPath)) {
        Debug.Log("Creating new directory at path: " + GenPath);
        Directory.CreateDirectory(GenPath);
      }
    }

    public static void RemovePlants(params PlantIndexEntry[] entries) {
      foreach ((PlantIndexEntry entry, TextureType type, string extension) in AllTextureFiles(entries)) {
        if (!entries.Contains(entry)) continue;
        string path = GetAbsolutePath(entry, type, extension, PlantCollection.Temporary); //collection doesn't matter here

        if (File.Exists(path)) File.Delete(path);
      }
    }

    private static IEnumerable<(PlantIndexEntry, TextureType, string)> AllTextureFiles(PlantIndexEntry[] entries) {
      string[] ext = TextureStorageManager.MoveExtensions;
      foreach (PlantIndexEntry entry in entries)
        foreach (TextureType type in Enum.GetValues(typeof(TextureType)))
          foreach (string extension in ext)
            yield return (entry, type, extension);
    }

    public static List<TextureType> MissingTexturesForLeaf(PlantIndexEntry entry, PlantCollection collection) {
      List<TextureType> list = new List<TextureType>();
      foreach (TextureType type in Enum.GetValues(typeof(TextureType))) {
        if (!File.Exists(GetAbsolutePath(entry, type, ImageTypeExtension, collection)))
          list.Add(type);
      }
      return list;
    }

    public static void SweepOrphans(string[] existingNames) {
      string[] files = Directory.GetFiles(GenPath).ToList().Adding(Directory.GetFiles(PermaPath)).ToArray();
      DebugBW.Log("files: " + files.ToLog());
      foreach (string file in files) {
        string fileName = Path.GetFileName(file);
        int lastIdx = fileName.LastIndexOf("_");
        if (lastIdx == -1) {
          continue;
        }
        string plantName = fileName.Substring(0, lastIdx);
        bool contains = existingNames.Contains(plantName);
        if (!contains) {
          // DebugBW.Log("Deleting orphan " + fileName, LColor.orange);
          File.Delete(file);
        }
      }
    }

    private static string GetAbsolutePath(PlantIndexEntry entry, TextureType type, string extension, PlantCollection collection) {
      if (entry.IsDefault()) {
        Debug.LogWarning("GetWriteablePath needs a valid leafName: " + entry);
        return null;
      }
      if (collection.IsPermanentCollection())
        return PermaPath + GetFileName(entry, type, extension);
      else
        return GenPath + GetFileName(entry, type, extension);
    }

    private static string GetFileName(PlantIndexEntry entry, TextureType type, string extension) {
      string suffix = "_" + type.ToString().ToLower();
      return PlantDataManager.GetSaveName(entry) + suffix + "." + extension;
    }

    private static string GetFileName(string saveName, TextureType type, string extension) {
      string suffix = "_" + type.ToString().ToLower();
      return saveName + suffix + "." + extension;
    }
  }

}
