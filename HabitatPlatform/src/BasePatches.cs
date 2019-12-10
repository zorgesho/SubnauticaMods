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
}