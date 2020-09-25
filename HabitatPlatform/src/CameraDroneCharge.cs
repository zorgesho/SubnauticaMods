using SMLHelper.V2.Crafting;
using Common.Crafting;

namespace HabitatPlatform
{
	class MapRoomCameraCharge: PoolCraftableObject
	{
		protected override TechData getTechData()
		{
			var techData = new TechData(new Ingredient(TechType.MapRoomCamera, 1));
			techData.LinkedItems.Add(TechType.MapRoomCamera);

			return techData;
		}

		public override void patch()
		{
			if (!Main.config.chargeCameras)
				return;

			register(L10n.ids_ChargeCamera, L10n.ids_ChargeCameraDesc, TechType.MapRoomCamera);

			addCraftingNodeTo(CraftTree.Type.MapRoom, "", TechType.MapRoomCamera);
			setTechTypeForUnlock(TechType.MapRoomCamera);
		}

		protected override void initPrefabPool() => addPrefabToPool(TechType.MapRoomCamera);
	}
}