using Harmony;
using MessagePack;
using System.Collections.Generic;

namespace CustomPrewarm.Serialize {
  [HarmonyPatch(typeof(BattleTech.AbilityDef), "FromJSON")]
  [HarmonyPriority(Priority.Last)]
  public static class AbilityDef_FromJSON {
    public static bool Prefix(BattleTech.AbilityDef __instance) {
      return Core.disableFromJSON == false;
    }
  }
  [MessagePackObject]
  public class AbilityDef: ISerializable {
    [Key(0)]
    public CustomPrewarm.Serialize.BaseDescriptionDef Description { get; set; } = new BaseDescriptionDef();
    [Key(1)]
    public string Type { get; set; } = string.Empty;
    [Key(2)]
    public string ShortDesc { get; set; } = string.Empty;
    [Key(3)]
    public BattleTech.AbilityDef.DisplayParameters DisplayParams { get; set; } = BattleTech.AbilityDef.DisplayParameters.NotSet;
    [Key(4)]
    public BattleTech.SkillType ReqSkill { get; set; } = BattleTech.SkillType.NotSet;
    [Key(5)]
    public int ReqSkillLevel { get; set; } = 0;
    [Key(6)]
    public BattleTech.AbilityDef.ActivationTiming ActivationTime { get; set; } = BattleTech.AbilityDef.ActivationTiming.NotSet;
    [Key(7)]
    public int ActivationCooldown { get; set; } = 0;
    [Key(8)]
    public int DurationActivations { get; set; } = 0;
    [Key(9)]
    public int ActivationETA { get; set; } = 0;
    [Key(10)]
    public int NumberOfUses { get; set; } = 0;
    [Key(11)]
    public BattleTech.AbilityDef.ResourceConsumed Resource { get; set; } = BattleTech.AbilityDef.ResourceConsumed.NotSet;
    [Key(12)]
    public List<CustomPrewarm.Serialize.EffectData> EffectData { get; set; } = new List<EffectData>();
    [Key(13)]
    public BattleTech.AbilityDef.SpecialRules specialRules { get; set; }
    [Key(14)]
    public BattleTech.AbilityDef.TargetingType Targeting { get; set; }
    [Key(15)]
    public int IntParam1 { get; set; } = 0;
    [Key(16)]
    public bool ShowIntParam1 { get; set; } = true;
    [Key(17)]
    public int IntParam2 { get; set; } = 0;
    [Key(18)]
    public bool ShowIntParam2 { get; set; } = true;
    [Key(19)]
    public float FloatParam1 { get; set; } = 0f;
    [Key(20)]
    public float FloatParam2 { get; set; } = 0f;
    [Key(21)]
    public string StringParam1 { get; set; } = string.Empty;
    [Key(22)]
    public string StringParam2 { get; set; } = string.Empty;
    [Key(23)]
    public string ActorResource { get; set; } = string.Empty;
    [Key(24)]
    public int ActorWeaponIndex { get; set; } = 0;
    [Key(25)]
    public string WeaponResource { get; set; } = string.Empty;
    [Key(26)]
    public bool IsPrimaryAbility { get; set; } = false;
    public List<BattleTech.EffectData> BT_EffectData() {
      List<BattleTech.EffectData> result = new List<BattleTech.EffectData>();
      foreach(EffectData e in this.EffectData) { result.Add(e.toBT()); }
      return result;
    }
    public AbilityDef() { }
    public AbilityDef(BattleTech.AbilityDef source) {
      this.Description = new BaseDescriptionDef(source.Description);
      ShortDesc = source.ShortDesc;
      DisplayParams = source.DisplayParams;
      ReqSkill = source.ReqSkill;
      ReqSkillLevel = source.ReqSkillLevel;
      ActivationTime = source.ActivationTime;
      ActivationCooldown = source.ActivationCooldown;
      ActivationETA = source.ActivationETA;
      NumberOfUses = source.NumberOfUses;
      Resource = source.Resource;
      EffectData = new List<EffectData>();
      foreach(BattleTech.EffectData effect in source.EffectData) { EffectData.Add(new EffectData(effect)); }
      specialRules = source.specialRules;
      IntParam1 = source.IntParam1;
      ShowIntParam1 = source.ShowIntParam1;
      IntParam2 = source.IntParam2;
      ShowIntParam2 = source.ShowIntParam2;
      FloatParam1 = source.FloatParam1;
      FloatParam2 = source.FloatParam2;
      StringParam1 = source.StringParam1;
      StringParam2 = source.StringParam2;
      ActorResource = source.ActorResource;
      ActorWeaponIndex = source.ActorWeaponIndex;
      WeaponResource = source.WeaponResource;
      IsPrimaryAbility = source.IsPrimaryAbility;
    }
    public object toBT(BattleTech.Data.DataManager dataManager) {
      BattleTech.AbilityDef result = new BattleTech.AbilityDef(this.Description.toBT(dataManager) as BattleTech.BaseDescriptionDef
        , this.DisplayParams, this.BT_EffectData(),this.ReqSkill, this.ReqSkillLevel, this.ActivationTime
        , this.ActivationCooldown, this.ActivationETA, this.NumberOfUses
        , this.specialRules, this.Targeting, this.IntParam1, this.IntParam2, this.FloatParam1, this.FloatParam2, this.StringParam1
        , this.StringParam2, this.ShowIntParam1, this.ShowIntParam2, this.ActorResource, this.ActorWeaponIndex, this.WeaponResource);
      result.Type = this.Type;
      result.ShortDesc = this.ShortDesc;
      result.IsPrimaryAbility = this.IsPrimaryAbility;
      result.dataManager = dataManager;
      return result;
    }
  }
}