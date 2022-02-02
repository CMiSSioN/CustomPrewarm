using Harmony;
using MessagePack;
using System.Collections.Generic;

namespace CustomPrewarm.Serialize {
  [MessagePackObject]
  public class TagSet {
    [Key(0)]
    public List<string> items = new List<string>();
    [Key(1)]
    public string tagSetSourceFile = string.Empty;
    public TagSet() {}
    public TagSet(HBS.Collections.TagSet tagSet) {
      this.items = new List<string>();
      this.items.AddRange(Traverse.Create(tagSet).Field<string[]>("items").Value);
      this.tagSetSourceFile = Traverse.Create(tagSet).Field<string>("tagSetSourceFile").Value;
    }
    public HBS.Collections.TagSet toBT() {
      return string.IsNullOrEmpty(this.tagSetSourceFile) ? new HBS.Collections.TagSet(this.items) : new HBS.Collections.TagSet(this.tagSetSourceFile);
    }
  }
  [MessagePackObject]
  public class SerializableVariant {
    [Key(0)]
    public string statName = string.Empty;
    [Key(1)]
    public string statValue = string.Empty;
    [Key(2)]
    public string statType = string.Empty;
    public SerializableVariant() {
    }
    public SerializableVariant(BattleTech.SerializableVariant source) {
      this.statName = source.statName;
      this.statValue = source.statValue;
      this.statType = source.statType;
    }
    public BattleTech.SerializableVariant toBT() {
      return new BattleTech.SerializableVariant(this.statName,this.statValue,this.statType);
    }
  }
  [MessagePackObject]
  public class DescriptionDef : BaseDescriptionDef {
    [Key(4)]
    public int Cost { get; set; } = 0;
    [Key(5)]
    public float Rarity { get; set; } = 0f;
    [Key(6)]
    public bool Purchasable { get; set; } = false;
    [Key(7)]
    public string Manufacturer { get; set; } = string.Empty;
    [Key(8)]
    public string Model { get; set; } = string.Empty;
    [Key(9)]
    public string UIName { get; set; } = string.Empty;
    public DescriptionDef():base() {
    }
    public DescriptionDef(BattleTech.DescriptionDef def): base((BattleTech.BaseDescriptionDef)def) {
      this.Cost = def.Cost;
      this.Rarity = def.Rarity;
      this.Purchasable = def.Purchasable;
      this.Manufacturer = def.Manufacturer;
      this.Model = def.Model;
      this.UIName = def.UIName;
    }
    public override object toBT(BattleTech.Data.DataManager dataManager) {
      return new BattleTech.DescriptionDef(this.Id,this.Name,this.Details,this.Icon,this.Cost,this.Rarity,this.Purchasable,this.Manufacturer,this.Model,this.UIName);
    }
  }
  [MessagePackObject]
  public class MechComponentDef {
    [Key(0)]
    public DescriptionDef Description { get; set; } = new DescriptionDef();
    [Key(1)]
    public string BonusValueA { get; set; } = string.Empty;
    [Key(2)]
    public string BonusValueB { get; set; } = string.Empty;
    [Key(3)]
    public BattleTech.ComponentType ComponentType { get; set; } = BattleTech.ComponentType.NotSet;
    [Key(4)]
    public BattleTech.MechComponentType ComponentSubType { get; set; } = BattleTech.MechComponentType.NotSet;
    [Key(5)]
    public string PrefabIdentifier { get; set; } = string.Empty;
    [Key(6)]
    public int BattleValue { get; set; } = 0;
    [Key(7)]
    public int InventorySize { get; set; } = 0;
    [Key(8)]
    public float Tonnage { get; set; } = 0f;
    [Key(9)]
    public BattleTech.ChassisLocations AllowedLocations { get; set; } = BattleTech.ChassisLocations.All;
    [Key(10)]
    public BattleTech.ChassisLocations DisallowedLocations { get; set; } = BattleTech.ChassisLocations.None;
    [Key(11)]
    public bool CriticalComponent { get; set; } = false;
    [Key(12)]
    public bool CanExplode { get; set; } = false;
    [Key(13)]
    public List<EffectData> statusEffects { get; set; } = new List<EffectData>();
    [Key(14)]
    public List<SerializableVariant> additionalData { get; set; } = new List<SerializableVariant>();
    [Key(15)]
    public TagSet ComponentTags { get; set; } = new TagSet();
    public MechComponentDef() { }
    public MechComponentDef(BattleTech.MechComponentDef source) {
      this.Description = new DescriptionDef(source.Description);
      BonusValueA = source.BonusValueA;
      BonusValueB = source.BonusValueB;
      ComponentType = source.ComponentType;
      ComponentSubType = source.ComponentSubType;
      PrefabIdentifier = source.PrefabIdentifier;
      InventorySize = source.InventorySize;
      Tonnage = source.Tonnage;
      AllowedLocations = source.AllowedLocations;
      DisallowedLocations = source.DisallowedLocations;
      CriticalComponent = source.CriticalComponent;
      CanExplode = source.CanExplode;
      statusEffects = new List<EffectData>();
      if (source.statusEffects != null) {
        foreach (BattleTech.EffectData effect in source.statusEffects) { statusEffects.Add(new EffectData(effect)); }
      }
      additionalData = new List<SerializableVariant>();
      if (source.additionalData != null) {
        foreach (BattleTech.SerializableVariant variant in source.additionalData) { additionalData.Add(new SerializableVariant(variant)); }
      }
      ComponentTags = new TagSet(source.ComponentTags);
    }
    public virtual void fill(BattleTech.MechComponentDef def, BattleTech.Data.DataManager dataManager) {
      def.dataManager = dataManager;
      def.Description = this.Description.toBT(dataManager) as BattleTech.DescriptionDef;
      def.BonusValueA = this.BonusValueA;
      def.BonusValueB = this.BonusValueB;
      def.ComponentType = this.ComponentType;
      def.ComponentSubType = this.ComponentSubType;
      def.PrefabIdentifier = this.PrefabIdentifier;
      def.InventorySize = this.InventorySize;
      def.Tonnage = this.Tonnage;
      def.AllowedLocations = this.AllowedLocations;
      def.DisallowedLocations = this.DisallowedLocations;
      def.CriticalComponent = this.CriticalComponent;
      def.CanExplode = this.CanExplode;
      List<BattleTech.EffectData> statusEffects = new List<BattleTech.EffectData>();
      foreach (EffectData effect in this.statusEffects) { statusEffects.Add(effect.toBT() as BattleTech.EffectData); }
      def.statusEffects = statusEffects.ToArray();
      List<BattleTech.SerializableVariant> additionalData = new List<BattleTech.SerializableVariant>();
      foreach (SerializableVariant variant in this.additionalData) { additionalData.Add(variant.toBT() as BattleTech.SerializableVariant); }
      def.additionalData = additionalData.ToArray();
      def.ComponentTags = this.ComponentTags.toBT() as HBS.Collections.TagSet;
    }
    //public virtual object toBT(BattleTech.Data.DataManager dataManager) {
    //  BattleTech.MechComponentDef result = new BattleTech.MechComponentDef();
    //  this.fill(result, dataManager);
    //  return result;
    //}
  }
}