using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;
using UnityEngine;

using Common;
using Common.Harmony;
using Common.Reflection;
using Common.Configuration;

namespace ConsoleImproved
{
#if GAME_SN
	using Text = UnityEngine.UI.Text;
#elif GAME_BZ
	using Text = TMPro.TextMeshProUGUI;
#endif

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
			var text = em.prefabMessage.GetComponent<Text>(); // for GAME_SN it's just 'prefabMessage'

			defaultSettings ??= new ModConfig.MessagesSettings()
			{
				offset = em.offset.x,
				messageSpacing = em.ySpacing,
				timeFly = em.timeFlyIn,
				timeDelay = em.timeDelay,
				timeFadeOut = em.timeFadeOut,
				timeInvisible = em.timeInvisible,
				fontSize = (int)text.fontSize, // in GAME_BZ 'fontSize' is float
				textWidth = text.rectTransform.sizeDelta.x,
				textLineSpacing = text.lineSpacing
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

			List<Text> texts = new (em.pool);
			em.messages.ForEach(message => texts.Add(message.entry));
			texts.Add(em.prefabMessage.GetComponent<Text>());

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
			static float _getYPos()
			{
#if GAME_SN
				return ErrorMessage.main.GetYPos();
#elif GAME_BZ
				return ErrorMessage.main.GetYPos(-1, 1.0f);
#endif
			}

			if (!ErrorMessage.main)
				return -1;

			float lastMsgPos = freeSlots? _getYPos(): 0f;
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

#if GAME_SN
			MethodInfo setAlpha = typeof(CanvasRenderer).method("SetAlpha");
#elif GAME_BZ
			MethodInfo setAlpha = typeof(TMProExtensions).method("SetAlpha");
#endif
			MethodInfo setLocalPosition = typeof(Transform).method("set_localPosition");

			int[] i = list.ciFindIndexes(ci => ci.isLDC(7f), // -2, jump index
										 ci => ci.isOp(OpCodes.Callvirt, setLocalPosition), // +1, inject index
										 ci => ci.isOp(Mod.Consts.isGameSN? OpCodes.Callvirt: OpCodes.Call, setAlpha)); // +1, jump index
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