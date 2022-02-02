using System.Collections.Generic;
using BattleTech.Data;
using Harmony;
using MessagePack;

namespace CustomPrewarm.Serialize {
  [HarmonyPatch(typeof(BattleTech.HardpointDataDef), "FromJSON")]
  [HarmonyPriority(Priority.Last)]
  public static class HardpointDataDef_FromJSON {
    public static bool Prefix(BattleTech.HardpointDataDef __instance) {
      return Core.disableFromJSON == false;
    }
  }
  [MessagePackObject]
  public class HardpointDataDef : ISerializable {
    [Key(0)]
    public string ID = string.Empty;
    [Key(1)]
    public List<HardpointDataDef._WeaponHardpointData> HardpointData = new List<_WeaponHardpointData>();
    public HardpointDataDef(){}
    public HardpointDataDef(BattleTech.HardpointDataDef src) {
      this.ID = src.ID;
      this.HardpointData = new List<_WeaponHardpointData>();
      if(src.HardpointData != null) {
        foreach(var el in src.HardpointData) { this.HardpointData.Add(new _WeaponHardpointData(el)); }
      }
    }
    public object toBT(DataManager dataManager) {
      List<BattleTech.HardpointDataDef._WeaponHardpointData> HardpointData = new List<BattleTech.HardpointDataDef._WeaponHardpointData>();
      foreach (var el in this.HardpointData) { HardpointData.Add(el.toBT()); };
      return new BattleTech.HardpointDataDef(this.ID, HardpointData.ToArray());
    }
    [MessagePackObject]
    public class _WeaponHardpointData {
      [Key(0)]
      public string location = string.Empty;
      [Key(1)]
      public List<List<string>> weapons = new List<List<string>>();
      [Key(2)]
      public List<string> blanks = new List<string>();
      [Key(3)]
      public List<string> mountingPoints = new List<string>();
      public _WeaponHardpointData() { }
      public _WeaponHardpointData(BattleTech.HardpointDataDef._WeaponHardpointData src) {
        this.location = src.location;
        this.weapons = new List<List<string>>();
        if(src.weapons != null) foreach (string[] weapon in src.weapons) { this.weapons.Add(weapon == null? new List<string>() : new List<string>(weapon)); };
        this.blanks = src.blanks == null? new List<string>(): new List<string>(src.blanks);
        this.mountingPoints = src.mountingPoints == null? new List<string>() : new List<string>(src.mountingPoints);
      }
      public BattleTech.HardpointDataDef._WeaponHardpointData toBT() {
        BattleTech.HardpointDataDef._WeaponHardpointData result = new BattleTech.HardpointDataDef._WeaponHardpointData() { location = this.location, weapons = null, blanks = this.blanks.ToArray(), mountingPoints = this.mountingPoints.ToArray() };
        result.weapons = new string[weapons.Count][];
        for(int i=0;i < this.weapons.Count; ++i) { result.weapons[i] = this.weapons[i].ToArray(); }
        return result;
      }
    }
  }
}