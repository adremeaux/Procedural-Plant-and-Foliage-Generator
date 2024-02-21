using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace BionicWombat {
  public interface IPlantRenderingResponder {
    void RenderComplete(Weak<Plant> weakPlant, PlantIndexEntry entry, Plant.RenderQuality renderQuality, bool texturesAvailable);
  }

#if UNITY_EDITOR
  [ExecuteInEditMode]
#endif
  [Serializable]
  public class Plant : MonoBehaviour {
    public enum RenderQuality {
      Current = 0,
      Custom,
      Minimum,
      Medium,
      MediumThenMaximum,
      Maximum,
    }

    public PlantIndexEntry indexEntry;
    public GameObject bundlePrefab;
    public GameObject trunkPrefab;
    public Transform scaler;
    public PlantPhysicsSimulator physicsSim;
    public ComputeShader computeShaderDistort;
    public ComputeShader computeShaderExtrude;
    public ComputeShader computeShaderNormals;
    public LeafDeps deps = new LeafDeps();
    private Weak<IPlantRenderingResponder> renderingResponder;

    [HideInInspector] public bool needsInspectorRedraw = false;
    [NonSerialized] public bool isEditingPrefab = false;
    [NonSerialized] public bool isAwake = false;
    [NonSerialized] public bool shouldCachePlant = false;
    [SerializeField] private bool isInstantiated = false;
    [SerializeField] private bool viewAsPlant = true;
    [SerializeField] private bool renderingEnabled = true;
    [SerializeField] private bool isHybridizing = false;
    [SerializeField] private bool didLoadFromCache = false;
    [SerializeField] public PlantCollection collection;
    private bool shouldRenderTextures = false;
    private bool hasInitParamsSet = false;

    [SerializeField][HideInInspector] private LeafParamDict fields;
    [SerializeField][HideInInspector] private FlowerPotType potType;
    [SerializeField][HideInInspector] private LeafDisplayMode displayMode = LeafDisplayMode.Plant;
    [SerializeField][HideInInspector] public RenderQuality currentRenderQuality;
    private LightLayers lightLayers = LightLayers.Default | LightLayers.Plant;

    private LeafBundle[] leafBundles;
    private GameObject trunkGameObj;

    private PlantTrunk trunk;
    private Material[] leafMats;
    private Material stemMat;
    private Material trunkMat;

    private LeafFactoryData lfd;
    private LeafFactory factory;
    private ArrangementData[] arrangementData;
    private LeafMaterialController matController = new LeafMaterialController();
    private FlowerPotController potController = new FlowerPotController();
    private Dictionary<TextureType, IMTextureFactory> textureFactories;
    private Dictionary<string, Color> cachedSwatches;
    private (int count, int total) textureRenderCount = (-1, -1);

    public LeafFactoryData GetLFD() => lfd;
    public ArrangementData[] GetArrangementData() => arrangementData;
    public PlantTrunk GetTrunk() => trunk;
    public LeafBundle[] GetLeafBundles() => leafBundles.Copy();
    public bool GetNeedsRedraw() => needsInspectorRedraw;
    public LeafParamDict GetFields() => fields != null ? fields : LeafParamDefaults.Defaults;
    public bool GetIsInstantiated() => isInstantiated;
    public FlowerPotType GetSelectedPot() => potType;
    public PlantCollection SetCollection(PlantCollection col) => collection = col;

    public void SetSeed(int seed) => deps.leafData.randomSeed = seed;
    public void RandomizeSeed() => SetSeed(Math.Abs(Guid.NewGuid().GetHashCode()));

    public void Awake() {
      // Debug.Log("Awake " + leafName + " childCount: " + scaler.childCount);
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

    private void OnDestroy() {
      DebugBW.Log("  (Destroy plant object " + indexEntry.name + ")", LColor.grey);
      isInstantiated = false;
    }

    public void SetInitArgs(PlantData data, FlowerPotType potType, LightLayers lightLayers,
      RenderQuality renderQuality, bool shouldRenderTextures) {
      this.fields = data.fields;
      this.collection = data.collection;
      this.potType = potType;
      this.lightLayers = lightLayers;
      this.indexEntry = data.indexEntry;
      this.name = "Plant (" + indexEntry.name + ")";
      this.shouldRenderTextures = shouldRenderTextures;
      SetSeed(data.seed);
      hasInitParamsSet = true;
      isEditingPrefab = false;
      SetRenderQuality(renderQuality);
    }

    public void SetRenderingResponder(IPlantRenderingResponder r) { renderingResponder = new Weak<IPlantRenderingResponder>(r); }
    public void SetRenderQuality(RenderQuality q) {
      if (q == currentRenderQuality || q == RenderQuality.Current) return;
      currentRenderQuality = q;
      if (q == RenderQuality.Custom) return;
      SetRenderQualityParams(q);
    }
    private void SetRenderQualityParams(RenderQuality q) {
      // Debug.Log("set render qual");
      if (q == RenderQuality.Custom || q == RenderQuality.Current) return;
      deps.baseParams.SubdivSteps = q == RenderQuality.Maximum ? 0 : 0; //!!!
      deps.baseParams.RenderLineSteps = q == RenderQuality.Maximum ? 32 : q != RenderQuality.Custom ? 12 : 4;
      deps.baseParams.TextureDownsample = q == RenderQuality.Maximum ? 1 : q > RenderQuality.Minimum ? 2 : 4;
    }

    public void SetLightLayers(LightLayers layers) {
      this.lightLayers = layers;

      LightLayers potLight = lightLayers & (~LightLayers.Plant);
      if (potLight == 0) potLight = LightLayers.Plant;
      potController?.SetLightLayers(potLight);

      DebugBW.Log("SetLightLayers " + layers + " on " + indexEntry);
      if (leafBundles != null)
        foreach (LeafBundle bundle in leafBundles)
          bundle.SetLightLayers(layers);

      if (trunkGameObj != null)
        trunkGameObj.GetComponent<MeshRenderer>().renderingLayerMask = (uint)lightLayers;
    }

    public async void Start() {
      // DebugBW.Log("Start " + leafName + " isInstantiated: " + isInstantiated + " | isEditingPrefab: " + isEditingPrefab + " | hasInitParamsSet: " + hasInitParamsSet + " | displayMode: " + displayMode, LColor.aqua);
      if (!isInstantiated && !isEditingPrefab) {
        PlantIndexEntry entry = this.indexEntry;
        LeafParamDict cachedDict = fields;
        LeafDisplayMode cachedMode = displayMode;
        ClearCache();
        transform.Reset();
        isInstantiated = true;

        if (hasInitParamsSet) {
          ChangePot(potType, false);
          PlantData pd = new PlantData(fields, entry, collection, deps.leafData.randomSeed);
          if ((PlantDataManager.PlantExistsForData(pd) && !shouldRenderTextures) ||
            (cachedDict == null || cachedDict.Count == 0)) LoadPreset(pd, true);
          else RenderHybrid(new PlantData(cachedDict, entry, collection, deps.leafData.randomSeed));
        } else {
          ChangePot(FlowerPotType.NurseryBlack, false);
          LoadPreset(new PlantData(PlantDataManager.TryGetIndexEntry("Gloriosum", PlantCollection.Classic), PlantCollection.Classic), false);
        }

        shouldRenderTextures = false;

        SetViewMode(cachedMode);
        isAwake = true;
      } else if (!isEditingPrefab) {
        if (!didLoadFromCache) {
          Debug.Log("did not load from cache");
          factory = new LeafFactory(deps);
          ChangePot(potType, false);
          await RenderAll(fields, indexEntry, currentRenderQuality);
        }
        isAwake = true;
      }
    }

    public void ClearCache() {
      indexEntry = default(PlantIndexEntry);
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
      stemMat = trunkMat = null;
      leafMats = null;
      displayMode = LeafDisplayMode.Plant;
    }

    public void Clear() {
      ClearBundles(true);
      displayMode = LeafDisplayMode.Plant;
    }

    private void InitTextureFactories() {
      textureFactories = new Dictionary<TextureType, IMTextureFactory>();
      foreach (TextureType type in Enum.GetValues(typeof(TextureType))) {
        textureFactories[type] = new IMTextureFactory(type);
        GetFactoryEnabled(type);
        GetTextureCommandStrings(type);
      }
    }

    private int enqueueRenderTicks = -1;
    private LeafParamDict enqFields;
    private PlantIndexEntry enqIndexEntry;
    private Dictionary<LPType, bool> enqDirtyDict;
    private RenderQuality queuedRenderQuality;
    public void EnqueueRenderAll(LeafParamDict fields, PlantIndexEntry indexEntry, RenderQuality renderQuality, Dictionary<LPType, bool> dirtyDict, int frameCount = 0) {
      // DebugBW.Log("Enqueue" + leafName, LColor.lightblue);
      enqueueRenderTicks = frameCount;
      enqFields = fields;
      enqIndexEntry = indexEntry;
      enqDirtyDict = dirtyDict;
      queuedRenderQuality = renderQuality;
    }

    private async void Update() {
      if (enqueueRenderTicks == 0) {
        enqueueRenderTicks = -1;
        await RenderAll(enqFields, enqIndexEntry, queuedRenderQuality, enqDirtyDict);
      } else {
        enqueueRenderTicks--;
      }

      if (shouldCachePlant) {
        shouldCachePlant = false;
        CachePlantToDisk();
      }
    }

    public async Task RenderAll(LeafParamDict fields, PlantIndexEntry indexEntry, RenderQuality renderQuality, Dictionary<LPType, bool> dirtyDict = null) {
      if (isEditingPrefab) return;
      if (dirtyDict == null) dirtyDict = BaseParams.AllDirty;
      bool allDirty = dirtyDict == BaseParams.AllDirty;
      if (!dirtyDict.Any(a => a.Value)) {
        Debug.LogWarning("Leaf.RenderAll dirtyDict contains no dirty values: " + dirtyDict.ToLog());
        return;
      }

      // DebugBW.Log("RenderAll " + leafName + " | renderQuality: " + renderQuality, LColor.olive);

      if (renderQuality == RenderQuality.Current) {
        if (currentRenderQuality == RenderQuality.Current) SetRenderQuality(RenderQuality.Medium);
        renderQuality = currentRenderQuality;
      } else {
        SetRenderQualityParams(renderQuality);
      }

      if (indexEntry != this.indexEntry) {
        if (indexEntry.IsNotDefault()) {
          this.indexEntry = indexEntry;
          dirtyDict[LPType.Texture] = true;
        }
      }

      this.fields = fields;
      int oldLeafCount = Arrangement.GetLeafCount(this.fields);
      BWRandom.SetSeed(deps.leafData.randomSeed);
      PrepareSwatches();

      bool didRenderMesh = await factory.RenderAll(fields, dirtyDict, renderQuality, deps, !renderingEnabled);
      if (!didRenderMesh) {
        if (renderQuality == RenderQuality.Minimum) {
          await RenderAll(fields, indexEntry, RenderQuality.Medium, dirtyDict);
        } else if (renderQuality == RenderQuality.Medium || renderQuality == RenderQuality.MediumThenMaximum) {
          await RenderAll(fields, indexEntry, RenderQuality.Maximum, dirtyDict);
        } else {
          Debug.LogError("Leaf Rendering failed for " + indexEntry.name + " at quality " + renderQuality);
        }
        return;
      }
      if (renderQuality == RenderQuality.MediumThenMaximum) {
        EnqueueRenderAll(fields, indexEntry, RenderQuality.Maximum, dirtyDict, 5);
      }

      bool shouldHardClear = ShouldHardClear();
      // Debug.Log("shouldHardClear: " + shouldHardClear);
      if (shouldHardClear || ShouldRespawnLeafBundles(fields)) ClearBundles(shouldHardClear);
      if (leafBundles == null) SpawnBundles(Arrangement.GetLeafCount(fields));
      if (shouldHardClear) ChangePot(potType, false);

      if (dirtyDict[LPType.Stem] || dirtyDict[LPType.Arrangement]) {
        BWRandom.SetSeed(BWRandomPlantShop.GenTypedSeed(deps.leafData.randomSeed, LPType.Stem));
        trunk = new PlantTrunk();
        trunk.CreateCurves(fields, ArrangementData.Zero, potController);
        Mesh trunkMesh = StemRenderer.Render(trunk, deps.baseParams.RenderLineSteps * 4);
        trunkGameObj.GetComponent<MeshFilter>().sharedMesh = trunkMesh;
        trunkGameObj.GetComponent<MeshRenderer>().renderingLayerMask = (uint)lightLayers;
      }

      bool needsPhysics = false;
      if (dirtyDict[LPType.Arrangement]) {
        ArrangeBundles();
        needsPhysics = true;
      }

      bool force = leafBundles.Length != oldLeafCount;
      bool newMeshes = await factory.DistortAndExtrudeLeafMeshes(computeShaderDistort, computeShaderExtrude, computeShaderNormals, fields, leafBundles.Length, dirtyDict, deps, force);
      lfd = factory.lfd;

      foreach ((LeafBundle leafBundle, int idx) in leafBundles.WithIndex()) {
        leafBundle.Setup(deps, fields, lightLayers);
        LeafStem stem = new LeafStem();
        stem.CreateCurves(fields, arrangementData[idx], potController);
        leafBundle.leafStem = stem;
        leafBundle.PositionLeaf(lfd, arrangementData[idx], viewAsPlant);
      }

      if (needsPhysics) {
        foreach (LeafBundle bundle in leafBundles) bundle.ResetCollisionAdjustment();
        Vector3[] adjustments = physicsSim.SolveCollisions(
          PlantPhysicsSimulator.CreateLeafData(leafBundles, lfd, fields),
          PlantPhysicsSimulator.CreatePotData(potController)
        );
        bool allHidden = adjustments.All(v => v.Equals(PlantPhysicsSimulator.CollideWithPotVector));
        if (allHidden || deps.baseParams.SkipPhysics)
          for (int i = 0; i < adjustments.Length; i++)
            adjustments[i] = Vector3.zero;

        for (int i = 0; i < adjustments.Length; i++)
          leafBundles[i].SetCollisionAdjustment(adjustments[i], viewAsPlant && displayMode != LeafDisplayMode.Propegating);
        ArrangeBundles(false);
        physicsSim.Cleanup(leafBundles);
      }

      //Build stem meshes after physics calcs
      foreach ((LeafBundle leafBundle, int idx) in leafBundles.WithIndex()) {
        if (newMeshes || dirtyDict[LPType.Stem] || dirtyDict[LPType.Arrangement] || leafBundle.NeedsStem()) {
          LeafStem stem = leafBundle.leafStem;
          if (displayMode != LeafDisplayMode.Propegating)
            stem.AddBaseExtension(leafBundle.collisionAdjustment, leafBundle.transform.localRotation.eulerAngles.y);
          int stemPoints = deps.baseParams.RenderLineSteps * 2;
          Mesh stemMesh = StemRenderer.Render(stem, stemPoints);
          Mesh perLeafMesh = lfd.leafMesh;

          BendPhysics bend = leafBundle.GetComponent<BendPhysics>();
          bend.CreateBones(stem, leafBundle, stemPoints, lfd, scaler.localScale.x, indexEntry.name);
          leafBundle.SetMeshes(perLeafMesh, stemMesh, true);
        }
      }

      if (leafMats == null || stemMat == null || trunkMat == null) FetchMaterials(MaterialType.LeafVelvet);
      if (dirtyDict[LPType.Texture]) {
        if (!isHybridizing) matController.ApplyAllTextures(indexEntry, collection, leafMats, GetLeafRenderers());
        //else matController.ClearAllTextures(leafMat, GetLeafRenderers());
      }

      // if (dirtyDict[LPType.Material] || dirtyDict[LPType.Distort]) {
      matController.SetStemAndTrunkMaterialParams(fields, stemMat, trunkMat,
        GetAvgStemLength(), arrangementData[arrangementData.Length / 2].scale);
      matController.SetMaterialParams(fields, leafMats,
        factory.deltaVertsBuffer, factory.adjustedNormalsBuffer, factory.ageSpotsBuffer,
        deps.baseParams.DistortionInstances, lfd.baseMesh.vertexCount, lfd.center);
      SetMaterials();
      // }

#if UNITY_EDITOR
      if (dirtyDict[LPType.Distort] || dirtyDict[LPType.Leaf]) {
        if (deps.inspector.showVertexNormals)
          GetComponent<DebugTriangleNormals>().SetVertexNormals(GetVertexNormals());
        if (deps.inspector.showSurfaceNormals)
          GetComponent<DebugTriangleNormals>().SetSurfaceNormals(GetSurfaceNormals());
      }
#endif

      potController.SetVisible(true);

      if (allDirty || !Application.isPlaying) {
        SetViewMode(displayMode);
      }

      string hash = "";
      await Task.Run(() => {
        hash = BWSaving.PresetManager.SavedPlantHash(new SavedPlant(
         indexEntry.name, indexEntry.uniqueID, fields, deps.leafData.randomSeed, SavedPlant.VersionNumber));
      });
      // DebugBW.Log("Render Complete: " + indexEntry);

      if (renderingResponder.Check()) {
        bool needsTextures = PlantDataManager.MissingTexturesForLeaf(indexEntry, collection).HasLength();
        // DebugBW.Log("  needsTextures: " + needsTextures + " | col: " + collection + " | leafName: " + leafName, LColor.orange);
        renderingResponder.obj.RenderComplete(new Weak<Plant>(this), indexEntry, renderQuality, !needsTextures);
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

    public void ArrangeBundles(bool redoArrangement = true) {
      if (fields == null || potController == null || trunk == null) {
        arrangementData = new ArrangementData[0];
        return;
      }

      if (redoArrangement || !arrangementData.HasLength()) {
        BWRandom.SetSeed(BWRandomPlantShop.GenTypedSeed(deps.leafData.randomSeed, LPType.Arrangement));
        ArrangementData[] data = Arrangement.Arrange(fields, leafBundles.Length, trunk, potController);
        arrangementData = data;
      }
      for (int i = 0; i < arrangementData.Length; i++) {
        if (displayMode == LeafDisplayMode.Propegating && i == arrangementData.Length - 1) {
          ArrangementData ad = arrangementData[i].Copy();
          float flopPerc = LeafStem.GetFlopPerc(fields, ad);
          ad.stemRotation = Quaternion.Euler(0f, 40f, flopPerc * 100f / 2f);
          ad.pos = new Vector3(0f, Arrangement.GetPotYAdd(fields, potController));
          leafBundles[i].ResetCollisionAdjustment();
          leafBundles[i].PositionStem(ad);
        } else {
          leafBundles[i].PositionStem(arrangementData[i]);
        }
      }
      potController.SetScale(arrangementData[0].potScale);
      scaler.localRotation = Quaternion.identity;
    }

    private bool ShouldRespawnLeafBundles(LeafParamDict fields) {
      if (leafBundles == null || trunkGameObj == null ||
          Mathf.RoundToInt(fields[LPK.LeafCount].value) != leafBundles.Length) return true;
      return false;
    }

    private bool ShouldHardClear() => leafBundles == null || scaler.childCount != leafBundles.Length + 2; //leaves + trunk + pot

    public void ClearBundles(bool hardClear = false) {
      bool play = Application.isPlaying;
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
      LightLayers potLight = lightLayers & (~LightLayers.Plant);
      if (potLight == 0) potLight = LightLayers.Plant;
      potController.SetLightLayers(potLight);
      potController.LoadPot(type, scaler, false);
      potController.SetVisible(fields != null && fields.Count > 0);
      if (shouldRearrange) ArrangeBundles();
    }

    public void ApplyOrCreateAllTextures(LeafParamDict fields, PlantCollection collection, bool shouldApply) {
      List<TextureType> missingTextures = PlantDataManager.MissingTexturesForLeaf(indexEntry, collection);
      // DebugBW.Log("ApplyOrCreateAllTextures: " + missingTextures, LColor.lightblue);
      IMTextureVars textureVars = missingTextures.HasLength() ? GetTextureVars(fields) : null;
      if (missingTextures.Count > 0) Debug.Log($"Creating missing textures for {indexEntry}: " + missingTextures.ToLog() +
        " quality " + currentRenderQuality + " textureDownsample " + textureVars.downsample);
      textureRenderCount = (0, missingTextures.Count);
      var texTypes = Enum.GetValues(typeof(TextureType));

      foreach (TextureType type in texTypes) {
        if (missingTextures.Contains(type))
          CreateTexture(fields, type, textureVars);
        else if (shouldApply)
          ApplyTexture(type);
      }
    }

    private void ApplyTexture(TextureType type, byte[] preloadData = null) {
      // DebugBW.Log("        " + leafName + " " + System.Reflection.MethodBase.GetCurrentMethod().Name, LColor.lightblue);
      matController.ApplyTexture(type, indexEntry, collection, leafMats,
        GetLeafRenderers(), false, preloadData);
    }

    public void CreateAllTextures(LeafParamDict fields) {
      Debug.Log("CreateAllTextures");
      IMTextureVars textureVars = GetTextureVars(fields);
      int total = 0;
      foreach (TextureType type in Enum.GetValues(typeof(TextureType))) {
        CreateTexture(fields, type, textureVars);
        if (GetFactoryEnabled(type)) total++;
      }
      textureRenderCount = (0, total);
    }

    public IMTextureVars GetTextureVars(LeafParamDict fields) =>
      IMTextureFactory.GetTextureVars(lfd.leafShape.curves, lfd.leafVeins, fields, deps, indexEntry, collection);

    private async void CreateTexture(LeafParamDict fields, TextureType type, IMTextureVars textureVars) {
      int rand = BWRandom.UnseededInt(0, 10000);
      // Debug.Log("CreateTexture (" + rand + ") " + type + " name " + textureVars.leafName);
      if (textureFactories == null) InitTextureFactories();
      IMTextureFactory textureFactory = textureFactories[type];
      if (!textureFactory.enabled) return;
      textureFactory.Prepare(deps, textureVars);

      int[] samples = new int[] { 1 };
      foreach (int targetScale in samples) {
        await textureFactory.DrawTexture(type, targetScale);
#if UNITY_EDITOR
        if (!Application.isPlaying) UnityEditor.AssetDatabase.Refresh();
#endif
        if (isInstantiated) {
          ApplyTexture(type);
          SetMaterials();
        }
        // Debug.Log("Finished " + type);
      }

      if (textureRenderCount.total != -1) {
        if (++textureRenderCount.count == textureRenderCount.total) {
          textureRenderCount = (-1, -1);
          Debug.Log("All textures finished rendering");
          if (!Application.isPlaying)
            await RenderAll(fields, indexEntry, RenderQuality.Current);
          else
            renderingResponder?.obj?.RenderComplete(new Weak<Plant>(this), indexEntry, currentRenderQuality, true);
        }
      }
      // Debug.Log("CreateTexture " + textureVars.leafName + "_" + type + " finished");
    }

    public void FetchMaterials(MaterialType type) {
      stemMat = LeafMaterialController.GetMaterial(MaterialType.Stem);
      // DebugBW.Log("stemMat: " + stemMat + " | stemMat.name: " + stemMat.name);
      trunkMat = LeafMaterialController.GetMaterial(MaterialType.Trunk);
      leafMats = LeafMaterialController.GetMaterials(type, deps.baseParams.DistortionInstances);
      SetMaterials();
    }

    private void SetMaterials() {
      foreach ((LeafBundle leafBundle, int idx) in leafBundles.WithIndex())
        leafBundle.SetMaterials(leafMats[idx % 3], stemMat);
      trunkGameObj.GetComponent<MeshRenderer>().material = trunkMat;
    }

    public void ResetMats() {
      if (leafMats == null) return;
      Debug.Log("Reset Mats " + indexEntry);
      if (LeafMaterialController.IsFlatMat(leafMats.First())) {
        matController.ApplyTexture(TextureType.Normal, indexEntry, collection, leafMats, GetLeafRenderers());
      } else {
        foreach (TextureType type in Enum.GetValues(typeof(TextureType)))
          matController.ApplyTexture(type, indexEntry, collection, leafMats, GetLeafRenderers());
      }
      SetMaterials();
    }

    private Renderer[] GetLeafRenderers() => leafBundles.Select<LeafBundle, Renderer>(b => b.leafMeshRenderer).ToArray();
    private Renderer[] GetStemRenderers() => leafBundles.Select<LeafBundle, Renderer>(b => b.stemMeshRenderer).ToArray();

    public NormalVector[] GetVertexNormals() {
      Vector3[] verts = lfd.baseMesh.vertices;
      NormalVector[] vecs = new NormalVector[verts.Length];
      int instanceIdx = deps.baseParams.InstanceIdxFromRandomBS();
      int offset = instanceIdx * verts.Length;
      int mult = instanceIdx + 1;

      Vector3[] normals = new Vector3[verts.Length * mult];
      if (factory.adjustedNormalsBuffer != null) {
        factory.adjustedNormalsBuffer.GetData(normals);
      } else {
        Array.Fill(normals, Vector3.back);
      }

      Vector3[] deltas = new Vector3[verts.Length * mult];
      if (factory.deltaVertsBuffer != null) {
        factory.deltaVertsBuffer.GetData(deltas);
      } else {
        Array.Fill(deltas, Vector3.zero);
      }

      for (int i = 0; i < verts.Length; i++) {
        vecs[i].origin = verts[i] + deltas[i + offset];
        vecs[i].normal = normals[i + offset];
      }

      return vecs;
    }

    public NormalVector[] GetSurfaceNormals() => factory.surfaceNormals;

    public async void SetViewMode(LeafDisplayMode mode) {
      // DebugBW.Log("SetViewMode: " + mode + " | " + leafName + " | displayMode: " + displayMode);
      void ViewAsPlantMode(bool viewAsPlant, bool showOnlyFirst, int idxForSolo) {
        this.viewAsPlant = viewAsPlant;
        renderingEnabled = showOnlyFirst;
        if (leafBundles != null) {
          foreach ((LeafBundle leafBundle, int idx) in leafBundles.WithIndex()) {
            if (mode != LeafDisplayMode.Propegating) {
              bool isFirst = leafBundle == leafBundles[idxForSolo];
              leafBundle.SetVisible(viewAsPlant || (showOnlyFirst && isFirst));
            } else {
              bool isLast = leafBundle == leafBundles.Last();
              leafBundle.SetVisible(isLast);
            }
            leafBundle.PositionLeaf(lfd, arrangementData[idx], viewAsPlant);
          }
          if (viewAsPlant) {
            ArrangeBundles();
            deps.inspector.SetDisplayMode(mode);
          }
        }
        potController.SetEnabled(viewAsPlant);
        float scaleTarget = viewAsPlant ? 0.05f : 1f;
        scaler.localScale = new Vector3(scaleTarget, scaleTarget, scaleTarget);
      }

      // if (mode == displayMode) return;
      // bool rearrange = displayMode == LeafDisplayMode.Propegating;
      displayMode = mode;
      // if (rearrange) await RenderAll(fields, leafName, RenderQuality.Current);

      deps.inspector.SetDisplayMode(mode);
      bool lastDirtortion = deps.baseParams.HideDistortion;
      deps.baseParams.HideDistortion = LeafInspectorParams.HideDistortionForMode(mode);
      deps.baseParams.HideTrunk = LeafInspectorParams.HideTrunkForMode(mode);
      if (trunkGameObj != null) trunkGameObj.SetActive(!deps.baseParams.HideTrunk);

      // #if UNITY_EDITOR
      //     if (editMode) {
      //       SceneView.lastActiveSceneView.cameraMode = SceneView.GetBuiltinCameraMode(LeafInspectorParams.DrawModeForMode(mode));
      //       if (LeafInspectorParams.Use2DForMode(mode) is bool use2D)
      //         SceneView.lastActiveSceneView.in2DMode = use2D;
      //     }
      // #endif

      int instanceIdx = deps.baseParams.InstanceIdxFromRandomBS();
      bool showMesh = LeafInspectorParams.ShowMeshForMode(mode);
      ViewAsPlantMode(LeafInspectorParams.AttachStemForMode(mode), showMesh, instanceIdx);

      if (lastDirtortion != deps.baseParams.HideDistortion) {
        Dictionary<LPType, bool> dict = BaseParams.AllClean;
        dict[LPType.Distort] = true;
        await RenderAll(fields, indexEntry, RenderQuality.Current, dict);
      }
    }

    public LeafDisplayMode GetDisplayMode() => displayMode;

    public void CachePlantToDisk() {
      SplitTimer st = new SplitTimer("CachePlant", true, false);
      st.Start();
      var cachedPlant = new CachedPlant(indexEntry.name, indexEntry.uniqueID, collection,
        leafMesh: lfd.leafMesh,
        trunkMesh: trunkGameObj.GetComponent<MeshFilter>().sharedMesh,
        stemMeshes: leafBundles.Select(lb => lb.GetStemMesh()).ToArray(),
        leafStems: leafBundles.Select(lb => lb.leafStem).ToArray(),
        lfd: lfd,
        arrangementData: arrangementData,
        collisionAdjustment: leafBundles.Select(lb => lb.collisionAdjustment).ToArray(),
        hiddenLeaves: leafBundles.Select(lb => !lb.gameObject.activeSelf).ToArray(),
        bones: leafBundles.Select(lb => lb.GetBones()).ToArray(),
        leafInstances: deps.baseParams.DistortionInstances,
        bufferVertDeltas: factory.ReadDeltaVertsBuffer(),
        bufferAdjNormals: factory.ReadAdjNormalsBuffer());
      st.Split("cache");
      PlantDataManager.CachePlantToDisk(cachedPlant, deps.leafData.randomSeed, fields);
      st.Stop();
    }

    public void SpawnCachedPlant(CachedPlant cachedPlant) {
      DebugBW.Log("    SpawnCachedPlant: " + cachedPlant.name, LColor.lightblue);
      SplitTimer st = new SplitTimer("CachePlant", false, false);
      st.Start();
      didLoadFromCache = true;
      isInstantiated = true;
      isHybridizing = false;
      viewAsPlant = true;
      indexEntry = PlantDataManager.TryGetIndexEntryUnqID(cachedPlant.uniqueID);
      collection = cachedPlant.collection;
      (LeafParamDict paramDict, SavedPlant savedPlant) = PlantDataManager.LoadPlant(indexEntry);
      this.fields = paramDict;
      deps.leafData.randomSeed = savedPlant.seed;

      lfd = cachedPlant.lfd;
      arrangementData = cachedPlant.arrangementData;
      Mesh leafMesh = cachedPlant.leafMesh.GetMesh();

      st.Split("Set Vars");

      trunk = new PlantTrunk();
      factory = new LeafFactory(deps);
      factory.InjectBufferData(cachedPlant.bufferVertDeltas, cachedPlant.bufferAdjNormals);
      ChangePot(potType, false);
      SpawnBundles(Arrangement.GetLeafCount(fields));

      st.Split("Spawn Bundles");

      trunkGameObj.GetComponent<MeshFilter>().sharedMesh = cachedPlant.trunkMesh.GetMesh();
      foreach ((LeafBundle leafBundle, int idx) in leafBundles.WithIndex()) {
        Mesh stemMesh = cachedPlant.stemMeshes[idx].GetMesh();
        leafBundle.Setup(deps, fields, lightLayers);
        leafBundle.leafStem = cachedPlant.leafStems[idx];
        leafBundle.SetCollisionAdjustment(cachedPlant.collisionAdjustment[idx]);
        leafBundle.PositionLeaf(lfd, arrangementData[idx], true);
        leafBundle.SetMeshes(leafMesh, stemMesh, false);

        BendPhysics bend = leafBundle.GetComponent<BendPhysics>();
        bend.SetCachedBones(cachedPlant.bones[idx], leafBundle.stemMeshRenderer,
          stemMesh);
        leafBundle.SetLeafJoint();
      }

      st.Split("Bundle Loop");

      ArrangeBundles(false);
      st.Split("Materials 1");

      FetchMaterials(MaterialType.LeafVelvet);
      st.Split("Materials 2");

      ApplyOrCreateAllTextures(fields, collection, true);
      st.Split("Materials 3");

      matController.SetStemAndTrunkMaterialParams(fields, stemMat, trunkMat,
        GetAvgStemLength(), arrangementData[arrangementData.Length / 2].scale);
      matController.SetMaterialParams(fields, leafMats,
        factory.deltaVertsBuffer, factory.adjustedNormalsBuffer, factory.ageSpotsBuffer,
        deps.baseParams.DistortionInstances, cachedPlant.leafMesh.verts.Length / 2, lfd.center);
      SetMaterials();

      potController.SetVisible(true);

      needsInspectorRedraw = true;

      st.Split("Materials 4");
      st.Stop();

      renderingResponder?.obj?.RenderComplete(new Weak<Plant>(this), indexEntry, RenderQuality.Maximum, true);
    }

    public async void LoadPreset(PlantData plantData, bool checkRenderTextures) {
      Debug.Log("Loading preset: " + plantData + " in collection " + plantData.collection);
      isHybridizing = false;
      indexEntry = plantData.indexEntry;
      collection = plantData.collection;
      (LeafParamDict paramDict, SavedPlant savedPlant) = PlantDataManager.LoadPlant(indexEntry);
      if (savedPlant == null) {
        Debug.LogError("Couldn't load preset " + plantData);
        return;
      }
      deps.leafData.randomSeed = savedPlant.seed;
      await RenderAll(paramDict, plantData.indexEntry, RenderQuality.Current);
      if (checkRenderTextures) ApplyOrCreateAllTextures(fields, collection, false);
      needsInspectorRedraw = true;
    }

    public async void RenderHybrid(PlantData hybrid, bool skipTextures = false) {
      if (hybrid.seed == -1) Debug.LogWarning("SEED -1!!!");
      if (hybrid.seed == -1) RandomizeSeed();
      else SetSeed(hybrid.seed);

      isHybridizing = true;
      collection = PlantCollection.Temporary;
      await RenderAll(hybrid.fields, hybrid.indexEntry, RenderQuality.Current);
      if (!skipTextures) ApplyOrCreateAllTextures(fields, collection, true);
      isHybridizing = false;
    }

    private float GetAvgStemLength() {
      if (leafBundles == null) return -1f;
      LeafBundle bund = leafBundles[leafBundles.Length / 2];
      return bund.leafStem.Length() * scaler.localScale.x;
    }

    public (Vector3 pos, Vector3 normal) GetSnippingLocation() {
      LeafBundle bundle = leafBundles.Last();
      var verts = bundle.stemMeshRenderer.sharedMesh.vertices;
      int idx = verts.Length / 3;
      Vector3 v = verts[idx];
      Vector3 normal = bundle.stemMeshRenderer.sharedMesh.normals[idx];
      Vector3 worldV = bundle.stemMeshRenderer.transform.TransformPoint(v);
      return (worldV, normal);
    }

    private void PrepareSwatches() {
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
    }

    public Dictionary<string, Color> GetSwatches() {
      return cachedSwatches;
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
