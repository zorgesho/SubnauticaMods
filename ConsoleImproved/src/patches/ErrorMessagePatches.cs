using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;
using UnityEngine;
using UnityEngine.UI;

using Common.Harmony;
using Common.Reflection;
using Common.Configuration;

namespace ConsoleImproved
{
	[HarmonyPatch(typeof(ErrorMessage), "Awake")]
	static class ErrorMessageSettings
	{
		static ModConfig.MessagesSettings defaultSettings;

		public class RefreshSettings: Config.Field.IAction
		{ public void action() => refresh(); }

		public class RefreshTimeDelaySetting: Config.Field.IAction
		{
			public void action()
			{
				ErrorMessage.main.messages.Where(m => m.timeEnd - Time.time < 1e3f).
										   ForEach(msg => msg.timeEnd = Time.time + Main.config.msgsSettings.timeDelay);
			}
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
				if (!ErrorMessage.main)
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
			if (!ErrorMessage.main)
				return -1;

			float lastMsgPos = freeSlots? ErrorMessage.main.GetYPos(): 0f;
			return (int)((ErrorMessage.main.messageCanvas.rect.height + lastMsgPos) / messageSlotHeight);
		}
	}

	// don't clear onscreen messages while console is open
	[OptionalPatch, HarmonyPatch(typeof(ErrorMessage), "OnUpdate")]
	static class ErrorMessage_OnUpdate_Patch
	{
		static bool Prepare() => Main.config.keepMessagesOnScreen;

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> cins, ILGenerator ilg)
		{
			var list = cins.ToList();

			// is console visible
			void _injectStateCheck(int indexToInject, int indexToJump)
			{
				var label = list.ciDefineLabel(indexToJump, ilg);

				list.ciInsert(indexToInject,
					OpCodes.Ldsfld, typeof(DevConsole).field(nameof(DevConsole.instance)),
					OpCodes.Ldfld,  typeof(DevConsole).field(nameof(DevConsole.state)),
					OpCodes.Brtrue_S, label);
			}

			MethodInfo CanvasRenderer_SetAlpha = typeof(CanvasRenderer).method("SetAlpha");
			MethodInfo Transform_setLocalPosition = typeof(Transform).method("set_localPosition");

			int[] i = list.ciFindIndexes(ci => ci.isLDC(7f), // -2, jump index
										 ci => ci.isOp(OpCodes.Callvirt, Transform_setLocalPosition), // +1, inject index
										 ci => ci.isOp(OpCodes.Callvirt, CanvasRenderer_SetAlpha)); // +1, jump index
			if (i == null)
				return cins;

			// ignoring alpha changes for message entry if console is visible (last two lines in the second loop)
			_injectStateCheck(i[1] + 1, i[2] + 1);

			// ignoring (time > message.timeEnd) loop if console is visible (just jumping to "float num = this.offsetY * 7f" line)
			_injectStateCheck(2, i[0] - 2);

			return list;
		}
	}
}