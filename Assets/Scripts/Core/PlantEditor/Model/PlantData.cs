namespace BionicWombat {
  public class PlantData {
    public LeafParamDict fields;
    public PlantCollection collection;
    public PlantIndexEntry indexEntry;
    public int seed;

    public PlantData(LeafParamDict fields, PlantIndexEntry indexEntry, PlantCollection collection, int seed) {
      this.fields = fields;
      this.collection = collection;
      this.indexEntry = indexEntry;
      this.seed = seed;
    }

    public PlantData(PlantIndexEntry indexEntry, PlantCollection collection) : this(null, indexEntry, collection, 0) { }

    public override string ToString() {
      return indexEntry.name + " (" + collection + ")";
    }

    public static PlantData FromSavedPlant(SavedPlant sp, LeafParamDict fields, PlantCollection col) {
      if (!Asserts.Assert(sp != null, "FromSavedPlant plant is null")) return null;
      PlantIndexEntry entry = PlantDataManager.TryGetIndexEntryUnqID(sp.uniqueID);
      if (!Asserts.Assert(entry.IsNotDefault(), "Couldn't fix indexed plant: " + sp)) return default(PlantData);
      PlantData pd = new PlantData(fields, entry, col, sp.seed);
      return pd;
    }
  }
}
