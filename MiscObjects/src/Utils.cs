using UnityEngine;
using Common;

namespace MiscObjects
{
	static class Utils
	{
		public static void addStorageToPrefab(GameObject prefab, GameObject storagePrefab)
		{
			var storageRoot = Object.Instantiate(storagePrefab.getChild("StorageRoot"), prefab.transform);

			var container = prefab.AddComponent<StorageContainer>();
			container.storageRoot = storageRoot.GetComponent<ChildObjectIdentifier>();
			container.prefabRoot = prefab;
		}
	}
}