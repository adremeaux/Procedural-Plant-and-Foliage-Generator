using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace BionicWombat {
#if UNITY_EDITOR
  [ExecuteInEditMode]
#endif
  [Serializable]
  public class Plant : MonoBehaviour {

    public string leafName = "";
    public bool editMode = false;
    public GameObject bundlePrefab;
    public GameObject trunkPrefab;
    public Transform scaler;
    public LeafDeps deps = new LeafDeps();
    [HideInInspector] public bool needsInspectorRedraw = false;
    [NonSerialized] public bool isEditingPrefab = false;
    [NonSerialized] public bool isAwake = false;

    [SerializeField] private bool isInstantiated = false;
    [SerializeField] private bool viewAsPlant = true;
    [SerializeField] private bool renderingEnabled = true;
    [SerializeField] private bool isHybridizing = false;
    [SerializeField] private PlantCollection collection = PlantCollection.Classic;
    private bool hasInitParamsSet = false;

    [SerializeField] private LeafParamDict fields;
    [SerializeField] private FlowerPotType potType;
    [SerializeField] private LeafDisplayMode displayMode;
    public LightLayers lightLayers = LightLayers.Default | LightLayers.Plant;

    private LeafBundle[] leafBundles;
    private GameObject trunkGameObj;

    private PlantTrunk trunk;
    private Material leafMat;
    private Material stemMat;
    private Material trunkMat;

    private LeafFactoryData lfd;
    private LeafFactory factory;
    private LeafMaterialController matController = new LeafMaterialController();
    private FlowerPotController potController = new FlowerPotController();
    private Dictionary<TextureType, IMTextureFactory> textureFactories;
    private Dictionary<string, Color> cachedSwatches;

    public LeafFactoryData GetLFD() => lfd;
    public PlantTrunk GetTrunk() => trunk;
    public bool GetNeedsRedraw() => needsInspectorRedraw;
    public LeafParamDict GetFields() => fields != null ? fields : LeafParamDefaults.Defaults();
    public bool GetIsInstantiated() => isInstantiated;
    public FlowerPotType GetSelectedPot() => potType;

    public void SetSeed(int seed) => deps.baseParams.RandomSeed = seed;

    public void Awake() {
      Debug.Log("Awake " + leafName + " childCount: " + scaler.childCount);
#if UNITY_EDITOR
      if (PrefabUtility.IsAnyPrefabInstanceRoot(gameObject)) {
        PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
      } else {
        if (!isInstantiated && !Application.isPlaying) {
          Debug.Log("Editing Prefab Plant");
          isEditingPrefab = true;
        }
      }
#endif
    }

    public void SetInitArgs(string preset, FlowerPotType potType, LightLayers lightLayers) {
      leafName = preset;
      this.potType = potType;
      this.lightLayers = lightLayers;
      hasInitParamsSet = true;
    }

    public void SetInitArgs(LeafParamDict fields, string plantName, FlowerPotType potType, LightLayers lightLayers) {
      this.fields = fields;
      this.potType = potType;
      this.lightLayers = lightLayers;
      this.leafName = plantName;
      hasInitParamsSet = true;
    }

    public async void Start() {
      Debug.Log("Start " + leafName + " isInstantiated: " + isInstantiated);
      if (!isInstantiated && !isEditingPrefab) {
        string leafName = this.leafName;
        LeafParamDict cachedDict = fields;
        ClearCache();
        transform.Reset();
        isInstantiated = true;

        if (hasInitParamsSet) {
          ChangePot(potType, false);
          if (cachedDict == null || cachedDict.Count == 0) LoadPreset(leafName);
          else RenderHybrid(cachedDict, leafName);
        } else {
          ChangePot(FlowerPotType.Terracotta, false);
          LoadPreset("Gloriosum");
        }

        SetViewMode(LeafDisplayMode.Plant);
        isAwake = true;
      } else if (!isEditingPrefab) {
        factory = new LeafFactory(deps);
        ChangePot(potType, false);
        await RenderAll(fields, leafName);
        isAwake = true;
      }
    }

    public void ClearCache() {
      Debug.Log("Clear Cache");
      leafName = "";
      // if (fields != null) fields.Clear();
      fields = null;
      matController = new LeafMaterialController();
      if (potController == null) {
        potController = new FlowerPotController();
      }
      potController.Clear();
      InitTextureFactories();
      factory = new LeafFactory(deps);
      ClearBundles();
      viewAsPlant = true;
      renderingEnabled = true;
      stemMat = trunkMat = leafMat = null;
    }

    private void InitTextureFactories() {
      textureFactories = new Dictionary<TextureType, IMTextureFactory>();
      foreach (TextureType type in Enum.GetValues(typeof(TextureType))) {
        textureFactories[type] = new IMTextureFactory(type);
        GetFactoryEnabled(type);
        GetTextureCommandStrings(type);
      }
    }

    public async Task RenderAll(LeafParamDict fields, string leafName, Dictionary<LPType, bool> dirtyDict = null) {
      if (isEditingPrefab) return;
      if (dirtyDict == null) dirtyDict = BaseParams.AllDirty;
      bool allDirty = dirtyDict == BaseParams.AllDirty;
      if (!dirtyDict.Any(a => a.Value)) {
        Debug.LogWarning("Leaf.RenderAll dirtyDict contains no dirty values: " + dirtyDict.ToLog());
        return;
      }

      if (leafName != this.leafName) {
        if (leafName.Length > 0) {
          this.leafName = leafName;
          dirtyDict[LPType.Texture] = true;
        }
      }

      this.fields = fields;
      BWRandom.SetSeed(deps.baseParams.RandomSeed);
      //PrepareSwatches();

      await factory.RenderAll(fields, dirtyDict, deps, !renderingEnabled);
      lfd = factory.lfd;

      bool shouldHardClear = ShouldHardClear();
      // Debug.Log("shouldHardClear: " + shouldHardClear);
      if (shouldHardClear || ShouldRespawnLeafBundles(fields)) ClearBundles(shouldHardClear);
      if (leafBundles == null) SpawnBundles(Arrangement.GetLeafCount(fields));
      if (shouldHardClear) ChangePot(potType, false);

      if (dirtyDict[LPType.Stem] || dirtyDict[LPType.Arrangement]) {
        BWRandom.SetSeed(deps.baseParams.RandomSeed);
        trunk = new PlantTrunk();
        trunk.CreateCurves(fields, deps, ArrangementData.Zero, potController);
        Mesh trunkMesh = StemRenderer.Render(trunk, deps.baseParams.RenderLineSteps * 4);
        trunkGameObj.GetComponent<MeshFilter>().sharedMesh = trunkMesh;
      }

      ArrangementData[] arrangementData = new ArrangementData[0];
      if (dirtyDict[LPType.Arrangement]) {
        arrangementData = ArrangeBundles();
      }

      foreach ((LeafBundle leafBundle, int idx) in leafBundles.WithIndex()) {
        leafBundle.Setup(deps, fields, lightLayers);
        if (dirtyDict[LPType.Stem] || dirtyDict[LPType.Arrangement] || leafBundle.NeedsStem()) {
          if (arrangementData.Length == 0) arrangementData = ArrangeBundles();

          LeafStem stem = new LeafStem();
          stem.CreateCurves(fields, deps, arrangementData[idx], potController);
          leafBundle.leafStem = stem;

          Mesh stemMesh = StemRenderer.Render(stem, deps.baseParams.RenderLineSteps * 2);
          leafBundle.SetMeshes(lfd.leafMesh, stemMesh);
        }
        leafBundle.PositionLeaf(lfd, viewAsPlant);
      }

      if (leafMat == null || stemMat == null || trunkMat == null) FetchMaterials(MaterialType.LeafVelvet);
      if (dirtyDict[LPType.Texture]) {
        if (!isHybridizing) matController.ApplyAllTextures(leafName, collection, leafMat, GetLeafRenderers());
        //else matController.ClearAllTextures(leafMat, GetLeafRenderers());
        SetMaterials();
      }

      if (dirtyDict[LPType.Material]) {
        matController.SetStemAndTrunkMaterialParams(fields, stemMat, trunkMat);
        matController.SetMaterialParams(fields, leafMat);
      }

      potController.SetVisible(true);

      if (allDirty) {
        SetViewMode(displayMode);
      }
    }

    private void SpawnBundles(int count) {
      if (leafBundles != null) {
        Debug.LogError("SpawnBundles needs null leafBundles: " + leafBundles.ToLog());
        return;
      }
      if (bundlePrefab == null || trunkPrefab == null) {
        Debug.LogWarning("SpawnBundles prefab isn't linked");
        return;
      }

      leafBundles = new LeafBundle[count];
      for (int i = 0; i < count; i++) {
        leafBundles[i] = Instantiate(bundlePrefab, Vector3.zero, Quaternion.identity, scaler).GetComponent<LeafBundle>();
        leafBundles[i].transform.Reset();
      }
      trunkGameObj = Instantiate(trunkPrefab, Vector3.zero, Quaternion.identity, scaler);
      trunkGameObj.transform.Reset();
    }

    public ArrangementData[] ArrangeBundles() {
      if (fields == null || potController == null || trunk == null) return new ArrangementData[0];
      BWRandom.SetSeed(deps.baseParams.RandomSeed);
      ArrangementData[] data = Arrangement.Arrange(fields, leafBundles.Length, trunk, potController);
      for (int i = 0; i < data.Length; i++) {
        leafBundles[i].PositionStem(data[i]);
      }
      potController.SetScale(data[0].potScale);
      return data;
    }

    private bool ShouldRespawnLeafBundles(LeafParamDict fields) {
      if (leafBundles == null || trunkGameObj == null ||
          Mathf.RoundToInt(fields[LPK.LeafCount].value) != leafBundles.Length) return true;
      return false;
    }

    private bool ShouldHardClear() => leafBundles == null || scaler.childCount != leafBundles.Length + 2; //leaves + trunk + pot

    public void ClearBundles(bool hardClear = false) {
      if (hardClear) {
        potController.Clear();
        // Debug.Log("Hard Clear count: " + scaler.childCount);
        // foreach (Transform t in scaler.GetComponentsInChildren<Transform>()) {
        GameObject[] objs = new GameObject[scaler.childCount];
        int i = 0;
        foreach (Transform t in scaler) {
          // Debug.Log(t + " : " + scaler.childCount);
          objs[i++] = t.gameObject;
        }
        foreach (GameObject o in objs)
          PDestroy(o);

      } else {
        if (leafBundles == null) return;
        foreach (LeafBundle b in leafBundles) {
          if (b == null) continue;
          PDestroy(b.gameObject);
        }
      }
      leafBundles = null;
      PDestroy(trunkGameObj);
      trunkGameObj = null;
    }

    public static void PDestroy(GameObject o) { if (Application.isPlaying) Destroy(o); else DestroyImmediate(o); }

    public void ChangePot(FlowerPotType type, bool shouldRearrange) {
      // Debug.Log(type + " | " + potType + " | has: " + potController.hasPot);
      if (type == potType && potController.hasPot) return;

      potType = type;
      potController.LoadPot(type, scaler);
      potController.SetVisible(fields != null && fields.Count > 0);
      if (shouldRearrange) ArrangeBundles();
    }

    public void ApplyOrCreateAllTextures(LeafParamDict fields, PlantCollection collection) {
      List<TextureType> missingTextures = DataManager.MissingTexturesForLeaf(leafName, collection);
      IMTextureVars textureVars = GetTextureVars(fields);
      foreach (TextureType type in Enum.GetValues(typeof(TextureType))) {
        if (missingTextures.Contains(type))
          CreateTexture(fields, type, textureVars);
        else
          ApplyTexture(type);
      }
    }

    private void ApplyTexture(TextureType type) =>
      matController.ApplyTexture(type, leafName, collection, leafMat, GetLeafRenderers());

    public void CreateAllTextures(LeafParamDict fields) {
      Debug.Log("CreateAllTextures");
      IMTextureVars textureVars = GetTextureVars(fields);
      foreach (TextureType type in Enum.GetValues(typeof(TextureType)))
        CreateTexture(fields, type, textureVars);
    }

    private IMTextureVars GetTextureVars(LeafParamDict fields) =>
      IMTextureFactory.GetTextureVars(lfd.leafShape.curves, lfd.leafVeins, fields, deps, leafName, collection);

    private async void CreateTexture(LeafParamDict fields, TextureType type, IMTextureVars textureVars) {
      if (textureFactories == null) InitTextureFactories();
      IMTextureFactory textureFactory = textureFactories[type];
      if (!textureFactory.enabled) return;
      textureFactory.Prepare(deps, textureVars);

      int[] samples = new int[] { 1 };
      foreach (int targetScale in samples) {
        await textureFactory.DrawTexture(type, targetScale);
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
        ApplyTexture(type);
        SetMaterials();
      }
    }

    public void FetchMaterials(MaterialType type) {
      stemMat = LeafMaterialController.GetMaterial(MaterialType.Stem);
      trunkMat = LeafMaterialController.GetMaterial(MaterialType.Trunk);
      leafMat = LeafMaterialController.GetMaterial(type);
      SetMaterials();
    }

    private void SetMaterials() {
      foreach (LeafBundle leafBundle in leafBundles)
        leafBundle.SetMaterials(leafMat, stemMat);
      trunkGameObj.GetComponent<MeshRenderer>().material = trunkMat;
    }

    public void ResetMats() {
      if (LeafMaterialController.IsFlatMat(leafMat)) {
        matController.ApplyTexture(TextureType.Normal, leafName, collection, leafMat, GetLeafRenderers());
      } else {
        foreach (TextureType type in Enum.GetValues(typeof(TextureType)))
          matController.ApplyTexture(type, leafName, collection, leafMat, GetLeafRenderers());
      }
      SetMaterials();
    }

    public Renderer[] GetLeafRenderers() => leafBundles.Select<LeafBundle, Renderer>(b => b.leafMeshRenderer).ToArray();
    public Renderer[] GetStemRenderers() => leafBundles.Select<LeafBundle, Renderer>(b => b.stemMeshRenderer).ToArray();

    public async void SetViewMode(LeafDisplayMode mode) {
      void ViewAsPlantMode(bool viewAsPlant, bool showOnlyFirst) {
        this.viewAsPlant = viewAsPlant;
        renderingEnabled = showOnlyFirst;
        if (leafBundles != null) {
          foreach (LeafBundle leafBundle in leafBundles) {
            bool isFirst = leafBundle == leafBundles.First();
            leafBundle.SetVisible(viewAsPlant || (showOnlyFirst && isFirst));
            leafBundle.PositionLeaf(lfd, viewAsPlant);
          }
          if (viewAsPlant) {
            BWRandom.SetSeed(deps.baseParams.RandomSeed);
            ArrangeBundles();
            deps.inspector.SetDisplayMode(LeafDisplayMode.FullMesh);
          }
        }
        potController.SetEnabled(viewAsPlant);
        float scaleTarget = viewAsPlant ? 0.05f : 1f;
        scaler.localScale = new Vector3(scaleTarget, scaleTarget, scaleTarget);
      }

      displayMode = mode;
      deps.inspector.SetDisplayMode(mode);
      bool lastDirtortion = deps.baseParams.HideDistortion;
      deps.baseParams.HideDistortion = LeafInspectorParams.HideDistortionForMode(mode);
      deps.baseParams.HideTrunk = LeafInspectorParams.HideTrunkForMode(mode);
      if (trunkGameObj != null) trunkGameObj.SetActive(!deps.baseParams.HideTrunk);

#if UNITY_EDITOR
      if (editMode) {
        SceneView.lastActiveSceneView.cameraMode = SceneView.GetBuiltinCameraMode(LeafInspectorParams.DrawModeForMode(mode));
        if (LeafInspectorParams.Use2DForMode(mode) is bool use2D)
          SceneView.lastActiveSceneView.in2DMode = use2D;
      }
#endif

      bool showMesh = LeafInspectorParams.ShowMeshForMode(mode);
      ViewAsPlantMode(LeafInspectorParams.AttachStemForMode(mode), showMesh);

      if (lastDirtortion != deps.baseParams.HideDistortion) {
        Dictionary<LPType, bool> dict = BaseParams.AllClean;
        dict[LPType.Distort] = true;
        await RenderAll(fields, leafName, dict);
      }
    }

    public async void SetPolyDepth(int depth) {
      int[] rendSteps = new int[] { 1, 6, 10, 10 };
      int[] subdivSteps = new int[] { 0, 0, 1, 2 };
      deps.baseParams.RenderLineSteps = rendSteps[depth];
      deps.baseParams.SubdivSteps = subdivSteps[depth];
      await RenderAll(fields, leafName);
    }

    /*public async void Hybridize(LeafParamDict fields1, LeafParamDict fields2) {
      needsInspectorRedraw = true;
      isHybridizing = true;

      LeafParamDict hybrid = Hybridizer.Hybridize(fields1, fields2);
      leafName = "Hybrid";
      matController.ClearAllTextures(leafMat, GetLeafRenderers());
      PresetManager.AddToCollection(PlantCollection.Temporary, leafName);

      await RenderAll(hybrid, leafName);
      CreateAllTextures(hybrid);
      isHybridizing = false;
    }*/

    public async void LoadPreset(string selectedPreset) {
      Debug.Log("Loading preset: " + selectedPreset);
      isHybridizing = false;
      collection = PlantCollection.Classic;
      leafName = selectedPreset;
      LeafParamDict preset = PresetManager.LoadPreset(selectedPreset, PlantCollection.Classic);
      await RenderAll(preset, selectedPreset);
      needsInspectorRedraw = true;
    }

    public async void RenderHybrid(LeafParamDict dict, string leafName) {
      isHybridizing = true;
      collection = PlantCollection.Temporary;
      PresetManager.AddToCollection(collection, leafName);
      await RenderAll(dict, leafName);
      ApplyOrCreateAllTextures(fields, PlantCollection.Temporary);
      isHybridizing = false;
    }

    //Relies on Shapes library
    /*private void PrepareSwatches() {
      if (!deps.inspector.showSwatches) {
        cachedSwatches = new Dictionary<string, Color>();
        return;
      }
      LPK[] keys = ColorSwatches.Keys;
      Dictionary<string, Color> d = new Dictionary<string, Color>();
      foreach (LPK key in keys) {
        string name = key.ToString();
        LeafParam param = fields[key];
        Color c = param.hasColorValue ? param.colorValue :
          LeafParamBehaviors.GetColorForParam(param, fields);
        d.Add(name, c);
      }
      cachedSwatches = d;
    }*/

    public Dictionary<string, Color> GetSwatches() {
      return editMode ? cachedSwatches : null;
    }

    [SerializeField][HideInInspector] TextureCommandDictCache cachedCommandStrings;

    public TextureCommandDict GetTextureCommandStrings(TextureType type) {
      if (textureFactories == null || textureFactories.Count == 0) InitTextureFactories();

      IMTextureFactory textureFactory = textureFactories[type];
      if (cachedCommandStrings.ContainsKey(type)) {
        textureFactory.SetTextureCommandStrings(cachedCommandStrings[type], type);
      }
      return textureFactory.GetTextureCommandStrings(type);
    }

    public void SetTextureCommandStrings(TextureCommandDict dict, TextureType type) {
      if (textureFactories == null || textureFactories.Count == 0) InitTextureFactories();

      IMTextureFactory textureFactory = textureFactories[type];
      if (cachedCommandStrings == null) cachedCommandStrings = new TextureCommandDictCache();
      textureFactory.SetTextureCommandStrings(dict, type);
      cachedCommandStrings[type] = dict;
    }

    public bool GetFactoryEnabled(TextureType type) {
      if (textureFactories == null || textureFactories.Count == 0) InitTextureFactories();
      IMTextureFactory textureFactory = textureFactories[type];
      if (cachedCommandStrings.ContainsKey(type)) {
        if (cachedCommandStrings[type].ContainsKey("enabled"))
          textureFactory.enabled = cachedCommandStrings[type]["enabled"];
        else textureFactory.enabled = true;
      }

      return textureFactory.enabled;
    }

    public void SetFactoryEnabled(TextureType type, bool enabled) {
      if (textureFactories == null || textureFactories.Count == 0) InitTextureFactories();
      if (!cachedCommandStrings.ContainsKey(type)) cachedCommandStrings[type] = new TextureCommandDict();
      textureFactories[type].enabled = enabled;

      cachedCommandStrings[type]["enabled"] = enabled;
    }
  }

}