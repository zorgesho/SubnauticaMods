using System.Collections;
using UnityEngine;
using Common.Crafting;

namespace Common.Stasis
{
	static class StasisSphereCreator
	{
		public static void create(Vector3 position, float time, float radius)
		{
			Patches.patcher.patch();

			var go = new GameObject("StasisSphere");
			go.transform.position = position;
			go.AddComponent<StasisExplosion>().setProps(time, radius);
		}

		class StasisExplosion: MonoBehaviour
		{
			static int instancesCount = 0; // for disabling stasis sound
			static bool alreadyInit = false;
			static GameObject stasisSpherePrefab;

			float time;
			float radius;
			StasisSphere sphere;

			public void setProps(float time, float radius)
			{
				this.time = time;
				this.radius = radius;
			}

			static IEnumerator initPrefab()
			{
				if (stasisSpherePrefab)
					yield break;

				if (alreadyInit || !(alreadyInit = true)) // to avoid simultaneous initialization
				{
					yield return new WaitUntil(() => stasisSpherePrefab);
					yield break;
				}
#if GAME_BZ
#pragma warning disable CS0612 // TechType.StasisRifle is obsolete in BZ
#endif
				var task = PrefabUtils.getPrefabAsync(TechType.StasisRifle);
#if GAME_BZ
#pragma warning restore CS0612
#endif
				yield return task;
				var stasisRiflePrefab = task.GetResult();

				if (!stasisRiflePrefab)
				{
					"StasisExplosion.initPrefab: invalid prefab for StasisRifle!".logError();
					yield break;
				}

				stasisSpherePrefab = PrefabUtils.storePrefabCopy(stasisRiflePrefab.GetComponent<StasisRifle>()?.effectSpherePrefab);
			}

			IEnumerator Start()
			{
				instancesCount++;																					$"StasisExplosion.Start: instances = {instancesCount}".logDbg();

				if (!stasisSpherePrefab)
					yield return initPrefab();

				if (!stasisSpherePrefab)
				{
					"StasisExplosion.Start: invalid prefab for StasisSphere!".logError();

					Destroy(gameObject);
					yield break;
				}

				// HACK, waiting to remove stasis sounds from other spheres before creating a new one
				// it's far from perfect, but it's better than silence
				yield return Utils.releaseAllEventInstances("event:/tools/stasis_gun/sphere_activate");

				sphere = gameObject.createChild(stasisSpherePrefab).GetComponent<StasisSphere>();

				sphere.time = time;
				sphere.radius = radius;
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

				// HACK, for some reason sound from the last stasis sphere doesn't shut up
				// also, this fixes sound for stasis rifle
				// different mods will have different counters, so minor conflicts are expected
				if (sphere && instancesCount == 0)
					sphere.soundActivate.evt.getDescription().releaseAllInstances();
			}
		}
	}
}