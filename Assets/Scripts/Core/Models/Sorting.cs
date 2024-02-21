using System;
using System.Collections.Generic;
using System.Linq;
using BionicWombat;
using Newtonsoft.Json;
using UnityEngine;

namespace BionicWombat.PlantCollections {
  public enum PlantSorting {
    Newest,
    Alphabetical,
    Favorites,
  }

  public static class PlantSortingExtensions {
    public static string DisplayName(this PlantSorting pc) {
      switch (pc) {
        default: return pc.ToString();
      }
    }
    private static Dictionary<string, PlantSorting> reverseDict;
    public static PlantSorting PlantSortingEnum(this string pcString) {
      if (reverseDict == null) {
        reverseDict = new Dictionary<string, PlantSorting>();
        foreach (PlantSorting pc in Enum.GetValues(typeof(PlantSorting))) {
          reverseDict[pc.DisplayName()] = pc;
        }
      }
      return reverseDict[pcString];
    }
  }

  public static class PlantSortingHelpers {
    public static List<PlantIndexEntry> EntriesWithSort(List<PlantIndexEntry> entries, PlantSorting sort) {
      if (sort == PlantSorting.Newest) {
        return entries.Sorted((p1, p2) => p1.creationDate.CompareTo(p2.creationDate)).ToList();
      } else if (sort == PlantSorting.Alphabetical) {
        return entries.Sorted((p1, p2) => String.Compare(p1.name, p2.name)).ToList();
      } else if (sort == PlantSorting.Favorites) {
        return entries.Filter(pie => pie.favorite).ToList()
          .Sorted((p1, p2) => p1.creationDate.CompareTo(p2.creationDate)).ToList();  //sort by new after
      }
      Debug.LogError("Unsupported sort: " + sort);
      return EntriesWithSort(entries, PlantSorting.Newest);
    }

    public static List<PlantIndexEntry> FilterOutNoHybridsRemaining(List<PlantIndexEntry> entries) {
      List<PlantIndexEntry> l = new List<PlantIndexEntry>();
      foreach (PlantIndexEntry e in entries)
        if (e.hybridsRemaining != 0)
          l.Add(e);
      return l;
    }
  }
}
