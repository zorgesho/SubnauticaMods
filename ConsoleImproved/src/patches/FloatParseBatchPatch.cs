using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;
using System.Collections.Generic;

using Harmony;

using Common;
using Common.Harmony;
using Common.Reflection;

namespace ConsoleImproved
{
	// patching vanilla console commands that use float.Parse method to use locale independent conversion
	[PatchClass]
	static class CommandsFloatParsePatch
	{
		static readonly MethodInfo floatParse = typeof(float).method("Parse", typeof(string));
		static readonly MethodInfo floatParseCulture = typeof(float).method("Parse", typeof(string), typeof(IFormatProvider));
		static readonly MethodInfo getInvariantCulture = typeof(CultureInfo).method("get_InvariantCulture");

		static bool prepare() => Main.config.fixVanillaCommandsFloatParse;

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(BaseFloodSim), "OnConsoleCommand_baseflood")]
		[HarmonyPatch(typeof(DayNightCycle), "OnConsoleCommand_daynightspeed")]
		[HarmonyPatch(typeof(CreateConsoleCommand), "OnConsoleCommand_create")]
		[HarmonyPatch(typeof(GameModeConsoleCommands), "OnConsoleCommand_damage")]
		[HarmonyPatch(typeof(PlayerMotor), "OnConsoleCommand_swimx")]
		[HarmonyPatch(typeof(SNCameraRoot), "OnConsoleCommand_farplane")]
		[HarmonyPatch(typeof(SNCameraRoot), "OnConsoleCommand_nearplane")]
		[HarmonyPatch(typeof(SpawnConsoleCommand), "OnConsoleCommand_spawn")]
		[HarmonyPatch(typeof(SpeedConsoleCommand), "OnConsoleCommand_speed")]
		[HarmonyPatch(typeof(WaterParkCreature), "OnConsoleCommand_setwpcage")]
		static IEnumerable<CodeInstruction> floatParseFix(IEnumerable<CodeInstruction> cins)
		{
			var list = cins.ciReplace(ci => ci.isOp(OpCodes.Call, floatParse),
				OpCodes.Call, getInvariantCulture,
				OpCodes.Call, floatParseCulture);

			Debug.assert(list.FindIndex(ci => ci.isOp(OpCodes.Call, floatParse)) == -1);

			return list;
		}
	}
}