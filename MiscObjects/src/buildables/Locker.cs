using UnityEngine;

using SMLHelper.V2.Crafting;

using Common;
using Common.Crafting;

namespace MiscObjects
{
	class MetalLocker: CraftableObject
	{
		protected override TechData getTechData() => new TechData(new Ingredient(TechType.Titanium, 2));

		public override void patch()
		{
			register("Locker", "Metal locker.", AssetsHelper.loadSprite(ClassID));

			setPDAGroup(TechGroup.InteriorModules, TechCategory.InteriorModule, TechType.SmallLocker);
			unlockOnStart();
		}

		protected override GameObject getGameObject()
		{
			GameObject prefab = Object.Instantiate(Resources.Load<GameObject>("Submarine/Build/submarine_locker_04"));
			GameObject model = prefab.FindChild("submarine_locker_04");

			GameObject door = prefab.FindChild("submarine_locker_03_door_01");
			door.setParent(model, false);
			door.destroyComponentInChildren<BoxCollider>();

			prefab.AddComponent<TechTag>().type = TechType;
			prefab.GetComponent<PrefabIdentifier>().ClassId = ClassID;

			prefab.destroyComponent<Rigidbody>();

			Constructable constructable = prefab.AddComponent<Constructable>().initDefault(model, TechType);
			constructable.allowedInBase = true;
			constructable.allowedInSub = true;
			constructable.allowedOnGround = true;
			constructable.allowedOnConstructables = true;
			constructable.forceUpright = true;
			constructable.placeDefaultDistance = 3f;
			
			StorageHelper.addStorageToPrefab(prefab,  "Open locker", "LOCKER", 4, 8);

			return prefab;
		}
	}
}