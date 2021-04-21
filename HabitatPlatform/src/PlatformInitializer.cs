using System.Collections;

using UnityEngine;
using UnityEngine.UI;

using Common;
using Common.Crafting;

namespace HabitatPlatform
{
	partial class PlatformInitializer: MonoBehaviour
	{
		static readonly Vector3 firstFoundationPos = new(13.7f, 1.4f, 7.5f);

		static readonly Vector3 floorPos = new(0.05f, 2.863f, 0.065f);
		static readonly Vector3 floorScale = new(42.44f, 0.1f, 34.51f);
#if DEBUG
		public class FloorTag: MonoBehaviour {} // for use in debug console commands
#endif

		IEnumerator Start()
		{																										"PlatformInitializer: starting".logDbg();
			yield return null; // wait for one frame while VFXConstructing is initialized

			var vfx = gameObject.getChild("Base").GetComponent<VFXConstructing>();

			if (vfx.constructed < 1)
			{
				gameObject.AddComponent<RigidbodyKinematicFixer>();
				yield return addFoundations(); // foundations added to platform explicitly only while constructing

				yield return new WaitWhile(() => vfx.constructed < 1); // wait until construction is complete
			}
			else
			{
				RigidbodyKinematicFixer.disablePhysics(gameObject);

				if (Main.config.tryFixCollisionBug)
				{
					PlatformCollisionFixer.fix(gameObject);
					gameObject.AddComponent<TransformFixer>();

					Player.main.gameObject.ensureComponent<PlayerTransformFixer>(); // it's safe because Player deserializes before bases
				}
			}

			addFloor();
			processChildren();

			Destroy(this);																						"PlatformInitializer: destroying".logDbg();
		}


		IEnumerator addFoundations()
		{																										"PlatformInitializer: adding foundations".logDbg();
			var task = PrefabUtils.getPrefabCopyAsync("WorldEntities/Structures/Base.prefab", PrefabUtils.CopyOptions.None);
			yield return task;
			var baseObject = task.GetResult(); // for some reason we can't instantiate it right to the gameObject

			LargeWorld.main?.streamer.cellManager.RegisterEntity(baseObject);

			// adding Base to platform
			baseObject.setParent(gameObject, localPos: firstFoundationPos);

			// creating ghost foundation
			task = PrefabUtils.getPrefabCopyAsync(TechType.BaseFoundation, PrefabUtils.CopyOptions.None);
			yield return task;
			var foundation = task.GetResult();

			var csBase = foundation.GetComponent<ConstructableBase>();
			var baseGhost = csBase.model.GetComponent<BaseGhost>();

			baseGhost.ghostBase.SetSize(Base.CellSize[2]);
			baseGhost.ghostBase.SetCell(Int3.zero, Base.CellType.Foundation);

			var platformBase = baseObject.GetComponent<Base>();
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
			var floor = gameObject.createChild(AssetsHelper.loadPrefab("floor"), localPos: floorPos, localScale: floorScale);
			var rend = floor.GetComponent<Renderer>();

			Material floorMaterial = rend.material;
			floorMaterial.shader = Shader.Find("MarmosetUBER");
			floorMaterial.DisableKeyword("UWE_LIGHTMAP");
			floorMaterial.DisableKeyword("_EMISSION");
			floorMaterial.DisableKeyword("_NORMALMAP");

			floorMaterial.SetTexture("_SpecTex", floorMaterial.GetTexture("_MainTex"));
			floorMaterial.SetTextureScale("_SpecTex", floorMaterial.mainTextureScale);
			floorMaterial.SetTextureScale("_BumpMap", floorMaterial.mainTextureScale);

			var skyApplier = gameObject.GetComponent<SkyApplier>();
			skyApplier.renderers = skyApplier.renderers.append(new[] { rend });
			skyApplier.RefreshDirtySky();

			floor.AddComponent<VFXSurface>().surfaceType = VFXSurfaceTypes.metal;
#if DEBUG
			floor.AddComponent<FloorTag>();
#endif
			CollidersPatch.addIgnored(floor.GetComponent<Collider>());
		}


		void processChildren()
		{																										"PlatformInitializer: processing child objects".logDbg();
			gameObject.getChild("Base/RocketConstructorPlatform").SetActive(true); // to enable colliders for terminal

			// disabling building and customization screens
			var guiScreen = gameObject.getChild("Base/BuildTerminal/GUIScreen").GetComponent<uGUI_RocketBuildScreen>();
			guiScreen.buildScreen.SetActive(false);
			guiScreen.customizeScreen.SetActive(false);

			// adding temporary text to the terminal
			guiScreen.buildAnimationScreen.SetActive(true);
			guiScreen.buildAnimationScreen.GetComponentsInChildren<Image>().forEach(image => image.enabled = false);

			var screen = guiScreen.buildAnimationScreen.GetComponent<uGUI_BuildBotScreen>();
			screen.enabled = false;
			screen.constructingText.text = L10n.str(L10n.ids_terminalText);

			// ignoring plaform colliders for builder so they don't interfere with foundations
			GameObject collisions = gameObject.getChild("Base/BaseCollisions");
			foreach (Transform child in collisions.transform)
				CollidersPatch.addIgnored(child.GetComponents<Collider>());

			// ignoring colliders for outer ladders
			GameObject ladders = gameObject.getChild("Base/Triggers");
			for (int i = 1; i <= 6; i++)
				CollidersPatch.addIgnored(ladders.getChild($"outerLadders{i}").GetComponentInChildren<Collider>());

			// moving engine colliders (engines are already moved in CraftableObject)
			var colliders = collisions.getChild("Cube").GetComponents<BoxCollider>();
			for (int i = 0; i < 4; i++)
			{
				colliders[i + 4].center += HabitatPlatform.engineOffsets[3 - i] / 0.006f; // really, UWE?

				if (!Main.config.ignoreEnginesColliders)
					CollidersPatch.removeIgnored(colliders[i]);
			}
		}
	}
}