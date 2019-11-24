using UnityEngine;
using Harmony;
using Common;

namespace HabitatPlatform
{
	[HarmonyPatch(typeof(Builder), "UpdateAllowed")]
	static class Builder_UpdateAllowed_Patch
	{
		static bool Prefix(ref bool __result)
		{
			if (Input.GetKey(KeyCode.V))
			{
				__result = true;
				return false;
			}
	
			return true;
		}
	}


	[HarmonyPatch(typeof(Constructor), "GetItemSpawnPoint")]
	static class Constructor_GetItemSpawnPoint_Patch
	{
		static bool Prefix(Constructor __instance, TechType techType, ref Transform __result)
		{
			if (techType != HabitatPlatform.TechType)
				return true;

			__result = __instance.GetItemSpawnPoint(TechType.RocketBase);
			return false;
		}
	}


	//[HarmonyPatch(typeof(Builder), "Begin")]
	//static class Builder_Begin_Patch
	//{
	//	static void Postfix(GameObject modulePrefab)
	//	{
	//		modulePrefab.dump("!prefab");
	//	}
	//}


	[HarmonyPatch(typeof(Player), "Update")]
	static class Player_Update_Patch
	{
		static void Prefix()
		{
			if (Input.GetKeyDown(KeyCode.Insert))
			{
				Rocket r = Object.FindObjectOfType<Rocket>();

				if (r != null)
					r.gameObject.dump("!!platform_builded");
			}
			
			if (Input.GetKeyDown(KeyCode.Home))
			{
				Base[] bb = Object.FindObjectsOfType<Base>();

				for (int i = 0; i < bb.Length; i++)
					bb[i].gameObject.dump("!!base" + i);
			}

			if (Input.GetKeyDown(KeyCode.PageUp))
			{
				Builder.Begin(CraftData.GetPrefabForTechType(TechType.BaseFoundation));
			}
			
			
			if (Input.GetKeyDown(KeyCode.PageDown))
			{
				//private static void SetDefaultPlaceTransform(ref Vector3 position, ref Quaternion rotation)
				try
				{
					Builder.Begin(CraftData.GetPrefabForTechType(TechType.BaseFoundation));
					Builder.TryPlace();
					Builder.Update();

					ConstructableBase componentInParent = Builder.ghostModel?.GetComponentInParent<ConstructableBase>();

					//componentInParent?.Start();
					if (componentInParent != null)
					{
						BaseGhost component = Builder.ghostModel.GetComponent<BaseGhost>();
						component.Place();

						if (component.TargetBase != null)
							componentInParent.transform.SetParent(component.TargetBase.transform, true);

						componentInParent._constructed = false;
						componentInParent.constructedAmount = 0f;
						componentInParent.InitializeModelCopy();
						componentInParent.SetupRenderers();
						componentInParent.NotifyConstructedChanged(false);

						componentInParent.UpdateMaterial();


						componentInParent.constructedAmount = 1f;
						componentInParent.UpdateMaterial();
						componentInParent.model.GetComponent<BaseGhost>().Finish();

						componentInParent._constructed = true;
						componentInParent.constructedAmount = 1f;
						if (componentInParent.ghostOverlay != null)
						{
							componentInParent.ghostOverlay.RemoveOverlay();
						}
						if (componentInParent.modelCopy != null)
						{
							//componentInParent.modelCopy.AddComponent<BuiltEffectController>();
							componentInParent.modelCopy = null;
						}

						componentInParent.NotifyConstructedChanged(true);
						componentInParent.SetupRenderers();

						UnityEngine.Object.Destroy(componentInParent.gameObject);


						Builder.ghostModel = null;
						Builder.prefab = null;
						Builder.canPlace = false;

						Builder.End();
						Builder.Update();
					}
				}
				catch (System.Exception e)
				{
					Log.msg(e);
				}
			}
		}
	}
}


