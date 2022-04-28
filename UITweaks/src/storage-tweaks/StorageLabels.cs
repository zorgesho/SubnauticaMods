using UnityEngine;
using Common;

#if GAME_BZ
using System.Collections.Generic;
#endif

namespace UITweaks.StorageTweaks
{
	interface IStorageLabel
	{
		ColoredLabel label { get; }
		StorageContainer owner { get; }
	}

	interface IStorageLabelInfo
	{
		string type { get; }
		Vector2 size { get; }
		int maxCharCount { get; }
		int maxLineCount { get; }
	}

	static class StorageLabels
	{
		public static IStorageLabel[] allLabels => Object.FindObjectsOfType<StorageLabel>();

		abstract class StorageLabel: MonoBehaviour, IStorageLabel
		{
			public ColoredLabel label => _label ??= initLabel();
			ColoredLabel _label;

			public StorageContainer owner => _owner ??= gameObject.GetComponent<StorageContainer>();
			StorageContainer _owner;

			protected abstract ColoredLabel initLabel();
		}

		[StorageHandler(TechType.SmallLocker)]
		class SmallLockerLabel: StorageLabel, IStorageLabelInfo
		{
			public string type => "SmallLocker";
			public Vector2 size => new (420f, 150f);
			public int maxCharCount => 80;
			public int maxLineCount => 4;

			protected override ColoredLabel initLabel() => gameObject.getChild("Label")?.GetComponent<ColoredLabel>();
		}

		[StorageHandler(TechType.SmallStorage)]
		class SmallStorageLabel: StorageLabel, IStorageLabelInfo
		{
			public string type => "SmallStorage";
			public Vector2 size => new (270f, 150f);
			public int maxCharCount => 70;
			public int maxLineCount => 3;

			protected override ColoredLabel initLabel() => gameObject.getChild("../LidLabel/Label")?.GetComponent<ColoredLabel>();
		}

#if GAME_BZ
		[StorageHandler(TechType.SeaTruckStorageModule)]
		[StorageHandler(TechType.SeaTruckFabricatorModule)]
		class SeaTruckStorageLabel: StorageLabel, IStorageLabelInfo
		{
			enum StorageType { Unknown, Vertical, HorizontalBig, HorizontalSmall }
			record LabelInfo(Vector2 size, int maxCharCount, int maxLineCount);

			static readonly Dictionary<StorageType, LabelInfo> labelInfos = new()
			{
				{ StorageType.Vertical,		   new (new (0f, 0f), 0, 0) },
				{ StorageType.HorizontalBig,   new (new (0f, 0f), 0, 0) },
				{ StorageType.HorizontalSmall, new (new (0f, 0f), 0, 0) }
			};

			static readonly Dictionary<string, (string name, StorageType type)> labels = new()
			{
				{ "StorageContainer",	  ("Label (2)", StorageType.Vertical) },
				{ "StorageContainer (1)", ("Label (4)", StorageType.HorizontalBig) },
				{ "StorageContainer (2)", ("Label",		StorageType.HorizontalSmall) },
				{ "StorageContainer (3)", ("Label (1)", StorageType.HorizontalSmall) },
				{ "StorageContainer (4)", ("Label (3)", StorageType.Vertical) }
			};

			StorageType storageType;
			LabelInfo labelInfo;

			public string type => storageType.ToString();
			public Vector2 size => labelInfo.size;
			public int maxCharCount => labelInfo.maxCharCount;
			public int maxLineCount => labelInfo.maxLineCount;

			protected override ColoredLabel initLabel()
			{
				Common.Debug.assert(labels.ContainsKey(gameObject.name), $"name not found: '{gameObject.name}'");

				if (!labels.TryGetValue(gameObject.name, out var tuple))
					return null;

				storageType = tuple.type;
				labelInfo = labelInfos[storageType];

				return gameObject.getChild($"../{tuple.name}")?.GetComponent<ColoredLabel>();
			}
		}
#endif // GAME_BZ
	}
}