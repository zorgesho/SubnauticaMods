using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;

using Common;

namespace ConsoleImproved
{
	// patching vanilla console commands that use float.Parse method to use locale independent conversion
	static class CommandsFloatParsePatch
	{
		static bool patched = false;
		static int _patchedCounter = 1;

		static IEnumerable<CodeInstruction> transpiler(IEnumerable<CodeInstruction> cins)
		{
			MethodInfo floatParse = typeof(float).method("Parse", typeof(string));
			MethodInfo floatParseCulture = typeof(float).method("Parse", typeof(string), typeof(IFormatProvider));
			MethodInfo getInvariantCulture = typeof(System.Globalization.CultureInfo).method("get_InvariantCulture");

			foreach (var ci in cins)
			{
				if (ci.isOp(OpCodes.Call, floatParse))
				{																												$"floatParse injected: {_patchedCounter++}".logDbg();
					yield return new CodeInstruction(OpCodes.Call, getInvariantCulture);
					yield return new CodeInstruction(OpCodes.Call, floatParseCulture);
				}
				else
					yield return ci;
			}
		}
		static readonly MethodInfo patch = typeof(CommandsFloatParsePatch).method(nameof(transpiler));


		static readonly MethodInfo[] toPatch = new MethodInfo[]
		{
			typeof(BaseFloodSim).method("OnConsoleCommand_baseflood"),
			typeof(DayNightCycle).method("OnConsoleCommand_daynightspeed"),
			typeof(CreateConsoleCommand).method("OnConsoleCommand_create"),
			typeof(GameModeConsoleCommands).method("OnConsoleCommand_damage"),
			typeof(PlayerMotor).method("OnConsoleCommand_swimx"),
			typeof(SNCameraRoot).method("OnConsoleCommand_farplane"),
			typeof(SNCameraRoot).method("OnConsoleCommand_nearplane"),
			typeof(SpawnConsoleCommand).method("OnConsoleCommand_spawn"),
			typeof(SpeedConsoleCommand).method("OnConsoleCommand_speed"),
			typeof(WaterParkCreature).method("OnConsoleCommand_setwpcage"),
		};

		public static void patchAll()
		{
			if (!patched)
			{																													$"floatParse toPatch size: {toPatch.Length}".logDbg();
				toPatch.forEach(p => HarmonyHelper.patch(p, transpiler: patch));
				patched = true;
			}
		}
	}
}