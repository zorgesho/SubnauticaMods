using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;

using Common;
using Common.Harmony;
using Common.Reflection;

namespace DayNightSpeed
{
	using CIEnumerable = IEnumerable<CodeInstruction>;
	using static CIHelper;

	// modifying egg hatching time
	[OptionalPatch, HarmonyPatch(typeof(CreatureEgg), "GetHatchDuration")]
	static class CreatureEgg_GetHatchDuration_Patch
	{
		static bool Prepare() => Main.config.useAuxSpeeds && Main.config.speedEggsHatching != 1.0f;

		static CIEnumerable Transpiler(CIEnumerable cins) =>
			ciInsert(cins, ci => ci.isLDC(1f), _codeForCfgVar(nameof(ModConfig.speedEggsHatching)), OpCodes.Div);
	}

	// modifying creature grow and breed time (breed time is half of grow time)
	static class WaterParkCreaturePatches
	{
		static bool prepare() => Main.config.useAuxSpeeds && Main.config.speedCreaturesGrow != 1.0f;

		static CIEnumerable transpiler(CIEnumerable cins)
		{
			FieldInfo growingPeriod = typeof(WaterParkCreatureParameters).field("growingPeriod");

			return ciInsert(cins, ci => ci.isOp(OpCodes.Ldfld, growingPeriod), +1, 0,
				_codeForCfgVar(nameof(ModConfig.speedCreaturesGrow)), OpCodes.Div);
		}

		[OptionalPatch, HarmonyPatch(typeof(WaterParkCreature), "Update")]
		static class Update_Patch
		{
			static bool Prepare() => prepare();
			static CIEnumerable Transpiler(CIEnumerable cins) => transpiler(cins);
		}

		[OptionalPatch, HarmonyPatch(typeof(WaterParkCreature), "SetMatureTime")]
		static class SetMatureTime_Patch
		{
			static bool Prepare() => prepare();
			static CIEnumerable Transpiler(CIEnumerable cins) => transpiler(cins);
		}
	}

	// modifying plants grow time
	[OptionalPatch, HarmonyPatch(typeof(GrowingPlant), "GetGrowthDuration")]
	static class GrowingPlant_GetGrowthDuration_Patch
	{
		static bool Prepare() => Main.config.useAuxSpeeds && Main.config.speedPlantsGrow != 1.0f;

		static CIEnumerable Transpiler(CIEnumerable cins) =>
			ciInsert(cins, ci => ci.isLDC(1f), _codeForCfgVar(nameof(ModConfig.speedPlantsGrow)), OpCodes.Div);
	}

	// modifying fruits grow time (on lantern tree)
	[OptionalPatch, HarmonyPatch(typeof(FruitPlant), "Initialize")]
	static class FruitPlant_Initialize_Patch
	{
		static bool Prepare() => Main.config.useAuxSpeeds && Main.config.speedPlantsGrow != 1.0f;

		static void Prefix(FruitPlant __instance) // don't want to use another transpilers here
		{
			if (!__instance.initialized)
				__instance.fruitSpawnInterval /= Main.config.speedPlantsGrow;
		}
	}

	// modifying medkit autocraft time
	[OptionalPatch, HarmonyPatch(typeof(MedicalCabinet), "Start")]
	static class MedicalCabinet_Start_Patch
	{
		static float medKitSpawnInterval = 0f;

		static bool Prepare() => Main.config.useAuxSpeeds && Main.config.speedMedkitInterval != 1.0f;

		static void Prefix(MedicalCabinet __instance)
		{
			if (medKitSpawnInterval == 0f)
				medKitSpawnInterval = __instance.medKitSpawnInterval;

			__instance.medKitSpawnInterval = medKitSpawnInterval / Main.config.speedMedkitInterval;
		}
	}


#if DEBUG
	[PatchClass]
	static class DebugPatches
	{
		[HarmonyPrefix, HarmonyPatch(typeof(Bed), "GetCanSleep")]
		static bool Bed_GetCanSleep_Prefix(ref bool __result)
		{
			__result = true;
			return false;
		}

		[HarmonyPostfix, HarmonyPatch(typeof(ToggleLights), "UpdateLightEnergy")]
		static void ToggleLights_UpdateLightEnergy_Postfix(ToggleLights __instance)
		{
			if (Main.config.dbgCfg.showToggleLightStats)
				$"{__instance.energyMixin?.charge} {__instance.energyPerSecond}".onScreen($"energy {__instance.name}");
		}

		[HarmonyPostfix, HarmonyPatch(typeof(WaterParkCreature), "Update")]
		static void WaterParkCreature_Update_Postfix(WaterParkCreature __instance)
		{
			if (Main.config.dbgCfg.showWaterParkCreatures)
			{
				$"age: {__instance.age} canBreed: {__instance.canBreed} matureTime: {__instance.matureTime} isMature: {__instance.isMature}".
					onScreen($"waterpark {__instance.name} {__instance.GetHashCode()}");
			}
		}

		[HarmonyPostfix, HarmonyPatch(typeof(CreatureEgg), "UpdateProgress")]
		static void CreatureEgg_UpdateProgress_Postfix(CreatureEgg __instance)
		{
			if (Main.config.dbgCfg.showWaterParkCreatures)
				$"progress: {__instance.progress}".onScreen($"waterpark {__instance.name} {__instance.GetHashCode()}");
		}

		[HarmonyPostfix, HarmonyPatch(typeof(Story.StoryGoalScheduler), "Schedule")]
		static void StoryGoalScheduler_Schedule_Postfix(Story.StoryGoal goal) => $"goal added: {goal.key} {goal.delay} {goal.goalType}".logDbg();

		[HarmonyPostfix, HarmonyPatch(typeof(Story.StoryGoal), "Execute")]
		static void StoryGoal_Execute_Postfix(string key, GoalType goalType) => $"goal removed: {key} {goalType}".logDbg();
	}
#endif
}