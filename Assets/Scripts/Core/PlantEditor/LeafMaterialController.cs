using System;
using System.Threading.Tasks;
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

    public void SetMaterialParams(LeafParamDict dict, Material[] mats,
        ComputeBuffer deltasBuffer, ComputeBuffer normalsBuffer, ComputeBuffer ageSpotsBuffer,
        int instances, int baseVertsCount, Vector2 center) {
      Color baseColor = dict[LPK.TexBaseColor].colorValue;
      float spec = dict[LPK.MaterialShininess].value;
      float met = dict[LPK.MaterialMetallicness].value;
      float ao = dict[LPK.MaterialAOStrength].value;
      float abaxDarkening = dict[LPK.AbaxialDarkening].value;
      float abaxPurple = dict[LPK.AbaxialPurpleTint].value;
      float abaxHue = dict[LPK.AbaxialHue].value;
      float rimPower = dict[LPK.MaterialRimPower].value;
      rimPower = (1f - rimPower) * 4f + 4f;
      float micro = dict[LPK.MaterialMicrotexAmp].value;
      micro = (micro * 0.2f) + 0.8f;
      float heightAmp = dict[LPK.MaterialHeightAmp].value * 0.04f;
      Color c = dict[LPK.MaterialRimColor].colorValue;
      float maskingStrength = Mathf.Clamp01(dict[LPK.TexMaskingStrength].value);

      for (int i = 0; i < mats.Length; i++) {
        mats[i].SetColor("_BaseColor", baseColor);
        mats[i].SetFloat("_Shininess", spec);
        mats[i].SetFloat("_Metallicness", met);
        mats[i].SetFloat("_AOMult", ao);
        mats[i].SetFloat("_AbaxialDarkening", abaxDarkening);
        mats[i].SetFloat("_AbaxialPurpleTint", abaxPurple);
        mats[i].SetFloat("_AbaxialHue", abaxHue);
        mats[i].SetFloat("_RimPower", rimPower);
        mats[i].SetColor("_RimColor", c);
        mats[i].SetFloat("_MicroBumps", micro);
        mats[i].SetFloat("_MaskingStrength", maskingStrength);

        mats[i].SetVector("_VertBumps", new Vector4(
            dict[LPK.VertBumpsPower].value, dict[LPK.VertBumpsScale].value, dict[LPK.VertBumpsStretch].value, 0));
        mats[i].SetVector("_VertBumps2", new Vector4(
          dict[LPK.VertBumpsPower2].value, dict[LPK.VertBumpsScale2].value, dict[LPK.VertBumpsStretch2].value, 0));
        mats[i].SetVector("_RadialBumps", new Vector4(
          dict[LPK.RadialBumpsPower].value, dict[LPK.RadialBumpsScale].value,
          dict[LPK.RadialBumpsLenScale].value, dict[LPK.RadialBumpsWidth].value));
        mats[i].SetVector("_Center", new Vector4(center.x, -center.y, 0, 0));

        if (dict[LPK.MaterialHeightAmp].enabled) mats[i].SetFloat("_HeightAmp", heightAmp);
        else mats[i].SetFloat("_HeightAmp", 0f);

        //vertex shader adjustments
        if (true) {
          mats[i].SetBuffer(Shader.PropertyToID("_VertDeltas"), deltasBuffer);
          mats[i].SetBuffer(Shader.PropertyToID("_AdjustedNormals"), normalsBuffer);
          mats[i].SetBuffer(Shader.PropertyToID("_AgeSpots"), ageSpotsBuffer);
          mats[i].SetFloat("_Instances", instances);
          mats[i].SetFloat("_InstanceIdx", i);
          mats[i].SetFloat("_LongCount", baseVertsCount * instances);
        }
      }
    }

    public void SetStemAndTrunkMaterialParams(LeafParamDict dict, Material stemMat, Material trunkMat,
        float stemLen, float leafScale) {
      stemMat.SetColor("_ColorBase", dict[LPK.StemBaseColor].colorValue);
      Color topColor = LeafParamBehaviors.GetColorForParam(dict[LPK.StemTopColorHue], dict);
      stemMat.SetColor("_ColorTop", topColor);
      stemMat.SetFloat("_Bias", dict[LPK.StemColorBias].value);
      stemMat.SetFloat("_Shine", dict[LPK.StemShine].value);

      float baseLines = dict[LPK.StemBaseTexType].value;
      float topLines = dict[LPK.StemTopTexType].value;
      stemMat.SetInt("_BaseUseLines", baseLines < 0f ? 1 : 0);
      stemMat.SetInt("_TopUseLines", topLines < 0f ? 1 : 0);
      stemMat.SetFloat("_TexStrBase", Mathf.Min(1f, Mathf.Max(0f, Mathf.Abs(baseLines) - 0.5f)));
      stemMat.SetFloat("_TexStrTop", Mathf.Min(1f, Mathf.Max(0f, Mathf.Abs(topLines) - 0.5f)));

      float stemRad = 0.25f * dict[LPK.StemWidth].value * leafScale;
      float circum = stemRad * Polar.Pi2;
      float yTiling = stemLen / circum * 4f; //4f is magic because for some reason this math doesn't work
                                             // DebugBW.Log("stemLen: " + stemLen + " | scale: " + leafScale + " stemRad: " + stemRad + " | circum: " + circum + " | yTiling: " + yTiling);
      stemMat.SetFloat("_yTiling", yTiling);

      Color c2 = LeafParamBehaviors.GetColorForParam(dict[LPK.TrunkBrowning], dict);
      trunkMat.SetColor("_ColorBase", c2);
    }

    public void ApplyAllTextures(PlantIndexEntry indexEntry, PlantCollection collection, Material[] mats, Renderer[] renderers) {
      foreach (TextureType type in Enum.GetValues(typeof(TextureType)))
        ApplyTexture(type, indexEntry, collection, mats, renderers);
    }

    /*public void ClearAllTextures(Material mat, Renderer renderer) => ClearAllTextures(mat, new Renderer[] { renderer });
    public void ClearAllTextures(Material mat, Renderer[] renderers) {
      foreach (TextureType type in Enum.GetValues(typeof(TextureType)))
        ApplyTexture(type, "", mat, renderers, true);
    }*/

    private byte[] GetPreloadTextureDataAsync(TextureType texType, PlantIndexEntry indexEntry, PlantCollection collection) =>
      PlantDataManager.GetPreloadTextureDataAsync(indexEntry, texType, collection);

    public void ApplyTexture(TextureType texType, PlantIndexEntry indexEntry, PlantCollection collection,
        Material[] mats, Renderer renderer, bool shouldClear = false) =>
      ApplyTexture(texType, indexEntry, collection, mats, new Renderer[] { renderer }, shouldClear);

    public async void ApplyTexture(TextureType texType, PlantIndexEntry indexEntry, PlantCollection collection,
        Material[] mats, Renderer[] renderers, bool shouldClear = false, byte[] preloadTextureData = null) {
      if (renderers == null || renderers.Length == 0) {
        Debug.LogWarning("No renderers provided to LeafMaterialController.ApplyTexture");
        return;
      }

      SplitTimer st = new SplitTimer("LeafMatController", true, false);
      st.Start();

      // Debug.Log("Applying Texture: " + texType + " | shouldClear: " + shouldClear + " | preloadTextureData: " + preloadTextureData);
      Texture2D tex = null; //BlankTex
      if (!shouldClear) {
        //true == Async texture load each time
        //false == Pull from the preload texture cache
        bool useNewAsync = !GlobalVars.instance.UseTexturePreloadCache;
        if (useNewAsync) {
          if (preloadTextureData == null)
            tex = await PlantDataManager.GetTextureAsync(indexEntry, texType, "apply " + indexEntry.name, collection);
          else
            tex = PlantDataManager.GetPreloadedTexture("apply " + indexEntry.name, preloadTextureData);
        } else {
          tex = PlantDataManager.GetTexture(indexEntry, texType, "apply " + indexEntry.name, collection);
        }
      } else if (shouldClear && texType != TextureType.Albedo) tex = null;
      CoalescingTimer.Coalesce(st, "GetTex");

      if (tex == null && texType != TextureType.Clipping) {
        // Debug.LogWarning("ApplyTexture null texture on leaf " + leafName + "_" + texType);
      }

      MaterialType matType = GetType(mats[0].name);
      string texName = "";
      if (matType == MaterialType.LeafFlat) {
        texName = "_MainTex";
      } else {
        for (int i = 0; i < mats.Length; i++) {
          texName = PropNameForTexType(texType);
          switch (texType) {
            case TextureType.Albedo:
              mats[i].SetInt("_HasAlbedo", tex == null ? 0 : 1);
              break;
            case TextureType.Normal:
              mats[i].SetInt("_HasNormal", tex == null ? 0 : 1);
              break;
            case TextureType.Height:
            case TextureType.VeinMask:
            case TextureType.Clipping:
              break;
            default:
              Debug.LogError("ApplyTexture shader type not supported: " + texType);
              break;
          }
        }
      }

      CoalescingTimer.Coalesce(st, "MatType");

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
      propBlock = null;

      CoalescingTimer.Coalesce(st, "PropBlocks");
    }

    public static Material GetMaterial(MaterialType type, bool makeNew = true) {
      Material m = Resources.Load(MaterialName(type), typeof(Material)) as Material;
      if (makeNew) m = new Material(m);
      return m;
    }

    public static Material[] GetMaterials(MaterialType type, int count) {
      Material m = Resources.Load(MaterialName(type), typeof(Material)) as Material;
      Material[] ms = new Material[count];
      for (int i = 0; i < count; i++)
        ms[i] = new Material(m);
      return ms;
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

    public static void DestroyTextures(MaterialPropertyBlock mpb) {
      foreach (TextureType tt in Enum.GetValues(typeof(TextureType))) {
        Texture t = mpb.GetTexture(PropNameForTexType(tt));
#if UNITY_EDITOR
        UnityEngine.Object.DestroyImmediate(t);
#else
      UnityEngine.Object.Destroy(t);
#endif
      }
    }
  }

}
