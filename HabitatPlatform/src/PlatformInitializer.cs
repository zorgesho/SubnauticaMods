using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Common;

#pragma warning disable

namespace HabitatPlatform
{
	class PlatformInitializer: MonoBehaviour
	{
		Base platformBase = null;
		GameObject floor = null;
		
		IEnumerator Start()
		{
			VFXConstructing c = gameObject.GetComponentInChildren<VFXConstructing>();

			if (c)
			{
				while (c.constructed < 3)
				{
					$"{c.constructed}".onScreen("constructed");
					yield return null;
				}
				
			}
			yield return new WaitForSeconds(1f);

			//yield break;

			//initOneTime();

			//_addAllFoundations();
			yield return new WaitForSeconds(1f);
			//_parentToPlatform();


			//init();

			//Destroy(this);
		}

		void _addAllFoundations()
		{
			_addFirstFoundation();
			//_addNextFoundation(-10, 0);
			for (int x = 0; x < Main.config.xxx; x++)
			{
				for (int z = 0; z < Main.config.zzz; z++)
				{
					if ((x == 0 && z == 0) || (x == 3 && z == 1))
						continue;

					_addNextFoundation(-10f * x, -10f * z);
				}
			}
		}

		void Update()
		{
			//VFXConstructing c = gameObject.GetComponentInChildren<VFXConstructing>();

			//if (c)
			//	$"constructed: {c.constructed}".onScreen();

			if (Input.GetKeyDown(KeyCode.PageDown))
			{
				_addAllFoundations();
//				Common.Debug.dump(gameObject);

			}

			if (Input.GetKeyDown(KeyCode.PageUp))
			{
				GameObject obj1 = gameObject.getChild("Base/rocketship_platform/Rocket_Geo/Rocketship_platform/Rocketship_platform_base-1/Rocketship_platform_base_MeshPart0");

				$"mesh   {obj1.GetComponent<MeshFilter>().mesh.subMeshCount}".log();

				Mesh mesh = obj1.GetComponent<MeshFilter>().sharedMesh;

				mesh.UploadMeshData(false);

				MeshRenderer meshRenderer = obj1.GetComponent<MeshRenderer>();

				var materials = meshRenderer.materials;

				int meshIndex = 0;
				
				for (int i = 0; i < materials.Length; i++)
				{
					$"{mesh.GetIndexCount(i)}  {materials[i].name}".log();

					if (materials[i].name.Contains("rocketship_wallmods_01_platform"))
					{
						//materials[i] = null;
						meshIndex = i;
					}
				}

				//meshRenderer.materials = materials;

				try
				{
					//List<Vector3> vertices = null;
					////new List<Vector3>();
					//mesh.GetVertices(vertices);

					Vector3[] vertices = mesh.vertices;
					$"{mesh.isReadable} {vertices.Length} {mesh.vertices.Length} {mesh.vertexCount}".log();
				}
				catch (System.Exception e)
				{
					Log.msg(e);
				}

				GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);

				Mesh mm = floor.GetComponent<MeshFilter>()?.mesh;

				if (mm == null)
					"null".log();
				else
				{
					$"{mm.vertices.Length}".log();
				}

				//List<int> indices = new List<int>();
				//mesh.GetIndices(indices, meshIndex);

				//$"{vertices.Count} {indices.Count}".log();

				//foreach (var i in indices)
				//{
				//	$"{i}  {vertices[i]}".log();
				//}





			}

			//if (Input.GetKeyDown(KeyCode.Alpha9))
			//	Common.Debug.dump(gameObject);


