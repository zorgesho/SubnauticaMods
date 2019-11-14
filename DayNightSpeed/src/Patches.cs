using System;
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
		
		static Instructions transpiler1(Instructions ins) => changeConstToConfigVar(ins, 1.0f, nameof(Main.config.dayNightSpeed));
		static readonly MethodInfo patch1 = AccessTools.Method(typeof(DayNightCyclePatches), "transpiler1");

		static Instructions transpiler2(Instructions ins)
		{
			MethodInfo deltaTime = AccessTools.Method(typeof(DayNightCycle), "get_deltaTime");
			MethodInfo dayNightSpeed = AccessTools.Method(typeof(DayNightCycle), "get_dayNightSpeed");
			
			foreach (var i in ins)
			{
				if (i.opcode.Equals(OpCodes.Callvirt) && (i.operand.Equals(deltaTime) || i.operand.Equals(dayNightSpeed)))
				{
					yield return i;
					yield return _codeForMainConfig();
					yield return new CodeInstruction(OpCodes.Callvirt,
						AccessTools.Method(typeof(ModConfig), nameof(ModConfig.getDayNightSpeedInverse)));

					yield return new CodeInstruction(OpCodes.Mul);

					"!!!!!!!!!!!!".log();
					continue;
				}
				yield return i;
			}
		}
		static readonly MethodInfo patch2 = AccessTools.Method(typeof(DayNightCyclePatches), "transpiler2");


		static MethodInfo _dncMethod(string method) => AccessTools.Method(typeof(DayNightCycle), method);

		static readonly Tuple<MethodInfo, MethodInfo>[] patches = new Tuple<MethodInfo, MethodInfo>[]
		{
			Tuple.New(patch1, _dncMethod("Update")),
			Tuple.New(patch1, _dncMethod("Resume")),
			Tuple.New(patch1, _dncMethod("StopSkipTimeMode")),
			Tuple.New(patch1, _dncMethod("OnConsoleCommand_day")),
			Tuple.New(patch1, _dncMethod("OnConsoleCommand_night")),
			Tuple.New(patch1, _dncMethod("OnConsoleCommand_daynight")),
			
			// deltaTime
			//Tuple.New(patch2, AccessTools.Method(typeof(Charger), "Update")),
			//Tuple.New(patch2, AccessTools.Method(typeof(SolarPanel), "Update")),
			//Tuple.New(patch2, AccessTools.Method(typeof(BaseBioReactor), "Update")),
			//Tuple.New(patch2, AccessTools.Method(typeof(BaseNuclearReactor), "Update")),
			//Tuple.New(patch2, AccessTools.Method(typeof(ToggleLights), "UpdateLightEnergy")),
			
			// dayNightSpeed
			Tuple.New(patch2, AccessTools.Method(typeof(BaseRoot), "ConsumePower")),
			Tuple.New(patch2, AccessTools.Method(typeof(ThermalPlant), "AddPower")),
			Tuple.New(patch2, AccessTools.Method(typeof(FiltrationMachine), "UpdateFiltering")),
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
		static void Postfix(DayNightCycle __instance) => __instance._dayNightSpeed = Main.config.dayNightSpeed;
	}

	
	[HarmonyPatch(typeof(CreatureEgg), "Awake")]
	static class CreatureEgg_Awake_Patch
	{
		static void Postfix(CreatureEgg __instance) => $"egg: {__instance.daysBeforeHatching}".onScreen().log();
	}


	[HarmonyPatch(typeof(Survival), "UpdateStats")]
	static class Survival_UpdateStats_Patch
	{
		static Instructions Transpiler(Instructions ins)
		{
			var list = new List<CodeInstruction>(ins);

			for (int i = list.Count - 1; i >= 0; i--) // changing list in the process, so iterate it backwards
			{
				void tryChangeVal(float val, string configVar)
				{
					if (list[i].isLDC(val))
					{
						list.RemoveAt(i);
						list.InsertRange(i, _codeForChangeConstToConfigVar(configVar));
					}
				}

				tryChangeVal(1800f, nameof(Main.config.thristSec));
				tryChangeVal(2520f, nameof(Main.config.hungerSec));
			}
			
			return list.AsEnumerable();
		}
	}


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
					yield return _codeForMainConfig();
					yield return new CodeInstruction(OpCodes.Callvirt,
						AccessTools.Method(typeof(ModConfig), nameof(ModConfig.getDayNightSpeed2)));

					yield return new CodeInstruction(OpCodes.Mul);

					continue;
				}
				yield return i;
			}
		}
	}


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
					
					foreach (var i1 in _codeForChangeConstToConfigVar(nameof(ModConfig.dayNightSpeed)))
						yield return i1;

					yield return new CodeInstruction(OpCodes.Div);
					
					continue;
				}

				yield return i;
			}
		}
	}


#if DEBUG
	// for debugging
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
}