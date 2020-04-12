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
			var prefab = CraftHelper.Utils.prefabCopy("WorldEntities/Doodads/Debris/Wrecks/Decoration/Starship_cargo");
			var model = prefab.FindChild("Starship_cargo");

			prefab.transform.localScale *= 2.1f;
			prefab.destroyComponent<Rigidbody>();

			var constructable = CraftHelper.Utils.initConstructable(prefab, model);
			constructable.allowedOutside = true;
			constructable.allowedOnGround = true;
			constructable.allowedOnConstructables = true;
			constructable.forceUpright = true;
			constructable.placeDefaultDistance = 6f;

			CraftHelper.Utils.addStorageToPrefab(prefab, 8, 8, L10n.str(L10n.ids_OpenBox), L10n.str(L10n.ids_BoxInv));
			prefab.GetComponent<StorageContainer>().modelSizeRadius *= 3f;

			return prefab;
		}
	}
}