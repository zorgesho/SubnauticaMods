using Harmony;
using UnityEngine;

using Common;

namespace HabitatPlatform
{
	[HarmonyPatch(typeof(Base), "BuildFoundationGeometry")]
	static class Base_BuildFoundationGeometry_Patch
	{
		static bool Prepare() => !Main.config.dbgVisibleFoundations;

		static bool Prefix(Base __instance, Int3 cell)
		{
			if (!__instance.gameObject.getComponentInHierarchy<HabitatPlatform.Tag>())
				return true;

			Transform foundation = __instance.SpawnPiece(Base.Piece.Foundation, cell);
			foundation.tag = "MainPieceGeometry";

			foundation.GetAllComponentsInChildren<MeshRenderer>()?.forEach(mesh => mesh.enabled = false);

			return false;
		}
	}
}