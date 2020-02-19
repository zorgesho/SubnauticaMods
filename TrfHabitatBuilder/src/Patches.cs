using Harmony;

namespace TrfHabitatBuilder
{
	[HarmonyPatch(typeof(BuilderTool), "Start")]
	static class BuilderTool_Start_Patch
	{
		static bool Prefix(BuilderTool __instance) => !__instance.gameObject.GetComponent<TrfBuilderControl>();
	}

	[HarmonyPatch(typeof(BuilderTool), "OnDisable")]
	static class BuilderTool_OnDisable_Patch
	{
		static bool Prefix(BuilderTool __instance) => !__instance.gameObject.GetComponent<TrfBuilderControl>();
	}

	[HarmonyPatch(typeof(BuilderTool), "LateUpdate")]
	static class BuilderTool_LateUpdate_Patch
	{
		static bool Prefix(BuilderTool __instance)
		{
			TrfBuilderControl tbc = __instance.gameObject.GetComponent<TrfBuilderControl>();
			if (tbc == null)
				return true;

			tbc.updateBeams();
			return false;
		}
	}

	[HarmonyPatch(typeof(QuickSlots), "SetAnimationState")]
	static class QuickSlots_SetAnimationState_Patch
	{
		static readonly string builderToolName = nameof(TrfBuilder).ToLower();

		static bool Prefix(QuickSlots __instance, string toolName)
		{
			if (toolName != builderToolName)
				return true;

			__instance.SetAnimationState("terraformer");
			return false;
		}
	}

#if DEBUG
	// for debugging
	[HarmonyPatch(typeof(Constructable), "GetConstructInterval")]
	static class Constructable_GetConstructInterval_Patch
	{
		static bool Prefix(ref float __result)
		{
			__result = Main.config.forcedBuildTime;
			return false;
		}
	}
#endif
}