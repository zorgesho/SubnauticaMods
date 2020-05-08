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
		static IEnumerable<CodeInstruction> transpiler(IEnumerable<CodeInstruction> cins)
		{
			MethodInfo floatParse = typeof(float).method("Parse", typeof(string));
			MethodInfo floatParseCulture = typeof(float).method("Parse", typeof(string), typeof(IFormatProvider));
			MethodInfo getInvariantCulture = typeof(System.Globalization.CultureInfo).method("get_InvariantCulture");

			var list = HarmonyHelper.ciReplace(cins, ci => ci.isOp(OpCodes.Call, floatParse),
												new CodeInstruction(OpCodes.Call, getInvariantCulture), new CodeInstruction(OpCodes.Call, floatParseCulture));

			Debug.assert(list.FindIndex(ci => ci.isOp(OpCodes.Call, floatParse)) == -1);

			return list;
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

		static bool patched = false;
		public static void patchAll()
		{
			if (!patched && (patched = true))
				toPatch.forEach(p => HarmonyHelper.patch(p, transpiler: patch));
		}
	}
}