namespace BionicWombat {
  public class SpanningVeinParams {
    public LeafParamDict fields;
    public LeafVeinCalcs calcs;
    public LeafDeps deps;
    public LeafVein fromVein;
    public int buffer;
    public int index;
    public float spacing;
    public float totalCount;
    public bool mirror;
    public bool mirrorRoot;
    public bool reverseDirection;
    public float bunching;

    public SpanningVeinParams(LeafParamDict fields, LeafVeinCalcs calcs, LeafDeps deps, LeafVein fromVein) {
      this.fields = fields;
      this.calcs = calcs;
      this.deps = deps;
      this.fromVein = fromVein;
    }
  }
}
