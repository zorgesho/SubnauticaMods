using UnityEngine;

using Common;
using Common.Stasis;
using Common.Crafting;

namespace StasisTorpedo
{
	partial class StasisTorpedo
	{
		class StasisExplosion: MonoBehaviour
		{
			void Start()
			{
				StasisSphereCreator.create(transform.position, Main.config.stasisTime, Main.config.stasisRadius);
				Destroy(gameObject);
			}
		}

		public static TorpedoType torpedoType { get; private set; }

		public static void initPrefab(GameObject gasTorpedoPrefab)
		{
			if (torpedoType != null)
				return;

			if (!gasTorpedoPrefab)
			{
				"StasisTorpedo.initPrefab: invalid prefab for GasTorpedo!".logError();
				return;
			}

			var explosionPrefab = new GameObject("StasisExplosion", typeof(StasisExplosion));
			SMLHelper.V2.Assets.ModPrefabCache.AddPrefab(explosionPrefab, false);

			var torpedoPrefab = PrefabUtils.storePrefabCopy(gasTorpedoPrefab);
			torpedoPrefab.GetComponent<SeamothTorpedo>().explosionPrefab = explosionPrefab;

			torpedoType = new() { techType = TechType, prefab = torpedoPrefab };
		}
	}
}