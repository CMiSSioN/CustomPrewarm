using Harmony;
using MessagePack;

namespace CustomPrewarm.Serialize {
  [HarmonyPatch(typeof(BattleTech.PathingCapabilitiesDef), "FromJSON")]
  [HarmonyPriority(Priority.Last)]
  public static class PathingCapabilitiesDef_FromJSON {
    public static bool Prefix(BattleTech.PathingCapabilitiesDef __instance) {
      return Core.disableFromJSON == false;
    }
  }
  [MessagePackObject]
  public class PathingCapabilitiesDef : ISerializable {
    [Key(0)]
    public BaseDescriptionDef Description { get; set; } = new BaseDescriptionDef();
    [Key(1)]
    public float MinGrade { get; set; } = 0f;
    [Key(2)]
    public float MaxGrade { get; set; } = 0f;
    [Key(3)]
    public float MaxSteepness { get; set; } = 0f;
    [Key(4)]
    public float GradeMultiplier { get; set; } = 0f;
    [Key(5)]
    public float GradeMultMaxAscending { get; set; } = 0f;
    [Key(6)]
    public float GradeMultMaxDescending { get; set; } = 0f;
    [Key(7)]
    public float MaxStairSize { get; set; } = 0f;
    [Key(8)]
    public float MoveCostNormal { get; set; } = 0f;
    [Key(9)]
    public float MoveCostBackward { get; set; } = 0f;
    [Key(10)]
    public float PathingAngleCost { get; set; } = 0f;
    public PathingCapabilitiesDef() { }
    public PathingCapabilitiesDef(BattleTech.PathingCapabilitiesDef src) {
      this.Description = new BaseDescriptionDef(src.Description);
      this.MinGrade = src.MinGrade;
      this.MaxGrade = src.MaxGrade;
      this.MaxSteepness = src.MaxSteepness;
      this.GradeMultiplier = src.GradeMultiplier;
      this.GradeMultMaxAscending = src.GradeMultMaxAscending;
      this.GradeMultMaxDescending = src.GradeMultMaxDescending;
      this.MaxStairSize = src.MaxStairSize;
      this.MoveCostNormal = src.MoveCostNormal;
      this.MoveCostBackward = src.MoveCostBackward;
      this.PathingAngleCost = src.PathingAngleCost;
    }
    public object toBT(BattleTech.Data.DataManager dataManager) {
      BattleTech.PathingCapabilitiesDef res = new BattleTech.PathingCapabilitiesDef();
      res.Description = this.Description.toBT(dataManager) as BattleTech.BaseDescriptionDef;
      res.MinGrade = this.MinGrade;
      res.MaxGrade = this.MaxGrade;
      res.MaxSteepness = this.MaxSteepness;
      res.GradeMultiplier = this.GradeMultiplier;
      res.GradeMultMaxAscending = this.GradeMultMaxAscending;
      res.GradeMultMaxDescending = this.GradeMultMaxDescending;
      res.MaxStairSize = this.MaxStairSize;
      res.MoveCostNormal = this.MoveCostNormal;
      res.MoveCostBackward = this.MoveCostBackward;
      res.PathingAngleCost = this.PathingAngleCost;
      return res;
    }
  }
}