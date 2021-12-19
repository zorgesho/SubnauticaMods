﻿#if GAME_SN
using System.Collections.Generic;

using HarmonyLib;

using Common;
using Common.Harmony;
#endif

namespace UITweaks.StorageTweaks
{
	static class Utils
	{
		public static bool isAllowedToPickUpNonEmpty(this PickupableStorage storage)
		{
#if GAME_SN
			return false;
#elif GAME_BZ
			return storage.allowPickupWhenNonEmpty;
#endif
		}

		public static int getItemSize(TechType techType)
		{
#if GAME_SN
			var size = CraftData.GetItemSize(techType);
#elif GAME_BZ
			var size = TechData.GetItemSize(techType);
#endif
			return size.x * size.y;
		}

#if GAME_SN // code is copied from BZ with some modifications
		static readonly Dictionary<GameInput.Button, string> bindingCache = new();
		static readonly Dictionary<GameInput.Button, Dictionary<string, string>> textCache = new();

		public static string GetText(this HandReticle _, string text, bool translate, GameInput.Button button)
		{
			if (text.isNullOrEmpty())
				return text;

			if (!textCache.TryGetValue(button, out Dictionary<string, string> buttonCache))
				textCache[button] = buttonCache = new();

			if (!buttonCache.TryGetValue(text, out string result))
			{
				result = translate? Language.main.Get(text): text;

				if (!bindingCache.TryGetValue(button, out string buttonBind))
					bindingCache[button] = buttonBind = uGUI.FormatButton(button);

				result = Language.main.GetFormat("HandReticleAddButtonFormat", result, buttonBind);
				buttonCache[text] = result;
			}

			return result;
		}

		[OptionalPatch, PatchClass]
		static class Patches
		{
			static bool prepare() => Main.config.storageTweaks.enabled;

			[HarmonyPostfix]
			[HarmonyPatch(typeof(HandReticle), "OnBindingsChanged")]
			[HarmonyPatch(typeof(HandReticle), "OnLanguageChanged")]
			static void clearCache()
			{
				bindingCache.Clear();
				textCache.Values.forEach(c => c.Clear());
			}
		}
#endif // GAME_SN
	}
}