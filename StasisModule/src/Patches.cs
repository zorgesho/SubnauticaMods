using System;
using System.Reflection.Emit;
using System.Collections.Generic;

using HarmonyLib;
using UnityEngine;

using Common.Harmony;

namespace StasisModule
{
	[PatchClass]
	static class Patches
	{
		// stasis spheres will ignore vehicles
		[HarmonyTranspiler, HarmonyPatch(typeof(StasisSphere), "Freeze")]
		static IEnumerable<CodeInstruction> StasisSphere_Freeze_Transpiler(IEnumerable<CodeInstruction> cins, ILGenerator ilg)
		{
			static bool _isVehicle(Rigidbody target)
			{
				if (target.gameObject.GetComponent<Vehicle>())
					return true;
#if GAME_BZ
				if (target.gameObject.GetComponent<SeaTruckSegment>())
					return true;
#endif
				return false;
			}

			var label = ilg.DefineLabel();

			return cins.ciInsert(ci => ci.isOp(OpCodes.Ret), // right after null check
				OpCodes.Ldarg_2,
				OpCodes.Ldind_Ref,
				CIHelper.emitCall<Func<Rigidbody, bool>>(_isVehicle),
				OpCodes.Brfalse, label,
				OpCodes.Ldc_I4_0,
				OpCodes.Ret,
				new CodeInstruction(OpCodes.Nop) { labels = { label } });
		}
	}

	[PatchClass]
	static class Vehicle_OnUpgradeModuleUse_Patch
	{
		static bool isStasisModule(TechType techType)
		{
			return techType == PrawnSuitStasisModule.TechType ||
#if GAME_SN
			techType == SeaMothStasisModule.TechType;
#elif GAME_BZ
			techType == SeaTruckStasisModule.TechType;
#endif
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Vehicle), "OnUpgradeModuleUse")]
#if GAME_SN
		[HarmonyPatch(typeof(SeaMoth), "OnUpgradeModuleUse")]
#elif GAME_BZ
		[HarmonyPatch(typeof(SeaTruckUpgrades), "OnUpgradeModuleUse")]
#endif
		static void Vehicle_OnUpgradeModuleUse_Postfix(MonoBehaviour __instance, TechType techType, int slotID)
		{
			if (!isStasisModule(techType))
				return;

			// didn't find a better way :(
			bool slotUsed = __instance switch
			{
				Vehicle v => useStasisModuleSlot(v, slotID),
#if GAME_BZ
				SeaTruckUpgrades s => useStasisModuleSlot(s, slotID),
#endif
				_ => false
			};

			if (slotUsed)
				new GameObject("stasis", typeof(StasisModule.StasisExplosion)).transform.position = __instance.transform.position;
		}

		static bool useStasisModuleSlot(Vehicle vehicle, int slotID)
		{
			if (!vehicle.ConsumeEnergy(Main.config.energyCost))
				return false;

			vehicle.quickSlotTimeUsed[slotID] = Time.time;
			vehicle.quickSlotCooldown[slotID] = Main.config.cooldown;
			return true;
		}
#if GAME_BZ
		static bool useStasisModuleSlot(SeaTruckUpgrades seatruck, int slotID)
		{
			if (!seatruck.ConsumeEnergy(Main.config.energyCost))
				return false;

			seatruck.quickSlotTimeUsed[slotID] = Time.time;
			seatruck.quickSlotCooldown[slotID] = Main.config.cooldown;
			return true;
		}

		// for some reason, seatruck starts cooldown for the slot even if there is not enough energy
		[HarmonyPrefix, HarmonyPatch(typeof(SeaTruckUpgrades), "OnUpgradeModuleUse")]
		static bool SeaTruckUpgrades_OnUpgradeModuleUse_Prefix(SeaTruckUpgrades __instance, TechType techType) =>
			techType != SeaTruckStasisModule.TechType || __instance.relay.GetPower() > Main.config.energyCost;
#endif
	}
}