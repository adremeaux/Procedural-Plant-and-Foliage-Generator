using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace BionicWombat {
  public static class StringExtentions {
    public static string AddColor(this string text, string hexColor) => $"<color=#{hexColor}>{text}</color>";
    public static string AddColor(this string text, Color col) => $"<color={ColorHexFromUnityColor(col)}>{text}</color>";
    public static string ColorHexFromUnityColor(this Color unityColor) => $"#{ColorUtility.ToHtmlStringRGBA(unityColor)}";

    public static string Lipsum(int numWords) {
      int idx = BWRandom.UnseededInt(0, lipsplit.Count - numWords);
      return string.Join(" ", lipsplit.GetRange(idx, numWords));
    }
    private static string lipstring = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla vehicula ligula erat, id efficitur mauris vestibulum a. Proin volutpat volutpat elit, sit amet viverra neque venenatis sed. Sed euismod risus eu facilisis euismod. In in mi sit amet lectus laoreet suscipit. Donec elementum eget turpis et bibendum. Nulla risus lacus, mollis nec orci eu, maximus tempus lorem. Nullam ac nunc massa. Fusce sed justo ut leo mollis hendrerit id nec est. Morbi dictum dignissim feugiat. Nam venenatis interdum tortor iaculis vulputate. Ut sed nunc lorem. Aenean bibendum dolor et viverra rutrum. Nam imperdiet odio ac semper finibus. Fusce magna libero, ullamcorper eu bibendum sit amet, dapibus nec purus. Interdum et malesuada fames ac ante ipsum primis in faucibus. Cras ut venenatis nisi, id rutrum erat.";
    private static List<string> lipsplit = lipstring.Split(" ").ToList();

    public static string Join(this IEnumerable<string> l, string seperator) {
      return string.Join(seperator, l);
    }
  }
}
