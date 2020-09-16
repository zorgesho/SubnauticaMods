using UnityEngine;
using SMLHelper.V2.Crafting;

using Common;
using Common.Crafting;

namespace MiscObjects
{
	class LuggageBag: PoolCraftableObject
	{
		protected override TechData getTechData() => new TechData
		(
			new Ingredient(TechType.FiberMesh, 2),
			new Ingredient(TechType.Silicone, 1)
		)	{ craftAmount = 1};

		protected override void initPrefabPool() => addPrefabToPool("WorldEntities/Doodads/Debris/Wrecks/Decoration/docking_luggage_01_bag4");

		protected override GameObject getGameObject(GameObject prefab)
		{
			prefab.getChild("model/docking_luggage_01_bag4").AddComponent<VFXFabricating>();
			PrefabUtils.initVFXFab(prefab, new Vector3(0f, 0f, 0.04f), new Vector3(-90f, 0f, 0f), -0.2f, 0.7f, 0.8f);

			return prefab;
		}

		public override void patch()
		{
			register(TechType.LuggageBag);

			addToGroup(TechGroup.Personal, TechCategory.Equipment);
			addCraftingNodeTo(CraftTree.Type.Fabricator, "Personal/Equipment");
			setTechTypeForUnlock(TechType.LuggageBag);
		}
	}
}