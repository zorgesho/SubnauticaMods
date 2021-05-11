using Harmony;
using UnityEngine;

using Common.Harmony;

namespace UITweaks
{
	static partial class BulkCraftingTooltip
	{
		[OptionalPatch, PatchClass]
		static class IconAnimPatches
		{
			const float maxAnimTime = 3f;
			static float initialAnimInterval = -1f;

			static bool prepare()
			{
				bool fasterAnim = Main.config.bulkCrafting.enabled && Main.config.bulkCrafting.inventoryItemsFasterAnim;

				if (uGUI_IconNotifier.main) // in case we changing option in runtime
				{
					if (!fasterAnim)
						uGUI_IconNotifier.main.interval = initialAnimInterval;
					else if (initialAnimInterval < 0f)
						init(uGUI_IconNotifier.main);
				}

				return fasterAnim;
			}

			[HarmonyPostfix, HarmonyPatch(typeof(uGUI_IconNotifier), "Awake")]
			static void init(uGUI_IconNotifier __instance)
			{
				initialAnimInterval = __instance.interval;
			}

			[HarmonyPostfix, HarmonyPatch(typeof(uGUI_IconNotifier), "Play")]
			static void changeAnimInterval(uGUI_IconNotifier __instance)
			{
				__instance.interval = Mathf.Min(maxAnimTime / __instance.queue.Count, initialAnimInterval);
			}
		}
	}
}