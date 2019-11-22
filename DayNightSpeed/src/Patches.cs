using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;
using Common;

namespace DayNightSpeed
{
	using Instructions = IEnumerable<CodeInstruction>;
	using static Common.HarmonyHelper;

	static class DayNightCyclePatches
	{
		static bool inited = false;

		// simple transpiler for changing 1.0 to current value of dayNightSpeed
		static Instructions transpilerSpeed(Instructions ins) => changeConstToConfigVar(ins, 1.0f, nameof(Main.config.dayNightSpeed));
		static readonly MethodInfo patchSpeedSimple = AccessTools.Method(typeof(DayNightCyclePatches), nameof(transpilerSpeed));

		// transpiler for correcting time if daynightspeed < 1
		static Instructions transpilerSpeedClamped01(Instructions ins)
		{
			MethodInfo deltaTime = AccessTools.Method(typeof(DayNightCycle), "get_deltaTime");
			MethodInfo dayNightSpeed = AccessTools.Method(typeof(DayNightCycle), "get_dayNightSpeed");

			foreach (var i in ins)
			{
				if (i.opcode.Equals(OpCodes.Callvirt) && (i.operand.Equals(deltaTime) || i.operand.Equals(dayNightSpeed)))
				{
					yield return i;

					foreach (var j in _codeForChangeConstToConfigMethodCall(nameof(ModConfig.getDayNightSpeedClamped01)))
						yield return j;

					yield return new CodeInstruction(OpCodes.Div);
					continue;
				}
				yield return i;
			}
		}
		static readonly MethodInfo patchSpeedClamped01 = AccessTools.Method(typeof(DayNightCyclePatches), nameof(transpilerSpeedClamped01));


		static MethodInfo _dncMethod(string method) => AccessTools.Method(typeof(DayNightCycle), method);

		static readonly Tuple<MethodInfo, MethodInfo>[] patches = new Tuple<MethodInfo, MethodInfo>[]
		{
			// 1.0 -> dayNightSpeed
			Tuple.New(patchSpeedSimple, _dncMethod("Update")),
			Tuple.New(patchSpeedSimple, _dncMethod("Resume")),
			Tuple.New(patchSpeedSimple, _dncMethod("StopSkipTimeMode")),
			Tuple.New(patchSpeedSimple, _dncMethod("OnConsoleCommand_day")),
			Tuple.New(patchSpeedSimple, _dncMethod("OnConsoleCommand_night")),
			Tuple.New(patchSpeedSimple, _dncMethod("OnConsoleCommand_daynight")),

			// deltaTime -> deltaTime/dayNightSpeed01
			Tuple.New(patchSpeedClamped01, AccessTools.Method(typeof(Charger), "Update")),
			Tuple.New(patchSpeedClamped01, AccessTools.Method(typeof(SolarPanel), "Update")),
			Tuple.New(patchSpeedClamped01, AccessTools.Method(typeof(BaseBioReactor), "Update")),
			Tuple.New(patchSpeedClamped01, AccessTools.Method(typeof(BaseNuclearReactor), "Update")),
			Tuple.New(patchSpeedClamped01, AccessTools.Method(typeof(ToggleLights), "UpdateLightEnergy")),

			// dayNightSpeed -> dayNightSpeed/dayNightSpeed01
			Tuple.New(patchSpeedClamped01, AccessTools.Method(typeof(BaseRoot), "ConsumePower")),
			Tuple.New(patchSpeedClamped01, AccessTools.Method(typeof(ThermalPlant), "AddPower")),
			Tuple.New(patchSpeedClamped01, AccessTools.Method(typeof(FiltrationMachine), "UpdateFiltering")),
		};

		public static void init()
		{
			if (!inited)
			{
				inited = true;

				foreach (var patch in patches)
					harmonyInstance.Patch(patch.second, transpiler: new HarmonyMethod(patch.first));
			}
		}
	}


	[HarmonyPatch(typeof(DayNightCycle), "Awake")]
	static class DayNightCycle_Awake_Patch
	{
		static void Postfix(DayNightCycle __instance)
		{
			__instance._dayNightSpeed = Main.config.dayNightSpeed;

			// unregistering vanilla daynightspeed console command, replacing it with ours in DayNightSpeedControl
			NotificationCenter.DefaultCenter.RemoveObserver(__instance, "OnConsoleCommand_daynightspeed");
		}
	}

