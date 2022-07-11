using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace BionicWombat {
#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
[Serializable]
public class PlantSpawner : MonoBehaviour {
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
  public bool showSubmenu;
  public SpawnerState state = SpawnerState.Choosing;
  public SpawnerLock locked = SpawnerLock.Open;
  private bool menuIsOpen = false;

  private Plant plant;

  private float baseArrowYPos = 0.13f;

  public void Awake() {
    if (Application.isPlaying) {
      foreach (Transform t in transform)
        if (t.tag == Tags.Plant)
          Destroy(t.gameObject);
    }

    if (locked == SpawnerLock.Locked) {
      SetState(SpawnerState.Displaying);
    }
  }

  public void Start() {
    arrowObj.transform.localPosition = arrowObj.transform.localPosition.WithY(baseArrowYPos);
  }

  private void OnMouseDown() {
    if (locked == SpawnerLock.Locked) return;

    GetComponent<Collider>().enabled = false;

    UnityEvent<string[]> ev = new UnityEvent<string[]>();
    ev.AddListener(DidTapMenuItem);
    Texture2D iconTex = null;//(Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Modern UI Pack/Textures/Icon/Reward/Trophy Filled.png", typeof(Texture2D));
    Sprite icon = iconTex == null ? null :
      Sprite.Create(iconTex, new Rect(0.0f, 0.0f, iconTex.width, iconTex.height), new Vector2(0.5f, 0.5f), 100.0f);

    // List<SubMenuItem> pots = new List<SubMenuItem>();
    // if (showSubmenu)
    //   foreach (FlowerPotType type in Enum.GetValues(typeof(FlowerPotType)))
    //     pots.Add(new SubMenuItem(type.ToString(), ContextItemType.Button, ev, icon));

    // List<ContextItem> items = new List<ContextItem>();
    // foreach (string name in PresetManager.GetCollection(PlantCollection.Classic).plantNames)
    //   items.Add(new ContextItem(name, ContextItemType.Button, ev, icon, pots));

    // menuContent.contextItems = items;
    // menuContent.ProcessClick();
    // menuIsOpen = true;
  }

  public void DidTapMenuItem(params string[] args) {
    FlowerPotType potType = FlowerPotType.Terracotta;
    if (args.Length > 1) {
      potType = (FlowerPotType)Enum.Parse(typeof(FlowerPotType), (string)args[1]);
    }

    SpawnPreset(args[0], potType);

    menuIsOpen = false;
    SetState(SpawnerState.Displaying);
  }

  public void SpawnHybrid(LeafParamDict fields, string plantName, FlowerPotType potType = FlowerPotType.Terracotta) {
    bool didSpawn = SpawnPlantPre(potType);
    if (didSpawn) plant.SetInitArgs(fields, plantName, potType, lightLayers);
    else plant.RenderHybrid(fields, plantName);
  }

  public void SpawnPreset(string presetName, FlowerPotType potType = FlowerPotType.Terracotta) {
    bool didSpawn = SpawnPlantPre(potType);
    if (didSpawn) plant.SetInitArgs(presetName, potType, lightLayers);
    else plant.LoadPreset(presetName);
  }

  private bool SpawnPlantPre(FlowerPotType potType = FlowerPotType.Terracotta) {
    if (plant == null) {
      plant = Instantiate(plantPrefab, Vector3.zero, Quaternion.identity, transform);
      return true;
    } else {
      plant.lightLayers = lightLayers;
      plant.ChangePot(potType, false);
      return false;
    }
  }

  private void MenuClosedWithoutSelection() {
    if (menuIsOpen) {
      GetComponent<Collider>().enabled = true;
      SetState(SpawnerState.Choosing);
      menuIsOpen = false;
    }
  }

  public void SavePlantAs(string name, PlantCollection collection) {
    if (plant == null) return;
    if (name == null || name.Length == 0) name = plant.leafName;
    LeafParamPreset p = new LeafParamPreset(name, plant.GetFields());
    DataManager.SavePreset(p, collection);
    DataManager.MigrateTexturesToPermanent(name, collection);
  }

  private void SetState(SpawnerState state) {
    this.state = state;
    if (state == SpawnerState.Choosing) {
      ringObj.SetActive(true);
      arrowObj.SetActive(true);

      if (plant != null) {
        Destroy(plant);
        plant = null;
      }
    } else if (state == SpawnerState.Displaying) {
      ringObj.SetActive(false);
      arrowObj.SetActive(false);
    }

    GetComponent<Collider>().enabled = true;
  }

  public LeafParamDict GetSpawnedParams() {
    if (plant == null) return null;
    return plant.GetFields();
  }

  public string GetPlantName() {
    if (plant == null) return null;
    return plant.leafName;
  }
}
}