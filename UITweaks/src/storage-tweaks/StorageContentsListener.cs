using System.Linq;
using System.Collections.Generic;

using UnityEngine; 

using Common.Reflection;

namespace UITweaks.StorageTweaks
{
	abstract class StorageContentsListener: MonoBehaviour
	{
		protected record ItemCount(TechType techType, int count)
		{
			public string name => Language.main.Get(techType);
		}

		static readonly EventWrapper onAddItem = typeof(ItemsContainer).evnt("onAddItem").wrap();
		static readonly EventWrapper onRemoveItem = typeof(ItemsContainer).evnt("onRemoveItem").wrap();

		protected ItemsContainer container { get; private set; }

		protected abstract void onContentsChanged();
		void contentsListener(InventoryItem _) => onContentsChanged();

		protected virtual void Awake()
		{
			container = GetComponent<StorageContainer>()?.container;
			Common.Debug.assert(container != null);

			onAddItem.add<OnAddItem>(container, contentsListener);
			onRemoveItem.add<OnRemoveItem>(container, contentsListener);
		}

		protected virtual void OnDestroy()
		{
			onAddItem.remove<OnAddItem>(container, contentsListener);
			onRemoveItem.remove<OnRemoveItem>(container, contentsListener);
		}

		// get items in container ordered by descending count
		protected List<ItemCount> getItems()
		{
			return container._items.
				Select(pair => new ItemCount(pair.Key, pair.Value.items.Count)).
				OrderByDescending(item => item.count).
				ToList();
		}
	}
}