using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace BionicWombat {
  public static class LeafParamMigrator {
    public delegate float MigrationFunction(LeafParamDict fields);
    // MakeStruct.html?a=LeafParamMigration LPK key int oldVersionNumber MigrationFunction func bool forceEnable
    public struct LeafParamMigration {
      public LPK key;
      public int oldVersionNumber; //see: SavedPlant.VersionNumber
      public MigrationFunction func;
      public bool forceEnable; //force new "key" to be set to enabled

      public LeafParamMigration(LPK key, int oldVersionNumber, MigrationFunction func, bool forceEnable = false) {
        this.key = key;
        this.oldVersionNumber = oldVersionNumber;
        this.func = func;
        this.forceEnable = forceEnable;
      }

      public override string ToString() {
        return "[LeafParamMigration] key: " + key + " | oldVersionNumber: " + oldVersionNumber + " | func: " + func + " | forceEnable: " + forceEnable;
      }
    }

    public static Dictionary<LPK, LeafParamMigration> GetMigrations(int oldVersionNumber) {
      LeafParamDict defaults = LeafParamDefaults.Defaults;
      Dictionary<LPK, LeafParamMigration> migrations = new Dictionary<LPK, LeafParamMigration>();
      if (oldVersionNumber <= 1) {
        migrations.Add(LPK.StemLengthIncrease, new LeafParamMigration(LPK.StemLengthIncrease, oldVersionNumber, (fields) => {
          return (fields[LPK.LeafCount].value - 1) * fields[LPK.StemLengthIncrease].value;
        }));
      }
      if (oldVersionNumber <= 3) {
        migrations.Add(LPK.Heart, new LeafParamMigration(LPK.Heart, oldVersionNumber, (loadedFields) => {
          return loadedFields[LPK.Heart].enabled ? 0.3f : -0.3f;
        }, true));
        migrations.Add(LPK.VeinSplit, new LeafParamMigration(LPK.VeinSplit, oldVersionNumber, (loadedFields) => {
          return loadedFields[LPK.VeinSplit].enabled ? 0.3f : -0.3f;
        }, true));
        migrations.Add(LPK.TexRadianceInversion, new LeafParamMigration(LPK.TexRadianceInversion, oldVersionNumber, (loadedFields) => {
          return loadedFields[LPK.TexRadianceInversion].enabled ? 0.3f : -0.3f;
        }, true));
        migrations.Add(LPK.Lobes, new LeafParamMigration(LPK.Lobes, oldVersionNumber, (loadedFields) => {
          return loadedFields[LPK.Lobes].enabled ? 0.5f : -0.3f;
        }, true));
      }
      if (oldVersionNumber <= 4) {
        migrations.Add(LPK.TexMaskingStrength, new LeafParamMigration(LPK.TexMaskingStrength, oldVersionNumber, (loadedFields) => {
          return loadedFields[LPK.TexVeinOpacity].valuePercent * 1.2f;
        }, true));
      }
      if (oldVersionNumber <= 5) {
        migrations.Add(LPK.StemLength, new LeafParamMigration(LPK.StemLength, oldVersionNumber, (loadedFields) => {
          FloatRange range = defaults[LPK.StemLength].range;
          return Mathf.Clamp(loadedFields[LPK.StemLength].value, range.Start, range.End);
        }, true));
      }
      return migrations;
    }

    public static void PerformFixedMigrations(SavedPlant oldPreset) {
      // if (oldPreset.version <= 2 && oldPreset.hybridsRemaining == 0) {
      //   oldPreset.hybridsRemaining = 3;
      // }
    }
  }
}
