using UnityEngine;

using Common;
using Common.Crafting;

namespace MiscObjects
{
	class CargoBox: PoolCraftableObject
	{
		protected override TechInfo getTechInfo() => new TechInfo(new TechInfo.Ing(TechType.Titanium, 2));

		public override void patch()
		{
			register("Cargo box", "Small cargo box.");

			addToGroup(TechGroup.InteriorModules, TechCategory.InteriorModule, TechType.Locker);
			unlockOnStart();
		}

		protected override void initPrefabPool()
		{
			addPrefabToPool($"WorldEntities/{(Mod.Consts.isGameSN? "Doodads/Debris/Wrecks/Decoration": "Alterra/Base")}/Starship_cargo");
			addPrefabToPool(TechType.SmallLocker, false);
		}

		protected override GameObject getGameObject(GameObject[] prefabs)
		{
			var prefab = prefabs[0];
			var model = prefab.getChild("Starship_cargo");

			prefab.destroyComponent<Rigidbody>();

			var constructable = PrefabUtils.initConstructable(prefab, model);
			constructable.allowedInBase = true;
			constructable.allowedInSub = true;
			constructable.allowedOnGround = true;
			constructable.allowedOnConstructables = true;
			constructable.forceUpright = true;
			constructable.placeDefaultDistance = 3f;

			Utils.addStorageToPrefab(prefab, prefabs[1]);
			PrefabUtils.initStorage(prefab, 5, 5, L10n.str(L10n.ids_OpenBox), L10n.str(L10n.ids_BoxInv));

			return prefab;
		}
	}
}