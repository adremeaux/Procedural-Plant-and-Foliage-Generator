using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static BionicWombat.ColorExtensions;
using static BionicWombat.LeafCurve;

namespace BionicWombat {
  [CustomEditor(typeof(Plant))]
  public class PlantInspector : Editor {
    private Plant plant;

    Dictionary<string, float> buttonFields = new Dictionary<string, float>();
    Dictionary<string, bool> foldouts = new Dictionary<string, bool>();

    string selectedPreset = "Select Preset";
    FlowerPotType selectedPot = FlowerPotType.Terracotta;
    string presetEntry = "";
    string currentLeaf = "";
    bool initialDirty = true;

    const float gap = 3f;
    const float buttonWidth = 300f;
    const float textWidth = 70f;
    const float textFieldWidth = 40f;
    const float sliderExtentsWidth = 45f;
    const float sliderWidth = 100f;
    const float handleMult = 0.008f;
    const float vertSpace = 2f;

    private void OnEnable() {
      if (target == null) return;
      initialDirty = true;
      plant = target as Plant;
      currentLeaf = plant.leafName;
      presetEntry = currentLeaf;

      SerializedProperty ser = serializedObject.FindProperty("fields");
      LeafParamDict d = ser != null ? (LeafParamDict)ser.boxedValue : null;

      foldouts = new Dictionary<string, bool>();
      string fString = EditorPrefs.GetString("foldouts");
      Dictionary<string, string> deserialized = DictHelpers.DeserializeDict(fString);
      foreach (string key in deserialized.Keys)
        foldouts[key] = deserialized[key] == "True";
    }

    private void OnSceneGUI() {
      if (!plant.GetIsInstantiated()) return;
      LeafFactoryData lfd = plant.GetLFD();
      if (lfd.leafShape == null) return;
      foreach (LeafCurve c in lfd.leafShape.curves) {
        LeafCurve curve = c.Transform(plant.deps.leafData.leafTransform);

        if (plant.deps.inspector.showHandles) {
          DrawHandles(curve);
          Handles.color = Color.yellow;
          Handles.DrawLine(curve.p0, curve.h0);
          Handles.DrawLine(curve.h1, curve.p1);
          Handles.color = new Color(1f, 0.92f, 0.016f, 0.2f); //alpha yellow
          Handles.DrawLine(curve.h0, curve.h1);
        }
        if (plant.deps.inspector.showSmooth)
          Handles.DrawBezier(curve.p0, curve.p1, curve.h0, curve.h1, Color.white, null, 2f);
      }
      if (plant.deps.inspector.showApprox) ShowApprox();
      if (plant.deps.inspector.showBoundingBox) ShowBoundingBox();
      if (plant.deps.inspector.showVeins) ShowVeins();
      if (plant.deps.inspector.showVeinPolys) ShowVeinPolys();
      if (plant.deps.inspector.showLinearPoints) ShowLinearPoints();
      if (plant.deps.inspector.showGravityCurve) ShowGravityCurve();
      if (plant.deps.inspector.showDistortionCurves) ShowDistortionCurves();
      if (plant.deps.inspector.showStem) ShowStem();
    }

    private void ShowApprox() {
      List<Curve> curves = LeafCurve.ToCurves(plant.GetLFD().leafShape.curves, plant.deps.leafData.leafTransform);
      // curves.Add(leaf.lfd.leafVeins.GetMidrib().Transform(leaf.deps.leafData.leafTransform));
      float steps = (float)plant.deps.baseParams.RenderLineSteps;
      foreach (Curve curve in curves) {
        Vector3 lineStart = curve.GetPoint(0f);
        for (int i = 1; i <= plant.deps.baseParams.RenderLineSteps; i++) {
          Vector3 lineEnd = curve.GetPoint((float)i / steps);
          Handles.color = Color.green;
          Handles.DrawLine(lineStart, lineEnd);
          lineStart = lineEnd;
        }
      }
    }

