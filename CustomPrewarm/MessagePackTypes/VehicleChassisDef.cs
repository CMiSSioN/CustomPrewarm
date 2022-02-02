using Harmony;
using MessagePack;
using System.Collections.Generic;

namespace CustomPrewarm.Serialize {
  [HarmonyPatch(typeof(BattleTech.VehicleChassisDef), "FromJSON")]
  [HarmonyPriority(Priority.Last)]
  public static class VehicleChassisDef_FromJSON {
    public static bool Prefix(BattleTech.VehicleChassisDef __instance) {
      return Core.disableFromJSON == false;
    }
  }
  [MessagePackObject]
  public class VehicleLocationDef {
    [Key(0)]
    public BattleTech.VehicleChassisLocations Location = BattleTech.VehicleChassisLocations.None;
    [Key(1)]
    public List<HardpointDef> Hardpoints = new List<HardpointDef>();
    [Key(2)]
    public float Tonnage = 0f;
    [Key(3)]
    public int InventorySlots = 0;
    [Key(4)]
    public float MaxArmor = 0f;
    [Key(5)]
    public float InternalStructure = 0f;
    public VehicleLocationDef() { }
    public VehicleLocationDef(BattleTech.VehicleLocationDef src) {
      this.Hardpoints = new List<HardpointDef>();
      foreach (var el in src.Hardpoints) { this.Hardpoints.Add(new HardpointDef(el)); };
      this.Location = src.Location;
      this.Tonnage = src.Tonnage;
      this.InventorySlots = src.InventorySlots;
      this.MaxArmor = src.MaxArmor;
      this.InternalStructure = src.InternalStructure;
    }
    public BattleTech.VehicleLocationDef toBT() {
      List<BattleTech.HardpointDef> hardpoints = new List<BattleTech.HardpointDef>();
      foreach (var el in this.Hardpoints) { hardpoints.Add(el.toBT()); }
      return new BattleTech.VehicleLocationDef(hardpoints.ToArray(), this.Location, this.Tonnage, this.InventorySlots, this.MaxArmor, this.InternalStructure );
    }
  }
  [MessagePackObject]
  public class VehicleChassisDef : ISerializable {
    [Key(0)]
    public List<FakeVector3> LOSSourcePositions = new List<FakeVector3>();
    [Key(1)]
    public List<FakeVector3> LOSTargetPositions = new List<FakeVector3>();
    [Key(2)]
    public List<VehicleLocationDef> Locations = new List<VehicleLocationDef>();
    [Key(3)]
    public bool HasTurret = false;
    [Key(4)]
    public string movementCapDefID = string.Empty;
    [Key(5)]
    public string pathingCapDefID = string.Empty;
    [Key(6)]
    public string hardpointDataDefID = string.Empty;
    [Key(7)]
    public DescriptionDef Description { get; set; } = new DescriptionDef();
    [Key(8)]
    public string PrefabIdentifier { get; set; } = string.Empty;
    [Key(9)]
    public string PrefabBase { get; set; } = string.Empty;
    [Key(10)]
    public float Tonnage { get; set; } = 0f;
    [Key(11)]
    public BattleTech.WeightClass weightClass { get; set; } = BattleTech.WeightClass.LIGHT;
    [Key(12)]
    public int BattleValue { get; set; } = 0;
    [Key(13)]
    public int TopSpeed { get; set; } = 0;
    [Key(14)]
    public int TurnRadius { get; set; } = 0;
    [Key(15)]
    public BattleTech.VehicleMovementType movementType { get; set; } = BattleTech.VehicleMovementType.Tracked;
    [Key(16)]
    public float SpotterDistanceMultiplier { get; set; } = 0f;
    [Key(17)]
    public float VisibilityMultiplier { get; set; } = 0f;
    [Key(18)]
    public float SensorRangeMultiplier { get; set; } = 0f;
    [Key(19)]
    public float Signature { get; set; } = 0f;
    [Key(20)]
    public float Radius { get; set; } = 0f;
    public VehicleChassisDef() { }
    public VehicleChassisDef(BattleTech.VehicleChassisDef src) {
      this.LOSSourcePositions = new List<FakeVector3>();
      foreach (var el in src.LOSSourcePositions) { this.LOSSourcePositions.Add(new FakeVector3(el)); };
      this.LOSTargetPositions = new List<FakeVector3>();
      foreach (var el in src.LOSTargetPositions) { this.LOSTargetPositions.Add(new FakeVector3(el)); };
      this.Locations = new List<VehicleLocationDef>();
      if (src.Locations != null) {
        for (int index = 0; index < src.Locations.Length; ++index) {
          this.Locations.Add(new VehicleLocationDef(src.Locations[index]));
        }
      }
      this.HasTurret = src.HasTurret;
      this.movementCapDefID = src.movementCapDefID;
      this.pathingCapDefID = src.pathingCapDefID;
      this.hardpointDataDefID = src.hardpointDataDefID;
      this.Description = new DescriptionDef(src.Description);
      this.PrefabIdentifier = src.PrefabIdentifier;
      this.PrefabBase = src.PrefabBase;
      this.Tonnage = src.Tonnage;
      this.weightClass = src.weightClass;
      this.BattleValue = src.BattleValue;
      this.TopSpeed = src.TopSpeed;
      this.TurnRadius = src.TurnRadius;
      this.movementType = src.movementType;
      this.SpotterDistanceMultiplier = src.SpotterDistanceMultiplier;
      this.VisibilityMultiplier = src.VisibilityMultiplier;
      this.SensorRangeMultiplier = src.SensorRangeMultiplier;
      this.Signature = src.Signature;
      this.Radius = src.Radius;
    }
    public object toBT(BattleTech.Data.DataManager dataManager) {
      BattleTech.VehicleChassisDef res = new BattleTech.VehicleChassisDef();
      res.dataManager = dataManager;
      List<BattleTech.FakeVector3> LOSSourcePositions = new List<BattleTech.FakeVector3>();
      foreach (var el in this.LOSSourcePositions) { LOSSourcePositions.Add(el.toBT()); };
      res.LOSSourcePositions = LOSSourcePositions.ToArray();
      List<BattleTech.FakeVector3> LOSTargetPositions = new List<BattleTech.FakeVector3>();
      foreach (var el in this.LOSTargetPositions) { LOSTargetPositions.Add(el.toBT()); };
      res.LOSTargetPositions = LOSTargetPositions.ToArray();
      res.Locations = new BattleTech.VehicleLocationDef[this.Locations.Count];
      for (int index = 0; index < this.Locations.Count; ++index) {
        res.Locations[index] = this.Locations[index].toBT();
      }
      res.HasTurret = this.HasTurret;
      res.movementCapDefID = this.movementCapDefID;
      res.pathingCapDefID = this.pathingCapDefID;
      res.hardpointDataDefID = this.hardpointDataDefID;
      res.Description = this.Description.toBT(dataManager) as BattleTech.DescriptionDef;
      res.PrefabIdentifier = this.PrefabIdentifier;
      res.PrefabBase = this.PrefabBase;
      res.Tonnage = this.Tonnage;
      res.weightClass = this.weightClass;
      res.BattleValue = this.BattleValue;
      res.TopSpeed = this.TopSpeed;
      res.TurnRadius = this.TurnRadius;
      res.movementType = this.movementType;
      res.SpotterDistanceMultiplier = this.SpotterDistanceMultiplier;
      res.VisibilityMultiplier = this.VisibilityMultiplier;
      res.SensorRangeMultiplier = this.SensorRangeMultiplier;
      res.Signature = this.Signature;
      res.Radius = this.Radius;
      return res;
    }
  }
}