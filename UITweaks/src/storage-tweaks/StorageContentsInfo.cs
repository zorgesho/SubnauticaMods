using System.Linq;
using System.Text;
using System.Collections.Generic;

using Common;
using Common.Configuration;

namespace UITweaks.StorageTweaks
{
	static partial class StorageContentsInfo
	{
		record Item(TechType techType, int count);

		static readonly Dictionary<ItemsContainer, string> contentsCache = new(); // TODO use weak references

		public class InvalidateCache: Config.Field.IAction
		{
			public void action() => contentsCache.Clear();
		}

		static string getRaw(ItemsContainer container, int maxItemCount, bool slotsInfo)
		{
			using var _ = Debug.profiler("StorageContentsInfo.getRaw");

			string result;
			int slotsUsed = 0;

			if (container.count == 0)
			{
				return Language.main.Get("Empty"); // don't show slot count for empty containers
			}
			else
			{
				var list = container._items.
					Select(pair => new Item(pair.Key, pair.Value.items.Count)).
					OrderByDescending(item => item.count).
					ToList();

				StringBuilder sb = new();

				for (int i = 0; i < list.Count; i++)
				{
					var item = list[i];

					if (i < maxItemCount)
						sb.Append($"{Language.main.Get(item.techType)}{(item.count == 1? "": $" ({item.count})")}, ");

					slotsUsed += Utils.getItemSize(item.techType) * item.count;
				}

				if (sb.Length > 0)
				{
					sb.Remove(sb.Length - 2, 2);

					if (maxItemCount < list.Count)
						sb.Append(L10n.str("ids_otherItems"));
				}

				result = sb.ToString();
			}

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

		public static string getInfo(ItemsContainer container)
		{
			if (contentsCache.TryGetValue(container, out string cachedInfo) && cachedInfo != null)
				return cachedInfo;

			return contentsCache[container] = getRaw(container, Main.config.storageTweaks.showMaxItemCount, Main.config.storageTweaks.showSlotsInfo);
		}
	}
}