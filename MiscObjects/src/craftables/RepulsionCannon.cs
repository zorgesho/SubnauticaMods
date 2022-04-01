#if GAME_BZ
using HarmonyLib;
using UnityEngine;

using Common;
using Common.Crafting;

namespace MiscObjects
{
	class RepulsionCannon: PoolCraftableObject
	{
		protected override TechInfo getTechInfo() => new
		(
			new (TechType.PropulsionCannon),
			new (TechType.ComputerChip),
			new (TechType.Magnetite, 2)
		);

		protected override void initPrefabPool() => addPrefabToPool("WorldEntities/Tools/RepulsionCannon.prefab");

		protected override GameObject getGameObject(GameObject prefab)
		{
			PrefabUtils.initVFXFab(prefab, localMinY: -.15f, localMaxY: .2f);
			return prefab;
		}

		public override void patch()
		{
#pragma warning disable CS0612 // obsolete
			register(TechType.RepulsionCannon);
#pragma warning restore CS0612

			addToGroup(TechGroup.Workbench, TechCategory.Workbench);
			addCraftingNodeTo(CraftTree.Type.Workbench, "");
			setTechTypeForUnlock(TechType.PropulsionCannon);
		}

		// TechType.RepulsionCannon is already exists, so we need to do it this way
		[HarmonyPatch(typeof(Language), "LoadLanguageFile")]
		static class StringsPatch
		{
			[HarmonyPriority(Priority.Low)]
			static void Postfix()
			{
				Language.main.strings["RepulsionCannon"] = "Repulsion cannon";
				Language.main.strings["Tooltip_RepulsionCannon"] = "Applies percussive force to entities in range.";
			}
		}
	}
}
#endif // GAME_BZ