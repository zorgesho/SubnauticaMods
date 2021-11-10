using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;

using Harmony;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

using Common;
using Common.Harmony;
using Common.Reflection;
using Common.Configuration;

#if GAME_BZ || BRANCH_EXP
using System.Linq;
#endif

#if GAME_BZ
using System.Reflection;
#endif

namespace MiscPatches
{
	using Object = UnityEngine.Object;

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
	[HarmonyPatch(typeof(VFXDestroyAfterSeconds), "OnEnable")]
	static class VFXDestroyAfterSeconds_OnEnable_Patch
	{
		static bool Prepare() => Main.config.dbg.keepParticleSystemsAlive;

		public class Purge: Config.Field.IAction
		{
			public void action() =>
				Object.FindObjectsOfType<VFXDestroyAfterSeconds>().forEach(vfx => vfx.lifeTime = 0f);
		}

		static void Postfix(VFXDestroyAfterSeconds __instance) => __instance.lifeTime = float.PositiveInfinity;
	}


	[OptionalPatch, PatchClass]
	static class ScannerRoomCheat
	{
		const float scanRange = 500f;
		static bool prepare() => Main.config.dbg.scannerRoomCheat;
#if GAME_SN
		[HarmonyPostfix, HarmonyPatch(typeof(MapRoomFunctionality), "GetScanRange")]
		static void MRF_GetScanRange_Postfix(ref float __result) => __result = scanRange;

		[HarmonyPostfix, HarmonyPatch(typeof(MapRoomFunctionality), "GetScanInterval")]
		static void MRF_GetScanInterval_Postfix(ref float __result) => __result = 0f;
#elif GAME_BZ
		[HarmonyTranspiler, HarmonyPatch(typeof(MapRoomFunctionality), "UpdateScanRangeAndInterval")]
		static IEnumerable<CodeInstruction> MRF_UpdateScanRangeAndInterval_Transpiler(IEnumerable<CodeInstruction> cins)
		{
			var list = cins.ToList();

			void _replaceVal(FieldInfo field, float val) => list.ciInsert(ci => ci.isOp(OpCodes.Stfld, field), 0, 1, OpCodes.Pop, OpCodes.Ldc_R4, val);

			_replaceVal(typeof(MapRoomFunctionality).field("scanRange"), scanRange);
			_replaceVal(typeof(MapRoomFunctionality).field("scanInterval"), 0f);

			return list;
		}
#endif
	}

	[OptionalPatch, PatchClass]
	static class FastStart
	{
		static bool prepare() => Main.config.dbg.fastStart.enabled;

		[HarmonyTranspiler, HarmonyPatch(typeof(MainGameController), "Start")]
		static IEnumerable<CodeInstruction> fastStartPatch(IEnumerable<CodeInstruction> cins)
		{
			return CIHelper.ciReplace(cins, ci => ci.isOp(OpCodes.Call),
				OpCodes.Pop, CIHelper.emitCall<Func<IEnumerator>>(_startGame));

			static IEnumerator _startGame()
			{
				Physics.autoSyncTransforms = Physics2D.autoSimulation = false;
#if BRANCH_STABLE && GAME_SN
				yield return SceneManager.LoadSceneAsync("Essentials", LoadSceneMode.Additive);
#else
				yield return AddressablesUtility.LoadSceneAsync("Essentials", LoadSceneMode.Additive);
#endif
				Application.backgroundLoadingPriority = ThreadPriority.Normal;

				foreach (var cmd in Main.config.dbg.fastStart.commandsAfterLoad)
					DevConsole.SendConsoleCommand(cmd);
			}
		}
#if GAME_SN
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
#endif
		[HarmonyPrefix]
		[HarmonyPatch(typeof(PlayerWorldArrows), "ArrowUpdate")]
		[HarmonyPatch(typeof(TemperatureDamage), "OnCollisionStay")]
#if GAME_SN
		[HarmonyPatch(typeof(uGUI_OptionsPanel), "SyncTerrainChangeRequiresRestartText")]
#endif
		static bool methodDisabler() => false;

		[HarmonyPrefix, HarmonyPatch(typeof(Pickupable), "Activate")]
		static void Pickupable_Activate_Prefix(ref bool registerEntity) => registerEntity = false;
	}

	// pause in ingame menu
	[OptionalPatch, HarmonyPatch(typeof(UWE.FreezeTime), "Begin")]
	static class FreezeTime_Begin_Patch
	{
		static bool Prepare() => !Main.config.dbg.ingameMenuPause;
#if GAME_SN
		static bool Prefix(string userId) => userId != "IngameMenu";
#elif GAME_BZ
		static bool Prefix(UWE.FreezeTime.Id id) => id != UWE.FreezeTime.Id.IngameMenu;
#endif
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

	[PatchClass]
	static class DevToolsDisabler
	{
		[HarmonyPrefix, HarmonyPatch(typeof(Telemetry), "Awake")]
		static bool Telemetry_Awake_Prefix(Telemetry __instance)
		{
			Object.Destroy(__instance);
			return false;
		}

		[HarmonyPrefix]
#if GAME_SN
		[HarmonyPatch(typeof(GameAnalytics), "Send", typeof(GameAnalytics.EventInfo), typeof(string))]
#elif GAME_BZ
		[HarmonyPatch(typeof(GameAnalytics), "Send", typeof(GameAnalytics.EventInfo), typeof(bool), typeof(string))]
#endif
		static bool GameAnalytics_Send_Prefix() => false;

#if GAME_SN && BRANCH_STABLE
		[HarmonyPostfix, HarmonyPatch(typeof(MonitorLauncher), "Awake")] // fix for exception at startup
		static void MonitorLauncher_Awake_Prefix(MonitorLauncher __instance) => Object.Destroy(__instance);
#endif

#if GAME_SN && BRANCH_EXP // fix for exception in exp branch because SentrySDK is deleted by QMM
		[HarmonyTranspiler]
		[HarmonyHelper.Patch(typeof(SystemsSpawner), "SetupSingleton")]
		[HarmonyHelper.Patch(HarmonyHelper.PatchOptions.PatchIteratorMethod)]
		static IEnumerable<CodeInstruction> SetupSingleton_Transpiler(IEnumerable<CodeInstruction> cins)
		{
			var list = cins.ToList();
			var getSentrySDK = typeof(GameObject).method<SentrySdk>("GetComponent");

			int[] i = list.ciFindIndexes(ci => ci.isOp(OpCodes.Callvirt, getSentrySDK),
										 ci => ci.isOp(OpCodes.Ldc_I4_0));

			return i == null? cins: list.ciRemoveRange(i[0] - 2, i[1] - 1);
		}
#endif
	}

	[OptionalPatch, HarmonyPatch(typeof(FPSInputModule), "GetMousePointerEventData", new Type[] {})]
	static class FPSInputModule_GetMousePointerEventData_Patch
	{
		static bool Prepare() => Main.config.dbg.showRaycastResult;

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> cins)
		{
			static void _printRaycastResults(List<RaycastResult> list)
			{
				StringBuilder sb = new ("\n");

				list.ForEach(raycast => sb.AppendLine(raycast.gameObject.name));
				sb.ToString().onScreen("raycast result");
			}

			var raycastAll = typeof(EventSystem).method("RaycastAll");

			return cins.ciInsert(ci => ci.isOp(OpCodes.Callvirt, raycastAll),
				OpCodes.Ldarg_0,
				OpCodes.Ldfld, typeof(BaseInputModule).field("m_RaycastResultCache"),
				CIHelper.emitCall<Action<List<RaycastResult>>>(_printRaycastResults));
		}
	}
}