using Harmony;
using MessagePack;
using System.Collections.Generic;

namespace CustomPrewarm.Serialize {
  [HarmonyPatch(typeof(BattleTech.BackgroundDef), "FromJSON")]
  [HarmonyPriority(Priority.Last)]
  public static class BackgroundDef_FromJSON {
    public static bool Prefix(BattleTech.BackgroundDef __instance) {
      return Core.disableFromJSON == false;
    }
  }
  [MessagePackObject]
  public class SimGameStat {
    [Key(0)]
    public string typeString = string.Empty;
    [Key(1)]
    public string name = string.Empty;
    [Key(2)]
    public string value = string.Empty;
    [Key(3)]
    public bool set = false;
    [Key(4)]
    public string valueConstant = string.Empty;
    public SimGameStat() { }
    public SimGameStat(BattleTech.SimGameStat src) {
      this.typeString = src.typeString;
      this.name = src.name;
      this.value = src.value;
      this.set = src.set;
      this.valueConstant = src.valueConstant;
    }
    public BattleTech.SimGameStat toBT() {
      BattleTech.SimGameStat result = new BattleTech.SimGameStat();
      result.typeString = this.typeString;
      result.name = this.name;
      result.value = this.value;
      result.set = this.set;
      result.valueConstant = this.valueConstant;
      return result;
    }
  }
  [MessagePackObject]
  public class SimGameResultAction  {
    [Key(0)]
    public BattleTech.SimGameResultAction.ActionType Type = BattleTech.SimGameResultAction.ActionType.Company_TravelTime;
    [Key(1)]
    public string value = string.Empty;
    [Key(2)]
    public string valueConstant = string.Empty;
    [Key(3)]
    public List<string> additionalValues = new List<string>();
    public SimGameResultAction() { }
    public SimGameResultAction(BattleTech.SimGameResultAction src) {
      this.Type = src.Type;
      this.value = src.value;
      this.valueConstant = src.valueConstant;
      this.additionalValues = new List<string>(src.additionalValues);
    }
    public BattleTech.SimGameResultAction toBT() {
      BattleTech.SimGameResultAction result = new BattleTech.SimGameResultAction();
      result.Type = this.Type;
      result.value = this.value;
      result.valueConstant = this.valueConstant;
      result.additionalValues = this.additionalValues.ToArray();
      return result;
    }
  }
  [MessagePackObject]
  public class SimGameForcedEvent {
    [Key(0)]
    public BattleTech.EventScope Scope = BattleTech.EventScope.None;
    [Key(1)]
    public string EventID = string.Empty;
    [Key(2)]
    public int MinDaysWait = 0;
    [Key(3)]
    public int MaxDaysWait = 0;
    [Key(4)]
    public int Probability = 0;
    [Key(5)]
    public bool RetainPilot = false;
    public SimGameForcedEvent() {}
    public SimGameForcedEvent(BattleTech.SimGameForcedEvent src){
      this.Scope = src.Scope;
      this.EventID = src.EventID;
      this.MinDaysWait = src.MinDaysWait;
      this.MaxDaysWait = src.MaxDaysWait;
      this.Probability = src.Probability;
      this.RetainPilot = src.RetainPilot;
    }
    public BattleTech.SimGameForcedEvent toBT() {
      BattleTech.SimGameForcedEvent result = new BattleTech.SimGameForcedEvent();
      result.Scope = this.Scope;
      result.EventID = this.EventID;
      result.MinDaysWait = this.MinDaysWait;
      result.MaxDaysWait = this.MaxDaysWait;
      result.Probability = this.Probability;
      result.RetainPilot = this.RetainPilot;
      return result;
    }
  }
  [MessagePackObject]
  public class SimGameEventResult {
    [Key(0)]
    public BattleTech.EventScope Scope = BattleTech.EventScope.None;
    [Key(1)]
    public RequirementDef Requirements = null;
    [Key(2)]
    public TagSet AddedTags = new TagSet();
    [Key(3)]
    public TagSet RemovedTags = new TagSet();
    [Key(4)]
    public List<SimGameStat> Stats = new List<SimGameStat>();
    [Key(5)]
    public List<SimGameResultAction> Actions = new List<SimGameResultAction>();
    [Key(6)]
    public List<SimGameForcedEvent> ForceEvents = new List<SimGameForcedEvent>();
    [Key(7)]
    public bool TemporaryResult;
    [Key(8)]
    public int ResultDuration;
    public SimGameEventResult() { }
    public SimGameEventResult(BattleTech.SimGameEventResult src) {
      this.Scope = src.Scope;
      this.Requirements = src.Requirements==null ? null : new RequirementDef(src.Requirements);
      this.AddedTags = src.AddedTags == null? new TagSet() : new TagSet(src.AddedTags);
      this.RemovedTags = src.RemovedTags == null ? new TagSet() : new TagSet(src.RemovedTags);
      this.Stats = new List<SimGameStat>();
      this.Actions = new List<SimGameResultAction>();
      this.ForceEvents = new List<SimGameForcedEvent>();
      if(src.Stats != null) foreach (var el in src.Stats) { this.Stats.Add(new SimGameStat(el)); };
      if (src.Actions != null) foreach (var el in src.Actions) { this.Actions.Add(new SimGameResultAction(el)); };
      if (src.ForceEvents != null) foreach (var el in src.ForceEvents) { this.ForceEvents.Add(new SimGameForcedEvent(el)); };
      this.TemporaryResult = src.TemporaryResult;
      this.ResultDuration = src.ResultDuration;
    }
    public BattleTech.SimGameEventResult toBT() {
      BattleTech.SimGameEventResult result = new BattleTech.SimGameEventResult();
      result.Scope = this.Scope;
      result.Requirements = this.Requirements == null? null :this.Requirements.toBT() as BattleTech.RequirementDef;
      result.AddedTags = this.AddedTags.toBT() as HBS.Collections.TagSet;
      result.RemovedTags = this.RemovedTags.toBT() as HBS.Collections.TagSet; ;
      result.TemporaryResult = this.TemporaryResult;
      result.ResultDuration = this.ResultDuration;
      List<BattleTech.SimGameStat>  Stats = new List<BattleTech.SimGameStat>();
      foreach (var el in this.Stats) { Stats.Add(el.toBT()); }; result.Stats = Stats.ToArray();
      List<BattleTech.SimGameResultAction> Actions = new List<BattleTech.SimGameResultAction>();
      foreach (var el in this.Actions) { Actions.Add(el.toBT()); }; result.Actions = Actions.ToArray();
      List<BattleTech.SimGameForcedEvent> ForceEvents = new List<BattleTech.SimGameForcedEvent>();
      foreach (var el in this.ForceEvents) { ForceEvents.Add(el.toBT()); }; result.ForceEvents = ForceEvents.ToArray();
      return result;
    }
  }
  [MessagePackObject]
  public class BackgroundDef : ISerializable {
    [Key(0)]
    public DescriptionDef Description = new DescriptionDef();
    [Key(1)]
    public SimGameEventResult Results = new SimGameEventResult();
    [Key(2)]
    public int BackgroundSection = 0;
    [Key(3)]
    public int UIOrdering = 0;
    [Key(4)]
    public string OptionName = string.Empty;
    [Key(5)]
    public string OptionDescription = string.Empty;
    [Key(6)]
    public string Intro = string.Empty;
    [Key(7)]
    public string OptionMusicTrigger = string.Empty;
    [Key(8)]
    public string BioDescription = string.Empty;
    public BackgroundDef() { }
    public BackgroundDef(BattleTech.BackgroundDef src) {
      this.Description = new DescriptionDef(src.Description);
      this.Results = new SimGameEventResult(src.Results);
      this.BackgroundSection = src.BackgroundSection;
      this.UIOrdering = src.UIOrdering;
      this.OptionName = src.OptionName;
      this.OptionDescription = src.OptionDescription;
      this.Intro = src.Intro;
      this.OptionMusicTrigger = src.OptionMusicTrigger;
      this.BioDescription = src.BioDescription;
    }
    public object toBT(BattleTech.Data.DataManager dataManager) {
      BattleTech.BackgroundDef result = new BattleTech.BackgroundDef();
      result.dataManager = dataManager;
      result.Description = this.Description.toBT(dataManager) as BattleTech.DescriptionDef;
      result.Results = this.Results.toBT();
      result.BackgroundSection = this.BackgroundSection;
      result.UIOrdering = this.UIOrdering;
      result.OptionName = this.OptionName;
      result.Intro = this.Intro;
      result.OptionMusicTrigger = this.OptionMusicTrigger;
      result.BioDescription = this.BioDescription;
      return result;
    }
  }
}