using Harmony;
using Common;

namespace RemoteTorpedoDetonator
{
	[HarmonyPatch(typeof(SeamothTorpedo), "Awake")]
	static class SeamothTorpedo_Awake_Patch
	{
		static bool Prepare() => (Main.config.torpedoSpeed != 10f || !Main.config.homingTorpedoes); // non-default values, need to patch

		static void Postfix(SeamothTorpedo __instance)
		{
			__instance.homingTorpedo = Main.config.homingTorpedoes;
			__instance.speed = Main.config.torpedoSpeed;
		}
	}

	[HarmonyPatch(typeof(Vehicle), "OnUpgradeModuleChange")]
	static class Vehicle_OnUpgradeModuleChange_Patch
	{
		static void Postfix(Vehicle __instance, TechType techType)
		{
			if (techType == TorpedoDetonatorModule.TechType)
				__instance.gameObject.getOrAddComponent<TorpedoDetonatorControl>().checkEnabled();
		}
	}

	// patching in main.cs, one patch used in two places
	static class Vehicle_OnUpgradeModuleUse_Patch
	{
		static void Postfix(Vehicle __instance, TechType techType, int slotID)
		{
			if (techType == TorpedoDetonatorModule.TechType)
				__instance.gameObject.GetComponent<TorpedoDetonatorControl>()?.detonateTorpedoes();

			if (techType == TechType.SeamothTorpedoModule && __instance.quickSlotCooldown[slotID] > Main.config.torpedoCooldown)
				__instance.quickSlotCooldown[slotID] = Main.config.torpedoCooldown;
		}
	}
}