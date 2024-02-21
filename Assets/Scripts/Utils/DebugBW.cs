using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UnityEngine {
  public static class DebugBWX {
    public static void Log(object s, string color = "") => Log("" + s, color);
    public static void Log(string s, string color = "") {
      string[] lines = s.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
      if (color != null && color.Length > 0) {
        if (lines.Length <= 1) {
          s = $"<color={color}>{s}</color>";
        } else {
          s = $"<color={color}>{lines[0]}</color>\n" +
           String.Join("\n", lines);
        }
      }
      Debug.Log(s);
    }

    public static void LogRegex(string s, string baseColor, string regexColor, string regex) {
      MatchCollection matches = Regex.Matches(s, regex);
      foreach (Match match in matches) {
        GroupCollection groups = match.Groups;
        foreach (Group g in groups) {
          if (g.Index >= 0 && g.Length > 0) {
            int endIndex = g.Index + g.Length;
            string replacement = $"<color={regexColor}>{s.Substring(g.Index, g.Length)}</color>";
            string result = s.Substring(0, g.Index) + replacement + s.Substring(endIndex + 1);
            Log(result, baseColor);
            break;
          }
        }
      }
      Log(s, baseColor); //if we didn't find a match
    }
  }

  public static class LColorX {
    public static string aqua = "aqua";
    public static string black = "black";
    public static string blue = "blue";
    public static string brown = "brown";
    public static string cyan = "cyan";
    public static string darkblue = "darkblue";
    public static string fuchsia = "fuchsia";
    public static string green = "green";
    public static string grey = "grey";
    public static string lightblue = "lightblue";
    public static string lime = "lime";
    public static string magenta = "magenta";
    public static string maroon = "maroon";
    public static string navy = "navy";
    public static string olive = "olive";
    public static string orange = "orange";
    public static string purple = "purple";
    // public static string red = "red";
    public static string silver = "silver";
    public static string teal = "teal";
    public static string white = "white";
    // public static string yellow = "yellow";

  }
}
