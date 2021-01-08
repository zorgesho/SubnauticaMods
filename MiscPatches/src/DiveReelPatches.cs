#if GAME_SN
using Harmony;
using UnityEngine;

namespace MiscPatches
{
	// for destoring nodes quicker
	[HarmonyPatch(typeof(DiveReelNode), "Update")]
	static class DiveReelNode_Update_Patch
	{
		static bool Prepare() => Main.config.gameplayPatches;

		static void Postfix(DiveReelNode __instance)
		{
			if (__instance.destroySelf && __instance.selfScale < 0.0001f)
				Object.Destroy(__instance.gameObject);
		}
	}

	// use dive reel on existing node to destroy it
	[HarmonyPatch(typeof(DiveReel), "OnToolUseAnim")]
	static class DiveReel_RemoveNodes_Patch
	{
		static bool Prepare() => Main.config.gameplayPatches;

		static bool Prefix(DiveReel __instance)
		{
			Targeting.GetTarget(Player.main.gameObject, 2f, out GameObject gameObject, out float num, null);

			if (gameObject)
			{
				DiveReelNode reelNode = gameObject.GetComponent<DiveReelNode>();

				if (reelNode && reelNode.firstArrow)
				{
					removeDiveReelNode(__instance, reelNode);
					return false;
				}
			}

			return true;
		}

		static void removeDiveReelNode(DiveReel reel, DiveReelNode node)
		{
			reel.nodes.Remove(node.gameObject);
			node.DestroySelf(0.1f);

			recalcNodes(reel);
		}

		static void recalcNodes(DiveReel reel)
		{
			reel.nodePositions.Clear();

			Transform prevTransform = reel.transform;

			foreach	(var nodeObj in reel.nodes)
			{
				DiveReelNode node = nodeObj.GetComponent<DiveReelNode>();

				reel.nodePositions.Add(node.transform.position);

				if (!node.firstArrow)
					node.previousArrowPos = prevTransform;

				prevTransform = node.transform;
				reel.lastNodeTransform = node.transform;
			}
		}
	}
}
#endif