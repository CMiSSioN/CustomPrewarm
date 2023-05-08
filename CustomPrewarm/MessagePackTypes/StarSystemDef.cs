using HarmonyLib;
using MessagePack;
using System.Collections.Generic;

namespace CustomPrewarm.Serialize {
  [HarmonyPatch(typeof(BattleTech.StarSystemDef), "FromJSON")]
  [HarmonyPriority(Priority.Last)]
  public static class StarSystemDef_FromJSON {
    public static bool Prefix(BattleTech.StarSystemDef __instance) {
      return Core.disableFromJSON == false;
    }
  }
  [MessagePackObject]
  public class ComparisonDef {
    [Key(0)]
    public string obj = string.Empty;
    [Key(1)]
    public BattleTech.Operator op = BattleTech.Operator.Equal;
    [Key(2)]
    public float val = 0f;
    [Key(3)]
    public string valueConstant = string.Empty;
    public ComparisonDef() { }
    public ComparisonDef(BattleTech.ComparisonDef source) {
      this.obj = source.obj;
      this.op = source.op;
      this.val = source.val;
      this.valueConstant = source.valueConstant;
    }
    public BattleTech.ComparisonDef toBT() {
      return new BattleTech.ComparisonDef() { obj = this.obj, op = this.op, val = this.val, valueConstant = this.valueConstant };
    }
  }
  [MessagePackObject]
  public class RequirementDef {
    [Key(0)]
    public BattleTech.EventScope Scope;
    [Key(1)]
    public TagSet RequirementTags = new TagSet();
    [Key(2)]
    public TagSet ExclusionTags = new TagSet();
    [Key(3)]
    public List<ComparisonDef> RequirementComparisons = new List<ComparisonDef>();
    public RequirementDef() {}
    public RequirementDef(BattleTech.RequirementDef def) {
      this.Scope = def.Scope;
      this.RequirementTags = def.RequirementTags==null ? new TagSet() : new TagSet(def.RequirementTags);
      this.ExclusionTags = def.ExclusionTags==null ? new TagSet() : new TagSet(def.ExclusionTags);
      this.RequirementComparisons = new List<ComparisonDef>();
      foreach(BattleTech.ComparisonDef src in def.RequirementComparisons) {
        this.RequirementComparisons.Add(new ComparisonDef(src));
      }
    }
    public BattleTech.RequirementDef toBT() {
      BattleTech.RequirementDef result = new BattleTech.RequirementDef();
      result.Scope = this.Scope;
      result.RequirementTags = this.RequirementTags.toBT() as HBS.Collections.TagSet;
      result.ExclusionTags = this.ExclusionTags.toBT() as HBS.Collections.TagSet;
      result.RequirementComparisons = new List<BattleTech.ComparisonDef>();
      foreach(ComparisonDef src in this.RequirementComparisons) { result.RequirementComparisons.Add(src.toBT()); }
      return result;
    }
  }
  [MessagePackObject]
  public class FakeVector3 {
    [Key(0)]
    public float x = 0f;
    [Key(1)]
    public float y = 0f;
    [Key(2)]
    public float z = 0f;
    public FakeVector3() { }
    public BattleTech.FakeVector3 toBT() { return new BattleTech.FakeVector3() { x = this.x, y = this.y, z = this.z }; }
    public FakeVector3(BattleTech.FakeVector3 src) { this.x = src.x;this.y = src.y;this.z = src.z; }
  }
  [MessagePackObject]
  public class Vector2 {
    [Key(0)]
    public float x = 0f;
    [Key(1)]
    public float y = 0f;
    public Vector2() { }
    public UnityEngine.Vector2 toBT() { return new UnityEngine.Vector2() { x = this.x, y = this.y}; }
    public Vector2(UnityEngine.Vector2 src) { this.x = src.x; this.y = src.y; }
  }
  public class StarSystemDef : ISerializable {
    public string ownerID = string.Empty;
    public List<string> contractEmployerIDs = new List<string>();
    public List<string> contractTargetIDs = new List<string>();
    public int DefaultDifficulty = 0;
    public List<int> DifficultyList = new List<int>();
    public List<BattleTech.SimGameState.SimGameType> DifficultyModes = new List<BattleTech.SimGameState.SimGameType>();
    public string factionShopOwnerID = BattleTech.FactionEnumeration.INVALID_DEFAULT_ID;
    public BaseDescriptionDef Description { get; set; } = new BaseDescriptionDef();
    public string CoreSystemID { get; set; } = string.Empty;
    public FakeVector3 Position { get; set; } = new FakeVector3();
    public TagSet Tags { get; set; } = new TagSet();
    public List<Biome.BIOMESKIN> SupportedBiomes { get; set; } = new List<Biome.BIOMESKIN>();
    public TagSet MapRequiredTags { get; set; } = new TagSet();
    public TagSet MapExcludedTags { get; set; } = new TagSet();
    public bool FuelingStation { get; set; } = false;
    public int JumpDistance { get; set; } = 0;
    public int ShopRefreshRate { get; set; } = 0;
    public int ShopMaxInventory { get; set; } = 0;
    public int ShopMaxSpecials { get; set; } = 0;
    public List<StarSystemDef.SystemInfluenceDef> SystemInfluence { get; set; } = new List<SystemInfluenceDef>();
    public List<RequirementDef> TravelRequirements { get; set; } = new List<RequirementDef>();
    public List<BattleTech.SimGameState.SimGameType> StartingSystemModes { get; set; } = new List<BattleTech.SimGameState.SimGameType>();
    public List<Vector2> StarPosition { get; set; } = new List<Vector2>();
    public BattleTech.SimGameSpaceController.StarType StarType { get; set; } = BattleTech.SimGameSpaceController.StarType.A;
    public bool Depletable { get; set; } = false;
    public bool UseSystemRoninHiringChance { get; set; } = false;
    public float RoninHiringChance { get; set; } = 0f;
    public bool UseMaxContractOverride { get; set; } = false;
    public int MaxContractOverride { get; set; } = 0;
    public List<string> SystemShopItems { get; set; } = new List<string>();
    public List<string> FactionShopItems { get; set; } = new List<string>();
    public List<string> BlackMarketShopItems { get; set; } = new List<string>();
    public StarSystemDef() {}
    public class SystemInfluenceDef {
      public float Influence = 0f;
      public string factionID = string.Empty;
      public SystemInfluenceDef() { }
      public SystemInfluenceDef(BattleTech.StarSystemDef.SystemInfluenceDef src) {
        this.factionID = src.FactionValue.Name;
        this.Influence = src.Influence;
      }
      public BattleTech.StarSystemDef.SystemInfluenceDef toBT() {
        BattleTech.StarSystemDef.SystemInfluenceDef result = new BattleTech.StarSystemDef.SystemInfluenceDef();
        result.factionID = this.factionID;
        result.Influence = this.Influence;
        return result;
      }
    }
    public StarSystemDef(BattleTech.StarSystemDef src) {
      ownerID = src.OwnerValue.Name;
      contractEmployerIDs =  new List<string>(src.ContractEmployerIDList);
      contractTargetIDs = new List<string>(src.ContractTargetIDList);
      DefaultDifficulty = src.DefaultDifficulty;
      DifficultyList = new List<int>(src.DifficultyList);
      DifficultyModes = new List<BattleTech.SimGameState.SimGameType>(src.DifficultyModes);
      factionShopOwnerID = src.FactionShopOwnerValue.Name;
      Description = new BaseDescriptionDef(src.Description);
      CoreSystemID = src.CoreSystemID;
      Position = new FakeVector3(src.Position);
      Tags = src.Tags == null? new TagSet() : new TagSet(src.Tags);
      SupportedBiomes = new List<Biome.BIOMESKIN>(src.SupportedBiomes);
      MapRequiredTags = src.MapRequiredTags == null ? new TagSet() : new TagSet(src.MapRequiredTags);
      MapExcludedTags = src.MapExcludedTags == null ? new TagSet() : new TagSet(src.MapExcludedTags);
      FuelingStation = src.FuelingStation;
      JumpDistance = src.JumpDistance;
      ShopRefreshRate = src.ShopRefreshRate;
      ShopMaxInventory = src.ShopMaxInventory;
      ShopMaxSpecials = src.ShopMaxSpecials;
      SystemInfluence = new List<SystemInfluenceDef>();
      foreach(var el in src.SystemInfluence) { SystemInfluence.Add(new SystemInfluenceDef(el)); };
      TravelRequirements = new List<RequirementDef>();
      foreach (var el in src.TravelRequirements) { TravelRequirements.Add(new RequirementDef(el)); };
      StartingSystemModes = new List<BattleTech.SimGameState.SimGameType>(src.StartingSystemModes);
      StarPosition = new List<Vector2>();
      if(src.StarPosition != null) foreach (var el in src.StarPosition) { StarPosition.Add(new Vector2(el)); };
      StarType = src.StarType;
      Depletable = src.Depletable;
      UseSystemRoninHiringChance = src.UseSystemRoninHiringChance;
      RoninHiringChance = src.RoninHiringChance;
      UseMaxContractOverride = src.UseMaxContractOverride;
      MaxContractOverride = src.MaxContractOverride;
      SystemShopItems = src.SystemShopItems == null? new List<string>() : new List<string>(src.SystemShopItems);
      FactionShopItems = src.FactionShopItems == null ? new List<string>() : new List<string>(src.FactionShopItems);
      BlackMarketShopItems  = src.BlackMarketShopItems == null ? new List<string>() : new List<string>(src.BlackMarketShopItems);
    }
    public object toBT(BattleTech.Data.DataManager dataManager) {
      BattleTech.StarSystemDef res = new BattleTech.StarSystemDef();
      res.ownerID = this.ownerID;
      res.contractEmployerIDs = new List<string>(this.contractEmployerIDs);
      res.contractTargetIDs = new List<string>(this.contractTargetIDs);
      res.DefaultDifficulty = this.DefaultDifficulty;
      res.DifficultyList = new List<int>(this.DifficultyList);
      res.DifficultyModes = new List<BattleTech.SimGameState.SimGameType>(this.DifficultyModes);
      res.factionShopOwnerID = this.factionShopOwnerID;
      res.Description = this.Description.toBT(dataManager) as BattleTech.BaseDescriptionDef;
      res.CoreSystemID = this.CoreSystemID;
      res.Position = this.Position.toBT();
      res.Tags = this.Tags.toBT();
      res.SupportedBiomes = new List<Biome.BIOMESKIN>(this.SupportedBiomes);
      res.MapRequiredTags = this.MapRequiredTags.toBT();
      res.MapExcludedTags = this.MapExcludedTags.toBT();
      res.FuelingStation = this.FuelingStation;
      res.JumpDistance = this.JumpDistance;
      res.ShopRefreshRate = this.ShopRefreshRate;
      res.ShopMaxInventory = this.ShopMaxInventory;
      res.ShopMaxSpecials = this.ShopMaxSpecials;
      res.SystemInfluence = new List<BattleTech.StarSystemDef.SystemInfluenceDef>();
      foreach (var el in this.SystemInfluence) { res.SystemInfluence.Add(el.toBT()); };
      res.TravelRequirements = new List<BattleTech.RequirementDef>();
      foreach (var el in this.TravelRequirements) { res.TravelRequirements.Add(el.toBT()); };
      res.StartingSystemModes = new List<BattleTech.SimGameState.SimGameType>(this.StartingSystemModes);
      List<UnityEngine.Vector2> StarPosition = new List<UnityEngine.Vector2>();
      foreach (var el in this.StarPosition) { StarPosition.Add(el.toBT()); };
      res.StarPosition = StarPosition.ToArray();
      res.StarType = this.StarType;
      res.Depletable = this.Depletable;
      res.UseSystemRoninHiringChance = this.UseSystemRoninHiringChance;
      res.RoninHiringChance = this.RoninHiringChance;
      res.UseMaxContractOverride = this.UseMaxContractOverride;
      res.MaxContractOverride = this.MaxContractOverride;
      res.SystemShopItems = new List<string>(this.SystemShopItems);
      res.FactionShopItems = new List<string>(this.FactionShopItems);
      res.BlackMarketShopItems = new List<string>(this.BlackMarketShopItems);
      return res;
    }
  }
}