	// fixing hunger/thrist timers
	[HarmonyPatch(typeof(Survival), "UpdateStats")]
	static class Survival_UpdateStats_Patch
	{
		static Instructions Transpiler(Instructions ins)
		{
			var list = new List<CodeInstruction>(ins);

			for (int i = list.Count - 1; i >= 0; i--) // changing list in the process, so iterate it backwards
			{
				void tryChangeVal(float val, string configMethod)
				{
					if (list[i].isLDC(val))
					{
						list.RemoveAt(i);
						list.InsertRange(i, _codeForChangeConstToConfigMethodCall(configMethod));
					}
				}

				tryChangeVal(ModConfig.hungerTimeInitial, nameof(Main.config.getHungerTime));
				tryChangeVal(ModConfig.thristTimeInitial, nameof(Main.config.getThristTime));
			}

			return list.AsEnumerable();
		}
	}

	// fixing crafting times
	[HarmonyPatch(typeof(CrafterLogic), "Craft")]
	static class CrafterLogic_Craft_Patch
	{
		static Instructions Transpiler(Instructions ins)
		{
			int ld = 0;
			foreach (var i in ins)
			{
				if ((i.opcode.Equals(OpCodes.Ldarg_2) && ++ld == 2) || i.isLDC(0.1f))
				{
					yield return i;

					foreach (var j in _codeForChangeInstructionToConfigVar(nameof(ModConfig.dayNightSpeed)))
						yield return j;

					yield return new CodeInstruction(OpCodes.Mul);
					continue;
				}

				yield return i;
			}
		}
	}

	// fixing sunbeam counter so it shows realtime seconds regardless of daynightspeed
	[HarmonyPatch(typeof(uGUI_SunbeamCountdown), "UpdateInterface")]
	static class uGUISunbeamCountdown_UpdateInterface_Patch
	{
		static Instructions Transpiler(Instructions ins)
		{
			foreach (var i in ins)
			{
				if (i.opcode.Equals(OpCodes.Sub))
				{
					yield return i;

					foreach (var j in _codeForChangeInstructionToConfigVar(nameof(ModConfig.dayNightSpeed)))
						yield return j;

					yield return new CodeInstruction(OpCodes.Div);
					continue;
				}

				yield return i;
			}
		}
	}

	#region Optional patches with config multipliers
	// optionally modifying egg hatching time
	[HarmonyPatch(typeof(CreatureEgg), "Awake")]
	static class CreatureEgg_Awake_Patch
	{
#if !DEBUG
		static bool Prepare() => Main.config.multEggsHatching != 1.0f;
#endif
		static void Postfix(CreatureEgg __instance) => __instance.daysBeforeHatching *= Main.config.multEggsHatching;
	}

	// optionally modifying plants grow time
	[HarmonyPatch(typeof(GrowingPlant), "GetGrowthDuration")]
	static class GrowingPlant_GetGrowthDuration_Patch
	{
#if !DEBUG
		static bool Prepare() => Main.config.multPlantsGrow != 1.0f;
#endif
		static Instructions Transpiler(Instructions ins) => changeConstToConfigVar(ins, 1.0f, nameof(ModConfig.multPlantsGrow));
	}

	// optionally modifying fruits grow time (on lantern tree)
	[HarmonyPatch(typeof(FruitPlant), "Initialize")]
	static class FruitPlant_Initialize_Patch
	{
#if !DEBUG
		static bool Prepare() => Main.config.multPlantsGrow != 1.0f;
#endif
		static void Prefix(FruitPlant __instance)
		{
			if (!__instance.initialized)
				__instance.fruitSpawnInterval *= Main.config.multPlantsGrow;
		}
	}

	// optionally modifying medkit autocraft time
	[HarmonyPatch(typeof(MedicalCabinet), "Start")]
	static class MedicalCabinet_Start_Patch
	{
		static float medKitSpawnInterval = 0f;
#if !DEBUG
		static bool Prepare() => Main.config.multMedkitInterval != 1.0f;
#endif
		static void Prefix(MedicalCabinet __instance)
		{
			if (medKitSpawnInterval == 0f)
				medKitSpawnInterval = __instance.medKitSpawnInterval;

			__instance.medKitSpawnInterval = medKitSpawnInterval * Main.config.multMedkitInterval;
		}
	}
	#endregion

	#region Debug patches
#if DEBUG
	[HarmonyPatch(typeof(Bed), "GetCanSleep")]
	static class Bed_GetCanSleep_Patch
	{
		static bool Prefix(ref bool __result)
		{
			__result = true;
			return false;
		}
	}

	[HarmonyPatch(typeof(ToggleLights), "UpdateLightEnergy")]
	static class ToggleLights_UpdateLightEnergy_Patch
	{
		static void Postfix(ToggleLights __instance)
		{
			$"{__instance.energyMixin?.charge} {__instance.energyPerSecond}".onScreen("energy " + __instance.name);
		}
	}

	[HarmonyPatch(typeof(Survival), "UpdateStats")]
	static class Survival_UpdateStats_Patch_Dbg
	{
		static void Postfix(Survival __instance)
		{
			$"food: {__instance.food} water: {__instance.water}".onScreen("survival stats");
		}
	}
	#endif
	#endregion
}