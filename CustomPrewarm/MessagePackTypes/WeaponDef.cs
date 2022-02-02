using System.Collections.Generic;
using BattleTech.Data;
using Harmony;
using MessagePack;

namespace CustomPrewarm.Serialize {
  [HarmonyPatch(typeof(BattleTech.WeaponDef), "FromJSON")]
  [HarmonyPriority(Priority.Last)]
  public static class WeaponDef_FromJSON {
    public static bool Prefix(BattleTech.WeaponDef __instance) {
      return Core.disableFromJSON == false;
    }
  }
  [MessagePackObject]
  public class WeaponDef : MechComponentDef, ISerializable {
    [Key(16)]
    public string weaponCategoryID = string.Empty;
    [Key(17)]
    public string ammoCategoryID = string.Empty;
    [Key(18)]
    public BattleTech.WeaponType Type { get; set; } = BattleTech.WeaponType.NotSet;
    [Key(19)]
    public BattleTech.WeaponSubType WeaponSubType { get; set; } = BattleTech.WeaponSubType.NotSet;
    [Key(20)]
    public float MinRange { get; set; } = 0f;
    [Key(21)]
    public float MaxRange { get; set; } = 0f;
    [Key(22)]
    public List<float> RangeSplit { get; set; } = new List<float>();
    [Key(23)]
    public int StartingAmmoCapacity { get; set; } = 0;
    [Key(24)]
    public int HeatGenerated { get; set; } = 0;
    [Key(25)]
    public float Damage { get; set; } = 0;
    [Key(26)]
    public float OverheatedDamageMultiplier { get; set; } = 1f;
    [Key(27)]
    public float EvasiveDamageMultiplier { get; set; } = 1f;
    [Key(28)]
    public float EvasivePipsIgnored { get; set; } = 0f;
    [Key(29)]
    public int DamageVariance { get; set; } = 0;
    [Key(30)]
    public float HeatDamage { get; set; } = 0f;
    [Key(31)]
    public float StructureDamage { get; set; } = 0f;
    [Key(32)]
    public float AccuracyModifier { get; set; } = 0f;
    [Key(33)]
    public float CriticalChanceMultiplier { get; set; } = 0f;
    [Key(34)]
    public bool AOECapable { get; set; } = false;
    [Key(35)]
    public bool IndirectFireCapable { get; set; } = false;
    [Key(36)]
    public int RefireModifier { get; set; } = 0;
    [Key(37)]
    public int ShotsWhenFired { get; set; } = 0;
    [Key(38)]
    public int ProjectilesPerShot { get; set; } = 0;
    [Key(39)]
    public int VolleyDivisor { get; set; } = 0;
    [Key(40)]
    public int AttackRecoil { get; set; } = 0;
    [Key(41)]
    public float Instability { get; set; } = 0f;
    [Key(42)]
    public float ClusteringModifier { get; set; } = 0f;
    [Key(43)]
    public string WeaponEffectID { get; set; } = string.Empty;
    public WeaponDef(): base() {}
    public WeaponDef(BattleTech.WeaponDef src) : base(src) {
      this.weaponCategoryID = src.WeaponCategoryValue.Name;
      this.ammoCategoryID = src.AmmoCategoryValue.Name;
      this.Type = src.Type;
      this.WeaponSubType = src.WeaponSubType;
      this.MinRange = src.MinRange;
      this.MaxRange = src.MaxRange;
      this.RangeSplit = src.RangeSplit == null? new List<float>() { 1f, 1f, 1f } : new List<float>(src.RangeSplit);
      this.StartingAmmoCapacity = src.StartingAmmoCapacity;
      this.HeatGenerated = src.HeatGenerated;
      this.Damage = src.Damage;
      this.OverheatedDamageMultiplier = src.OverheatedDamageMultiplier;
      this.DamageVariance = src.DamageVariance;
      this.HeatDamage = src.HeatDamage;
      this.StructureDamage = src.StructureDamage;
      this.AccuracyModifier = src.AccuracyModifier;
      this.CriticalChanceMultiplier = src.CriticalChanceMultiplier;
      this.AOECapable = src.AOECapable;
      this.IndirectFireCapable = src.IndirectFireCapable;
      this.RefireModifier = src.RefireModifier;
      this.ShotsWhenFired = src.ShotsWhenFired;
      this.ProjectilesPerShot = src.ProjectilesPerShot;
      this.VolleyDivisor = src.VolleyDivisor;
      this.AttackRecoil = src.AttackRecoil;
      this.Instability = src.Instability;
      this.ClusteringModifier = src.ClusteringModifier;
      this.WeaponEffectID = src.WeaponEffectID;
    }
    public void fill(BattleTech.WeaponDef def, BattleTech.Data.DataManager dataManager) {
      base.fill(def, dataManager);
      def.weaponCategoryID = this.weaponCategoryID;
      def.ammoCategoryID = this.ammoCategoryID;
      def.Type = this.Type;
      def.WeaponSubType = this.WeaponSubType;
      def.MinRange = this.MinRange;
      def.MaxRange = this.MaxRange;
      def.RangeSplit = this.RangeSplit.ToArray();
      def.StartingAmmoCapacity = this.StartingAmmoCapacity;
      def.HeatGenerated = this.HeatGenerated;
      def.Damage = this.Damage;
      def.OverheatedDamageMultiplier = this.OverheatedDamageMultiplier;
      def.DamageVariance = this.DamageVariance;
      def.HeatDamage = this.HeatDamage;
      def.StructureDamage = this.StructureDamage;
      def.AccuracyModifier = this.AccuracyModifier;
      def.CriticalChanceMultiplier = this.CriticalChanceMultiplier;
      def.AOECapable = this.AOECapable;
      def.IndirectFireCapable = this.IndirectFireCapable;
      def.RefireModifier = this.RefireModifier;
      def.ShotsWhenFired = this.ShotsWhenFired;
      def.ProjectilesPerShot = this.ProjectilesPerShot;
      def.VolleyDivisor = this.VolleyDivisor;
      def.AttackRecoil = this.AttackRecoil;
      def.Instability = this.Instability;
      def.ClusteringModifier = this.ClusteringModifier;
      def.WeaponEffectID = this.WeaponEffectID;
    }
    public object toBT(DataManager dataManager) {
      BattleTech.WeaponDef res = new BattleTech.WeaponDef();
      this.fill(res,dataManager);
      return res;
    }
  }
}