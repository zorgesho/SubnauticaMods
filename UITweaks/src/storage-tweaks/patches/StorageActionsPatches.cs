using System;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

using HarmonyLib;
using UnityEngine;

using Common;
using Common.Harmony;
using Common.Reflection;
using Common.Configuration;

namespace UITweaks.StorageTweaks
{
	static partial class StorageActions
	{
#if DEBUG
		public static bool dbgDumpStorage = false;
		public static int dbgDumpStorageParent = 0;
#endif
		static bool actionsTweakEnabled => Main.config.storageTweaks.enabled && Main.config.storageTweaks.allInOneActions;

		public class UpdateStorages: Config.Field.IAction
		{
			public void action()
			{
				using var _ = Common.Debug.profiler("StorageActions.UpdateStorages");

				Patches.ColliderPatches.setCollidersEnabled<ColoredLabel>(!actionsTweakEnabled);
				Patches.ColliderPatches.setCollidersEnabled<PickupableStorage>(!actionsTweakEnabled);

				if (actionsTweakEnabled)
					UnityHelper.FindObjectsOfTypeAll<StorageContainer>().forEach(Patches.ensureActionHandler);
			}
		}

		[OptionalPatch, PatchClass]
		static class Patches
		{
			static bool prepare() => actionsTweakEnabled;

			static readonly Dictionary<string, Type> handlersByClassId =
				typeof(StorageActions).GetNestedTypes(ReflectionHelper.bfAll).
				Where(type => type.checkAttr<StorageHandlerAttribute>()).
				SelectMany(type => type.getAttrs<StorageHandlerAttribute>(), (type, attr) => (type, attr.classId)).
				ToDictionary(pair => pair.classId, pair => pair.type);

			static string getPrefabClassId(MonoBehaviour cmp) => cmp.GetComponentInParent<PrefabIdentifier>(true)?.ClassId ?? "";

			public static void ensureActionHandler(StorageContainer container)
			{
				if (handlersByClassId.TryGetValue(getPrefabClassId(container), out Type actionsHandler))
					container.gameObject.ensureComponent(actionsHandler);
			}

			[HarmonyPostfix, HarmonyPatch(typeof(StorageContainer), "Awake")]
			static void StorageContainer_Awake_Postfix(StorageContainer __instance)
			{
				ensureActionHandler(__instance);
#if DEBUG
				if (dbgDumpStorage)
					__instance.gameObject.dump(dumpParent: dbgDumpStorageParent);
#endif
			}

			[HarmonyTranspiler, HarmonyPatch(typeof(StorageContainer), "OnHandHover")]
			static IEnumerable<CodeInstruction> StorageContainer_OnHandHover_Transpiler(IEnumerable<CodeInstruction> cins)
			{
				static void _updateAndProcessActions(StorageContainer instance)
				{
					if (instance.GetComponent<IActionHandler>() is not IActionHandler actionHandler)
						return;

					HandReticle.main.setText(textHand: actionHandler.actions);
					actionHandler.processActions();
				}

				return cins.ciInsert(new CIHelper.MemberMatch(nameof(HandReticle.SetIcon)),
					OpCodes.Ldarg_0,
					CIHelper.emitCall<Action<StorageContainer>>(_updateAndProcessActions));
			}

			[OptionalPatch, PatchClass]
			public static class ColliderPatches
			{
				static bool prepare() => actionsTweakEnabled;

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
					if (handlersByClassId.ContainsKey(getPrefabClassId(__instance)))
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