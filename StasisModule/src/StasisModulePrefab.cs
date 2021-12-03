using System.Collections;

using UnityEngine;

using Common;
using Common.Crafting;

namespace StasisModule
{
	partial class StasisModule
	{
		public class StasisExplosion: MonoBehaviour
		{
			static int instancesCount = 0; // for disabling stasis sound
			static GameObject stasisSpherePrefab;

			StasisSphere sphere;

			public static void initPrefab(GameObject stasisRiflePrefab)
			{
				if (stasisSpherePrefab)
					return;

				if (!stasisRiflePrefab)
				{
					"StasisExplosion.initPrefab: invalid prefab for StasisRifle!".logError();
					return;
				}

				stasisSpherePrefab = PrefabUtils.storePrefabCopy(stasisRiflePrefab.GetComponent<StasisRifle>().effectSpherePrefab);
				stasisSpherePrefab.transform.position = Vector3.zero;
			}

			IEnumerator Start()
			{
				instancesCount++;																					$"StasisExplosion.Start: instances = {instancesCount}".logDbg();

				if (!stasisSpherePrefab)
				{
					"StasisExplosion.Start: invalid prefab for StasisSphere!".logError();

					Destroy(gameObject);
					yield break;
				}

				// HACK, waiting to remove stasis sounds from other torpedoes before creating stasis sphere
				// it's far from perfect, but it's better than silence
				yield return Utils.releaseAllEventInstances("event:/tools/stasis_gun/sphere_activate");

				sphere = gameObject.createChild(stasisSpherePrefab).GetComponent<StasisSphere>();

				sphere.time = Main.config.stasisTime;
				sphere.radius = Main.config.stasisRadius;
				sphere.fieldEnergy = 1f;

				sphere.go.SetActive(true); // 'go' is deactivated in Bullet.Awake
				sphere.EnableField();
			}

			void Update()
			{
				if (sphere?.fieldEnabled == false)
					Destroy(gameObject);
			}

			void OnDestroy()
			{
				instancesCount--;																					$"StasisExplosion.OnDestroy: instances = {instancesCount}".logDbg();

				// HACK, for some reason sound from the last exploded torpedo doesn't shut up
				// also, this fixes sound for stasis rifle
				if (sphere && instancesCount == 0)
					sphere.soundActivate.evt.getDescription().releaseAllInstances();
			}
		}
	}
}