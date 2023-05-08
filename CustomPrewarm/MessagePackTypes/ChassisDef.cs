using BattleTech.Data;
using HarmonyLib;
using MessagePack;
using System.Collections.Generic;

namespace CustomPrewarm.Serialize {
  [HarmonyPatch(typeof(BattleTech.ChassisDef), "FromJSON")]
  [HarmonyPriority(Priority.Last)]
  public static class ChassisDef_FromJSON {
    public static bool Prefix(BattleTech.ChassisDef __instance) {
      if (Core.disableFromJSON == false) { return true; }
      __instance.refreshLocationReferences();
      return false;
    }
  }
  [MessagePackObject]
  public class HardpointDef {
    [Key(0)]
    public string weaponMountID = string.Empty ;
    [Key(1)]
    public bool Omni = false;
    public HardpointDef() { }
    public HardpointDef(BattleTech.HardpointDef src) {
      this.weaponMountID = src.WeaponMountValue.Name;
      this.Omni = src.Omni;
    }
    public BattleTech.HardpointDef toBT() {
      return new BattleTech.HardpointDef(BattleTech.WeaponCategoryEnumeration.GetWeaponCategoryByName(this.weaponMountID), this.Omni);
    }
  }
  [MessagePackObject]
  public class LocationDef {
    [Key(0)]
    public BattleTech.ChassisLocations Location = BattleTech.ChassisLocations.None;
    [Key(1)]
    public List<HardpointDef> Hardpoints = new List<HardpointDef>();
    [Key(2)]
    public float Tonnage = 0f;
    [Key(3)]
    public int InventorySlots = 0;
    [Key(4)]
    public float MaxArmor = 0f;
    [Key(5)]
    public float MaxRearArmor = 0f;
    [Key(6)]
    public float InternalStructure = 0f;
    public LocationDef() { }
    public LocationDef(BattleTech.LocationDef def) {
      this.Hardpoints = new List<HardpointDef>();
      foreach(var el in def.Hardpoints) { this.Hardpoints.Add(new HardpointDef(el)); };
      this.Location = def.Location;
      this.Tonnage = def.Tonnage;
      this.InventorySlots = def.InventorySlots;
      this.MaxArmor = def.MaxArmor;
      this.MaxRearArmor = def.MaxRearArmor;
      this.InternalStructure = def.InternalStructure;
    }
    public BattleTech.LocationDef toBT() {
      List<BattleTech.HardpointDef> hardpoints = new List<BattleTech.HardpointDef>();
      foreach (var el in this.Hardpoints) { hardpoints.Add(el.toBT()); }
      return new BattleTech.LocationDef(hardpoints.ToArray(), this.Location, this.Tonnage, this.InventorySlots, this.MaxArmor, this.MaxRearArmor, this.InternalStructure);
    }
  }
  [MessagePackObject]
  public class ChassisDef : ISerializable {
    [Key(0)]
    public List<FakeVector3> LOSSourcePositions = new List<FakeVector3>();
    [Key(1)]
    public List<FakeVector3> LOSTargetPositions = new List<FakeVector3>();
    [Key(2)]
    public List<LocationDef> Locations = new List<LocationDef>();
    [Key(3)]
    public int MechPartCount = 0;
    [Key(4)]
    public string movementCapDefID = string.Empty;
    [Key(5)]
    public string pathingCapDefID = string.Empty;
    [Key(6)]
    public string hardpointDataDefID = string.Empty;
    [Key(7)]
    public DescriptionDef Description { get; set; } = new DescriptionDef();
    [Key(8)]
    public string VariantName { get; set; } = string.Empty;
    [Key(9)]
    public string StockRole { get; set; } = string.Empty;
    [Key(10)]
    public string YangsThoughts { get; set; } = string.Empty;
    [Key(11)]
    public string PrefabIdentifier { get; set; } = string.Empty;
    [Key(12)]
    public string PrefabBase { get; set; } = string.Empty;
    [Key(13)]
    public float Tonnage { get; set; } = 0f;
    [Key(14)]
    public float InitialTonnage { get; set; } = 0f;
    [Key(15)]
    public BattleTech.WeightClass weightClass { get; set; } = BattleTech.WeightClass.LIGHT;
    [Key(16)]
    public int BattleValue { get; set; } = 0;
    [Key(17)]
    public int Heatsinks { get; set; } = 0;
    [Key(18)]
    public int TopSpeed { get; set; } = 0;
    [Key(19)]
    public int TurnRadius { get; set; } = 0;
    [Key(20)]
    public int MaxJumpjets { get; set; } = 0;
    [Key(21)]
    public float Stability { get; set; } = 0f;
    [Key(22)]
    public List<float> StabilityDefenses { get; set; } = new List<float>();
    [Key(23)]
    public float SpotterDistanceMultiplier { get; set; } = 0f;
    [Key(24)]
    public float VisibilityMultiplier { get; set; } = 0f;
    [Key(25)]
    public float SensorRangeMultiplier { get; set; } = 0f;
    [Key(26)]
    public float Signature { get; set; } = 0f;
    [Key(27)]
    public float Radius { get; set; } = 0f;
    [Key(28)]
    public bool PunchesWithLeftArm { get; set; } = false;
    [Key(29)]
    public bool AlwaysPunchIfPossible { get; set; } = false;
    [Key(30)]
    public float EngageRangeModifier { get; set; } = 0f;
    [Key(31)]
    public float MeleeDamage { get; set; } = 0f;
    [Key(32)]
    public float MeleeInstability { get; set; } = 0f;
    [Key(33)]
    public float MeleeToHitModifier { get; set; } = 0f;
    [Key(34)]
    public float DFADamage { get; set; } = 0f;
    [Key(35)]
    public float DFAToHitModifier { get; set; } = 0f;
    [Key(36)]
    public float DFASelfDamage { get; set; } = 0f;
    [Key(37)]
    public float DFAInstability { get; set; } = 0f;
    [Key(38)]
    public List<MechComponentRef> FixedEquipment { get; set; } = new List<MechComponentRef>();
    [Key(39)]
    public TagSet ChassisTags { get; set; } = new TagSet();
    [Key(40)]
    public int MechPartMax { get; set; } = 0;
    public ChassisDef() { }
    public ChassisDef(BattleTech.ChassisDef def) {
      this.Description = new DescriptionDef(def.Description);
      this.VariantName = def.VariantName;
      this.StockRole = def.StockRole;
      this.YangsThoughts = def.YangsThoughts;
      this.PrefabIdentifier = def.PrefabIdentifier;
      this.PrefabBase = def.PrefabBase;
      this.Tonnage = def.Tonnage;
      this.InitialTonnage = def.InitialTonnage;
      this.weightClass = def.weightClass;
      this.BattleValue = def.BattleValue;
      this.Heatsinks = def.Heatsinks;
      this.TopSpeed = def.TopSpeed;
      this.TurnRadius = def.TurnRadius;
      this.MaxJumpjets = def.MaxJumpjets;
      this.Stability = def.Stability;
      this.StabilityDefenses = new List<float>(def.StabilityDefenses);
      this.SpotterDistanceMultiplier = def.SpotterDistanceMultiplier;
      this.VisibilityMultiplier = def.VisibilityMultiplier;
      this.SensorRangeMultiplier = def.SensorRangeMultiplier;
      this.Signature = def.Signature;
      this.Radius = def.Radius;
      this.PunchesWithLeftArm = def.PunchesWithLeftArm;
      this.EngageRangeModifier = def.EngageRangeModifier;
      this.MeleeDamage = def.MeleeDamage;
      this.MeleeInstability = def.MeleeInstability;
      this.MeleeToHitModifier = def.MeleeToHitModifier;
      this.DFAToHitModifier = def.DFAToHitModifier;
      this.DFADamage = def.DFADamage;
      this.DFASelfDamage = def.DFASelfDamage;
      this.DFAInstability = def.DFAInstability;
      this.LOSSourcePositions = new List<FakeVector3>();
      foreach (var el in def.LOSSourcePositions) { this.LOSSourcePositions.Add(new FakeVector3(el)); };
      this.LOSTargetPositions = new List<FakeVector3>();
      foreach (var el in def.LOSTargetPositions) { this.LOSTargetPositions.Add(new FakeVector3(el)); };
      this.Locations = new List<LocationDef>();
      if (def.Locations != null) {
        for (int index = 0; index < def.Locations.Length; ++index) {
          this.Locations.Add(new LocationDef(def.Locations[index]));
        }
      }
      this.FixedEquipment = new List<MechComponentRef>();
      if (def.FixedEquipment != null) {
        for (int index = 0; index < def.fixedEquipment.Length; ++index)
          this.FixedEquipment.Add(new MechComponentRef(def.fixedEquipment[index]));
      }
      this.ChassisTags = new TagSet(def.ChassisTags);
      this.movementCapDefID = def.movementCapDefID;
      this.pathingCapDefID = def.pathingCapDefID;
      this.hardpointDataDefID = def.hardpointDataDefID;
    }
    public object toBT(DataManager dataManager) {
      BattleTech.ChassisDef res = new BattleTech.ChassisDef();
      res.Description = this.Description.toBT(dataManager) as BattleTech.DescriptionDef;
      res.VariantName = this.VariantName;
      res.StockRole = this.StockRole;
      res.YangsThoughts = this.YangsThoughts;
      res.PrefabIdentifier = this.PrefabIdentifier;
      res.PrefabBase = this.PrefabBase;
      res.Tonnage = this.Tonnage;
      res.InitialTonnage = this.InitialTonnage;
      res.weightClass = this.weightClass;
      res.BattleValue = this.BattleValue;
      res.Heatsinks = this.Heatsinks;
      res.TopSpeed = this.TopSpeed;
      res.TurnRadius = this.TurnRadius;
      res.MaxJumpjets = this.MaxJumpjets;
      res.Stability = this.Stability;
      res.StabilityDefenses = new List<float>(this.StabilityDefenses).ToArray();
      res.SpotterDistanceMultiplier = this.SpotterDistanceMultiplier;
      res.VisibilityMultiplier = this.VisibilityMultiplier;
      res.SensorRangeMultiplier = this.SensorRangeMultiplier;
      res.Signature = this.Signature;
      res.Radius = this.Radius;
      res.PunchesWithLeftArm = this.PunchesWithLeftArm;
      res.EngageRangeModifier = this.EngageRangeModifier;
      res.MeleeDamage = this.MeleeDamage;
      res.MeleeInstability = this.MeleeInstability;
      res.MeleeToHitModifier = this.MeleeToHitModifier;
      res.DFAToHitModifier = this.DFAToHitModifier;
      res.DFADamage = this.DFADamage;
      res.DFASelfDamage = this.DFASelfDamage;
      res.DFAInstability = this.DFAInstability;
      List<BattleTech.FakeVector3> LOSSourcePositions = new List<BattleTech.FakeVector3>();
      foreach (var el in this.LOSSourcePositions) { LOSSourcePositions.Add(el.toBT()); };
      res.LOSSourcePositions = LOSSourcePositions.ToArray();
      List<BattleTech.FakeVector3> LOSTargetPositions = new List<BattleTech.FakeVector3>();
      foreach (var el in this.LOSTargetPositions) { LOSTargetPositions.Add(el.toBT()); };
      res.LOSTargetPositions = LOSTargetPositions.ToArray();
      res.Locations = new BattleTech.LocationDef[this.Locations.Count];
      for (int index = 0; index < this.Locations.Count; ++index) {
        res.Locations[index] = this.Locations[index].toBT();
      }
      res.fixedEquipment = new BattleTech.MechComponentRef[this.FixedEquipment.Count];
      for (int index = 0; index < this.FixedEquipment.Count; ++index) {
        res.fixedEquipment[index] = this.FixedEquipment[index].toBT(dataManager);
      }
      res.ChassisTags = this.ChassisTags.toBT();
      res.movementCapDefID = this.movementCapDefID;
      res.pathingCapDefID = this.pathingCapDefID;
      res.hardpointDataDefID = this.hardpointDataDefID;
      return res;
    }
  }
}