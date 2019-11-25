using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;

namespace DayNightSpeed
{
	using Instructions = IEnumerable<CodeInstruction>;
	using static Common.HarmonyHelper;
	
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
}