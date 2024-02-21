using System;

namespace BionicWombat {
  public class Weak<T> where T : class {
    private WeakReference<T> _weakRef;
    public T obj {
      get {
        T t;
        _weakRef.TryGetTarget(out t);
        return t;
      }
    }

    public Weak(T obj) {
      _weakRef = new WeakReference<T>(obj);
    }

    public override string ToString() {
      if (obj == null) return "Weak<" + typeof(T) + ">(null)";
      return "Weak<" + typeof(T) + ">(" + obj + ")";
    }
  }

  public static class WeakExtensions {
    public static bool Check<T>(this Weak<T> w) where T : class => w != null && w.obj != null;
    public static T GetObjNullable<T>(this Weak<T> w) where T : class => w == null ? null : w.obj;
  }
}
