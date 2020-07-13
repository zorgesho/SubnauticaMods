using System.Linq;

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

		const float dx = 0.024f, dy = 0.030f;
		public static readonly Vector3[] engineOffsets = new[]
		{
			new Vector3( dx, -dy, 0f),
			new Vector3( dx,  dy, 0f),
			new Vector3(-dx,  dy, 0f),
			new Vector3(-dx, -dy, 0f)
		};

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

			GameObject platform = prefab.getChild("Base/rocketship_platform/Rocket_Geo/Rocketship_platform");

			// moving platform engines closer to the corners (to free up some space for building on the bottom)
			for (int i = 1; i <= 4; i++)
				platform.transform.Find($"Rocketship_platform_power_0{i}").localPosition = engineOffsets[i - 1];

			// removing barrels for construction bots
			for (int i = 1; i <= 4; i++)
				platform.destroyChild($"rocketship_platform_barrel_01_0{i}");

			// removing inner ladders with colliders
			GameObject ladders = prefab.getChild("Base/Triggers");
			for (int i = 1; i <= 4; i++)
				ladders.destroyChild($"innerLadder{i}");

			// removing collider for barrels
			Object.DestroyImmediate(prefab.getChild("Base/RocketConstructorPlatform/Model/Collision").GetComponents<BoxCollider>()[1]);

			// removing unnecessary objects and components
			prefab.destroyChildren("Base_Ladder", "Stage01", "Stage02", "Stage03", "ElevatorPosition_Top", "ElevatorPosition_Bottom");
			prefab.destroyChildren("AudioHolder", "AtmosphereVolume", "EndSequence", "SkyBaseGlass", "SkyBaseInterior");

			prefab.destroyComponent<Rocket>();
			prefab.destroyComponent<PingInstance>();
			prefab.destroyComponent<RocketPreflightCheckManager>();
			prefab.destroyComponentInChildren<RocketConstructor>();

			// changing lightmap for the bottom (because of moved engines)
			Texture2D lightmap = AssetsHelper.loadTexture("platform_lightmap");
			foreach (var rend in prefab.GetAllComponentsInChildren<Renderer>())
				foreach (var m in rend.materials)
					if (m.GetTexture("_Lightmap")?.name == "Rocketship_exterior_platform_lightmap")
						m.SetTexture("_Lightmap", lightmap);

			// cleaning up sky appliers
			var skyAppliers = prefab.GetComponents<SkyApplier>();
			skyAppliers[0].renderers = skyAppliers[0].renderers.Where(rend => rend != null).ToArray();

			for (int i = 1; i < skyAppliers.Length; i++) // keep only first one, others are for the interior
				Object.DestroyImmediate(skyAppliers[i]);

			prefab.SetActive(true);
			return prefab;
		}
	}
}