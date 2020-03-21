using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;

using Common;

namespace DayNightSpeed
{
	using CIEnumerable = IEnumerable<CodeInstruction>;
	using static Common.HarmonyHelper;

	static class DayNightCyclePatches
	{
		// simple transpiler for changing 1.0 to current value of dayNightSpeed
		static CIEnumerable transpiler_dayNightSpeed(CIEnumerable cins) => constToCfgVar(cins, 1.0f, nameof(Main.config.dayNightSpeed));
		static readonly MethodInfo patchSpeedSimple = typeof(DayNightCyclePatches).method(nameof(transpiler_dayNightSpeed));

		// transpiler for correcting time if daynightspeed < 1 (with additional multiplier)
		static CIEnumerable transpiler_dnsClamped01(CIEnumerable cins, string multCfgVarName)
		{
			MethodInfo deltaTime = typeof(DayNightCycle).method("get_deltaTime");
			MethodInfo dayNightSpeed = typeof(DayNightCycle).method("get_dayNightSpeed");

			return ciInsert(cins, ci => ci.isOp(OpCodes.Callvirt, deltaTime) || ci.isOp(OpCodes.Callvirt, dayNightSpeed), +1, 0,
				_dnsClamped01.ci, OpCodes.Div,
				_codeForCfgVar(multCfgVarName), OpCodes.Mul);
		}

		static CIEnumerable transpiler_dnsClamped01_charge(CIEnumerable cins) => transpiler_dnsClamped01(cins, nameof(ModConfig.auxSpeedPowerCharge));
		static readonly MethodInfo patchSpeedClamped01_charge = typeof(DayNightCyclePatches).method(nameof(transpiler_dnsClamped01_charge));

		static CIEnumerable transpiler_dnsClamped01_consume(CIEnumerable cins) => transpiler_dnsClamped01(cins, nameof(ModConfig.auxSpeedPowerConsume));
		static readonly MethodInfo patchSpeedClamped01_consume = typeof(DayNightCyclePatches).method(nameof(transpiler_dnsClamped01_consume));

		static readonly Tuple<MethodInfo, MethodInfo>[] patches = new Tuple<MethodInfo, MethodInfo>[]
		{
			// 1.0 -> dayNightSpeed
			Tuple.Create(patchSpeedSimple, typeof(DayNightCycle).method("Update")),
			Tuple.Create(patchSpeedSimple, typeof(DayNightCycle).method("Resume")),
			Tuple.Create(patchSpeedSimple, typeof(DayNightCycle).method("StopSkipTimeMode")),
			Tuple.Create(patchSpeedSimple, typeof(DayNightCycle).method("OnConsoleCommand_day")),
			Tuple.Create(patchSpeedSimple, typeof(DayNightCycle).method("OnConsoleCommand_night")),
			Tuple.Create(patchSpeedSimple, typeof(DayNightCycle).method("OnConsoleCommand_daynight")),

			// power charging
			Tuple.Create(patchSpeedClamped01_charge, typeof(Charger).method("Update")),
			Tuple.Create(patchSpeedClamped01_charge, typeof(SolarPanel).method("Update")),
			Tuple.Create(patchSpeedClamped01_charge, typeof(ThermalPlant).method("AddPower")),
			Tuple.Create(patchSpeedClamped01_charge, typeof(BaseBioReactor).method("Update")),
			Tuple.Create(patchSpeedClamped01_charge, typeof(BaseNuclearReactor).method("Update")),

			// power consuming
			Tuple.Create(patchSpeedClamped01_consume, typeof(BaseRoot).method("ConsumePower")),
			Tuple.Create(patchSpeedClamped01_consume, typeof(ToggleLights).method("UpdateLightEnergy")),
			Tuple.Create(patchSpeedClamped01_consume, typeof(FiltrationMachine).method("UpdateFiltering")),
		};

		static bool inited = false;

		public static void init()
		{
			if (!inited && (inited = true))
				patches.forEach(p => patch(p.Item2, transpiler: p.Item1));
		}
	}
}