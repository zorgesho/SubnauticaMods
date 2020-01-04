using System;
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
		static Instructions transpilerSpeed(Instructions cins) => changeConstToConfigVar(cins, 1.0f, nameof(Main.config.dayNightSpeed));
		static readonly MethodInfo patchSpeedSimple = typeof(DayNightCyclePatches).method(nameof(transpilerSpeed));


		// transpiler for correcting time if daynightspeed < 1
		static Instructions transpilerSpeedClamped01(Instructions cins)
		{
			MethodInfo deltaTime = typeof(DayNightCycle).method("get_deltaTime");
			MethodInfo dayNightSpeed = typeof(DayNightCycle).method("get_dayNightSpeed");

			foreach (var ci in cins)
			{
				yield return ci;

				if (ci.opcode == OpCodes.Callvirt && (ci.operand.Equals(deltaTime) || ci.operand.Equals(dayNightSpeed)))
				{
					yield return _dayNightSpeedClamped01.ci;
					yield return new CodeInstruction(OpCodes.Div);
				}
			}
		}
		static readonly MethodInfo patchSpeedClamped01 = typeof(DayNightCyclePatches).method(nameof(transpilerSpeedClamped01));


		static readonly Tuple<MethodInfo, MethodInfo>[] patches = new Tuple<MethodInfo, MethodInfo>[]
		{
			// 1.0 -> dayNightSpeed
			Tuple.Create(patchSpeedSimple, typeof(DayNightCycle).method("Update")),
			Tuple.Create(patchSpeedSimple, typeof(DayNightCycle).method("Resume")),
			Tuple.Create(patchSpeedSimple, typeof(DayNightCycle).method("StopSkipTimeMode")),
			Tuple.Create(patchSpeedSimple, typeof(DayNightCycle).method("OnConsoleCommand_day")),
			Tuple.Create(patchSpeedSimple, typeof(DayNightCycle).method("OnConsoleCommand_night")),
			Tuple.Create(patchSpeedSimple, typeof(DayNightCycle).method("OnConsoleCommand_daynight")),

			// deltaTime -> deltaTime/dayNightSpeed01
			Tuple.Create(patchSpeedClamped01, typeof(Charger).method("Update")),
			Tuple.Create(patchSpeedClamped01, typeof(SolarPanel).method("Update")),
			Tuple.Create(patchSpeedClamped01, typeof(BaseBioReactor).method("Update")),
			Tuple.Create(patchSpeedClamped01, typeof(BaseNuclearReactor).method("Update")),
			Tuple.Create(patchSpeedClamped01, typeof(ToggleLights).method("UpdateLightEnergy")),

			// dayNightSpeed -> dayNightSpeed/dayNightSpeed01
			Tuple.Create(patchSpeedClamped01, typeof(BaseRoot).method("ConsumePower")),
			Tuple.Create(patchSpeedClamped01, typeof(ThermalPlant).method("AddPower")),
			Tuple.Create(patchSpeedClamped01, typeof(FiltrationMachine).method("UpdateFiltering")),
		};

		public static void init()
		{
			if (!inited)
			{
				inited = true;
				patches.forEach(p => patch(p.Item2, transpiler: p.Item1));
			}
		}
	}
}