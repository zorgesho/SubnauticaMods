using Common.Configuration;

namespace SeamothStorageSlots
{
	class ModConfig: Config
	{
		[Field.Range(max: 8)]
		readonly int extraStorageSlotsDelta = 8;

		public int slotsDelta => extraStorageSlotsDelta < 3? 0: extraStorageSlotsDelta;
	}
}