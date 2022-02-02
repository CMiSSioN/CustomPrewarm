using BattleTech;
using BattleTech.Data;
using BattleTech.Save;
using BattleTech.Save.SaveGameStructure;
using BattleTech.Save.Test;
using BattleTech.UI;
using Harmony;
using HBS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomPrewarm {
  [HarmonyPatch(typeof(SimGameContext), "Rehydrate")]
  public static class SimGameContext_Rehydrate {
    private static bool callOriginal = false;
    public static bool Prefix(SimGameContext __instance, SimGameState simState,SimGameSave simGameSave, SerializableReferenceContainer references) {
      if (callOriginal) { return true; }
      callOriginal = true;
      try {
        Log.M?.TWL(0, "SimGameContext.Rehydrate");
        Log.M?.WL(1, "singleObjectReferences");
        foreach (var singleRefs in simGameSave.GlobalReferences.singleObjectReferences) {
          Log.M?.WL(2, "name:"+ singleRefs.Key+" GUID:"+ singleRefs.Value, true);
        }
        Log.M?.WL(1, "handlers");
        foreach (var handler in __instance.typeHandlerLookup) {
          IGuid handler_GUID = handler.Value as IGuid;
          Log.M?.WL(2,"GUID:"+ (handler_GUID==null?"null": handler_GUID.GUID)+" Type:"+ handler.Value.GetType().Name);
          handler.Value.Rehydrate(simState, (GameContext)__instance, simGameSave, references);
        }
      } catch (Exception e) {
        Log.M_Err?.TWL(0, e.ToString(), true);
      }
      callOriginal = false;
      return false;
    }
  }
  [HarmonyPatch(typeof(MetadataDatabase), "WriteInMemoryDBToDisk")]
  public static class MetadataDatabase_WriteInMemoryDBToDisk {
    private static bool callOriginal = false;
    public static bool Prefix(MetadataDatabase __instance, string filePath) {
      if (callOriginal) { return true; }
      callOriginal = true;
      try {
        Log.M?.TWL(0, "MetadataDatabase.WriteInMemoryDBToDisk "+filePath);
        Log.M?.WL(1,Environment.StackTrace,true);
        __instance.WriteInMemoryDBToDisk(filePath);
      } catch (Exception e) {
        Log.M_Err?.TWL(0, e.ToString(), true);
      }
      callOriginal = false;
      return false;
    }
  }
  [HarmonyPatch(typeof(SimGameUXCreator), "OnSimGameInitializeComplete")]
  public static class SimGameUXCreator_OnSimGameInitializeComplete {
    public static IEnumerator InitializeUXRoutineLoc(this SimGameUXCreator __instance) {
      if (!__instance.sim.RoomManager.OnSimGameInitialize()) {
        Debug.LogError((object)"[SimGameUXCreator.OnSimGameInitializeComplete skipped!");
        __instance.activeRoutine = (Coroutine)null;
      } else {
        SimGameCameraController camController = null;
        try {
          camController = UnityEngine.Object.FindObjectOfType<SimGameCameraController>();
          camController.Init(__instance.sim);
          UnityEngine.Object.FindObjectOfType<SimGameSpaceController>().Init(__instance.sim);
        }catch(Exception e) {
          Log.M?.TWL(0, e.ToString(), true);
        }
        yield return (object)null;
        try {
          __instance.sim.SetStarmap(camController.gameObject.AddComponent<Starmap>());
          __instance.sim.Starmap.PopulateMap(__instance.sim);
          LazySingletonBehavior<UIManager>.Instance.GetOrCreatePopupModule<SGDialogWidget>().Init(__instance.sim);
          LazySingletonBehavior<UIManager>.Instance.GetOrCreatePopupModule<UITitleScreenOverlay>().Init(__instance.sim);
        }catch(Exception e) {
          Log.M?.TWL(0, e.ToString(), true);
        }
        yield return (object)null;
        try { 
          LazySingletonBehavior<UIManager>.Instance.GetOrCreatePopupModule<UISummaryScreenOverlay>().Init(__instance.sim);
          __instance.sim.SetCharacterCreationWidget(LazySingletonBehavior<UIManager>.Instance.GetOrCreateUIModule<SGCharacterCreationWidget>());
        } catch (Exception e) {
          Log.M?.TWL(0, e.ToString(), true);
        }
        yield return (object)null;
        try {
          __instance.sim.SetBattleSimPanel(LazySingletonBehavior<UIManager>.Instance.GetOrCreateUIModule<SGSimBattleWidget>());
          LazySingletonBehavior<UIManager>.Instance.GetOrCreateUIModule<SGDebugEventWidget>().Init(__instance.sim);
          __instance.activeRoutine = (Coroutine)null;
        } catch (Exception e) {
          Log.M?.TWL(0, e.ToString(), true);
        }
      }
    }
    public static bool Prefix(SimGameUXCreator __instance, MessageCenterMessage message) {
      try {
        Log.M?.TWL(0, "SimGameUXCreator.OnSimGameInitializeComplete");
        __instance.activeRoutine = __instance.StartCoroutine(__instance.InitializeUXRoutineLoc());
      } catch (Exception e) {
        Log.M_Err?.TWL(0, e.ToString(), true);
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(SGRoomController_Navigation), "LeaveRoom")]
  public static class SGRoomController_Navigation_LeaveRoom {
    public static bool Prefix(SGRoomController_Navigation __instance, bool ignoreFader) {
      try {
        Log.M?.TWL(0, "SGRoomController_Navigation.LeaveRoom " + ignoreFader);
        __instance.curMenuType = DropshipMenuType.INVALID_UNSET;
        __instance.navigationScreen.LeaveRoomPopupCheck();
        __instance.navigationScreen.Visible = false;
        __instance.roomManager.SetAllUIVisibility(true);
        if(__instance.simState == null) {
          Log.M?.WL(1, "Warning simState is null");
          __instance.simState = BattleTech.UnityGameInstance.BattleTechGame.Simulation;
        }
        if (__instance.simState.Starmap == null) {
          Log.M?.WL(1, "Warning simState.Starmap is null");
        }
        if (__instance.simState.Starmap.Screen == null) {
          Log.M?.WL(1, "Warning simState.Starmap.Screen is null");
        }
        __instance.simState.Starmap.Screen.StarmapVisible = false;
        __instance.roomActive = false;
      } catch (Exception e) {
        Log.M_Err?.TWL(0, e.ToString(), true);
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(SimGameContext.MechDefHandler), "Rehydrate")]
  public static class MechDefHandler_Rehydrate {
    public static bool Prefix(SimGameContext.MechDefHandler __instance, SimGameState simState, GameContext gameContext, SimGameSave simGameSave, SerializableReferenceContainer references) {
      try {
        Log.M?.TWL(0, "SimGameContext.MechDefHandler.Rehydrate");
        foreach (GameContextObjectTagEnum setTagEnum in __instance.setTagEnums) {
          string name = setTagEnum.ToString();
          Log.M?.WL(1, name);
          try {
            MechDef mechDef = simGameSave.GlobalReferences.GetItem<MechDef>((IGuid)__instance, name);
            gameContext.SetObject(setTagEnum, (object)mechDef);
          }catch(Exception e) {
            Log.M?.WL(2,e.Message);
          }
        }
      } catch (Exception e) {
        Log.M_Err?.TWL(0, e.ToString(), true);
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(SimGameContext.PilotHandler), "Rehydrate")]
  public static class PilotHandler_Rehydrate {
    public static bool Prefix(SimGameContext.PilotHandler __instance, SimGameState simState, GameContext gameContext, SimGameSave simGameSave, SerializableReferenceContainer references) {
      try {
        Log.M?.TWL(0, "SimGameContext.PilotHandler.Rehydrate");
        foreach (GameContextObjectTagEnum setTagEnum in __instance.setTagEnums) {
          string name = setTagEnum.ToString();
          Log.M?.WL(1, name);
          try {
            Pilot pilot = simGameSave.GlobalReferences.GetItem<Pilot>((IGuid)__instance, name);
            gameContext.SetObject(setTagEnum, (object)pilot);
          } catch (Exception e) {
            Log.M?.WL(2, e.Message);
          }
        }
      } catch (Exception e) {
        Log.M_Err?.TWL(0, e.ToString(), true);
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(SimGameContext.StarSystemHandler), "Rehydrate")]
  public static class StarSystemHandler_Rehydrate {
    public static bool Prepare() { return true; }
    public static bool Prefix(SimGameContext.StarSystemHandler __instance, SimGameState simState, GameContext gameContext, SimGameSave simGameSave, SerializableReferenceContainer references) {
      try {
        Log.M?.TWL(0, "SimGameContext.StarSystemHandler.Rehydrate");
        foreach (GameContextObjectTagEnum setTagEnum in __instance.setTagEnums) {
          string name = setTagEnum.ToString();
          Log.M?.WL(1, name);
          try {
            StarSystem starSystem = references.GetItem<StarSystem>((IGuid)__instance, name);
            gameContext.SetObject(setTagEnum, (object)starSystem);
          } catch (Exception e) {
            Log.M?.WL(2, e.Message);
          }
        }
      } catch (Exception e) {
        Log.M_Err?.TWL(0, e.ToString(), true);
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(SimGameContext.FactionDefHandler), "Rehydrate")]
  public static class FactionDefHandler_Rehydrate {
    public static bool Prefix(SimGameContext.FactionDefHandler __instance, SimGameState simState, GameContext gameContext, SimGameSave simGameSave, SerializableReferenceContainer references) {
      try {
        Log.M?.TWL(0, "SimGameContext.FactionDefHandler.Rehydrate");
        if (simGameSave.FactionDefContainerValues == null || simGameSave.FactionDefContainerTags == null) { return false; }
        foreach (GameContextObjectTagEnum setTagEnum in __instance.setTagEnums) {
          try {
            string str = setTagEnum.ToString();
            Log.M?.WL(1, str);
            if (simGameSave.FactionDefContainerTags.Contains(str)) {
              string defContainerValue = simGameSave.FactionDefContainerValues[simGameSave.FactionDefContainerTags.IndexOf(str)];
              FactionDef factionDef = simState.DataManager.Factions.Get(defContainerValue);
              if (simGameSave.FactionDefContainerTags.Contains(setTagEnum.ToString()))
                gameContext.SetObject(setTagEnum, (object)factionDef);
            }
          }catch(Exception e) {
            Log.M?.WL(2, e.Message);
          }
        }
      } catch (Exception e) {
        Log.M_Err?.TWL(0, e.ToString(), true);
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(SimGameContext.FlashpointHandler), "Rehydrate")]
  public static class FlashpointHandler_Rehydrate {
    public static bool Prefix(SimGameContext.FlashpointHandler __instance, SimGameState simState, GameContext gameContext, SimGameSave simGameSave, SerializableReferenceContainer references) {
      try {
        Log.M?.TWL(0, "SimGameContext.FlashpointHandler.Rehydrate");
        foreach (GameContextObjectTagEnum setTagEnum in __instance.setTagEnums) {
          string name = setTagEnum.ToString();
          Log.M?.WL(1, name);
          try {
            Flashpoint flashpoint = references.GetItem<Flashpoint>((IGuid)__instance, name);
            if (flashpoint == null)
              Log.M?.TWL(0, string.Format("Flashpoint attatched to TagEnum {0} was null coming from Ref Container", name), true);
            else
              gameContext.SetObject(setTagEnum, (object)flashpoint);
          }catch(Exception e) {
            Log.M?.WL(2, e.Message);
          }
        }
      } catch (Exception e) {
        Log.M_Err?.TWL(0, e.ToString(), true);
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(GameInstance), "CreateSim")]
  public static class GameInstance_CreateSim {
    private static bool callOriginal = false;
    public static bool Prefix(GameInstance __instance, SimGameState.SimGameType gameType, bool debug) {
      if (callOriginal) { return true; }
      callOriginal = true;
      try {
        Log.M?.TWL(0, "GameInstance.CreateSim gameType:" + gameType+" debug:"+debug, true);
        __instance.CreateSim(gameType, debug);
      } catch (Exception e) {
        Log.M_Err?.TWL(0, e.ToString(), true);
      }
      callOriginal = false;
      return false;
    }
  }
  [HarmonyPatch(typeof(BattleTech.Save.SaveGameStructure.SaveGameStructure), "GetAllCareerSlots")]
  public static class SaveGameStructure_GetAllCareerSlots {
    //private static bool callOriginal = false;
    public static void Postfix(SaveGameStructure __instance,ref List<SlotModel> __result) {
      //if (callOriginal) { return true; }
      //callOriginal = true;
      //try {
        Log.M?.TWL(0, "SaveGameStructure.GetAllCareerSlots gameType:" + __result.Count, true);
        //__instance.CreateSim(gameType, debug);
      //} catch (Exception e) {
        //Log.M_Err?.TWL(0, e.ToString(), true);
      //}
      //callOriginal = false;
      //return false;
    }
  }
  [HarmonyPatch(typeof(MainMenu), "EnableCareerLoadIfCareerSaves")]
  public static class MainMenu_EnableCareerLoadIfCareerSaves {
    private static bool callOriginal = false;
    public static bool Prefix(MainMenu __instance, MessageCenterMessage message) {
      if (callOriginal) { return true; }
      callOriginal = true;
      try {
      Log.M?.TWL(0, "MainMenu.EnableCareerLoadIfCareerSaves", true);
        __instance.EnableCareerLoadIfCareerSaves(message);
      } catch (Exception e) {
      Log.M_Err?.TWL(0, e.ToString(), true);
      }
      callOriginal = false;
      return false;
    }
  }

}