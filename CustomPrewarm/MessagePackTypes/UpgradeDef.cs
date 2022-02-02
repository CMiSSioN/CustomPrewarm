using Harmony;
using MessagePack;

namespace CustomPrewarm.Serialize {
  [HarmonyPatch(typeof(BattleTech.HeatSinkDef), "FromJSON")]
  [HarmonyPriority(Priority.Last)]
  public static class HeatSinkDef_FromJSON {
    public static bool Prefix(BattleTech.HeatSinkDef __instance) {
      return Core.disableFromJSON == false;
    }
  }
  [HarmonyPatch(typeof(BattleTech.JumpJetDef), "FromJSON")]
  [HarmonyPriority(Priority.Last)]
  public static class JumpJetDef_FromJSON {
    public static bool Prefix(BattleTech.JumpJetDef __instance) {
      return Core.disableFromJSON == false;
    }
  }
  [HarmonyPatch(typeof(BattleTech.UpgradeDef), "FromJSON")]
  [HarmonyPriority(Priority.Last)]
  public static class UpgradeDef_FromJSON {
    public static bool Prefix(BattleTech.UpgradeDef __instance) {
      return Core.disableFromJSON == false;
    }
  }
  [MessagePackObject]
  public class HeatSinkDef : MechComponentDef, ISerializable {
    [Key(16)]
    public float DissipationCapacity { get; set; } = 0f;
    public HeatSinkDef() {
    }
    public HeatSinkDef(BattleTech.HeatSinkDef src) : base(src) {
      this.DissipationCapacity = src.DissipationCapacity;
    }
    public void fill(BattleTech.HeatSinkDef trg, BattleTech.Data.DataManager dataManager) {
      base.fill(trg, dataManager);
      trg.DissipationCapacity = this.DissipationCapacity;
    }
    public object toBT(BattleTech.Data.DataManager dataManager) {
      BattleTech.HeatSinkDef res = new BattleTech.HeatSinkDef();
      this.fill(res, dataManager);
      return res;
    }
  }
  [MessagePackObject]
  public class JumpJetDef : MechComponentDef, ISerializable {
    [Key(16)]
    public float JumpCapacity { get; set; } = 0f;
    [Key(17)]
    public float MinTonnage { get; set; } = 0f;
    [Key(18)]
    public float MaxTonnage { get; set; } = 0f;
    public JumpJetDef() {}
    public JumpJetDef(BattleTech.JumpJetDef src): base(src) { 
      this.JumpCapacity = src.JumpCapacity;
      this.MinTonnage = src.MinTonnage;
      this.MaxTonnage = src.MaxTonnage;
    }
    public void fill(BattleTech.JumpJetDef trg, BattleTech.Data.DataManager dataManager) {
      base.fill(trg, dataManager);
      trg.JumpCapacity = this.JumpCapacity;
      trg.MinTonnage = this.MinTonnage;
      trg.MaxTonnage = this.MaxTonnage;
    }
    public object toBT(BattleTech.Data.DataManager dataManager) {
      BattleTech.JumpJetDef res = new BattleTech.JumpJetDef();
      this.fill(res, dataManager);
      return res;
    }
  }

  [MessagePackObject]
  public class UpgradeDef : MechComponentDef, ISerializable {
    [Key(16)]
    public string StatName { get; set; } = string.Empty;
    [Key(17)]
    public float RelativeModifier { get; set; } = 0f;
    [Key(18)]
    public float AbsoluteModifier { get; set; } = 0f;
    public UpgradeDef() {}
    public UpgradeDef(BattleTech.UpgradeDef src) : base(src) {
      this.StatName = src.StatName;
      this.RelativeModifier = src.RelativeModifier;
      this.AbsoluteModifier = src.AbsoluteModifier;
    }
    public void fill(BattleTech.UpgradeDef trg, BattleTech.Data.DataManager dataManager) {
      base.fill(trg, dataManager);
      trg.StatName = this.StatName;
      trg.RelativeModifier = this.RelativeModifier;
      trg.AbsoluteModifier = this.AbsoluteModifier;
    }
    public object toBT(BattleTech.Data.DataManager dataManager) {
      BattleTech.UpgradeDef res = new BattleTech.UpgradeDef();
      this.fill(res, dataManager);
      return res;
    }
  }
}