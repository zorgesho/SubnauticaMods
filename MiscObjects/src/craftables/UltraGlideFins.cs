#if GAME_BZ
using HarmonyLib;

using Common.Harmony;
using Common.Crafting;

#if DEBUG
using System.Collections;
using Common;
#endif

namespace MiscObjects
{
	class UltraGlideFins: PoolCraftableObject
	{
		protected override TechInfo getTechInfo() => new
		(
			new (TechType.Fins),
			new (TechType.Silicone, 2),
			new (TechType.Titanium),
			new (TechType.Lithium)
		);

		protected override void initPrefabPool() => addPrefabToPool("WorldEntities/Tools/UltraGlideFins.prefab");

		public override void patch()
		{
			register(TechType.UltraGlideFins);

			addToGroup(TechGroup.Workbench, TechCategory.Workbench);
			addCraftingNodeTo(CraftTree.Type.Workbench, "", TechType.SwimChargeFins);
			setTechTypeForUnlock(TechType.SwimChargeFins);
		}

		[OptionalPatch, PatchClass]
		static class SwimSpeedPatch
		{
			static bool prepare() => Main.config.swimSpeedPatch;
#if DEBUG
			static bool inited = false;

			static void pinSpeed(UnderwaterMotor instance)
			{
				instance.StartCoroutine(_pinSpeed());

				IEnumerator _pinSpeed()
				{
					while (true)
					{
						$"{instance.rb.velocity.magnitude}".onScreen("player speed");
						yield return null;
					}
				}
			}
#endif
			[HarmonyPostfix, HarmonyPatch(typeof(UnderwaterMotor), "UpdateMove")]
			static void UnderwaterMotor_UpdateMove_Postfix(UnderwaterMotor __instance)
			{
#if DEBUG
				if (!inited && (inited = true))
					pinSpeed(__instance);
#endif
				if (__instance.movementInputDirection.z > 0f)
					__instance.rb.drag *= 0.5f;
			}

			[HarmonyPostfix, HarmonyPatch(typeof(UnderwaterMotor), "AlterMaxSpeed")]
			static void UnderwaterMotor_AlterMaxSpeed_Postfix(UnderwaterMotor __instance)
			{
				if (Player.main.motorMode != Player.MotorMode.Dive)
					return;

				__instance.waterAcceleration = Inventory.main.equipment.GetTechTypeInSlot("Foots") switch
				{
					TechType.Fins => Player.main.playerController.swimWaterAcceleration * 1.2f,
					TechType.UltraGlideFins => Player.main.playerController.swimWaterAcceleration * 1.5f,
					_ => __instance.waterAcceleration
				};
			}
		}
	}
}
#endif // GAME_BZ