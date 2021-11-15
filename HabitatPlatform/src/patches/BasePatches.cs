using HarmonyLib;
using UnityEngine;

namespace HabitatPlatform
{
	// hiding foundations on the platform (part of the code copied from vanilla)
	[HarmonyPatch(typeof(Base), "BuildFoundationGeometry")]
	static class Base_BuildFoundationGeometry_Patch
	{
		static bool Prefix(Base __instance, Int3 cell)
		{
#if DEBUG
			// foundations are rebuilded with each base piece, so we can't use Prepare here
			if (Main.config.dbgVisibleFoundations)
				return true;
#endif
			if (!__instance.gameObject.GetComponentInParent<HabitatPlatform.Tag>())
				return true;

			Transform foundation = __instance.SpawnPiece(Base.Piece.Foundation, cell);
			foundation.tag = "MainPieceGeometry";
			foundation.GetComponentsInChildren<MeshRenderer>()?.ForEach(mesh => mesh.enabled = false);

			return false;
		}
	}

	// don't add pillars for anything builded on the platform (in case we build in shallow water)
	[HarmonyPatch(typeof(BaseFoundationPiece), "OnGenerate")]
	static class BaseFoundationPiece_OnGenerate_Patch
	{
		static bool Prefix(BaseFoundationPiece __instance)
		{
			if (!__instance.gameObject.GetComponentInParent<HabitatPlatform.Tag>())
				return true;

			__instance.pillars.ForEach(pillar => pillar.root.SetActive(false));
			return false;
		}
	}
}