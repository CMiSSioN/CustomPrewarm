using Harmony;
using MessagePack;
using System.Collections.Generic;

namespace CustomPrewarm.Serialize {
  [HarmonyPatch(typeof(BattleTech.VehicleDef), "FromJSON")]
  [HarmonyPriority(Priority.Last)]
  public static class VehicleDef_FromJSON {
    public static bool Prefix(BattleTech.VehicleDef __instance) {
      return Core.disableFromJSON == false;
    }
  }
  [MessagePackObject]
  public class VehicleLocationLoadoutDef {
    [Key(0)]
    public BattleTech.VehicleChassisLocations Location = BattleTech.VehicleChassisLocations.None;
    [Key(1)]
    public float CurrentArmor = 0f;
    [Key(2)]
    public float CurrentInternalStructure = 0f;
    [Key(3)]
    public float AssignedArmor = 0f;
    [Key(4)]
    public BattleTech.LocationDamageLevel DamageLevel { get; set; } = BattleTech.LocationDamageLevel.Functional;
    public VehicleLocationLoadoutDef() { }
    public VehicleLocationLoadoutDef(BattleTech.VehicleLocationLoadoutDef def) {
      this.Location = def.Location;
      this.CurrentArmor = def.CurrentArmor;
      this.CurrentInternalStructure = def.CurrentInternalStructure;
      this.AssignedArmor = def.AssignedArmor;
      this.DamageLevel = def.DamageLevel;
    }
    public BattleTech.VehicleLocationLoadoutDef toBT() {
      return new BattleTech.VehicleLocationLoadoutDef(
        this.Location,
        this.CurrentArmor,
        this.CurrentInternalStructure,
        this.AssignedArmor,
        this.DamageLevel);
    }
  }
  [MessagePackObject]
  public class VehicleComponentRef : BaseComponentRef {
    [Key(8)]
    public BattleTech.VehicleChassisLocations MountedLocation { get; set; } = BattleTech.VehicleChassisLocations.None;
    public VehicleComponentRef() { }
    public VehicleComponentRef(BattleTech.VehicleComponentRef src) : base((BattleTech.BaseComponentRef)src) {
      this.MountedLocation = src.MountedLocation;
    }
    public void fill(BattleTech.VehicleComponentRef trg, BattleTech.Data.DataManager dataManager) {
      base.fill(trg, dataManager);
      trg.MountedLocation = this.MountedLocation;
    }
    public BattleTech.VehicleComponentRef toBT(BattleTech.Data.DataManager dataManager) {
      BattleTech.VehicleComponentRef res = new BattleTech.VehicleComponentRef();
      this.fill(res, dataManager);
      return res;
    }
  }
  [MessagePackObject]
  public class VehicleDef : PilotableActorDef, ISerializable {
    [Key(8)]
    public List<VehicleLocationLoadoutDef> Locations = new List<VehicleLocationLoadoutDef>();
    [Key(9)]
    public List<VehicleComponentRef> inventory = new List<VehicleComponentRef>();
    [Key(10)]
    public TagSet VehicleTags { get; set; } = new TagSet();
    public VehicleDef() { }
    public VehicleDef(BattleTech.VehicleDef src) : base(src) {
      this.VehicleTags = src.VehicleTags == null ? new TagSet() : new TagSet(src.VehicleTags);
      this.Locations = new List<VehicleLocationLoadoutDef>(); foreach (var el in src.Locations) { this.Locations.Add(new VehicleLocationLoadoutDef(el)); };
      this.inventory = new List<VehicleComponentRef>(); foreach (var el in src.inventory) { this.inventory.Add(new VehicleComponentRef(el)); };
    }
    public void fill(BattleTech.VehicleDef trg, BattleTech.Data.DataManager dataManager) {
      base.fill(trg, dataManager);
      List<BattleTech.VehicleLocationLoadoutDef> Locations = new List<BattleTech.VehicleLocationLoadoutDef>(); foreach (var el in this.Locations) { Locations.Add(el.toBT()); };
      trg.Locations = Locations.ToArray();
      List<BattleTech.VehicleComponentRef> inventory = new List<BattleTech.VehicleComponentRef>(); foreach (var el in this.inventory) { inventory.Add(el.toBT(dataManager)); };
      trg.inventory = inventory.ToArray();
      trg.VehicleTags = this.VehicleTags.toBT();
    }
    public object toBT(BattleTech.Data.DataManager dataManager) {
      BattleTech.VehicleDef res = new BattleTech.VehicleDef();
      this.fill(res, dataManager);
      return res;
    }
  }
}