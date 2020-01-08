using UnityEngine;
using SMLHelper.V2.Crafting;

using Common;
using Common.Crafting;

namespace MiscObjects
{
	class MetalLocker: CraftableObject
	{
		class L10n: LanguageHelper
		{
			public const string ids_LockerItem		= "Locker";
			public const string ids_LockerItemDesc	= "Metal locker.";

			public static string ids_LockerInv	= "LOCKER";
			public static string ids_OpenLocker = "Open locker";
		}

		protected override TechData getTechData() => new TechData(new Ingredient(TechType.Titanium, 2));

		public override void patch()
		{
			register(L10n.ids_LockerItem, L10n.ids_LockerItemDesc);

			addToGroup(TechGroup.InteriorModules, TechCategory.InteriorModule, TechType.SmallLocker);
			unlockOnStart();
		}

		public override GameObject getGameObject()
		{
			GameObject prefab = Object.Instantiate(Resources.Load<GameObject>("Submarine/Build/submarine_locker_04"));
			GameObject model = prefab.FindChild("submarine_locker_04");

			GameObject door = prefab.FindChild("submarine_locker_03_door_01");
			door.setParent(model, false);
			door.destroyComponentInChildren<BoxCollider>();

			prefab.AddComponent<TechTag>(); // just in case
			prefab.destroyComponent<Rigidbody>();

			Constructable constructable = prefab.AddComponent<Constructable>().initDefault(model);
			constructable.allowedInBase = true;
			constructable.allowedInSub = true;
			constructable.allowedOnGround = true;
			constructable.allowedOnConstructables = true;
			constructable.forceUpright = true;
			constructable.placeDefaultDistance = 3f;

			StorageHelper.addStorageToPrefab(prefab, L10n.str("ids_OpenLocker"), L10n.str("ids_LockerInv"), 4, 8);

			return prefab;
		}
	}
}