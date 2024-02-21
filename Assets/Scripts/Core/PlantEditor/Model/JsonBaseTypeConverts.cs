#region License
// The MIT License (MIT)
//
// Copyright (c) 2020 Wanzyee Studio
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Newtonsoft.Json.UnityConverters {
  public class Vector3Converter : PartialConverter<Vector3> {
    protected override void ReadValue(ref Vector3 value, string name, JsonReader reader, JsonSerializer serializer) {
      switch (name) {
        case nameof(value.x):
          value.x = (float)reader.ReadAsDouble();
          break;
        case nameof(value.y):
          value.y = (float)reader.ReadAsDouble();
          break;
        case nameof(value.z):
          value.z = (float)reader.ReadAsDouble();
          break;
      }
    }

    protected override void WriteJsonProperties(JsonWriter writer, Vector3 value, JsonSerializer serializer) {
      writer.WritePropertyName(nameof(value.x));
      writer.WriteValue(value.x);
      writer.WritePropertyName(nameof(value.y));
      writer.WriteValue(value.y);
      writer.WritePropertyName(nameof(value.z));
      writer.WriteValue(value.z);
    }
  }

  public class Vector2Converter : PartialConverter<Vector2> {
    protected override void ReadValue(ref Vector2 value, string name, JsonReader reader, JsonSerializer serializer) {
      switch (name) {
        case nameof(value.x):
          value.x = (float)reader.ReadAsDouble();
          break;
        case nameof(value.y):
          value.y = (float)reader.ReadAsDouble();
          break;
      }
    }

    protected override void WriteJsonProperties(JsonWriter writer, Vector2 value, JsonSerializer serializer) {
      writer.WritePropertyName(nameof(value.x));
      writer.WriteValue(value.x);
      writer.WritePropertyName(nameof(value.y));
      writer.WriteValue(value.y);
    }
  }

  public class QuaternionConverter : PartialConverter<Quaternion> {
    protected override void ReadValue(ref Quaternion value, string name, JsonReader reader, JsonSerializer serializer) {
      switch (name) {
        case nameof(value.x):
          value.x = (float)reader.ReadAsDouble();
          break;
        case nameof(value.y):
          value.y = (float)reader.ReadAsDouble();
          break;
        case nameof(value.z):
          value.z = (float)reader.ReadAsDouble();
          break;
        case nameof(value.w):
          value.w = (float)reader.ReadAsDouble();
          break;
      }
    }

    protected override void WriteJsonProperties(JsonWriter writer, Quaternion value, JsonSerializer serializer) {
      writer.WritePropertyName(nameof(value.x));
      writer.WriteValue(value.x);
      writer.WritePropertyName(nameof(value.y));
      writer.WriteValue(value.y);
      writer.WritePropertyName(nameof(value.z));
      writer.WriteValue(value.z);
      writer.WritePropertyName(nameof(value.w));
      writer.WriteValue(value.w);
    }
  }

  public class Matrix4x4Converter : PartialConverter<Matrix4x4> {
    // https://github.com/Unity-Technologies/UnityCsReference/blob/2019.2/Runtime/Export/Math/Matrix4x4.cs#L21-L29
    private static readonly string[] _names = GetMemberNames();
    private static readonly Dictionary<string, int> _namesToIndex = GetNamesToIndex(_names);

    /// <summary>
    /// Get the property names include from <c>m00</c> to <c>m33</c>.
    /// </summary>
    /// <returns>The property names.</returns>
    private static string[] GetMemberNames() {
      string[] indexes = new[] { "0", "1", "2", "3" };
      return indexes.SelectMany((row) => indexes.Select((column) => "m" + column + row)).ToArray();
    }

    // Reusing the same strings here instead of creating new ones. Tiny bit lower memory footprint
    private static Dictionary<string, int> GetNamesToIndex(string[] names) {
      var dict = new Dictionary<string, int>();
      for (int i = 0; i < names.Length; i++) {
        dict[names[i]] = i;
      }
      return dict;
    }

    protected override void ReadValue(ref Matrix4x4 value, string name, JsonReader reader, JsonSerializer serializer) {
      if (_namesToIndex.TryGetValue(name, out var index)) {
        value[index] = (float)reader.ReadAsDouble();
      }
    }

    protected override void WriteJsonProperties(JsonWriter writer, Matrix4x4 value, JsonSerializer serializer) {
      for (int i = 0; i < _names.Length; i++) {
        writer.WritePropertyName(_names[i]);
        writer.WriteValue(value[i]);
      }
    }
  }
}
