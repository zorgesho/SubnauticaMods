using System.Linq;

using HarmonyLib;
using UnityEngine;

using Common;
using Common.Harmony;

namespace StasisTorpedo
{
	[PatchClass]
	static class Patches
	{
		[HarmonyPostfix, HarmonyPatch(typeof(Vehicle), "Awake")]
		static void Vehicle_Awake_Postfix(Vehicle __instance)
		{																																		$"Vehicle.Awake: {__instance.gameObject.name}".logDbg();
			if (StasisTorpedo.torpedoType == null)
				StasisTorpedo.initPrefab(__instance.torpedoTypes.FirstOrDefault(type => type.techType == TechType.GasTorpedo)?.prefab);

			__instance.torpedoTypes = __instance.torpedoTypes.append(new[] { StasisTorpedo.torpedoType });
		}

		// TODO transpiler
		[HarmonyPrefix, HarmonyPatch(typeof(SeaMoth), "OpenTorpedoStorage")]
		static bool OpenTorpedoStorage(SeaMoth __instance, Transform useTransform)
		{
			if (__instance.modules.GetCount(TechType.SeamothTorpedoModule) > 0)
			{
				Inventory.main.ClearUsedStorage();
				int num = __instance.slotIDs.Length;
				for (int i = 0; i < num; i++)
				{
					ItemsContainer storageInSlot = __instance.GetStorageInSlot(i, TechType.SeamothTorpedoModule);
					storageInSlot?.SetAllowedTechTypes(new[] { TechType.GasTorpedo, TechType.WhirlpoolTorpedo, StasisTorpedo.TechType }); // TODO
					Inventory.main.SetUsedStorage(storageInSlot, append: true);
				}
				Player.main.GetPDA().Open(PDATab.Inventory, useTransform);
			}

			return false;
		}

		// TODO transpiler
		[HarmonyPrefix, HarmonyPatch(typeof(ExosuitTorpedoArm), "OpenTorpedoStorageExternal")]
		static bool OpenTorpedoStorageExternal(ExosuitTorpedoArm __instance, Transform useTransform)
		{
			if (__instance.container != null)
			{
				__instance.container.SetAllowedTechTypes(new[] { TechType.GasTorpedo, TechType.WhirlpoolTorpedo, StasisTorpedo.TechType }); // TODO
				Inventory.main.SetUsedStorage(__instance.container);
				Player.main.GetPDA().Open(PDATab.Inventory, useTransform);
			}

			return false;
		}
	}
}