using System.Collections.Generic;

using UnityEngine;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

using Common.Crafting;

namespace OxygenRefill
{
	[CraftHelper.PatchFirst]
	class OxygenRefillStation: CraftableObject
	{
		public static ModCraftTreeRoot treeRootNode { get; private set; } = null;
		CraftTree.Type treeType;

		protected override TechData getTechData() => new TechData() { craftAmount = 1, Ingredients = new List<Ingredient>
		{
			new Ingredient(TechType.AdvancedWiringKit, 1),
		}};

		public override void patch()
		{
			register(L10n.ids_OxygenStation, L10n.ids_OxygenStationDesc, TechType.Workbench);
			treeRootNode = CraftTreeHandler.CreateCustomCraftTreeAndType(ClassID, out treeType);

			addToGroup(TechGroup.InteriorModules, TechCategory.InteriorModule, TechType.Workbench);
			unlockOnStart();
		}

		public override GameObject getGameObject()
		{
			GameObject prefab = Object.Instantiate(CraftData.GetPrefabForTechType(TechType.Workbench));

			GhostCrafter crafter = prefab.GetComponent<Workbench>();
			crafter.craftTree = treeType;
			crafter.handOverText = L10n.str("ids_UseStation");

			prefab.GetComponentInChildren<SkinnedMeshRenderer>().material.color = new Color(0, 1, 1, 1);

			return prefab;
		}
	}

	[CraftHelper.NoAutoPatch]
	abstract class TankRefill: CraftableObject
	{
		// used for fill tank after creating at refilling station (we can't just change it in prefab)
		class RefillOxygen: MonoBehaviour
		{
			void Awake()
			{
				float capacity = Main.config.getTankCapacity(gameObject);

				if (capacity > 0)
				{
					Oxygen oxygen = gameObject.GetComponent<Oxygen>();
					oxygen.oxygenAvailable = oxygen.oxygenCapacity = capacity;
				}

				Destroy(this);
			}
		}

		readonly float craftingTime;
		readonly TechType tankType;

		public TankRefill(TechType _tankType, float _craftingTime): base(_tankType.AsString() + "_Refill")
		{
			tankType = _tankType;
			craftingTime = _craftingTime;
		}

		public override GameObject getGameObject()
		{
			GameObject prefab = Object.Instantiate(CraftData.GetPrefabForTechType(tankType));
			prefab.AddComponent<RefillOxygen>();

			return prefab; // using this as exact prefab, so no need in LinkedItems
		}

		protected override TechData getTechData() => new TechData(new Ingredient(tankType, 1)) { craftAmount = 1 };

		public override void patch()
		{
			register(L10n.ids_RefillOxygen, L10n.ids_RefillOxygenDesc, tankType);

			addCraftingNodeTo(OxygenRefillStation.treeRootNode);
			setTechTypeForUnlock(tankType);
			setCraftingTime(craftingTime);
			useExactPrefab();
		}
	}

	// for CraftHelper patchAll
	class T1: TankRefill { public T1(): base(TechType.Tank, 5f) {} }
	class T2: TankRefill { public T2(): base(TechType.DoubleTank, 10f) {} }
	class T3: TankRefill { public T3(): base(TechType.PlasteelTank, 10f) {} }
	class T4: TankRefill { public T4(): base(TechType.HighCapacityTank, 15f) {} }
}