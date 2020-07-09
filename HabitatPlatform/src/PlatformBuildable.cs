using UnityEngine;
using SMLHelper.V2.Crafting;

using Common;
using Common.Crafting;

namespace HabitatPlatform
{
	class HabitatPlatform: CraftableObject
	{
		public class Tag: MonoBehaviour {}

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
			GameObject prefab = CraftHelper.Utils.prefabCopy(TechType.RocketBase, false);

			prefab.AddComponent<Tag>();
			prefab.AddComponent<PlatformInitializer>();

			// removing inner ladders with colliders
			GameObject ladders = prefab.getChild("Base/Triggers");
			for (int i = 1; i <= 4; i++)
				ladders.destroyChild($"innerLadder{i}");

			// removing barrels for construction bots
			GameObject platform = prefab.getChild("Base/rocketship_platform/Rocket_Geo/Rocketship_platform");
			for (int i = 1; i <= 4; i++)
				platform.destroyChild($"rocketship_platform_barrel_01_0{i}");

			// removing collider for barrels
			Object.DestroyImmediate(prefab.getChild("Base/RocketConstructorPlatform/Model/Collision").GetComponents<BoxCollider>()[1]);

			prefab.destroyChildren("AudioHolder", "AtmosphereVolume", "EndSequence", "Base_Ladder", "Stage01", "Stage02", "Stage03");

			prefab.destroyComponent<Rocket>();
			prefab.destroyComponent<PingInstance>();
			prefab.destroyComponent<RocketPreflightCheckManager>();
			prefab.destroyComponentInChildren<RocketConstructor>();

			prefab.SetActive(true);
			return prefab;
		}
	}
}