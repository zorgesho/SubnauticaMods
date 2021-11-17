using System;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

using HarmonyLib;
using UnityEngine;

using Common;
using Common.Harmony;
using Common.Reflection;

namespace HabitatPlatform
{
	// don't allow to build foundations on the platform
	[HarmonyPatch(typeof(Builder), "UpdateAllowed")]
	static class Builder_UpdateAllowed_Patch
	{
		static void Postfix(ref bool __result)
		{
			if (Builder.ghostModel?.GetComponent<BaseAddCellGhost>() is not BaseAddCellGhost cellGhost)
				return;

			if (cellGhost.cellType == Base.CellType.Foundation && cellGhost.targetBase?.GetComponentInParent<HabitatPlatform.Tag>())
				__result = false;
		}
	}

	// checking if we trying to build Constructable on the platform (and adding constructed object to the platform's Base in that case)
	[HarmonyPatch(typeof(Builder), "TryPlace")]
	static class Builder_TryPlace_Patch
	{
		static void checkPlatform(GameObject gameObject)
		{
			if (Builder.placementTarget?.GetComponentInParent<HabitatPlatform.Tag>() is HabitatPlatform.Tag tag)
				gameObject.setParent(tag.GetComponentInChildren<Base>().gameObject);
		}

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> cins, ILGenerator ilg)
		{
			var list = cins.ToList();

			// adding check there -> SubRoot componentInParent2 = Builder.placementTarget.GetComponentInParent<SubRoot>();
			int index = list.ciFindIndexForLast(
				new CIHelper.OpMatch(OpCodes.Callvirt, typeof(GameObject).method<SubRoot>("GetComponentInParent", new Type[0])),
				ci => ci.isOp(OpCodes.Brfalse));

			if (index == -1)
				return cins;

			list[index].opcode = OpCodes.Br;

			var label = list.ciDefineLabel(index + 1, ilg);

			CIHelper.ciInsert(list, index,
				OpCodes.Brtrue_S, label,
				OpCodes.Ldloc_2,
				CIHelper.emitCall<Action<GameObject>>(checkPlatform));

			return list;
		}
	}

	// helper class for ignoring some of the platform's colliders while building
	[PatchClass]
	static class CollidersPatch
	{
		static bool dirty = true;

		static Base lastBase;
		static HabitatPlatform.Tag lastPlatform;

		static readonly HashSet<Collider> ignoredColliders = new();

		public static void addIgnored(Collider collider) => ignoredColliders.Add(collider);
		public static void addIgnored(IEnumerable<Collider> colliders) => ignoredColliders.AddRange(colliders);

		public static void removeIgnored(Collider collider) => ignoredColliders.Remove(collider);

		static bool isHabitatPlatform
		{
			get
			{
				if (dirty)
				{
					lastBase = Builder.ghostModel?.GetComponent<BaseGhost>()?.targetBase;
					lastPlatform = lastBase?.gameObject.GetComponentInParent<HabitatPlatform.Tag>();
					dirty = false;
				}

				return lastPlatform != null;
			}
		}

		[HarmonyPostfix, HarmonyPatch(typeof(BaseGhost), "FindBase")]
		static void BaseGhost_FindBase_Postfix(Base __result) => dirty = (lastBase != __result);

		[HarmonyPostfix, HarmonyPatch(typeof(Builder), "GetOverlappedColliders")]
		static void Builder_GetOverlappedColliders_Postfix(List<Collider> results)
		{
			if (!isHabitatPlatform)
				return;
#if DEBUG
			if (Main.config.dbgPrintColliders)
				results.Select(coll => coll.name).ToList().onScreen("colliders");
#endif
			results.RemoveAll(coll => ignoredColliders.Contains(coll));
		}

		[HarmonyPostfix, HarmonyPatch(typeof(SceneCleaner), "Open")]
		static void SceneCleaner_Open_Postfix() => ignoredColliders.Clear(); // clear on exit to main menu
	}
}