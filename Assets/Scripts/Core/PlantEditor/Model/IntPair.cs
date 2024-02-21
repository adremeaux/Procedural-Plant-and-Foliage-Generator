using System;

namespace BionicWombat {

  // MakeStruct.html?a=IntPair int x int y
  [Serializable]
  public struct IntPair {
    public int x;
    public int y;

    public IntPair(int x, int y) {
      this.x = x;
      this.y = y;
    }

    public override string ToString() {
      return "[IntPair] x: " + x + " | y: " + y;
    }
  }
}
