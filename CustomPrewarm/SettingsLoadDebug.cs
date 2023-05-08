using BattleTech;
using BattleTech.Save;
using BattleTech.Serialization;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomPrewarm {
  [HarmonyPatch(typeof(UnityGameInstance))]
  [HarmonyPatch("InitUserSettings")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class UnityGameInstance_InitUserSettings {
    public static bool Prefix(UnityGameInstance __instance) {
      try {
        ActiveOrDefaultSettings.SetSaveManager(__instance.Game.SaveManager);
        ActiveOrDefaultSettings.LoadUserSettings((Action)(() =>
        {
          ActiveOrDefaultSettings.CloudSettings.CustomUnitsAndLances.MountMemoryStore(__instance.Game.DataManager);
          UserSettings settings = ActiveOrDefaultSettings.Settings;
          bool forceSave = false;
          if (FirstRun.FirstApplicationRun) {
            settings.LocalSettings.advancedSettings.SetDefaultQuality(2);
            QualitySettings.SetQualityLevel(2);
            forceSave = true;
          }
          if (FirstRun.VSyncUpdate) {
            settings.LocalSettings.advancedSettings.VsyncEnabled = 1;
            forceSave = true;
          }
          Cursor.lockState = ActiveOrDefaultSettings.CloudSettings.lockCursor ? CursorLockMode.Confined : CursorLockMode.None;
          __instance.SyncUnityVideoSettings(forceSave);
        }));
        return false;
      } catch (Exception e) {
        Log.M_Err?.TWL(0,e.ToString(),true);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(UnityGameInstance))]
  [HarmonyPatch("OnModsInitComplete")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class UnityGameInstance_OnModsInitComplete {
    public class OnModsInitCompleteDelay {
      public UnityGameInstance __instance { get; set; } = null;
      public OnModsInitCompleteDelay(UnityGameInstance __instance) {
        this.__instance = __instance;
      }
      public IEnumerator waitforSettings() {
        if (ActiveOrDefaultSettings.userSettings == null) { yield return null; }
        if (ActiveOrDefaultSettings.userSettings.activeLoadRequests > 0) { yield return null; }
        __instance.OnModsInitComplete();
        yield break;
      }
    }
    public static bool Prefix(UnityGameInstance __instance) {
      try {
        Log.M?.TWL(0, $"UnityGameInstance.OnModsInitComplete activeLoadRequests:{(ActiveOrDefaultSettings.userSettings==null?"null": ActiveOrDefaultSettings.userSettings.activeLoadRequests.ToString())}");
        if (ActiveOrDefaultSettings.userSettings == null) {
          UnityGameInstance.Instance.StartCoroutine(new OnModsInitCompleteDelay(__instance).waitforSettings());
          return false;
        }
        if (ActiveOrDefaultSettings.userSettings.activeLoadRequests > 0) {
          UnityGameInstance.Instance.StartCoroutine(new OnModsInitCompleteDelay(__instance).waitforSettings());
          return false;
        }
        Log.M?.WL(0,$"{ActiveOrDefaultSettings.userSettings.CloudSettings.keybindData}");
        //Log.M?.WL(0, JsonConvert.SerializeObject(ActiveOrDefaultSettings.userSettings.CloudSettings, Formatting.Indented));
      } catch (Exception e) {
        Log.M_Err?.TWL(0, e.ToString(), true);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(UserSettings))]
  [HarmonyPatch("Save")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class UserSettings_Save {
    public class SaveSettingsDelay {
      public UserSettings userSettings { get; set; } = null;
      public SaveSettingsDelay(UserSettings userSettings) {
        this.userSettings = userSettings;
      }
      public IEnumerator waitforSettings() {
        if (userSettings.activeLoadRequests > 0) { yield return null; }
        userSettings.Save();
        yield break;
      }
    }
    public static bool Prefix(UserSettings __instance) {
      try {
        Log.M?.TWL(0, $"UserSettings.Save activeLoadRequests:{__instance.activeLoadRequests}");
        if (__instance.activeLoadRequests != 0) {
          Log.M?.WL(0, "can't save settings cause they are in loading state");
          UnityGameInstance.Instance.StartCoroutine(new SaveSettingsDelay(__instance).waitforSettings());
          return false;
        }
        //Log.M?.WL(0,Environment.StackTrace);
        if (__instance.saveManager == null) {
          UserSettings.logger.LogError((object)"Save canceled. User Settings is missing a SaveManager");
          Log.M_Err?.TWL(0, "Save canceled. User Settings is missing a SaveManager");
        } else {
          if (__instance.LocalSettings != null) {
            __instance.saveManager.SaveBytes(Serializer.Serialize<LocalUserSettings>(__instance.LocalSettings, SerializationTarget.Persistence), "settings_local.sav", (Action)(() => {
              UserSettings.logger.Log((object)"User local settings saved");
              Log.M?.TWL(0, "User local settings saved");
              //Log.M?.WL(0, JsonConvert.SerializeObject(__instance.LocalSettings, Formatting.Indented));
            }), (Action<string>)(error => {
              UserSettings.logger.LogError((object)("User settings save failed: " + error));
              Log.M_Err?.TWL(0, "User settings save failed: " + error);
            }));
          } else {
            UserSettings.logger.LogWarning((object)"User Settings had NULL LocalSettings field during save.");
            Log.M_Err?.TWL(0, "User Settings had NULL LocalSettings field during save.");
          }
          if (__instance.CloudSettings != null)
            __instance.saveManager.CloudSaveBytes(Serializer.Serialize<CloudUserSettings>(__instance.CloudSettings, SerializationTarget.Persistence), "settings_cloud.sav", (Action)(() => {
              UserSettings.logger.Log((object)"User cloud settings saved");
              Log.M?.TWL(0, "User cloud settings saved");
              Log.M?.WL(0, $"{ActiveOrDefaultSettings.userSettings.CloudSettings.keybindData}");
              //Log.M?.WL(0, JsonConvert.SerializeObject(__instance.CloudSettings, Formatting.Indented));
            }), (Action<string>)(error => {
              UserSettings.logger.LogError((object)("User settings save failed: " + error));
              Log.M_Err?.TWL(0, "User settings save failed: " + error);
            }));
          else {
            UserSettings.logger.LogWarning((object)"User Settings had NULL CloudSettings field during save.");
            Log.M_Err?.TWL(0, "User Settings had NULL CloudSettings field during save.");
          }
        }
        Log.M?.WL(0, "saved effective settings");
        return false;
      } catch (Exception e) {
        Log.M_Err?.TWL(0, e.ToString(), true);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(BTInput), "LoadSavedBindings")]
  public static class BTInput_LoadSavedBindings {
    public static void Prefix(BTInput __instance) {
      Log.M?.TWL(0, $"BTInput.LoadSavedBindings {ActiveOrDefaultSettings.CloudSettings.keybindData}");
    }
  }
  [HarmonyPatch(typeof(UserSettings))]
  [HarmonyPatch("Load")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Action) })]
  public static class UserSettings_Load {
    public static bool Prefix(UserSettings __instance, Action loadComplete) {
      try {
        Log.M?.TWL(0,$"UserSettings.Load");
        Log.M?.WL(0, Environment.StackTrace);

        if (__instance.saveManager == null) {
          UserSettings.logger.LogError((object)"Load canceled. User Settings is missing a SaveManager");
          Log.M_Err?.TWL(0, $"Exception: Load canceled. User Settings is missing a SaveManager", true);
        } else {
          Log.M?.WL(1, $"localWriteLocation:{__instance.saveManager.saveSystem.localWriteLocation.rootPath}");
          Log.M?.WL(1, $"cloudWriteLocation:{__instance.saveManager.saveSystem.cloudWriteLocation.rootPath}");
          __instance.loadComplete = (Action)null;
          __instance.activeLoadRequests = 0;
          bool localSettings_exists = __instance.saveManager.ExistsLocally("settings_local.sav");
          bool cloudSettings_exists = __instance.saveManager.ExistsInCloud("settings_cloud.sav");
          if (localSettings_exists)
            ++__instance.activeLoadRequests;
          if (cloudSettings_exists)
            ++__instance.activeLoadRequests;
          if (localSettings_exists) {
            __instance.loadComplete = loadComplete;
            __instance.saveManager.LoadBytes("settings_local.sav", (Action<byte[]>)(bytes =>
            {
              try {
                LocalUserSettings localUserSettings = (LocalUserSettings)null;
                try {
                  localUserSettings = Serializer.Deserialize<LocalUserSettings>(bytes, SerializationTarget.Persistence);
                } catch (Exception ex) {
                  UserSettings.logger.LogError((object)ex);
                  Log.M_Err?.TWL(0,ex.ToString(),true);
                }
                if (localUserSettings == null) {
                  UserSettings.logger.LogError((object)"Unable to use existing local settings from save managers");
                  Log.M_Err?.TWL(0, "Unable to use existing local settings from save managers", true);
                } else {
                  UserSettings.logger.LogDebug((object)"Using existing local settings from save manager");
                  __instance.LocalSettings = localUserSettings;
                  Log.M_Err?.TWL(0, "Using existing local settings from save manager", true);
                  //Log.M?.WL(0, JsonConvert.SerializeObject(__instance.LocalSettings, Formatting.Indented));
                  //Log.M?.WL(0, $"{__instance.CloudSettings.keybindData}");
                }
                __instance.TryCompleteLoad();
              }catch(Exception e) {
                Log.M_Err?.TWL(0, e.ToString(), true);
              }
            }), (Action<string>)(error =>
            {
              UserSettings.logger.LogError((object)("Error loading local user settings, setting to CachedSettings " + error));
              Log.M_Err?.TWL(0, $"Exception: Error loading cloud user settings, setting to CachedSettings {error}", true);
              __instance.LocalSettings = CachedSettings.Settings.LocalSettings;
              //Log.M?.WL(0, JsonConvert.SerializeObject(__instance.LocalSettings, Formatting.Indented));
              //Log.M?.WL(0, $"{__instance.CloudSettings.keybindData}");
              __instance.TryCompleteLoad();
            }));
          }
          if (cloudSettings_exists) {
            __instance.loadComplete = loadComplete;
            __instance.saveManager.CloudLoadBytes("settings_cloud.sav", (Action<byte[]>)(bytes =>
            {
              try {
                CloudUserSettings cloudUserSettings = (CloudUserSettings)null;
                try {
                  cloudUserSettings = Serializer.Deserialize<CloudUserSettings>(bytes, SerializationTarget.Persistence);
                } catch (Exception ex) {
                  UserSettings.logger.LogError((object)ex);
                  Log.M_Err?.TWL(0, ex.ToString(), true);
                }
                if (cloudUserSettings == null) {
                  UserSettings.logger.LogError((object)"Unable to use existing cloud settings from save managers");
                  Log.M_Err?.TWL(0, "Unable to use existing cloud settings from save managers", true);
                } else {
                  UserSettings.logger.LogDebug((object)"Using existing cloud settings from save manager");
                  __instance.CloudSettings = cloudUserSettings;
                  Log.M_Err?.TWL(0, "Using existing cloud settings from save manager", true);
                  //Log.M?.WL(0,JsonConvert.SerializeObject(__instance.CloudSettings,Formatting.Indented));
                  BTInput.Instance.LoadSavedBindings();
                  Log.M?.WL(0, $"{__instance.CloudSettings.keybindData}");
                }
                __instance.TryCompleteLoad();
              } catch(Exception e) {
                Log.M_Err?.TWL(0,e.ToString(),true);
              }
            }), (Action<string>)(error =>
            {
              UserSettings.logger.LogError((object)("Error loading cloud user settings, setting to CachedSettings " + error));
              Log.M_Err?.TWL(0, $"Error loading cloud user settings, setting to CachedSettings {error}", true);
              __instance.CloudSettings = CachedSettings.Settings.CloudSettings;
              Log.M?.WL(0, $"{__instance.CloudSettings.keybindData}");
              BTInput.Instance.LoadSavedBindings();
              //Log.M?.WL(0, JsonConvert.SerializeObject(__instance.CloudSettings, Formatting.Indented));
              __instance.TryCompleteLoad();
            }));
          }
          if (__instance.loadComplete != null || loadComplete == null) { return false; }
          loadComplete();
        }
        Log.M?.WL(0,"loaded settings");
        return false;
      } catch (Exception e) {
        Log.M_Err?.TWL(0, e.ToString(), true);
      }
      return true;
    }
  }
}
