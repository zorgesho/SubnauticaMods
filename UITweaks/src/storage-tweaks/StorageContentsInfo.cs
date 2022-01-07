using System.Text;

using UnityEngine; 

using Common;
using Common.Reflection;
using Common.Configuration;

namespace UITweaks.StorageTweaks
{
	partial class StorageContentsInfo: MonoBehaviour
	{
		public class InvalidateCache: Config.Field.IAction
		{
			public void action() =>
				UnityHelper.FindObjectsOfTypeAll<StorageContentsInfo>().forEach(info => info.invalidateCache());
		}

		static readonly EventWrapper onAddItem = typeof(ItemsContainer).evnt("onAddItem").wrap();
		static readonly EventWrapper onRemoveItem = typeof(ItemsContainer).evnt("onRemoveItem").wrap();

		string cachedInfo;
		ItemsContainer container;

		void invalidateCache() => cachedInfo = null;
		void contentsListener(InventoryItem _) => invalidateCache();

		public string getInfo() =>
			cachedInfo ??= getRawInfo(Main.config.storageTweaks.showMaxItemCount, Main.config.storageTweaks.showSlotsInfo);

		void Awake()
		{
			container = GetComponent<StorageContainer>()?.container;
			Common.Debug.assert(container != null);

			onAddItem.add<OnAddItem>(container, contentsListener);
			onRemoveItem.add<OnRemoveItem>(container, contentsListener);
		}

		void OnDestroy()
		{
			onAddItem.remove<OnAddItem>(container, contentsListener);
			onRemoveItem.remove<OnRemoveItem>(container, contentsListener);
		}

		string getRawInfo(int maxItemCount, bool slotsInfo)
		{
			using var _ = Common.Debug.profiler("StorageContentsInfo.getRawInfo");

			string result;
			int slotsUsed = 0;

			if (container.count == 0)
			{
				return Language.main.Get("Empty"); // don't show slot count for empty containers
			}
			else
			{
				var list = Utils.getItems(container);
				StringBuilder sb = new();

				for (int i = 0; i < list.Count; i++)
				{
					var item = list[i];

					if (i < maxItemCount)
						sb.Append($"{item.name}{(item.count == 1? "": $" ({item.count})")}, ");

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
	}
}