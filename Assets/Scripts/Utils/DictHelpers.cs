using System.Collections.Generic;

namespace BionicWombat {
  public static class DictHelpers {
    public static string SerializeDict<T>(Dictionary<string, T> d) {
      string s = "";
      foreach (string key in d.Keys) {
        s += key + "|" + d[key] + "|";
      }
      return s;
    }

    public static Dictionary<string, string> DeserializeDict(string s) {
      string key = null;
      Dictionary<string, string> d = new Dictionary<string, string>();
      foreach (string piece in s.Split("|")) {
        if (key == null) {
          key = piece;
          continue;
        }
        d[key] = piece;
        key = null;
      }
      return d;
    }

    public static Dictionary<T, Y> MergeWith<T, Y>(this Dictionary<T, Y> dictA,
              Dictionary<T, Y> dictB) {
      foreach (var item in dictB) {
        if (dictA.ContainsKey(item.Key))
          dictA[item.Key] = item.Value;
        else
          dictA.Add(item.Key, item.Value);
      }
      return dictA;
    }
  }

}
