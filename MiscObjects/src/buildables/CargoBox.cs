using UnityEngine;

using SMLHelper.V2.Crafting;

using Common;
using Common.Crafting;

namespace MiscObjects
{
	class CargoBox: CraftableObject
	{
		protected override TechData getTechData() => new TechData(new Ingredient(TechType.Titanium, 2));

		public override void patch()
		{
			register("Cargo box", "Small cargo box.", AssetsHelper.loadSprite(ClassID));

			addToGroup(TechGroup.InteriorModules, TechCategory.InteriorModule, TechType.Locker);
			unlockOnStart();
		}

		public override GameObject getGameObject()
		{
			GameObject  prefab = Object.Instantiate(Resources.Load<GameObject>("WorldEntities/Doodads/Debris/Wrecks/Decoration/Starship_cargo"));
			GameObject model = prefab.FindChild("Starship_cargo");
				
			prefab.getOrAddComponent<TechTag>().type = TechType;
			prefab.GetComponent<PrefabIdentifier>().ClassId = ClassID;

			prefab.destroyComponent<Rigidbody>();

			Constructable constructable = prefab.AddComponent<Constructable>().initDefault(model, TechType);
			constructable.allowedInBase = true;
			constructable.allowedInSub = true;
			constructable.allowedOnGround = true;
			constructable.allowedOnConstructables = true;
			constructable.forceUpright = true;
			constructable.placeDefaultDistance = 3f;
			
			StorageHelper.addStorageToPrefab(prefab,  "Open box", "BOX", 5, 5);

			return prefab;
		}
	}
}