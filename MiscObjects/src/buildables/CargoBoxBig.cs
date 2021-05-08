using UnityEngine;

using Common;
using Common.Crafting;

namespace MiscObjects
{
	class CargoBoxBig: PoolCraftableObject
	{
		protected override TechInfo getTechInfo() => new (new TechInfo.Ing(TechType.Titanium, 4));

		public override void patch()
		{
			register("Big cargo box", "Cargo box for outside.", SpriteHelper.getSprite(nameof(CargoBox)));
#if GAME_SN
			addToGroup(TechGroup.ExteriorModules, TechCategory.ExteriorOther, TechType.FarmingTray);
#elif GAME_BZ
			addToGroup(TechGroup.ExteriorModules, TechCategory.ExteriorModule, TechType.FarmingTray);
#endif
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

			prefab.transform.localScale *= 2.1f;
			prefab.destroyComponent<Rigidbody>();

			var constructable = PrefabUtils.initConstructable(prefab, model);
			constructable.allowedOutside = true;
			constructable.allowedOnGround = true;
			constructable.allowedOnConstructables = true;
			constructable.forceUpright = true;
			constructable.placeDefaultDistance = 6f;

			Utils.addStorageToPrefab(prefab, prefabs[1]);
			PrefabUtils.initStorage(prefab, 8, 8, L10n.str(L10n.ids_OpenBox), L10n.str(L10n.ids_BoxInv));
#if GAME_SN
			prefab.GetComponent<StorageContainer>().modelSizeRadius *= 3f; // TODO: ???
#endif
			return prefab;
		}
	}
}