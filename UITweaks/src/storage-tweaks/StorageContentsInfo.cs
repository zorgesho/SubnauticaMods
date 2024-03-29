﻿using System.Text;

using Common;
using Common.Configuration;

namespace UITweaks.StorageTweaks
{
	partial class StorageContentsInfo: StorageContentsListener
	{
		public class InvalidateCache: Config.Field.IAction
		{
			public void action() =>
				UnityHelper.FindObjectsOfTypeAll<StorageContentsInfo>().forEach(info => info.invalidateCache());
		}

		string cachedInfo;
		void invalidateCache() => cachedInfo = null;

		protected override void onContentsChanged() => invalidateCache();

		public string getInfo() =>
			cachedInfo ??= getRawInfo(Main.config.storageTweaks.showMaxItemCount, Main.config.storageTweaks.showSlotsInfo);

		string getRawInfo(int maxItemCount, bool slotsInfo)
		{
			if (container.count == 0)
				return Language.main.Get("Empty"); // don't show slot count for empty containers

			using var _ = Debug.profiler("StorageContentsInfo.getRawInfo");

			int slotsUsed = 0;
			var items = getItems();
			StringBuilder sb = new();

			for (int i = 0; i < items.Count; i++)
			{
				var item = items[i];

				if (i < maxItemCount)
					sb.Append($"{item.name}{(item.count == 1? "": $" ({item.count})")}, ");

				slotsUsed += Utils.getItemSize(item.techType) * item.count;
			}

			if (sb.Length > 0)
			{
				sb.removeFromEnd(2);

				if (maxItemCount < items.Count)
					sb.Append(L10n.str("ids_otherItems"));
			}

			string result = sb.ToString();

			if (slotsInfo)
			{
				int slotCount = container.sizeX * container.sizeY;

				if (result != "")
					result += "\n";

				if (slotCount == slotsUsed)
					result += L10n.str("ids_fullContainer");
				else
					result += L10n.str("ids_freeSlots").format(slotCount - slotsUsed, slotCount);
			}

			return result;
		}
	}
}