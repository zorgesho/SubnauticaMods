using Harmony;

namespace TerraformerBuilder
{
	[HarmonyPatch(typeof(BuilderTool), "Start")]
	static class BuilderTool_Start_Patch
	{
		static bool Prefix(BuilderTool __instance) => !__instance.gameObject.GetComponent<TerraBuilderControl>();
	}

	[HarmonyPatch(typeof(BuilderTool), "OnDisable")]
	static class BuilderTool_OnDisable_Patch
	{
		static bool Prefix(BuilderTool __instance) => !__instance.gameObject.GetComponent<TerraBuilderControl>();
	}
	
	[HarmonyPatch(typeof(BuilderTool), "LateUpdate")]
	static class BuilderTool_LateUpdate_Patch
	{
		static bool Prefix(BuilderTool __instance)
		{
			TerraBuilderControl	tbc = __instance.gameObject.GetComponent<TerraBuilderControl>();
			if (tbc == null)
				return true;

			tbc.updateBeams();
			return false;
		}
	}

	[HarmonyPatch(typeof(QuickSlots), "SetAnimationState")]
	static class QuickSlots_SetAnimationState_Patch
	{
		static readonly string builderToolName = nameof(TerraBuilder).ToLower();
		
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