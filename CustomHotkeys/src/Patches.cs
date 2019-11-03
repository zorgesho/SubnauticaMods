using UnityEngine;
using Harmony;

using Common;

namespace CustomHotkeys
{
	[HarmonyPatch(typeof(MainGameController), "Update")]
	class MainGameController_Update_Patch
	{
		static void Postfix(MainGameController __instance)
		{
			if (Main.config.disableDevTools)
			{
				if (Input.GetKeyDown(KeyCode.F1))
					foreach (TerrainDebugGUI terrainDebugGUI in Object.FindObjectsOfType<TerrainDebugGUI>())
						terrainDebugGUI.enabled = false;

				if (Input.GetKeyDown(KeyCode.F3))
					foreach (GraphicsDebugGUI graphicsDebugGUI in Object.FindObjectsOfType<GraphicsDebugGUI>())
						graphicsDebugGUI.enabled = false;
			}
		}
	}
	
	[HarmonyPatch(typeof(GUIController), "Update")]
	class GUIController_Update_Patch
	{
		static bool Prefix(GUIController __instance)
		{
			return !Main.config.disableDevTools;
		}
	}
	
	[HarmonyPatch(typeof(uGUI_FeedbackCollector), "Awake")]
	class uGUI_FeedbackCollector_Update_Patch
	{
		static void Postfix(uGUI_FeedbackCollector __instance)
		{
			__instance.enabled = false;
		}
	}

	

	
}
