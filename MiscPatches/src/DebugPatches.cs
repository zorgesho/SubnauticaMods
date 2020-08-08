#if BRANCH_STABLE
using System;
using System.Reflection.Emit;
using System.Collections.Generic;
#endif
using System.Collections;

using Harmony;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Common;
using Common.Harmony;
using Common.Configuration;

namespace MiscPatches
{
	// for testing loot spawning (Main.config.loolSpawnRerollCount for control)
	[OptionalPatch, HarmonyPatch(typeof(CellManager), "GetPrefabForSlot")]
	static class CellManager_GetPrefabForSlot_Patch
	{
		static bool Prepare() => Main.config.dbg.lootSpawnRerollCount > 0;

		static bool Prefix(CellManager __instance, IEntitySlot slot, ref EntitySlot.Filler __result)
		{
			if (__instance.spawner == null || __instance.streamer.debugDisableSlotEnts)
			{
				__result = default;
				return false;
			}

			for (int i = 0; i < Main.config.dbg.lootSpawnRerollCount; i++)
			{
				__result = __instance.spawner.GetPrefabForSlot(slot, true);

				if (__result.count > 0 || slot.IsCreatureSlot())
					break;
			}

			return false;
		}
	}

	// ignore limits for propulsion cannon
	[OptionalPatch, HarmonyPatch(typeof(PropulsionCannon), "ValidateObject")]
	static class PropulsionCannon_ValidateObject_Patch
	{
		static bool Prepare() => Main.config.dbg.propulsionCannonIgnoreLimits;

		static bool Prefix(ref bool __result)
		{
			__result = true;
			return false;
		}
	}

	// keep particles alive on pause
	[HarmonyPatch(typeof(VFXController), "SpawnFX")]
	static class VFXController_SpawnFX_Patch
	{
		static bool Prepare() => Main.config.dbg.keepParticleSystemsAlive;

		public class Purge: Config.Field.IAction
		{
			public void action() =>
				UnityEngine.Object.FindObjectsOfType<VFXDestroyAfterSeconds>().forEach(vfx => vfx.lifeTime = 0f);
		}

		static void Postfix(VFXController __instance, int i)
		{
			if (__instance.emitters[i].instanceGO.GetComponent<VFXDestroyAfterSeconds>() is VFXDestroyAfterSeconds vfx)
				vfx.lifeTime = float.PositiveInfinity;
		}
	}


	[OptionalPatch, PatchClass]
	static class ScannerRoomCheat
	{
		static bool prepare() => Main.config.dbg.scannerRoomCheat;

		[HarmonyPrefix, HarmonyPatch(typeof(MapRoomFunctionality), "GetScanRange")]
		static bool MRF_GetScanRange_Prefix(ref float __result)
		{
			__result = 500f;
			return false;
		}

		[HarmonyPrefix, HarmonyPatch(typeof(MapRoomFunctionality), "GetScanInterval")]
		static bool MRF_GetScanInterval_Prefix(ref float __result)
		{
			__result = 0f;
			return false;
		}
	}

	[OptionalPatch, PatchClass]
	static class FastStart
	{
		static bool prepare() => Main.config.dbg.fastStart.enabled;

		[HarmonyPatch(typeof(MainGameController), "Start")]
#if BRANCH_STABLE
		[HarmonyTranspiler]
		static IEnumerable<CodeInstruction> fastStartPatch(IEnumerable<CodeInstruction> cins) =>
			CIHelper.ciReplace(cins, ci => ci.isOp(OpCodes.Call), OpCodes.Pop, CIHelper.emitCall<Func<IEnumerator>>(_startGame));
#elif BRANCH_EXP
		[HarmonyPrefix]
		static bool fastStartPatch(ref IEnumerator __result)
		{
			__result = _startGame();
			return false;
		}
#endif
		static IEnumerator _startGame()
		{
			Physics.autoSyncTransforms = Physics2D.autoSimulation = false;

			yield return SceneManager.LoadSceneAsync("Essentials", LoadSceneMode.Additive);

			Application.backgroundLoadingPriority = ThreadPriority.Normal;

			foreach (var cmd in Main.config.dbg.fastStart.commandsAfterLoad)
				DevConsole.SendConsoleCommand(cmd);
		}

		[HarmonyPrefix, HarmonyPatch(typeof(LightmappedPrefabs), "Awake")]
		static bool LightmappedPrefabs_Awake_Prefix(LightmappedPrefabs __instance)
		{
			if (Main.config.dbg.fastStart.loadEscapePod)
			{
				__instance.autoloadScenes = new[]
				{
					new LightmappedPrefabs.AutoLoadScene() { sceneName = "EscapePod", spawnOnStart = true }
				};
			}

			return Main.config.dbg.fastStart.loadEscapePod;
		}

		[HarmonyPrefix, HarmonyPatch(typeof(uGUI_OptionsPanel), "SyncTerrainChangeRequiresRestartText")]
		static bool ModOptionsPanelFix() => false;
	}

	// pause in ingame menu
	[OptionalPatch, HarmonyPatch(typeof(UWE.FreezeTime), "Begin")]
	static class FreezeTime_Begin_Patch
	{
		static bool Prepare() => !Main.config.dbg.ingameMenuPause;

		static bool Prefix(string userId) => userId != "IngameMenu";
	}

	// add game slot info to the load buttons
	[HarmonyPatch(typeof(MainMenuLoadPanel), "UpdateLoadButtonState")]
	static class MainMenuLoadPanel_UpdateLoadButtonState_Patch
	{
		static bool Prepare() => Main.config.dbg.showSaveSlotID;

		static void Postfix(MainMenuLoadButton lb) =>
			lb.load.FindChild("SaveGameLength").GetComponent<Text>().text += $" | {lb.saveGame}";
	}

	// change initial equipment in creative mode
	[OptionalPatch, HarmonyPatch(typeof(Player), "SetupCreativeMode")]
	static class Player_SetupCreativeMode_Patch
	{
		static bool Prepare() => Main.config.dbg.overrideInitialEquipment;

		static bool Prefix()
		{
			foreach (var item in Main.config.dbg.initialEquipment)
				CraftData.AddToInventory(item.Key, item.Value);

			KnownTech.UnlockAll(false);
			return false;
		}
	}
}