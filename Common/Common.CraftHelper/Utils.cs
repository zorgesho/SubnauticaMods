using System;
using System.Collections;

using UWE;
using UnityEngine;
using SMLHelper.V2.Assets;

#if BRANCH_STABLE
using System.IO;
#endif

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

		public static string getPrefabClassId(TechType techType) => CraftData.GetClassIdForTechType(techType);

		public static string getPrefabFilename(string classId) => PrefabDatabase.TryGetPrefabFilename(classId, out string filename)? filename: null;
		public static string getPrefabFilename(TechType techType) => getPrefabFilename(getPrefabClassId(techType));

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

		public static GameObject storePrefabCopy(GameObject prefab)
		{
			if (!prefab)
				return null;

			var copiedPrefab = _instantiate(prefab, CopyOptions.UseCache);
			copiedPrefab.name = copiedPrefab.name.Replace("(Clone)", "");
			copiedPrefab.transform.position = Vector3.zero;

			return copiedPrefab;
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

#if GAME_SN && BRANCH_STABLE // to avoid warnings :(
#pragma warning disable CS0618 // marked obsolete in stable branch
#endif
		static IPrefabRequest _getPrefabForFilenameAsync(string filename) => PrefabDatabase.GetPrefabForFilenameAsync(filename);
#if GAME_SN && BRANCH_STABLE
#pragma warning restore CS0618
#endif

		public static CoroutineTask<GameObject> getPrefabAsync(TechType techType)
		{																												$"PrefabUtils: getPrefabAsync(TechType.{techType})".logDbg();
			return CraftData.GetPrefabForTechTypeAsync(techType);
		}

		public static CoroutineTask<GameObject> getPrefabAsync(string filename)
		{
			TaskResult<GameObject> result = new();
			return new CoroutineTask<GameObject>(getPrefabAsync(filename, result), result);
		}

		static IEnumerator getPrefabAsync(string filename, IOut<GameObject> result)
		{																												$"PrefabUtils: getPrefabAsync(\"{filename}\")".logDbg();
			var request = _getPrefabForFilenameAsync(filename);

			yield return request;
			request.TryGetPrefab(out GameObject prefab);

#if BRANCH_STABLE
			if (!prefab) // trying without extension
			{																											$"PrefabUtils: trying to load \"{filename}\" without extension".logDbg();
				request = _getPrefabForFilenameAsync(Path.ChangeExtension(filename, null));
				yield return request;
				request.TryGetPrefab(out prefab);
			}
#endif
			result.Set(prefab);
		}

		public static CoroutineTask<GameObject> getPrefabCopyAsync(TechType techType, CopyOptions options = CopyOptions.Default)
		{
			TaskResult<GameObject> result = new();
			return new CoroutineTask<GameObject>(getPrefabCopyAsync(techType, result, options), result);
		}

		public static IEnumerator getPrefabCopyAsync(TechType techType, IOut<GameObject> result, CopyOptions options = CopyOptions.Default)
		{																												$"PrefabUtils: getPrefabCopyAsync(TechType.{techType})".logDbg();
			TaskResult<GameObject> prefabResult = new();
			yield return CraftData.GetPrefabForTechTypeAsync(techType, false, prefabResult);

			result.Set(_instantiate(prefabResult.Get(), options));
		}

		public static CoroutineTask<GameObject> getPrefabCopyAsync(string filename, CopyOptions options = CopyOptions.Default)
		{
			TaskResult<GameObject> result = new();
			return new CoroutineTask<GameObject>(getPrefabCopyAsync(filename, result, options), result);
		}

		public static IEnumerator getPrefabCopyAsync(string filename, IOut<GameObject> result, CopyOptions options = CopyOptions.Default)
		{																												$"PrefabUtils: getPrefabCopyAsync(\"{filename}\")".logDbg();
			TaskResult<GameObject> prefabResult = new();
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

		public static void initVFXFab(
			GameObject prefab,
			Vector3? posOffset = null,
			Vector3? eulerOffset = null,
			float? localMinY = null,
			float? localMaxY = null,
			float? scaleFactor = null)
		{
			var vfxFab = prefab.GetComponentInChildren<VFXFabricating>();

			if (!vfxFab)
			{
				$"VFXFabricating for {prefab?.name} not found".logError();
				return;
			}

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