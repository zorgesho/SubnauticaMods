using UnityEngine;
using SMLHelper.V2.Crafting;

using Common;
using Common.Crafting;

namespace MiscObjects
{
	class CargoBoxBig: CraftableObject
	{
		protected override TechData getTechData() => new TechData(new Ingredient(TechType.Titanium, 4));

		public override void patch()
		{
			register("Big cargo box", "Cargo box for outside.", SpriteHelper.getSprite(nameof(CargoBox)));

			addToGroup(TechGroup.ExteriorModules, TechCategory.ExteriorOther, TechType.FarmingTray);
			unlockOnStart();
		}

		public override GameObject getGameObject()
		{
			GameObject prefab = Object.Instantiate(Resources.Load<GameObject>("WorldEntities/Doodads/Debris/Wrecks/Decoration/Starship_cargo"));
			GameObject model = prefab.FindChild("Starship_cargo");

			prefab.transform.localScale *= 2.1f;
			prefab.destroyComponent<Rigidbody>();

			Constructable constructable = prefab.AddComponent<Constructable>().initDefault(model);
			constructable.allowedOutside = true;
			constructable.allowedOnGround = true;
			constructable.allowedOnConstructables = true;
			constructable.forceUpright = true;
			constructable.placeDefaultDistance = 6f;

			StorageHelper.addStorageToPrefab(prefab, L10n.str(L10n.ids_OpenBox), L10n.str(L10n.ids_BoxInv), 8, 8);
			prefab.GetComponent<StorageContainer>().modelSizeRadius *= 3f;

			return prefab;
		}
	}
}