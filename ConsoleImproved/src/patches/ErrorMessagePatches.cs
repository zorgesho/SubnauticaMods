using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;
using UnityEngine;

using Common;

namespace ConsoleImproved
{
	using Debug = Common.Debug;
#if VER_1_1_0
	// change position for messages
	[HarmonyPatch(typeof(ErrorMessage), "Awake")]
	static class ErrorMessage_Awake_Patch
	{
		static bool Prepare() => !Main.config.msgsSettings.useDefault;

		static void Postfix(ErrorMessage __instance)
		{
			__instance.offset = new Vector2(Main.config.msgsSettings.offsetX, Main.config.msgsSettings.offsetY);

			__instance.ySpacing = Main.config.msgsSettings.ySpacing;
			__instance.timeFlyIn = Main.config.msgsSettings.timeFlyIn;
			__instance.timeDelay = Main.config.msgsSettings.timeDelay;
			__instance.timeFadeOut  = Main.config.msgsSettings.timeFadeOut;
			__instance.timeInvisible = Main.config.msgsSettings.timeInvisible;
		}
	}
#endif

	// don't clear onscreen messages while console is open
	[HarmonyPatch(typeof(ErrorMessage), "OnUpdate")]
	static class ErrorMessage_OnUpdate_Patch
	{
		static bool Prepare() => Main.config.keepMessagesOnScreen;

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> cins, ILGenerator ilg)
		{
			var list = cins.ToList();

			// is console visible
			void _injectStateCheck(int indexToInject, object labelToJump)
			{
				HarmonyHelper.ciInsert(list, indexToInject, new List<CodeInstruction>()
				{
					new CodeInstruction(OpCodes.Ldsfld, typeof(DevConsole).field(nameof(DevConsole.instance))),
					new CodeInstruction(OpCodes.Ldfld,  typeof(DevConsole).field(nameof(DevConsole.state))),
					new CodeInstruction(OpCodes.Brtrue_S, labelToJump)
				});
			}

			// ignoring (time > message.timeEnd) loop if console is visible (just jumping to "float num = this.offsetY * 7f" line)
			int indexToJump1 = list.FindIndex(ci => ci.isLDC(7f)) - 2;																$"ErrorMessage.OnUpdate patch: indexToJump1: {indexToJump1}".logDbg();

			Debug.assert(indexToJump1 >= 0, "ErrorMessage.OnUpdate patch: indexToJump1 is invalid");
			if (indexToJump1 < 0)
				return cins;

			Label lb1 = ilg.DefineLabel();
			list[indexToJump1].labels.Add(lb1);

			int indexToInject1 = 2;
			_injectStateCheck(indexToInject1, lb1);


			// ignoring alpha changes for message entry if console is visible (last two lines in the second loop)
			MethodInfo CanvasRenderer_SetAlpha = typeof(CanvasRenderer).method(nameof(CanvasRenderer.SetAlpha));
			int indexToJump2 = list.FindIndex(indexToJump1, ci => ci.isOp(OpCodes.Callvirt, CanvasRenderer_SetAlpha)) + 1;			$"ErrorMessage.OnUpdate patch: indexToJump2: {indexToJump2}".logDbg();

			Debug.assert(indexToJump2 >= 0, "ErrorMessage.OnUpdate patch: indexToJump2 is invalid");
			if (indexToJump2 < 0)
				return cins;

			Label lb2 = ilg.DefineLabel();
			list[indexToJump2].labels.Add(lb2);

			MethodInfo Transform_setLocalPosition = typeof(Transform).method("set_localPosition");
			int indexToInject2 = list.FindIndex(indexToJump1, ci => ci.isOp(OpCodes.Callvirt, Transform_setLocalPosition)) + 1;		$"ErrorMessage.OnUpdate patch: indexToInject2: {indexToInject2}".logDbg();

			Debug.assert(indexToInject2 >= 0, "ErrorMessage.OnUpdate patch: indexToInject2 is invalid");
			if (indexToInject2 < 0)
				return cins;

			_injectStateCheck(indexToInject2, lb2);

			return list;
		}
	}
}