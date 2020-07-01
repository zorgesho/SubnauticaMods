using System.Collections.Generic;

using UnityEngine;
using Harmony;

using Common;

namespace HabitatPlatform
{
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


	static class BuilderStuff
	{
		public static bool dirty = true;

		public static Base lastBase = null;

		public static bool isHabitatPlatform
		{
			get
			{
				if (dirty)
				{
					_isHabitatPlatform = Builder.ghostModel?.GetComponent<BaseGhost>()?.targetBase?.gameObject.getComponentInHierarchy<Rocket>();
					dirty = false;
				}

				return _isHabitatPlatform;
			}
		}
		static bool _isHabitatPlatform = false;

	}

	//[HarmonyPatch(typeof(BaseGhost), "FindBase")]
	static class BaseGhost_FindBase_Patch
	{
		static void Postfix(Base __result)
		{
			if (BuilderStuff.lastBase != __result)
				BuilderStuff.dirty = true;
		}
	}

	//public static void GetObstacles(Vector3 position, Quaternion rotation, List<OrientedBounds> localBounds, List<GameObject> results)
	//[HarmonyPatch(typeof(Builder), "GetObstacles")]
	static class Builder_GetObstacles_Patch
	{
		static void Postfix(List<GameObject> results)
		{
			$"{results.Count}".onScreen("obstacles");

			if (results.Count > 0)
				$"{results[0].name}".onScreen("obstacles1");
		}
	}

	//[HarmonyPatch(typeof(Builder), "GetOverlappedColliders")]
	static class Builder_GetOverlappedColliders_Patch
	{
		static void Postfix(List<Collider> results)
		{
			//if (BuilderStuff.isHabitatPlatform)
			//{
			//	"PLATFORM".onScreen("builder");
			//	return;
			//}

			//"NOT PLATFORM".onScreen("builder");

			//return;
			//"----------".log();
			//foreach (var collider in results)
			//	collider.name.log();

			results.Clear();
		}
	}
}