using Harmony;
using UnityEngine;

namespace MiscPatches
{
	[HarmonyPatch(typeof(Builder), "UpdateAllowed")]
	static class Builder_UpdateAllowed_Patch
	{
		static bool Prepare() => Main.config.dbg.buildAnywhere;

		static bool Prefix(ref bool __result)
		{
			if (!Input.GetKey(Main.config.dbg.forceBuildAllowKey))
				return true;

			__result = true;
			return false;
		}
	}
}