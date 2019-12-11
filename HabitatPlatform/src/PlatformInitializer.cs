using System.Collections;

using UnityEngine;

using Common;

namespace HabitatPlatform
{
	class PlatformInitializer: MonoBehaviour
	{
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

			yield break;

			//initOneTime();

			_addFoundation();
			yield return new WaitForSeconds(1f);
			_parentToPlatform();


			init();

			//Destroy(this);
		}

		void Update()
		{
			//VFXConstructing c = gameObject.GetComponentInChildren<VFXConstructing>();

			//if (c)
			//	$"constructed: {c.constructed}".onScreen();

			if (Input.GetKeyDown(KeyCode.PageDown))
				_addFoundation();

			if (Input.GetKeyDown(KeyCode.Delete))
				_parentToPlatform();

		}


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


		void _addFoundation()
		{
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