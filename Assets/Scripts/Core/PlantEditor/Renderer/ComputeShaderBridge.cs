using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BionicWombat {
  public abstract class ComputeShaderBridge {
    public static T[] ReadData<T>(ComputeBuffer buffer, int count) {
      T[] after = new T[count];
      buffer.GetData(after);
      return after;
    }

    protected static void ReleaseBuffer(ref ComputeBuffer buf) {
      if (buf != null) buf.Release();
      buf = null;
    }

    protected static void SwapBuffer(ref ComputeBuffer a, ref ComputeBuffer b) {
      var tmp = a;
      a = b;
      b = tmp;
    }

    public static ComputeBuffer MakeBuffer<T>(int count, T[] data) {
      ComputeBuffer b = new ComputeBuffer(count, Marshal.SizeOf<T>());
      if (data != null) b.SetData(data);
      return b;
    }
  }
}
