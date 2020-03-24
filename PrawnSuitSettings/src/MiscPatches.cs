using Harmony;
using Common;

namespace PrawnSuitSettings
{
	// don't play propulsion cannon arm 'ready' animation when pointed on pickable object
	[HarmonyHelper.OptionalPatch]
	[HarmonyPatch(typeof(PropulsionCannon), "UpdateActive")]
	static class PropulsionCannon_UpdateActive_Patch
	{
		static bool Prepare() => !Main.config.readyAnimationForPropulsionCannon;

		static void Postfix(PropulsionCannon __instance)
		{
			if (Player.main.GetVehicle() != null)
				__instance.animator.SetBool("cangrab_propulsioncannon", __instance.grabbedObject != null);
		}
	}
}