    private void ShowBoundingBox() {
      Vector2[] polyPoints = LeafRenderer.GetPolyPathPoints(
          LeafCurve.ToCurves(plant.GetLFD().leafShape.curves, plant.deps.leafData.leafTransform),
          plant.deps.baseParams.RenderLineSteps);
      (Vector2 min, Vector2 max) = LeafRenderer.GetBoundingBox(polyPoints);
      Vector2 offset = LeafRenderer.GetUVScaleAndOffset(polyPoints).offset;
      float spanX = max.x - min.x;
      float spanY = max.y - min.y;
      Vector2 scaleVec = spanX > spanY ? new Vector2(1, spanX / spanY) : new Vector2(spanY / spanX, 1);
      offset = spanY > spanX ? Vector2.zero : new Vector2(0, (offset.y - max.y) / 2f);
      min = min * scaleVec + offset;
      max = max * scaleVec + offset;
      Handles.color = Color.cyan;
      Handles.DrawLines(new Vector3[] {
      min, new Vector2(min.x, max.y),
      new Vector2(min.x, max.y), max,
      max, new Vector2(max.x, min.y),
      new Vector2(max.x, min.y), min
    });
    }

    private void ShowLinearPoints() {
      List<Vector3> linearPoints = plant.GetLFD().leafVeins.linearPoints;
      Transform t = plant.deps.leafData.leafTransform;
      Handles.color = Color.cyan;
      foreach (Vector3 p in linearPoints) {
        Handles.DrawWireDisc(p, Vector3.forward, .05f);
      }
      if (plant.GetTrunk().linearPoints != null)
        foreach (Vector3 p in plant.GetTrunk().linearPoints)
          Handles.DrawWireDisc(p, Vector3.forward, .05f);
    }

    private void ShowGravityCurve() {
      List<Vector3> gravityPoints = plant.GetLFD().leafVeins.gravityPoints;
      if (gravityPoints.Count == 0) return;
      Vector3 lastPoint = gravityPoints[0];//leaf.deps.leafData.leafTransform.TransformPoint(gravityPoints[0]);
      bool swap = false;
      foreach (Vector3 p in gravityPoints) {
        Vector3 point = p;//leaf.deps.leafData.leafTransform.TransformPoint(p);
        if (point == lastPoint) continue;
        Handles.color = swap ? Color.magenta : Color.yellow;
        Handles.DrawLine(lastPoint, point);
        lastPoint = point;
        swap = !swap;
      }
    }

    private void ShowVeins() {
      foreach (LeafVein vein in plant.GetLFD().leafVeins.GetVeins(plant.deps.leafData.leafTransform)) {
        Handles.DrawBezier(vein.p0, vein.p1, vein.h0, vein.h1, Color.white, null, 2f);
        DrawHandles(true, false, vein.FlattenedPoints());
      }
    }

    private void ShowVeinPolys() {
      Color c = ColorExtensions.ColorRGB(237, 203, 255);
      foreach (LeafVein vein in plant.GetLFD().leafVeins.GetVeins(plant.deps.leafData.leafTransform)) {
        foreach ((Vector2 v1, Vector2 v2) in ListExtensions.Pairwise(vein.AsPoly(), true)) {
          Handles.color = c;
          Handles.DrawLine(v1, v2);
        }
      }
    }

    private void ShowDistortionCurves() {
      List<DistortionCurve> curves = plant.GetLFD().distortionCurves;
      Transform t = plant.deps.leafData.leafTransform;
      foreach (DistortionCurve curve in curves) {
        // Curve3D distCurve = curve.distortionCurve;
        // Handles.DrawBezier(distCurve.p0, distCurve.p1, distCurve.h0, distCurve.h1, Color.cyan, null, 2f);
        // DrawHandles(distCurve, false);
        Handles.color = Color.cyan;
        foreach ((Vector3 v1, Vector3 v2) in curve.distortionPoints.Pairwise())
          Handles.DrawLine(v1, v2);
        // Handles.DrawLine(t.TransformPoint(v1), t.TransformPoint(v2));

        Handles.color = new Color(1f, 0.5f, 0f);
        foreach ((Vector3 v1, Vector3 v2) in curve.influencePoints.Pairwise())
          Handles.DrawLine(v1, v2);
        // Handles.DrawLine(t.TransformPoint(v1), t.TransformPoint(v2));

        // Curve3D inflCurve = curve.influenceCurve;
        // Handles.DrawBezier(inflCurve.p0,  inflCurve.p1, inflCurve.h0, inflCurve.h1, new Color(1f, 0.5f, 0f), null, 2f);
      }
    }

