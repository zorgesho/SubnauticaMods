using UnityEngine;

using Common;
using Common.Crafting;

namespace FloatingCargoCrate
{
	class FloatingCargoCrate: PoolCraftableObject
	{
		public static new TechType TechType { get; private set; } = 0;

		protected override TechInfo getTechInfo() => new TechInfo
		(
			new TechInfo.Ing(TechType.Titanium, Main.config.cheapBlueprint? 3: 6),
			new TechInfo.Ing(TechType.Silicone, Main.config.cheapBlueprint? 1: 2),
			new TechInfo.Ing(TechType.AirBladder, Main.config.cheapBlueprint? 1: 2)
		);

		public override void patch()
		{
			TechType = register(L10n.ids_crateName, L10n.ids_crateDesc);

			addToGroup(TechGroup.ExteriorModules, TechCategory.ExteriorOther);
			setTechTypeForUnlock(TechType.AirBladder);
		}

		protected override void initPrefabPool()
		{
			addPrefabToPool(TechType.SmallStorage);
			addPrefabToPool("WorldEntities/Doodads/Debris/Wrecks/Decoration/" + Main.config.crateModelName, false);
		}

		protected override GameObject getGameObject(GameObject[] prefabs)
		{
			var prefab = prefabs[0];
			var model = prefab.getChild("3rd_person_model");

			var modelCargo = Object.Instantiate(prefabs[1].getChild(Main.config.crateModelName), model.transform);
			modelCargo.transform.localScale *= 2.1f;

			model.GetComponentInChildren<Animator>().enabled = false;

			var rigidbody = prefab.GetComponent<Rigidbody>();
			rigidbody.mass = Main.config.crateMass;
			rigidbody.angularDrag = 1f; //default 1f
			prefab.GetComponent<Stabilizer>().uprightAccelerationStiffness = 0.3f; //default 2.0f

			prefab.destroyComponent<DeployableStorage>();
			prefab.destroyComponentInChildren<PickupableStorage>();
			prefab.destroyComponent<Pickupable>();

			prefab.destroyComponent<FPModel>();
			prefab.destroyComponent<FPModel>();

			prefab.destroyComponent<LiveMixin>();
			prefab.destroyComponentInChildren<SmallStorage>();


			var storageContainer = PrefabUtils.initStorage(prefab, Main.config.storageWidth, Main.config.storageHeight, L10n.str(L10n.ids_hoverText), L10n.str(L10n.ids_storageLabel));
			storageContainer.modelSizeRadius *= 3f;
			storageContainer.enabled = false; // disable until fully constructed


			prefab.destroyChildren("LidLabel", "1st_person_model");

			var storagePillow = model.getChild("floating_storage_cube_tp");
			storagePillow.destroyChildren("Floating_storage_container_geo", "Floating_storage_lid_geo");
			storagePillow.transform.localPosition = new Vector3(0f, 1.155f, 0.18f);
			storagePillow.transform.localScale = new Vector3(3.4f, 7.0f, 8.1f);

			var collider = prefab.getChild("StorageContainer").GetComponent<BoxCollider>();
			collider.center = new Vector3(0.013f, 1.23f, 0.204f);
			collider.size = new Vector3(2.4f, 2.292f, 2.854f);

			collider = prefab.getChild("collider_main").GetComponent<BoxCollider>();
			collider.center = new Vector3(0.014f, -0.415f, 0.173f);
			collider.size = new Vector3(2.47f, 0.89f, 3.0f);

			prefab.GetComponent<SkyApplier>().renderers = new[] { model.GetComponentInChildren<Renderer>(), modelCargo.GetComponent<Renderer>() };


			var constructable = PrefabUtils.initConstructable(prefab, model);
			constructable.allowedOutside = true;
			constructable.forceUpright = true;

			constructable.placeMaxDistance = 7f;
			constructable.placeMinDistance = 5f;
			constructable.placeDefaultDistance = 6f;

			prefab.AddComponent<FloatingCargoCrateControl>();

			return prefab;
		}
	}
}