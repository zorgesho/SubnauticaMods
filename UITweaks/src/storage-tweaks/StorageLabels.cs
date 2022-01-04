using UnityEngine;
using Common;

namespace UITweaks.StorageTweaks
{
	interface IStorageLabel
	{
		ColoredLabel label { get; }
	}

	static class StorageLabels
	{
		abstract class StorageLabel: MonoBehaviour, IStorageLabel
		{
			public ColoredLabel label => _label ??= initLabel();
			ColoredLabel _label;

			protected abstract ColoredLabel initLabel();
		}

		[StorageHandler(TechType.SmallLocker)]
		class SmallLockerLabel: StorageLabel
		{
			protected override ColoredLabel initLabel() => gameObject.getChild("Label")?.GetComponent<ColoredLabel>();
		}

		[StorageHandler(TechType.SmallStorage)]
		class SmallStorageLabel: StorageLabel
		{
			protected override ColoredLabel initLabel() => gameObject.getChild("../LidLabel/Label")?.GetComponent<ColoredLabel>();
		}

#if GAME_BZ
		[StorageHandler(TechType.SeaTruckStorageModule)]
		[StorageHandler(TechType.SeaTruckFabricatorModule)]
		class SeaTruckStorageLabel: StorageLabel
		{
			static readonly System.Collections.Generic.Dictionary<string, string> labels = new() // :((
			{
				{ "StorageContainer", "Label (2)" },
				{ "StorageContainer (1)", "Label (4)" },
				{ "StorageContainer (2)", "Label" },
				{ "StorageContainer (3)", "Label (1)" },
				{ "StorageContainer (4)", "Label (3)" }
			};

			protected override ColoredLabel initLabel()
			{
				Common.Debug.assert(labels.ContainsKey(gameObject.name), $"name not found: '{gameObject.name}'");

				if (labels.TryGetValue(gameObject.name, out string labelName))
					return gameObject.getChild($"../{labelName}")?.GetComponent<ColoredLabel>();

				return null;
			}
		}
#endif // GAME_BZ
	}
}