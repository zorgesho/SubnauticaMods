using UnityEngine;
using SMLHelper.V2.Handlers;

using Common;
using Common.Crafting;

namespace TrfHabitatBuilder
{
	class TrfBuilder: PoolCraftableObject
	{
		public static new TechType TechType { get; private set; } = 0;

		protected override void initPrefabPool()
		{
			addPrefabToPool(TechType.Terraformer);
			addPrefabToPool(TechType.Builder, false);
		}

		protected override GameObject getGameObject(GameObject[] prefabs)
		{
			var prefab = prefabs[0];

			Terraformer trfCmp = prefab.GetComponent<Terraformer>();
			BuilderTool bldCmp = prefab.AddComponent<BuilderTool>();

			bldCmp.copyFieldsFrom(trfCmp, "rightHandIKTarget", "leftHandIKTarget", "ikAimRightArm", "ikAimLeftArm", "mainCollider", "pickupable", "useLeftAimTargetOnPlayer", "drawSound");
			bldCmp.buildSound = trfCmp.placeLoopSound;
			bldCmp.completeSound = prefabs[1].GetComponent<BuilderTool>().completeSound;

			Object.DestroyImmediate(trfCmp);

			bldCmp.animator = prefab.getChild("terraformer_anim").GetComponent<Animator>();
			bldCmp.powerConsumptionConstruct = bldCmp.powerConsumptionDeconstruct = Main.config.powerConsumption;

			prefab.AddComponent<TrfBuilderControl>();

			PrefabUtils.initVFXFab(prefab, eulerOffset: new Vector3(-10f, 90f, 0f), posOffset: new Vector3(-0.4f, 0.11f, 0f), localMaxY: 0.24f);

			return prefab;
		}

		protected override TechInfo getTechInfo() => new TechInfo
		(
			new TechInfo.Ing(TechType.ComputerChip),
			new TechInfo.Ing(TechType.WiringKit),
			new TechInfo.Ing(TechType.Battery)
		);

		public override void patch()
		{
			TechType = register();
			useTextFrom(TechType.Builder, TechType.Builder);

			addToGroup(TechGroup.Personal, TechCategory.Tools, TechType.Flare);
			addCraftingNodeTo(CraftTree.Type.Fabricator, "Personal/Tools", TechType.Flare);

			if (Main.config.removeVanillaBuilder)
			{
				CraftTreeHandler.RemoveNode(CraftTree.Type.Fabricator, "Personal", "Tools", "Builder");
				CraftDataHandler.RemoveFromGroup(TechGroup.Personal, TechCategory.Tools, TechType.Builder);
			}

			setEquipmentType(EquipmentType.Hand);
			setCraftingTime(7f);

			if (Main.config.bigInInventory)
				setItemSize(2, 2);

			unlockOnStart();
		}
	}
}