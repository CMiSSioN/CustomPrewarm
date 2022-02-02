using BattleTech;
using Harmony;
using MessagePack;

namespace CustomPrewarm.Serialize {
  [HarmonyPatch(typeof(BattleTech.AmmunitionBoxDef), "FromJSON")]
  [HarmonyPriority(Priority.Last)]
  public static class AmmunitionBoxDef_FromJSON {
    public static bool Prefix(BattleTech.MechDef __instance) {
      return Core.disableFromJSON == false;
    }
  }
  [MessagePackObject]
  public class AmmunitionBoxDef : MechComponentDef, ISerializable {
    [Key(16)]
    public string AmmoID = string.Empty;
    [Key(17)]
    public int Capacity { get; set; } = 0;
    public AmmunitionBoxDef() : base() { }
    public AmmunitionBoxDef(BattleTech.AmmunitionBoxDef src) : base(src) {
      this.AmmoID = src.AmmoID;
      this.Capacity = src.Capacity;
    }
    public void fill(BattleTech.AmmunitionBoxDef def, BattleTech.Data.DataManager dataManager) {
      base.fill(def, dataManager);
      def.ammoID = this.AmmoID;
      def.Capacity = this.Capacity;
    }
    public object toBT(BattleTech.Data.DataManager dataManager) {
      BattleTech.AmmunitionBoxDef result = new BattleTech.AmmunitionBoxDef();
      this.fill(result, dataManager);
      return result;
    }
  }
}