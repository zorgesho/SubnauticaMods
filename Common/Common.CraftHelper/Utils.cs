using UnityEngine;

namespace Common.Crafting
{
	static partial class CraftHelper
	{
		public static class Utils
		{
			public static GameObject prefabCopy(TechType techType) => Object.Instantiate(CraftData.GetPrefabForTechType(techType));
			public static GameObject prefabCopy(string resourcePath) => Object.Instantiate(Resources.Load<GameObject>(resourcePath));


			public static Constructable initConstructable(GameObject prefab, GameObject model)
			{
				Constructable c = prefab.ensureComponent<Constructable>();

				c.allowedInBase = false;
				c.allowedInSub = false;
				c.allowedOutside = false;
				c.allowedOnWall = false;
				c.allowedOnGround = false;
				c.allowedOnCeiling = false;
				c.allowedOnConstructables = false;

				c.enabled = true;
				c.rotationEnabled = true;
				c.controlModelState = true;
				c.deconstructionAllowed = true;

				c.model = model;

				return c;
			}


			public static void initVFXFab(GameObject prefab,
				Vector3? posOffset = null,
				Vector3? eulerOffset = null,
				float? localMinY = null,
				float? localMaxY = null,
				float? scaleFactor = null)
			{
				var vfxFab = prefab.GetComponentInChildren<VFXFabricating>();

				if (!vfxFab && $"VFXFabricating for {prefab?.name} not found".logError())
					return;

				if (posOffset != null)		vfxFab.posOffset	= (Vector3)posOffset;
				if (eulerOffset != null)	vfxFab.eulerOffset	= (Vector3)eulerOffset;
				if (localMinY != null)		vfxFab.localMinY	= (float)localMinY;
				if (localMaxY != null)		vfxFab.localMaxY	= (float)localMaxY;
				if (scaleFactor != null)	vfxFab.scaleFactor	= (float)scaleFactor;
			}


			public static StorageContainer addStorageToPrefab(GameObject prefab, int width, int height, string hoverText = "HoverText", string storageLabel = "StorageLabel")
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
}