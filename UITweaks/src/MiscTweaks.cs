using System.Collections;

using Harmony;
using UnityEngine;

using Common.Harmony;

namespace UITweaks
{
	static class MiscTweaks
	{
		[OptionalPatch, PatchClass]
		static class BuilderMenuHotkeys
		{
			static bool prepare() => Main.config.builderMenuTabHotkeysEnabled;

			static IEnumerator builderMenuTabHotkeys()
			{
				while (uGUI_BuilderMenu.singleton.state)
				{
					for (int i = 0; i < 5; i++)
						if (Input.GetKeyDown(KeyCode.Alpha1 + i))
							uGUI_BuilderMenu.singleton.SetCurrentTab(i);

					yield return null;
				}
			}

			[HarmonyPostfix, HarmonyPatch(typeof(uGUI_BuilderMenu), "GetToolbarTooltip")]
			static void modifyTooltip(int index, ref string tooltipText)
			{
				if (Main.config.showToolbarHotkeys)
					tooltipText = $"<size=25><color=#ADF8FFFF>{index + 1}</color> - </size>{tooltipText}";
			}

			[HarmonyPostfix, HarmonyPatch(typeof(uGUI_BuilderMenu), "Open")]
			static void openMenu()
			{
				UWE.CoroutineHost.StartCoroutine(builderMenuTabHotkeys());
			}
		}
	}
}