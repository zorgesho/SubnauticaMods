using System;

using Harmony;
using UnityEngine;

namespace DebrisRecycling
{
	[HarmonyPatch(typeof(BuilderTool), "Construct")]
	static class BuilderTool_Construct_Patch
	{
		// don't allow construct debris back
		static bool Prefix(Constructable c, bool state) => !(state && c.gameObject.GetComponent<DebrisDeconstructable>());
	}

	[HarmonyPatch(typeof(BuilderTool), "OnHover", new Type[] { typeof(Constructable) })]
	static class BuilderTool_OnHover_Patch
	{
		static bool Prefix(BuilderTool __instance, Constructable constructable)
		{
			if (!constructable.gameObject.GetComponent<DebrisDeconstructable>())
				return true;

			HandReticle hand = HandReticle.main;
			hand.SetInteractText(L10n.str("ids_salvageableDebris"), __instance.deconstructText, false, false, HandReticle.Hand.None);

			if (!constructable.constructed)
			{
				hand.SetProgress(constructable.amount);
				hand.SetIcon(HandReticle.IconType.Progress, 1.5f);
			}

			return false;
		}
	}

	[HarmonyPatch(typeof(BuilderTool), "HandleInput")]
	static class BuilderTool_HandleInput_Patch
	{
		static void Prefix(BuilderTool __instance)
		{
			if (__instance.isDrawn && !Builder.isPlacing && AvatarInputHandler.main.IsEnabled())
			{
				Targeting.GetTarget(Player.main.gameObject, 10f, out GameObject go, out float num, null);

				if (go)
					DebrisPatcher.processObject(go);
			}
		}
	}
}