using BattleTech;
using BattleTech.Data;
using BattleTech.Save;
using BattleTech.Save.Core;
using BattleTech.Save.Test;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using CustomComponents;
using CustomTranslation;
using HarmonyLib;
using HBS;
using HBS.Data;
using HBS.Util;
using MessagePack;
using MessagePack.Resolvers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace CustomPrewarm {
  //[HarmonyPatch]
  //public static class JSONSerializationUtility_RehydrateObjectFromDictionary_Patch {
  //  public static MethodBase TargetMethod() => (MethodBase)typeof(JSONSerializationUtility).GetMethod("RehydrateObjectFromDictionary", BindingFlags.Static | BindingFlags.NonPublic);

  //  public static void Prefix(object target, Dictionary<string, object> values) {
  //    try {
  //      Log.M?.TWL(0, "RehydrateObjectFromDictionary:" + target.GetType() + " values:" + values.Count);
  //      foreach (var val in values) { 
  //        Log.M?.WL(1, val.Key+":"+val.Value.GetType());
  //      }
  //    } catch (Exception ex) {
  //    }
  //  }
  //}
  public static class TransformHelper {
    public static T FindObject<T>(this GameObject go, string name) where T : MonoBehaviour {
      T[] components = go.GetComponentsInChildren<T>(true);
      foreach (T component in components) { if (component.transform.name == name) { return component; } }
      return null;
    }
  }
  [HarmonyPatch(typeof(MainMenu))]
  [HarmonyPatch("ShowRefreshingSaves")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class MainMenu_ShowRefreshingSaves {
    public class PreCacheItem {
      public HBS.Util.IJsonTemplated definition { get; set; } = null;
      public Dictionary<string, object> custom = null;
      public string originalJson = string.Empty;
      public byte[] hash = new byte[0];
      public PreCacheItem(HBS.Util.IJsonTemplated def, Dictionary<string, object> cust, string json, byte[] hash) {
        this.definition = def;
        this.custom = cust;
        this.originalJson = json;
        this.hash = hash;
      }
    }
    public class PostCacheItem {
      public HBS.Util.IJsonTemplated definition { get; set; } = null;
      public string id = string.Empty;
      public string originalJson = string.Empty;
      public string details = string.Empty;
      public PostCacheItem(string id, HBS.Util.IJsonTemplated def, string json, string details) {
        this.definition = def;
        this.id = id;
        this.originalJson = json;
        this.details = details;
      }
    }
    [MessagePackObject]
    public class CacheItem {
      [Key(0)]
      public object definition = null;
      [IgnoreMember]
      public Serialize.ISerializable serializable { get { return this.definition as Serialize.ISerializable; } }
      [Key(1)]
      public Dictionary<string, object> custom = null;
      [Key(2)]
      public string originalJson = string.Empty;
      [Key(3)]
      public Dictionary<string, object> registred = null;
      [Key(4)]
      public byte[] hash = null;
      [IgnoreMember]
      public bool invalidated = false;
      [IgnoreMember]
      public HBS.Util.IJsonTemplated btDefinition = null;
      [IgnoreMember]
      public string details = string.Empty;
      public CacheItem(HBS.Util.IJsonTemplated def, Dictionary<string, object> cust, Dictionary<string, object> reg, string originalJson) {
        //Log.M?.TWL(0,"definition");
        definition = Serialize.SerializableFactory.Create(def);
        //Log.M?.TWL(0, "custom");
        custom = cust;
        this.registred = reg;
        this.originalJson = originalJson;
      }
      public void AddToDataBase(BattleTech.Data.DataManager dataManager, BattleTechResourceType resType, string id) {
        Serialize.ISerializable def = this.definition as Serialize.ISerializable;
        if (def == null) {
          Log.M?.TWL(0, resType + ":" + id + " is not ISerializable " + this.definition.GetType());
          return;
        }
        this.btDefinition = def.toBT(dataManager) as IJsonTemplated;
        if (this.btDefinition == null) {
          Log.M?.TWL(0, resType + ":" + id + " is not IJsonTemplated");
          return;
        }
        this.details = Serialize.SerializableFactory.getDescription(this.btDefinition);
        Registry_ProcessCustomFactories(def, this.custom);
        AddToDataManager(dataManager, resType, id, this.btDefinition);
        Core.registerDeserializedObjects(resType, id, this.registred);
      }
      public CacheItem() { }
    }
    [MessagePackObject]
    public class CacheData {
      [Key(0)]
      public string Version;
      [Key(1)]
      public Dictionary<BattleTechResourceType, Dictionary<string, CacheItem>> payload = new Dictionary<BattleTechResourceType, Dictionary<string, CacheItem>>();
      [IgnoreMember]
      public bool update = false;
      public CacheData() { }
    }
    public static void AddToDataManager<T>(DataManager dataManager, BattleTechResourceType resType, string id, IJsonTemplated data) where T : IJsonTemplated {
      try {
        switch (resType) {
          case BattleTechResourceType.AbilityDef: { dataManager.abilityDefs.Add(id, (AbilityDef)data); }; break;
          case BattleTechResourceType.AmmunitionBoxDef: { dataManager.ammoBoxDefs.Add(id, (AmmunitionBoxDef)data); }; break;
          case BattleTechResourceType.BackgroundDef: { dataManager.backgroundDefs.Add(id, (BackgroundDef)data); }; break;
          case BattleTechResourceType.BaseDescriptionDef: { dataManager.baseDescriptionDefs.Add(id, (BaseDescriptionDef)data); }; break;
          case BattleTechResourceType.BuildingDef: { dataManager.buildingDefs.Add(id, (BuildingDef)data); }; break;
          case BattleTechResourceType.CastDef: { dataManager.castDefs.Add(id, (CastDef)data); }; break;
          case BattleTechResourceType.ChassisDef: { dataManager.chassisDefs.Add(id, (ChassisDef)data); }; break;
          case BattleTechResourceType.DesignMaskDef: { dataManager.designMaskDefs.Add(id, (DesignMaskDef)data); }; break;
          case BattleTechResourceType.HardpointDataDef: { dataManager.hardpointDataDefs.Add(id, (HardpointDataDef)data); }; break;
          case BattleTechResourceType.HeatSinkDef: { dataManager.heatSinkDefs.Add(id, (HeatSinkDef)data); }; break;
          case BattleTechResourceType.JumpJetDef: { dataManager.jumpJetDefs.Add(id, (JumpJetDef)data); }; break;
          case BattleTechResourceType.LanceDef: { dataManager.lanceDefs.Add(id, (LanceDef)data); }; break;
          case BattleTechResourceType.MechDef: { dataManager.mechDefs.Add(id, (MechDef)data); }; break;
          case BattleTechResourceType.MovementCapabilitiesDef: { dataManager.movementCapDefs.Add(id, (MovementCapabilitiesDef)data); }; break;
          case BattleTechResourceType.PathingCapabilitiesDef: { dataManager.pathingCapDefs.Add(id, (PathingCapabilitiesDef)data); }; break;
          case BattleTechResourceType.PilotDef: { dataManager.pilotDefs.Add(id, (PilotDef)data); }; break;
          case BattleTechResourceType.StarSystemDef: { dataManager.systemDefs.Add(id, (StarSystemDef)data); }; break;
          case BattleTechResourceType.TurretChassisDef: { dataManager.turretChassisDefs.Add(id, (TurretChassisDef)data); }; break;
          case BattleTechResourceType.UpgradeDef: { dataManager.upgradeDefs.Add(id, (UpgradeDef)data); }; break;
          case BattleTechResourceType.VehicleChassisDef: { dataManager.vehicleChassisDefs.Add(id, (VehicleChassisDef)data); }; break;
          case BattleTechResourceType.VehicleDef: { dataManager.vehicleDefs.Add(id, (VehicleDef)data); }; break;
          case BattleTechResourceType.WeaponDef: { dataManager.weaponDefs.Add(id, (WeaponDef)data); }; break;
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
    public static void AddToDataManager(DataManager dataManager, BattleTechResourceType resType, string id, IJsonTemplated data) {
      switch (resType) {
        case BattleTechResourceType.AbilityDef: AddToDataManager<AbilityDef>(dataManager, resType, id, data); break;
        case BattleTechResourceType.AmmunitionBoxDef: AddToDataManager<AmmunitionBoxDef>(dataManager, resType, id, data); break;
        case BattleTechResourceType.AmmunitionDef: AddToDataManager<AmmunitionDef>(dataManager, resType, id, data); break;
        case BattleTechResourceType.BackgroundDef: AddToDataManager<BackgroundDef>(dataManager, resType, id, data); break;
        case BattleTechResourceType.BuildingDef: AddToDataManager<BuildingDef>(dataManager, resType, id, data); break;
        case BattleTechResourceType.BaseDescriptionDef: AddToDataManager<BaseDescriptionDef>(dataManager, resType, id, data); break;
        case BattleTechResourceType.CastDef: AddToDataManager<CastDef>(dataManager, resType, id, data); break;
        case BattleTechResourceType.ChassisDef: AddToDataManager<ChassisDef>(dataManager, resType, id, data); break;
        case BattleTechResourceType.DesignMaskDef: AddToDataManager<DesignMaskDef>(dataManager, resType, id, data); break;
        case BattleTechResourceType.FactionDef: AddToDataManager<FactionDef>(dataManager, resType, id, data); break;
        case BattleTechResourceType.HardpointDataDef: AddToDataManager<HardpointDataDef>(dataManager, resType, id, data); break;
        case BattleTechResourceType.HeatSinkDef: AddToDataManager<HeatSinkDef>(dataManager, resType, id, data); break;
        case BattleTechResourceType.HeraldryDef: AddToDataManager<HeraldryDef>(dataManager, resType, id, data); break;
        case BattleTechResourceType.JumpJetDef: AddToDataManager<JumpJetDef>(dataManager, resType, id, data); break;
        case BattleTechResourceType.LanceDef: AddToDataManager<LanceDef>(dataManager, resType, id, data); break;
        case BattleTechResourceType.MechDef: AddToDataManager<MechDef>(dataManager, resType, id, data); break;
        case BattleTechResourceType.MovementCapabilitiesDef: AddToDataManager<MovementCapabilitiesDef>(dataManager, resType, id, data); break;
        case BattleTechResourceType.PathingCapabilitiesDef: AddToDataManager<PathingCapabilitiesDef>(dataManager, resType, id, data); break;
        case BattleTechResourceType.PilotDef: AddToDataManager<PilotDef>(dataManager, resType, id, data); break;
        case BattleTechResourceType.StarSystemDef: AddToDataManager<StarSystemDef>(dataManager, resType, id, data); break;
        case BattleTechResourceType.TurretChassisDef: AddToDataManager<TurretChassisDef>(dataManager, resType, id, data); break;
        case BattleTechResourceType.TurretDef: AddToDataManager<TurretDef>(dataManager, resType, id, data); break;
        case BattleTechResourceType.UpgradeDef: AddToDataManager<UpgradeDef>(dataManager, resType, id, data); break;
        case BattleTechResourceType.VehicleChassisDef: AddToDataManager<VehicleChassisDef>(dataManager, resType, id, data); break;
        case BattleTechResourceType.VehicleDef: AddToDataManager<VehicleDef>(dataManager, resType, id, data); break;
        case BattleTechResourceType.WeaponDef: AddToDataManager<WeaponDef>(dataManager, resType, id, data); break;
      }
    }
    public delegate void d_Registry_ProcessCustomFactories(object target, Dictionary<string, object> values, bool replace);
    public static d_Registry_ProcessCustomFactories i_Registry_ProcessCustomFactories = null;
    public delegate void d_Registry_ProcessCustomFactoriesNoReplace(object target, Dictionary<string, object> values);
    public static d_Registry_ProcessCustomFactoriesNoReplace i_Registry_ProcessCustomFactoriesNoReplace = null;
    public static void Registry_ProcessCustomFactories(object target, Dictionary<string, object> values, bool replace = true) {
      if (i_Registry_ProcessCustomFactories != null) { i_Registry_ProcessCustomFactories(target, values, replace); return; }
      if (i_Registry_ProcessCustomFactoriesNoReplace != null) { i_Registry_ProcessCustomFactoriesNoReplace(target, values); return; }
      {
        {
          MethodInfo method = typeof(CustomComponents.Registry).GetMethod("ProcessCustomFactories", BindingFlags.NonPublic | BindingFlags.Static);
          var parameters = method.GetParameters();
          if (parameters.Length == 3) {
            var dm = new DynamicMethod("CPProcessCustomFactories", null, new Type[] { typeof(object), typeof(Dictionary<string, object>), typeof(bool) });
            var gen = dm.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Ldarg_2);
            gen.Emit(OpCodes.Call, method);
            gen.Emit(OpCodes.Ret);
            i_Registry_ProcessCustomFactories = (d_Registry_ProcessCustomFactories)dm.CreateDelegate(typeof(d_Registry_ProcessCustomFactories));
          }else if (parameters.Length == 2) {
            var dm = new DynamicMethod("CPProcessCustomFactories", null, new Type[] { typeof(object), typeof(Dictionary<string, object>) });
            var gen = dm.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Call, method);
            gen.Emit(OpCodes.Ret);
            i_Registry_ProcessCustomFactoriesNoReplace = (d_Registry_ProcessCustomFactoriesNoReplace)dm.CreateDelegate(typeof(d_Registry_ProcessCustomFactoriesNoReplace));
          }
        }
      }
      if (i_Registry_ProcessCustomFactories != null) { i_Registry_ProcessCustomFactories(target, values, replace); return; }
      if (i_Registry_ProcessCustomFactoriesNoReplace != null) { i_Registry_ProcessCustomFactoriesNoReplace(target, values); return; }
      throw new Exception("can't find CustomComponents.Registry.ProcessCustomFactories");
    }
    public static void BuildCacheFile(d_MainManuEnableButtons p, DataManager dataManager, string cachepath) {
      Stopwatch stopwatch = new Stopwatch();
      Core.CacheLoadingInited = true;
      p.messageText = "BUILDING CACHE";
      Core.disableFromJSON = false;
      CacheData definitionsCache = null;
      if (File.Exists(cachepath)) {
        definitionsCache = MessagePackSerializer.Typeless.Deserialize(File.ReadAllBytes(cachepath)) as CacheData;
      }
      if (definitionsCache == null) {
        definitionsCache = new CacheData();
        definitionsCache.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
      } else
      if (definitionsCache.Version != Assembly.GetExecutingAssembly().GetName().Version.ToString()) {
        definitionsCache = new CacheData();
        definitionsCache.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
      }

      Dictionary<BattleTechResourceType, int> statistic = new Dictionary<BattleTechResourceType, int>();
      //FullCache fullCache = new FullCache();
      foreach (BattleTechResourceType resType in FastDataLoadHelper.prewarmTypesOrder) {
        VersionManifestEntry[] manifest = dataManager.ResourceLocator.AllEntriesOfResource(resType, false);
        foreach (VersionManifestEntry entry in manifest) {
          if (entry.IsAssetBundled) { continue; }
          if (entry.IsFileAsset == false) { continue; }
          if (entry.IsTemplate) { continue; }
          if (entry.Id.StartsWith("Gear_EngineCore_")) { continue; }
          if (File.Exists(entry.FilePath) == false) {
            Log.M?.TWL(0, "File does not exists:" + entry.FileName + ":" + entry.FilePath);
            continue;
          }
          p.allDataCount += 1;
          if (statistic.ContainsKey(resType) == false) { statistic.Add(resType, 1); } else { statistic[resType] += 1; }
        }
      }
      Log.M?.TWL(0, "Statistics:");
      List<KeyValuePair<BattleTechResourceType, int>> ordered_stat = new List<KeyValuePair<BattleTechResourceType, int>>();
      foreach (var stat in statistic) {
        ordered_stat.Add(stat);
      }
      ordered_stat.Sort((a, b) => { return a.Value.CompareTo(b.Value); });
      foreach (var stat in ordered_stat) {
        Log.M?.WL(1, stat.Key + ":" + stat.Value);
      }
      stopwatch.Start();
      //Dictionary<BattleTechResourceType, Dictionary<string, PreCacheItem>> preCache = new Dictionary<BattleTechResourceType, Dictionary<string, PreCacheItem>>();
      HashSet<CacheItem> needSerialize = new HashSet<CacheItem>();
      foreach (BattleTechResourceType resType in FastDataLoadHelper.prewarmTypesOrder) {
        VersionManifestEntry[] manifest = dataManager.ResourceLocator.AllEntriesOfResource(resType, false);
        if (definitionsCache.payload.TryGetValue(resType, out Dictionary<string, CacheItem> cacheItems) == false) {
          cacheItems = new Dictionary<string, CacheItem>();
          definitionsCache.payload.Add(resType, cacheItems);
        }
        foreach (VersionManifestEntry entry in manifest) {
          if (entry.IsAssetBundled) { continue; }
          if (entry.IsFileAsset == false) { continue; }
          if (entry.IsTemplate) { continue; }
          if (entry.id.StartsWith("Gear_EngineCore")) { continue; }
          if (File.Exists(entry.FilePath) == false) { continue; }
          Log.M?.WL(2, entry.id);
          p.curDataCount += 1;
          try {
            //string content = string.Empty;
            if (cacheItems.TryGetValue(entry.id, out CacheItem cacheItem)) {
              if (Core.Settings.UseHashedPreloading == false) {
                cacheItem.AddToDataBase(dataManager, resType, entry.id);
                continue;
              }
            }
            //using (StreamReader reader = new StreamReader(entry.FilePath)) { content = reader.ReadToEnd(); };
            string content = File.ReadAllText(entry.FilePath);
            if (cacheItem != null) {
              byte[] hash = content.SHA1();
              if (hash.SequenceEqual(cacheItem.hash)) {
                cacheItem.AddToDataBase(dataManager, resType, entry.id);
                continue;
              }
              cacheItem.hash = hash;
            } else {
              cacheItem = new CacheItem();
            }
            needSerialize.Add(cacheItem);
            cacheItems.Add(entry.Id, cacheItem);
            HashSet<jtProcGenericEx> procs = entry.FilePath.getLocalizationProcs();
            if (procs != null) {
              bool updated = CustomTranslation.Core.LocalizeString(ref content, entry.FilePath, procs);
            }
            cacheItem.originalJson = content;
            object definition = Activator.CreateInstance(FastDataLoadHelper.PrewarmDelegate.GetTypeFromBattletechResource(resType));
            cacheItem.btDefinition = definition as IJsonTemplated;
            if (cacheItem.btDefinition == null) {
              Log.M?.TWL(0, resType + " is not IJsonTemplated. definition:" + (definition == null ? "null" : "not null"));
            }
            cacheItem.btDefinition.FromJSON(content);
            cacheItem.custom = (Dictionary<string, object>)fastJSON.JSON.ToObject(content, true);
            cacheItem.registred = Core.GatherRegistredObjects(resType, entry.Id);
            cacheItem.invalidated = true;
            cacheItem.details = Serialize.SerializableFactory.getDescription(cacheItem.btDefinition);
            AddToDataManager(dataManager, resType, entry.Id, cacheItem.btDefinition);
          } catch (Exception e) {
            Log.M_Err?.TWL(0, entry.FilePath);
            Log.M_Err?.TWL(0, e.ToString(), true);
          }
        }
        //Log.M?.WL(1, "Cached:" + counter + " in cache:" + (definitionsCache.ContainsKey(resType)? definitionsCache[resType].Count: -1)+" exists:"+exists_counter);
      }
      Log.M?.TWL(0, "reading/building end: " + stopwatch.Elapsed.TotalSeconds, true);
      Core.disableFromJSON = true;
      p.messageText = "INVOKING FromJSON";
      p.allDataCount = 0;
      p.curDataCount = 0;
      foreach (var cacheItems in definitionsCache.payload)
        p.allDataCount += cacheItems.Value.Count;
      foreach (var cacheItems in definitionsCache.payload) {
        System.Diagnostics.Stopwatch fromJSONstopwatch = new System.Diagnostics.Stopwatch();
        fromJSONstopwatch.Start();
        foreach (var cacheItem in cacheItems.Value) {
          if (cacheItem.Value.invalidated) { continue; }
          cacheItem.Value.btDefinition.FromJSON(cacheItem.Value.originalJson);
          Serialize.SerializableFactory.setDescription(cacheItem.Value.btDefinition, cacheItem.Value.details);
        }
        fromJSONstopwatch.Stop();
        Log.M?.TWL(0, "FromJSON: " + (object)cacheItems.Key + ":" + (object)fromJSONstopwatch.Elapsed.TotalSeconds, true);
      }
      Core.disableFromJSON = false;

      p.messageText = "REFRESHING";
      p.allDataCount = 0; p.curDataCount = 0;
      foreach (var resType in definitionsCache.payload) {
        p.allDataCount += resType.Value.Count;
      }
      foreach (var resType in FastDataLoadHelper.prewarmTypesOrder) {
        foreach (var def in Serialize.SerializableFactory.getStorage(dataManager, resType).Values) {
          p.curDataCount += 1;
          IJsonTemplated json = def as IJsonTemplated;
          if (json == null) { continue; }
          Serialize.SerializableFactory.Refresh(json, dataManager);
        }
      }
      p.messageText = "AUTOFIXING";
      p.allDataCount = 0;
      p.curDataCount = 0;
      if (definitionsCache.payload.TryGetValue(BattleTechResourceType.MechDef, out var mechDefsCache)) {
        List<MechDef> mechDefs = new List<MechDef>();
        foreach (var mechDef in mechDefsCache) {
          MechDef mechDefBT = mechDef.Value.btDefinition as MechDef;
          if (mechDefBT == null) { Log.M?.TWL(0, mechDef.Key + " is not MechDef"); }
          mechDefs.Add(mechDefBT);
        }
        p.allDataCount = mechDefs.Count;
        CustomComponents.AutoFixer.Shared.ProcessMechDefs(mechDefs);
        foreach (var mechDef in mechDefs) {
          if (mechDef.MechTags.Contains(FastDataLoadHelper.NOAUTOFIX_TAG) == false) {
            mechDef.MechTags.Add(FastDataLoadHelper.NOAUTOFIX_TAG);
          }
        }
      }
      Log.M?.TWL(0, "autofixing end: " + stopwatch.Elapsed.TotalSeconds, true);
      p.messageText = "SERIALIZING";
      p.allDataCount = needSerialize.Count;
      p.curDataCount = 0;
      foreach (CacheItem cacheItem in needSerialize) {
        p.curDataCount += 1;
        cacheItem.definition = Serialize.SerializableFactory.Create(cacheItem.btDefinition);
      }
      if (needSerialize.Count > 0) {
        p.messageText = "WRITE TO DISK";
        p.allDataCount = 0;
        p.curDataCount = 0;
        File.WriteAllBytes(cachepath, MessagePackSerializer.Typeless.Serialize(definitionsCache));
        Log.M?.TWL(0, "write to disk " + cachepath + " ended:" + stopwatch.Elapsed.TotalSeconds, true);
      }
      Log.M?.TWL(0, "serializing end: " + needSerialize.Count + " " + stopwatch.Elapsed.TotalSeconds, true);
      dependencyLoad = null;
      loadingDeps = true;
      while (dependencyLoad == null) { Thread.Sleep(10); }
      Dictionary<BattleTech.BattleTechResourceType, List<BattleTech.VersionManifestEntry>> dependensies = new Dictionary<BattleTechResourceType, List<BattleTech.VersionManifestEntry>>();
      foreach (var loadRequests in dependencyLoad.loadRequests) {
        foreach (var loadRequest in loadRequests.loadRequests) {
          if (dependensies.TryGetValue(loadRequest.Key.BattleTechResourceType(), out var resDeps) == false) {
            resDeps = new List<VersionManifestEntry>();
            dependensies.Add(loadRequest.Key.BattleTechResourceType(), resDeps);
          }
          resDeps.Add(loadRequest.Key);
        }
      }
      Log.M?.TWL(0, "GatherDependencies:");
      foreach (var resDeps in dependensies) {
        Log.M?.WL(1, resDeps.Key + ":" + resDeps.Value.Count);
        if (FastDataLoadHelper.prewarmTypes.Contains(resDeps.Key)) {
          foreach (var dependence in resDeps.Value) {
            if (dataManager.Exists(resDeps.Key, dependence.id)) { continue; }
            Log.M?.WL(2, "not loaded " + dependence.id + " manifest exists:" + dataManager.ResourceEntryExists(resDeps.Key, dependence.id));
          }
        }
      }
      p.allDataCount = dependencyLoad.DependencyCount();
      int watchdog = 0;
      while (loadingDeps == true) {
        Thread.Sleep(10);
        int curr = 0;
        foreach (var request in dependencyLoad.loadRequests) {
          curr += request.GetCompletedRequestCount();
        };
        if (curr > p.curDataCount) { watchdog = 0; p.curDataCount = curr; } else { ++watchdog; }
        if (watchdog > 100) { break; }
      }
      foreach (var mechDef in dataManager.mechDefs) {
        if (mechDef.Value.Chassis == null) { Log.M?.TWL(0, mechDef.Key + " has no chassis"); }
        foreach (var component in mechDef.Value.inventory) {
          if (component.Def == null) { Log.M?.TWL(0, mechDef.Key + " has no component:" + component.componentDefID); }
        }
      }
      foreach (var chassisDef in dataManager.chassisDefs) {
        if (chassisDef.Value.MovementCapDef == null) { Log.M?.TWL(0, chassisDef.Key + " has no MovementCap " + chassisDef.Value.movementCapDefID + " exits:" + dataManager.Exists(BattleTechResourceType.MovementCapabilitiesDef, chassisDef.Value.movementCapDefID)); }
        if (chassisDef.Value.PathingCapDef == null) { Log.M?.TWL(0, chassisDef.Key + " has no PathingCap " + chassisDef.Value.pathingCapDefID + " exits:" + dataManager.Exists(BattleTechResourceType.PathingCapabilitiesDef, chassisDef.Value.pathingCapDefID)); }
      }
      stopwatch.Stop();
      Log.M?.TWL(0, "dependencies: " + cachepath + " " + stopwatch.Elapsed.TotalSeconds, true);
      foreach (BattleTechResourceType resType in FastDataLoadHelper.prewarmTypesOrder) {
        Log.M?.WL(1, resType + ":" + Serialize.SerializableFactory.getStorage(dataManager, resType).Count);
      }
    }

    public static DataManager.InjectedDependencyLoadRequest dependencyLoad = null;
    public static bool loadingDeps = false;
    public static DataManager dataManager = null;
    public static CacheData CreateCacheFile(MainMenu_ShowRefreshingSaves.d_MainManuEnableButtons p, DataManager dataManager) {
      System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
      p.messageText = "BUILDING CACHE";
      Log.M?.TWL(0,"Building new cache started");
      Core.disableFromJSON = false;
      Dictionary<BattleTechResourceType, int> statDict = new Dictionary<BattleTechResourceType, int>();
      foreach (BattleTechResourceType techResourceType in FastDataLoadHelper.prewarmTypesOrder) {
        statDict.Add(techResourceType, 0);
      }
      foreach (BattleTechResourceType techResourceType in FastDataLoadHelper.prewarmTypesOrder) {
        foreach (VersionManifestEntry versionManifestEntry in dataManager.ResourceLocator.AllEntriesOfResource(techResourceType)) {
          if (!versionManifestEntry.IsAssetBundled && versionManifestEntry.IsFileAsset && !versionManifestEntry.IsTemplate) {
            if (versionManifestEntry.id.StartsWith("Gear_EngineCore_")) { continue; }
            if (!File.Exists(versionManifestEntry.FilePath)) {
              Log.M?.TWL(0, "File does not exists:" + versionManifestEntry.FileName + ":" + versionManifestEntry.FilePath);
            } else {
              ++p.allDataCount;
              //if (!statDict.ContainsKey(techResourceType))
                //statDict.Add(techResourceType, 1);
              //else
              statDict[techResourceType] += 1;
            }
          }
        }
      }
      Log.M?.TWL(0, "Statistics:");
      List<KeyValuePair<BattleTechResourceType, int>> keyValuePairList = new List<KeyValuePair<BattleTechResourceType, int>>();
      foreach (KeyValuePair<BattleTechResourceType, int> keyValuePair in statDict)
        keyValuePairList.Add(keyValuePair);
      keyValuePairList.Sort((Comparison<KeyValuePair<BattleTechResourceType, int>>)((a, b) => a.Value.CompareTo(b.Value)));
      foreach (KeyValuePair<BattleTechResourceType, int> keyValuePair in keyValuePairList)
        Log.M?.WL(1, keyValuePair.Key.ToString() + ":" + (object)keyValuePair.Value);
      stopwatch.Start();

      CacheData cacheData = new MainMenu_ShowRefreshingSaves.CacheData();
      cacheData.update = true;
      cacheData.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
      Log.M?.TWL(0, "reading:"+stopwatch.Elapsed.TotalSeconds);
      foreach (BattleTechResourceType resType in FastDataLoadHelper.prewarmTypesOrder) {
        VersionManifestEntry[] versionManifestEntryArray = dataManager.ResourceLocator.AllEntriesOfResource(resType);
        Log.M?.TWL(0, resType.ToString() + ":" + versionManifestEntryArray.Length);
        if(cacheData.payload.TryGetValue(resType, out var cacheItems) == false) {
          cacheItems = new Dictionary<string, CacheItem>();
          cacheData.payload.Add(resType, cacheItems);
        }
        foreach (VersionManifestEntry entry in versionManifestEntryArray) {
          if (entry.IsAssetBundled) { continue; }
          if (entry.IsFileAsset == false) { continue; }
          if (entry.IsTemplate) { continue; }
          if (entry.id.StartsWith("Gear_EngineCore_")) { continue; }
          if (File.Exists(entry.FilePath) == false) { continue; }
          ++p.curDataCount;
          if (cacheItems.TryGetValue(entry.Id, out var cacheItem) == false) {
            cacheItem = new CacheItem();
            cacheItems.Add(entry.Id, cacheItem);
            cacheItem.invalidated = false;
          }
          try {
            string content = File.ReadAllText(entry.FilePath);//string.Empty;
            cacheItem.hash = content.SHA1();
            HashSet<jtProcGenericEx> procs = entry.FilePath.getLocalizationProcs();
            if (procs != null) {
              bool updated = CustomTranslation.Core.LocalizeString(ref content, entry.FilePath, procs);
            }
            cacheItem.originalJson = content;
            //using (StreamReader streamReader = new StreamReader(entry.FilePath))
            //json = streamReader.ReadToEnd();
            object instance = Activator.CreateInstance(FastDataLoadHelper.PrewarmDelegate.GetTypeFromBattletechResource(resType));
            IJsonTemplated jsonTemplated = instance as IJsonTemplated;
            if (jsonTemplated == null) {
              Log.M?.TWL(0, resType.ToString() + " is not IJsonTemplated. definition:" + (instance == null ? "null" : "not null"));
              continue;
            }
            bool remove_noautofix = true;
            if (jsonTemplated is MechComponentDef) {
              JObject jcontent = JObject.Parse(content);
              HBS.Collections.TagSet ComponentTags = null;
              if (jcontent["ComponentTags"] != null) {
                ComponentTags = new HBS.Collections.TagSet();
                ComponentTags.FromJSON(jcontent["ComponentTags"].ToString());
                if (ComponentTags.Contains("noautofix")) { remove_noautofix = false; }
              } else {
                ComponentTags = new HBS.Collections.TagSet();
              }
              if (remove_noautofix) {
                ComponentTags.Add("noautofix");
                jcontent["ComponentTags"] = JObject.Parse(ComponentTags.ToJSON());
                content = jcontent.ToString(Formatting.Indented);
              }
            }
            jsonTemplated.FromJSON(content);
            if (jsonTemplated is MechComponentDef mechComponent) {
              if(remove_noautofix)mechComponent.ComponentTags.Remove("noautofix");
            }
            Dictionary<string, object> cust = (Dictionary<string, object>)fastJSON.JSON.ToObject(content, true);
            //MainMenu_ShowRefreshingSaves.PreCacheItem preCacheItem = new PreCacheItem(jsonTemplated, cust, json, new byte[0]);
            if(cust.TryGetValue("Description",out object Description)) {
              cacheItem.details = Serialize.SerializableFactory.extractDetails(Description);
            }
            cacheItem.btDefinition = jsonTemplated;
            cacheItem.originalJson = content;
            cacheItem.custom = cust;
            MainMenu_ShowRefreshingSaves.AddToDataManager(dataManager, resType, entry.Id, jsonTemplated);
          } catch (Exception ex) {
            Log.M_Err?.TWL(0, entry.FilePath);
            Log.M_Err?.TWL(0, ex.ToString(), true);
          }          
        }
      }
      Log.M?.TWL(0, "refreshing:" + stopwatch.Elapsed.TotalSeconds);
      p.messageText = "REFRESHING";
      p.allDataCount = 0;
      p.curDataCount = 0;
      foreach (var cacheItems in cacheData.payload) {
        p.allDataCount += cacheItems.Value.Count;
      }
      foreach (var cacheItems in cacheData.payload) {
        foreach (var cacheItem in cacheItems.Value) {
          Serialize.SerializableFactory.Refresh(cacheItem.Value.btDefinition, dataManager);
          ++p.curDataCount;
        }
      }
      Log.M?.TWL(0, "autofixing:" + stopwatch.Elapsed.TotalSeconds);
      p.messageText = "AUTOFIXING";
      p.allDataCount = 0;
      p.curDataCount = 0;
      Log.M?.WL(0, "before");
      if (cacheData.payload.TryGetValue(BattleTechResourceType.MechDef, out var mechDefinitions)) {
        List<BattleTech.MechDef> mechDefs = new List<BattleTech.MechDef>();
        foreach (var mechDefinition in mechDefinitions) {
          BattleTech.MechDef definition = mechDefinition.Value.btDefinition as BattleTech.MechDef;
          if (definition == null) {
            Log.M?.TWL(0, mechDefinition.Key + " is not MechDef");
            continue;
          }
          if (definition.MechTags.Contains(FastDataLoadHelper.NOAUTOFIX_TAG)) { continue; }
          if (definition.MechTags.Contains(FastDataLoadHelper.FAKE_VEHICLE_TAG)) { continue; }
          mechDefs.Add(definition);
          if(definition.Description.Id == "mechdef_apollo_APL-1M") {
            Log.M?.WL(1,$"{definition.Description.Id}");
            foreach(var cmp in definition.Inventory) {
              Log.M?.WL(2, $"{cmp.ComponentDefID}:{cmp.LocalGUID}");
            }
          }
        }
        p.allDataCount = mechDefs.Count;
        AutoFixer.Shared.ProcessMechDefs(mechDefs);
        Log.M?.WL(0, "after");
        foreach (BattleTech.MechDef mechDef in mechDefs) {
          if (mechDef.Description.Id == "mechdef_apollo_APL-1M") {
            Log.M?.WL(1, $"{mechDef.Description.Id}");
            foreach (var cmp in mechDef.Inventory) {
              Log.M?.WL(2, $"{cmp.ComponentDefID}:{cmp.LocalGUID}");
            }
          }
          if (!mechDef.MechTags.Contains(FastDataLoadHelper.NOAUTOFIX_TAG))
            mechDef.MechTags.Add(FastDataLoadHelper.NOAUTOFIX_TAG);
        }
      }
      Log.M?.TWL(0, "serializing:" + stopwatch.Elapsed.TotalSeconds);
      p.messageText = "SERIALIZING";
      p.allDataCount = 0;
      p.curDataCount = 0;
      foreach (var cacheItems in cacheData.payload) {
        p.allDataCount += cacheItems.Value.Count;
      }
      foreach (var cacheItems in cacheData.payload) {
        foreach (var cacheItem in cacheItems.Value) {
          Serialize.SerializableFactory.setDescription(cacheItem.Value.btDefinition, cacheItem.Value.details);
          //MainMenu_ShowRefreshingSaves.CacheItem cacheItem = new CacheItem(keyValuePair.Value.definition, keyValuePair.Value.custom, Core.GatherRegistredObjects(techResourceType, keyValuePair.Key), keyValuePair.Value.originalJson);
          cacheItem.Value.definition = Serialize.SerializableFactory.Create(cacheItem.Value.btDefinition);
          cacheItem.Value.registred = Core.GatherRegistredObjects(cacheItems.Key, cacheItem.Key);
          ++p.curDataCount;
        }
      }
      foreach (BattleTechResourceType resType in FastDataLoadHelper.prewarmTypesOrder) {
        Serialize.SerializableFactory.getStorage(dataManager, resType).Clear();
      }
      stopwatch.Stop();
      Log.M?.TWL(0, "finished:" + stopwatch.Elapsed.TotalSeconds);
      return cacheData;
    }
    public static bool PLEASE_RESTART = false;
    public static bool PLEASE_DISABLE = false;
    public static void ReadCacheFile(MainMenu_ShowRefreshingSaves.d_MainManuEnableButtons p, DataManager dataManager, string cachepath) {
      Core.CacheLoadingInited = true;
      p.allDataCount = 0;
      p.curDataCount = 0;
      p.messageText = "READING CACHE";
      System.Diagnostics.Stopwatch stopwatch1 = new System.Diagnostics.Stopwatch();
      stopwatch1.Start();
      CacheData cacheData = null;
      PLEASE_RESTART = false;
      PLEASE_DISABLE = false;
      if (!File.Exists(cachepath)) {
        cacheData = MainMenu_ShowRefreshingSaves.CreateCacheFile(p, dataManager);
        PLEASE_RESTART = true;
      }
      if(cacheData == null) {
        cacheData = MessagePackSerializer.Typeless.Deserialize(File.ReadAllBytes(cachepath)) as CacheData;
        if(cacheData == null) {
          cacheData = MainMenu_ShowRefreshingSaves.CreateCacheFile(p, dataManager);
          PLEASE_RESTART = true;
        }
      }
      if(cacheData.Version != Assembly.GetExecutingAssembly().GetName().Version.ToString()) {
        cacheData = MainMenu_ShowRefreshingSaves.CreateCacheFile(p, dataManager);
        PLEASE_RESTART = true;
      }
      Dictionary<BattleTechResourceType, int> statistic = new Dictionary<BattleTechResourceType, int>();
      foreach (var cacheItems in cacheData.payload) {
        p.allDataCount += cacheItems.Value.Count;
        statistic.Add(cacheItems.Key, cacheItems.Value.Count);
      }
      Log.M?.TWL(0, "Statistics:");
      List<KeyValuePair<BattleTechResourceType, int>> stat_list = new List<KeyValuePair<BattleTechResourceType, int>>();
      foreach (var stat in statistic) { stat_list.Add(stat); }        
      stat_list.Sort((Comparison<KeyValuePair<BattleTechResourceType, int>>)((a, b) => a.Value.CompareTo(b.Value)));
      foreach (var stat in stat_list) { Log.M?.WL(1, stat.Key.ToString() + ":" + stat.Value); }
      Dictionary<BattleTechResourceType, Dictionary<string, VersionManifestEntry>> manifest = new Dictionary<BattleTechResourceType, Dictionary<string, VersionManifestEntry>>();
      if (Core.Settings.UseHashedPreloading) {
        foreach (var resType in FastDataLoadHelper.prewarmTypesOrder) {
          if (manifest.TryGetValue(resType, out var entries) == false) {
            entries = new Dictionary<string, VersionManifestEntry>();
            manifest.Add(resType, entries);
          }
          VersionManifestEntry[] versionManifestEntryArray = dataManager.ResourceLocator.AllEntriesOfResource(resType);
          foreach (var entry in versionManifestEntryArray) {
            entries.Add(entry.id, entry);
          }
        }
      }
      //Dictionary<BattleTechResourceType, List<MainMenu_ShowRefreshingSaves.PostCacheItem>> dictionary2 = new Dictionary<BattleTechResourceType, List<MainMenu_ShowRefreshingSaves.PostCacheItem>>();
      foreach (var cacheItems in cacheData.payload) {
        foreach (var cacheItem in cacheItems.Value) {
          ++p.curDataCount;
          if (!dataManager.Exists(cacheItems.Key, cacheItem.Key)) {
            try {
              if (Core.Settings.UseHashedPreloading) {
                if(manifest.TryGetValue(cacheItems.Key, out var entries)) {
                  if(entries.TryGetValue(cacheItem.Key, out var entry)) {
                    string content = File.ReadAllText(entry.FilePath);
                    byte[] hash = content.SHA1();
                    if(hash.SequenceEqual(cacheItem.Value.hash) == false) {
                      cacheData.update = true;
                      cacheItem.Value.invalidated = true;
                      cacheItem.Value.hash = hash;
                      HashSet<jtProcGenericEx> procs = entry.FilePath.getLocalizationProcs();
                      if (procs != null) {
                        bool updated = CustomTranslation.Core.LocalizeString(ref content, entry.FilePath, procs);
                      }
                      cacheItem.Value.originalJson = content;
                      //using (StreamReader streamReader = new StreamReader(entry.FilePath))
                      //json = streamReader.ReadToEnd();
                      object instance = Activator.CreateInstance(FastDataLoadHelper.PrewarmDelegate.GetTypeFromBattletechResource(cacheItems.Key));
                      IJsonTemplated jsonTemplated = instance as IJsonTemplated;
                      if (jsonTemplated == null) {
                        Log.M?.TWL(0, cacheItems.Key.ToString() + " is not IJsonTemplated. definition:" + (instance == null ? "null" : "not null"));
                        continue;
                      }
                      jsonTemplated.FromJSON(content);
                      Dictionary<string, object> cust = (Dictionary<string, object>)fastJSON.JSON.ToObject(content, true);
                      //MainMenu_ShowRefreshingSaves.PreCacheItem preCacheItem = new PreCacheItem(jsonTemplated, cust, json, new byte[0]);
                      cacheItem.Value.btDefinition = jsonTemplated;
                      cacheItem.Value.originalJson = content;
                      cacheItem.Value.custom = cust;
                      cacheItem.Value.details = Serialize.SerializableFactory.getDescription(cacheItem.Value.btDefinition);
                      MainMenu_ShowRefreshingSaves.AddToDataManager(dataManager, cacheItems.Key, entry.Id, jsonTemplated);
                      continue;
                    }
                  }
                }
              }
              cacheItem.Value.invalidated = false;
              cacheItem.Value.btDefinition = cacheItem.Value.serializable.toBT(dataManager) as IJsonTemplated;
              //cacheItem.Value.details = Serialize.SerializableFactory.getDescription(cacheItem.Value.btDefinition);
              MainMenu_ShowRefreshingSaves.Registry_ProcessCustomFactories(cacheItem.Value.btDefinition, cacheItem.Value.custom);
              MainMenu_ShowRefreshingSaves.AddToDataManager(dataManager, cacheItems.Key, cacheItem.Key, cacheItem.Value.btDefinition);
              Core.registerDeserializedObjects(cacheItems.Key, cacheItem.Key, cacheItem.Value.registred);
            } catch (Exception ex) {
              Log.M_Err?.TWL(0, cacheItems.Key.ToString() + ":" + cacheItem.Key);
              Log.M_Err?.WL(0, ex.ToString(), true);
            }
          }
        }
      }
      Log.M?.TWL(0, "Reading cache ended: " + cachepath + " " + (object)stopwatch1.Elapsed.TotalSeconds, true);
      Core.disableFromJSON = true;
      p.messageText = "INVOKING FromJSON";
      p.allDataCount = 0;
      p.curDataCount = 0;
      foreach (var cacheItems in cacheData.payload) {
        p.allDataCount += cacheItems.Value.Count;
      }
      foreach (var cacheItems in cacheData.payload) {
        System.Diagnostics.Stopwatch fromJSONStopwatch = new System.Diagnostics.Stopwatch();
        fromJSONStopwatch.Start();
        foreach (var cacheItem in cacheItems.Value) {
          if (cacheItem.Value.invalidated) { continue; }
          if (cacheItem.Value.btDefinition == null) { continue; }
          cacheItem.Value.btDefinition.FromJSON(cacheItem.Value.originalJson);
          //Log.M?.WL(0, "FromJSON "+ cacheItem.Value.btDefinition.GetType().Name);
          //if(cacheItem.Value.btDefinition is MechComponentDef mechComponent) {
          //  Log.M?.WL(1, "ID:" + mechComponent.Description.Id);
          //  foreach(ICustom custom in mechComponent.GetComponents<ICustom>()) {
          //    Log.M?.WL(1, "Custom:" + custom.GetType().Name);
          //  }
          //}
          
          //Serialize.SerializableFactory.setDescription(cacheItem.Value.btDefinition, cacheItem.Value.details);
        }
        fromJSONStopwatch.Stop();
        Log.M?.TWL(0, "FromJSON: " + (object)cacheItems.Key + ":" + (object)fromJSONStopwatch.Elapsed.TotalSeconds, true);
      }
      Core.disableFromJSON = false;
      p.messageText = "REFRESHING";
      p.allDataCount = 0;
      p.curDataCount = 0;
      foreach (KeyValuePair<BattleTechResourceType, Dictionary<string, MainMenu_ShowRefreshingSaves.CacheItem>> keyValuePair in cacheData.payload)
        p.allDataCount += keyValuePair.Value.Count;
      foreach (BattleTechResourceType resType in FastDataLoadHelper.prewarmTypesOrder) {
        foreach (object obj in Serialize.SerializableFactory.getStorage(dataManager, resType).Values) {
          ++p.curDataCount;
          if (obj is IJsonTemplated source)
            Serialize.SerializableFactory.Refresh(source, dataManager);
        }
      }
      p.messageText = "AUTOFIXING";
      p.allDataCount = 0;
      p.curDataCount = 0;
      Log.M?.TWL(0, "invoking FromJSON ended:" + (object)stopwatch1.Elapsed.TotalSeconds, true);
      if (cacheData.payload.TryGetValue(BattleTechResourceType.MechDef, out var mechDefinitions)) {
        List<BattleTech.MechDef> mechDefs = new List<BattleTech.MechDef>();
        Log.M?.WL(0, "before autofixing");
        foreach (var mechDefinition in mechDefinitions) {
          BattleTech.MechDef definition = mechDefinition.Value.btDefinition as BattleTech.MechDef;
          if (definition == null) { continue; }
          if (definition.Description.Id == "mechdef_apollo_APL-1M") {
            Log.M?.WL(1, $"{definition.Description.Id}");
            foreach (var cmp in definition.Inventory) {
              Log.M?.WL(2, $"{cmp.ComponentDefID}:{cmp.LocalGUID}");
            }
          }
          if (definition.MechTags.Contains(FastDataLoadHelper.NOAUTOFIX_TAG)) { continue; }
          if (definition.MechTags.Contains(FastDataLoadHelper.FAKE_VEHICLE_TAG)) { continue; }
          mechDefs.Add(definition);
        }
        p.allDataCount = mechDefs.Count;
        AutoFixer.Shared.ProcessMechDefs(mechDefs);
        Log.M?.WL(0,"after autofixing");
        foreach(var definition in mechDefs) {
          if (definition.Description.Id == "mechdef_apollo_APL-1M") {
            Log.M?.WL(1, $"{definition.Description.Id}");
            foreach (var cmp in definition.Inventory) {
              Log.M?.WL(2, $"{cmp.ComponentDefID}:{cmp.LocalGUID}");
            }
          }
        }
      }
      Log.M?.TWL(0, "autofixing ended:" + (object)stopwatch1.Elapsed.TotalSeconds, true);
      p.messageText = "Gather Dependencies";
      p.allDataCount = 0;
      p.curDataCount = 0;
      foreach (var cacheItems in cacheData.payload)
        p.allDataCount += cacheItems.Value.Count;
      MainMenu_ShowRefreshingSaves.loadingDeps = true;
      while (MainMenu_ShowRefreshingSaves.dependencyLoad == null)
        Thread.Sleep(10);
      Dictionary<BattleTechResourceType, List<VersionManifestEntry>> loadingDeps = new Dictionary<BattleTechResourceType, List<VersionManifestEntry>>();
      foreach (LoadRequest loadRequests in MainMenu_ShowRefreshingSaves.dependencyLoad.loadRequests) {
        foreach (var loadRequest in loadRequests.loadRequests) {
          List<VersionManifestEntry> versionManifestEntryList;
          if (!loadingDeps.TryGetValue(loadRequest.Key.BattleTechResourceType(), out versionManifestEntryList)) {
            versionManifestEntryList = new List<VersionManifestEntry>();
            loadingDeps.Add(loadRequest.Key.BattleTechResourceType(), versionManifestEntryList);
          }
          versionManifestEntryList.Add(loadRequest.Key);
        }
      }
      Log.M?.TWL(0, "GatherDependencies:");
      foreach (var loadRequests in loadingDeps) {
        Log.M?.WL(1, loadRequests.Key.ToString() + ":" + loadRequests.Value.Count);
        if (FastDataLoadHelper.prewarmTypes.Contains(loadRequests.Key)) {
          foreach (VersionManifestEntry versionManifestEntry in loadRequests.Value) {
            if (!dataManager.Exists(loadRequests.Key, versionManifestEntry.id))
              Log.M?.WL(2, "not loaded " + versionManifestEntry.id + " manifest exists:" + dataManager.ResourceEntryExists(loadRequests.Key, versionManifestEntry.id).ToString());
          }
        }
      }
      p.allDataCount = MainMenu_ShowRefreshingSaves.dependencyLoad.DependencyCount();
      int watchdog = 0;
      while (MainMenu_ShowRefreshingSaves.loadingDeps) {
        Thread.Sleep(10);
        int curCount = 0;
        foreach (LoadRequest loadRequest in MainMenu_ShowRefreshingSaves.dependencyLoad.loadRequests)
          curCount += loadRequest.GetCompletedRequestCount();
        if (curCount > p.curDataCount) { watchdog = 0; p.curDataCount = curCount; } else { ++watchdog; }
        if (watchdog > 100) { break; }
      }
      //throw new Exception("test exception");
      if (cacheData.update) {
        Log.M?.TWL(0, "serializing:" + stopwatch1.Elapsed.TotalSeconds);
        p.messageText = "SERIALIZING";
        p.allDataCount = 0;
        p.curDataCount = 0;
        foreach (var cacheItems in cacheData.payload) {
          p.allDataCount += cacheItems.Value.Count;
        }
        foreach (var cacheItems in cacheData.payload) {
          foreach (var cacheItem in cacheItems.Value) {
            ++p.curDataCount;
            if (cacheItem.Value.invalidated == false) { continue; }
            cacheItem.Value.definition = Serialize.SerializableFactory.Create(cacheItem.Value.btDefinition);
            cacheItem.Value.registred = Core.GatherRegistredObjects(cacheItems.Key, cacheItem.Key);
          }
        }
        Log.M?.TWL(0, "writing to disk:" + stopwatch1.Elapsed.TotalSeconds);
        p.messageText = "WRITING TO DISK";
        p.allDataCount = 0;
        p.curDataCount = 0;
        File.WriteAllBytes(cachepath, MessagePackSerializer.Typeless.Serialize(cacheData));
      }
      stopwatch1.Stop();
      Log.M?.TWL(0, "finish: " + cachepath + " " + (object)stopwatch1.Elapsed.TotalSeconds, true);
    }

    //public static void ReadCacheFile(d_MainManuEnableButtons p, DataManager dataManager, string cachepath) {
    //  Core.CacheLoadingInited = true;
    //  p.allDataCount = 0;
    //  p.curDataCount = 0;
    //  p.messageText = "READING CACHE";
    //  Stopwatch stopwatch = new Stopwatch();
    //  stopwatch.Start();
    //  //if (File.Exists(cachepath) == false) { CreateCacheFile(p, dataManager, cachepath); }
    //  //if (File.Exists(cachepath) == false) { return; }
    //  //CacheData definitionsCache = MessagePackSerializer.Typeless.Deserialize(File.ReadAllBytes(cachepath)) as CacheData;
    //  //if(definitionsCache.Version != Assembly.GetExecutingAssembly().GetName().Version.ToString()) {
    //  //  CreateCacheFile(p, dataManager, cachepath);
    //  //}
    //  //if (File.Exists(cachepath) == false) { return; }
    //  CacheData definitionsCache = null;// BuildCacheFile(p, dataManager, cachepath);
    //  Dictionary<BattleTechResourceType, int> statistic = new Dictionary<BattleTechResourceType, int>();
    //  foreach (var resCache in definitionsCache.payload) {
    //    p.allDataCount += resCache.Value.Count;
    //    statistic.Add(resCache.Key, resCache.Value.Count);
    //  }
    //  Log.M?.TWL(0, "Statistics:");
    //  List<KeyValuePair<BattleTechResourceType, int>> ordered_stat = new List<KeyValuePair<BattleTechResourceType, int>>();
    //  foreach (var stat in statistic) {
    //    ordered_stat.Add(stat);
    //  }
    //  ordered_stat.Sort((a, b) => { return a.Value.CompareTo(b.Value); });
    //  foreach (var stat in ordered_stat) {
    //    Log.M?.WL(1, stat.Key + ":" + stat.Value);
    //  }
    //  Dictionary<BattleTechResourceType, List<PostCacheItem>> fromJSON = new Dictionary<BattleTechResourceType, List<PostCacheItem>>();
    //  //string dumpDir = Path.Combine(Core.Settings.directory, "dump");
    //  //if (Directory.Exists(dumpDir) == false) { Directory.CreateDirectory(dumpDir); };
    //  foreach (var resCache in definitionsCache.payload) {
    //    foreach (var resItem in resCache.Value) {
    //      p.curDataCount += 1;
    //      if (dataManager.Exists(resCache.Key, resItem.Key)) { continue; }
    //      try {
    //        Serialize.ISerializable definition = resItem.Value.definition as Serialize.ISerializable;
    //        if (definition == null) {
    //          Log.M?.TWL(0, resCache.Key + ":" + resItem.Key + " is not ISerializable " + resItem.Value.GetType());
    //          continue;
    //        }
    //        IJsonTemplated def = definition.toBT(dataManager) as IJsonTemplated;
    //        if (def == null) {
    //          Log.M?.TWL(0, resCache.Key + ":" + resItem.Key + " is not IJsonTemplated");
    //          continue;
    //        }
    //        if (fromJSON.TryGetValue(resCache.Key, out List<PostCacheItem> fromJSONlist) == false) {
    //          fromJSONlist = new List<PostCacheItem>();
    //          fromJSON.Add(resCache.Key, fromJSONlist);
    //        }
    //        fromJSONlist.Add(new PostCacheItem(resItem.Key, def, resItem.Value.originalJson, Serialize.SerializableFactory.getDescription(def)));
    //        Registry_ProcessCustomFactories(def, resItem.Value.custom);
    //        AddToDataManager(dataManager, resCache.Key, resItem.Key, def);
    //        Core.registerDeserializedObjects(resCache.Key, resItem.Key, resItem.Value.registred);
    //        //string itemDumpDir = Path.Combine(dumpDir, resCache.Key.ToString(), resItem.Key);
    //        //if (Directory.Exists(itemDumpDir) == false) { Directory.CreateDirectory(itemDumpDir); };
    //      } catch (Exception e) {
    //        Log.M_Err?.TWL(0, resCache.Key + ":" + resItem.Key);
    //        Log.M_Err?.WL(0, e.ToString(), true);
    //      }
    //    }
    //  }
    //  Log.M?.TWL(0, "Reading cache ended: " + cachepath + " " + stopwatch.Elapsed.TotalSeconds, true);
    //  Core.disableFromJSON = true;
    //  p.messageText = "INVOKING FromJSON";
    //  p.allDataCount = 0;
    //  p.curDataCount = 0;
    //  foreach (var fromJSONlist in fromJSON) {
    //    p.allDataCount += fromJSONlist.Value.Count;
    //  }
    //  foreach (var fromJSONlist in fromJSON) {
    //    Stopwatch fromJSONsw = new Stopwatch();
    //    fromJSONsw.Start();
    //    foreach (var def in fromJSONlist.Value) {
    //      def.definition.FromJSON(def.originalJson);
    //      Serialize.SerializableFactory.setDescription(def.definition, def.details);
    //    }
    //    fromJSONsw.Stop();
    //    Log.M?.TWL(0, "FromJSON: " + fromJSONlist.Key + ":" + fromJSONsw.Elapsed.TotalSeconds, true);
    //  }
    //  Core.disableFromJSON = false;
    //  p.messageText = "REFRESHING";
    //  p.allDataCount = 0; p.curDataCount = 0;
    //  foreach (var resType in definitionsCache.payload) {
    //    p.allDataCount += resType.Value.Count;
    //  }
    //  foreach (var resType in FastDataLoadHelper.prewarmTypesOrder) {
    //    foreach (var def in Serialize.SerializableFactory.getStorage(dataManager, resType).Values) {
    //      p.curDataCount += 1;
    //      IJsonTemplated json = def as IJsonTemplated;
    //      if (json == null) { continue; }
    //      Serialize.SerializableFactory.Refresh(json, dataManager);
    //    }
    //  }
    //  p.messageText = "AUTOFIXING";
    //  p.allDataCount = 0;
    //  p.curDataCount = 0;
    //  Log.M?.TWL(0, "invoking FromJSON ended:" + stopwatch.Elapsed.TotalSeconds, true);
    //  if (fromJSON.TryGetValue(BattleTechResourceType.MechDef, out var mechDefsCache)) {
    //    List<MechDef> mechDefs = new List<MechDef>();
    //    foreach (var mechDef in mechDefsCache) {
    //      MechDef mechDefBT = mechDef.definition as MechDef;
    //      mechDefs.Add(mechDefBT);
    //    }
    //    p.allDataCount = mechDefs.Count;
    //    CustomComponents.AutoFixer.Shared.FixMechDef(mechDefs);
    //  }
    //  Log.M?.TWL(0, "autofixing ended:" + stopwatch.Elapsed.TotalSeconds, true);
    //  p.messageText = "Gather Dependencies";
    //  p.allDataCount = 0; p.curDataCount = 0;
    //  //foreach (var resType in definitionsCache.payload) {
    //  //  p.allDataCount += resType.Value.Count;
    //  //}
    //  loadingDeps = true;
    //  while (dependencyLoad == null) { Thread.Sleep(10); }
    //  Dictionary<BattleTech.BattleTechResourceType, List<BattleTech.VersionManifestEntry>> dependensies = new Dictionary<BattleTechResourceType, List<BattleTech.VersionManifestEntry>>();
    //  foreach (var loadRequests in dependencyLoad.loadRequests) {
    //    foreach (var loadRequest in loadRequests.loadRequests) {
    //      if (dependensies.TryGetValue(loadRequest.Key.BattleTechResourceType(), out var resDeps) == false) {
    //        resDeps = new List<VersionManifestEntry>();
    //        dependensies.Add(loadRequest.Key.BattleTechResourceType(), resDeps);
    //      }
    //      resDeps.Add(loadRequest.Key);
    //    }
    //  }
    //  Log.M?.TWL(0, "GatherDependencies:");
    //  foreach (var resDeps in dependensies) {
    //    Log.M?.WL(1, resDeps.Key + ":" + resDeps.Value.Count);
    //    if (FastDataLoadHelper.prewarmTypes.Contains(resDeps.Key)) {
    //      foreach (var dependence in resDeps.Value) {
    //        if (dataManager.Exists(resDeps.Key, dependence.id)) { continue; }
    //        Log.M?.WL(2, "not loaded " + dependence.id + " manifest exists:" + dataManager.ResourceEntryExists(resDeps.Key, dependence.id));
    //      }
    //    }
    //  }
    //  p.allDataCount = dependencyLoad.DependencyCount();
    //  int watchdog = 0;
    //  while (loadingDeps == true) {
    //    Thread.Sleep(10);
    //    int curr = 0;
    //    foreach (var request in dependencyLoad.loadRequests) {
    //      curr += request.GetCompletedRequestCount();
    //    };
    //    if (curr > p.curDataCount) { watchdog = 0; p.curDataCount = curr; } else { ++watchdog; }
    //    if (watchdog > 100) { break; }
    //  }
    //  foreach (var mechDef in dataManager.mechDefs) {
    //    if (mechDef.Value.Chassis == null) { Log.M?.TWL(0, mechDef.Key + " has no chassis"); }
    //    foreach (var component in mechDef.Value.inventory) {
    //      if (component.Def == null) { Log.M?.TWL(0, mechDef.Key + " has no component:" + component.componentDefID); }
    //    }
    //  }
    //  foreach (var chassisDef in dataManager.chassisDefs) {
    //    if (chassisDef.Value.MovementCapDef == null) { Log.M?.TWL(0, chassisDef.Key + " has no MovementCap " + chassisDef.Value.movementCapDefID + " exits:" + dataManager.Exists(BattleTechResourceType.MovementCapabilitiesDef, chassisDef.Value.movementCapDefID)); }
    //    if (chassisDef.Value.PathingCapDef == null) { Log.M?.TWL(0, chassisDef.Key + " has no PathingCap " + chassisDef.Value.pathingCapDefID + " exits:" + dataManager.Exists(BattleTechResourceType.PathingCapabilitiesDef, chassisDef.Value.pathingCapDefID)); }
    //  }
    //  stopwatch.Stop();
    //  Log.M?.TWL(0, "dependencies: " + cachepath + " " + stopwatch.Elapsed.TotalSeconds, true);
    //}
    private static string f_CachePath = string.Empty;
    public static string CachePath {
      get {
        if (string.IsNullOrEmpty(f_CachePath) == false) { return f_CachePath; }
        f_CachePath = SearchCacheFolder();
        return f_CachePath;
      }
    }
    public static string SearchCacheFolder() {
      string cur_dir = Core.Settings.directory;
      while(string.IsNullOrEmpty(cur_dir) == false) {
        string cache_dir = Path.Combine(cur_dir, ".modtek");
        if (Directory.Exists(cache_dir)) { return Path.Combine(cache_dir, "msgpackcache.bin"); }
        cur_dir = Path.GetDirectoryName(cur_dir);
      }
      return string.Empty;
    }
    public static void ResetCache() {
      string cachepath = CachePath;
      Log.M?.TWL(0, "Reset cache "+ cachepath, true);
      if (File.Exists(cachepath)) {
        Log.M?.WL(0, cachepath + " deleting", true);
        File.Delete(cachepath);
      }
      cachepath = Path.Combine(Core.Settings.savesdirectory, "msgpackcache.bin");
      if (File.Exists(cachepath)) {
        Log.M?.WL(0, cachepath + " deleting", true);
        File.Delete(cachepath);
      }
    }
    public static void DoPrewarm(object param) {
      d_MainManuEnableButtons p = param as d_MainManuEnableButtons;
      Thread.Sleep(Core.Settings.ModTekIndexingDelay);
      Log.M?.TWL(0, "PrewarmTest started " + (p == null ? "null" : "not null"), true);
      Stopwatch stopwatch = new Stopwatch();
      
      try {
        if (p == null) { return; }
        string cachepath = CachePath;
        if (Core.Settings.UseHashedPreloading) {
          cachepath = Path.Combine(Core.Settings.savesdirectory, "msgpackcache.bin");
        }
        try {
          dataManager = SceneSingletonBehavior<DataManagerUnityInstance>.Instance.DataManager;
          if (Core.Settings.UseFastPreloading) {
            ReadCacheFile(p, dataManager, cachepath);
          }
        } catch (Exception e) {
          PLEASE_DISABLE = true;
          Log.M?.TWL(0, "Prewarm ended warning", true);
          Log.M?.TWL(0, e.ToString(), true);
        }
      } catch (Exception e) {
        PLEASE_DISABLE = true;
        Log.M?.TWL(0, "Prewarm ended crit", true);
        Log.M?.TWL(0, e.ToString(), true);
      }
      Log.M?.TWL(0, "Prewarm ended success:" + stopwatch.Elapsed.TotalSeconds, true);
      p.PrewarmEnded = true;
    }
    public class PrewarmEndMessager : MonoBehaviour {
      public d_MainManuEnableButtons EnableButtonsDelegate { get; set; }
      public string text { get; set; } = string.Empty;
      public int WaitingForStart { get; set; } = 0;
      public int WaitingForSaves { get; set; } = 0;
      public GameObject refreshingSavesSpinner { get; set; } = null;
      public void addtoJson(JToken dump, AssetBundle bundle, bool is_async) {
        dump["dump"] = true;
        JArray content = dump["content"] as JArray;
        //Log.M?.WL(0, $"{bundle.name} {(is_async ? "loaded":"existing")}");
        foreach (var name in bundle.GetAllAssetNames()) {
          //Log.M?.WL(1, $"{go.name}");
          content.Add(name);
        }
        JArray manifest = dump["manifest"] as JArray;
        JArray manifest_existing = dump["manifest_existing"] as JArray;
        JArray manifest_missing = dump["manifest_missing"] as JArray;
        foreach(var name in manifest) {
          var obj = bundle.LoadAsset((string)name);
          if (obj == null) { manifest_missing.Add(name); } else { manifest_existing.Add(name); }
        }
        //Log.M?.WL(0, $"", true);
      }
      public bool is_requesting_assemblies { get; set; } = false;
      public void Awake() {
        is_requesting_assemblies = false;
      }
      public void GatherAssetsBundlesInfo() {
        if (is_requesting_assemblies) { return; }
        Log.M?.TWL(0, "LOADING ALL ASSET BUNDLES");
        is_requesting_assemblies = true;
        EnableButtonsDelegate.refreshingSavesPercentageMessage.SetText("DUMP ALL ASSETBUNDLES");
        JObject dump = new JObject();
        var assetsManifest = UnityGameInstance.BattleTechGame.DataManager.ResourceLocator.AllEntriesOfResource(BattleTechResourceType.AssetBundle);
        int counter = 0;
        foreach (var assetManifest in assetsManifest) {
          if (File.Exists(assetManifest.FilePath) == false) {
            Log.M?.WL(0, $"{assetManifest.FilePath} does not exists");
            continue;
          }
          if (assetManifest.FilePath.EndsWith("assetbundles-info")) { continue; }
          if (assetManifest.FilePath.EndsWith("computeshaders")) { continue; }
          if (assetManifest.FilePath.EndsWith("shaders")) { continue; }
          dump[assetManifest.Id] = new JObject();
          dump[assetManifest.Id]["name"] = assetManifest.Id;
          dump[assetManifest.Id]["filename"] = assetManifest.FilePath;
          dump[assetManifest.Id]["dump"] = false;
          dump[assetManifest.Id]["content"] = new JArray();
          dump[assetManifest.Id]["manifest"] = new JArray();
          dump[assetManifest.Id]["manifest_missing"] = new JArray();
          dump[assetManifest.Id]["manifest_existing"] = new JArray();
          ++counter;
        }
        var prefabsManifest = UnityGameInstance.BattleTechGame.DataManager.ResourceLocator.AllEntriesOfResource(BattleTechResourceType.Prefab);
        foreach (var prefabManifest in prefabsManifest) {
          if (prefabManifest.IsAssetBundled == false) { continue; }
          var assetJson = dump[prefabManifest.AssetBundleName];
          if (assetJson == null) { continue; }
          JArray manifest = assetJson["manifest"] as JArray;
          manifest.Add(prefabManifest.Id);
        }
        this.StartCoroutine(this.GatherAssetsBundlesInfoRoutine(dump, counter));
      }
      public IEnumerator GatherAssetsBundlesInfoRoutine(JObject dump, int amount) {
        int counter = 0;
        foreach (var assetManifest in dump) {
          yield return null;
          string id = assetManifest.Key;
          bool is_have_dump = (bool)assetManifest.Value["dump"];
          if (is_have_dump) { continue; }
          if (UnityGameInstance.BattleTechGame.DataManager.AssetBundleManager.IsBundleLoaded(id)) {
            var assetbundle = UnityGameInstance.BattleTechGame.DataManager.AssetBundleManager.GetLoadedAssetBundle(id);
            //printAssetBundleContent(assetbundle);
            EnableButtonsDelegate.refreshingSavesPercentageMessage.SetText(id);
            this.addtoJson(assetManifest.Value, assetbundle, false);
            ++counter;
            EnableButtonsDelegate.refreshingSavesPercentageText.SetText($"{counter}/{amount}");
          } else {
            Log.M?.WL(0, $"requesting:{id}:{(string)assetManifest.Value["filename"]}");
            EnableButtonsDelegate.refreshingSavesPercentageMessage.SetText(id);
            bool in_requesting = true;
            UnityGameInstance.BattleTechGame.DataManager.AssetBundleManager.RequestBundle(id,
              (bundle) => {
                in_requesting = false;
                this.addtoJson(assetManifest.Value, bundle, true);
              }
            );
            int watchdog = 100;
            while (in_requesting) {
              if (watchdog <= 0) {
                Log.M?.WL(1, $"timeout: {id}", true);
                break;
              }
              --watchdog;
              yield return new WaitForSeconds(0.1f);
            }
            ++counter;
            EnableButtonsDelegate.refreshingSavesPercentageText.SetText($"{counter}/{amount}");
          }
        }
        foreach(var bundlejson in dump) {
          JArray missing = bundlejson.Value["manifest_missing"] as JArray;
          if (missing.Count == 0) { continue; }
          Log.M?.WL(0, bundlejson.Key);
          foreach(var missprefab in missing) {
            Log.M?.WL(1, (string)missprefab);
          }
        }
        Log.M?.TWL(0, "LOADING ALL ASSET BUNDLES END", true);
        File.WriteAllText(Path.Combine(Log.BaseDirectory, "assetbundlesdump.json"), dump.ToString(Formatting.Indented));
        EnableButtonsDelegate.EnableButtons(EnableButtonsDelegate.message);
        EnableButtonsDelegate = null;
        yield break;
      }
      public bool StartPrewarm(MessageCenterMessage message) {
        Log.M?.TWL(0, "StartPrewarm");
        EnableButtonsDelegate.message = message;
        WaitingForStart = 10;
        WaitingForSaves = 10;
        PreloaderAPI = typeof(ModTek.ModTek).Assembly.GetType("ModTek.PreloaderAPI");
        if (PreloaderAPI == null) {
          Log.M?.TWL(0, "Can't find ModTek.PreloaderAPI");
        } else {
          if (IsPreloading == null) {
            IsPreloading = PreloaderAPI.GetProperty("IsPreloading", BindingFlags.Static | BindingFlags.Public);
            if (IsPreloading == null) { Log.M?.TWL(0, "Can't find ModTek.PreloaderAPI.IsPreloading"); }
          }
        }
        this.refreshingSavesSpinner = EnableButtonsDelegate.__instance.refreshingSavesSpinner;
        EnableButtonsDelegate.refreshingSavesPercentageMessage = refreshingSavesSpinner.FindObject<LocalizableText>("message_text");
        EnableButtonsDelegate.refreshingSavesPercentageText = EnableButtonsDelegate.__instance.refreshingSavesPercentageText;
        return true;
      }
      public void SetWaitingForModTek() {
        EnableButtonsDelegate.__instance.refreshingSavesPercentageText = null;
        if (EnableButtonsDelegate.__instance._continueButton != null)
          EnableButtonsDelegate.__instance._continueButton.SetState(ButtonState.Disabled);
        if (EnableButtonsDelegate.__instance.campaignLoadButton != null)
          EnableButtonsDelegate.__instance.campaignLoadButton.SetState(ButtonState.Disabled);
        if (EnableButtonsDelegate.__instance.skirmishLoadButton != null)
          EnableButtonsDelegate.__instance.skirmishLoadButton.SetState(ButtonState.Disabled);
        if (EnableButtonsDelegate.__instance._careerContinueButton != null)
          EnableButtonsDelegate.__instance._careerContinueButton.SetState(ButtonState.Disabled);
        if (EnableButtonsDelegate.__instance.careerLoadButton != null)
          EnableButtonsDelegate.__instance.careerLoadButton.SetState(ButtonState.Disabled);
        if (EnableButtonsDelegate.__instance.mainNavPanel != null && EnableButtonsDelegate.__instance.mainNavPanel.activeSelf)
          EnableButtonsDelegate.__instance.mainNavPanel.SetActive(false);
        if (EnableButtonsDelegate.__instance.refreshingSavesSpinner != null)
          EnableButtonsDelegate.__instance.refreshingSavesSpinner.SetActive(true);
        EnableButtonsDelegate.messageText = "WAITING FOR MODTEK";
        EnableButtonsDelegate.refreshingSavesPercentageMessage.SetText("WAITING FOR MODTEK");
      }
      private static Type PreloaderAPI = null;
      private static PropertyInfo IsPreloading = null;
      public void Update() {
        if (EnableButtonsDelegate == null) { return; }
        //Log.M?.WL(0, $"Update WaitingForStart:{WaitingForStart} WaitingForSaves:{WaitingForSaves}");
        if(WaitingForSaves > 0) {
          if (this.refreshingSavesSpinner == null) { WaitingForSaves = 0; } else {
            if (this.refreshingSavesSpinner.activeSelf == false) {
              --WaitingForSaves;
              if (WaitingForSaves <= 0) { SetWaitingForModTek(); }
            } else {
              WaitingForSaves = 10;
            }
          }
          return;
        }
        if (WaitingForStart > 0) {
          bool is_preloading = false;
          if (IsPreloading != null) {
            is_preloading = (bool)IsPreloading.GetValue(null);
          }
          if (LoadingCurtain.activeInstance.Visible) { is_preloading = true; }
          Log.M?.TWL(0, $"is in waiting for ModTek:{is_preloading}");
          if (is_preloading == false) {
            --WaitingForStart;
            if (WaitingForStart <= 0) { EnableButtonsDelegate.StartPrewarm(); }
          } else {
            WaitingForStart = 10;
          }
          return;
        }
        if (EnableButtonsDelegate.PrewarmEnded == false) {
          try {
            if ((MainMenu_ShowRefreshingSaves.loadingDeps == true) && (MainMenu_ShowRefreshingSaves.dependencyLoad == null)) {
              DataManager.InjectedDependencyLoadRequest dependencyLoad = new DataManager.InjectedDependencyLoadRequest(MainMenu_ShowRefreshingSaves.dataManager);
              foreach (var resType in FastDataLoadHelper.prewarmTypesOrder) {
                foreach (var def in Serialize.SerializableFactory.getStorage(MainMenu_ShowRefreshingSaves.dataManager, resType).Values) {
                  EnableButtonsDelegate.curDataCount += 1;
                  BattleTech.Data.DataManager.ILoadDependencies deps = def as BattleTech.Data.DataManager.ILoadDependencies;
                  if (deps == null) { continue; }
                  if (deps.DependenciesLoaded(10u)) { continue; }
                  deps.GatherDependencies(MainMenu_ShowRefreshingSaves.dataManager, dependencyLoad, 10u);
                }
              }
              dependencyLoad.RegisterLoadCompleteCallback((Action)(() => { MainMenu_ShowRefreshingSaves.loadingDeps = false; }));
              MainMenu_ShowRefreshingSaves.dataManager.InjectDependencyLoader(dependencyLoad, 10U);
              MainMenu_ShowRefreshingSaves.dependencyLoad = dependencyLoad;
            }
          } catch (Exception e) {
            Log.M_Err?.TWL(0, e.ToString(), true);
          }
          if (EnableButtonsDelegate.refreshingSavesPercentageText != null) {
            if (Time.frameCount % 4 != 0) {
              if (this.text != EnableButtonsDelegate.messageText) {
                EnableButtonsDelegate.refreshingSavesPercentageMessage.SetText(EnableButtonsDelegate.messageText);
                this.text = EnableButtonsDelegate.messageText;
              }
              EnableButtonsDelegate.refreshingSavesPercentageText.SetText(string.Format("{0}", EnableButtonsDelegate.curDataCount));
            }
          }
          return;
        }
        if (Core.Settings.AssetBundlesValidate) {
          this.GatherAssetsBundlesInfo();
        } else {
          EnableButtonsDelegate?.EnableButtons(EnableButtonsDelegate.message);
          EnableButtonsDelegate = null;
        }
        if (PLEASE_DISABLE) {
          Core.Settings.UseFastPreloading = false;
          CustomSettings.ModsLocalSettingsHelper.SaveSettings("CustomPrewarm");
          if (Localize.Strings.CurrentCulture != Localize.Strings.Culture.CULTURE_RU_RU) {
            GenericPopup popup = GenericPopupBuilder.Create("SOMETHING IS WRONG", "CACHING IS DISABLED. RESTART IS NEEDED").AddButton("Exit Game", () => {
              Application.Quit();
            }).IsNestedPopupWithBuiltInFader().SetAlwaysOnTop().Render();
          } else {
            GenericPopup popup = GenericPopupBuilder.Create("ШО ТО ТАКИ ПОШЛО НЕ ТАК", "Отключаю все нафиг. НУЖНО ПЕРЕЗАПУСТИТЬ ИГРУ").AddButton("ВЫЙТИ ИЗ ИГРЫ", () => {
              Application.Quit();
            }).IsNestedPopupWithBuiltInFader().SetAlwaysOnTop().Render();
          }
        } else
        if (PLEASE_RESTART) {
          if (Localize.Strings.CurrentCulture != Localize.Strings.Culture.CULTURE_RU_RU) {
            GenericPopup popup = GenericPopupBuilder.Create("RESTART NEEDED", "RESTART IS NEEDED").AddButton("Exit Game", () => {
              Application.Quit();
            }).IsNestedPopupWithBuiltInFader().SetAlwaysOnTop().Render();
          } else {
            GenericPopup popup = GenericPopupBuilder.Create("ТАКИ НУЖНО ПЕРЕЗАПУСТИТЬСЯ", "НУЖНО ПЕРЕЗАПУСТИТЬ ИГРУ").AddButton("ВЫЙТИ ИЗ ИГРЫ", () => {
              Application.Quit();
            }).IsNestedPopupWithBuiltInFader().SetAlwaysOnTop().Render();
          }
        }
      }
    }
    public class d_MainManuEnableButtons {
      public MainMenu __instance { get; set; }
      public MessageCenterMessage message { get; set; }
      public bool PrewarmEnded { get; set; } = false;
      public int allDataCount { get; set; } = 0;
      public int curDataCount { get; set; } = 0;
      public LocalizableText refreshingSavesPercentageText { get; set; } = null;
      public LocalizableText refreshingSavesPercentageMessage { get; set; } = null;
      public string messageText { get; set; }
      public d_MainManuEnableButtons(MainMenu i) {
        __instance = i;
      }
      public bool StartPrewarm() {
        Log.M?.TWL(0, "d_MainManuEnableButtons.StartPrewarm", true);
        try {
          Thread thread = new Thread(DoPrewarm);
          thread.Start(this);
        } catch (Exception e) {
          Log.M?.TWL(0, e.ToString(), true);
        }
        return true;
      }
      public bool EnableButtons(MessageCenterMessage message) {
        try {
          Log.M?.TWL(0, "EnableButtons", true);
          GameObject mainNavPanel = __instance.mainNavPanel;
          if (mainNavPanel != null && !mainNavPanel.activeSelf) { mainNavPanel.SetActive(true); }
          __instance.refreshingSavesSpinner.SetActive(false);
          __instance.EnableSkirmishLoadIfSkirmishSaves(message);
          __instance.EnableCampaignLoadIfSaves(message);
          __instance.EnableCareerLoadIfCareerSaves(message);
        } catch (Exception e) {
          Log.M?.TWL(0, e.ToString(), true);
        }
        return true;
      }
    }
    public static void Postfix(MainMenu __instance, MessageCenterMessage message) {
      if (Core.CacheLoadingInited) { return; }
      //__result = true;
      try {
        Log.M?.TWL(0, "MainMenu.ShowRefreshingSaves", true);
        PrewarmEndMessager messager = __instance.gameObject.GetComponent<PrewarmEndMessager>();
        if (messager == null) { messager = __instance.gameObject.AddComponent<PrewarmEndMessager>(); }
        messager.WaitingForStart = 0;
        messager.EnableButtonsDelegate = new d_MainManuEnableButtons(__instance);
        messager.StartPrewarm(message);
        return;
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        return;
      }
    }
  }
  [HarmonyPatch(typeof(JSONSerializationUtility))]
  [HarmonyPatch("LogWarning")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class JSONSerializationUtility_JSONSerializationUtility {
    public static bool Prefix() { return false; }
  }
  //[HarmonyPatch(typeof(UnityGameInstance))]
  //[HarmonyPatch("Update")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { })]
  //public static class UnityGameInstance_Update {
  //  public static void Prefix(UnityGameInstance __instance) {
  //    FastDataLoadHelper.Update();
  //  }
  //}
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("OnStateBitsChanged")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class SimGameState_OnStateBitsChanged {
    public static void Prefix(SimGameState __instance) {
      if (__instance == null) { return; }
      try {
        var state = __instance.initState;
        var prevstate = __instance.previousInitState;
        Log.M?.TWL(0, "SimGameState.OnStateBitsChanged:" + (prevstate) + ">" + state, true);
        Log.M?.WL(1, prevstate.ToString(), true);
        Log.M?.WL(1, state.ToString(), true);
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        SimGameState.logger.LogException(e);
      }
    }
    public static void Finalizer(Exception __exception) {
      if (__exception != null) {
        Log.M?.TWL(0, __exception.ToString(), true);
        SimGameState.logger.LogError("I'M JUST A VICTIM HERE!");
        SimGameState.logger.LogException(__exception);
      }
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("RemoveItemStat")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(System.Type), typeof(bool) })]
  public static class SimGameState_RemoveItemStat_0 {
    public static void Postfix(SimGameState __instance, string id, System.Type type, bool damaged) {
      if (__instance == null) { return; }
      try {
        id = __instance.GetItemStatID(id, type);
        if (damaged) { id += string.Format(".{0}", (object)"DAMAGED"); }
        var stat = __instance.companyStats.GetStatistic(id);
        if (stat == null) { return; }
        if (stat.Value<int>() <= 0) {
          if (CustomPrewarm.ItemsCountSanitizeBlackListDef.noSanitizeIds.Contains(id)) {
            SimGameState.logger.LogDebug($"{id} have value {stat.Value<int>()} keep cause in blacklist");
          } else {
            SimGameState.logger.LogDebug($"{id} have value {stat.Value<int>()} removing");
            Log.M?.TWL(0, $"{id} have value {stat.Value<int>()} removing");
            __instance.companyStats.RemoveStatistic(id);
          }
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        SimGameState.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("Rehydrate")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(GameInstanceSave) })]
  public static class SimGameState_Rehydrate {
    public static void SanitiazeItems(this SimGameState sim) {
      Log.M?.TWL(0, $"SimGameState.SanitiazeItems");
      HashSet<string> to_delete = new HashSet<string>();
      foreach (var stat in sim.companyStats) {
        if (stat.Key.StartsWith("Item.") == false) { continue; }
        if (stat.Value.CheckType(typeof(int)) == false) { continue; }
        if (stat.Value.Value<int>() > 0) { continue; }
        var name = stat.Key.Split('.');
        if (name.Length < 3) { continue; }
        if ((ItemsCountSanitizeBlackListDef.noSanitizeIds.Contains(name[2]) == false) && (ItemsCountSanitizeBlackListDef.noSanitizeTypes.Contains(name[1]) == false)) {
          Log.M?.WL(1, $"{stat.Key}:{stat.Value.value} - deleting");
          to_delete.Add(stat.Key);
        } else {
          Log.M?.WL(1, $"{stat.Key}:{stat.Value.value} - keep cause in blacklist");
        }
      }
      foreach (var name in to_delete) { sim.CompanyStats.RemoveStatistic(name); }
    }
    public static void Postfix(SimGameState __instance, GameInstanceSave gameInstanceSave) {
      if (__instance == null) { return; }
      try {
        __instance.SanitiazeItems();
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        SimGameState.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("Dehydrate")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(SimGameSave), typeof(SerializableReferenceContainer) })]
  public static class SimGameState_Dehydrate {
    public static void Prefix(SimGameState __instance, SimGameSave save, SerializableReferenceContainer references) {
      if (__instance == null) { return; }
      try {
        __instance.SanitiazeItems();
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        SimGameState.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("RemoveItemStat")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(string), typeof(bool) })]
  public static class SimGameState_RemoveItemStat_1 {
    public static void Postfix(SimGameState __instance, string id, string type, bool damaged) {
      if (__instance == null) { return; }
      try {
        id = __instance.GetItemStatID(id, type);
        if (damaged) { id += string.Format(".{0}", (object)"DAMAGED"); }
        var stat = __instance.companyStats.GetStatistic(id);
        if (stat == null) { return; }
        if (stat.Value<int>() <= 0) {
          SimGameState.logger.LogDebug($"{id} have value {stat.Value<int>()} removing");
          Log.M?.TWL(0, $"{id} have value {stat.Value<int>()} removing");
          __instance.companyStats.RemoveStatistic(id);
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        SimGameState.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("_OnInit")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(GameInstance), typeof(SimGameDifficulty) })]
  public static class SimGameState__OnInit {
    public static void Finalizer(SimGameState __instance, GameInstance game, SimGameDifficulty difficulty, Exception __exception) {
      //FastDataLoadHelper.Reset();
      if (__exception != null) {
        SimGameState.logger.LogError("I'M JUST A VICTIM HERE!");
        SimGameState.logger.LogException(__exception);
      }
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("_OnHeadlessComplete")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class SimGameState__OnHeadlessComplete {
    public static void Finalizer(SimGameState __instance, ref bool __result, Exception __exception) {
      Log.M?.TWL(0, "SimGameState._OnHeadlessComplete:" + __result, true);
      if (__exception != null) {
        SimGameState.logger.LogError("I'M JUST A VICTIM HERE!");
        SimGameState.logger.LogException(__exception);
      }
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("HandleSaveHydrate")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class SimGameState_HandleSaveHydrate {
    public static void Prefix(ref bool __runOriginal, SimGameState __instance, ref bool __result) {
      Log.M?.TWL(0, "SimGameState.HandleSaveHydrate", true);
      try {
        if (__instance.save == null) { __result = true; __runOriginal = false; return; }
        try {
          if (DebugBridge.TestToolsEnabled && BattleTech.Save.SaveGameStructure.SaveGameStructure.ForceSimGameRehydrateFailure) {
            __result = false; __runOriginal = false; return;
          }
          __instance.Rehydrate(__instance.save);
          if (__instance.save.SimGameSave.PreviouslyAttachedHeadState) {
            //__instance.SetInitStateBits(SimGameState.InitStates.UX_ATTACHED_PREVIOUSLY);
            __instance.SetInitStateBits(SimGameState.InitStates.UX_ATTACHED_PREVIOUSLY);
          }
          Log.M?.TWL(0, "SimGameState.HandleSaveHydrate success", true);
          __result = true; __runOriginal = false; return;
        } catch (Exception e) {
          Log.M?.TWL(0, e.ToString(), true);
        } finally {
          __instance.save = (GameInstanceSave)null;
        }
        __result = false;
        __runOriginal = false; return;
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        __result = false;
        SimGameState.logger.LogError("I'M JUST A VICTIM HERE!");
        SimGameState.logger.LogException(e);
        __runOriginal = false; return;
      }
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("RespondToDefsLoadComplete")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(LoadRequest) })]
  public static class SimGameState_RespondToDefsLoadComplete {
    public static void Prefix() {
      SimGameState_RequestDataManagerResources.timer.Stop();
      Log.M?.TWL(0, "SimGameState.RespondToDefsLoadComplete:" + SimGameState_RequestDataManagerResources.timer.Elapsed.TotalSeconds, true);
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("RequestDataManagerResources")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class SimGameState_RequestDataManagerResources {
    public static Stopwatch timer = new Stopwatch();
    public class RespondToDefsLoadComplete_delegate {
      public SimGameState simgame { get; set; }
      public RespondToDefsLoadComplete_delegate(SimGameState simgame) { this.simgame = simgame; }
      public void Invoke(LoadRequest loadRequest) {
        simgame.RespondToDefsLoadComplete(loadRequest);
      }
    }
    public static bool Prefix(SimGameState __instance) {
      timer.Restart();
      return true;
      //if (Core.Settings.UseFastPreloading == false) { return true; }
      ////return true;
      //try {
      //  Log.M?.TWL(0, "SimGameState.RequestDataManagerResources", true);
      //  FastDataLoadHelper.Reset();
      //  FastDataLoadHelper.FastPrewarm(__instance);
      //  //LoadRequest loadRequest = __instance.DataManager.CreateLoadRequest(new Action<LoadRequest>(new RespondToDefsLoadComplete_delegate(__instance).Invoke), true);
      //  //loadRequest.ProcessRequests();
      //  return false;
      //} catch (Exception e) {
      //  Log.M?.TWL(0, e.ToString(), true);
      //  return true;
      //}
    }
  }
  [HarmonyPatch(typeof(GameInstanceSave))]
  [HarmonyPatch("RequestResourcesCustom")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(DataManager) })]
  public static class GameInstanceSave_RequestResourcesCustom {
    public static void Prefix(ref Stopwatch __state) {
      __state = new Stopwatch();
      __state.Start();
    }
    public static void Postfix(ref Stopwatch __state) {
      __state.Stop();
      Log.M?.TWL(0, "GameInstanceSave.RequestResourcesCustom:" + __state.Elapsed.TotalSeconds, true);
    }
  }
  [HarmonyPatch(typeof(SaveManager))]
  [HarmonyPatch(MethodType.Constructor)]
  [HarmonyPatch(new Type[] { typeof(MessageCenter) })]
  public static class SaveManager_Constructor {
    public static void Postfix(SaveManager __instance) {
      Log.M?.TWL(0, "SaveManager:" + __instance.saveSystem.localWriteLocation.rootPath);
      Core.Settings.savesdirectory = Path.Combine(__instance.saveSystem.localWriteLocation.rootPath, "cache");
      if (Directory.Exists(Core.Settings.savesdirectory) == false) { Directory.CreateDirectory(Core.Settings.savesdirectory); };
    }
  }
  public enum FastLoadStage { Preload, Dependencies, RefreshMechs, Final, None }
  public class MechDefCacheItem {
    public string id { get; set; } = string.Empty;
    public string Hash { get { return hash.toString(); } set { hash = value.toByteArray(); } }
    [JsonIgnore]
    public byte[] hash { get; set; } = new byte[] { };
    [JsonIgnore]
    public bool needUpdate { get; set; } = false;
    //[JsonIgnore]
    //public DateTime ModDate { get; set; }
    //public string moddate { get { return ModDate.ToString(FastDataLoadHelper.DATETIME_FORMAT); } set { ModDate = DateTime.ParseExact(value, FastDataLoadHelper.DATETIME_FORMAT, CultureInfo.InvariantCulture); } }
    public string payload { get; set; }
  }
  public static class FixedMechDefHelper {
    private static string CacheFilePath = string.Empty;
    private static ConcurrentDictionary<string, MechDefCacheItem> mechDefsCache = new ConcurrentDictionary<string, MechDefCacheItem>();
    private static readonly uint[] _lookup32 = CreateLookup32();
    private static uint[] CreateLookup32() {
      var result = new uint[256];
      for (int i = 0; i < 256; i++) {
        string s = i.ToString("X2");
        result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
      }
      return result;
    }
    public static string toString(this byte[] bytes) {
      var lookup32 = _lookup32;
      var result = new char[bytes.Length * 2];
      for (int i = 0; i < bytes.Length; i++) {
        var val = lookup32[bytes[i]];
        result[2 * i] = (char)val;
        result[2 * i + 1] = (char)(val >> 16);
      }
      return new string(result);
    }
    public static byte[] toByteArray(this string hex) {
      if (hex.Length % 2 == 1)
        throw new Exception("The binary key cannot have an odd number of digits");
      byte[] arr = new byte[hex.Length >> 1];
      for (int i = 0; i < hex.Length >> 1; ++i) {
        arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
      }
      return arr;
    }
    private static int GetHexVal(char hex) {
      int val = (int)hex;
      return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
    }
    private static Aes aes = null;
    private static readonly string key = "DLyTRmKhA0OK39BWnK5qsA9y9NiUMwtfX83dhXhq1qo=";
    private static readonly string IV = "1eiJnoJJ+UHtOurHeaNZ9w==";
    //public static void Init() {
    //  if (Core.Settings.UseFastPreloading == false) { return; }
    //  try {
    //    Log.M?.TWL(0, "FixedMechDefHelper.CacheFilePath:" + CacheFilePath);
    //    FixedMechDefHelper.aes = Aes.Create();
    //    FixedMechDefHelper.aes.Mode = CipherMode.CBC;
    //    FixedMechDefHelper.aes.KeySize = 256;
    //    FixedMechDefHelper.aes.BlockSize = 128;
    //    FixedMechDefHelper.aes.FeedbackSize = 128;
    //    FixedMechDefHelper.aes.Padding = PaddingMode.PKCS7;
    //    FixedMechDefHelper.aes.Key = Convert.FromBase64String(key);
    //    FixedMechDefHelper.aes.IV = Convert.FromBase64String(IV);
    //    if (File.Exists(FixedMechDefHelper.CacheFilePath)) {
    //      byte[] result = null;
    //      byte[] cacheContent = File.ReadAllBytes(FixedMechDefHelper.CacheFilePath);
    //      ICryptoTransform encryptor = aes.CreateDecryptor(aes.Key, aes.IV);
    //      using (MemoryStream msEncrypt = new MemoryStream()) {
    //        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)) {
    //          csEncrypt.Write(cacheContent, 0, cacheContent.Length);
    //        }
    //        result = msEncrypt.ToArray();
    //      }
    //      mechDefsCache = JsonConvert.DeserializeObject<ConcurrentDictionary<string, MechDefCacheItem>>(Encoding.UTF8.GetString(result));
    //    }
    //  } catch (Exception e) {
    //    Log.M?.TWL(0, e.ToString(), true);
    //  }
    //  Log.M?.WL(1, "cache content:" + mechDefsCache.Count);
    //}
    //public static void Init(string directory) {
    //  FixedMechDefHelper.CacheFilePath = Path.Combine(directory, "mechdefs_autofix_cache.json");
    //  //Init();
    //}
    public static void Flush() {
      Log.M?.TWL(0, "FixedMechDefHelper.Flush:" + mechDefsCache.Count + ":" + CacheFilePath);
      try {
        byte[] cacheContent = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(mechDefsCache, Formatting.Indented));
        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using (MemoryStream msEncrypt = new MemoryStream()) {
          using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)) {
            csEncrypt.Write(cacheContent, 0, cacheContent.Length);
          }
          File.WriteAllBytes(FixedMechDefHelper.CacheFilePath, msEncrypt.ToArray());
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
    private static System.Text.UTF8Encoding hash_enc = new UTF8Encoding();
    public static byte[] SHA1(this string content) {
      SHA1 crypto = new SHA1CryptoServiceProvider();
      return crypto.ComputeHash(Encoding.UTF8.GetBytes(content));
    }
    public static bool GetFromCache(string id, string path, ref string content, ref byte[] hash) {
      byte[] origContent = File.ReadAllBytes(path);
      SHA1 crypto = new SHA1CryptoServiceProvider();
      hash = crypto.ComputeHash(origContent);
      if (mechDefsCache.TryGetValue(id, out MechDefCacheItem cacheItem)) {
        if (cacheItem.hash.SequenceEqual(hash)) {
          content = cacheItem.payload;
          return true;
        }
        cacheItem.hash = hash;
        cacheItem.payload = Encoding.UTF8.GetString(origContent);
        cacheItem.needUpdate = true;
        content = cacheItem.payload;
        return true;
      } else {
        content = Encoding.UTF8.GetString(origContent);
        return false;
      }
    }
    public static void AddToCache(string id, string content, byte[] hash) {
      MechDefCacheItem cacheItem = new MechDefCacheItem();
      cacheItem.id = id;
      cacheItem.hash = hash;
      cacheItem.payload = content;
      cacheItem.needUpdate = true;
      mechDefsCache.AddOrUpdate(id, cacheItem, (k, v) => { return cacheItem; });
    }
    //public static void AutoFixMechs(SimGameState simgame) {
    //  try {
    //    if (Core.Settings.UseFastPreloading == false) { return; }
    //    while (FastDataLoadHelper.CheckUnitsChassis() == false) { };
    //    List<MechDef> list = simgame.DataManager.MechDefs.Select<KeyValuePair<string, MechDef>, MechDef>((Func<KeyValuePair<string, MechDef>, MechDef>)(pair => pair.Value)).ToList<MechDef>();
    //    int count = 0;
    //    foreach (MechDef def in list) {
    //      if (def.MechTags.Contains(FastDataLoadHelper.NOAUTOFIX_TAG)) { continue; }
    //      if (def.MechTags.Contains(FastDataLoadHelper.FAKE_VEHICLE_TAG)) { continue; }
    //      if (mechDefsCache.TryGetValue(def.Description.Id, out MechDefCacheItem cacheItem)) {
    //        if (cacheItem.needUpdate == false) {
    //          def.MechTags.Add(FastDataLoadHelper.NOAUTOFIX_TAG);
    //          continue;
    //        }
    //      }
    //      ++count;
    //    }
    //    Log.M?.TWL(0, "MechDefs need autofix:" + count);
    //    AutoFixer.Shared.FixMechDef(list);
    //    //string cacheDir = Path.Combine(Path.GetDirectoryName(FixedMechDefHelper.DBFilePath), "autofixcache");
    //    //if (Directory.Exists(cacheDir) == false) { Directory.CreateDirectory(cacheDir); }
    //    count = 0;
    //    foreach (MechDef def in list) {
    //      if (def.MechTags.Contains(FastDataLoadHelper.FAKE_VEHICLE_TAG)) { continue; }
    //      if (mechDefsCache.TryGetValue(def.Description.Id, out MechDefCacheItem cacheItem) == false) { continue; };
    //      def.MechTags.Remove(FastDataLoadHelper.NOAUTOFIX_TAG);
    //      if (cacheItem.needUpdate == false) { continue; }
    //      JObject originalJson = JObject.Parse(cacheItem.payload);
    //      JObject fixedJson = JObject.Parse(def.ToJSON());
    //      if (fixedJson["Chassis"] != null) { fixedJson.Remove("Chassis"); };
    //      originalJson.Merge(fixedJson, new JsonMergeSettings() { MergeArrayHandling = MergeArrayHandling.Replace, MergeNullValueHandling = MergeNullValueHandling.Ignore });
    //      cacheItem.payload = originalJson.ToString(Formatting.None);
    //      cacheItem.needUpdate = false;
    //      ++count;
    //    }
    //    if (count > 0) { Flush(); }
    //    Log.M?.TWL(0, "AutoFixMechs saved to cache:" + count);
    //  } catch (Exception e) {
    //    Log.M?.TWL(0, e.ToString(), true);
    //  }
    //}
  }
  public static class FastDataLoadHelper {
    public static readonly string NOAUTOFIX_TAG = "noautofix";
    public static readonly string FAKE_VEHICLE_TAG = "fake_vehicle";
    public static readonly string DATETIME_FORMAT = "yyyy-MM-dd HH:mm:ss.ff";
    public static readonly int THREADS_COUNT = 1;
    private static FastLoadStage stage = FastLoadStage.None;
    private static Stopwatch stageWatcher = new Stopwatch();
    public static void Reset() { stage = FastLoadStage.None; stageWatcher.Stop(); stageWatcher.Reset(); }
    public static List<BattleTechResourceType> prewarmTypesOrder = new List<BattleTechResourceType>() {
      BattleTechResourceType.BaseDescriptionDef,
      BattleTechResourceType.BackgroundDef,
      BattleTechResourceType.AmmunitionDef,
      BattleTechResourceType.AmmunitionBoxDef,
      BattleTechResourceType.WeaponDef,
      BattleTechResourceType.HeatSinkDef,
      BattleTechResourceType.JumpJetDef,
      BattleTechResourceType.UpgradeDef,
      BattleTechResourceType.StarSystemDef,
      BattleTechResourceType.AbilityDef,
      BattleTechResourceType.MovementCapabilitiesDef,
      BattleTechResourceType.PathingCapabilitiesDef,
      BattleTechResourceType.HardpointDataDef,
      BattleTechResourceType.VehicleChassisDef,
      BattleTechResourceType.VehicleDef,
      BattleTechResourceType.ChassisDef,
      BattleTechResourceType.MechDef
    };
    public static HashSet<BattleTechResourceType> prewarmTypes = new HashSet<BattleTechResourceType>() {
      BattleTechResourceType.AbilityDef,
      BattleTechResourceType.AmmunitionBoxDef,
      BattleTechResourceType.AmmunitionDef,
      BattleTechResourceType.BackgroundDef,
      //BattleTechResourceType.BackgroundQuestionDef,
      BattleTechResourceType.BaseDescriptionDef,
      //BattleTechResourceType.BuildingDef,
      //BattleTechResourceType.CastDef,
      BattleTechResourceType.ChassisDef,
      ////BattleTechResourceType.DesignMaskDef,
      ////BattleTechResourceType.FactionDef,
      ////BattleTechResourceType.FlashpointDef,
      BattleTechResourceType.HardpointDataDef,
      BattleTechResourceType.HeatSinkDef,
      //BattleTechResourceType.HeraldryDef,
      BattleTechResourceType.JumpJetDef,
      //BattleTechResourceType.LanceDef,
      BattleTechResourceType.MechDef,
      BattleTechResourceType.MovementCapabilitiesDef,
      BattleTechResourceType.PathingCapabilitiesDef,
      ////BattleTechResourceType.PilotDef,
      ////BattleTechResourceType.RegionDef,
      BattleTechResourceType.StarSystemDef,
      ////BattleTechResourceType.TurretChassisDef,
      ////BattleTechResourceType.TurretDef,
      BattleTechResourceType.UpgradeDef,
      BattleTechResourceType.VehicleChassisDef,
      BattleTechResourceType.VehicleDef,
      BattleTechResourceType.WeaponDef
    };
    public class PrewarmItem {
      public string path { get; set; }
      public string id { get; set; }
      public BattleTechResourceType resType { get; set; }
      public PrewarmItem(string p, string ID, BattleTechResourceType resType) {
        this.path = p;
        this.id = ID;
        this.resType = resType;
      }
    }
    public static Queue<PrewarmItem> prewarmQueue = new Queue<PrewarmItem>();
    public class PrewarmDelegate {
      public int index { get; set; }
      public DataManager dataManager { get; set; }
      public PrewarmDelegate(DataManager dm, int i) {
        this.dataManager = dm; index = i;
      }
      public static Type GetTypeFromBattletechResource(BattleTechResourceType bt) {
        switch (bt) {
          case BattleTechResourceType.AbilityDef: return typeof(AbilityDef);
          case BattleTechResourceType.AmmunitionBoxDef: return typeof(AmmunitionBoxDef);
          case BattleTechResourceType.AmmunitionDef: return typeof(AmmunitionDef);
          case BattleTechResourceType.BackgroundDef: return typeof(BackgroundDef);
          case BattleTechResourceType.BackgroundQuestionDef: return typeof(BackgroundQuestionDef);
          case BattleTechResourceType.BaseDescriptionDef: return typeof(BaseDescriptionDef);
          case BattleTechResourceType.BuildingDef: return typeof(BuildingDef);
          case BattleTechResourceType.CastDef: return typeof(CastDef);
          case BattleTechResourceType.ChassisDef: return typeof(ChassisDef);
          case BattleTechResourceType.DesignMaskDef: return typeof(DesignMaskDef);
          case BattleTechResourceType.FactionDef: return typeof(FactionDef);
          case BattleTechResourceType.FlashpointDef: return typeof(FlashpointDef);
          case BattleTechResourceType.HardpointDataDef: return typeof(HardpointDataDef);
          case BattleTechResourceType.HeatSinkDef: return typeof(HeatSinkDef);
          case BattleTechResourceType.HeraldryDef: return typeof(HeraldryDef);
          case BattleTechResourceType.JumpJetDef: return typeof(JumpJetDef);
          case BattleTechResourceType.LanceDef: return typeof(LanceDef);
          case BattleTechResourceType.MechDef: return typeof(MechDef);
          case BattleTechResourceType.MovementCapabilitiesDef: return typeof(MovementCapabilitiesDef);
          case BattleTechResourceType.PathingCapabilitiesDef: return typeof(PathingCapabilitiesDef);
          case BattleTechResourceType.PilotDef: return typeof(PilotDef);
          case BattleTechResourceType.RegionDef: return typeof(RegionDef);
          case BattleTechResourceType.StarSystemDef: return typeof(StarSystemDef);
          case BattleTechResourceType.TurretChassisDef: return typeof(TurretChassisDef);
          case BattleTechResourceType.TurretDef: return typeof(TurretDef);
          case BattleTechResourceType.UpgradeDef: return typeof(UpgradeDef);
          case BattleTechResourceType.VehicleChassisDef: return typeof(VehicleChassisDef);
          case BattleTechResourceType.VehicleDef: return typeof(VehicleDef);
          case BattleTechResourceType.WeaponDef: return typeof(WeaponDef);
          default: throw new Exception("Unknown type " + bt);
        }
      }
      public static string GetStoreNameFromBattletechResource(BattleTechResourceType bt) {
        switch (bt) {
          case BattleTechResourceType.AbilityDef: return "abilityDefs";
          case BattleTechResourceType.AmmunitionBoxDef: return "ammoBoxDefs";
          case BattleTechResourceType.AmmunitionDef: return "ammoDefs";
          case BattleTechResourceType.BackgroundDef: return "backgroundDefs";
          case BattleTechResourceType.BackgroundQuestionDef: return "backgroundQuestionDefs";
          case BattleTechResourceType.BaseDescriptionDef: return "baseDescriptionDefs";
          case BattleTechResourceType.BuildingDef: return "buildingDefs";
          case BattleTechResourceType.CastDef: return "castDefs";
          case BattleTechResourceType.ChassisDef: return "chassisDefs";
          case BattleTechResourceType.DesignMaskDef: return "designMaskDefs";
          case BattleTechResourceType.FactionDef: return "factions";
          case BattleTechResourceType.FlashpointDef: return "flashpointDefs";
          case BattleTechResourceType.HardpointDataDef: return "hardpointDataDefs";
          case BattleTechResourceType.HeatSinkDef: return "heatSinkDefs";
          case BattleTechResourceType.HeraldryDef: return "heraldries";
          case BattleTechResourceType.JumpJetDef: return "jumpJetDefs";
          case BattleTechResourceType.LanceDef: return "lanceDefs";
          case BattleTechResourceType.MechDef: return "mechDefs";
          case BattleTechResourceType.MovementCapabilitiesDef: return "movementCapDefs";
          case BattleTechResourceType.PathingCapabilitiesDef: return "pathingCapDefs";
          case BattleTechResourceType.PilotDef: return "pilotDefs";
          case BattleTechResourceType.RegionDef: return "regions";
          case BattleTechResourceType.StarSystemDef: return "systemDefs";
          case BattleTechResourceType.TurretChassisDef: return "turretChassisDefs";
          case BattleTechResourceType.TurretDef: return "turretDefs";
          case BattleTechResourceType.UpgradeDef: return "upgradeDefs";
          case BattleTechResourceType.VehicleChassisDef: return "vehicleChassisDefs";
          case BattleTechResourceType.VehicleDef: return "vehicleDefs";
          case BattleTechResourceType.WeaponDef: return "weaponDefs";
          default: throw new Exception("Unknown type " + bt);
        }
      }
      private static ConcurrentDictionary<object, MethodInfo> addMethods_cache = new ConcurrentDictionary<object, MethodInfo>();
      private static ConcurrentDictionary<BattleTechResourceType, FieldInfo> Field_cache = new ConcurrentDictionary<BattleTechResourceType, FieldInfo>();
      private static SpinLock cacheLock = new SpinLock();
      public static void ParceJson(ref IJsonTemplated jsonTemplate, BattleTechResourceType resType, string json) {
        switch (resType) {
          case BattleTechResourceType.AbilityDef: JSONSerializationUtility.FromJSON<AbilityDef>(jsonTemplate as AbilityDef, json); break;
          case BattleTechResourceType.AmmunitionBoxDef: JSONSerializationUtility.FromJSON<AmmunitionBoxDef>(jsonTemplate as AmmunitionBoxDef, json); break;
          case BattleTechResourceType.AmmunitionDef: JSONSerializationUtility.FromJSON<AmmunitionDef>(jsonTemplate as AmmunitionDef, json); break;
          case BattleTechResourceType.BackgroundDef: JSONSerializationUtility.FromJSON<BackgroundDef>(jsonTemplate as BackgroundDef, json); break;
          case BattleTechResourceType.BackgroundQuestionDef: JSONSerializationUtility.FromJSON<BackgroundQuestionDef>(jsonTemplate as BackgroundQuestionDef, json); break;
          case BattleTechResourceType.BaseDescriptionDef: JSONSerializationUtility.FromJSON<BaseDescriptionDef>(jsonTemplate as BaseDescriptionDef, json); break;
          case BattleTechResourceType.BuildingDef: JSONSerializationUtility.FromJSON<BuildingDef>(jsonTemplate as BuildingDef, json); break;
          case BattleTechResourceType.CastDef: JSONSerializationUtility.FromJSON<CastDef>(jsonTemplate as CastDef, json); break;
          case BattleTechResourceType.ChassisDef: JSONSerializationUtility.FromJSON<ChassisDef>(jsonTemplate as ChassisDef, json); break;
          case BattleTechResourceType.DesignMaskDef: JSONSerializationUtility.FromJSON<DesignMaskDef>(jsonTemplate as DesignMaskDef, json); break;
          case BattleTechResourceType.FactionDef: JSONSerializationUtility.FromJSON<FactionDef>(jsonTemplate as FactionDef, json); break;
          case BattleTechResourceType.FlashpointDef: JSONSerializationUtility.FromJSON<FlashpointDef>(jsonTemplate as FlashpointDef, json); break;
          case BattleTechResourceType.HardpointDataDef: JSONSerializationUtility.FromJSON<HardpointDataDef>(jsonTemplate as HardpointDataDef, json); break;
          case BattleTechResourceType.HeatSinkDef: JSONSerializationUtility.FromJSON<HeatSinkDef>(jsonTemplate as HeatSinkDef, json); break;
          case BattleTechResourceType.HeraldryDef: JSONSerializationUtility.FromJSON<HeraldryDef>(jsonTemplate as HeraldryDef, json); break;
          case BattleTechResourceType.JumpJetDef: JSONSerializationUtility.FromJSON<JumpJetDef>(jsonTemplate as JumpJetDef, json); break;
          case BattleTechResourceType.LanceDef: JSONSerializationUtility.FromJSON<LanceDef>(jsonTemplate as LanceDef, json); break;
          case BattleTechResourceType.MechDef: JSONSerializationUtility.FromJSON<MechDef>(jsonTemplate as MechDef, json); break;
          case BattleTechResourceType.MovementCapabilitiesDef: JSONSerializationUtility.FromJSON<MovementCapabilitiesDef>(jsonTemplate as MovementCapabilitiesDef, json); break;
          case BattleTechResourceType.PathingCapabilitiesDef: JSONSerializationUtility.FromJSON<PathingCapabilitiesDef>(jsonTemplate as PathingCapabilitiesDef, json); break;
          case BattleTechResourceType.PilotDef: JSONSerializationUtility.FromJSON<PilotDef>(jsonTemplate as PilotDef, json); break;
          case BattleTechResourceType.RegionDef: JSONSerializationUtility.FromJSON<RegionDef>(jsonTemplate as RegionDef, json); break;
          case BattleTechResourceType.StarSystemDef: JSONSerializationUtility.FromJSON<StarSystemDef>(jsonTemplate as StarSystemDef, json); break;
          case BattleTechResourceType.TurretChassisDef: JSONSerializationUtility.FromJSON<TurretChassisDef>(jsonTemplate as TurretChassisDef, json); break;
          case BattleTechResourceType.TurretDef: JSONSerializationUtility.FromJSON<TurretDef>(jsonTemplate as TurretDef, json); break;
          case BattleTechResourceType.UpgradeDef: JSONSerializationUtility.FromJSON<UpgradeDef>(jsonTemplate as UpgradeDef, json); break;
          case BattleTechResourceType.VehicleChassisDef: JSONSerializationUtility.FromJSON<VehicleChassisDef>(jsonTemplate as VehicleChassisDef, json); break;
          case BattleTechResourceType.VehicleDef: JSONSerializationUtility.FromJSON<VehicleDef>(jsonTemplate as VehicleDef, json); break;
          case BattleTechResourceType.WeaponDef: JSONSerializationUtility.FromJSON<WeaponDef>(jsonTemplate as WeaponDef, json); break;
          default: throw new Exception("Unknown type " + resType);
        }
      }
      public void Load() {
        do {
          PrewarmItem item = null;
          try {
            item = FastDataLoadHelper.prewarmQueue.Dequeue();
          } catch (InvalidOperationException) {
            break;
          }
          string content = string.Empty;
          try {
            if (dataManager.Exists(item.resType, item.id)) { continue; }
            //Log.M.TWL(0, "PrewarmThread[" + index + "]:" + item.resType + ":" + item.path);
            object definition = Activator.CreateInstance(GetTypeFromBattletechResource(item.resType));
            IJsonTemplated jsonTemplate = definition as IJsonTemplated;
            if (jsonTemplate == null) { continue; }
            MechDef mechDef = jsonTemplate as MechDef;
            if (mechDef == null) {
              using (var reader = new StreamReader(item.path)) {
                content = reader.ReadToEnd();
                jsonTemplate.FromJSON(content);
              }
            } else {
              byte[] hash = new byte[] { };
              bool cached = FixedMechDefHelper.GetFromCache(item.id, item.path, ref content, ref hash);
              //using (var reader = new StreamReader(path)) {
              //content = reader.ReadToEnd();
              if (cached == false) {
                string origContent = content;
                HashSet<jtProcGenericEx> procs = item.path.getLocalizationProcs();
                if (procs != null) {
                  bool updated = CustomTranslation.Core.LocalizeString(ref content, item.path, procs);
                }
                mechDef.FromJSON(content);
                if ((mechDef.MechTags.Contains(FastDataLoadHelper.NOAUTOFIX_TAG) == false) && (mechDef.MechTags.Contains(FastDataLoadHelper.FAKE_VEHICLE_TAG) == false)) {
                  FixedMechDefHelper.AddToCache(item.id, origContent, hash);
                }
              } else {
                HashSet<jtProcGenericEx> procs = item.path.getLocalizationProcs();
                if (procs != null) {
                  bool updated = CustomTranslation.Core.LocalizeString(ref content, item.path, procs);
                }
                mechDef.FromJSON(content);
              }
              //}
              //if (path == item.path) {
              //  if ((mechDef.MechTags.Contains(FastDataLoadHelper.NOAUTOFIX_TAG) == false) && (mechDef.MechTags.Contains(FastDataLoadHelper.FAKE_VEHICLE_TAG) == false)) {
              //    FixedMechDefHelper.StoreOriginalContent(item.id, JObject.Parse(content), File.GetLastWriteTime(item.path));
              //    Log.M?.TWL(0, item.id + " original");
              //  }
              //} else {
              //  if (mechDef.MechTags.Contains(FastDataLoadHelper.NOAUTOFIX_TAG) == false) {
              //    Log.M?.TWL(0, item.id + " cached. without "+ FastDataLoadHelper.NOAUTOFIX_TAG+"?");
              //  }
              //}
            }
            //Log.M.TWL(0, "PrewarmThread[" + index + "]:parsing success");
            //ParceJson(ref jsonTemplate, item.resType, content);
            object store = null;
            if (Field_cache.TryGetValue(item.resType, out FieldInfo storeField) == false) {
              storeField = this.dataManager.GetType().GetField(GetStoreNameFromBattletechResource(item.resType), BindingFlags.Instance | BindingFlags.NonPublic);
              Field_cache.AddOrUpdate(item.resType, storeField, (k, v) => { return storeField; });
            }
            store = storeField.GetValue(this.dataManager);
            if (store == null) {
              Log.M?.TWL(0, "STORAGE: " + GetStoreNameFromBattletechResource(item.resType) + " is null");
              continue;
            }
            MethodInfo addMethod = null;
            if (addMethods_cache.TryGetValue(store, out addMethod) == false) {
              addMethod = store.GetType().GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
              if (addMethod == null) {
                Log.M?.TWL(0, "STORAGE: " + GetStoreNameFromBattletechResource(item.resType) + " has no Add method");
              } else {
                addMethods_cache.AddOrUpdate(store, addMethod, (k, v) => { return addMethod; });
              }
            }
            bool locked = false;
            try {
              //Log.M.TWL(0, "PrewarmThread[" + index + "]: waiting lock");
              cacheLock.Enter(ref locked);
              //Log.M.TWL(0, "PrewarmThread[" + index + "]: lock getted");
              if (addMethod != null) addMethod.Invoke(store, new object[] { item.id, definition });
            } catch (Exception e) {
              if (locked) {
                cacheLock.Exit(); locked = false;
                //Log.M.TWL(0, "PrewarmThread[" + index + "]: lock released");
              }
              Log.M?.TWL(0, "PrewarmThread[" + index + "]:" + item.id + ":" + item.path + "\n" + e.ToString(), true);
              Log.M?.TWL(0, content == null ? "null" : JsonConvert.SerializeObject(definition, Formatting.Indented));
            }
            if (locked) {
              cacheLock.Exit(); locked = false;
              //Log.M?.TWL(0, "PrewarmThread[" + index + "]: lock released");
            }
          } catch (Exception e) {
            Log.M?.TWL(0, "PrewarmThread[" + index + "]:" + item.id + ":" + item.path + "\n" + e.ToString(), true);
            Log.M?.TWL(0, content);
            //break;
          }
        } while (true);
        Log.M?.TWL(0, "PrewarmThread[" + index + "]: exit", true);
      }
    }
    public static void PrewarmDoWorkDelegate(object param) {
      PrewarmDelegate threadItem = param as PrewarmDelegate;
      if (threadItem != null) { threadItem.Load(); }
    }
    public static void AddAllOfTypeBlindLoadRequestPrewarm(this LoadRequest loadRequest, BattleTechResourceType resourceType, bool? filterByOwnerShip = false) {
      if (prewarmTypes.Contains(resourceType) == false) {
        loadRequest.AddAllOfTypeBlindLoadRequest(resourceType, filterByOwnerShip);
        return;
      }
      DataManager dataManager = loadRequest.dataManager;
      foreach (VersionManifestEntry versionManifestEntry in dataManager.ResourceLocator.AllEntriesOfResource(resourceType)) {
        if (versionManifestEntry.IsTemplate) { continue; }
        if (dataManager.Exists(resourceType, versionManifestEntry.Id)) { continue; }
        loadRequest.AddBlindLoadRequest(resourceType, versionManifestEntry.Id, filterByOwnerShip);
      }
    }
    public static Queue<PilotableActorDef> validateUnitsDefs = new Queue<PilotableActorDef>();
    public static void ValidateMechDefWork() {
      do {
        PilotableActorDef item = null;
        try {
          item = FastDataLoadHelper.validateUnitsDefs.Dequeue();
        } catch (InvalidOperationException) {
          break;
        }
        try {
          if (item == null) { continue; }
          if (item.DataManager == null) { item.DataManager = simgame.DataManager; }
          MechDef mech = item as MechDef;
          if (mech != null) {
            mech.Refresh();
            if (mech.Chassis == null) {
              Log.M?.TWL(0, item.Description.Id + " still has null chassis:" + item.ChassisID + " datamanager:" + item.DataManager.Exists(BattleTechResourceType.ChassisDef, item.ChassisID));
            }
            continue;
          }
          VehicleDef vehicle = item as VehicleDef;
          if (vehicle != null) {
            vehicle.Refresh();
            if (vehicle.Chassis == null) {
              Log.M?.TWL(0, item.Description.Id + " still has null chassis:" + item.ChassisID + " datamanager:" + item.DataManager.Exists(BattleTechResourceType.VehicleChassisDef, item.ChassisID));
            }
            continue;
          }
        } catch (Exception e) {
          Log.M?.TWL(0, e.ToString(), true);
        }
        if (FastDataLoadHelper.validateUnitsDefs.Count == 0) { break; };
      } while (true);
    }
    public static bool CheckUnitsChassis() {
      Log.M?.TWL(0, "FastDataLoadHelper.CheckUnitsChassis start", true);
      HashSet<MechDef> toDeleteMech = new HashSet<MechDef>();
      HashSet<VehicleDef> toDeleteVehicle = new HashSet<VehicleDef>();
      bool result = true;
      foreach (var mech in simgame.DataManager.MechDefs) {
        if (mech.Value.Chassis == null) {
          Log.M?.WL(1, mech.Key + " has no chassis " + mech.Value.ChassisID + " chassis exists:" + simgame.DataManager.ChassisDefs.Exists(mech.Value.ChassisID));
          if (simgame.DataManager.ChassisDefs.Exists(mech.Value.ChassisID)) {
            mech.Value.Refresh();
            Log.M?.WL(2, (mech.Value.Chassis == null ? "null" : mech.Value.Chassis.Description.Id));
            if (mech.Value.Chassis == null) { toDeleteMech.Add(mech.Value); result = false; }
          }
        }
      }
      foreach (var vehicle in simgame.DataManager.VehicleDefs) {
        if (vehicle.Value.Chassis == null) {
          Log.M?.WL(1, vehicle.Key + " has no chassis " + vehicle.Value.ChassisID + " vehicle chassis exists:" + simgame.DataManager.VehicleChassisDefs.Exists(vehicle.Value.ChassisID));
          if (simgame.DataManager.VehicleChassisDefs.Exists(vehicle.Value.ChassisID)) {
            vehicle.Value.Refresh();
            Log.M?.WL(2, (vehicle.Value.Chassis == null ? "null" : vehicle.Value.Chassis.Description.Id));
            if (vehicle.Value.Chassis == null) { toDeleteVehicle.Add(vehicle.Value); result = false; }
          }
        }
      }
      foreach (MechDef def in toDeleteMech) {
        simgame.DataManager.mechDefs.Remove(def.Description.Id);
      }
      foreach (VehicleDef def in toDeleteVehicle) {
        simgame.DataManager.vehicleDefs.Remove(def.Description.Id);
      }
      Log.M?.TWL(0, "FastDataLoadHelper.CheckUnitsChassis end", true);
      return result;
    }
    public static void ValidateMechDefs() {
      try {
        foreach (var mech in simgame.DataManager.MechDefs) {
          if (mech.Value.Chassis == null) { validateUnitsDefs.Enqueue(mech.Value); }
          //if (mech.Value.MechTags.Contains(FastDataLoadHelper.NOAUTOFIX_TAG) == false) {
          //  mech.Value.MechTags.Add(FastDataLoadHelper.NOAUTOFIX_TAG);
          //}
        }
        foreach (var vehicle in simgame.DataManager.VehicleDefs) {
          if (vehicle.Value.Chassis == null) { validateUnitsDefs.Enqueue(vehicle.Value); }
        }
        //HashSet<Thread> threads = new HashSet<Thread>();
        //Log.M?.TWL(0, "DataManager.ValidateMechs: " + validateUnitsDefs.Count, true);
        //for (int t = 0; t < 16; ++t) {
        //  Thread thread = new Thread(ValidateMechDefWork);
        //  thread.Start();
        //  threads.Add(thread);
        //}
        Log.M?.TWL(0, "units validation start:" + stageWatcher.Elapsed.TotalSeconds, true);
        ValidateMechDefWork();
        Log.M?.TWL(0, "units validation end:" + stageWatcher.Elapsed.TotalSeconds, true);
        while (CheckUnitsChassis() == false) { };
        stage = FastLoadStage.RefreshMechs;
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
    public static void PrewarmAllCompleeteDelegate() {
      //stageWatcher.Stop();
      Log.M?.TWL(0, "FastDataLoadHelper.Deps success:" + stageWatcher.Elapsed.TotalSeconds);
      //ValidateMechDefs();
      //Final();
      //stageWatcher.Start();
    }
    public static void PrewarmAllFailDelegate() {
      //stageWatcher.Stop();
      Log.M?.TWL(0, "FastDataLoadHelper.Deps fail:" + stageWatcher.Elapsed.TotalSeconds);
      //ValidateMechDefs();
      //Final();
      //stageWatcher.Start();
    }
    private static SimGameState simgame = null;
    private static Stopwatch preloadTimer = new Stopwatch();
    public static void Update() {
      if (stage == FastLoadStage.None) { return; }
      if (simgame == null) { return; }
      if (stage == FastLoadStage.Preload) {
        if (preloadTimer.IsRunning == false) { preloadTimer.Restart(); }
        if (preloadTimer.Elapsed.TotalSeconds > 10.0) {
          Log.M?.TWL(0, "FastDataLoadHelper.Prewarming: " + prewarmQueue.Count + " time:" + stageWatcher.Elapsed.TotalSeconds);
          preloadTimer.Restart();
        }
        if (prewarmQueue.Count > 0) { return; }
        preloadTimer.Stop(); preloadTimer.Reset();
        ValidateMechDefs();
      } else if (stage == FastLoadStage.RefreshMechs) {
        if (preloadTimer.IsRunning == false) { preloadTimer.Restart(); }
        if (preloadTimer.Elapsed.TotalSeconds > 2.0) {
          Log.M?.TWL(0, "FastDataLoadHelper.RefreshMechs: " + validateUnitsDefs.Count + " time:" + stageWatcher.Elapsed.TotalSeconds);
          preloadTimer.Restart();
        }
        if (validateUnitsDefs.Count > 0) { return; }
        preloadTimer.Stop(); preloadTimer.Reset();
        GatherDependencies();
      }
    }
    public static void PrepareDataBase() {

    }
    public static void GatherDependencies() {
      stage = FastLoadStage.Dependencies;
      Log.M?.TWL(0, "FastDataLoadHelper.Prewarm complete:" + stageWatcher.Elapsed.TotalSeconds);
      DataManager.InjectedDependencyLoadRequest dependencyLoad = new DataManager.InjectedDependencyLoadRequest(simgame.DataManager);
      foreach (BattleTechResourceType resType in prewarmTypes) {
        VersionManifestEntry[] manifest = simgame.DataManager.ResourceLocator.AllEntriesOfResource(resType, false);
        foreach (VersionManifestEntry entry in manifest) {
          if (entry.IsAssetBundled) { continue; }
          if (entry.IsFileAsset == false) { continue; }
          if (entry.IsTemplate) { continue; }
          DataManager.ILoadDependencies item = simgame.DataManager.Get(resType, entry.Id) as DataManager.ILoadDependencies;
          if (item == null) { continue; }
          try {
            if (item.DependenciesLoaded(10u)) { continue; }
          } catch (Exception) {
            //Log.M.TWL(0,entry.Id+":"+resType);
            //Log.M.TWL(0, e.StackTrace, true);
          }
          item.GatherDependencies(simgame.DataManager, dependencyLoad, 10u);
        }
      }
      Log.M.TWL(0, "DataManager.ProcessPrewarmRequests Dependencies:" + dependencyLoad.DependencyCount());
      if (dependencyLoad.DependencyCount() > 0) {
        dependencyLoad.RegisterLoadCompleteCallback(new Action(PrewarmAllCompleeteDelegate));
        dependencyLoad.RegisterLoadFailedCallback(new Action(PrewarmAllFailDelegate));
        simgame.DataManager.InjectDependencyLoader(dependencyLoad, 10U);
      }
    }
    public static void FastPrewarm(SimGameState sg) {
      try {
        Log.M?.TWL(0, "DataManager.FastPrewarm", true);
        FastDataLoadHelper.simgame = sg;
        stage = FastLoadStage.Preload;
        stageWatcher.Start();
        foreach (BattleTechResourceType resType in prewarmTypes) {
          VersionManifestEntry[] manifest = simgame.DataManager.ResourceLocator.AllEntriesOfResource(resType, false);
          foreach (VersionManifestEntry entry in manifest) {
            if (entry.IsAssetBundled) { continue; }
            if (entry.IsFileAsset == false) { continue; }
            if (entry.IsTemplate) { continue; }
            if (simgame.DataManager.Exists(resType, entry.Id)) { continue; }
            prewarmQueue.Enqueue(new PrewarmItem(entry.FilePath, entry.Id, resType));
          }
        }
        HashSet<Thread> threads = new HashSet<Thread>();
        Log.M?.TWL(0, "DataManager.FastPrewarm: " + prewarmQueue.Count, true);
        for (int t = 0; t < THREADS_COUNT; ++t) {
          Thread thread = new Thread(PrewarmDoWorkDelegate);
          thread.Start(new PrewarmDelegate(simgame.DataManager, t));
          threads.Add(thread);
        }
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
  }
}