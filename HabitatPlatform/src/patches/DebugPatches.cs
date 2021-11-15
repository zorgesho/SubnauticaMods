#if DEBUG
using HarmonyLib;
using UnityEngine;

using Common;
using Common.Harmony;

namespace HabitatPlatform
{
	[PatchClass]
	static class DebugPatches
	{
		static bool prepare() => Main.config.dbgPatches;

		static bool _platform(Base @base, out HabitatPlatform.Tag platform)
		{
			platform = @base.gameObject.getComponentInParent<HabitatPlatform.Tag>();
			return platform != null;
		}

		static void dumpTransform(string prefix, Transform tr)
		{
			string msg = $"{prefix}\tpos: {tr.position.ToString("F4")} rot: {tr.rotation.ToString("F4")}";

			msg.appendToFile("platform-debug");
			msg.logDbg();
		}

		[HarmonyPrefix, HarmonyPatch(typeof(Base), "OnProtoSerialize")]
		static void Base_OnProtoSerialize_Prefix(Base __instance)
		{
			if (_platform(__instance, out var platform))
				dumpTransform("Base.OnProtoSerialize: ", platform.transform);
		}

		[HarmonyPostfix, HarmonyPatch(typeof(Base), "OnProtoDeserialize")]
		static void Base_OnProtoDeserialize_Postfix(Base __instance)
		{
			if (_platform(__instance, out var platform))
				dumpTransform("Base.OnProtoDeserialize: ", platform.transform);
		}

		[HarmonyPrefix, HarmonyPatch(typeof(Player), "OnProtoSerialize")]
		static void Player_OnProtoSerialize_Prefix(Player __instance)
		{
			dumpTransform("Player.OnProtoSerialize: ", __instance.transform);
		}

		[HarmonyPostfix, HarmonyPatch(typeof(Player), "OnProtoDeserialize")]
		static void Player_OnProtoDeserialize_Postfix(Player __instance)
		{
			dumpTransform("Player.OnProtoDeserialize: ", __instance.transform);
		}
	}
}
#endif // DEBUG