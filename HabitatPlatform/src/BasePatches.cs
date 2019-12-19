using System;
using System.Collections.Generic;

using UnityEngine;
using Harmony;
using Common;

namespace HabitatPlatform
{
	[HarmonyPatch(typeof(Base), "Start")]
	static class Base_Start_Patch
	{
		static void Prefix(Base __instance)
		{
			$"{__instance.gameObject.transform.parent}".log();
		}
	}

	[HarmonyPatch(typeof(Base), "BuildFoundationGeometry")]
	static class Base_BuildFoundationGeometry_Patch
	{
		static bool Prefix(Base __instance, Int3 cell)
		{
			if (!__instance.gameObject.getComponentInHierarchy<Rocket>()) // todo: make new component for platform
				return true;

			Transform foundation = __instance.SpawnPiece(Base.Piece.Foundation, cell);
			foundation.tag = "MainPieceGeometry";

			foundation.GetAllComponentsInChildren<MeshRenderer>()?.forEach(mesh => mesh.enabled = false);
			//MeshRenderer[] meshes = foundation.GetAllComponentsInChildren<MeshRenderer>();
			//$"{meshes.Length}".log();

			return false;
		}
	}
}