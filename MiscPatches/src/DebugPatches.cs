using System.Linq;
using System.Reflection.Emit;
using System.Collections;
using System.Collections.Generic;

using Harmony;
using UnityEngine;
using UnityEngine.SceneManagement;

using Common;
using Common.Harmony;
using Common.Reflection;
using Common.Configuration;

namespace MiscPatches
{
	// for testing loot spawning (Main.config.loolSpawnRerollCount for control)
	[OptionalPatch]
	[HarmonyPatch(typeof(CellManager), "GetPrefabForSlot")]
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
	[OptionalPatch]
	[HarmonyPatch(typeof(PropulsionCannon), "ValidateObject")]
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
				Object.FindObjectsOfType<VFXDestroyAfterSeconds>().forEach(vfx => vfx.lifeTime = 0f);
		}

		static void Postfix(VFXController __instance, int i)
		{
			if (__instance.emitters[i].instanceGO.GetComponent<VFXDestroyAfterSeconds>() is VFXDestroyAfterSeconds vfx)
				vfx.lifeTime = float.PositiveInfinity;
		}
	}


	static class ScannerRoomCheat
	{
		[OptionalPatch]
		[HarmonyPatch(typeof(MapRoomFunctionality), "GetScanRange")]
		static class MapRoomFunctionality_GetScanRange_Patch
		{
			static bool Prepare() => Main.config.dbg.scannerRoomCheat;

			static bool Prefix(ref float __result)
			{
				__result = 500f;
				return false;
			}
		}

		[OptionalPatch]
		[HarmonyPatch(typeof(MapRoomFunctionality), "GetScanInterval")]
		static class MapRoomFunctionality_GetScanInterval_Patch
		{
			static bool Prepare() => Main.config.dbg.scannerRoomCheat;

			static bool Prefix(ref float __result)
			{
				__result = 0f;
				return false;
			}
		}
	}


	static class FastStart
	{
		[OptionalPatch]
		[HarmonyPatch(typeof(MainGameController), "Start")]
		static class MainGameController_Start_Patch
		{
			static bool Prepare() => Main.config.dbg.fastStart.enabled;

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> cins)
			{
				var list = cins.ToList();
				list[list.FindIndex(cin => cin.opcode == OpCodes.Call)].operand = typeof(MainGameController_Start_Patch).method(nameof(StartGame));

				return list;
			}

			static IEnumerator StartGame(MainGameController _)
			{																												"Fast start".logDbg();
				Physics.autoSyncTransforms = Physics2D.autoSimulation = false;

				if (Main.config.dbg.fastStart.loadEssentials)
					yield return SceneManager.LoadSceneAsync("Essentials", LoadSceneMode.Additive);

				Application.backgroundLoadingPriority = ThreadPriority.Normal;

				if (!Main.config.dbg.fastStart.loadEssentials)
					new GameObject("ConsoleCommands", typeof(InventoryConsoleCommands), typeof(SpawnConsoleCommand));

				if (Main.config.dbg.fastStart.initPrefabCache)
					CraftData.PreparePrefabIDCache();

				foreach (var cmd in Main.config.dbg.fastStart.commandsAfterLoad)
					DevConsole.SendConsoleCommand(cmd);
			}
		}

		[OptionalPatch]
		[HarmonyPatch(typeof(LightmappedPrefabs), "Awake")]
		static class LightmappedPrefabs_Awake_Patch
		{
			static bool Prepare() => Main.config.dbg.fastStart.enabled;

			static bool Prefix(LightmappedPrefabs __instance)
			{
				if (Main.config.dbg.fastStart.loadEscapePod)
				{
					__instance.autoloadScenes = new LightmappedPrefabs.AutoLoadScene[]
					{
						new LightmappedPrefabs.AutoLoadScene() { sceneName = "EscapePod", spawnOnStart = true }
					};
				}

				return Main.config.dbg.fastStart.loadEscapePod;
			}
		}

		[OptionalPatch]
		[HarmonyPatch(typeof(uGUI_OptionsPanel), "SyncTerrainChangeRequiresRestartText")]
		static class ModOptionsPanelFix
		{
			static bool Prepare() => Main.config.dbg.fastStart.enabled;
			static bool Prefix()  => false;
		}
	}
}