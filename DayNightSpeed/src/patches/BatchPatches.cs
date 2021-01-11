using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;

using Common.Harmony;
using Common.Reflection;

namespace DayNightSpeed
{
	using CIEnumerable = IEnumerable<CodeInstruction>;

	[PatchClass]
	static class DayNightCyclePatches
	{
		static readonly MethodInfo deltaTime = typeof(DayNightCycle).method("get_deltaTime");
		static readonly MethodInfo dayNightSpeed = typeof(DayNightCycle).method("get_dayNightSpeed");

		// simple transpiler for changing 1.0 to current value of dayNightSpeed
		[HarmonyTranspiler]
		[HarmonyPatch(typeof(DayNightCycle), "Update")]
		[HarmonyPatch(typeof(DayNightCycle), "Resume")]
		[HarmonyPatch(typeof(DayNightCycle), "StopSkipTimeMode")]
		[HarmonyPatch(typeof(DayNightCycle), "OnConsoleCommand_day")]
		[HarmonyPatch(typeof(DayNightCycle), "OnConsoleCommand_night")]
		[HarmonyPatch(typeof(DayNightCycle), "OnConsoleCommand_daynight")]
		static CIEnumerable transpiler_dayNightSpeed(CIEnumerable cins) =>
			CIHelper.constToCfgVar(cins, 1.0f, nameof(Main.config.dayNightSpeed));

		// transpiler for correcting time if daynightspeed < 1 (with additional multiplier)
		static CIEnumerable transpiler_dnsClamped01(CIEnumerable cins, string multCfgVarName) =>
			cins.ciInsert(ci => ci.isOp(OpCodes.Callvirt, deltaTime) || ci.isOp(OpCodes.Callvirt, dayNightSpeed), +1, 0,
				_dnsClamped01.ci, OpCodes.Div,
				CIHelper._codeForCfgVar(multCfgVarName), OpCodes.Mul);

		[HarmonyTranspiler] // power charging
		[HarmonyPatch(typeof(Charger), "Update")]
		[HarmonyPatch(typeof(SolarPanel), "Update")]
		[HarmonyPatch(typeof(ThermalPlant), "AddPower")]
		[HarmonyPatch(typeof(BaseBioReactor), "Update")]
		[HarmonyPatch(typeof(BaseNuclearReactor), "Update")]
		static CIEnumerable transpiler_dnsClamped01_charge(CIEnumerable cins) =>
			transpiler_dnsClamped01(cins, nameof(ModConfig.auxSpeedPowerCharge));

		[HarmonyTranspiler] // power consuming
#if GAME_SN
		[HarmonyPatch(typeof(BaseRoot), "ConsumePower")]
#endif
		[HarmonyPatch(typeof(ToggleLights), "UpdateLightEnergy")]
		[HarmonyPatch(typeof(FiltrationMachine), "UpdateFiltering")]
		static CIEnumerable transpiler_dnsClamped01_consume(CIEnumerable cins) =>
			transpiler_dnsClamped01(cins, nameof(ModConfig.auxSpeedPowerConsume));
	}
}