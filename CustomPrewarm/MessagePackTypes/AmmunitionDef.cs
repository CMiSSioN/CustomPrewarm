using HarmonyLib;
using MessagePack;

namespace CustomPrewarm.Serialize {
  [HarmonyPatch(typeof(BattleTech.AmmunitionDef), "FromJSON")]
  [HarmonyPriority(Priority.Last)]
  public static class AmmunitionDef_FromJSON {
    public static bool Prefix(BattleTech.AmmunitionDef __instance) {
      return Core.disableFromJSON == false;
    }
  }
  [MessagePackObject]
  public class AmmunitionDef: ISerializable {
    [Key(0)]
    public string ammunitionTypeID = string.Empty;
    [Key(1)]
    public string ammoCategoryID = string.Empty;
    [Key(2)]
    public DescriptionDef Description { get; set; } = new DescriptionDef();
    [Key(3)]
    public int HeatGenerated { get; set; } = 0;
    [Key(4)]
    public float HeatGeneratedModifier { get; set; } = 0f;
    [Key(5)]
    public int BaseDamage { get; set; } = 0;
    [Key(6)]
    public float ArmorDamageModifier { get; set; } = 0f;
    [Key(7)]
    public float ISDamageModifier { get; set; } = 0f;
    [Key(8)]
    public float CriticalDamageModifier { get; set; } = 0f;
    public AmmunitionDef() { }
    public AmmunitionDef(BattleTech.AmmunitionDef src) {
      this.ammoCategoryID = src.AmmoCategoryValue.Name;
      this.ammunitionTypeID = src.AmmunitionTypeValue.Name;
      Description = new DescriptionDef(src.Description);
      HeatGenerated = src.HeatGenerated;
      HeatGeneratedModifier = src.HeatGeneratedModifier;
      BaseDamage = src.BaseDamage;
      ArmorDamageModifier = src.ArmorDamageModifier;
      ISDamageModifier = src.ISDamageModifier;
      CriticalDamageModifier = src.CriticalDamageModifier;
    }
    public object toBT(BattleTech.Data.DataManager dataManager) {
      BattleTech.AmmunitionDef result = new BattleTech.AmmunitionDef();
      result.ammoCategoryID = this.ammoCategoryID;
      result.ammunitionTypeID = this.ammunitionTypeID;
      result.Description = this.Description.toBT(dataManager) as BattleTech.DescriptionDef;
      result.HeatGenerated = this.HeatGenerated;
      result.HeatGeneratedModifier = this.HeatGeneratedModifier;
      result.BaseDamage = this.BaseDamage;
      result.ArmorDamageModifier = this.ArmorDamageModifier;
      result.ISDamageModifier = this.ISDamageModifier;
      result.CriticalDamageModifier = this.CriticalDamageModifier;
      return result;
    }
  }
}