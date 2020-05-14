using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;
using UnityEngine;
using UnityEngine.UI;

using Common;
using Common.Configuration;

namespace ConsoleImproved
{
	using Debug = Common.Debug;

	[HarmonyPatch(typeof(ErrorMessage), "Awake")]
	static class ErrorMessageSettings
	{
		static ModConfig.MessagesSettings defaultSettings;

		public class RefreshSettings: Config.Field.IAction
		{ public void action() => refresh(); }

		public class RefreshTimeDelaySetting: Config.Field.IAction
		{
			public void action() => ErrorMessage.main.messages.Where(m => m.timeEnd - Time.time < 1e3f).
															   ForEach(msg => msg.timeEnd = Time.time + Main.config.msgsSettings.timeDelay);
		}

		[HarmonyPriority(Priority.High)]
		static void Postfix(ErrorMessage __instance)
		{
			var em = __instance;

			defaultSettings ??= new ModConfig.MessagesSettings()
			{
				offset = em.offset.x,
				messageSpacing = em.ySpacing,
				timeFly = em.timeFlyIn,
				timeDelay = em.timeDelay,
				timeFadeOut = em.timeFadeOut,
				timeInvisible = em.timeInvisible,

				fontSize = em.prefabMessage.fontSize,
				textLineSpacing = em.prefabMessage.lineSpacing,
				textWidth = em.prefabMessage.rectTransform.sizeDelta.x
			};

			if (Main.config.msgsSettings.customize)
				refresh();
		}

		static void refresh()
		{
			var em = ErrorMessage.main;
			var settings = Main.config.msgsSettings.customize? Main.config.msgsSettings: defaultSettings;

			em.offset = new Vector2(settings.offset, settings.offset);
			em.ySpacing = settings.messageSpacing;
			em.timeFlyIn = settings.timeFly;
			em.timeDelay = settings.timeDelay;
			em.timeFadeOut = settings.timeFadeOut;
			em.timeInvisible = settings.timeInvisible;

			var texts = new List<Text>(em.pool);
			em.messages.ForEach(message => texts.Add(message.entry));
			texts.Add(em.prefabMessage);

			if (sampleMessage)
				texts.Add(sampleMessage.GetComponent<Text>());

			foreach (var text in texts)
			{
				text.lineSpacing = settings.textLineSpacing;
				text.rectTransform.sizeDelta = new Vector2(settings.textWidth, 0f);
				text.fontSize = settings.fontSize;
			}
		}

		public static float messageSlotHeight
		{
			get
			{
				if (ErrorMessage.main == null)
					return -1f;

				if (!sampleMessage) // using this to get real preferredHeight
				{
					sampleMessage = Object.Instantiate(ErrorMessage.main.prefabMessage.gameObject);
					sampleMessage.GetComponent<Text>().rectTransform.SetParent(ErrorMessage.main.messageCanvas);
					sampleMessage.SetActive(false);
				}

				return sampleMessage.GetComponent<Text>().preferredHeight + ErrorMessage.main.ySpacing;
			}
		}
		static GameObject sampleMessage;

		// get max message slots with current settings
		public static int getSlotCount(bool freeSlots)
		{
			if (ErrorMessage.main == null)
				return -1;

			float lastMsgPos = freeSlots? ErrorMessage.main.GetYPos(): 0f;
			return (int)((ErrorMessage.main.messageCanvas.rect.height + lastMsgPos) / messageSlotHeight);
		}
	}


	// don't clear onscreen messages while console is open
	[HarmonyHelper.OptionalPatch]
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
				HarmonyHelper.ciInsert(list, indexToInject, HarmonyHelper.toCIList
				(
					OpCodes.Ldsfld, typeof(DevConsole).field(nameof(DevConsole.instance)),
					OpCodes.Ldfld,  typeof(DevConsole).field(nameof(DevConsole.state)),
					OpCodes.Brtrue_S, labelToJump
				));
			}

			// ignoring (time > message.timeEnd) loop if console is visible (just jumping to "float num = this.offsetY * 7f" line)
			int indexToJump1 = list.FindIndex(ci => ci.isLDC(7f)) - 2;
			Debug.assert(indexToJump1 >= 0);

			if (indexToJump1 < 0)
				return cins;

			Label lb1 = ilg.DefineLabel();
			list[indexToJump1].labels.Add(lb1);

			int indexToInject1 = 2;
			_injectStateCheck(indexToInject1, lb1);

			// ignoring alpha changes for message entry if console is visible (last two lines in the second loop)
			MethodInfo CanvasRenderer_SetAlpha = typeof(CanvasRenderer).method(nameof(CanvasRenderer.SetAlpha));
			int indexToJump2 = list.FindIndex(indexToJump1, ci => ci.isOp(OpCodes.Callvirt, CanvasRenderer_SetAlpha)) + 1;
			Debug.assert(indexToJump2 >= 0);

			if (indexToJump2 < 0)
				return cins;

			Label lb2 = ilg.DefineLabel();
			list[indexToJump2].labels.Add(lb2);

			MethodInfo Transform_setLocalPosition = typeof(Transform).method("set_localPosition");
			int indexToInject2 = list.FindIndex(indexToJump1, ci => ci.isOp(OpCodes.Callvirt, Transform_setLocalPosition)) + 1;
			Debug.assert(indexToInject2 >= 0);

			if (indexToInject2 < 0)
				return cins;

			_injectStateCheck(indexToInject2, lb2);

			return list;
		}
	}
}