using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using static BionicWombat.IMTextureFactory;

namespace BionicWombat {
  public enum MaterialType {
    LeafVelvet,
    LeafFlat,
    Stem,
    Trunk,
  }

#if UNITY_EDITOR
  [ExecuteInEditMode]
#endif
  [Serializable]
  public class LeafMaterialController {

    private MaterialPropertyBlock propBlock;

    public void SetMaterialParams(LeafParamDict dict, Material mat) {
      Color baseColor = dict[LPK.TexBaseColor].colorValue;
      mat.SetColor("_BaseColor", baseColor);

      float spec = dict[LPK.MaterialShininess].value;
      mat.SetFloat("_Shininess", spec);

      float met = dict[LPK.MaterialMetallicness].value;
      mat.SetFloat("_Metallicness", met);

      float ao = dict[LPK.MaterialAOStrength].value;
      mat.SetFloat("_AOMult", ao);

      float rimPower = dict[LPK.MaterialRimPower].value;
      rimPower = (1f - rimPower) * 7.5f + 0.5f;
      mat.SetFloat("_RimPower", rimPower);

      float vert = dict[LPK.MaterialVertBumps].value * 0.01f;
      mat.SetFloat("_VertBumps", vert);

      float micro = dict[LPK.MaterialMicrotexAmp].value;
      mat.SetFloat("_MicroBumps", micro);

      float heightAmp = dict[LPK.MaterialHeightAmp].value * 0.04f;
      if (dict[LPK.MaterialHeightAmp].enabled) mat.SetFloat("_HeightAmp", heightAmp);
      else mat.SetFloat("_HeightAmp", 0f);

      Color c = dict[LPK.MaterialRimColor].colorValue;
      mat.SetColor("_RimColor", c);
    }

    public void SetStemAndTrunkMaterialParams(LeafParamDict dict, Material stemMat, Material trunkMat) {
      Color c = LeafParamBehaviors.GetColorForParam(dict[LPK.TexShadowStrength], dict);
      stemMat.SetColor("_Color", c);

      Color c2 = LeafParamBehaviors.GetColorForParam(dict[LPK.TrunkBrowning], dict);
      trunkMat.SetColor("_Color", c2);
    }

    public void ApplyAllTextures(string leafName, PlantCollection collection, Material mat, Renderer[] renderers) {
      foreach (TextureType type in Enum.GetValues(typeof(TextureType)))
        ApplyTexture(type, leafName, collection, mat, renderers);
    }

    /*public void ClearAllTextures(Material mat, Renderer renderer) => ClearAllTextures(mat, new Renderer[] { renderer });
    public void ClearAllTextures(Material mat, Renderer[] renderers) {
      foreach (TextureType type in Enum.GetValues(typeof(TextureType)))
        ApplyTexture(type, "", mat, renderers, true);
    }*/

    public void ApplyTexture(TextureType texType, string leafName, PlantCollection collection,
        Material mat, Renderer renderer, bool shouldClear = false) =>
      ApplyTexture(texType, leafName, collection, mat, new Renderer[] { renderer }, shouldClear);

    public void ApplyTexture(TextureType texType, string leafName, PlantCollection collection,
        Material mat, Renderer[] renderers, bool shouldClear = false) {
      if (renderers == null || renderers.Length == 0) {
        Debug.LogWarning("No renderers provided to LeafMaterialController.ApplyTexture");
        return;
      }

      // Debug.Log("Applying Texture: " + IMTextureFactory.GetPath(leafName, texType) + "\r");
      Texture2D tex = null; //BlankTex
      if (!shouldClear) tex = DataManager.GetTexture(leafName, texType, collection);
      else if (shouldClear && texType != TextureType.Albedo) tex = null;

      if (tex == null) {
        Debug.LogWarning("ApplyTexture null texture on leaf " + leafName + "_" + texType);
      }

      MaterialType matType = GetType(mat.name);
      string texName = "";
      if (matType == MaterialType.LeafFlat) {
        texName = "_MainTex";
      } else {
        switch (texType) {
          case TextureType.Albedo:
            texName = "_Albedo";
            mat.SetInt("_HasAlbedo", tex == null ? 0 : 1);
            break;
          case TextureType.Normal:
            texName = "_Normal";
            mat.SetInt("_HasNormal", tex == null ? 0 : 1);
            break;
          case TextureType.Height:
            texName = "_Height";
            break;
          case TextureType.VeinMask:
            texName = "_VeinMask";
            break;
          default:
            Debug.LogError("ApplyTexture shader type not supported: " + texType);
            break;
        }
      }

      // if (propBlock != null) {
      //   Texture t = propBlock.GetTexture(texName);
      //   if (Application.isPlaying && t != null) Destroy.PDestroy(t);
      //   // if (t != null) Debug.Log(leafName + " | tex.name: " + tex.name + " | texName: " + texName + " | texType: " + texType);
      //   // Debug.Log(leafName + " int: " + propBlock.GetInt("int"));
      // }

      if (propBlock == null) propBlock = new MaterialPropertyBlock();
      foreach (Renderer rend in renderers) {
        rend.GetPropertyBlock(propBlock);
        if (tex == null) propBlock.Clear();
        else propBlock.SetTexture(texName, tex);
        rend.SetPropertyBlock(propBlock);
      }
    }

    // private static Texture2D _blankTex;
    // private static Texture2D blankTex {
    //   get {
    //     if (_blankTex == null) _blankTex = DataManager.GetBlankTexture();
    //     return _blankTex;
    //   }
    // }

    public static Material GetMaterial(MaterialType type, bool makeNew = true) {
      Material m = Resources.Load(MaterialName(type), typeof(Material)) as Material;
      if (makeNew) m = new Material(m);
      return m;
    }

    public static string MaterialName(MaterialType type) {
      switch (type) {
        case MaterialType.LeafVelvet: return "Velvet_mat";
        case MaterialType.LeafFlat: return "Flat";
        case MaterialType.Stem: return "Stem_mat";
        case MaterialType.Trunk: return "Trunk_mat";
      }
      Debug.LogWarning("MaterialName enum not recognized: " + type);
      return "Flat";
    }

    public static MaterialType GetType(string name) {
      foreach (MaterialType type in Enum.GetValues(typeof(MaterialType)))
        if (name == MaterialName(type)) return type;
      return MaterialType.LeafFlat;
    }

    public static bool IsFlatMat(Material mat) => mat.name == LeafMaterialController.MaterialName(MaterialType.LeafFlat);
  }

}