			if (false &&Input.GetKeyDown(KeyCode.Insert))
			{
				//BaseFoundationPiece[] foundations = platformBase.GetAllComponentsInChildren<BaseFoundationPiece>();
				BaseFoundationPiece[] foundations = platformBase.GetComponentsInChildren<BaseFoundationPiece>();

				$"{foundations.Length}".onScreen();

				foreach (var f in foundations)
				{
					GameObject models = f.gameObject.getChild("models");

					$"{models.transform.childCount}".log();
					for (int i = 0; i < models.transform.childCount; i++)
					{
						if (models.transform.GetChild(i).GetComponent<MeshRenderer>())
							models.transform.GetChild(i).GetComponent<MeshRenderer>().enabled = false;
					}
					
					//MeshRenderer[] meshes = f.gameObject.GetAllComponentsInChildren<MeshRenderer>();
					//$"{meshes.Length}".log();

					//meshes.forEach(mesh => mesh.enabled = false);
				}
			}



			//_addFirstFoundation();

			//if (Input.GetKeyDown(KeyCode.Delete))
			//{
			//	for (int x = 0; x < Main.config.xxx; x++)
			//	{
			//		for (int z = 0; z < Main.config.zzz; z++)
			//		{
			//			if ((x == 0 && z == 0) || (x == 3 && z == 1))
			//				continue;

			//			_addNextFoundation(-10f * x, -10f * z);
			//		}
			//	}
			//}
			//if (Input.GetKeyDown(KeyCode.Alpha8))
			//{
			//	Vector3 pp = floor.transform.localPosition;
			//	pp.y += 0.01f;
			//	floor.transform.localPosition = pp;
			//	$"{pp.y}".log();
			//}

			if (Input.GetKeyDown(KeyCode.Alpha7))
			{
				gameObject.getChild("Base/RocketConstructorPlatform").SetActive(true);
				uGUI_RocketBuildScreen ss = gameObject.GetComponentInChildren<uGUI_RocketBuildScreen>();

				if (ss)
				{
					ss.buildScreen.SetActive(false);
					ss.customizeScreen.SetActive(true);
					ss.buildAnimationScreen.SetActive(false);

				}
			}


			if (Input.GetKeyDown(KeyCode.Home))
			{
				//Base[] bb = Object.FindObjectsOfType<Base>();

				//for (int i = 0; i < bb.Length; i++)
				//	bb[i].gameObject.dump("!!base" + i);
		

				//Builder.ghostModel?.dump("!ghost_model");
				floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
				Common.Debug.dump(floor);
				//floor.transform.localScale = new Vector3(12.0f, 1.0f, 12.0f);
				floor.GetComponent<Renderer>().material.color = Color.gray;
				//floor.destroyComponent<Collider>(false);
				
				//debugSphere.SetActive(false);
				
				//Base b = Object.FindObjectOfType<Base>();
				//GameObject baseGo = b.gameObject;

				floor.transform.parent = gameObject.getChild("Base").transform;
				floor.transform.localPosition = new Vector3(0, 0, 0);
				floor.transform.localScale = new Vector3(43f, 0.1f, 35f);
					//baseGo.transform.localPosition = new Vector3(11.7f, -1f, 7.5f);
				floor.transform.localEulerAngles = Vector3.zero;
				//floor.GetComponent<MeshRenderer>().material = new Material(Resources.Load<Material>("Materials/starship_exploded_interrior_Locker_room_floormods_01_rocketship_platform"));
				
				//floor.GetComponent<MeshRenderer>().material.SetTexture


				//GameObject obj1 = gameObject.getChild("Base/rocketship_platform/Rocket_Geo/Rocketship_platform/Rocketship_platform_base-1/Rocketship_platform_base_MeshPart0");
				//////Common.Debug.dump(gameObject);

				//MeshRenderer renderer = obj1?.GetComponent<MeshRenderer>();

				//if (renderer)
				//{
				//	foreach (var m in renderer.materials)
				//	{
				//		$"{m.name}".log();

				//		if (m.name.Contains("starship_exploded_interrior_Locker_room_floormods_01_rocketship_platform"))
				//			floor.GetComponent<MeshRenderer>().material = m;

				//		m.sett

				//	}
				//}



			}
		}


		//IEnumerator _add()
		//{
		//	for (int x = 0; x < Main.config.xxx; x++)
		//	{
		//		for (int z = 0; z < Main.config.zzz; z++)
		//		{
		//			if ((x == 0 && z == 0) || (x == 3 && z == 1))
		//				continue;

