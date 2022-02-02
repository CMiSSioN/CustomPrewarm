using System;
using System.Collections.Generic;
using Harmony;
using MessagePack;

namespace CustomPrewarm.Serialize {
  [HarmonyPatch(typeof(BattleTech.MechDef), "FromJSON")]
  [HarmonyPriority(Priority.Last)]
  public static class MechDef_FromJSON {
    public static bool Prefix(BattleTech.MechDef __instance) {
      if (Core.disableFromJSON == false) { return true; }
      __instance.RefreshLocationReferences();
      return false;
    }
  }
  //[HarmonyPatch(typeof(BattleTech.UI.MechBayMechInfoWidget), "SetData")]
  //[HarmonyPatch(new Type[] { typeof(BattleTech.SimGameState), typeof(BattleTech.UI.MechBayPanel), typeof(BattleTech.Data.DataManager), typeof(BattleTech.UI.MechBayMechUnitElement), typeof(bool), typeof(bool) })]
  //[HarmonyPriority(Priority.Last)]
  //public static class MechBayMechInfoWidget_SetData_0 {
  //  public static bool originalCall = false;
  //  public static bool Prefix(BattleTech.UI.MechBayMechInfoWidget __instance, BattleTech.SimGameState sim, BattleTech.UI.MechBayPanel mechBay, BattleTech.Data.DataManager dataManager, BattleTech.UI.MechBayMechUnitElement mechElement,bool useNoMechOverlay, bool useRepairButton) {
  //    if (originalCall == true) { return true; }
  //    originalCall = true;
  //    Log.M?.TWL(0, "MechBayMechInfoWidget.SetData0:" + (mechElement == null ? "null" : mechElement.MechDef.Description.Id));
  //    if (mechElement != null) {
  //      Log.M?.WL(0, mechElement.MechDef.ToJSON());
  //    }
  //    try {
  //      __instance.SetData(sim, mechBay, dataManager, mechElement, useNoMechOverlay, useRepairButton);
  //    } catch(Exception e) {
  //      Log.M_Err?.TWL(0, e.ToString(), true);
  //    }
  //    originalCall = false;
  //    return false;
  //  }
  //}
  //[HarmonyPatch(typeof(BattleTech.UI.MechBayMechInfoWidget), "SetData")]
  //[HarmonyPatch(new Type[] { typeof(BattleTech.MechDef), typeof(BattleTech.Data.DataManager) })]
  //[HarmonyPriority(Priority.Last)]
  //public static class MechBayMechInfoWidget_SetData_1 {
  //  public static void Prefix(BattleTech.UI.MechBayMechInfoWidget __instance, BattleTech.MechDef mechDef, BattleTech.Data.DataManager dataManager) {
  //    Log.M?.TWL(0, "MechBayMechInfoWidget.SetData1:" + (mechDef == null ? "null" : mechDef.Description.Id));
  //  }
  //}
  [MessagePackObject]
  public class ActorDef {
    [Key(0)]
    public int Version = 0;
    [Key(1)]
    public TagSet requiredToSpawnCompanyTags = new TagSet();
    [Key(2)]
    public DateTime minAppearanceDate = DateTime.MinValue;
    [Key(3)]
    public DescriptionDef Description { get; set; } = new DescriptionDef();
    public ActorDef() {}
    public ActorDef(BattleTech.ActorDef src) {
      this.Version = src.Version;
      this.requiredToSpawnCompanyTags = new TagSet(src.requiredToSpawnCompanyTags);
      this.minAppearanceDate = src.minAppearanceDate;
      this.Description = new DescriptionDef(src.Description);
    }
    public void fill(BattleTech.ActorDef trg, BattleTech.Data.DataManager dataManager) {
      trg.dataManager = dataManager;
      trg.Version = this.Version;
      trg.requiredToSpawnCompanyTags = this.requiredToSpawnCompanyTags.toBT() as HBS.Collections.TagSet;
      trg.minAppearanceDate = this.minAppearanceDate;
      trg.Description = this.Description.toBT(dataManager) as BattleTech.DescriptionDef;
    }
  }
  [MessagePackObject]
  public class PilotableActorDef : ActorDef {
    [Key(4)]
    public string chassisID = string.Empty;
    [Key(5)]
    public int battleValue = 0;
    [Key(6)]
    public string paintTextureID = string.Empty;
    [Key(7)]
    public string heraldryID = string.Empty;
    public PilotableActorDef() {}
    public PilotableActorDef(BattleTech.PilotableActorDef src):base(src) {
      this.chassisID = src.chassisID;
      this.battleValue = src.battleValue;
      this.paintTextureID = src.paintTextureID;
      this.heraldryID = src.heraldryID;
    }
    public void fill(BattleTech.PilotableActorDef trg, BattleTech.Data.DataManager dataManager) {
      base.fill(trg, dataManager);
      trg.chassisID = this.chassisID;
      trg.battleValue = this.battleValue;
      trg.paintTextureID = this.paintTextureID;
      trg.heraldryID = this.heraldryID;
    }
  }
  [MessagePackObject]
  public class LocationLoadoutDef {
    [Key(0)]
    public BattleTech.ChassisLocations Location = BattleTech.ChassisLocations.None;
    [Key(1)]
    public float CurrentArmor = 0f;
    [Key(2)]
    public float CurrentRearArmor = 0f;
    [Key(3)]
    public float CurrentInternalStructure = 0f;
    [Key(4)]
    public float AssignedArmor = 0f;
    [Key(5)]
    public float AssignedRearArmor = 0f;
    [Key(6)]
    public BattleTech.LocationDamageLevel DamageLevel { get; set; } = BattleTech.LocationDamageLevel.Functional;
    public LocationLoadoutDef() {}
    public LocationLoadoutDef(BattleTech.LocationLoadoutDef def) {
      this.Location = def.Location;
      this.CurrentArmor = def.CurrentArmor;
      this.CurrentRearArmor = def.CurrentRearArmor;
      this.CurrentInternalStructure = def.CurrentInternalStructure;
      this.AssignedArmor = def.AssignedArmor;
      this.AssignedRearArmor = def.AssignedRearArmor;
      this.DamageLevel = def.DamageLevel;
    }
    public BattleTech.LocationLoadoutDef toBT() {
      return new BattleTech.LocationLoadoutDef(
        this.Location,
        this.CurrentArmor,
        this.CurrentRearArmor,
        this.CurrentInternalStructure,
        this.AssignedArmor,
        this.AssignedRearArmor,
        this.DamageLevel);
    }
  }
  [MessagePackObject]
  public class BaseComponentRef {
    [Key(0)]
    public string ComponentDefID = string.Empty;
    [Key(1)]
    public BattleTech.ComponentDamageLevel DamageLevel = BattleTech.ComponentDamageLevel.Functional;
    [Key(2)]
    public string prefabName = string.Empty;
    [Key(3)]
    public bool hasPrefabName = false;
    [Key(4)]
    public string SimGameUID { get; set; } = string.Empty;
    [Key(5)]
    public BattleTech.ComponentType ComponentDefType { get; set; } = BattleTech.ComponentType.NotSet;
    [Key(6)]
    public int HardpointSlot { get; set; } = -1;
    [Key(7)]
    public bool IsFixed { get; set; } = false;
    public BaseComponentRef() {}
    public BaseComponentRef(BattleTech.BaseComponentRef src) {
      this.ComponentDefType = src.ComponentDefType;
      this.HardpointSlot = src.HardpointSlot;
      this.DamageLevel = src.DamageLevel;
      this.prefabName = src.prefabName;
      this.hasPrefabName = src.hasPrefabName;
      this.IsFixed = src.IsFixed;
      this.ComponentDefID = src.ComponentDefID;
      this.SimGameUID = src.SimGameUID;
    }
    public void fill(BattleTech.BaseComponentRef trg, BattleTech.Data.DataManager dataManager) {
      trg.dataManager = dataManager;
      trg.ComponentDefType = this.ComponentDefType;
      trg.HardpointSlot = this.HardpointSlot;
      trg.DamageLevel = this.DamageLevel;
      trg.prefabName = this.prefabName;
      trg.hasPrefabName = this.hasPrefabName;
      trg.IsFixed = this.IsFixed;
      trg.ComponentDefID = this.ComponentDefID;
      trg.SimGameUID = this.SimGameUID;
    }
  }
  [MessagePackObject]
  public class MechComponentRef : BaseComponentRef {
    [Key(8)]
    public BattleTech.ChassisLocations MountedLocation { get; set; } = BattleTech.ChassisLocations.None;
    public MechComponentRef() {}
    public MechComponentRef(BattleTech.MechComponentRef src) : base((BattleTech.BaseComponentRef)src) { 
      this.MountedLocation = src.MountedLocation;
    }
    public void fill(BattleTech.MechComponentRef trg, BattleTech.Data.DataManager dataManager) {
      base.fill(trg, dataManager);
      trg.MountedLocation = this.MountedLocation;
    }
    public BattleTech.MechComponentRef toBT(BattleTech.Data.DataManager dataManager) {
      BattleTech.MechComponentRef res = new BattleTech.MechComponentRef();
      this.fill(res, dataManager);
      return res;
    }
  }
  [MessagePackObject]
  public class MechDef : PilotableActorDef, ISerializable {
    [Key(8)]
    public string prefabOverride = string.Empty;
    [Key(9)]
    public List<LocationLoadoutDef> Locations = new List<LocationLoadoutDef>();
    [Key(10)]
    public List<MechComponentRef> inventory = new List<MechComponentRef>();
    [Key(11)]
    public int simGameMechPartCost = 0;
    [Key(12)]
    public TagSet MechTags { get; set; } = new TagSet();
    public MechDef() { }
    public MechDef(BattleTech.MechDef src):base(src) {
      this.prefabOverride = src.prefabOverride;
      this.simGameMechPartCost = src.simGameMechPartCost;
      this.MechTags = src.MechTags==null? new TagSet() : new TagSet(src.MechTags);
      this.Locations = new List<LocationLoadoutDef>(); foreach (var el in src.Locations) { this.Locations.Add(new LocationLoadoutDef(el)); };
      this.inventory = new List<MechComponentRef>(); foreach (var el in src.inventory) { this.inventory.Add(new MechComponentRef(el)); };
    }
    public void fill(BattleTech.MechDef trg, BattleTech.Data.DataManager dataManager) {
      base.fill(trg, dataManager);
      trg.prefabOverride = this.prefabOverride;
      trg.simGameMechPartCost = this.simGameMechPartCost;
      List<BattleTech.LocationLoadoutDef> Locations = new List<BattleTech.LocationLoadoutDef>(); foreach (var el in this.Locations) { Locations.Add(el.toBT()); };
      trg.Locations = Locations.ToArray();
      List<BattleTech.MechComponentRef> inventory = new List<BattleTech.MechComponentRef>(); foreach (var el in this.inventory) { inventory.Add(el.toBT(dataManager)); };
      trg.inventory = inventory.ToArray();
      trg.MechTags = this.MechTags.toBT() as HBS.Collections.TagSet;
    }
    public object toBT(BattleTech.Data.DataManager dataManager) {
      BattleTech.MechDef res = new BattleTech.MechDef();
      this.fill(res, dataManager);
      return res;
    }
  }
}