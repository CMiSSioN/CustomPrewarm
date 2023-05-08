using HarmonyLib;
using MessagePack;

namespace CustomPrewarm.Serialize {
  [HarmonyPatch(typeof(BattleTech.MovementCapabilitiesDef), "FromJSON")]
  [HarmonyPriority(Priority.Last)]
  public static class MovementCapabilitiesDef_FromJSON {
    public static bool Prefix(BattleTech.HardpointDataDef __instance) {
      return Core.disableFromJSON == false;
    }
  }
  [MessagePackObject]
  public class MovementCapabilitiesDef : ISerializable {
    [Key(0)]
    public BaseDescriptionDef Description { get; set; } = new BaseDescriptionDef();
    [Key(1)]
    public float MaxWalkDistance { get; set; } = 0f;
    [Key(2)]
    public float MaxSprintDistance { get; set; } = 0f;
    [Key(3)]
    public float WalkVelocity { get; set; } = 0f;
    [Key(4)]
    public float RunVelocity { get; set; } = 0f;
    [Key(5)]
    public float SprintVelocity { get; set; } = 0f;
    [Key(6)]
    public float LimpVelocity { get; set; } = 0f;
    [Key(7)]
    public float WalkAcceleration { get; set; } = 0f;
    [Key(8)]
    public float SprintAcceleration { get; set; } = 0f;
    [Key(9)]
    public float MaxRadialVelocity { get; set; } = 0f;
    [Key(10)]
    public float MaxRadialAcceleration { get; set; } = 0f;
    [Key(11)]
    public float MaxJumpVel { get; set; } = 0f;
    [Key(12)]
    public float MaxJumpAccel { get; set; } = 0f;
    [Key(13)]
    public float MaxJumpVerticalAccel { get; set; } = 0f;
    public MovementCapabilitiesDef() { }
    public MovementCapabilitiesDef(BattleTech.MovementCapabilitiesDef src) {
      this.Description = new BaseDescriptionDef(src.Description);
      this.MaxWalkDistance = src.MaxWalkDistance;
      this.MaxSprintDistance = src.MaxSprintDistance;
      this.WalkVelocity = src.WalkVelocity;
      this.RunVelocity = src.RunVelocity;
      this.SprintVelocity = src.SprintVelocity;
      this.LimpVelocity = src.LimpVelocity;
      this.WalkAcceleration = src.WalkAcceleration;
      this.SprintAcceleration = src.SprintAcceleration;
      this.MaxRadialVelocity = src.MaxRadialVelocity;
      this.MaxRadialAcceleration = src.MaxRadialAcceleration;
      this.MaxJumpVel = src.MaxJumpVel;
      this.MaxJumpAccel = src.MaxJumpAccel;
      this.MaxJumpVerticalAccel = src.MaxJumpVerticalAccel;
    }
    public object toBT(BattleTech.Data.DataManager dataManager) {
      BattleTech.MovementCapabilitiesDef res = new BattleTech.MovementCapabilitiesDef();
      res.Description = this.Description.toBT(dataManager) as BattleTech.BaseDescriptionDef;
      res.MaxWalkDistance = this.MaxWalkDistance;
      res.MaxSprintDistance = this.MaxSprintDistance;
      res.WalkVelocity = this.WalkVelocity;
      res.RunVelocity = this.RunVelocity;
      res.SprintVelocity = this.SprintVelocity;
      res.LimpVelocity = this.LimpVelocity;
      res.WalkAcceleration = this.WalkAcceleration;
      res.SprintAcceleration = this.SprintAcceleration;
      res.MaxRadialVelocity = this.MaxRadialVelocity;
      res.MaxRadialAcceleration = this.MaxRadialAcceleration;
      res.MaxJumpVel = this.MaxJumpVel;
      res.MaxJumpAccel = this.MaxJumpAccel;
      res.MaxJumpVerticalAccel = this.MaxJumpVerticalAccel;
      return res;
    }
  }
}