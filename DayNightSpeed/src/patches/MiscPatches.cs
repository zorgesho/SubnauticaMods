using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;

using Common;

namespace DayNightSpeed
{
	using CIEnumerable = IEnumerable<CodeInstruction>;
	using static Common.HarmonyHelper;

	// fixing hunger/thrist timers
	[HarmonyPatch(typeof(Survival), "UpdateStats")]
	static class Survival_UpdateStats_Patch
	{
		static CIEnumerable Transpiler(CIEnumerable cins) =>
			ciInsert(cins, ci => ci.isLDC(100f), +1, 2, _codeForCfgVar(nameof(ModConfig.dayNightSpeed)), OpCodes.Mul,
														_codeForCfgVar(nameof(ModConfig.auxSpeedHungerThrist)), OpCodes.Mul);

#if DEBUG // debug patch
		static void Postfix(Survival __instance)
		{
			if (Main.config.dbgCfg.showSurvivalStats)
				$"food: {__instance.food} water: {__instance.water}".onScreen("survival stats");
		}
#endif
	}

	// fixing crafting times
	[HarmonyPatch(typeof(CrafterLogic), "Craft")]
	static class CrafterLogic_Craft_Patch
	{
		static CIEnumerable Transpiler(CIEnumerable cins)
		{
			int ld = 0;
			return ciInsert(cins, ci => (ci.isOp(OpCodes.Ldarg_2) && ++ld == 2) || ci.isLDC(0.1f), +1, 2,
				_codeForCfgVar(nameof(ModConfig.dayNightSpeed)), OpCodes.Mul);
		}
	}

	// fixing maproom scan times
	[HarmonyPatch(typeof(MapRoomFunctionality), "GetScanInterval")]
	static class MapRoomFunctionality_GetScanInterval_Patch
	{
		static CIEnumerable Transpiler(CIEnumerable cins) =>
			ciInsert(cins, ci => ci.isOp(OpCodes.Call), _dnsClamped01.ci, OpCodes.Mul);
	}

	// fixing sunbeam counter so it shows realtime seconds regardless of daynightspeed
	[HarmonyPatch(typeof(uGUI_SunbeamCountdown), "UpdateInterface")]
	static class uGUISunbeamCountdown_UpdateInterface_Patch
	{
		static CIEnumerable Transpiler(CIEnumerable cins) =>
			ciInsert(cins, ci => ci.isOp(OpCodes.Sub), _codeForCfgVar(nameof(ModConfig.dayNightSpeed)), OpCodes.Div);
	}

	// we can use object with propulsion cannon after shot in 3 seconds
	[HarmonyPatch(typeof(PropulseCannonAmmoHandler), "Update")]
	static class PropulseCannonAmmoHandler_Update_Patch
	{
		static CIEnumerable Transpiler(CIEnumerable cins) =>
			ciInsert(cins, ci => ci.isLDC(3.0f), _dnsClamped01.ci, OpCodes.Mul);
	}

	// fixed lifetime for explosion
	[HarmonyPatch(typeof(WorldForces), "AddExplosion")]
	static class WorldForces_AddExplosion_Patch
	{
		static CIEnumerable Transpiler(CIEnumerable cins) =>
			ciInsert(cins, ci => ci.isLDC(500f), _dnsClamped01.ci, OpCodes.Div);
	}

	// fixed lifetime for current
	[HarmonyPatch(typeof(WorldForces), "AddCurrent")]
	static class WorldForces_AddCurrent_Patch
	{
		static CIEnumerable Transpiler(CIEnumerable cins) =>
			ciInsert(cins, ci => ci.isOp(OpCodes.Ldarg_S, (byte)5), _dnsClamped01.ci, OpCodes.Mul);
	}

	// fixes for explosions and currents
	[HarmonyPatch(typeof(WorldForces), "DoFixedUpdate")]
	static class WorldForces_DoFixedUpdate_Patch
	{
		static CIEnumerable Transpiler(CIEnumerable cins)
		{
			var list = cins.ToList();

			ciInsert(list, ci => ci.isLDC<double>(0.03f), _dnsClamped01.ci, OpCodes.Mul); // do not change to 0.03d !
			ciInsert(list, ci => ci.isLDC(500f), _dnsClamped01.ci, OpCodes.Div); // changing only first '500f'

			return list;
		}
	}

	// peeper enzyme recharging interval, just use speed setting at the moment of start
	[HarmonyPatch(typeof(Peeper), "Start")]
	static class Peeper_Start_Patch
	{
		static float rechargeIntervalInitial = 0;
		static void Postfix(Peeper __instance)
		{
			if (rechargeIntervalInitial == 0)
				rechargeIntervalInitial = __instance.rechargeInterval;

			__instance.rechargeInterval = rechargeIntervalInitial * Main.config.dayNightSpeed;
		}
	}
}