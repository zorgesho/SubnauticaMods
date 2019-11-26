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
		static Instructions Transpiler(Instructions cins)
		{
			int ldc100 = 0;
			foreach (var ci in cins)
			{
				if (ci.isLDC(100f) && ++ldc100 <= 2)
				{
					yield return ci;

					foreach (var i in _codeForChangeInstructionToConfigVar(nameof(ModConfig.dayNightSpeed)))
						yield return i;

					yield return new CodeInstruction(OpCodes.Mul);

					if (Main.config.multHungerThrist != 1f)
					{
						foreach (var i in _codeForChangeInstructionToConfigVar(nameof(ModConfig.multHungerThrist)))
							yield return i;

						yield return new CodeInstruction(OpCodes.Div);
					}

					continue;
				}

				yield return ci;
			}
		}
	}

	// fixing crafting times
	[HarmonyPatch(typeof(CrafterLogic), "Craft")]
	static class CrafterLogic_Craft_Patch
	{
		static Instructions Transpiler(Instructions cins)
		{
			int ld = 0;
			foreach (var ci in cins)
			{
				if ((ci.opcode == OpCodes.Ldarg_2 && ++ld == 2) || ci.isLDC(0.1f))
				{
					yield return ci;

					foreach (var i in _codeForChangeInstructionToConfigVar(nameof(ModConfig.dayNightSpeed)))
						yield return i;

					yield return new CodeInstruction(OpCodes.Mul);
					continue;
				}

				yield return ci;
			}
		}
	}

	// fixing sunbeam counter so it shows realtime seconds regardless of daynightspeed
	[HarmonyPatch(typeof(uGUI_SunbeamCountdown), "UpdateInterface")]
	static class uGUISunbeamCountdown_UpdateInterface_Patch
	{
		static Instructions Transpiler(Instructions cins)
		{
			foreach (var ci in cins)
			{
				if (ci.opcode == OpCodes.Sub)
				{
					yield return ci;

					foreach (var i in _codeForChangeInstructionToConfigVar(nameof(ModConfig.dayNightSpeed)))
						yield return i;

					yield return new CodeInstruction(OpCodes.Div);
					continue;
				}

				yield return ci;
			}
		}
	}

	// we can use object with propulsion cannon after shot in 3 seconds
	[HarmonyPatch(typeof(PropulseCannonAmmoHandler), "Update")]
	static class PropulseCannonAmmoHandler_Update_Patch
	{
		static Instructions Transpiler(Instructions cins)
		{
			foreach (var ci in cins)
			{
				if (ci.isLDC(3.0f))
				{
					yield return ci;

					yield return _dayNightSpeedClamped01.ci;
					yield return new CodeInstruction(OpCodes.Mul);

					continue;
				}

				yield return ci;
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
		static Instructions Transpiler(Instructions cins)
		{
			foreach (var ci in cins)
			{
				if (ci.isLDC(500f))
				{
					yield return ci;

					yield return _dayNightSpeedClamped01.ci;
					yield return new CodeInstruction(OpCodes.Div);

					continue;
				}

				yield return ci;
			}
		}
	}

	// fixed lifetime for current
	[HarmonyPatch(typeof(WorldForces), "AddCurrent")]
	static class WorldForces_AddCurrent_Patch
	{
		static Instructions Transpiler(Instructions cins)
		{
			foreach (var ci in cins)
			{
				if (ci.isOp(OpCodes.Ldarg_S, (byte)5))
				{
					yield return ci;

					yield return _dayNightSpeedClamped01.ci;
					yield return new CodeInstruction(OpCodes.Mul);

					continue;
				}

				yield return ci;
			}
		}
	}

	// fixes for explosions and currents
	[HarmonyPatch(typeof(WorldForces), "DoFixedUpdate")]
	static class WorldForces_DoFixedUpdate_Patch
	{
		static Instructions Transpiler(Instructions cins)
		{
			bool injected500 = false;
			foreach (var ci in cins)
			{
				if (ci.isLDC<double>(0.03f)) // is it safe ?
				{
					yield return ci;

					yield return _dayNightSpeedClamped01.ci;
					yield return new CodeInstruction(OpCodes.Mul);

					continue;
				}
				
				if (!injected500 && ci.isLDC(500f))
				{
					injected500 = true;
					yield return ci;

					yield return _dayNightSpeedClamped01.ci;
					yield return new CodeInstruction(OpCodes.Div);

					continue;
				}

				yield return ci;
			}
		}
	}
}