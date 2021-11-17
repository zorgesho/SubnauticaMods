using System;
using System.Reflection.Emit;
using System.Collections;
using System.Collections.Generic;

using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

#if GAME_BZ
using System.Linq;
using System.Reflection;
#endif

namespace Common.Utils
{
	using Harmony;

#if GAME_BZ
	using Reflection;
#endif
	static class MainMenuMessages
	{
		public const int defaultSize = 25;
		public const string defaultColor = "red";

		static List<string> messageQueue;
		static List<ErrorMessage._Message> messages;

		public static void add(string msg, int size = defaultSize, string color = defaultColor, bool autoformat = true)
		{
			if (autoformat)
				msg = $"<size={size}><color={color}><b>[{Mod.name}]:</b> {msg}</color></size>";

			init();

			if (ErrorMessage.main != null)
				_add(msg);
			else
				messageQueue.Add(msg);
		}

		static void init()
		{
			if (messageQueue != null)
				return;

			messageQueue = new List<string>();
			messages = new List<ErrorMessage._Message>();
			HarmonyHelper.patch(typeof(Patches));

			SceneManager.sceneLoaded += onSceneLoaded;
		}

		static void _add(string msg)
		{
			ErrorMessage.AddDebug(msg);

			var message = ErrorMessage.main.GetExistingMessage(msg);
			messages.Add(message);
			message.timeEnd += 1e6f;
		}

		static void onSceneLoaded(Scene scene, LoadSceneMode _)
		{
			if (scene.name != "Main")
				return;
																						"MainMenuMessages: game loading started".logDbg();
			SceneManager.sceneLoaded -= onSceneLoaded;
			ErrorMessage.main.StartCoroutine(_waitForLoad());

			static IEnumerator _waitForLoad()
			{
				yield return new WaitForSeconds(1f);
				yield return new WaitWhile(() => SaveLoadManager.main.isLoading);
																						"MainMenuMessages: game loading finished".logDbg();
				messages.ForEach(msg => msg.timeEnd = Time.time + 1f);

				messages.Clear();
				HarmonyHelper.patch(typeof(Patches), false);
			}
		}

		static class Patches
		{
			[HarmonyPostfix, HarmonyPatch(typeof(ErrorMessage), "Awake")]
			static void addMessages()
			{
				messageQueue.ForEach(_add);
				messageQueue.Clear();
			}

			// we changing result for 'float value = Mathf.Clamp01(MathExtensions.EvaluateLine(...' to 1.0f
			// so text don't stay in the center of the screen (because of changed 'timeEnd')
			[HarmonyTranspiler, HarmonyPatch(typeof(ErrorMessage), "OnUpdate")]
			static IEnumerable<CodeInstruction> updateMessages(IEnumerable<CodeInstruction> cins)
			{
				static float _getVal(float val, ErrorMessage._Message message) => messages.Contains(message)? 1f: val;

				return CIHelper.ciInsert(cins,
					ci => ci.isOpLoc(OpCodes.Stloc_S, 11), +0, 1,
						OpCodes.Ldloc_S, 6,
						CIHelper.emitCall<Func<float, ErrorMessage._Message, float>>(_getVal));
			}
		}

#if GAME_BZ
		public static class BZFixPatches
		{
			public static readonly HarmonyHelper.LazyPatcher patcher = new();

			// Time.time -> PDA.time in 'OnSceneLoaded'
			[HarmonyTranspiler]
			[HarmonyHelper.Patch("QModManager.Utility.MainMenuMessages+<>c, QModInstaller", "<OnSceneLoaded>b__13_2")]
			[HarmonyHelper.Patch(HarmonyHelper.PatchOptions.PatchOnce | HarmonyHelper.PatchOptions.CanBeAbsent)]
			static IEnumerable<CodeInstruction> MainMenuMessages_OnSceneLoaded_Transpiler(IEnumerable<CodeInstruction> cins)
			{
				var list = cins.ToList();
				int i = list.FindIndex(new CIHelper.OpMatch(OpCodes.Call, typeof(Time).property("time").GetGetMethod()));

				if (i == -1)
				{																								"QModManager.Utility.MainMenuMessages.OnSceneLoaded doesn't need to be patched".logDbg();
					return cins;
				}

				list[i].operand = typeof(PDA).property("time").GetGetMethod();
				return list;
			}

			// adding binding flags to GetField("m_rectTransform") in 'LoadDynamicAssembly'
			[HarmonyTranspiler]
			[HarmonyHelper.Patch("QModManager.Utility.MainMenuMessages, QModInstaller", "LoadDynamicAssembly")]
			[HarmonyHelper.Patch(HarmonyHelper.PatchOptions.PatchOnce | HarmonyHelper.PatchOptions.CanBeAbsent)]
			static IEnumerable<CodeInstruction> MainMenuMessages_LoadDynamicAssembly_Transpiler(IEnumerable<CodeInstruction> cins)
			{
				var list = cins.ToList();
				int i = list.FindIndex(ci => ci.isOp(OpCodes.Ldstr, "m_rectTransform"));

				if (i == -1 || !Equals(list[i + 1].operand, typeof(Type).method("GetField", typeof(string))))
				{																								"QModManager.Utility.MainMenuMessages.LoadDynamicAssembly doesn't need to be patched".logDbg();
					return cins;
				}

				return list.ciReplace(i + 1,
					OpCodes.Ldc_I4_S, (int)(BindingFlags.Instance | BindingFlags.NonPublic),
					OpCodes.Callvirt, typeof(Type).method("GetField", typeof(string), typeof(BindingFlags)));
			}
		}
#endif // GAME_BZ
	}
}