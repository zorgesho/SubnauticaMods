using System.Linq;
using System.Collections.Generic;

using Harmony;
using UnityEngine;

using Common;

namespace HabitatPlatform
{
	[HarmonyPatch(typeof(Base), "BuildFoundationGeometry")]
	static class Base_BuildFoundationGeometry_Patch
	{
		static bool Prefix(Base __instance, Int3 cell)
		{
			// foundations are rebuilded with each base piece, so we can't use Prepare here
			if (Main.config.dbgVisibleFoundations)
				return true;

			if (!__instance.gameObject.getComponentInHierarchy<HabitatPlatform.Tag>())
				return true;

			Transform foundation = __instance.SpawnPiece(Base.Piece.Foundation, cell);
			foundation.tag = "MainPieceGeometry";

			foundation.GetAllComponentsInChildren<MeshRenderer>()?.forEach(mesh => mesh.enabled = false);

			return false;
		}
	}


	[HarmonyPatch(typeof(Constructor), "GetItemSpawnPoint")]
	static class Constructor_GetItemSpawnPoint_Patch
	{
		static bool Prefix(Constructor __instance, TechType techType, ref Transform __result)
		{
			if (techType != HabitatPlatform.TechType)
				return true;

			__result = __instance.GetItemSpawnPoint(TechType.RocketBase);
			return false;
		}
	}


	static class BuilderPatches
	{
		static bool dirty = true;

		static Base lastBase = null;
		static HabitatPlatform.Tag lastPlatform = null;

		static bool isHabitatPlatform
		{
			get
			{
				if (dirty)
				{
					lastBase = Builder.ghostModel?.GetComponent<BaseGhost>()?.targetBase;
					lastPlatform = lastBase?.gameObject.getComponentInHierarchy<HabitatPlatform.Tag>();

					dirty = false;
				}

				return lastPlatform != null;
			}
		}

		[HarmonyPatch(typeof(BaseGhost), "FindBase")]
		static class BaseGhost_FindBase_Patch
		{
			static void Postfix(Base __result) => dirty = (lastBase != __result);
		}

		[HarmonyPatch(typeof(Builder), "GetOverlappedColliders")]
		static class Builder_GetOverlappedColliders_Patch
		{
			static void Postfix(List<Collider> results)
			{
				if (!isHabitatPlatform)
					return;

				if (Main.config.dbgPrintColliders)
					results.Select(coll => coll.name).ToList().onScreen("colliders");

				results.Clear(); // TODO: make list of ignored colliders (like floor) // TODO: also check that this is our platform
			}
		}
	}
}