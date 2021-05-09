using Common.Crafting;

namespace HabitatPlatform
{
	class MapRoomCameraCharge: PoolCraftableObject
	{
		protected override TechInfo getTechInfo() =>
			new (new TechInfo.Ing(TechType.MapRoomCamera)) { craftAmount = 0, linkedItems = { TechType.MapRoomCamera } };

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