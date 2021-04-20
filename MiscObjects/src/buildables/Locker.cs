using UnityEngine;

using Common;
using Common.Crafting;

namespace MiscObjects
{
	class MetalLocker: PoolCraftableObject
	{
		class L10n: LanguageHelper
		{
			public const string ids_LockerItem		= "Locker";
			public const string ids_LockerItemDesc	= "Metal locker.";

			public static string ids_LockerInv	= "LOCKER";
			public static string ids_OpenLocker = "Open locker";
		}

		protected override TechInfo getTechInfo() => new(new TechInfo.Ing(TechType.Titanium, 2));

		public override void patch()
		{
			register(L10n.ids_LockerItem, L10n.ids_LockerItemDesc);

			addToGroup(TechGroup.InteriorModules, TechCategory.InteriorModule, TechType.SmallLocker);
			unlockOnStart();
		}

		protected override void initPrefabPool()
		{
			addPrefabToPool("Submarine/Build/submarine_locker_04");
			addPrefabToPool(TechType.SmallLocker, false);
		}

		protected override GameObject getGameObject(GameObject[] prefabs)
		{
			var prefab = prefabs[0];
			var model = prefab.getChild("submarine_locker_04");

			var door = prefab.getChild("submarine_locker_03_door_01");
			door.setParent(model);
			door.destroyComponentInChildren<BoxCollider>();

			prefab.AddComponent<TechTag>(); // just in case
			prefab.destroyComponent<Rigidbody>();

			var constructable = PrefabUtils.initConstructable(prefab, model);
			constructable.allowedInBase = true;
			constructable.allowedInSub = true;
			constructable.allowedOnGround = true;
			constructable.allowedOnConstructables = true;
			constructable.forceUpright = true;
			constructable.placeDefaultDistance = 3f;

			Utils.addStorageToPrefab(prefab, prefabs[1]);
			PrefabUtils.initStorage(prefab, 4, 8, L10n.str("ids_OpenLocker"), L10n.str("ids_LockerInv"));

			return prefab;
		}
	}
}