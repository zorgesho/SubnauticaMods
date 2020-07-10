using UnityEngine;
using SMLHelper.V2.Crafting;

using Common.Crafting;

namespace HabitatPlatform
{
	class MapRoomCameraCharge: CraftableObject
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

			register("Charge camera drone", "Charge and repair camera drone.", TechType.MapRoomCamera);

			addCraftingNodeTo(CraftTree.Type.MapRoom, "", TechType.MapRoomCamera);
			setTechTypeForUnlock(TechType.MapRoomCamera);
		}

		public override GameObject getGameObject() => CraftHelper.Utils.prefabCopy(TechType.MapRoomCamera);
	}
}