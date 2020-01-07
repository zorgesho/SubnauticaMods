using System.Collections.Generic;

using UnityEngine;
using SMLHelper.V2.Crafting;

using Common;
using Common.Crafting;

namespace FloatingCargoCrate
{
	class FloatingCargoCrate: CraftableObject
	{
		public static new TechType TechType { get; private set; } = 0;

		protected override TechData getTechData() => new TechData() { craftAmount = 1, Ingredients = new List<Ingredient>
		{
			new Ingredient(TechType.Titanium, Main.config.cheapBlueprint? 3: 6),
			new Ingredient(TechType.Silicone, Main.config.cheapBlueprint? 1: 2),
			new Ingredient(TechType.AirBladder, Main.config.cheapBlueprint? 1: 2)
		}};

		public override void patch()
		{
			TechType = register("Floating cargo crate", "Big cargo crate that floats and maintains position in the water.");

			addToGroup(TechGroup.ExteriorModules, TechCategory.ExteriorOther);
			setTechTypeForUnlock(TechType.AirBladder);
		}

		public override GameObject getGameObject()
		{
			var prefab = Object.Instantiate(Resources.Load<GameObject>("WorldEntities/tools/smallstorage"));
			GameObject model = prefab.FindChild("3rd_person_model");

			string crateModelName = "Starship_cargo" + ((Main.config.cargoModelType == 2)? "_02": "");
			var prefabCargo = Resources.Load<GameObject>("WorldEntities/Doodads/Debris/Wrecks/Decoration/" + crateModelName);
			GameObject modelCargo = Object.Instantiate(prefabCargo.FindChild(crateModelName));

			modelCargo.transform.parent = model.transform;
			modelCargo.transform.localScale *= 2.1f;

			prefab.GetComponent<PrefabIdentifier>().ClassId = ClassID;

			Animator anim = model.GetComponentInChildren<Animator>();
			anim.enabled = false;

			var rigidbody = prefab.GetComponent<Rigidbody>();
			rigidbody.mass  = Main.config.crateMass;
			rigidbody.angularDrag = 1f; //default 1f
			prefab.GetComponent<Stabilizer>().uprightAccelerationStiffness = 0.3f; //default 2.0f

			prefab.destroyComponent<DeployableStorage>();
			prefab.destroyComponentInChildren<PickupableStorage>();
			prefab.destroyComponent<Pickupable>();

			prefab.destroyComponent<FPModel>();
			prefab.destroyComponent<FPModel>();

			prefab.destroyComponent<LiveMixin>();
			prefab.destroyComponentInChildren<SmallStorage>();


			StorageContainer storageContainer = prefab.GetComponentInChildren<StorageContainer>();
			storageContainer.modelSizeRadius *= 3f;
			storageContainer.hoverText = "Open crate";
			storageContainer.storageLabel = "CRATE";

			storageContainer.width =  Main.config.storageWidth;
			storageContainer.height = Main.config.storageHeight;
			storageContainer.preventDeconstructionIfNotEmpty = true;


			prefab.GetComponent<TechTag>().type = TechType;

			prefab.destroyChild("LidLabel");
			prefab.destroyChild("1st_person_model");

			model.destroyChild("floating_storage_cube_tp/Floating_storage_container_geo");
			model.destroyChild("floating_storage_cube_tp/Floating_storage_lid_geo");

			var storagePillow = model.getChild("floating_storage_cube_tp");
			storagePillow.transform.localPosition = new Vector3(0f, 1.155f, 0.18f);
			storagePillow.transform.localScale = new Vector3(3.4f, 7.0f, 8.1f);

			var collider = prefab.FindChild("StorageContainer").GetComponent<BoxCollider>();
			//collider.center = new Vector3(0.013f, 0.8f, 0.18f);
			//collider.size = new Vector3(2.4f, 3.154f, 3.08f);
			collider.center = new Vector3(0.013f, 1.23f, 0.204f);
			collider.size = new Vector3(2.4f, 2.292f, 2.854f);

			collider = prefab.FindChild("collider_main").GetComponent<BoxCollider>();
			collider.center = new Vector3(0.014f, -0.415f, 0.173f);
			collider.size = new Vector3(2.47f, 0.89f, 3.0f);

			//prefab.destroyChild("collider_main");

			prefab.GetComponent<SkyApplier>().renderers = new Renderer[] { model.GetComponentInChildren<Renderer>(), modelCargo.GetComponent<Renderer>() };


			Constructable constructable = prefab.AddComponent<Constructable>().initDefault(model, TechType);
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