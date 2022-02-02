using Harmony;
using MessagePack;

namespace CustomPrewarm.Serialize {
  [HarmonyPatch(typeof(BattleTech.BaseDescriptionDef), "FromJSON")]
  [HarmonyPriority(Priority.Last)]
  public static class BaseDescriptionDef_FromJSON {
    public static bool Prefix(BattleTech.BaseDescriptionDef __instance) {
      return Core.disableFromJSON == false;
    }
  }
  [MessagePackObject]
  public class BaseDescriptionDef: ISerializable {
    [Key(0)]
    public string Id { get; set; } = string.Empty;
    [Key(1)]
    public string Name { get; set; } = string.Empty;
    [Key(2)]
    public string Details { get; set; } = string.Empty;
    [Key(3)]
    public string Icon { get; set; } = string.Empty;
    public virtual object toBT(BattleTech.Data.DataManager dataManager) {
      return new BattleTech.BaseDescriptionDef(this.Id, this.Name, this.Details, this.Icon);
    }
    public BaseDescriptionDef(BattleTech.BaseDescriptionDef source) {
      this.Id = source.Id;
      this.Name = source.Name;
      this.Details = source.Details;
      this.Icon = source.Icon;
    }
    public BaseDescriptionDef() {

    }
  }
}