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
			int ldc100 = 0;
			foreach (var i in ins)
			{
				if (i.isLDC(100f) && ++ldc100 <= 2)
				{
					yield return i;

					foreach (var j in _codeForChangeInstructionToConfigVar(nameof(ModConfig.dayNightSpeed)))
						yield return j;

					yield return new CodeInstruction(OpCodes.Mul);

					if (Main.config.multHungerThrist != 1f)
					{
						foreach (var j in _codeForChangeInstructionToConfigVar(nameof(ModConfig.multHungerThrist)))
							yield return j;

						yield return new CodeInstruction(OpCodes.Div);
					}

					continue;
				}

				yield return i;
			}
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

	// we can use object with propulsion cannon after shot in 3 seconds
	[HarmonyPatch(typeof(PropulseCannonAmmoHandler), "Update")]
	static class PropulseCannonAmmoHandler_Update_Patch
	{
		static Instructions Transpiler(Instructions ins)
		{
			foreach (var i in ins)
			{
				if (i.isLDC(3.0f))
				{
					yield return i;

					yield return _dayNightSpeedClamped01.ci;
					yield return new CodeInstruction(OpCodes.Mul);

					continue;
				}

				yield return i;
			}
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

	// fixed lifetime for explosion
	[HarmonyPatch(typeof(WorldForces), "AddExplosion")]
	static class WorldForces_AddExplosion_Patch
	{
		static Instructions Transpiler(Instructions ins)
		{
			foreach (var i in ins)
			{
				if (i.isLDC(500f))
				{
					yield return i;

					yield return _dayNightSpeedClamped01.ci;
					yield return new CodeInstruction(OpCodes.Div);

					continue;
				}

				yield return i;
			}
		}
	}

	// fixed lifetime for current
	[HarmonyPatch(typeof(WorldForces), "AddCurrent")]
	static class WorldForces_AddCurrent_Patch
	{
		static Instructions Transpiler(Instructions ins)
		{
			foreach (var i in ins)
			{
				if (i.isOp(OpCodes.Ldarg_S, (byte)5))
				{
					yield return i;

					yield return _dayNightSpeedClamped01.ci;
					yield return new CodeInstruction(OpCodes.Mul);

					continue;
				}

				yield return i;
			}
		}
	}

	// fixes for explosions and currents
	[HarmonyPatch(typeof(WorldForces), "DoFixedUpdate")]
	static class WorldForces_DoFixedUpdate_Patch
	{
		static Instructions Transpiler(Instructions ins)
		{
			bool injected500 = false;
			foreach (var i in ins)
			{
				if (i.isLDC<double>(0.03f))
				{
					yield return i;

					yield return _dayNightSpeedClamped01.ci;
					yield return new CodeInstruction(OpCodes.Mul);

					continue;
				}
				
				if (!injected500 && i.isLDC(500f))
				{
					injected500 = true;
					yield return i;

					yield return _dayNightSpeedClamped01.ci;
					yield return new CodeInstruction(OpCodes.Div);

					continue;
				}

				yield return i;
			}
		}
	}
}