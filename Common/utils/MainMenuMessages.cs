﻿using System;
using System.Reflection.Emit;
using System.Collections;
using System.Collections.Generic;

using Harmony;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Common.Utils
{
	using Harmony;

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
				messageQueue.ForEach(msg => _add(msg));
				messageQueue.Clear();
			}

			// we changing result for 'float value = Mathf.Clamp01(MathExtensions.EvaluateLine(...' to 1.0f
			// so text don't stay in the center of the screen (because of changed 'timeEnd')
			[HarmonyTranspiler, HarmonyPatch(typeof(ErrorMessage), "OnUpdate")]
			static IEnumerable<CodeInstruction> updateMessages(IEnumerable<CodeInstruction> cins)
			{
				static float _getVal(float val, ErrorMessage._Message message) => messages.Contains(message)? 1f: val;

				return CIHelper.ciInsert(cins,
					cin => cin.isOpLoc(OpCodes.Stloc_S, 11), +0, 1,
						OpCodes.Ldloc_S, 6,
						CIHelper.emitCall<Func<float, ErrorMessage._Message, float>>(_getVal));
			}
		}
	}
}