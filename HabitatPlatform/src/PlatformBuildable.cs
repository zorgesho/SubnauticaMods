using UnityEngine;
using SMLHelper.V2.Crafting;

using Common;
using Common.Crafting;

namespace HabitatPlatform
{
	class HabitatPlatform: CraftableObject
	{
		public static new TechType TechType { get; private set; } = 0;

		protected override TechData getTechData() => new TechData(new Ingredient(TechType.Titanium, 1));

		public override void patch()
		{
			TechType = register();

			addToGroup(TechGroup.Constructor, TechCategory.Constructor);
			addCraftingNodeTo(CraftTree.Type.Constructor, "");

			unlockOnStart();
		}

		public override GameObject getGameObject()
		{
			GameObject prefab = Object.Instantiate(CraftData.GetPrefabForTechType(TechType.RocketBase));

			prefab.AddComponent<PlatformInitializer>();
			prefab.AddComponent<PlatformMove>();

			//prefab.getChild("Base/RocketConstructorPlatform").SetActive(true);

			//GameObject foundation =  Object.Instantiate(CraftData.GetPrefabForTechType(TechType.BaseRoom));

			//foundation.transform.parent = prefab.getChild("Base").transform;
			//foundation.transform.localPosition = Vector3.zero;

			//prefab.dump("!!platform");

			return prefab;
		}
	}
}