    private void ShowStem() {
      // foreach (Curve3D curve in plant.GetLFD().leafStem.curves) {
      //   Handles.DrawBezier(curve.p0, curve.p1, curve.h0, curve.h1, Color.white, null, 2f);
      //   DrawHandles(curve);
      // }

      // (Vector3[] stemPoints, Vector3[] normals) = plant.GetLFD().leafStem.GetStemPoints(10);
      // Handles.color = Color.red;
      // Handles.DrawLine(stemPoints.Last(), stemPoints.Last() + normals.Last().normalized, 2f);
    }

    private async void LoadPreset(string selectedPreset) {
      Debug.Log("Loading preset: " + selectedPreset);
      currentLeaf = selectedPreset;
      LeafParamDict preset = PresetManager.LoadPreset(selectedPreset, PlantCollection.Classic);
      await plant.RenderAll(preset, currentLeaf);
      EditorUtility.SetDirty(plant);
      serializedObject.ApplyModifiedProperties();
    }

#pragma warning disable CS4014
    private string lastDepsString = "";
    public override void OnInspectorGUI() {
      DrawDefaultInspector();

      if (!plant.GetIsInstantiated()) return;

      LeafParamDict fields = plant.GetFields();
      if (selectedPreset == "Select Preset") {
        selectedPreset = presetEntry = plant.leafName;
        selectedPot = plant.GetSelectedPot();
      }
      // if (plant.needsInspectorRedraw) {
      //   plant.needsInspectorRedraw = false;
      // }

      float width = EditorGUIUtility.currentViewWidth - 35f;
      float buttonHalfWidth = (width - gap) / 2f;
      float buttonThirdWidth = (width - gap * 2f) / 3f;
      float buttonFourthWidth = (width - gap * 3f) / 4f;
      float buttonFifthWidth = (width - gap * 4f) / 5f;

      if (Foldout("Core", true)) {
        EditorGUILayout.BeginHorizontal();
        AddButton("Clear Cache", () => {
          plant.ClearCache();
          serializedObject.ApplyModifiedProperties();

          foldouts = new Dictionary<string, bool>();
          EditorPrefs.SetString("foldouts", "");

          LoadPreset("Gloriosum");
        }, buttonHalfWidth);
        AddButton("Refresh", () => {
          UsefulShortcuts.RefreshAndClear();
        }, buttonHalfWidth);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        AddButton("Randomize Seed", () => {
          plant.SetSeed(Math.Abs((int)System.DateTime.Now.Ticks));
        }, buttonHalfWidth);
        AddButton("Create Texture", () => {
          plant.CreateAllTextures(fields);
        }, buttonHalfWidth);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(vertSpace);

        EditorGUILayout.BeginHorizontal();
        AddButton("Beziers", () => plant.SetViewMode(LeafDisplayMode.Bezier), buttonFourthWidth);
        AddButton("Veins", () => plant.SetViewMode(LeafDisplayMode.Veins), buttonFourthWidth);
        AddButton("Prerender", () => plant.SetViewMode(LeafDisplayMode.PreRender), buttonFourthWidth);
        AddButton("Mesh", () => plant.SetViewMode(LeafDisplayMode.Mesh), buttonFourthWidth);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        AddButton("Full Mesh", () => plant.SetViewMode(LeafDisplayMode.FullMesh), buttonFourthWidth);
        AddButton("Dist Mesh", () => plant.SetViewMode(LeafDisplayMode.DistortionMesh), buttonFourthWidth);
        AddButton("Stem Wire", () => plant.SetViewMode(LeafDisplayMode.StemWire), buttonFourthWidth);
        AddButton("Plant", () => plant.SetViewMode(LeafDisplayMode.Plant), buttonFourthWidth);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(vertSpace);

        EditorGUILayout.BeginHorizontal();
        AddButton("Min Poly", () => plant.SetPolyDepth(0), buttonFourthWidth);
        AddButton("Low Poly", () => plant.SetPolyDepth(1), buttonFourthWidth);
        AddButton("Reg Poly", () => plant.SetPolyDepth(2), buttonFourthWidth);
        AddButton("Max Poly", () => plant.SetPolyDepth(3), buttonFourthWidth);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(vertSpace);

        EditorGUILayout.BeginHorizontal();
        AddButton("Flat Mat", () => plant.FetchMaterials(MaterialType.LeafFlat), buttonFourthWidth);
        AddButton("Standard Mat", () => plant.FetchMaterials(MaterialType.LeafVelvet), buttonFourthWidth);
        AddButton("Fix Mats", () => plant.ResetMats(), buttonFourthWidth);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(vertSpace);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.EndHorizontal();

        if (Foldout("Texture Commands", false)) {
          EditorGUI.indentLevel = 1;
          void SetTexCmdStrings(TextureType type) {
            bool texCmdDirty = false;
            TextureCommandDict d = new TextureCommandDict();
            TextureCommandDict orig = plant.GetTextureCommandStrings(type);
            foreach (string name in orig.Keys) {
              bool oldEnabled = orig[name];
              bool newEnabled = AddToggle(name, oldEnabled);
              d.Add(name, newEnabled);
              texCmdDirty |= (oldEnabled != newEnabled);
            }
            if (texCmdDirty) plant.SetTextureCommandStrings(d, type);
          };

          if (Foldout("Render Layers", false)) {
            foreach (TextureType type in Enum.GetValues(typeof(TextureType))) {
              bool oldEnabled = plant.GetFactoryEnabled(type);
              bool newEnabled = AddToggle(type.ToString(), oldEnabled);
              if (oldEnabled != newEnabled) plant.SetFactoryEnabled(type, newEnabled);
            }
          }

          foreach (TextureType type in Enum.GetValues(typeof(TextureType)))
            if (Foldout(type.ToString(), false))
              SetTexCmdStrings(type);

        }
        EditorGUI.indentLevel = 0;
      }

      // if (Foldout("Edit", false)) {
      //   EditorGUILayout.BeginHorizontal();
      //   AddButton("Rebuild Joins", () => curveGroup.RebuildJoins(), buttonThirdWidth);
      //   AddButton("Mirror", () => curveGroup.Mirror(), buttonThirdWidth);
      //   AddButton("Join End", () => curveGroup.JoinEnd(), buttonThirdWidth);
      //   EditorGUILayout.EndHorizontal();
      //   EditorGUILayout.BeginHorizontal();
      //   AddButton("Add Curve", () => curveGroup.AddCurve(), buttonThirdWidth);
      //   AddButtonWithVals("Flatten", (int[] v) => curveGroup.FlattenAngle(v[0]), buttonThirdWidth, 1);
      //   // AddButtonWithVals("Subdivide", (int[] v) => curveGroup.Subdivide(v[0]), buttonThirdWidth, 0);
      //   EditorGUILayout.EndHorizontal();
      // }

      if (Foldout("Preset", true)) {
        EditorGUILayout.BeginHorizontal();
        AddButton("Load Preset", () => {
          LoadPreset(selectedPreset);
        }, 150);
        if (EditorGUILayout.DropdownButton(new GUIContent(selectedPreset), FocusType.Keyboard)) {
          GenericMenu menu = new GenericMenu();
          GenericMenu.MenuFunction2 setSelected = (object s) =>
            selectedPreset = presetEntry = (string)s;
          foreach (string s in PresetManager.GetCollection(PlantCollection.Classic).plantNames)
            menu.AddItem(new GUIContent(s), false, setSelected, s);
          menu.ShowAsContext();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        AddButton("Save Preset As", () => {
          if (presetEntry.Length == 0) return;
          LeafParamPreset p = new LeafParamPreset(presetEntry, fields);
          DataManager.SavePreset(p, PlantCollection.Classic);
          Debug.Log("Preset " + presetEntry + " saved successfully");
        }, 150);
        presetEntry = EditorGUILayout.TextField(presetEntry);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        AddButton("Next Preset", () => {
          List<string> list = PresetManager.GetCollection(PlantCollection.Classic).plantNames.ToList();
          int idx = list.IndexOf(selectedPreset) + 1;
          if (idx >= list.Count) idx = 0;
          selectedPreset = list[idx];
          LoadPreset(selectedPreset);
        }, 150);

        /*EditorGUILayout.Space(vertSpace);
        AddButton("Hybrid w/ Preset", () => {
          LeafParamDict otherDict = PresetManager.LoadPreset(PlantCollection.Classic, selectedPreset);
          selectedPreset = "Hybrid";
          presetEntry = "Hybrid";
          plant.Hybridize(fields, otherDict);
        }, 150);*/
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(vertSpace);

        EditorGUILayout.BeginHorizontal();
        AddButton("Change Pot", () => {
          plant.ChangePot(selectedPot, true);
        }, 150);
        if (EditorGUILayout.DropdownButton(new GUIContent(selectedPot.ToString()), FocusType.Keyboard)) {
          GenericMenu menu = new GenericMenu();
          GenericMenu.MenuFunction2 setSelected = (object s) => {
            try {
              selectedPot = (FlowerPotType)Enum.Parse(typeof(FlowerPotType), (string)s);
            } catch { Debug.Log("Invalid FlowerPotType found: " + s); }
          };
          foreach (FlowerPotType type in Enum.GetValues(typeof(FlowerPotType)))
            menu.AddItem(new GUIContent(type.ToString()), false, setSelected, type.ToString());
          menu.ShowAsContext();
        }
        EditorGUILayout.EndHorizontal();
      }

      if (Foldout("Generator", false)) {
        EditorGUILayout.BeginHorizontal();
        AddButton("Render", () => plant.RenderAll(fields, currentLeaf), buttonHalfWidth);
        AddButton("Randomize", () => {
          fields = Randomize(fields);
          SetSpecifics(fields);
          plant.RenderAll(fields, currentLeaf);
        }, buttonHalfWidth);
        EditorGUILayout.EndHorizontal();
      }

      string group = "";
      bool dirty = false;
      Dictionary<LPType, bool> dirtyDict = new Dictionary<LPType, bool>();

      foreach (LPType type in Enum.GetValues(typeof(LPType))) {
        group = "";
        if (!dirtyDict.ContainsKey(type)) dirtyDict.Add(type, false);
        if (Foldout(GetNameForLPKType(type), false)) {
          bool unfolded = true;
          EditorGUI.indentLevel = 1;
          foreach (LPK key in (LPK[])Enum.GetValues(typeof(LPK))) {
            if (key == LPK.None) continue;
            if (!fields.ContainsKey(key)) {
              if (selectedPreset != null && selectedPreset.Length > 0 && selectedPreset != "Select Preset")
                LoadPreset(selectedPreset);
              else
                plant.ClearCache();
              return;
            }
            LeafParam p = fields[key];
            if (p.type != type) continue;
            if (p.group != group) {
              group = p.group;
              unfolded = Foldout(group, true, "Nest");
            }
            if (unfolded) {
              Func<LeafParam, LeafParamDict, bool> AddAction = GetAddAction(p);
              bool result = AddAction(p, fields);
              dirtyDict[type] |= result;
              if (result && p.triggersFullRedraw)
                dirtyDict = BaseParams.AllDirty;
            }
          }
          EditorGUI.indentLevel = 0;
          dirty |= dirtyDict[type];
        }
      }

      if (lastDepsString != plant.deps.baseParams.ToString()) {
        dirtyDict = plant.deps.baseParams.FigureOutWhatsDirty(lastDepsString);
        lastDepsString = plant.deps.baseParams.ToString();
        dirty = true;
      }

      if (initialDirty) {
        initialDirty = false;
        dirty = true;
        dirtyDict = BaseParams.AllDirty;
      }

      if (dirty) {
        plant.RenderAll(fields, currentLeaf, dirtyDict);
        EditorUtility.SetDirty(plant);
        serializedObject.ApplyModifiedProperties();
      }

      Repaint();
    }

#pragma warning restore CS4014

    private bool Foldout(string name, bool defaultState, string disambiguate = "") {
      string key = name + disambiguate;
      if (!foldouts.ContainsKey(key)) foldouts[key] = defaultState;
      bool old = foldouts[key];
      foldouts[key] = EditorGUILayout.Foldout(foldouts[key], name, true, EditorStyles.foldoutHeader);
      if (foldouts[key] != old) EditorPrefs.SetString("foldouts", DictHelpers.SerializeDict(foldouts));
      return foldouts[key];
    }

    private Func<LeafParam, LeafParamDict, bool> GetAddAction(LeafParam p) {
      if (p.mode == LPMode.Toggle) return AddToggle;
      if (p.mode == LPMode.ColorHSL) return AddColorHSLSlider;

      //slider
      if (LeafParamBehaviors.ColorTransformFuncs.ContainsKey(p.key)) return AddColorModSlider;
      return AddSlider;
    }

    private void AddLabel(string name, float gapBefore = 10f, bool bold = false) {
      GUIStyle style = new GUIStyle();
      style.richText = true;
      name = "<color=white>" + name + "</color>";
      EditorGUILayout.Space(gapBefore);
      EditorGUILayout.LabelField(bold ? "<b>" + name + "</b>" : name, style);
    }

    private void AddButton(string name, System.Action func, float width = buttonWidth) {
      if (GUILayout.Button(name, GUILayout.Width(width))) {
        Undo.RecordObject(plant, name);
        func();
        EditorUtility.SetDirty(plant);
      }
    }

    private void AddButtonWithVals(string name, System.Action<int[]> func, float width, params int[] defaultVals) {
      int i;
      for (i = 0; i < defaultVals.Length; i++)
        if (!buttonFields.ContainsKey(name + i))
          buttonFields[name + i] = defaultVals[i];

      int[] GetArgs() {
        int[] a = new int[defaultVals.Length];
        for (int j = 0; j < a.Length; j++) a[j] = (int)buttonFields[name + j];
        return a;
      }

      EditorGUILayout.BeginHorizontal();
      float bWidth = width - (5f + textFieldWidth) * defaultVals.Length;
      if (GUILayout.Button(name, GUILayout.Width(bWidth))) {
        Undo.RecordObject(plant, name);
        func(GetArgs());
        EditorUtility.SetDirty(plant);
      }

      for (i = 0; i < defaultVals.Length; i++)
        buttonFields[name + i] =
          float.Parse(EditorGUILayout.TextField(buttonFields[name + i].ToString(), GUILayout.Width(textFieldWidth)));
      EditorGUILayout.EndHorizontal();
    }

    private bool AddSlider(LeafParam param, LeafParamDict fields) {
      FloatRange range = param.range;
      if (range == null) {
        Debug.LogWarning("Attempting to build slider with no specified range: " + param.name);
        return false;
      }

      // GUIStyle blackStyle = new GUIStyle();
      // Texture2D tex = new Texture2D(2, 2);
      // tex.SetColor(new Color(20, 20, 20, 255));
      // blackStyle.normal.background = tex;

      float markerHeight = 4f;

      LPK key = param.key;
      string name = key.ToString("F");
      float old = param.value;
      bool wasEnabled = param.enabled;

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField(name, GUILayout.MinWidth(textWidth), GUILayout.ExpandWidth(true));
      EditorGUILayout.Space(-20);
      EditorGUILayout.LabelField(range.Start.ToString(), GUILayout.Width(sliderExtentsWidth));
      Rect sliderStart = EditorGUILayout.GetControlRect(GUILayout.Height(markerHeight), GUILayout.Width(0));
      param.value = GUILayout.HorizontalSlider(
        param.value, range.Start, range.End,
        GUILayout.MinWidth(sliderWidth), GUILayout.ExpandWidth(true)
      );
      Rect sliderEnd = EditorGUILayout.GetControlRect(GUILayout.Height(markerHeight), GUILayout.Width(0));

      EditorGUILayout.LabelField(range.End.ToString(), GUILayout.Width(sliderExtentsWidth));
      EditorGUILayout.Space(-20);

      float pv = param.value;
      pv = (int)(pv * LeafParam.precision) / LeafParam.precision;
      // EditorGUILayout.LabelField("" + pv, GUILayout.Width(sliderExtentsWidth), GUILayout.ExpandWidth(false));

      param.enabled = EditorGUILayout.Toggle(param.enabled, GUILayout.Width(30f));
      EditorGUILayout.EndHorizontal();

      sliderStart.width = sliderEnd.width = 1;
      sliderStart.x += 3;
      sliderEnd.x -= 3;
      FloatRange r = param.range;
      float x = (r.Default - r.Start) / (r.End - r.Start);
      sliderStart.x = (sliderEnd.x - sliderStart.x) * x + sliderStart.x;
      sliderStart.y += 4;
      EditorGUI.DrawRect(sliderStart, new Color(0.6f, 0.6f, 0.6f));

      return old != param.value || wasEnabled != param.enabled;
    }

    private Dictionary<string, bool> colorDragDict = new Dictionary<string, bool>();

    private bool AddColorModSlider(LeafParam param, LeafParamDict fields) {
      LPK key = param.key;
      string keyString = key.ToString();
      if (!fields.ContainsKey(key) || param == null)
        Debug.LogError(param == null ? "Param missing" : "Param missing in dict: " + param.key);
      if (!colorDragDict.ContainsKey(keyString)) colorDragDict[keyString] = false;
      string name = key.ToString("F");
      float old = param.value;

      bool dragging = colorDragDict[keyString];
      param.value = GradientSlider(name,
        LeafParamBehaviors.GradientWithParamTransform(param, fields), param.value, param.range.Start, param.range.End, ref dragging);
      colorDragDict[keyString] = dragging;

      return old != param.value;
    }

    private bool AddColorHSLSlider(LeafParam param, LeafParamDict fields) {
      LPK key = param.key;
      if (!fields.ContainsKey(key) || param == null)
        Debug.LogError(param == null ? "Param missing" : "Param missing in dict: " + param.key);

      string baseKeyString = key.ToString();
      string hueString = baseKeyString + "Hue";
      string satString = baseKeyString + "Sat";
      string valString = baseKeyString + "Lit";

      if (!colorDragDict.ContainsKey(hueString)) {
        colorDragDict[hueString] = false;
        colorDragDict[satString] = false;
        colorDragDict[valString] = false;
      }

      HSL baseColor = param.hslValue;
      HSL newColor = new HSL(baseColor);
      HSLRange range = param.hslRange;
      float oldVal;
      bool dirty = false;

      bool dragging = colorDragDict[hueString];
      oldVal = newColor.hue;
      newColor.hue = GradientSlider(param.name + " (Hue)",
        GradientWith(baseColor.WithHue(range.hueRange.Start),
                     baseColor.WithHue(range.hueRange.End)),
                                    baseColor.hue, range.hueRange.Start, range.hueRange.End, ref dragging);
      colorDragDict[hueString] = dragging;
      dirty |= oldVal != newColor.hue;

      dragging = colorDragDict[satString];
      oldVal = newColor.saturation;
      newColor.saturation = GradientSlider(param.name + " (Sat)",
        GradientWith(baseColor.WithSat(range.satRange.Start),
                     baseColor.WithSat(range.satRange.End)),
                                    baseColor.saturation, range.satRange.Start, range.satRange.End, ref dragging);
      colorDragDict[satString] = dragging;
      dirty |= oldVal != newColor.saturation;

      dragging = colorDragDict[valString];
      oldVal = newColor.lightness;
      newColor.lightness = GradientSlider(param.name + " (Lit)",
        GradientWith(baseColor.WithLit(range.valRange.Start),
                     baseColor.WithLit(range.valRange.End)),
                                          baseColor.lightness, range.valRange.Start, range.valRange.End, ref dragging);
      colorDragDict[valString] = dragging;
      dirty |= oldVal != newColor.lightness;

      param.hslValue = newColor;
      return dirty;
    }

    private float GradientSlider(string label, Gradient gradient,
        float value, float min, float max, ref bool dragging) {
      EditorGUILayout.BeginHorizontal();

      EditorGUILayout.LabelField(label, GUILayout.MinWidth(100f));

      DisableGUI();
      EditorGUILayout.GradientField(gradient);
      EnableGUI();

      Rect cursor = GetCursor(ref dragging, ref value, min, max);
      EditorGUILayout.EndHorizontal();

      DisableGUI();
      EditorGUI.DrawRect(cursor, Color.white);
      EnableGUI();

      return value;
    }

    private bool AddToggle(LeafParam param, LeafParamDict fields) {
      LPK key = param.key;
      if (!fields.ContainsKey(key) || param == null)
        Debug.LogError(param == null ? "Param missing" : "Param missing in dict: " + param.key);
      string name = key.ToString("F");
      bool wasEnabled = param.enabled;

      EditorGUILayout.BeginHorizontal();
      GUILayout.Label(name, GUILayout.Width(textWidth * 2f));
      GUILayout.Space(5);
      param.enabled = GUILayout.Toggle(param.enabled, "");
      GUILayout.FlexibleSpace();
      GUILayout.EndHorizontal();

      // param.enabled = EditorGUILayout.Toggle(name, param.enabled, GUILayout.Width(textWidth));

      return wasEnabled != param.enabled;
    }

    private bool AddToggle(string name, bool enabled) {
      EditorGUILayout.BeginHorizontal();
      GUILayout.Label(name, GUILayout.Width(textWidth * 4f));
      GUILayout.Space(5);
      enabled = GUILayout.Toggle(enabled, "");
      GUILayout.FlexibleSpace();
      GUILayout.EndHorizontal();
      return enabled;
    }

    private void DrawHandles(LeafCurve leafCurve) => DrawHandles(false, true, leafCurve.p0, leafCurve.h0, leafCurve.h1, leafCurve.p1);
    private void DrawHandles(Curve3D leafCurve) => DrawHandles(false, true, leafCurve.p0, leafCurve.h0, leafCurve.h1, leafCurve.p1);

    private void DrawHandles(bool isVein, bool force = false, params Vector3[] points) {
      for (int i = 0; i < points.Length; i++)
        DrawHandle(points[i], i % 4 == 0 || i % 4 == 3, isVein, force);
    }

    private void DrawHandle(Vector3 point, bool isPrimary, bool isVein = false, bool force = false) {
      if (!force && ((!isVein && !plant.deps.inspector.showHandles) ||
           (isVein && !plant.deps.inspector.showVeinHandles))) return;
      CustomHandles.DragHandleResult dhResult;

      Vector3 cameraPos = Camera.current.WorldToScreenPoint(Vector3.zero);
      float size = handleMult * cameraPos.z * (isPrimary ? 1f : 0.5f);

      Vector3 newPosition = CustomHandles.DragHandle(point,
        size,
        isPrimary,
        isVein ? (isPrimary ? Color.red : Color.green) :
                 (isPrimary ? Color.magenta : Color.cyan),
        Color.yellow,
        out dhResult);

      switch (dhResult) {
        case CustomHandles.DragHandleResult.LMBDrag:
          Debug.LogWarning("Point dragging needs to be reimplemented");
          // Undo.RecordObject(curveGroup, "Move Point");
          // point = newPosition;
          // EditorUtility.SetDirty(curveGroup);
          // curveGroup.UpdatePoint(index, deps.leafData.leafTransform.InverseTransformPoint(point), true);
          break;
      }
    }

    private static LeafParamDict Randomize(LeafParamDict dict) {
      LeafParamDict d = new LeafParamDict();
      foreach (LPK key in dict.Keys) {
        LeafParam p = dict[key].Copy();
        if (p.range == FloatRange.Zero)
          p.enabled = BWRandom.Range(0, 2) % 2 == 1;
        else
          p.value = BWRandom.Range(p.range.Start, p.range.End);
        d[key] = p;
      }
      return d;
    }

    private static void SetSpecifics(LeafParamDict dict) {
      dict[LPK.Pudge].enabled = !dict[LPK.Heart].enabled;
    }

    private string GetNameForLPKType(LPType type) {
      switch (type) {
        case LPType.Leaf: return "Leaf Shape";
        case LPType.Vein: return "Veins";
        case LPType.Texture: return "Texture";
        case LPType.Normal: return "Normals";
      }
      return type.ToString();
    }

    private Color previousColor;
    private Color previousBackgroundColor;
    public void EnableGUI() {
      GUI.backgroundColor = previousBackgroundColor;
      GUI.color = previousColor;
      GUI.enabled = true;
    }

    public void DisableGUI() {
      previousColor = GUI.color;
      previousBackgroundColor = GUI.backgroundColor;
      GUI.backgroundColor = new Color(1f, 1f, 1f, 2f);
      GUI.color = new Color(1f, 1f, 1f, 2f);
      GUI.enabled = false;
    }

    public static Rect GetCursor(ref bool dragging, ref float value, float min, float max) {
      Rect rect = GUILayoutUtility.GetLastRect();
      Vector2 mousePosition = Event.current.mousePosition;

      if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(mousePosition)) {
        dragging = true;
      } else if (Event.current.type == EventType.MouseUp && Event.current.button == 0) {
        dragging = false;
      }

      if (dragging && Event.current.type == EventType.Repaint) {
        value = Mathf.Lerp(min, max, (mousePosition.x - rect.x) / rect.width);
        if (!rect.Contains(mousePosition)) dragging = false;
      }

      rect.x += Mathf.Lerp(0f, rect.width - 5f, Mathf.InverseLerp(min, max, value)) + 1f;
      rect.width = 3f;

      return rect;
    }
  }

}