using System.Reflection;

namespace BionicWombat {
  public static class SystemExtensions {
    public static string PublicParamsString(System.Type t, object o) {
      string s = "";
      FieldInfo[] fields = t.GetFields();
      foreach (FieldInfo field in fields) {
        s += field.Name + ":" + field.GetValue(o) + ",";
      }
      return s;
    }
  }
}
