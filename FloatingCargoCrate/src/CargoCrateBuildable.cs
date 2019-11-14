using System.Collections.Generic;

using UnityEngine;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;

using Common;

namespace FloatingCargoCrate
{
	public class FloatingCargoCrate: Buildable
	{
		internal static Buildable PatchMe()
		{
			Singleton.Patch();
			return Singleton;
		}

		internal static TechType TechTypeID => Singleton.TechType;

		private static FloatingCargoCrate Singleton { get; } = new FloatingCargoCrate();
		
		public override TechGroup GroupForPDA { get; } = TechGroup.ExteriorModules;
		public override TechCategory CategoryForPDA { get; } = TechCategory.ExteriorOther;
		public override string AssetsFolder { get; } = "FloatingCargoCrate/Assets";
		public override TechType RequiredForUnlock { get; } = TechType.AirBladder;

		public FloatingCargoCrate():	base(classId: "FloatingCargoCrate",
										friendlyName: "Floating cargo crate",
										description: "Big cargo crate that floats and maintains position in the water.")
		{
		}
	
		protected override TechData GetBlueprintRecipe()
		{
			if (Main.config.cheapBlueprint)
				return new TechData()
				{
					Ingredients = new List<Ingredient>
					{
						new Ingredient(TechType.Titanium, 3),
						new Ingredient(TechType.Silicone, 1),
						new Ingredient(TechType.AirBladder, 1)
					}
				};
			else
				return new TechData()
				{
					Ingredients = new List<Ingredient>
					{
						new Ingredient(TechType.Titanium, 6),
						new Ingredient(TechType.Silicone, 2),
						new Ingredient(TechType.AirBladder, 2),
					}
				};
		}

		
		public override GameObject GetGameObject()
		{
			var prefab = GameObject.Instantiate(Resources.Load<GameObject>("WorldEntities/tools/smallstorage"));
			GameObject model = prefab.FindChild("3rd_person_model");

			string crateModelName = "Starship_cargo" + ((Main.config.cargoModelType == 2)? "_02": "");
			var prefabCargo = Resources.Load<GameObject>("WorldEntities/Doodads/Debris/Wrecks/Decoration/" + crateModelName);
			GameObject modelCargo = GameObject.Instantiate(prefabCargo.FindChild(crateModelName));
		
			modelCargo.transform.parent = model.transform;
			modelCargo.transform.localScale *= 2.1f;

			prefab.GetComponent<PrefabIdentifier>().ClassId = this.ClassID;

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
			
			prefab.GetComponent<TechTag>().type = FloatingCargoCrate.TechTypeID;
			
		
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

			Constructable constructible = prefab.AddComponent<Constructable>();

			constructible.allowedInBase = false;
			constructible.allowedInSub = false;
			constructible.allowedOutside = true;
			constructible.allowedOnCeiling = false;
			constructible.allowedOnGround = false;
			constructible.allowedOnWall = false;
			constructible.allowedOnConstructables = false;
			constructible.controlModelState = true;
			constructible.rotationEnabled = true;
			constructible.techType = this.TechType;
			constructible.forceUpright = true;
			constructible.model = model;

			constructible.enabled = true;
			
			constructible.placeMaxDistance = 7f;
			constructible.placeMinDistance = 5f;
			constructible.placeDefaultDistance = 6f;

			prefab.AddComponent<FloatingCargoCrateControl>();

			return prefab;
		}
	}
}
