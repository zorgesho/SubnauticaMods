using UnityEngine;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

using Common;
using Common.Crafting;

namespace DebrisRecycling
{
	class SalvageableDebrisDR: CraftableObject // just for scanner room
	{
		public static new TechType TechType { get; private set; } = 0;

		protected override TechData getTechData()  => null;
		public override GameObject getGameObject() => null;

		public override void patch() => TechType = register(L10n.ids_salvageableDebris, "", TechType.ScrapMetal);
	}


	[CraftHelper.PatchFirst]
	class ScrapMetalSmall: CraftableObject
	{
		public static new TechType TechType { get; private set; } = 0;

		protected override TechData getTechData() => null;

		public override void patch()
		{
			TechType = register(L10n.str(L10n.ids_smallScrapName), "", TechType.ScrapMetal);
			useTextFrom(descriptionFrom: TechType.ScrapMetal);
		}

		public override GameObject getGameObject()
		{
			GameObject prefab = CraftHelper.Utils.prefabCopy(TechType.Titanium);

			prefab.destroyComponent<ResourceTracker>();

			int modelType = Random.value < 0.5f? 1: 2;

			GameObject prefabMetal = CraftData.GetPrefabForTechType(TechType.ScrapMetal);
			GameObject modelMetal = Object.Instantiate(prefabMetal.getChild((modelType == 1? "Model/Metal_wreckage_03_11": "Model/Metal_wreckage_03_10")));

			prefab.destroyChild("model/Titanium_small");
			modelMetal.transform.parent = prefab.getChild("model").transform;
			modelMetal.transform.localPosition = Vector3.zero;
			modelMetal.transform.localEulerAngles = new Vector3(-90f, 0f, 0f);

			GameObject collision = prefab.getChild("collision");
			collision.destroyComponent<SphereCollider>();

			var collider = collision.AddComponent<BoxCollider>();
			collider.center = modelType == 1? new Vector3(0f, 0.032f, -0.004f): new Vector3(0.007f, 0.128f, -0.005f);
			collider.size = modelType == 1? new Vector3(0.303f, 0.073f, 0.46f): new Vector3(0.832f, 0.331f, 0.681f);

			return prefab;
		}
	}


	// in case we not using dynamic titanium recipe
	[CraftHelper.NoAutoPatch]
	class TitaniumFromScrap: CraftableObject
	{
		readonly TechType sourceTech;
		readonly int sourceCount, resultCount;

		public TitaniumFromScrap(TechType sourceTech, int sourceCount, int resultCount): base(nameof(TitaniumFromScrap) + resultCount)
		{
			this.sourceTech  = sourceTech;
			this.sourceCount = sourceCount;
			this.resultCount = resultCount;
		}

		public override GameObject getGameObject() => CraftHelper.Utils.prefabCopy(TechType.Titanium);

		protected override TechData getTechData()
		{
			var techData = new TechData(new Ingredient(sourceTech, sourceCount));
			techData.LinkedItems.add(TechType.Titanium, resultCount);

			return techData;
		}

		public override void patch()
		{
			if (Main.config.craftConfig.dynamicTitaniumRecipe)
				return;

			initNodes();

			register($"Titanium (x{resultCount})", "", TechType.Titanium);
			useTextFrom(descriptionFrom: TechType.Titanium);

			setCraftingTime(0.7f * resultCount);
			addCraftingNodeTo(CraftTree.Type.Fabricator, "Resources/Titanium");
			unlockOnStart();
		}

		static bool nodesInited = false;

		static void initNodes()
		{
			if (nodesInited || !(nodesInited = true))
				return;

			CraftTreeHandler.RemoveNode(CraftTree.Type.Fabricator, "Resources", "BasicMaterials", "Titanium");
			CraftTreeHandler.AddTabNode(CraftTree.Type.Fabricator, "Titanium", "Titanium", SpriteManager.Get(TechType.Titanium), "Resources");
			CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, TechType.Titanium, "Resources", "Titanium");
		}
	}

	// for CraftHelper patchAll
	class T1: TitaniumFromScrap  { public T1(): base(ScrapMetalSmall.TechType, 1, 1 * Main.config.craftConfig.titaniumPerSmallScrap) {} }
	class T5: TitaniumFromScrap  { public T5(): base(ScrapMetalSmall.TechType, 5, 5 * Main.config.craftConfig.titaniumPerSmallScrap) {} }
	//class T10: TitaniumFromScrap { public T10(): base(ScrapMetalSmall.TechType, 10, 10) {} }
	//class T20: TitaniumFromScrap { public T20(): base(TechType.ScrapMetal, 5, 20) {} }
}