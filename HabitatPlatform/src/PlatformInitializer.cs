using System.Collections;

using UnityEngine;

using Common;
using Common.Crafting;

namespace HabitatPlatform
{
	class PlatformInitializer: MonoBehaviour
	{
		static readonly Vector3 initialFoundationPos = new Vector3(11.7f, 1.4f, 7.5f);

		class RigidbodyKinematicFixer: MonoBehaviour
		{
			Rigidbody rb;
			float timeBuildCompleted;

			void Awake()
			{
				rb = gameObject.GetComponent<Rigidbody>();
				rb.isKinematic = true;
			}

			void SubConstructionComplete()
			{
				rb.isKinematic = false;
				timeBuildCompleted = Time.time;
			}

			void FixedUpdate()
			{
				if (!rb.isKinematic && timeBuildCompleted + 5f < Time.time && rb.velocity.sqrMagnitude < 0.1f)
				{
					rb.isKinematic = true;
					Destroy(this);
				}
			}
		}


		IEnumerator Start()
		{
			if (gameObject.transform.position.y < -4500f) // don't add stuff to SMLHelper prefab
				yield break;

			yield return null; // wait for one frame while VFXConstructing is initialized

			var vfx = gameObject.getChild("Base").GetComponent<VFXConstructing>();

			if (vfx.constructed < 1)
			{
				gameObject.AddComponent<RigidbodyKinematicFixer>();
				addFoundations(); // foundations added to platform explicitly only while constructing

				while (vfx.constructed < 1) // wait until construction is complete
					yield return null;
			}

			addFloor();

			gameObject.getChild("Base/RocketConstructorPlatform").SetActive(true);

			var guiScreen = gameObject.getChild("Base/BuildTerminal/GUIScreen").GetComponent<uGUI_RocketBuildScreen>();
			guiScreen.buildScreen.SetActive(false);
			guiScreen.customizeScreen.SetActive(false);
			guiScreen.buildAnimationScreen.SetActive(false);

			// ignoring plaform colliders for builder so they don't interfere with foundations
			GameObject collisions = gameObject.getChild("Base/BaseCollisions");
			for (int i = 0; i < collisions.transform.childCount; i++)
				CollidersPatch.addIgnored(collisions.transform.GetChild(i).GetComponents<Collider>());

			// ignoring colliders for outer ladders
			GameObject ladders = gameObject.getChild("Base/Triggers");
			for (int i = 1; i <= 6; i++)
				CollidersPatch.addIgnored(ladders.getChild($"outerLadders{i}").GetComponentInChildren<Collider>());

			Destroy(this);																						"PlatformInitializer: destroying".logDbg();
		}


		void addFoundations()
		{																										"PlatformInitializer: adding foundations".logDbg();
			var baseObject = CraftHelper.Utils.prefabCopy("WorldEntities/Structures/Base");
			LargeWorld.main?.streamer.cellManager.RegisterEntity(baseObject);

			Base platformBase = baseObject.GetComponent<Base>();

			// adding to platform
			platformBase.transform.parent = gameObject.transform;
			platformBase.transform.localPosition = initialFoundationPos;
			platformBase.transform.localEulerAngles = Vector3.zero;

			// creating ghost foundation
			GameObject foundation = CraftHelper.Utils.prefabCopy(TechType.BaseFoundation);
			var csBase = foundation.GetComponent<ConstructableBase>();
			var baseGhost = csBase.model.GetComponent<BaseGhost>();

			baseGhost.ghostBase.SetSize(Base.CellSize[2]);
			baseGhost.ghostBase.SetCell(Int3.zero, Base.CellType.Foundation);

			baseGhost.targetBase = platformBase;
			csBase.transform.parent = platformBase.transform;

			for (int x = 0; x < 4; x++)
			{
				for (int z = 0; z < 3; z++)
				{
					csBase.transform.localPosition = new Vector3(-10f * x, 0f, -10f * z);

					baseGhost.targetOffset = platformBase.WorldToGrid(baseGhost.transform.position);
					platformBase.CopyFrom(baseGhost.ghostBase, baseGhost.ghostBase.Bounds, baseGhost.targetOffset); // actual foundation creating
				}
			}

			Destroy(foundation); // destroying ghost
		}


		void addFloor()
		{																										"PlatformInitializer: adding floor".logDbg();
			GameObject prefab = CraftData.GetPrefabForTechType(TechType.RocketBase);
			GameObject platformBase = prefab.getChild("Base/rocketship_platform/Rocket_Geo/Rocketship_platform/Rocketship_platform_base-1/Rocketship_platform_base_MeshPart0");
			Material floorMaterialToCopy = platformBase.GetComponent<Renderer>().materials[6];

			var floor = Instantiate(AssetsHelper.loadPrefab("floor.prefab"));
			Material floorMaterial = floor.GetComponent<Renderer>().material;

			Vector2 texScale = floorMaterial.mainTextureScale;
			Texture texMain = floorMaterial.GetTexture("_MainTex");
			Texture texBump = floorMaterial.GetTexture("_BumpMap");

			floorMaterial.shader = Shader.Find("MarmosetUBER");
			floorMaterial.CopyPropertiesFromMaterial(floorMaterialToCopy); // TODO: get rid of this
			floorMaterial.DisableKeyword("UWE_LIGHTMAP");

			floorMaterial.SetTexture("_MainTex", texMain);
			floorMaterial.SetTexture("_SpecTex", texMain);
			floorMaterial.SetTexture("_BumpMap", texBump);

			floorMaterial.SetTextureScale("_MainTex", texScale);
			floorMaterial.SetTextureScale("_BumpMap", texScale);
			floorMaterial.SetTextureScale("_SpecTex", texScale);

			var skyApplier = floor.AddComponent<SkyApplier>();
			skyApplier.renderers = floor.GetComponents<Renderer>();
			skyApplier.anchorSky = Skies.Auto;

			floor.transform.parent = gameObject.transform;
			floor.transform.localRotation = Quaternion.identity;
			floor.transform.localPosition = new Vector3(0.05f, 2.863f, 0.065f);
			floor.transform.localScale = new Vector3(42.44f, 0.1f, 34.51f);

			CollidersPatch.addIgnored(floor.GetComponent<Collider>());
		}
	}
}