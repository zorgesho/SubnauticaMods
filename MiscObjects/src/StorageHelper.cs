using UnityEngine;
using Common;

namespace MiscObjects
{
	static class StorageHelper
	{
		public static StorageContainer addStorageToPrefab(GameObject prefab, string hoverText, string storageLabel, int width, int height)
		{
			GameObject storageRoot = Object.Instantiate(CraftData.GetBuildPrefab(TechType.SmallLocker).getChild("StorageRoot"));
			storageRoot.setParent(prefab, false);

			prefab.SetActive(false); // deactivating gameobject in order to not invoke Awake for StorageContainer when added

			StorageContainer container = prefab.AddComponent<StorageContainer>();
			container.storageRoot = storageRoot.GetComponent<ChildObjectIdentifier>();
			container.prefabRoot = prefab;
			container.preventDeconstructionIfNotEmpty = true;
			container.hoverText = hoverText;
			container.storageLabel = storageLabel;
			container.Resize(width, height);
			
			prefab.SetActive(true);

			return container;
		}
	}
}