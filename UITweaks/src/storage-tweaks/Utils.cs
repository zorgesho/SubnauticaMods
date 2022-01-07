﻿using System.Linq;
using System.Collections.Generic;

using UnityEngine;

#if GAME_SN
using HarmonyLib;

using Common;
using Common.Harmony;
#endif

namespace UITweaks.StorageTweaks
{
	static class Utils
	{
		public record ItemCount(TechType techType, int count)
		{
			public string name => Language.main.Get(techType);
		}

		// return items in container ordered by descending count
		public static List<ItemCount> getItems(ItemsContainer container)
		{
			return container._items.
				Select(pair => new ItemCount(pair.Key, pair.Value.items.Count)).
				OrderByDescending(item => item.count).
				ToList();
		}

		public static string getPrefabClassId(MonoBehaviour cmp)
		{
			return cmp.GetComponentInParent<PrefabIdentifier>(true)?.ClassId ?? "";
		}

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