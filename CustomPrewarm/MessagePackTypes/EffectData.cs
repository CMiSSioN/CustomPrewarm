using HarmonyLib;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace CustomPrewarm.Serialize {
  [MessagePackObject]
  public class EffectTargetingData {
    [Key(0)]
    public BattleTech.EffectTriggerType effectTriggerType = BattleTech.EffectTriggerType.Passive;
    [Key(1)]
    public int triggerLimit = 0;
    [Key(2)]
    public int extendDurationOnTrigger = 0;
    [Key(3)]
    public BattleTech.AbilityDef.SpecialRules specialRules = BattleTech.AbilityDef.SpecialRules.NotSet;
    [Key(4)]
    public BattleTech.AuraEffectType auraEffectType = BattleTech.AuraEffectType.NotSet;
    [Key(5)]
    public BattleTech.EffectTargetType effectTargetType = BattleTech.EffectTargetType.NotSet;
    [Key(6)]
    public bool alsoAffectCreator = false;
    [Key(7)]
    public float range = 0f;
    [Key(8)]
    public bool forcePathRebuild = false;
    [Key(9)]
    public bool forceVisRebuild = false;
    [Key(10)]
    public bool showInTargetPreview = false;
    [Key(11)]
    public bool showInStatusPanel = false;
    [Key(12)]
    public bool hideApplicationFloatie = false;
    public BattleTech.EffectTargetingData toBT() {
      return new BattleTech.EffectTargetingData() {
        effectTriggerType = this.effectTriggerType,
        triggerLimit = this.triggerLimit,
        extendDurationOnTrigger = this.extendDurationOnTrigger,
        specialRules = this.specialRules,
        auraEffectType = this.auraEffectType,
        effectTargetType = this.effectTargetType,
        alsoAffectCreator = this.alsoAffectCreator,
        range = this.range,
        forcePathRebuild = this.forcePathRebuild,
        forceVisRebuild = this.forceVisRebuild,
        showInTargetPreview = this.showInTargetPreview,
        showInStatusPanel = this.showInStatusPanel,
        hideApplicationFloatie = this.hideApplicationFloatie
      };
    }
    public EffectTargetingData() { }
    public EffectTargetingData(BattleTech.EffectTargetingData source) {
      effectTriggerType = source.effectTriggerType;
      triggerLimit = source.triggerLimit;
      extendDurationOnTrigger = source.extendDurationOnTrigger;
      specialRules = source.specialRules;
      auraEffectType = source.auraEffectType;
      effectTargetType = source.effectTargetType;
      alsoAffectCreator = source.alsoAffectCreator;
      range = source.range;
      forcePathRebuild = source.forcePathRebuild;
      forceVisRebuild = source.forceVisRebuild;
      showInTargetPreview = source.showInTargetPreview;
      showInStatusPanel = source.showInStatusPanel;
      hideApplicationFloatie = source.hideApplicationFloatie;
    }
  }
  [MessagePackObject]
  public class EffectDurationData {
    [Key(0)]
    public int duration = 0;
    [Key(1)]
    public bool ticksOnActivations = false;
    [Key(2)]
    public bool useActivationsOfTarget = false;
    [Key(3)]
    public bool ticksOnEndOfRound = false;
    [Key(4)]
    public bool ticksOnMovements = false;
    [Key(5)]
    public int stackLimit = 0;
    [Key(6)]
    public int uniqueEffectIdStackLimit = 0;
    [Key(7)]
    public bool clearedWhenAttacked = false;
    [Key(8)]
    public bool activeTrackedEffect = false;
    [Key(9)]
    public string stackId = string.Empty;
    public object toBT() {
      BattleTech.EffectDurationData result = new BattleTech.EffectDurationData();
      result.duration = this.duration;
      result.ticksOnActivations = this.ticksOnActivations;
      result.useActivationsOfTarget = this.useActivationsOfTarget;
      result.ticksOnEndOfRound = this.ticksOnEndOfRound;
      result.ticksOnMovements = this.ticksOnMovements;
      result.stackLimit = this.stackLimit;
      result.uniqueEffectIdStackLimit = this.uniqueEffectIdStackLimit;
      result.clearedWhenAttacked = this.clearedWhenAttacked;
      result.activeTrackedEffect = this.activeTrackedEffect;
      result.stackId(this.stackId);
      return result;
    }
    public EffectDurationData() { }
    public EffectDurationData(BattleTech.EffectDurationData source) {
      if (source == null) { return; }
      duration = source.duration;
      ticksOnActivations = source.ticksOnActivations;
      useActivationsOfTarget = source.useActivationsOfTarget;
      ticksOnEndOfRound = source.ticksOnEndOfRound;
      ticksOnMovements = source.ticksOnMovements;
      stackLimit = source.stackLimit;
      uniqueEffectIdStackLimit = source.uniqueEffectIdStackLimit;
      clearedWhenAttacked = source.clearedWhenAttacked;
      activeTrackedEffect = source.activeTrackedEffect;
      stackId = source.stackId();
    }
  }
  public static class EffectDurationDataHelper {
    private delegate string d_Field_get(BattleTech.EffectDurationData src);
    private delegate void d_Field_set(BattleTech.EffectDurationData src, string value);
    private static d_Field_get i_stackId_get = null;
    private static d_Field_set i_stackId_set = null;
    public static string stackId(this BattleTech.EffectDurationData src) {
      if (i_stackId_get == null) { return string.Empty; }
      return i_stackId_get(src);
    }
    public static void stackId(this BattleTech.EffectDurationData src, string value) {
      if (i_stackId_set == null) { return; }
      i_stackId_set(src, value);
    }
    public static void Prepare() {
      FieldInfo stackId = typeof(BattleTech.EffectDurationData).GetField("stackId", BindingFlags.Public | BindingFlags.Instance);
      Log.M_Err?.WL(1, $"EffectDurationData.stackId {(stackId == null ? "not found" : "found")}");
      if (stackId != null) {
        {
          var dm = new DynamicMethod("get_stackId", typeof(string), new Type[] { typeof(BattleTech.EffectDurationData) });
          var gen = dm.GetILGenerator();
          gen.Emit(OpCodes.Ldarg_0);
          gen.Emit(OpCodes.Ldfld, stackId);
          gen.Emit(OpCodes.Ret);
          i_stackId_get = (d_Field_get)dm.CreateDelegate(typeof(d_Field_get));
        }
        {
          var dm = new DynamicMethod("set_stackId", null, new Type[] { typeof(BattleTech.EffectDurationData), typeof(string) });
          var gen = dm.GetILGenerator();
          gen.Emit(OpCodes.Ldarg_0);
          gen.Emit(OpCodes.Ldarg_1);
          gen.Emit(OpCodes.Stfld, stackId);
          gen.Emit(OpCodes.Ret);
          i_stackId_set = (d_Field_set)dm.CreateDelegate(typeof(d_Field_set));
        }
      }
    }
  }
  public static class StatisticEffectDataHelper {
    private delegate string d_Field_get(BattleTech.StatisticEffectData src);
    private delegate void d_Field_set(BattleTech.StatisticEffectData src, string value);
    private static d_Field_get i_Location_get = null;
    private static d_Field_set i_Location_set = null;
    private static d_Field_get i_ShouldNotHaveTags_get = null;
    private static d_Field_set i_ShouldNotHaveTags_set = null;
    private static d_Field_get i_ShouldHaveTags_get = null;
    private static d_Field_set i_ShouldHaveTags_set = null;
    private static d_Field_get i_abilifierId_get = null;
    private static d_Field_set i_abilifierId_set = null;
    public static string abilifierId(this BattleTech.StatisticEffectData src) {
      if (i_abilifierId_get == null) { return string.Empty; }
      return i_abilifierId_get(src);
    }
    public static void abilifierId(this BattleTech.StatisticEffectData src, string value) {
      if (i_abilifierId_set == null) { return; }
      i_abilifierId_set(src, value);
    }
    public static string Location(this BattleTech.StatisticEffectData src) {
      if (i_Location_get == null) { return string.Empty; }
      return i_Location_get(src);
    }
    public static void Location(this BattleTech.StatisticEffectData src, string value) {
      if (i_Location_set == null) { return; }
      i_Location_set(src, value);
    }
    public static string ShouldNotHaveTags(this BattleTech.StatisticEffectData src) {
      if (i_ShouldNotHaveTags_get == null) { return string.Empty; }
      return i_ShouldNotHaveTags_get(src);
    }
    public static void ShouldNotHaveTags(this BattleTech.StatisticEffectData src, string value) {
      if (i_ShouldNotHaveTags_set == null) { return; }
      i_ShouldNotHaveTags_set(src,value);
    }
    public static string ShouldHaveTags(this BattleTech.StatisticEffectData src) {
      if (i_ShouldHaveTags_get == null) { return string.Empty; }
      return i_ShouldHaveTags_get(src);
    }
    public static void ShouldHaveTags(this BattleTech.StatisticEffectData src, string value) {
      if (i_ShouldHaveTags_set == null) { return; }
      i_ShouldHaveTags_set(src,value);
    }
    public static void Prepare() {
      FieldInfo Location = typeof(BattleTech.StatisticEffectData).GetField("Location", BindingFlags.Public | BindingFlags.Instance);
      Log.M_Err?.WL(1, $"StatisticEffectData.Location {(Location == null ? "not found" : "found")}");
      if (Location != null) {
        {
          var dm = new DynamicMethod("get_Location", typeof(string), new Type[] { typeof(BattleTech.StatisticEffectData) });
          var gen = dm.GetILGenerator();
          gen.Emit(OpCodes.Ldarg_0);
          gen.Emit(OpCodes.Ldfld, Location);
          gen.Emit(OpCodes.Ret);
          i_Location_get = (d_Field_get)dm.CreateDelegate(typeof(d_Field_get));
        }
        {
          var dm = new DynamicMethod("set_Location", null, new Type[] { typeof(BattleTech.StatisticEffectData), typeof(string) });
          var gen = dm.GetILGenerator();
          gen.Emit(OpCodes.Ldarg_0);
          gen.Emit(OpCodes.Ldarg_1);
          gen.Emit(OpCodes.Stfld, Location);
          gen.Emit(OpCodes.Ret);
          i_Location_set = (d_Field_set)dm.CreateDelegate(typeof(d_Field_set));
        }
      }
      FieldInfo ShouldHaveTags = typeof(BattleTech.StatisticEffectData).GetField("ShouldHaveTags", BindingFlags.Public | BindingFlags.Instance);
      Log.M_Err?.WL(1, $"StatisticEffectData.ShouldHaveTags {(ShouldHaveTags == null ? "not found" : "found")}");
      if (ShouldHaveTags != null) {
        {
          var dm = new DynamicMethod("get_ShouldHaveTags", typeof(string), new Type[] { typeof(BattleTech.StatisticEffectData) });
          var gen = dm.GetILGenerator();
          gen.Emit(OpCodes.Ldarg_0);
          gen.Emit(OpCodes.Ldfld, ShouldHaveTags);
          gen.Emit(OpCodes.Ret);
          i_ShouldHaveTags_get = (d_Field_get)dm.CreateDelegate(typeof(d_Field_get));
        }
        {
          var dm = new DynamicMethod("set_ShouldHaveTags", null, new Type[] { typeof(BattleTech.StatisticEffectData), typeof(string) });
          var gen = dm.GetILGenerator();
          gen.Emit(OpCodes.Ldarg_0);
          gen.Emit(OpCodes.Ldarg_1);
          gen.Emit(OpCodes.Stfld, ShouldHaveTags);
          gen.Emit(OpCodes.Ret);
          i_ShouldHaveTags_set = (d_Field_set)dm.CreateDelegate(typeof(d_Field_set));
        }
      }
      FieldInfo ShouldNotHaveTags = typeof(BattleTech.StatisticEffectData).GetField("ShouldNotHaveTags", BindingFlags.Public | BindingFlags.Instance);
      Log.M_Err?.WL(1, $"StatisticEffectData.ShouldNotHaveTags {(ShouldNotHaveTags == null ? "not found" : "found")}");
      if (ShouldNotHaveTags != null) {
        {
          var dm = new DynamicMethod("get_ShouldNotHaveTags", typeof(string), new Type[] { typeof(BattleTech.StatisticEffectData) });
          var gen = dm.GetILGenerator();
          gen.Emit(OpCodes.Ldarg_0);
          gen.Emit(OpCodes.Ldfld, ShouldNotHaveTags);
          gen.Emit(OpCodes.Ret);
          i_ShouldNotHaveTags_get = (d_Field_get)dm.CreateDelegate(typeof(d_Field_get));
        }
        {
          var dm = new DynamicMethod("set_ShouldNotHaveTags", null, new Type[] { typeof(BattleTech.StatisticEffectData), typeof(string) });
          var gen = dm.GetILGenerator();
          gen.Emit(OpCodes.Ldarg_0);
          gen.Emit(OpCodes.Ldarg_1);
          gen.Emit(OpCodes.Stfld, ShouldNotHaveTags);
          gen.Emit(OpCodes.Ret);
          i_ShouldNotHaveTags_set = (d_Field_set)dm.CreateDelegate(typeof(d_Field_set));
        }
      }
      FieldInfo abilifierId = typeof(BattleTech.StatisticEffectData).GetField("abilifierId", BindingFlags.Public | BindingFlags.Instance);
      Log.M_Err?.WL(1, $"StatisticEffectData.abilifierId {(abilifierId == null ? "not found" : "found")}");
      if (abilifierId != null) {
        {
          var dm = new DynamicMethod("get_abilifierId", typeof(string), new Type[] { typeof(BattleTech.StatisticEffectData) });
          var gen = dm.GetILGenerator();
          gen.Emit(OpCodes.Ldarg_0);
          gen.Emit(OpCodes.Ldfld, abilifierId);
          gen.Emit(OpCodes.Ret);
          i_abilifierId_get = (d_Field_get)dm.CreateDelegate(typeof(d_Field_get));
        }
        {
          var dm = new DynamicMethod("set_abilifierId", null, new Type[] { typeof(BattleTech.StatisticEffectData), typeof(string) });
          var gen = dm.GetILGenerator();
          gen.Emit(OpCodes.Ldarg_0);
          gen.Emit(OpCodes.Ldarg_1);
          gen.Emit(OpCodes.Stfld, abilifierId);
          gen.Emit(OpCodes.Ret);
          i_abilifierId_set = (d_Field_set)dm.CreateDelegate(typeof(d_Field_set));
        }
      }
    }
  }
  [MessagePackObject]
  public class StatisticEffectData {
    [Key(0)]
    public bool appliesEachTick = false;
    [Key(1)]
    public bool effectsPersistAfterDestruction = false;
    [Key(2)]
    public string statName = string.Empty;
    [Key(3)]
    public BattleTech.StatCollection.StatOperation operation = BattleTech.StatCollection.StatOperation.Set;
    [Key(4)]
    public string modValue = string.Empty;
    [Key(5)]
    public string modType = string.Empty;
    [Key(6)]
    public BattleTech.EffectType additionalRules = BattleTech.EffectType.NotSet;
    [Key(7)]
    public BattleTech.StatisticEffectData.TargetCollection targetCollection = BattleTech.StatisticEffectData.TargetCollection.NotSet;
    [Key(8)]
    public BattleTech.WeaponType targetWeaponType;
    [Key(9)]
    public string targetWeaponCategoryID = string.Empty;
    [Key(10)]
    public string targetAmmoCategoryID = string.Empty;
    [Key(11)]
    public BattleTech.WeaponSubType targetWeaponSubType = BattleTech.WeaponSubType.NotSet;
    [Key(12)]
    public string Location = string.Empty;
    [Key(13)]
    public string ShouldHaveTags = string.Empty;
    [Key(14)]
    public string ShouldNotHaveTags = string.Empty;
    [Key(15)]
    public string abilifierId = string.Empty;
    public StatisticEffectData() { }
    public object toBT() {
      BattleTech.StatisticEffectData result = new BattleTech.StatisticEffectData();
      result.appliesEachTick = this.appliesEachTick;
      result.effectsPersistAfterDestruction = this.effectsPersistAfterDestruction;
      result.statName = this.statName;
      result.operation = this.operation;
      result.modValue = this.modValue;
      result.modType = this.modType;
      result.additionalRules = this.additionalRules;
      result.targetCollection = this.targetCollection;
      result.targetWeaponType = this.targetWeaponType;
      result.targetWeaponSubType = this.targetWeaponSubType;
      result.targetWeaponCategoryID = this.targetWeaponCategoryID;
      result.targetAmmoCategoryID = this.targetAmmoCategoryID;
      result.Location(this.Location);
      result.ShouldHaveTags(this.ShouldHaveTags);
      result.ShouldNotHaveTags(this.ShouldNotHaveTags);
      result.abilifierId(this.abilifierId);
      return result;
    }
    public StatisticEffectData(BattleTech.StatisticEffectData source) {
      if (source == null) { return; }
      appliesEachTick = source.appliesEachTick;
      effectsPersistAfterDestruction = source.effectsPersistAfterDestruction;
      statName = source.statName;
      operation = source.operation;
      modValue = source.modValue;
      modType = source.modType;
      additionalRules = source.additionalRules;
      targetCollection = source.targetCollection;
      targetWeaponType = source.targetWeaponType;
      targetWeaponSubType = source.targetWeaponSubType;
      targetWeaponCategoryID = source.TargetWeaponCategoryValue.Name;
      targetAmmoCategoryID = source.TargetAmmoCategoryValue.Name;
      Location = source.Location();
      ShouldNotHaveTags = source.ShouldNotHaveTags();
      ShouldHaveTags = source.ShouldHaveTags();
      abilifierId = source.abilifierId();
    }
  }
  [MessagePackObject]
  public class TagEffectData {
    [Key(0)]
    public List<string> tagList = new List<string>();
    public TagEffectData() { tagList = new List<string>(); }
    public object toBT() {
      return new BattleTech.TagEffectData() { tagList = this.tagList.ToArray() };
    }
    public TagEffectData(BattleTech.TagEffectData source) {
      tagList = new List<string>();
      if(source != null) { if (source.tagList != null) { tagList.AddRange(source.tagList); } }
    }
  }
  [MessagePackObject]
  public class FloatieEffectData {
    [Key(0)]
    public BattleTech.StatisticEffectData.TargetCollection targetCollection = BattleTech.StatisticEffectData.TargetCollection.NotSet;
    [Key(1)]
    public BattleTech.WeaponType targetWeaponType = BattleTech.WeaponType.NotSet;
    [Key(2)]
    public string targetWeaponCategory = string.Empty;
    [Key(3)]
    public string targetAmmoCategory = string.Empty;
    [Key(4)]
    public string targetWeaponCategoryID = string.Empty;
    [Key(5)]
    public string targetAmmoCategoryID = string.Empty;
    [Key(6)]
    public BattleTech.WeaponSubType targetWeaponSubType = BattleTech.WeaponSubType.NotSet;
    public FloatieEffectData() { }
    public object toBT() {
      BattleTech.FloatieEffectData result = new BattleTech.FloatieEffectData() {
        targetCollection = this.targetCollection,
        targetWeaponType = this.targetWeaponType,
        targetWeaponSubType = this.targetWeaponSubType,
      };
      if (string.IsNullOrEmpty(targetWeaponCategory) == false) {
        Traverse.Create(result).Field<string>("targetWeaponCategoryID").Value = this.targetWeaponCategory;
      }
      if (string.IsNullOrEmpty(targetAmmoCategory) == false) {
        Traverse.Create(result).Field<string>("targetAmmoCategoryID").Value = this.targetAmmoCategory;
      }
      if (string.IsNullOrEmpty(targetWeaponCategoryID) == false) {
        Traverse.Create(result).Field<string>("targetWeaponCategoryID").Value = this.targetWeaponCategoryID;
      }
      if (string.IsNullOrEmpty(targetWeaponCategoryID) == false) {
        Traverse.Create(result).Field<string>("targetAmmoCategoryID").Value = this.targetAmmoCategoryID;
      }
      return result;
    }
    public FloatieEffectData(BattleTech.FloatieEffectData source) {
      if (source == null) { return; }
      targetCollection = source.targetCollection;
      targetWeaponType = source.targetWeaponType;
      targetWeaponSubType = source.targetWeaponSubType;
      targetWeaponCategoryID = source.TargetWeaponCategoryValue.Name;
      targetAmmoCategoryID = source.TargetAmmoCategoryValue.Name;
    }
  }
  [MessagePackObject]
  public class ActorBurningEffectData {
    [Key(0)]
    public int damagePerTick = 0;
    [Key(1)]
    public int heatPerTick = 0;
    public ActorBurningEffectData() { }
    public object toBT() {
      return new BattleTech.ActorBurningEffectData() { damagePerTick = this.damagePerTick, heatPerTick = this.heatPerTick };
    }
    public ActorBurningEffectData(BattleTech.ActorBurningEffectData source) {
      damagePerTick = source.damagePerTick;
      heatPerTick = source.damagePerTick;
    }
  }
  [MessagePackObject]
  public class VFXEffectData {
    [Key(0)]
    public string vfxName = string.Empty;
    [Key(1)]
    public bool attachToImpactPoint = false;
    [Key(2)]
    public int location = 0;
    [Key(3)]
    public bool isAttached = false;
    [Key(4)]
    public bool facesAttacker = false;
    [Key(5)]
    public bool isOneShot = false;
    [Key(6)]
    public float duration = 0f;
    public VFXEffectData() { }
    public object toBT() {
      return new BattleTech.VFXEffectData() {
        vfxName = this.vfxName,
        attachToImpactPoint = this.attachToImpactPoint,
        location = this.location,
        isAttached = this.isAttached,
        facesAttacker = this.facesAttacker,
        isOneShot = this.isOneShot,
        duration = this.duration
      };
    }
    public VFXEffectData(BattleTech.VFXEffectData source) {
      if (source == null) { return; }
      vfxName = source.vfxName;
      attachToImpactPoint = source.attachToImpactPoint;
      location = source.location;
      isAttached = source.isAttached;
      facesAttacker = source.facesAttacker;
      isOneShot = source.isOneShot;
      duration = source.duration;
    }
  }
  [MessagePackObject]
  public class InstantModEffectData {
    [Key(0)]
    public BattleTech.InstantModEffectData.StatToMod targetStat = BattleTech.InstantModEffectData.StatToMod.NotSet;
    [Key(1)]
    public int intMod = 0;
    [Key(2)]
    public float floatMod = 0f;
    public InstantModEffectData() { }
    public object toBT() {
      return new BattleTech.InstantModEffectData() {
        targetStat = this.targetStat,
        intMod = this.intMod,
        floatMod = this.floatMod
      };
    }
    public InstantModEffectData(BattleTech.InstantModEffectData source) {
      targetStat = source.targetStat;
      intMod = source.intMod;
      floatMod = source.floatMod;
    }
  }
  [MessagePackObject]
  public class PoorlyMaintainedEffectData  {
    [Key(0)]
    public float armorMod = 1f;
    [Key(1)]
    public float ammoMod = 1f;
    public PoorlyMaintainedEffectData() { }
    public object toBT() {
      return new BattleTech.PoorlyMaintainedEffectData() {
        armorMod = this.armorMod,
        ammoMod = this.ammoMod
      };
    }
    public PoorlyMaintainedEffectData(BattleTech.PoorlyMaintainedEffectData source) {
      armorMod = source.armorMod;
      ammoMod = source.ammoMod;
    }
  }
  [MessagePackObject]
  public class ActiveAbilityEffectData {
    [Key(0)]
    public string abilityName = string.Empty;
    public ActiveAbilityEffectData() { }
    public object toBT() {
      return new BattleTech.ActiveAbilityEffectData() {
        abilityName = this.abilityName
      };
    }
    public ActiveAbilityEffectData(BattleTech.ActiveAbilityEffectData source) {
      abilityName = source.abilityName;
    }
  }
  [MessagePackObject]
  public class EffectData {
    [Key(0)]
    public CustomPrewarm.Serialize.EffectDurationData durationData = null;
    [Key(1)]
    public EffectTargetingData targetingData = new EffectTargetingData();
    [Key(2)]
    public BattleTech.EffectType effectType = BattleTech.EffectType.NotSet;
    [Key(3)]
    public CustomPrewarm.Serialize.BaseDescriptionDef Description = new BaseDescriptionDef();
    [Key(4)]
    public BattleTech.EffectNature nature = BattleTech.EffectNature.NotSet;
    [Key(5)]
    public StatisticEffectData statisticData = null;
    [Key(6)]
    public TagEffectData tagData = null;
    [Key(7)]
    public FloatieEffectData floatieData = null;
    [Key(8)]
    public ActorBurningEffectData actorBurningData = null;
    [Key(9)]
    public VFXEffectData vfxData = null;
    [Key(10)]
    public InstantModEffectData instantModData = null;
    [Key(11)]
    public PoorlyMaintainedEffectData poorlyMaintainedEffectData = null;
    [Key(12)]
    public ActiveAbilityEffectData activeAbilityEffectData = null;
    public BattleTech.EffectData toBT() {
      return new BattleTech.EffectData() {
        durationData = this.durationData==null? null: this.durationData.toBT() as BattleTech.EffectDurationData,
        targetingData = this.targetingData.toBT(),
        effectType = this.effectType,
        Description = this.Description == null ? null : this.Description.toBT(null) as BattleTech.BaseDescriptionDef,
        nature = this.nature,
        statisticData = this.statisticData == null ? null : this.statisticData.toBT() as BattleTech.StatisticEffectData,
        tagData = this.tagData == null ? null : this.tagData.toBT() as BattleTech.TagEffectData,
        floatieData = this.floatieData == null ? null : this.floatieData.toBT() as BattleTech.FloatieEffectData,
        actorBurningData = this.actorBurningData == null ? null : this.actorBurningData.toBT() as BattleTech.ActorBurningEffectData,
        vfxData = this.vfxData == null ? null : this.vfxData.toBT() as BattleTech.VFXEffectData,
        instantModData = this.instantModData == null ? null : this.instantModData.toBT() as BattleTech.InstantModEffectData,
        poorlyMaintainedEffectData = this.poorlyMaintainedEffectData == null ? null : this.poorlyMaintainedEffectData.toBT() as BattleTech.PoorlyMaintainedEffectData,
        activeAbilityEffectData = this.activeAbilityEffectData == null ? null : this.activeAbilityEffectData.toBT() as BattleTech.ActiveAbilityEffectData
      };
    }
    public EffectData() { }
    public EffectData(BattleTech.EffectData source) {
      durationData = source.durationData == null? null : new EffectDurationData(source.durationData);
      targetingData = new EffectTargetingData(source.targetingData);
      effectType = source.effectType;
      Description = new CustomPrewarm.Serialize.BaseDescriptionDef(source.Description);
      nature = source.nature;
      statisticData = source.statisticData == null ? null : new StatisticEffectData(source.statisticData);
      tagData = source.tagData == null ? null : new TagEffectData(source.tagData);
      floatieData = source.floatieData == null ? null : new FloatieEffectData(source.floatieData);
      actorBurningData = source.actorBurningData == null ? null : new ActorBurningEffectData(source.actorBurningData);
      vfxData = source.vfxData == null ? null : new VFXEffectData(source.vfxData);
      instantModData = source.instantModData == null ? null : new InstantModEffectData(source.instantModData);
      poorlyMaintainedEffectData = source.poorlyMaintainedEffectData == null ? null : new PoorlyMaintainedEffectData(source.poorlyMaintainedEffectData);
      activeAbilityEffectData = source.activeAbilityEffectData == null ? null : new ActiveAbilityEffectData(source.activeAbilityEffectData);
    }
  }
}