		//			_addNextFoundation(-10f * x, -10f * z);
		//			yield return new WaitForSeconds(0.5f);
		//		}
		//	}	
		//}


		void init()
		{
																													"PlatformInit.init".onScreen().logDbg();
			gameObject.getChild("Base/RocketConstructorPlatform").SetActive(true);
		}

		void initOneTime()
		{
			// check one time
//			_addFoundation();
			//_parentToPlatform();
		}


		void _addFirstFoundation()
		{
			GameObject newObject = Instantiate(CraftData.GetPrefabForTechType(TechType.BaseFoundation));
			ConstructableBase constructableBase = newObject.GetComponent<ConstructableBase>();

			GameObject ghostModel = constructableBase.model;
			BaseGhost baseGhost = ghostModel.GetComponent<BaseGhost>();

			baseGhost.ghostBase.SetSize(Base.CellSize[2]);
			baseGhost.ghostBase.SetCell(Int3.zero, Base.CellType.Foundation);

			if (baseGhost.targetBase != null)
			{
				constructableBase.transform.parent = baseGhost.targetBase.transform;
			}

			// !!!!!!!!!
			//if (baseGhost.TargetBase != null)
			//	constructableBase.transform.SetParent(baseGhost.TargetBase.transform, true);

			if (baseGhost.targetBase == null)
			{
				GameObject gameObject;
				if (!UWE.PrefabDatabase.TryGetPrefabForFilename("WorldEntities/Structures/Base", out gameObject))
				{
					//Debug.LogErrorFormat(this, "Failed to load Base prefab in BaseGhost.Finish()", new object[0]);
					return;
				}
				GameObject gameObject2 = Object.Instantiate<GameObject>(gameObject, constructableBase.transform.position, constructableBase.transform.rotation);
				if (LargeWorld.main)
				{
					LargeWorld.main.streamer.cellManager.RegisterEntity(gameObject2);
				}
				baseGhost.targetBase = gameObject2.GetComponent<Base>();
				
				platformBase = baseGhost.targetBase; //!!!!!!!!!!!!
			}

			baseGhost.targetOffset = baseGhost.targetBase.WorldToGrid(baseGhost.transform.position);
			baseGhost.targetBase.CopyFrom(baseGhost.ghostBase, baseGhost.ghostBase.Bounds, baseGhost.targetOffset);

			// parenting to platform
			baseGhost.targetBase.transform.parent = gameObject.transform;
					
			//baseGo.transform.localPosition = new Vector3(11.7f, 1.4f, 7.5f);
			baseGhost.targetBase.transform.localPosition = new Vector3(11.7f, 1.4f, 7.5f);
			baseGhost.targetBase.transform.localEulerAngles = Vector3.zero;

			Destroy(newObject);
		}
		
		
		void _addNextFoundation(float deltaX, float deltaZ)
		{
			if (platformBase == null)
			{
				"NULL BASE".onScreen();
				return;
			}

			GameObject newObject = Instantiate(CraftData.GetPrefabForTechType(TechType.BaseFoundation));
			ConstructableBase constructableBase = newObject.GetComponent<ConstructableBase>();

			GameObject ghostModel = constructableBase.model;
			BaseGhost baseGhost = ghostModel.GetComponent<BaseGhost>();

			baseGhost.ghostBase.SetSize(Base.CellSize[2]);
			baseGhost.ghostBase.SetCell(Int3.zero, Base.CellType.Foundation);

			baseGhost.targetBase = platformBase;
			
			constructableBase.transform.parent = baseGhost.targetBase.transform;

			constructableBase.transform.localPosition = new Vector3(deltaX, 0, deltaZ);

			// !!!!!!!!!
			//if (baseGhost.TargetBase != null)
			//	constructableBase.transform.SetParent(baseGhost.TargetBase.transform, true);

			baseGhost.targetOffset = baseGhost.targetBase.WorldToGrid(baseGhost.transform.position);
			baseGhost.targetBase.CopyFrom(baseGhost.ghostBase, baseGhost.ghostBase.Bounds, baseGhost.targetOffset);

			//// parenting to platform
			//baseGhost.targetBase.transform.parent = gameObject.transform;
					
			////baseGo.transform.localPosition = new Vector3(11.7f, 1.4f, 7.5f);
			//baseGhost.targetBase.transform.localPosition = new Vector3(11.7f, 1.7f, 7.5f);
			//baseGhost.targetBase.transform.localEulerAngles = Vector3.zero;

			Destroy(newObject);
		}
		
		
		void _addFoundation()
		{
			return;
			$"111 - {FindObjectsOfType<Base>().Length}".log();

			///////////////////////////////////////
			if (false)
			{
				Builder.Begin(CraftData.GetPrefabForTechType(TechType.BaseFoundation));
			}
			else if (false)
			{
				Builder.prefab = CraftData.GetPrefabForTechType(TechType.BaseFoundation);
			}

			
			/////////////////////////////////////////
			if (false)
			{
				Builder.Update();
			}
			else
			{
				//Builder.Initialize();
				//Builder.placeLayerMask = ~(1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Trigger"));
				//Builder.ghostStructureMaterial = new Material(Resources.Load<Material>("Materials/ghostmodel"));

				//Builder.canPlace = false;

				//if (Builder.prefab == null)
				//{
				//	return;
				//}
				//if (Builder.CreateGhost())
				//{
				//	Builder.inputHandler.canHandleInput = true;
				//	InputHandlerStack.main.Push(Builder.inputHandler);
				//}

				if (false)
				{
					Builder.CreateGhost();
				}
				else
				{
					//Constructable component = Builder.prefab.GetComponent<Constructable>();
					//Builder.constructableTechType = component.techType;
					//Builder.placeMinDistance = component.placeMinDistance;
					//Builder.placeMaxDistance = component.placeMaxDistance;
					//Builder.placeDefaultDistance = component.placeDefaultDistance;
					//Builder.allowedSurfaceTypes = component.allowedSurfaceTypes;
					//Builder.forceUpright = component.forceUpright;
					//Builder.allowedInSub = component.allowedInSub;
					//Builder.allowedInBase = component.allowedInBase;
					//Builder.allowedOutside = component.allowedOutside;
					//Builder.allowedOnConstructables = component.allowedOnConstructables;
					//Builder.rotationEnabled = component.rotationEnabled;
					//if (Builder.rotationEnabled)
					//{
					//	Builder.ShowRotationControlsHint();
					//}
					//ConstructableBase component2 = Builder.prefab.GetComponent<ConstructableBase>();
					//if (component2 != null)
					if (false)
					{
						/*
						GameObject gameObject = Object.Instantiate<GameObject>(Builder.prefab);
						//ConstructableBase component2 = gameObject.GetComponent<ConstructableBase>();
						//Builder.ghostModel = component2.model;
						
						Builder.ghostModel = gameObject.GetComponent<ConstructableBase>().model;
						//BaseGhost component3 = Builder.ghostModel.GetComponent<BaseGhost>();
						//component3.SetupGhost();
						Builder.ghostModel.GetComponent<BaseGhost>().SetupGhost();
						*/

						//Builder.ghostModelPosition = Vector3.zero;
						//Builder.ghostModelRotation = Quaternion.identity;
						//Builder.ghostModelScale = Vector3.one;
						//Builder.renderers = MaterialExtensions.AssignMaterial(Builder.ghostModel, Builder.ghostStructureMaterial);
						//Builder.InitBounds(Builder.ghostModel);
					}
					//else
					//{
					//	Builder.ghostModel = Object.Instantiate<GameObject>(component.model);
					//	Builder.ghostModel.SetActive(true);
					//	Transform component4 = component.GetComponent<Transform>();
					//	Transform component5 = component.model.GetComponent<Transform>();
					//	Quaternion quaternion = Quaternion.Inverse(component4.rotation);
					//	Builder.ghostModelPosition = quaternion * (component5.position - component4.position);
					//	Builder.ghostModelRotation = quaternion * component5.rotation;
					//	Builder.ghostModelScale = component5.lossyScale;
					//	Collider[] componentsInChildren = Builder.ghostModel.GetComponentsInChildren<Collider>();
					//	for (int i = 0; i < componentsInChildren.Length; i++)
					//	{
					//		Object.Destroy(componentsInChildren[i]);
					//	}
					//	Builder.renderers = MaterialExtensions.AssignMaterial(Builder.ghostModel, Builder.ghostStructureMaterial);
					//	Builder.SetupRenderers(Builder.ghostModel, Player.main.IsInSub());
					//	Builder.CreatePowerPreview(Builder.constructableTechType, Builder.ghostModel);
					//	Builder.InitBounds(Builder.prefab);
					//}
				}

				//Builder.canPlace = Builder.UpdateAllowed();
				//Transform transform = Builder.ghostModel.transform;
				//transform.position = Builder.placePosition + Builder.placeRotation * Builder.ghostModelPosition;
				//transform.rotation = Builder.placeRotation * Builder.ghostModelRotation;
				//transform.localScale = Builder.ghostModelScale;
				
				//Color color = (!Builder.canPlace) ? Builder.placeColorDeny : Builder.placeColorAllow;
				//IBuilderGhostModel[] components = Builder.ghostModel.GetComponents<IBuilderGhostModel>();
				//for (int i = 0; i < components.Length; i++)
				//{
				//	components[i].UpdateGhostModelColor(Builder.canPlace, ref color);
				//}

				//Builder.ghostStructureMaterial.SetColor(ShaderPropertyID._Tint, color);
			}



								//////////////////////////////////////////////////Builder.TryPlace();
								///////////////////////////////////////////////////Builder.Update();
								///
			
			//GameObject gameObject = Object.Instantiate<GameObject>(CraftData.GetPrefabForTechType(TechType.BaseFoundation));
			
			
			GameObject newObject = Instantiate(CraftData.GetPrefabForTechType(TechType.BaseFoundation));
			ConstructableBase constructableBase = newObject.GetComponent<ConstructableBase>();

			GameObject ghostModel = constructableBase.model;
			BaseGhost baseGhost = ghostModel.GetComponent<BaseGhost>();

			if (false)
			{
				baseGhost.SetupGhost();
			}
			else
			{
				baseGhost.ghostBase.SetSize(Base.CellSize[2]);
				baseGhost.ghostBase.SetCell(Int3.zero, Base.CellType.Foundation);
				
				//baseGhost.RebuildGhostGeometry();
			}

			if (false)
			{
				baseGhost.Place();
			}
			else
			{
				//baseGhost.OnPlace();
				//baseGhost.ghostBase.RebuildGeometry();
//				baseGhost.DisableGhostModelScripts();
//				baseGhost.RecalculateBounds();
				
				if (baseGhost.targetBase != null)
				{
					constructableBase.transform.parent = baseGhost.targetBase.transform;
				}
				//else if (LargeWorld.main)
				//{
				//	LargeWorldEntity largeWorldEntity = constructableBase.gameObject.AddComponent<LargeWorldEntity>();
				//	largeWorldEntity.cellLevel = LargeWorldEntity.CellLevel.Global;
				//	LargeWorld.main.streamer.cellManager.RegisterEntity(largeWorldEntity);
				//}
			}

			// !!!!!!!!!
			//if (baseGhost.TargetBase != null)
			//	constructableBase.transform.SetParent(baseGhost.TargetBase.transform, true);

			if (false)
			{
				baseGhost.Finish();
			}
			else
			{
				if (baseGhost.targetBase == null)
				{
					GameObject gameObject;
					if (!UWE.PrefabDatabase.TryGetPrefabForFilename("WorldEntities/Structures/Base", out gameObject))
					{
						//Debug.LogErrorFormat(this, "Failed to load Base prefab in BaseGhost.Finish()", new object[0]);
						return;
					}
					GameObject gameObject2 = Object.Instantiate<GameObject>(gameObject, constructableBase.transform.position, constructableBase.transform.rotation);
					if (LargeWorld.main)
					{
						LargeWorld.main.streamer.cellManager.RegisterEntity(gameObject2);
					}
					baseGhost.targetBase = gameObject2.GetComponent<Base>();
				}

				baseGhost.targetOffset = baseGhost.targetBase.WorldToGrid(baseGhost.transform.position);
				baseGhost.targetBase.CopyFrom(baseGhost.ghostBase, baseGhost.ghostBase.Bounds, baseGhost.targetOffset);


				baseGhost.targetBase.transform.parent = gameObject.transform;
					
				//baseGo.transform.localPosition = new Vector3(11.7f, 1.4f, 7.5f);
				baseGhost.targetBase.transform.localPosition = new Vector3(11.7f, 1.7f, 7.5f);
				baseGhost.targetBase.transform.localEulerAngles = Vector3.zero;

			}

			Destroy(newObject);


			//if (constructableBase != null)
			{
				//BaseGhost component = ghostModel.GetComponent<BaseGhost>();
				//component.Place();

				//if (component.TargetBase != null)
				//	constructableBase.transform.SetParent(component.TargetBase.transform, true);

				/////////////constructableBase._constructed = false;
				/////////////constructableBase.constructedAmount = 0f;
				/////////////////constructableBase.InitializeModelCopy();
				///
				//////////////////////////////////////constructableBase.SetupRenderers();
				//////////////////////////////////////constructableBase.NotifyConstructedChanged(false);

				////////////////////////////////////////constructableBase.UpdateMaterial();


				///////////////////////////////////////constructableBase.constructedAmount = 1f;
				///////////////////////////////////////constructableBase.UpdateMaterial();
				
				
//				constructableBase.model.GetComponent<BaseGhost>().Finish();
				

				////////////////////////////////////////////constructableBase._constructed = true;
				//////////////////////////////////////////constructableBase.constructedAmount = 1f;
				////////////////////////if (constructableBase.ghostOverlay != null)
				////////////////////////{
				////////////////////////	constructableBase.ghostOverlay.RemoveOverlay();
				////////////////////////}

				////////////////////////if (constructableBase.modelCopy != null)
				////////////////////////{
				////////////////////////	//componentInParent.modelCopy.AddComponent<BuiltEffectController>();
				////////////////////////	constructableBase.modelCopy = null;
				////////////////////////}

				///////////////////////////////////////////////////constructableBase.NotifyConstructedChanged(true);
				//////////////////////////////////////////////////////constructableBase.SetupRenderers();

//				UnityEngine.Object.Destroy(constructableBase.gameObject);

				//Builder.ghostModel = null;
				//Builder.prefab = null;
				//Builder.canPlace = false;

				//Builder.End();
			
				//Builder.Update();
			}
		}

		void _parentToPlatform()
		{
			return;
			// parenting to platform
			{
				//Rocket r = Object.FindObjectOfType<Rocket>();
				Base b = Object.FindObjectOfType<Base>();
				GameObject baseGo = b.gameObject;

				//if (baseGo.transform.parent != r.gameObject.getChild("Base").transform)
				//{
				//	baseGo.transform.parent = r.gameObject.getChild("Base").transform;
				//	//baseGo.transform.localPosition = new Vector3(12.5f, -1f, 9f);
				//	baseGo.transform.localPosition = new Vector3(11.7f, -1.4f, 7.5f);
				//	baseGo.transform.localEulerAngles = Vector3.zero;
				//}
				if (baseGo.transform.parent != gameObject.transform)
				{
					baseGo.transform.parent = gameObject.transform;
					
					//baseGo.transform.localPosition = new Vector3(11.7f, 1.4f, 7.5f);
					baseGo.transform.localPosition = new Vector3(11.7f, 1.7f, 7.5f);
					baseGo.transform.localEulerAngles = Vector3.zero;
				}
			}
		}
	}
}