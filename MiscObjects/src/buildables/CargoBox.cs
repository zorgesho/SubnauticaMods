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
			register("Cargo box", "Small cargo box.");

			addToGroup(TechGroup.InteriorModules, TechCategory.InteriorModule, TechType.Locker);
			unlockOnStart();
		}

		public override GameObject getGameObject()
		{
			var prefab = CraftHelper.Utils.prefabCopy("WorldEntities/Doodads/Debris/Wrecks/Decoration/Starship_cargo");
			var model = prefab.FindChild("Starship_cargo");

			prefab.destroyComponent<Rigidbody>();

			var constructable = CraftHelper.Utils.initConstructable(prefab, model);
			constructable.allowedInBase = true;
			constructable.allowedInSub = true;
			constructable.allowedOnGround = true;
			constructable.allowedOnConstructables = true;
			constructable.forceUpright = true;
			constructable.placeDefaultDistance = 3f;

			CraftHelper.Utils.addStorageToPrefab(prefab, 5, 5, L10n.str(L10n.ids_OpenBox), L10n.str(L10n.ids_BoxInv));

			return prefab;
		}
	}
}