using System;
using System.Reflection.Emit;
using System.Collections.Generic;

using HarmonyLib;
using UnityEngine;

using Common;
using Common.Harmony;
using Common.Configuration;

namespace UITweaks.StorageTweaks
{
	static partial class StorageActions
	{
		static bool tweakEnabled => Main.config.storageTweaks.enabled && Main.config.storageTweaks.allInOneActions;

		public class UpdateStorages: Config.Field.IAction
		{
			public void action()
			{
				using var _ = Common.Debug.profiler("StorageActions.UpdateStorages");

				Patches.ColliderPatches.setCollidersEnabled<ColoredLabel>(!tweakEnabled);
				Patches.ColliderPatches.setCollidersEnabled<PickupableStorage>(!tweakEnabled);

				if (tweakEnabled)
					UnityHelper.FindObjectsOfTypeAll<StorageContainer>().forEach(StorageHandlerProcessor.ensureHandlers);
			}
		}

		[OptionalPatch, PatchClass]
		static class Patches
		{
			static bool prepare() => tweakEnabled;

			[HarmonyTranspiler, HarmonyPatch(typeof(StorageContainer), "OnHandHover")]
			static IEnumerable<CodeInstruction> StorageContainer_OnHandHover_Transpiler(IEnumerable<CodeInstruction> cins)
			{
				static void _updateAndProcessActions(StorageContainer instance)
				{
					if (instance.GetComponent<IStorageActions>() is not IStorageActions storageActions)
						return;

					HandReticle.main.setText(textHand: storageActions.actions);
					storageActions.processActions();
				}

				return cins.ciInsert(new CIHelper.MemberMatch(nameof(HandReticle.SetIcon)),
					OpCodes.Ldarg_0,
					CIHelper.emitCall<Action<StorageContainer>>(_updateAndProcessActions));
			}

			[OptionalPatch, PatchClass]
			public static class ColliderPatches
			{
				static bool prepare() => tweakEnabled;

				static void setColliderEnabled(MonoBehaviour cmp, bool enabled)
				{
					if (cmp?.GetComponent<BoxCollider>() is BoxCollider collider)
						collider.enabled = enabled;
				}

				public static void setCollidersEnabled<T>(bool enabled) where T: MonoBehaviour
				{
					UnityHelper.FindObjectsOfTypeAll<T>().forEach(cmp => setColliderEnabled(cmp, enabled));
				}

				[HarmonyPostfix, HarmonyPatch(typeof(ColoredLabel), "OnEnable")]
				static void ColoredLabel_OnEnable_Postfix(ColoredLabel __instance)
				{
					if (StorageHandlerProcessor.hasHandlers(Utils.getPrefabClassId(__instance)))
						setColliderEnabled(__instance, false);
				}

				[HarmonyPrefix, HarmonyPriority(Priority.Low)]
				[HarmonyPatch(typeof(PickupableStorage), "OnHandHover")]
				static bool PickupableStorage_OnHandHover_Prefix(PickupableStorage __instance)
				{
					setColliderEnabled(__instance, false);
					HandReticle.main.setText(textHand: "");

					return false;
				}
			}
		}
	}
}