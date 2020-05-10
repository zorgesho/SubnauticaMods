using UnityEngine;
using Common;

namespace OxygenRefill
{
	static class OxygenTankUtils
	{
		public static bool isTankFull(Pickupable tank)
		{
			Oxygen oxygen = tank.GetComponent<Oxygen>();
			return (oxygen.oxygenCapacity - oxygen.oxygenAvailable < 0.1f);
		}

		public static InventoryItem getTankInSlot() => Inventory.main.equipment.GetItemInSlot("Tank");

		public static bool isTankUsed(Oxygen oxygen) => Player.main.oxygenMgr.sources.Contains(oxygen);

		public static bool isTankAtSlot(TechType tankType)
		{
			InventoryItem tankItem = getTankInSlot();
			if (tankItem == null || tankItem.item.GetTechType() != tankType)
				return false;

			return !isTankFull(tankItem.item);
		}

		public static bool isTankTechType(TechType techType) =>
			techType == TechType.Tank || techType == TechType.DoubleTank || techType == TechType.PlasteelTank || techType == TechType.HighCapacityTank;

		public static void toggleTankUsage()
		{
			InventoryItem item = getTankInSlot();
			if (item == null)
				return;

			Oxygen oxygen = item.item.GetComponent<Oxygen>();

			if (isTankUsed(oxygen))
				Player.main.oxygenMgr.UnregisterSource(oxygen);
			else
				Player.main.oxygenMgr.RegisterSource(oxygen);
		}
	}

	static class ConsoleCommands
	{
		static GameObject go = null;

		class Commands: PersistentConsoleCommands
		{
			void OnConsoleCommand_toggletankusage(NotificationCenter.Notification _) => OxygenTankUtils.toggleTankUsage();

			void OnConsoleCommand_filltanks(NotificationCenter.Notification n)
			{
				float forcedCapacity = -1f;

				if (n.getArgCount() > 0)
					forcedCapacity = n.getArg<float>(0);

				if (OxygenTankUtils.getTankInSlot()?.item.GetComponent<Oxygen>() is Oxygen oxygenInSlot)
					oxygenInSlot.oxygenAvailable = forcedCapacity >= 0f? forcedCapacity: oxygenInSlot.oxygenCapacity;

				foreach (var item in Inventory.main.container)
				{
					if (item.item.GetComponent<Oxygen>() is Oxygen oxygen)
						oxygen.oxygenAvailable = forcedCapacity >= 0f? forcedCapacity: oxygen.oxygenCapacity;
				}
			}
		}

		public static void init()
		{
			if (!go)
				go = PersistentConsoleCommands.createGameObject<Commands>("OxygenRefill.ConsoleCommands");
		}
	}
}