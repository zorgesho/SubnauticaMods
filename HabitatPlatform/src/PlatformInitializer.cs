using System.Collections;

using UnityEngine;

using Common;
using Common.Crafting;

namespace HabitatPlatform
{
	class PlatformInitializer: MonoBehaviour
	{
		static readonly Vector3 initialFoundationPos = new Vector3(11.7f, 1.4f, 7.5f);
#if DEBUG
		public class FloorTag: MonoBehaviour {} // for use in debug console commands
#endif
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
			else
			{
#if DEBUG
				if (Main.config.dbgKinematicForBuilded)
#endif
					gameObject.GetComponent<Rigidbody>().isKinematic = true;
			}

			addFloor();
			processChildren();

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
			var floor = Instantiate(AssetsHelper.loadPrefab("floor"));

			Material floorMaterial = floor.GetComponent<Renderer>().material;
			floorMaterial.shader = Shader.Find("MarmosetUBER");
			floorMaterial.DisableKeyword("UWE_LIGHTMAP");
			floorMaterial.DisableKeyword("_EMISSION");
			floorMaterial.DisableKeyword("_NORMALMAP");

			floorMaterial.SetTexture("_SpecTex", floorMaterial.GetTexture("_MainTex"));
			floorMaterial.SetTextureScale("_SpecTex", floorMaterial.mainTextureScale);
			floorMaterial.SetTextureScale("_BumpMap", floorMaterial.mainTextureScale);

			floor.transform.parent = gameObject.transform;
			floor.transform.localRotation = Quaternion.identity;
			floor.transform.localPosition = new Vector3(0.05f, 2.863f, 0.065f);
			floor.transform.localScale = new Vector3(42.44f, 0.1f, 34.51f);

			CollidersPatch.addIgnored(floor.GetComponent<Collider>());
#if DEBUG
			floor.AddComponent<FloorTag>();
#endif
		}


		void processChildren()
		{																										"PlatformInitializer: processing child objects".logDbg();
			gameObject.getChild("Base/RocketConstructorPlatform").SetActive(true); // to enable colliders for terminal

			// disabling terminal screen
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

			// moving platform engines and their colliders closer to the corners (to free up some space for building on the bottom)
			const float dx = 0.025f, dy = 0.031f;
			Vector3[] offsets = new[] { new Vector3(dx, -dy, 0f), new Vector3(dx, dy, 0f), new Vector3(-dx, dy, 0f), new Vector3(-dx, -dy, 0f) };

			GameObject platform = gameObject.getChild("Base/rocketship_platform/Rocket_Geo/Rocketship_platform");
			for (int i = 1; i <= 4; i++)
				platform.transform.Find($"Rocketship_platform_power_0{i}").localPosition = offsets[i - 1];

			var colliders = collisions.getChild("Cube").GetComponents<BoxCollider>();
			for (int i = 4; i <= 7; i++)
			{
				colliders[i].center += offsets[7 - i] / 0.006f; // really, UWE?

				if (!Main.config.ignoreEnginesColliders)
					CollidersPatch.removeIgnored(colliders[i]);
			}

			// changing lightmap for the bottom (we need this because of moved engines)
			Texture2D lightmap = AssetsHelper.loadTexture("platform_lightmap");
			GameObject platformBase = platform.getChild("Rocketship_platform_base-1/Rocketship_platform_base_MeshPart0");

			foreach (var m in platformBase.GetComponent<MeshRenderer>().materials)
				if (m.GetTexture("_Lightmap")?.name == "Rocketship_exterior_platform_lightmap")
					m.SetTexture("_Lightmap", lightmap);
		}
	}
}