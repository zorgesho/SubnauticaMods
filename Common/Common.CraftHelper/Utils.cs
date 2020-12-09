using System;
using System.Collections;
#if BRANCH_STABLE
using System.IO;
#endif

using UWE;
using UnityEngine;
using SMLHelper.V2.Assets;

namespace Common.Crafting
{
	using Object = UnityEngine.Object;

	static class PrefabUtils
	{
		[Flags]
		public enum CopyOptions
		{
			None = 0,
			AutoRemove = 1,
			UseCache = 2,
			Default = AutoRemove | UseCache
		}

		// BRANCH_EXP TODO: remove for SN after async update
		#region sync 'getPrefab' methods
		public static GameObject getPrefab(TechType techType)
		{																												$"PrefabUtils: getPrefab(TechType.{techType})".logDbg();
#if GAME_SN && BRANCH_STABLE
			return CraftData.GetPrefabForTechType(techType);
#else
			return null;
#endif
		}

		public static GameObject getPrefab(string filename)
		{																												$"PrefabUtils: getPrefab(\"{filename}\")".logDbg();
#if GAME_SN && BRANCH_STABLE
			return Resources.Load<GameObject>(filename);
#else
			return null;
#endif
		}

		public static GameObject getPrefabCopy(TechType techType, CopyOptions options = CopyOptions.Default)
		{																												$"PrefabUtils: getPrefabCopy(TechType.{techType})".logDbg();
			return _instantiate(getPrefab(techType), options);
		}

		public static GameObject getPrefabCopy(string filename, CopyOptions options = CopyOptions.Default)
		{																												$"PrefabUtils: getPrefabCopy(\"{filename}\")".logDbg();
			return _instantiate(getPrefab(filename), options);
		}
		#endregion


		#region async 'getPrefab' methods
		public static CoroutineTask<GameObject> getPrefabAsync(TechType techType)
		{																												$"PrefabUtils: getPrefabAsync(TechType.{techType})".logDbg();
			return CraftData.GetPrefabForTechTypeAsync(techType);
		}

		public static CoroutineTask<GameObject> getPrefabAsync(string filename)
		{
			var result = new TaskResult<GameObject>();
			return new CoroutineTask<GameObject>(getPrefabAsync(filename, result), result);
		}

		static IEnumerator getPrefabAsync(string filename, IOut<GameObject> result)
		{																												$"PrefabUtils: getPrefabAsync(\"{filename}\")".logDbg();
#pragma warning disable CS0618 // marked obsolete in stable branch
			var request = PrefabDatabase.GetPrefabForFilenameAsync(filename);
#pragma warning restore CS0618
			yield return request;
			request.TryGetPrefab(out GameObject prefab);

#if BRANCH_STABLE
			if (!prefab) // trying without extension
			{																											$"PrefabUtils: trying to load \"{filename}\" without extension".logDbg();
#pragma warning disable CS0618
				request = PrefabDatabase.GetPrefabForFilenameAsync(Path.ChangeExtension(filename, null));
#pragma warning restore CS0618
				yield return request;
				request.TryGetPrefab(out prefab);
			}
#endif
			result.Set(prefab);
		}

		public static CoroutineTask<GameObject> getPrefabCopyAsync(TechType techType, CopyOptions options = CopyOptions.Default)
		{
			var result = new TaskResult<GameObject>();
			return new CoroutineTask<GameObject>(getPrefabCopyAsync(techType, result, options), result);
		}

		public static IEnumerator getPrefabCopyAsync(TechType techType, IOut<GameObject> result, CopyOptions options = CopyOptions.Default)
		{																												$"PrefabUtils: getPrefabCopyAsync(TechType.{techType})".logDbg();
			var prefabResult = new TaskResult<GameObject>();
			yield return CraftData.GetPrefabForTechTypeAsync(techType, false, prefabResult);

			result.Set(_instantiate(prefabResult.Get(), options));
		}

		public static CoroutineTask<GameObject> getPrefabCopyAsync(string filename, CopyOptions options = CopyOptions.Default)
		{
			var result = new TaskResult<GameObject>();
			return new CoroutineTask<GameObject>(getPrefabCopyAsync(filename, result, options), result);
		}

		public static IEnumerator getPrefabCopyAsync(string filename, IOut<GameObject> result, CopyOptions options = CopyOptions.Default)
		{																												$"PrefabUtils: getPrefabCopyAsync(\"{filename}\")".logDbg();
			var prefabResult = new TaskResult<GameObject>();
			yield return getPrefabAsync(filename, prefabResult);

			result.Set(_instantiate(prefabResult.Get(), options));
		}

		static GameObject _instantiate(GameObject gameObject, CopyOptions options)
		{
			if (options.HasFlag(CopyOptions.UseCache))
				return ModPrefabCache.AddPrefabCopy(gameObject, options.HasFlag(CopyOptions.AutoRemove));
			else
				return Object.Instantiate(gameObject);
		}
		#endregion


		#region misc helpers
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

			if (posOffset != null)	 vfxFab.posOffset	= (Vector3)posOffset;
			if (eulerOffset != null) vfxFab.eulerOffset	= (Vector3)eulerOffset;
			if (localMinY != null)	 vfxFab.localMinY	= (float)localMinY;
			if (localMaxY != null)	 vfxFab.localMaxY	= (float)localMaxY;
			if (scaleFactor != null) vfxFab.scaleFactor	= (float)scaleFactor;
		}

		public static StorageContainer initStorage(GameObject prefab, int width, int height, string hoverText = "HoverText", string storageLabel = "StorageLabel")
		{
			StorageContainer container = prefab.GetComponentInChildren<StorageContainer>();
			container.preventDeconstructionIfNotEmpty = true;
			container.hoverText = hoverText;
			container.storageLabel = storageLabel;
			container.Resize(width, height);

			return container;
		}
		#endregion
	}
}