using System.Reflection.Emit;
using System.Collections.Generic;

using HarmonyLib;

using Common;
using Common.Harmony;

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
			cins.ciInsert(ci => ci.isLDC(1f), _codeForCfgVar(nameof(ModConfig.speedEggsHatching)), OpCodes.Div);
	}

	// modifying creature grow and breed time (breed time is half of grow time)
	[OptionalPatch, PatchClass]
	static class WaterParkCreaturePatch
	{
		static bool prepare() => Main.config.useAuxSpeeds && Main.config.speedCreaturesGrow != 1.0f;

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(WaterParkCreature), "SetMatureTime")]
		[HarmonyPatch(typeof(WaterParkCreature), Mod.Consts.isGameSN? "Update": "ManagedUpdate")]
		static CIEnumerable WaterParkCreature_Transpiler(CIEnumerable cins)
		{
			string growingPeriod = $"{(Mod.Consts.isGameSN? "": "get_")}growingPeriod";
			return cins.ciInsert(new MemberMatch(growingPeriod), +1, 0, _codeForCfgVar(nameof(ModConfig.speedCreaturesGrow)), OpCodes.Div);
		}
	}

	// modifying plants grow time and fruits grow time (on lantern tree)
	[OptionalPatch, PatchClass]
	static class PlantsGrowPatch
	{
		static bool prepare() => Main.config.useAuxSpeeds && Main.config.speedPlantsGrow != 1.0f;

		[HarmonyTranspiler, HarmonyPatch(typeof(GrowingPlant), "GetGrowthDuration")]
		static CIEnumerable GrowingPlant_GetGrowthDuration_Transpiler(CIEnumerable cins) =>
			cins.ciInsert(ci => ci.isLDC(1f), _codeForCfgVar(nameof(ModConfig.speedPlantsGrow)), OpCodes.Div);

		[HarmonyPrefix, HarmonyPatch(typeof(FruitPlant), "Initialize")]
		static void FruitPlant_Initialize_Prefix(FruitPlant __instance) // don't want to use another transpilers here
		{
			if (!__instance.initialized)
				__instance.fruitSpawnInterval /= Main.config.speedPlantsGrow;
		}
	}

	// modifying food decay time
	[OptionalPatch, PatchClass]
	static class FoodDecayPatch
	{
		static bool prepare() => Main.config.speedFoodDecay != 1.0f;

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(Eatable), "GetFoodValue")]
		[HarmonyPatch(typeof(Eatable), "GetWaterValue")]
		static CIEnumerable Eatable_GetFoodWaterValue_Transpiler(CIEnumerable cins) =>
			cins.ciInsert(ci => ci.isOp(OpCodes.Mul), _codeForCfgVar(nameof(ModConfig.speedFoodDecay)), OpCodes.Mul);
	}

	// modifying water/salt filtering time
	[OptionalPatch, PatchClass]
	static class FiltrationMachinePatch
	{
		static bool prepare() => Main.config.speedFiltrationMachine != 1.0f;

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(FiltrationMachine), "OnHover")] // fix on hover info
		[HarmonyPatch(typeof(FiltrationMachine), "TryFilterSalt")]
		[HarmonyPatch(typeof(FiltrationMachine), "TryFilterWater")]
		static CIEnumerable FiltrationMachine_Transpiler(CIEnumerable cins) =>
			cins.ciInsert(ci => ci.isLDC(420f) || ci.isLDC(840f), +1, 0, _codeForCfgVar(nameof(ModConfig.speedFiltrationMachine)), OpCodes.Div);
	}

#if GAME_SN
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
#endif

#if DEBUG
	[OptionalPatch, PatchClass]
	static class DebugPatches
	{
		static bool prepare() => Main.config.dbgCfg.enabled;

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

		[HarmonyPostfix, HarmonyPatch(typeof(WaterParkCreature), Mod.Consts.isGameSN? "Update": "ManagedUpdate")]
		static void WaterParkCreature_Update_Postfix(WaterParkCreature __instance)
		{
			if (Main.config.dbgCfg.showWaterParkCreatures)
			{
				$"age: {__instance.age} canBreed: {__instance.GetCanBreed()} matureTime: {__instance.matureTime} isMature: {__instance.isMature}".
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