using System;
using Newtonsoft.Json;
using UnityEngine;

namespace BionicWombat {
  [Serializable]
  public struct PlantIndexEntry {
    public static int DefaultHybridsRemaining = 3;
    public string name;
    public string uniqueID;
    public DateTime creationDate;
    public bool favorite;
    public int hybridsRemaining;
    public bool propegating;

    public PlantIndexEntry(SavedPlant savedPlant) :
      this(savedPlant.name, savedPlant.uniqueID, DateTime.Now, false,
        DefaultHybridsRemaining, false) { }

    [JsonConstructor]
    public PlantIndexEntry(string name, string uniqueID, DateTime creationDate,
        bool favorite, int hybridsRemaining, bool propegating) {
      this.name = name;
      this.uniqueID = uniqueID;
      this.creationDate = creationDate;
      this.favorite = favorite;
      this.hybridsRemaining = hybridsRemaining;
      this.propegating = propegating;
    }

    public static PlantIndexEntry GenerateEntry(string name) =>
      new PlantIndexEntry(name, GuidHelpers.Generate(), DateTime.Now, false,
        DefaultHybridsRemaining, false);

    public override string ToString() {
      return "[PlantIndexEntry] name: " + $"<color={LColor.orange}>{name}</color>" + " | creationDate: " + creationDate + " | favorite: " + favorite + " | uniqueID: " + uniqueID;
    }

    public string SafeName() {
      return PlantDataManager.StripNameChars(name);
    }

    public static bool operator ==(PlantIndexEntry s1, PlantIndexEntry s2) {
      return s1.uniqueID == s2.uniqueID;
    }

    public static bool operator !=(PlantIndexEntry s1, PlantIndexEntry s2) {
      return !(s1 == s2);
    }

    public override bool Equals(object obj) {
      if (!(obj is PlantIndexEntry))
        return false;

      return (PlantIndexEntry)obj == this;
    }

    public override int GetHashCode() {
      return uniqueID.GetHashCode();
    }

    public PlantIndexEntry Copy() {
      return new PlantIndexEntry(name, uniqueID, creationDate, favorite, hybridsRemaining, propegating);
    }
  }
}
