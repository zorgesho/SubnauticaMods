using System;
using System.Reflection.Emit;
using System.Collections.Generic;
using Harmony;
using Common;

namespace DayNightSpeed
{
	// modifying egg hatching time
	[HarmonyPatch(typeof(CreatureEgg), "Awake")]
	static class CreatureEgg_Awake_Patch
	{
#if !DEBUG
		static bool Prepare() => Main.config.multEggsHatching != 1.0f;
#endif
		static void Postfix(CreatureEgg __instance) => __instance.daysBeforeHatching *= Main.config.multEggsHatching;
	}
	
	// modifying creature grow and breed time (breed time is half of grow time)
	[HarmonyPatch(typeof(WaterParkCreatureParameters), MethodType.Constructor)]
	[HarmonyPatch(new Type[] { typeof(float), typeof(float), typeof(float), typeof(float), typeof(bool)})]
	static class WaterParkCreature_Constructor_Patch
	{
#if !DEBUG
		static bool Prepare() => Main.config.multCreaturesGrow != 1.0f;
#endif
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> cins)
		{
			foreach (var ci in cins)
			{
				yield return ci;

				if (ci.isLDC(1200f))
				{
					foreach (var i in HarmonyHelper._codeForChangeInstructionToConfigVar(nameof(ModConfig.multCreaturesGrow)))
						yield return i;
					yield return new CodeInstruction(OpCodes.Mul);
				}
			}
		}
	}

	// modifying plants grow time
	[HarmonyPatch(typeof(GrowingPlant), "GetGrowthDuration")]
	static class GrowingPlant_GetGrowthDuration_Patch
	{
#if !DEBUG
		static bool Prepare() => Main.config.multPlantsGrow != 1.0f;
#endif
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> cins) =>
			HarmonyHelper.changeConstToConfigVar(cins, 1.0f, nameof(ModConfig.multPlantsGrow));
	}

	// modifying fruits grow time (on lantern tree)
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

	// modifying medkit autocraft time
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
			if (Main.config.dbgCfg.showToggleLightStats)
				$"{__instance.energyMixin?.charge} {__instance.energyPerSecond}".onScreen("energy " + __instance.name);
		}
	}

	[HarmonyPatch(typeof(Survival), "UpdateStats")]
	static class Survival_UpdateStats_Patch_Dbg
	{
		static void Postfix(Survival __instance)
		{
			if (Main.config.dbgCfg.showSurvivalStats)
				$"food: {__instance.food} water: {__instance.water}".onScreen("survival stats");
		}
	}

	[HarmonyPatch(typeof(Story.StoryGoalScheduler), "Schedule")]
	static class StoryGoalScheduler_Schedule_Patch
	{
		static void Postfix(Story.StoryGoal goal) => $"goal added: {goal.key} {goal.delay} {goal.goalType}".logDbg();
	}
	
	[HarmonyPatch(typeof(Story.StoryGoal), "Execute")]
	static class StoryGoal_Execute_Patch
	{
		static void Postfix(string key, GoalType goalType) => $"goal removed: {key} {goalType}".logDbg();
	}
	#endif
	#endregion
}