using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BionicWombat {
  [Serializable]
  public abstract class BWCustomEditor : Editor {
    const float gap = 3f;
    const float buttonWidth = 300f;
    const float textWidth = 70f;
    const float textFieldWidth = 40f;
    const float sliderExtentsWidth = 45f;
    const float sliderWidth = 100f;
    const float handleMult = 0.008f;
    const float vertSpace = 2f;
    const float precision = 1f;

    // float width = EditorGUIUtility.currentViewWidth - 35f;
    // float buttonHalfWidth = (width - gap) / 2f;
    // float buttonThirdWidth = (width - gap * 2f) / 3f;
    // float buttonFourthWidth = (width - gap * 3f) / 4f;
    // float buttonFifthWidth = (width - gap * 4f) / 5f;

    protected void AddButton(string name, System.Action func, float width = buttonWidth) {
      if (GUILayout.Button(name, GUILayout.Width(width))) {
        func();
      }
    }

    protected SliderParam AddSlider(SliderParam param) {
      FloatRange range = param.range;
      if (range == null) {
        Debug.LogWarning("Attempting to build slider with no specified range: " + param.name);
        return param;
      }

      float markerHeight = 4f;

      string name = param.name;
      float old = param.value;

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
      pv = (int)(pv * precision) / precision;

      EditorGUILayout.EndHorizontal();

      sliderStart.width = sliderEnd.width = 1;
      sliderStart.x += 3;
      sliderEnd.x -= 3;
      FloatRange r = param.range;
      float x = (r.Default - r.Start) / (r.End - r.Start);
      sliderStart.x = (sliderEnd.x - sliderStart.x) * x + sliderStart.x;
      sliderStart.y += 4;
      EditorGUI.DrawRect(sliderStart, new Color(0.6f, 0.6f, 0.6f));

      return new SliderParam(name, pv, param.range);
    }

    // MakeStruct.html?a=SliderParam string name float value FloatRange range
    public struct SliderParam {
      public string name;
      public float value;
      public FloatRange range;

      public SliderParam(string name, float value, FloatRange range) {
        this.name = name;
        this.value = value;
        this.range = range;
      }

      public override string ToString() {
        return "[SliderParam] name: " + name + " | value: " + value + " | range: " + range;
      }
    }
  }

}
