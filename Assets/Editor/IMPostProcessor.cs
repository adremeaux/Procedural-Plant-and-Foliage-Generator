using System;
using UnityEditor;
using UnityEngine;

namespace BionicWombat {
public class IMPostProcessor : AssetPostprocessor {
  public void OnPreprocessTexture() {
    if (assetPath.Contains("_diffuse") || assetPath.Contains("_normal")) {
      bool isNormalMap = false;//assetPath.Contains("_normal");
      TextureImporter textureImporter = (TextureImporter)assetImporter;
      TextureImporterPlatformSettings settings = new TextureImporterPlatformSettings();
      settings.textureCompression = TextureImporterCompression.Uncompressed;
      settings.format = isNormalMap ? TextureImporterFormat.RGBA32 : TextureImporterFormat.RGB24;
      textureImporter.SetPlatformTextureSettings(settings);
      // if (isNormalMap) textureImporter.textureType = TextureImporterType.NormalMap;
    }
  }
}

public class SaveProcessor : AssetModificationProcessor {
  public static string[] OnWillSaveAssets(string[] paths) {
    // GameObject[] plants = GameObject.FindGameObjectsWithTag("Plant");
    // foreach (GameObject go in plants) {
    //   Plant plant = go.GetComponent<Plant>();
    //   plant.WillSave();
    // }
    return paths;
  }
}

}