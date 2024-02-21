using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace BionicWombat {
  public interface IPlantSpawnerResponder {
    void WillStartRendering();
    void DidFinishRendering(Weak<Plant> weakPlant, PlantIndexEntry indexEntry, Plant.RenderQuality renderQuality, bool texturesAvailable);
  }

#if UNITY_EDITOR
  [ExecuteInEditMode]
#endif
  [Serializable]
  [RequireComponent(typeof(CapsuleCollider))]
  public class PlantSpawner : MonoBehaviour, IPlantRenderingResponder {
    public enum SpawnerState {
      Choosing,
      Displaying,
    }
    public enum SpawnerLock {
      Open,
      Locked,
    }

    public SegmentedRing ringObj;
    public GameObject arrowObj;
    public Plant plantPrefab;
    public LightLayers lightLayers = LightLayers.Default | LightLayers.Plant;
    public bool showSubmenu = false;
    public SpawnerState state = SpawnerState.Choosing;
    public SpawnerLock locked = SpawnerLock.Open;
    private Weak<IPlantSpawnerResponder> spawnerResponder;
    private bool menuIsOpen = false;
    private Dictionary<string, PlantData> collections = new Dictionary<string, PlantData>();
    private GameObject selectedEffect;
    private Plant.RenderQuality _renderQuality;
    private new CapsuleCollider collider;

    public Plant plant { get; private set; }
    private bool animating = false;

    private float baseArrowYPos = 0.13f;
    private float arrowAnimY = 0.07f;
    private float distanceFromWindSource = -1f;
    private Vector3 vectorFromWindSource = Vector3.left;

    public void Awake() {
      collider = GetComponent<CapsuleCollider>();
    }

    public void Start() {
      arrowObj.transform.localPosition = arrowObj.transform.localPosition.WithY(baseArrowYPos);
    }

    public void SetSpawnerResponder(IPlantSpawnerResponder r) { spawnerResponder = new Weak<IPlantSpawnerResponder>(r); }

    public void SetWindData(float distanceFromWindSource, Vector3 vectorFromWindSource) {
      this.distanceFromWindSource = distanceFromWindSource;
      this.vectorFromWindSource = vectorFromWindSource;
    }

    public void SpawnPlant(PlantData plantData) {
      FlowerPotType potType = plant != null ? plant.GetSelectedPot() : FlowerPotType.Terracotta;
      SpawnPlant(plantData, potType, true);
    }

    public void SpawnPlant(PlantData plantData,
        FlowerPotType potType = FlowerPotType.Terracotta, bool ignoreDiskCache = true) {
      if (!Asserts.Assert(plantData != null, "PlantData is null")) return;

      Debug.Log("SpawnPlant: " + plantData.indexEntry.name);
      Clear();

      if (!PlantDataManager.PlantExistsForData(plantData)) {
        SpawnHybrid(plantData, potType);
        return;
      }

      if (plantData.indexEntry.propegating) potType = FlowerPotType.NurseryBlack;

      if (!GlobalVars.instance.UsePlantCache) ignoreDiskCache = true;
      if (!ignoreDiskCache && PlantDataManager.CacheExistsForData(plantData.indexEntry)) {
        Debug.Log("Cache exists for " + plantData.indexEntry.name);
        CachedPlant p = PlantDataManager.LoadCachedPlant(plantData.indexEntry);
        if (p.IsNotDefault()) {
          SpawnDiskCachedPlant(p, plantData, potType);
          return;
        }
      } else {
        SpawnPreset(plantData, potType);
      }
    }

    private void SpawnHybrid(PlantData hybrid, FlowerPotType potType = FlowerPotType.Terracotta) {
      if (spawnerResponder != null) spawnerResponder.obj.WillStartRendering();
      DebugBW.Log("  SpawnHybrid " + hybrid.indexEntry.name, LColor.aqua);

      bool didSpawn = SpawnPlantPre(false, potType);
      if (didSpawn) plant.SetInitArgs(hybrid, potType, lightLayers, _renderQuality, true);
      else plant.RenderHybrid(hybrid);
      SetState(SpawnerState.Displaying);
    }

    private void SpawnPreset(PlantData preset, FlowerPotType potType = FlowerPotType.Terracotta) {
      if (spawnerResponder != null) spawnerResponder.obj.WillStartRendering();
      DebugBW.Log("  SpawnPreset " + preset.indexEntry.name + " | preset.collection: " + preset.collection, LColor.aqua);

      bool didSpawn = SpawnPlantPre(preset.indexEntry.propegating, potType);
      // Debug.Log("preset: " + preset + " | didSpawn: " + didSpawn);
      if (didSpawn) plant.SetInitArgs(preset, potType, lightLayers, _renderQuality, false);
      else plant.LoadPreset(preset, true);
      SetState(SpawnerState.Displaying);
    }

    private void SpawnDiskCachedPlant(CachedPlant p, PlantData plantData,
        FlowerPotType potType = FlowerPotType.Terracotta) {
      if (spawnerResponder != null) spawnerResponder.obj.WillStartRendering();

      bool didSpawn = SpawnPlantPre(plantData.indexEntry.propegating, potType);
      if (didSpawn) plant.SetInitArgs(plantData, potType, lightLayers, _renderQuality, true);
      plant.transform.Reset();
      plant.SpawnCachedPlant(p);
      SetState(SpawnerState.Displaying);
    }

    private bool SpawnPlantPre(bool propViewMode, FlowerPotType potType = FlowerPotType.Terracotta) {
      // transform.localRotation = Quaternion.identity;
      if (plant == null) {
        plant = Instantiate(plantPrefab, Vector3.zero, Quaternion.identity, transform);
        plant.SetRenderingResponder(this);
        plant.SetRenderQuality(_renderQuality);
        plant.SetViewMode(propViewMode ? LeafDisplayMode.Propegating : LeafDisplayMode.Plant);
        plant.SetLightLayers(lightLayers);
        return true;
      } else {
        plant.Clear();
        plant.ChangePot(potType, false);
        plant.SetRenderQuality(_renderQuality);
        plant.SetViewMode(propViewMode ? LeafDisplayMode.Propegating : LeafDisplayMode.Plant);
        plant.SetLightLayers(lightLayers);
        return false;
      }
    }

    public void RenderComplete(Weak<Plant> weakPlant, PlantIndexEntry indexEntry, Plant.RenderQuality renderQuality, bool texturesAvailable) {
      if (plant == null || !weakPlant.Check()) {
        Debug.LogWarning("Plant has been destroyed before RenderComplete called: " + plant + " | " + weakPlant.obj);
        return;
      }
      if (weakPlant.obj != plant) {
        Debug.Log("RenderComplete plant and self plant are different: " + plant + " | " + weakPlant.obj);
        plant = weakPlant.obj;
      }
      Bounds bounds = plant.gameObject.GetBoundsRecursive();
      collider.center = new Vector3(0, bounds.extents.y, 0);
      collider.height = bounds.extents.y * 2f;
      collider.radius = bounds.extents.x;
      collider.enabled = true;

      spawnerResponder?.obj?.DidFinishRendering(weakPlant, indexEntry, renderQuality, texturesAvailable);
    }

    public void RenderHighResTexturesIfNeeded() {
      if (plant == null) return;
      int width = PlantDataManager.TextureSizeForLeaf(plant.indexEntry, plant.collection);
      if (width < 1024) {
        renderQuality = Plant.RenderQuality.Maximum;
        plant.CreateAllTextures(plant.GetFields());
      }
    }

    public void Clear() {
      plant?.Clear();
      plant = null;
      collider.enabled = false;
    }

    private void MenuClosedWithoutSelection() {
      if (menuIsOpen) {
        GetComponent<Collider>().enabled = true;
        SetState(SpawnerState.Choosing);
        menuIsOpen = false;
      }
    }

    public void SetState(SpawnerState state) {
      if (collider == null) collider = GetComponent<CapsuleCollider>();
      this.state = state;
      if (state == SpawnerState.Choosing) {
        ringObj.SetActive(true);
        arrowObj.SetActive(true);
        animating = true;
        collider.radius = 0.21f;

        if (plant != null) {
          Destroy(plant.gameObject);
          plant = null;
        }
      } else if (state == SpawnerState.Displaying) {
        ringObj.SetActive(false);
        arrowObj.SetActive(false);
        animating = false;
        //plant?.Clear();
      }
      collider.enabled = true;
    }

    public void Update() {
      if (Application.isPlaying) {
        if (animating) {
          ringObj.transform.Rotate(0, 0.25f, 0);
        } else {
        }
      }
    }

    public bool selected {
      get => selectedEffect != null;
      set {
        if (value) {
          Debug.Log("selecting " + GetPlantName() + " | hasEffect: " + (selectedEffect != null));
          if (selectedEffect == null)
            selectedEffect = EffectsFactory.CreateEffect(EffectName.LeafyCircleHighlight, Vector3.zero, transform);
        } else if (selectedEffect) {
          Destroy(selectedEffect);
          selectedEffect = null;
        }
      }
    }

    public Plant.RenderQuality renderQuality {
      set {
        _renderQuality = value;
        if (plant != null) {
          plant.SetRenderQuality(value);
        }
      }
    }

    public PlantData GetSpawnedData() {
      if (plant == null) return null;
      return new PlantData(plant.GetFields(), plant.indexEntry, plant.collection, plant.deps.leafData.randomSeed);
    }

    public string GetPlantName() {
      if (plant == null) return null;
      return plant.indexEntry.name;
    }

    public FlowerPotType PotType {
      get {
        if (plant == null) return FlowerPotType.NurseryBlack;
        return plant.GetSelectedPot();
      }
      set {
        plant?.ChangePot(value, true);
      }
    }

    private void OnDestroy() {
      if (selectedEffect != null) Destroy(selectedEffect);
    }
  }
}