#if SCRAPS
				Builder.Begin(CraftData.GetPrefabForTechType(TechType.BaseFoundation));
				Builder.TryPlace();
				Builder.Update();
				
				ConstructableBase componentInParent = Builder.ghostModel?.GetComponentInParent<ConstructableBase>();

				componentInParent?.Start();
				if (componentInParent != null)
				{
					BaseGhost component = Builder.ghostModel.GetComponent<BaseGhost>();
					component.Place();

					if (component.TargetBase != null)
						componentInParent.transform.SetParent(component.TargetBase.transform, true);

					//componentInParent.SetState(false, true);
					//componentInParent.constructedAmount = 1f;
					//componentInParent.Construct();

					//componentInParent.SetState(false, true);
					{
						bool value = false;
						//bool setAmount = true;
						$"componentInParent._constructed = {componentInParent._constructed}".logDbg();
						//if (componentInParent._constructed != value && value)
						//{
						//	List<ConstructableBounds> list = new List<ConstructableBounds>();
						//	componentInParent.GetComponentsInChildren<ConstructableBounds>(true, list);
						//	List<GameObject> list2 = new List<GameObject>();
						//	for (int i = 0; i < list.Count; i++)
						//	{
						//		ConstructableBounds constructableBounds = list[i];
						//		OrientedBounds orientedBounds = OrientedBounds.ToWorldBounds(constructableBounds.transform, constructableBounds.bounds);
						//		list2.Clear();
						//		Builder.GetOverlappedObjects(orientedBounds.position, orientedBounds.rotation, orientedBounds.extents, list2);
						//		int j = 0;
						//		int count = list2.Count;
						//		while (j < count)
						//		{
						//			GameObject gameObject = list2[j];
						//			if (Builder.CanDestroyObject(gameObject))
						//			{
						//				UnityEngine.Object.Destroy(gameObject);
						//			}
						//			j++;
						//		}
						//	}
						//	componentInParent.model.GetComponent<BaseGhost>().Finish();
						//	//component;
						//}

						//bool result = (componentInParent as Constructable).SetState(value, setAmount);
						{
							componentInParent._constructed = value;
							//MonoBehaviour[] components = componentInParent.gameObject.GetComponents<MonoBehaviour>();
							//int i = 0;
							//int num = components.Length;
							//while (i < num)
							//{
							//	MonoBehaviour monoBehaviour = components[i];
							//	if (!(monoBehaviour == null) && !(monoBehaviour == componentInParent) && monoBehaviour.GetType() != typeof(SubModuleHandler))
							//	{
							//		components[i].enabled = componentInParent._constructed;
							//	}
							//	i++;
							//}
							//if (componentInParent.controlledBehaviours != null)
							//{
							//	int j = 0;
							//	int num2 = componentInParent.controlledBehaviours.Length;
							//	while (j < num2)
							//	{
							//		MonoBehaviour x = componentInParent.controlledBehaviours[j];
							//		if (!(x == null) && !(x == componentInParent))
							//		{
							//			componentInParent.controlledBehaviours[j].enabled = componentInParent._constructed;
							//		}
							//		j++;
							//	}
							//}
							//if (setAmount)
							{
								componentInParent.constructedAmount = 0f;// ((!componentInParent._constructed) ? 0f : 1f);
							}
							//if (componentInParent._constructed)
							//{
							//	componentInParent.DestroyModelCopy();
							//	componentInParent.NotifyConstructedChanged(true);
							//	componentInParent.SetupRenderers();
							//	//ItemGoalTracker.OnConstruct(componentInParent.techType);
							//}
							//else
							{
								componentInParent.InitializeModelCopy();
								componentInParent.SetupRenderers();
								componentInParent.NotifyConstructedChanged(false);
							}
						}

						//if (componentInParent._constructed)
						//{
						//	UnityEngine.Object.Destroy(componentInParent.gameObject);
						//}
						//else
						{
							componentInParent.UpdateMaterial();
						}
					}


					componentInParent.constructedAmount = 1f;
					componentInParent.UpdateMaterial();
					//componentInParent.SetState(true, true);
					{
						bool value = true;
						bool setAmount = true;
						if (componentInParent._constructed != value && value)
						{
							//List<ConstructableBounds> list = new List<ConstructableBounds>();
							//componentInParent.GetComponentsInChildren<ConstructableBounds>(true, list);
							//List<GameObject> list2 = new List<GameObject>();
							//for (int i = 0; i < list.Count; i++)
							//{
							//	"11111111111111".log();
							//	ConstructableBounds constructableBounds = list[i];
							//	OrientedBounds orientedBounds = OrientedBounds.ToWorldBounds(constructableBounds.transform, constructableBounds.bounds);
							//	list2.Clear();
							//	Builder.GetOverlappedObjects(orientedBounds.position, orientedBounds.rotation, orientedBounds.extents, list2);
							//	int j = 0;
							//	int count = list2.Count;
							//	while (j < count)
							//	{
							//		GameObject gameObject = list2[j];
							//		if (Builder.CanDestroyObject(gameObject))
							//		{
							//			UnityEngine.Object.Destroy(gameObject);
							//		}
							//		j++;
							//	}
							//}
							componentInParent.model.GetComponent<BaseGhost>().Finish();
							//component;
						}

						//bool result = (componentInParent as Constructable).SetState(value, setAmount);
						if (true)
						{
							componentInParent._constructed = value;
							//MonoBehaviour[] components = componentInParent.gameObject.GetComponents<MonoBehaviour>();
							//int i = 0;
							//int num = components.Length;
							//while (i < num)
							//{
							//	MonoBehaviour monoBehaviour = components[i];
							//	if (!(monoBehaviour == null) && !(monoBehaviour == componentInParent) && monoBehaviour.GetType() != typeof(SubModuleHandler))
							//	{
							//		components[i].enabled = componentInParent._constructed;
							//	}
							//	i++;
							//}
							//if (componentInParent.controlledBehaviours != null)
							//{
							//	int j = 0;
							//	int num2 = componentInParent.controlledBehaviours.Length;
							//	while (j < num2)
							//	{
							//		MonoBehaviour x = componentInParent.controlledBehaviours[j];
							//		if (!(x == null) && !(x == componentInParent))
							//		{
							//			componentInParent.controlledBehaviours[j].enabled = componentInParent._constructed;
							//		}
							//		j++;
							//	}
							//}
							//if (setAmount)
							{
								componentInParent.constructedAmount = 1f;//((!componentInParent._constructed) ? 0f : 1f);
							}
							//if (componentInParent._constructed)
							{
								//(componentInParent as Constructable).DestroyModelCopy();
								{
									if (componentInParent.ghostOverlay != null)
									{
										componentInParent.ghostOverlay.RemoveOverlay();
									}
									if (componentInParent.modelCopy != null)
									{
										//componentInParent.modelCopy.AddComponent<BuiltEffectController>();
										componentInParent.modelCopy = null;
									}
									//if (componentInParent.builtBoxFX != null)
									//{
									//	componentInParent.GetComponentsInChildren<Renderer>(false, Constructable.sRenderers);
									//	Matrix4x4 worldToLocalMatrix = componentInParent.transform.worldToLocalMatrix;
									//	Vector3 position;
									//	Vector3 extents;
									//	OrientedBounds.EncapsulateRenderers(worldToLocalMatrix, Constructable.sRenderers, out position, out extents);
									//	Constructable.sRenderers.Clear();
									//	if (extents.x > 0f && extents.y > 0f && extents.z > 0f)
									//	{
									//		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(componentInParent.builtBoxFX);
									//		Transform transform = gameObject.transform;
									//		OrientedBounds localBounds = new OrientedBounds(position, Quaternion.identity, extents);
									//		OrientedBounds orientedBounds = OrientedBounds.ToWorldBounds(componentInParent.transform, localBounds);
									//		transform.position = orientedBounds.position;
									//		transform.rotation = orientedBounds.rotation;
									//		transform.localScale = orientedBounds.size;
									//	}
									//}

								}

								(componentInParent as Constructable).NotifyConstructedChanged(true);
								(componentInParent as Constructable).SetupRenderers();
								//Story.ItemGoalTracker.OnConstruct(componentInParent.techType);
							}
							//else
							//{
							//	componentInParent.InitializeModelCopy();
							//	componentInParent.SetupRenderers();
							//	componentInParent.NotifyConstructedChanged(false);
							//}
							//return true;

						}

						//if (componentInParent._constructed)
						{
							UnityEngine.Object.Destroy(componentInParent.gameObject);
						}
						//else
						//{
						//	componentInParent.UpdateMaterial();
						//}
					}
#endif