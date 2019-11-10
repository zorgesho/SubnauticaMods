using Harmony;

namespace PrawnSuitSettings
{
	// don't play propulsion cannon arm 'ready' animation when pointed on pickable object
	[HarmonyPatch(typeof(PropulsionCannon), "UpdateActive")]
	static class PropulsionCannon_UpdateActive_Patch
	{
		static void Postfix(PropulsionCannon __instance)
		{
			if (!Main.config.readyAnimationForPropulsionCannon && Player.main.GetVehicle() != null)
				__instance.animator.SetBool("cangrab_propulsioncannon", __instance.grabbedObject != null);
		}
	}
}