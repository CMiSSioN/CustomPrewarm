using Harmony;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
namespace CustomPrewarm.Serialize {
  //[HarmonyPatch(typeof(BattleTech.MechValidationRules), "ValidateMechDef")]
  //public static class MechValidationRulesValidate_ValidateMech_Patch {
  //  public static void Prefix(Dictionary<BattleTech.MechValidationType, List<Localize.Text>> __result,BattleTech.MechValidationLevel validationLevel,BattleTech.MechDef mechDef) {
  //    try {
  //      Log.M?.TWL(0, "MechValidationRules.ValidateMechDef:"+mechDef.Description.Id);
  //      Log.M?.WL(0, mechDef.ToJSON());
  //    } catch (Exception e) {
  //      Log.M_Err?.TWL(0,e.ToString(),true);
  //    }
  //  }
  //}
  public class LocalSettingsHelper {
    public static string ResetSettings() {
      MainMenu_ShowRefreshingSaves.ResetCache();
      return JsonConvert.SerializeObject(Core.GlobalSettings, Formatting.Indented);
    }
    public static void ReadSettings(string json) {
      try {
        Log.M?.TWL(0, "Reading local settings");
        CPSettings local = JsonConvert.DeserializeObject<CPSettings>(json);
        Core.Settings.ApplyLocal(local);
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
  }
  public interface ISerializable {
    object toBT(BattleTech.Data.DataManager dataManager);
  };
  public static class SerializableFactory {
    public static string extractDetails(object Description) {
      IDictionary<string, object> dictionary = Description as IDictionary<string, object>;
      if(dictionary == null) {
        Log.M?.TWL(0, Description.GetType().ToString() + " is not IDictionary<string, object>");
        return string.Empty;
      }
      if(dictionary.TryGetValue("Details", out object Details)) {
        return Details==null?string.Empty:Details.ToString();
      }
      return string.Empty;
    }
    public static void setDescription(HBS.Util.IJsonTemplated source, string details) {
      if (source is BattleTech.MechComponentDef componentDef) {
        componentDef.Description.Details = details;
      } else
      if (source is BattleTech.AbilityDef abilityDef) {
        abilityDef.Description.Details = details;
      } else
      if (source is BattleTech.AmmunitionDef ammoDef) {
        ammoDef.Description.Details = details;
      } else
      if (source is BattleTech.BaseDescriptionDef baseDescrDef) {
        baseDescrDef.Details = details;
      } else
      if (source is BattleTech.BackgroundDef backDef) {
        backDef.Description.Details = details;
      } else
      if (source is BattleTech.StarSystemDef starDef) {
        starDef.Description.Details = details;
      } else
      if (source is BattleTech.ChassisDef chassisDef) {
        chassisDef.Description.Details = details;
      } else
      if (source is BattleTech.VehicleChassisDef vchassisDef) {
        vchassisDef.Description.Details = details;
      } else
      if (source is BattleTech.HardpointDataDef hardpointDef) {
        return;
      } else
      if (source is BattleTech.PathingCapabilitiesDef pathingDef) {
        pathingDef.Description.Details = details;
      } else
      if (source is BattleTech.MovementCapabilitiesDef movementDef) {
        movementDef.Description.Details = details;
      } else
      if (source is BattleTech.VehicleDef vehicleDef) {
        vehicleDef.Description.Details = details;
      } else
      if (source is BattleTech.MechDef mechDef) {
        mechDef.Description.Details = details;
      } else {
        throw new System.Exception("unknown type " + source.GetType());
      }
    }
    public static string getDescription(HBS.Util.IJsonTemplated source) {
      if (source is BattleTech.MechComponentDef componentDef) {
        return componentDef.Description.Details;
      } else
      if (source is BattleTech.AbilityDef abilityDef) {
        return abilityDef.Description.Details;
      } else
      if (source is BattleTech.AmmunitionDef ammoDef) {
        return ammoDef.Description.Details;
      } else
      if (source is BattleTech.BaseDescriptionDef baseDescrDef) {
        return baseDescrDef.Details;
      } else
      if (source is BattleTech.BackgroundDef backDef) {
        return backDef.Description.Details;
      } else
      if (source is BattleTech.StarSystemDef starDef) {
        return starDef.Description.Details;
      } else
      if (source is BattleTech.ChassisDef chassisDef) {
        return chassisDef.Description.Details;
      } else
      if (source is BattleTech.VehicleChassisDef vchassisDef) {
        return vchassisDef.Description.Details;
      } else
      if (source is BattleTech.HardpointDataDef hardpointDef) {
        return string.Empty;
      } else
      if (source is BattleTech.PathingCapabilitiesDef pathingDef) {
        return pathingDef.Description.Details;
      } else
      if (source is BattleTech.MovementCapabilitiesDef movementDef) {
        return movementDef.Description.Details;
      } else
      if (source is BattleTech.VehicleDef vehicleDef) {
        return vehicleDef.Description.Details;
      } else
      if (source is BattleTech.MechDef mechDef) {
        return mechDef.Description.Details;
      } else {
        throw new System.Exception("unknown type " + source.GetType());
      }
    }
    public static IDictionary getStorage(this BattleTech.Data.DataManager dataManager, BattleTech.BattleTechResourceType resType) {
      switch (resType) {
        case BattleTech.BattleTechResourceType.AbilityDef: return dataManager.abilityDefs.items;
        case BattleTech.BattleTechResourceType.AmmunitionDef: return dataManager.ammoDefs.items;
        case BattleTech.BattleTechResourceType.AmmunitionBoxDef: return dataManager.ammoBoxDefs.items;
        case BattleTech.BattleTechResourceType.BackgroundDef: return dataManager.backgroundDefs.items;
        case BattleTech.BattleTechResourceType.BaseDescriptionDef: return dataManager.baseDescriptionDefs.items;
        case BattleTech.BattleTechResourceType.BuildingDef: return dataManager.buildingDefs.items;
        case BattleTech.BattleTechResourceType.CastDef: return dataManager.castDefs.items;
        case BattleTech.BattleTechResourceType.ChassisDef: return dataManager.chassisDefs.items;
        case BattleTech.BattleTechResourceType.DesignMaskDef: return dataManager.designMaskDefs.items;
        case BattleTech.BattleTechResourceType.FactionDef: return dataManager.factions.items;
        case BattleTech.BattleTechResourceType.HardpointDataDef: return dataManager.hardpointDataDefs.items;
        case BattleTech.BattleTechResourceType.HeatSinkDef: return dataManager.heatSinkDefs.items;
        case BattleTech.BattleTechResourceType.HeraldryDef: return dataManager.heraldries.items;
        case BattleTech.BattleTechResourceType.JumpJetDef: return dataManager.jumpJetDefs.items;
        case BattleTech.BattleTechResourceType.LanceDef: return dataManager.lanceDefs.items;
        case BattleTech.BattleTechResourceType.MechDef: return dataManager.mechDefs.items;
        case BattleTech.BattleTechResourceType.MovementCapabilitiesDef: return dataManager.movementCapDefs.items;
        case BattleTech.BattleTechResourceType.PathingCapabilitiesDef: return dataManager.pathingCapDefs.items;
        case BattleTech.BattleTechResourceType.PilotDef: return dataManager.pilotDefs.items;
        case BattleTech.BattleTechResourceType.StarSystemDef: return dataManager.systemDefs.items;
        case BattleTech.BattleTechResourceType.TurretChassisDef: return dataManager.turretChassisDefs.items;
        case BattleTech.BattleTechResourceType.TurretDef: return dataManager.turretDefs.items;
        case BattleTech.BattleTechResourceType.UpgradeDef: return dataManager.upgradeDefs.items;
        case BattleTech.BattleTechResourceType.VehicleChassisDef: return dataManager.vehicleChassisDefs.items;
        case BattleTech.BattleTechResourceType.VehicleDef: return dataManager.vehicleDefs.items;
        case BattleTech.BattleTechResourceType.WeaponDef: return dataManager.weaponDefs.items;
        default: throw new Exception("unknown resource type:" + resType);
      }
    }
    public static ISerializable Create(HBS.Util.IJsonTemplated source) {
      if (source is BattleTech.AbilityDef abilityDef) {
        return new AbilityDef(abilityDef);
      } else
      if (source is BattleTech.BaseDescriptionDef baseDescrDef) {
        return new BaseDescriptionDef(baseDescrDef);
      } else
      if (source is BattleTech.AmmunitionBoxDef ammoBoxDef) {
        return new AmmunitionBoxDef(ammoBoxDef);
      } else
      if (source is BattleTech.WeaponDef weaponDef) {
        return new WeaponDef(weaponDef);
      } else
      if (source is BattleTech.AmmunitionDef ammoDef) {
        return new AmmunitionDef(ammoDef);
      } else
      if (source is BattleTech.BackgroundDef backDef) {
        return new BackgroundDef(backDef);
      } else
      if (source is BattleTech.StarSystemDef starDef) {
        return new StarSystemDef(starDef);
      } else
      if (source is BattleTech.ChassisDef chassisDef) {
        return new ChassisDef(chassisDef);
      } else
      if (source is BattleTech.UpgradeDef upgradeDef) {
        return new UpgradeDef(upgradeDef);
      } else
      if (source is BattleTech.HeatSinkDef heatSinkDef) {
        return new HeatSinkDef(heatSinkDef);
      } else
      if (source is BattleTech.JumpJetDef jumpDef) {
        return new JumpJetDef(jumpDef);
      } else
      if (source is BattleTech.VehicleChassisDef vchassisDef) {
        return new VehicleChassisDef(vchassisDef);
      } else
      if (source is BattleTech.HardpointDataDef hardpointDef) {
        return new HardpointDataDef(hardpointDef);
      } else
      if (source is BattleTech.PathingCapabilitiesDef pathingDef) {
        return new PathingCapabilitiesDef(pathingDef);
      } else
      if (source is BattleTech.MovementCapabilitiesDef movementDef) {
        return new MovementCapabilitiesDef(movementDef);
      } else
      if (source is BattleTech.VehicleDef vehicleDef) {
        return new VehicleDef(vehicleDef);
      } else
      if (source is BattleTech.MechDef mechDef) {
        return new MechDef(mechDef);
      } else {
        throw new System.Exception("unknown type " + source.GetType());
      }
    }
    public static void Refresh(HBS.Util.IJsonTemplated source, BattleTech.Data.DataManager dataManager) {
      if (source is BattleTech.AbilityDef abilityDef) {
        abilityDef.dataManager = dataManager;
      } else
      if (source is BattleTech.BaseDescriptionDef baseDescrDef) {
        return;
      } else
      if (source is BattleTech.AmmunitionBoxDef ammoBoxDef) {
        ammoBoxDef.dataManager = dataManager;
        try { ammoBoxDef.refreshAbilityDefs(); } catch (Exception e) { Log.M_Err.TWL(0,ammoBoxDef.Description.Id); Log.M_Err.WL(0,e.ToString(),true); }
      } else
      if (source is BattleTech.WeaponDef weaponDef) {
        weaponDef.dataManager = dataManager;
        try { weaponDef.refreshAbilityDefs(); } catch (Exception e) { Log.M_Err.TWL(0, weaponDef.Description.Id); Log.M_Err.WL(0, e.ToString(), true); }
      } else
      if (source is BattleTech.AmmunitionDef ammoDef) {
        return;
      } else
      if (source is BattleTech.BackgroundDef backDef) {
        backDef.dataManager = dataManager;
      } else
      if (source is BattleTech.StarSystemDef starDef) {
        return;
      } else
      if (source is BattleTech.ChassisDef chassisDef) {
        chassisDef.dataManager = dataManager;
        try {chassisDef.Refresh(); } catch (Exception e) { Log.M_Err.TWL(0, chassisDef.Description.Id); Log.M_Err.WL(0, e.ToString(), true); }
      } else
      if (source is BattleTech.UpgradeDef upgradeDef) {
        upgradeDef.dataManager = dataManager;
        try {upgradeDef.refreshAbilityDefs(); } catch (Exception e) { Log.M_Err.TWL(0, upgradeDef.Description.Id); Log.M_Err.WL(0, e.ToString(), true); }
      } else
      if (source is BattleTech.HeatSinkDef heatSinkDef) {
        heatSinkDef.dataManager = dataManager;
        try {heatSinkDef.refreshAbilityDefs(); } catch (Exception e) { Log.M_Err.TWL(0, heatSinkDef.Description.Id); Log.M_Err.WL(0, e.ToString(), true); }
      } else
      if (source is BattleTech.JumpJetDef jumpDef) {
        jumpDef.dataManager = dataManager;
        try {jumpDef.refreshAbilityDefs(); } catch (Exception e) { Log.M_Err.TWL(0, jumpDef.Description.Id); Log.M_Err.WL(0, e.ToString(), true); }
      } else
      if (source is BattleTech.VehicleChassisDef vchassisDef) {
        vchassisDef.dataManager = dataManager;
        try {vchassisDef.Refresh(); } catch (Exception e) { Log.M_Err.TWL(0, vchassisDef.Description.Id); Log.M_Err.WL(0, e.ToString(), true); }
      } else
      if (source is BattleTech.PathingCapabilitiesDef pathingDef) {
        return;
      } else
      if (source is BattleTech.MovementCapabilitiesDef movementDef) {
        return;
      } else
      if (source is BattleTech.HardpointDataDef hardpointDef) {
        return;
      } else
      if (source is BattleTech.VehicleDef vehicleDef) {
        vehicleDef.dataManager = dataManager;
        try { vehicleDef.Refresh(); } catch (Exception e) { Log.M_Err.TWL(0, vehicleDef.Description.Id); Log.M_Err.WL(0, e.ToString(), true); }
      } else
      if (source is BattleTech.MechDef mechDef) {
        mechDef.dataManager = dataManager;
        try { mechDef.Refresh(); } catch (Exception e) { Log.M_Err.TWL(0, mechDef.Description.Id); Log.M_Err.WL(0, e.ToString(), true); }
      } else {
        throw new System.Exception("unknown type " + source.GetType());
      }
    }
  }
}

namespace CustomPrewarm{
  public class CPSettings {
    public bool debugLog { get; set; } = false;
    public bool UseFastPreloading { get; set; } = false;
    public bool UseHashedPreloading { get; set; } = false;
    public int ModTekIndexingDelay { get; set; } = 1000;
    [JsonIgnore]
    public string savesdirectory { get; set; } = string.Empty;
    [JsonIgnore]
    public string directory { get; set; } = string.Empty;
    public CPSettings() { }
    public void ApplyLocal(CPSettings local) {
      this.debugLog = local.debugLog;
      this.UseFastPreloading = local.UseFastPreloading;
      this.UseHashedPreloading = local.UseHashedPreloading;
    }
  }
  public class SerializeRegistred {
    public string id { get; set; } = string.Empty;
    public Func<string, object> gatherObjectCallback { get; set; } = null;
  }
  public static class Core {
    public static bool CacheLoadingInited = false;
    private static Dictionary<BattleTech.BattleTechResourceType, Dictionary<string, Dictionary<string, object>>> deserializedObjects = new Dictionary<BattleTech.BattleTechResourceType, Dictionary<string, Dictionary<string, object>>>();
    public static void registerDeserializedObjects(BattleTech.BattleTechResourceType resType, string id, Dictionary<string, object> deserialized) {
      if(deserializedObjects.TryGetValue(resType, out Dictionary<string, Dictionary<string, object>> regObjects) == false) {
        regObjects = new Dictionary<string, Dictionary<string, object>>();
        deserializedObjects.Add(resType, regObjects);
      }
      if(regObjects.TryGetValue(id, out Dictionary<string, object> objects) == false) {
        regObjects.Add(id, deserialized);
        return;
      }
      foreach (var obj in deserialized) {
        if (objects.ContainsKey(obj.Key)) { objects[obj.Key] = obj.Value; } else { objects.Add(obj.Key, obj.Value); }
      }
    }
    public static object getDeserializedObject(BattleTech.BattleTechResourceType resType, string id, string modname) {
      if (deserializedObjects.TryGetValue(resType, out Dictionary<string, Dictionary<string, object>> dobjects) == false) { return null;}
      if (dobjects.TryGetValue(id, out Dictionary<string, object> dobject) == false) { return null; }
      if (dobject.TryGetValue(modname, out object result) == false) { return null; }
      return result;
    }
    private static Dictionary<BattleTech.BattleTechResourceType, Dictionary<string, SerializeRegistred>> registredSerializators = new Dictionary<BattleTech.BattleTechResourceType, Dictionary<string, SerializeRegistred>>();
    public static void RegisterSerializator(string id, BattleTech.BattleTechResourceType resType, Func<string, object> callback) {
      if(registredSerializators.TryGetValue(resType, out Dictionary<string, SerializeRegistred> registred) == false) {
        registred = new Dictionary<string, SerializeRegistred>();
        registredSerializators.Add(resType,registred);
      }
      SerializeRegistred regCallback = new SerializeRegistred() { id = id, gatherObjectCallback = callback };
      if (registred.ContainsKey(id)) {
        registred[id] = regCallback;
      } else {
        registred.Add(id, regCallback);
      }
    }
    public static Dictionary<string, object> GatherRegistredObjects(BattleTech.BattleTechResourceType resType, string id) {
      Dictionary<string, object> result = new Dictionary<string, object>();
      if(registredSerializators.TryGetValue(resType, out Dictionary<string, SerializeRegistred> registred)) {
        foreach(var regCallback in registred) {
          result.Add(regCallback.Value.id, regCallback.Value.gatherObjectCallback(id));
        }
      }
      return result;
    }
    public static bool disableFromJSON { get; set; } = false;
    public static CPSettings Settings { get; set; }
    public static CPSettings GlobalSettings { get; set; }
    public static void FinishedLoading(List<string> loadOrder) {
      Log.M_Err?.TWL(0, "FinishedLoading", true);
      try {
        var harmony = HarmonyInstance.Create("io.kmission.fastload");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        CustomTranslation.Core.RegisterResetCache(MainMenu_ShowRefreshingSaves.ResetCache);
        CustomSettings.ModsLocalSettingsHelper.RegisterLocalSettings("CustomPrewarm", "Custom Cache", Serialize.LocalSettingsHelper.ResetSettings, Serialize.LocalSettingsHelper.ReadSettings);
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
    public static void Init(string directory, string settingsJson) {
      Log.BaseDirectory = directory;
      Core.Settings = new CPSettings();
      Core.Settings.directory = directory;
      Log.InitLog();
      try {
        Log.M_Err?.TWL(0, "Initing... " + directory + " version: " + Assembly.GetExecutingAssembly().GetName().Version, true);
        Core.Settings = JsonConvert.DeserializeObject<CPSettings>(settingsJson);
        Core.GlobalSettings = JsonConvert.DeserializeObject<CPSettings>(settingsJson);
        Core.Settings.directory = directory;
        string codeBase = typeof(BattleTech.Data.DataManager).Assembly.CodeBase;
        UriBuilder uri = new UriBuilder(codeBase);
        string path = Uri.UnescapeDataString(uri.Path);
        string dllDir = Path.GetDirectoryName(path);
        Log.M_Err?.WL(1,"game dlls dir:"+dllDir);
        codeBase = Assembly.GetExecutingAssembly().CodeBase;
        uri = new UriBuilder(codeBase);
        path = Uri.UnescapeDataString(uri.Path);
        string selfName = Path.GetFileName(path);
        Log.M_Err?.WL(1, "self dll name" + selfName);

        string[] dlls = Directory.GetFiles(directory, "*.dll", SearchOption.TopDirectoryOnly);
        foreach(string dllName in dlls) {
          if (Path.GetFileName(dllName) == selfName) { continue; }
          string trgPath = Path.Combine(dllDir, Path.GetFileName(dllName));
          if (File.Exists(trgPath)) { continue; }
          Log.M_Err?.WL(1, "copy");
          Log.M_Err?.WL(2, dllName);
          Log.M_Err?.WL(2, trgPath);
          File.Copy(dllName, trgPath);
        }

        //Assembly SystemRuntime = AppDomain.CurrentDomain.Load(Path.Combine(directory, "System.Runtime.dll"));
        //if (SystemRuntime != null) {
        //  Log.M?.TWL(0, "System.Runtime.dll loaded successfully " + SystemRuntime.FullName);
        //}
        //Assembly SystemThreadingTasksExtensions = AppDomain.CurrentDomain.Load(Path.Combine(directory, "System.Threading.Tasks.Extensions.dll"));
        //if (SystemThreadingTasksExtensions != null) {
        //  Log.M?.TWL(0, "System.Threading.Tasks.Extensions.dll loaded successfully " + SystemThreadingTasksExtensions.FullName);
        //}
        //Assembly messagePack = AppDomain.CurrentDomain.Load(Path.Combine(directory, "MessagePack.dll"));
        //if (messagePack != null) {
        //  Log.M?.TWL(0, "MessagePack.dll loaded successfully " + messagePack.FullName);
        //}
      } catch (Exception e) {
        Log.M_Err?.TWL(0, e.ToString(), true);
      }
    }
  }
}
