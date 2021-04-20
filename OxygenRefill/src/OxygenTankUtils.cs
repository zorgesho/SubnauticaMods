using Common;

namespace OxygenRefill
{
	static class OxygenTankUtils
	{
		public static bool isTankFull(Pickupable tank)
		{
			var oxygen = tank.GetComponent<Oxygen>();
			return (oxygen.oxygenCapacity - oxygen.oxygenAvailable < 0.1f);
		}

		public static InventoryItem getTankInSlot() => Inventory.main.equipment.GetItemInSlot("Tank");

		public static bool isTankUsed(Oxygen oxygen) => Player.main.oxygenMgr.sources.Contains(oxygen);

		// used for crafting, doesn't take into account full tank in slot
		public static bool isTankAtSlot(TechType tankType)
		{
			InventoryItem tankItem = getTankInSlot();
			return tankItem?.item.GetTechType() == tankType && !isTankFull(tankItem.item);
		}

		public static bool isTankTechType(TechType techType) =>
			techType == TechType.Tank || techType == TechType.DoubleTank || techType == TechType.PlasteelTank || techType == TechType.HighCapacityTank;

		public static void toggleTankUsage()
		{
			if (getTankInSlot() is not InventoryItem item)
				return;

			var oxygen = item.item.GetComponent<Oxygen>();

			if (isTankUsed(oxygen))
				Player.main.oxygenMgr.UnregisterSource(oxygen);
			else
				Player.main.oxygenMgr.RegisterSource(oxygen);
		}
	}

	class ConsoleCommands: PersistentConsoleCommands
	{
		public void toggletankusage() => OxygenTankUtils.toggleTankUsage();

		public void filltanks(float forcedCapacity = -1f)
		{
			void _fill(Oxygen ox) { if (ox) ox.oxygenAvailable = forcedCapacity >= 0f? forcedCapacity: ox.oxygenCapacity; }

			_fill(OxygenTankUtils.getTankInSlot()?.item.GetComponent<Oxygen>());

			Inventory.main.container.ForEach(item => _fill(item.item.GetComponent<Oxygen>()));
		}
	}
}